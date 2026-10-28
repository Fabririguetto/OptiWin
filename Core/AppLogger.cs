using System;

namespace OptiWin.Core
{
    public static class AppLogger
    {
        public static event Action<string>? OnLog;

        public static void Info(string message)
        {
            var formatted = $"[{DateTime.Now:HH:mm:ss}] [INFO] {message}";
            System.Diagnostics.Debug.WriteLine(formatted);
            OnLog?.Invoke(formatted);
        }

        public static void Success(string message)
        {
            var formatted = $"[{DateTime.Now:HH:mm:ss}] [ÉXITO] {message}";
            System.Diagnostics.Debug.WriteLine(formatted);
            OnLog?.Invoke(formatted);
        }

        public static void Warning(string message)
        {
            var formatted = $"[{DateTime.Now:HH:mm:ss}] [AVISO] {message}";
            System.Diagnostics.Debug.WriteLine(formatted);
            OnLog?.Invoke(formatted);
        }

        public static void Error(string message, Exception? ex = null)
        {
            var formatted = $"[{DateTime.Now:HH:mm:ss}] [ERROR] {message} {(ex != null ? ex.Message : "")}";
            System.Diagnostics.Debug.WriteLine(formatted);
            OnLog?.Invoke(formatted);
        }
    }
}
