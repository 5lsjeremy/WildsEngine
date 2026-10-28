using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace WildsEngine.Tests.Logging
{
    public static class TestLog
    {
        private static readonly object Sync = new object();
        private static string? _path;

        public static string Path => _path ?? string.Empty;

        public static void Initialize(string? fileName = null)
        {
            lock (Sync)
            {
                if (_path != null)
                    return;

                var logs = System.IO.Path.Combine(AppContext.BaseDirectory, "Logs");
                Directory.CreateDirectory(logs);
                var name = fileName ?? $"wildsengine-tests-{DateTime.Now:yyyyMMdd-HHmmss}.log";
                _path = System.IO.Path.Combine(logs, name);
                File.WriteAllText(_path, $"WildsEngine test log{Environment.NewLine}", Encoding.UTF8);
            }
        }

        public static void Section(string title)
        {
            Write("");
            Write("============================================");
            Write($" {title}");
            Write("============================================");
        }

        public static void Write(string message)
        {
            lock (Sync)
            {
                if (_path == null)
                    Initialize();

                Console.WriteLine(message);
                File.AppendAllText(_path!,
                    $"{DateTime.Now.ToString("O", CultureInfo.InvariantCulture)} {message}{Environment.NewLine}",
                    Encoding.UTF8);
            }
        }

        public static void Test(string message) => Write($"[TEST] {message}");
        public static void Pass(string message) => Write($"[PASS] {message}");
        public static void Task(string message) => Write($"    [TASK] {message}");
        public static void State(string message) => Write($"    [STATE] {message}");
        public static void Expected(string message) => Write($"    [EXPECTED] {message}");
        public static void Fail(string message, Exception ex) =>
            Write($"[FAIL] {message}: {ex.GetType().Name}: {ex.Message}");
    }
}
