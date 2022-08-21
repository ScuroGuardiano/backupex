namespace Backupex.Backblaze {
    public class StorageCapExceedException : Exception
    {
        public StorageCapExceedException()
        {
        }

        public StorageCapExceedException(string message)
            : base(message)
        {
        }

        public StorageCapExceedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}