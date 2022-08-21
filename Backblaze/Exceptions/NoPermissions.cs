namespace Backupex.Backblaze {
    public class NoPermissionsException : Exception
    {
        public NoPermissionsException()
        {
        }

        public NoPermissionsException(string message)
            : base(message)
        {
        }

        public NoPermissionsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}