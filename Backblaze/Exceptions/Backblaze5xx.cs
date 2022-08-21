namespace Backupex.Backblaze {
    public class Backblaze5xxException : Exception
    {
        public Backblaze5xxException()
        {
        }

        public Backblaze5xxException(string message)
            : base(message)
        {
        }

        public Backblaze5xxException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}