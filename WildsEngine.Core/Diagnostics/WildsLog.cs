using System;

namespace WildsEngine.Core.Diagnostics
{
    public static class WildsLog
    {
        public static Action<string>? Writer { get; set; }

        public static bool Enabled { get; set; } = true;

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Process(string message)
        {
            Write("PROCESS", message);
        }

        public static void State(string message)
        {
            Write("STATE", message);
        }

        public static void Warning(string message)
        {
            Write("WARN", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(
            string category,
            string message)
        {
            if (!Enabled)
                return;

            var formatted =
                $"[Wilds:{category}] {message}";

            if (Writer != null)
            {
                Writer(formatted);
                return;
            }

            Console.WriteLine(formatted);
        }
    }
}