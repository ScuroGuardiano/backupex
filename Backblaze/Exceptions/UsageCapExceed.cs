namespace Backupex.Backblaze {
    public class UsageCapExceedException : Exception
    {
        public UsageCapExceedException()
        {
        }

        public UsageCapExceedException(string message)
            : base(message)
        {
        }

        public UsageCapExceedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}