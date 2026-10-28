using System;

namespace WildsEngine.Core
{
    /// <summary>
    /// Configures processor retention and concurrency
    /// for one WildsEngine instance.
    /// </summary>
    public sealed class WildsEngineOptions
    {
        public string Layer { get; set; } =
            "runtime";

        public int RetainedInstructionsProcessors { get; set; } =
            8;

        public int ConcurrentInstructionsProcessors { get; set; } =
            Math.Max(1, Environment.ProcessorCount);

        public int RetainedTaskProcessors { get; set; } =
            16;

        public int ConcurrentTaskProcessors { get; set; } =
            Math.Max(1, Environment.ProcessorCount);
    }
}