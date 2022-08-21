namespace Backupex.Backblaze {
    public record ErrorData {
        public int? status;
        public string? code;
        public string? message;
    }
}