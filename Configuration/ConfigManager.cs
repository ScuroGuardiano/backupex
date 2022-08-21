using System.Text.Json;
using System.Text.Encodings.Web;

namespace Backupex.Configuration {
    public class ConfigManager {

        private ConfigManager() {}
        private static ConfigManager? instance;
        public static ConfigManager Instance {
            get {
                if (instance == null) {
                    instance = new ConfigManager();
                }
                return instance;
            }
        }

        private readonly JsonSerializerOptions serializeOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private ConfigData? config;
        public ConfigData? Config {
            get {
                return config;
            }
        }

        public bool ConfigFileExists() {
            return File.Exists("config.json");
        }

        public void LoadConfig() {
            if (!ConfigFileExists()) {
                Console.WriteLine("Config file does not exists.");
                throw new FileNotFoundException();
            }
            using var fileStream = File.OpenRead("config.json");
            config = JsonSerializer.Deserialize<ConfigData>(fileStream, serializeOptions);
        }

        public void GenerateConfigTemplate() {
            if (ConfigFileExists()) {
                Console.WriteLine("Configuration file already exists, cannot generate new");
            }

            string data = JsonSerializer.Serialize(new ConfigData {
                AppId = "<put your backblaze application id here>",
                AppKey = "<put your backblaze application key here>",
                BucketId = "<put your backblaze bucket id here>",
                Filter = "*",
                DirectoriesToWatch = new List<DirectoryToWatch>() {
                    new DirectoryToWatch() {
                        Path = "/path/to/directory",
                        FilenamePrefix = "Filename/prefix"
                    }
                }
            }, serializeOptions);
            File.WriteAllText("config.json", data, System.Text.Encoding.UTF8);
        }
    }
}