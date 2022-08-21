namespace Backupex {
    public class FileToBackup {
        public FileToBackup(string FullPath, string filenamePrefix, long LastChanged) {
            this.FullPath = FullPath;
            this.FilenamePrefix = filenamePrefix;
            this.LastChanged = LastChanged;
        }

        public string? FilenamePrefix { get; set; }
        public string? FullPath { get; set; }
        public long LastChanged { get; set; }
    }
}