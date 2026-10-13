using System;
using System.IO;
using System.Text;

namespace SpaceBourne2ModTool.Services
{
    public static class LoggerService
    {
        private static readonly object _lock = new object();
        private static StreamWriter _writer;
        private static string _logPath;

        public static void Initialize()
        {
            try
            {
                string appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SpaceBourne2ModTool");
                Directory.CreateDirectory(appData);
                _logPath = Path.Combine(appData, $"modtool_{DateTime.Now:yyyyMMdd}.log");
                _writer = new StreamWriter(_logPath, true, Encoding.UTF8) { AutoFlush = true };
                Log("=== SpaceBourne 2 Mod Tool started ===");
            }
            catch
            {
                // Fail silently if logging cannot be initialized
            }
        }

        public static void Log(string message)
        {
            Write("INFO", message);
        }

        public static void LogWarning(string message)
        {
            Write("WARN", message);
        }

        public static void LogError(string message, Exception ex = null)
        {
            Write("ERROR", message + (ex != null ? " | " + ex.ToString() : ""));
        }

        private static void Write(string level, string message)
        {
            lock (_lock)
            {
                try
                {
                    string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                    _writer?.WriteLine(line);
                    System.Diagnostics.Debug.WriteLine(line);
                }
                catch { }
            }
        }

        public static void Shutdown()
        {
            lock (_lock)
            {
                try
                {
                    Log("=== SpaceBourne 2 Mod Tool shutdown ===");
                    _writer?.Dispose();
                    _writer = null;
                }
                catch { }
            }
        }

        public static string GetLogPath() => _logPath;
    }
}
