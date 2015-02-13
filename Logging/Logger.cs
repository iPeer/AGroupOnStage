using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AGroupOnStage.Logging
{
    public class Logger
    {

        public static void Log(String message, params object[] data) { Log(message, LogLevel.NORMAL, data); }
        public static void Warning(String message) { logWarning(message); }
        public static void logWarning(String message) { Log(message, LogLevel.WARNING); }
        public static void Error(Exception e) { LogError(e); }
        public static void Log(Exception e) { LogError(e); }
        public static void LogError(Exception e) { Log("{0}", LogLevel.ERROR, e.StackTrace); }
        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebug(String message, params object[] data) { Log(message, LogLevel.DEBUG, data); }

        public static void Log(String message, LogLevel level, params object[] data)
        {

            message = String.Format(message, data);

            foreach (string line in Regex.Split(message, "\r\n")) {

                string logLine = String.Format("{1} {2}", DateTime.Now, "[AGroupOnStage]:", line);
                if (level == LogLevel.WARNING) // Warning (3)
                    UnityEngine.Debug.LogWarning(logLine);
                else if (level == LogLevel.ERROR) // Error (2)
                    UnityEngine.Debug.LogError(logLine);
                else // "normal" (1)
                    UnityEngine.Debug.Log(logLine);


            }

        }

    }

    public enum LogLevel
    {

        DEBUG = 0, // Not used
        NORMAL = 1,
        ERROR = 2,
        WARNING = 3

    }
}
