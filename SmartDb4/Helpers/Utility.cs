//using log4net;

using NLog;

namespace SmartDb4.Helpers
{
    public static class Utility
    {
        private static readonly Logger _logger = LogManager.GetLogger("SmartDbApp");
        //private static readonly ILog Log = LogManager.GetLogger(typeof(AccountController).Name);


        public static void WriteToLog(string message, string loglevel)
        {
            _logger.Error("Error : " + message);
        }

        public static void LogInfo(string message)
        {
            _logger.Info("Info : " + message);
        }

        public static void LogDebug(string message)
        {
            _logger.Info("Debug : " + message);
        }

        public static void LogWarn(string message)
        {
            _logger.Info("Warn : " + message);
        }


        //public static void WriteToLog(string message, string loglevel)
        //{
        //    Log.Error("Error : " + message);
        //}

        //public static void LogError(string message)
        //{
        //    Log.Error("Error : " + message);
        //}

        //public static void LogInfo(string message)
        //{
        //    Log.Info("Information : " + message);
        //}

        //public static void LogWarn(string message)
        //{
        //    Log.Warn("Warning : " + message);
        //}

        //public static void LogDebug(string message)
        //{
        //    Log.Warn("LogDebug : " + message);
        //}
        //private static LogLevel GetLogLevel(string loglevel)
        //{
        //    switch (loglevel.ToLower())
        //    {
        //        case "error":
        //            return LogLevel.Error;
        //        case "info":
        //            return LogLevel.Info;
        //        case "debug":
        //            return LogLevel.Debug;
        //        case "trace":
        //            return LogLevel.Trace;
        //    }
        //}
    }
}