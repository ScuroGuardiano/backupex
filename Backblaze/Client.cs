using System.Net;
using System.Net.Http.Headers;
using System.Web;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace Backupex.Backblaze {
    public class Client {
        public Client(string appId, string applicationKey, string bucketId) {
            this.appId = appId;
            this.applicationKey = applicationKey;
            this.bucketId = bucketId;
        }
        
        private string? authorizationToken;
        private string? apiUrl;
        private string appId;
        private string applicationKey;
        private string bucketId;

        private readonly string authUrl = "https://api.backblazeb2.com/b2api/v2/b2_authorize_account";

        private HttpClient httpClient = new HttpClient();

        private readonly JsonSerializerOptions serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        /// <summary>
        ///     Will log in to Backblaze using given credentials in URL
        /// </summary>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="WrongCredentialsException"></exception>
        /// <exception cref="UsageCapExceedException"></exception>
        /// <exception cref="Exception">I throw it when for some reason JSON is empty. Shouldn't happen tho</exception>
        public void Authorize() {
            Logger.Info("Backblaze: Authorizing...");
            using var request = new HttpRequestMessage(HttpMethod.Get, authUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", GetAuthCredentials());
            var response = httpClient.Send(request);
            try {
                response.EnsureSuccessStatusCode();
                var jsonStream = response.Content.ReadAsStream();
                var authData = JsonSerializer.Deserialize<AuthResponse>(jsonStream, serializeOptions);
                if (authData == null) {
                    throw new Exception("Shit happened, JSON returned by Backblaze was invalid.");
                }
                this.apiUrl = authData.ApiUrl;
                this.authorizationToken = authData.AuthorizationToken;
                Logger.Info("Backblaze: Authorized!");
            }
            catch(HttpRequestException ex) {
                if ((int)response.StatusCode >= 500 && (int)response.StatusCode <= 599) {
                    throw new Backblaze5xxException(response.StatusCode.ToString());
                }

                switch(ex.StatusCode) {
                    case HttpStatusCode.BadRequest:
                        Logger.Error("Backblaze: Bad request... This shouldn't happen, propably I fucked up something in request. Sowwi.");
                        throw;
                    case HttpStatusCode.Unauthorized:
                        Logger.Error("Backblaze: The applicationKeyId and/or the applicationKey are wrong.");
                        throw new WrongCredentialsException();
                    case HttpStatusCode.Forbidden:
                        Logger.Error("Backblaze: Usage cap exceeded.");
                        throw new UsageCapExceedException();
                    default:
                        throw;
                }
            }
        }

        private string GetAuthCredentials() {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(appId + ":" + applicationKey));
        }

        private UploadInfo GetFileUploadInfo() {
            if (apiUrl == null || authorizationToken == null) {
                Logger.Info("Backblaze is not authorized.");
                Authorize();
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}/b2api/v2/b2_get_upload_url");
            request.Headers.TryAddWithoutValidation("Authorization", authorizationToken);
            request.Content = new StringContent(
                JsonSerializer.Serialize(new { bucketId }),
                Encoding.UTF8,
                "application/json"
            );

            var response = httpClient.Send(request);
            
            if (response.IsSuccessStatusCode) {
                var jsonStream = response.Content.ReadAsStream();
                var uploadInfo = JsonSerializer.Deserialize<UploadInfo>(jsonStream, serializeOptions);
                if (uploadInfo == null) {
                    throw new Exception("GetUploadInfo returned invalid JSON");
                }
                return uploadInfo;
            }

            // Errors
            ErrorData? error = JsonSerializer.Deserialize<ErrorData>(response.Content.ReadAsStream());

            if (error == null) {
                throw new HttpRequestException(response.StatusCode.ToString(), null, response.StatusCode);
            }
            
            if ((int)response.StatusCode >= 500 && (int)response.StatusCode <= 599) {
                throw new Backblaze5xxException(response.StatusCode.ToString());
            }
            switch(error.code) {
                case "unauthorized":
                    throw new NoPermissionsException();
                case "bad_auth_token":
                case "expired_auth_token":
                    throw new BadOrExpiredAuthTokenException();
                case "storage_cap_exceeded":
                    throw new StorageCapExceedException();
                default:
                    throw new HttpRequestException(response.StatusCode.ToString(), null, response.StatusCode); 
            }
        }

        /// <summary>
        ///     Will upload file to backblaze.
        /// 
        ///     It can throw more exceptions than I written but fuck it, the list is long. For example all exception with File and IO.
        ///     Basically only in case of NoPermissionException you shouldn't try again so... ye.
        /// </summary>
        /// <param name="filename">Destination file name</param>
        /// <param name="filePath">File path on local system</param>
        /// <exception cref="HttpRequestException"></exception>
        /// <exception cref="Backblaze5xxException"></exception>
        /// <exception cref="NoPermissionsException"></exception>
        /// <exception cref="BadOrExpiredAuthTokenException"></exception>
        /// <exception cref="UsageCapExceedException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void UploadFile(string filename, string filePath) {
            Logger.Info($"Backblaze: Started file upload: {filePath} as {filename}...");
            var uploadInfo = GetFileUploadInfo();
            using var request = new HttpRequestMessage(HttpMethod.Post, uploadInfo.UploadUrl);
            using var fileStream = File.OpenRead(filePath);
            SHA1 sha1 = SHA1.Create();
            byte[] hashData = sha1.ComputeHash(fileStream);
            fileStream.Position = 0;
            string sha1Str = Convert.ToHexString(hashData);

            request.Headers.TryAddWithoutValidation("Authorization", uploadInfo.AuthorizationToken);
            request.Headers.Add("X-Bz-File-Name", filename);
            request.Headers.Add("X-Bz-Content-Sha1", sha1Str);
            request.Headers.Add("X-Bz-Info-Author", "ScuroGuardianoBackupex");
            request.Content = new StreamContent(fileStream);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("b2/x-auto");

            var response = httpClient.Send(request);

            if (response.IsSuccessStatusCode) {
                Logger.Info($"Backblaze: File {filePath} was uploaded as {filename}! SHA1 of uploaded file: {sha1Str}");
                return;
            }

            // Errors
            Stream responseStream = response.Content.ReadAsStream();
            ErrorData? error = JsonSerializer.Deserialize<ErrorData>(responseStream);

            if (error == null) {
                throw new HttpRequestException(response.StatusCode.ToString(), null, response.StatusCode);
            }
            
            if ((int)response.StatusCode >= 500 && (int)response.StatusCode <= 599) {
                throw new Backblaze5xxException(response.StatusCode.ToString());
            }


            switch(error.code) {
                case "unauthorized":
                    throw new NoPermissionsException();
                case "bad_auth_token":
                case "expired_auth_token":
                    // This shouldn't be throwed here at all, because I am calling GetFileUploadInfo2 before uploading
                    // But just in case I will leave it here. If it's thrown it is suggested to retry upload.
                    throw new BadOrExpiredAuthTokenException();
                case "cap_exceeded":
                    throw new UsageCapExceedException();
                case "request_timeout":
                    throw new TimeoutException();
                default:
                    throw new HttpRequestException(response.StatusCode.ToString(), null, response.StatusCode); 
            }

        }
    }
}