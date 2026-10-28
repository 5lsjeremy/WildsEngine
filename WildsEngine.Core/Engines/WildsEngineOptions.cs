using System;

namespace WildsEngine.Core
{
    public sealed class WildsEngineOptions
    {
        public string Layer { get; set; } = "runtime";

        public int RetainedTaskProcessors { get; set; } = 16;

        public int ConcurrentTaskProcessors { get; set; } =
            Math.Max(1, Environment.ProcessorCount);

        public int RetainedInstructionsProcessors { get; set; } = 8;

        public int ConcurrentInstructionsProcessors { get; set; } =
            Math.Max(1, Environment.ProcessorCount);
    }
}