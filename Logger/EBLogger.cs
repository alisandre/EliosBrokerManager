using Serilog;

namespace EliosBrokerManager.Logger
{
    public static class EBLogger
    {
        public static void Init() {

            if (!System.IO.Directory.Exists("Logs")) System.IO.Directory.CreateDirectory("Logs");
         
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()          
            .WriteTo.File("Logs/EBLogger.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }

        public static void LogDebug(string message)
        {            
            Log.Debug(message);
        }

        public static void LogInformation(string message)
        {
            Log.Information(message);
        }

        public static void LogWarning(string message)
        {
            Log.Warning(message);
        }

        public static void LogError(string message)
        {
            Log.Error(message);
        }

        public static void LogFatal(string message)
        {
            Log.Fatal(message);
        }
    }
}
