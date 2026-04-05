using System;

namespace Hoyo_Inventory_Scanner
{
    public static class Logger {
        public static Action<string> DebugLog;
        public static Action<string> ResultLog;
        public static Action ClearDebugAction;
        public static Action ClearResultAction;

        public static void LogDebug(string message) {
            DebugLog?.Invoke(message);
        }

        public static void LogResult(string message) {
            ResultLog?.Invoke(message);
        }

        public static void ClearDebug() {
            ClearDebugAction?.Invoke();
        }

        public static void ClearResult() {
            ClearResultAction?.Invoke();
        }
    }
}