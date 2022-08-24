namespace Backupex {
    public class BackupToB2 {
        public BackupToB2(Backblaze.Client client) {
            this.client = client;
        }

        readonly int maxRetry = 3;
        private Backblaze.Client client;
        public void SendBackup(string filePath, string filename) {
            Logger.Info($"Sending backup file {filePath} to B2 Storage");
            bool result = _SendBackup(filePath, filename);
            if (result) {
                Console.WriteLine($"Backup of file {filePath} sent successfully!");
            }
            else {
                Console.WriteLine($"Failed to upload backup file: {filePath}. Backupex will SKIP this file.");
            }
        }

        private bool _SendBackup(string filePath, string filename, int retryCount = 0, bool retryWithAuth = false) {
            if (retryCount == maxRetry) {
                Logger.Error($"Max retries reached while uploading file {filePath}. Aborting.");
                return false;
            }
            try {
                if (retryWithAuth) {
                    client.Authorize();
                }
                client.UploadFile(filename, filePath);
                return true;
            }
            catch(Backupex.Backblaze.NoPermissionsException) {
                Logger.Error("B2 Client has no permission to upload file.");
                throw;
            }
            catch(Backupex.Backblaze.WrongCredentialsException) {
                Logger.Error("B2 Client got wrong credentials.");
                throw;
            }
            catch(Backupex.Backblaze.BadOrExpiredAuthTokenException) {
                Logger.Error("B2 Client got unauthorized.");
                return _SendBackup(filePath, filename, retryCount, true);
            }
            catch(Exception ex) {
                switch(ex) {
                    case Backupex.Backblaze.StorageCapExceedException:
                    case Backupex.Backblaze.UsageCapExceedException:
                        Logger.Error("Storage/Usage cap of B2 exceed, aborting upload.");
                        return false;
                    default:
                    // In any other exception it's safe to retry.
                        Logger.Error($"An exception has been thrown - {ex.GetType().Name}. Retrying...");
                        return _SendBackup(filePath, filename, retryCount + 1);
                }
            }
        }
    }
}