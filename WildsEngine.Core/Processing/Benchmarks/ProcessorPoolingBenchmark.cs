using System;
using System.Diagnostics;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Pooling;
using WildsEngine.Core.Processing.Processor;

namespace WildsEngine.Benchmarks
{
    /// <summary>
    /// Compares direct TaskProcessor allocation against
    /// pooled TaskProcessor leasing.
    /// </summary>
    public sealed class ProcessorPoolingBenchmark
    {
        private readonly ITaskRegistry _registry;

        public ProcessorPoolingBenchmark(
            ITaskRegistry registry)
        {
            _registry = registry ??
                throw new ArgumentNullException(nameof(registry));
        }

        public ProcessorPoolingBenchmarkResult Run(
            int iterations,
            int maximumRetained = 64,
            int maximumConcurrent = 64)
        {
            if (iterations <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(iterations));
            }

            if (maximumRetained < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumRetained));
            }

            if (maximumConcurrent <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maximumConcurrent));
            }

            WarmUp();

            var allocationMeasurementAvailable =
                IsAllocationMeasurementAvailable();

            var directAllocatedBytesBefore =
                GetAllocatedBytes();

            var directWatch =
                Stopwatch.StartNew();

            for (var i = 0; i < iterations; i++)
            {
                var processor =
                    new TaskProcessor(
                        "benchmark.direct",
                        _registry);

                Consume(processor);
            }

            directWatch.Stop();

            var directAllocatedBytes =
                allocationMeasurementAvailable
                    ? GetAllocatedBytes() -
                      directAllocatedBytesBefore
                    : 0;

            using (var pool =
                new ObjectPool<ITaskProcessor>(
                    factory: () =>
                        new TaskProcessor(
                            "benchmark.pooled",
                            _registry),
                    maximumRetained:
                        maximumRetained,
                    maximumConcurrent:
                        maximumConcurrent))
            {
                var pooledAllocatedBytesBefore =
                    GetAllocatedBytes();

                var pooledWatch =
                    Stopwatch.StartNew();

                for (var i = 0; i < iterations; i++)
                {
                    using (var lease = pool.Rent())
                    {
                        Consume(lease.Instance);
                    }
                }

                pooledWatch.Stop();

                var pooledAllocatedBytes =
                    allocationMeasurementAvailable
                        ? GetAllocatedBytes() -
                          pooledAllocatedBytesBefore
                        : 0;

                return new ProcessorPoolingBenchmarkResult(
                    iterations,
                    directWatch.Elapsed,
                    pooledWatch.Elapsed,
                    directAllocatedBytes,
                    pooledAllocatedBytes,
                    pool.CreatedCount,
                    allocationMeasurementAvailable);
            }
        }

        private void WarmUp()
        {
            for (var i = 0; i < 128; i++)
            {
                var processor =
                    new TaskProcessor(
                        "benchmark.warmup",
                        _registry);

                Consume(processor);
            }
        }

        private static bool IsAllocationMeasurementAvailable()
        {
#if NETSTANDARD2_1
            return true;
#else
            return false;
#endif
        }

        private static long GetAllocatedBytes()
        {
#if NETSTANDARD2_1
            return GC.GetAllocatedBytesForCurrentThread();
#else
            return 0;
#endif
        }

        private static void Consume(
            ITaskProcessor processor)
        {
            if (processor == null)
                throw new InvalidOperationException();
        }
    }
}