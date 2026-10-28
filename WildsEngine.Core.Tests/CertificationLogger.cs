using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Tests
{
    /// <summary>
    /// Shared console logger for WildsEngine certification
    /// and diagnostic test output.
    /// </summary>
    public static class CertificationLogger
    {
        public static void LogSection(string title)
        {
            Console.WriteLine();
            Console.WriteLine(
                "============================================");
            Console.WriteLine(
                $" {title}");
            Console.WriteLine(
                "============================================");
        }

        public static void LogTest(string name)
        {
            Console.WriteLine(
                $"[TEST] {name}");
        }

        public static void LogPass(string name)
        {
            WriteColored(
                ConsoleColor.Green,
                $"[PASS] {name}");
        }

        public static void LogTask(string message)
        {
            WriteColored(
                ConsoleColor.DarkGray,
                $"    [TASK] {message}");
        }

        public static void LogState(
            string name,
            object? value)
        {
            WriteColored(
                ConsoleColor.Cyan,
                $"    [STATE] {name} = {value}");
        }

        public static void LogPool<T>(
            string name,
            IObjectPool<T> pool)
            where T : class
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            WriteColored(
                ConsoleColor.DarkCyan,
                $"    [POOL] {name}");

            Console.WriteLine(
                $"           Active     : {pool.ActiveCount}");

            Console.WriteLine(
                $"           Available  : {pool.AvailableCount}");

            Console.WriteLine(
                $"           Created    : {pool.CreatedCount}");

            Console.WriteLine(
                $"           Retained   : {pool.MaximumRetained}");

            Console.WriteLine(
                $"           Concurrent : {pool.MaximumConcurrent}");
        }

        public static void LogExpected<TException>()
            where TException : Exception
        {
            WriteColored(
                ConsoleColor.DarkYellow,
                $"    [EXPECTED] {typeof(TException).Name}");
        }

        private static void WriteColored(
            ConsoleColor color,
            string message)
        {
            var previousColor =
                Console.ForegroundColor;

            try
            {
                Console.ForegroundColor = color;

                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor =
                    previousColor;
            }
        }
    }
}