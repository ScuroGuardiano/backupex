namespace Backupex.Backblaze {
    public class BadOrExpiredAuthTokenException : Exception
    {
        public BadOrExpiredAuthTokenException()
        {
        }

        public BadOrExpiredAuthTokenException(string message)
            : base(message)
        {
        }

        public BadOrExpiredAuthTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}