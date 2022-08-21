using System.ComponentModel.DataAnnotations;

namespace Backupex.Configuration {
    public class DirectoryToWatch {
        public string? Path { get; set; }
        public string? FilenamePrefix { get; set;}
    }

    public class ConfigData {
        [Required]
        [MinLength(1)]
        public string? AppId { get; set; }

        [Required]
        [MinLength(1)]
        public string? AppKey { get; set; }

        [Required]
        [MinLength(1)]
        public string? BucketId { get; set; }

        public string Filter { get; set; } = "*";

        [Required]
        [MinLength(1)]
        public List<DirectoryToWatch>? DirectoriesToWatch { get; set; }
    }
}