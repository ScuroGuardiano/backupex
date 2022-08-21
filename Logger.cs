namespace Backupex {
    public class Logger {
        public static void Info(string message, params object?[]? args) {
            var x = String.Format("[{0}] [INFO]: {1}", DateTime.Now, message);
            Console.WriteLine(x, args);
        }
        
        public static void Error(string message, params object?[] args) {
            var x = String.Format("[{0}] [ERR]: {1}", DateTime.Now, message);
            Console.Error.WriteLine(x, args);
        }
    }
}