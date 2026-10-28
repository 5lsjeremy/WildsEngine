using System;

namespace WildsEngine.Benchmarks
{
    /// <summary>
    /// Contains the results of a processor pooling benchmark.
    /// </summary>
    public sealed class ProcessorPoolingBenchmarkResult
    {
        public ProcessorPoolingBenchmarkResult(
            int iterations,
            TimeSpan directDuration,
            TimeSpan pooledDuration,
            long directAllocatedBytes,
            long pooledAllocatedBytes,
            int pooledInstancesCreated,
            bool allocationMeasurementAvailable)
        {
            Iterations = iterations;
            DirectDuration = directDuration;
            PooledDuration = pooledDuration;
            DirectAllocatedBytes =
                directAllocatedBytes;
            PooledAllocatedBytes =
                pooledAllocatedBytes;
            PooledInstancesCreated =
                pooledInstancesCreated;
            AllocationMeasurementAvailable =
                allocationMeasurementAvailable;
        }

        public int Iterations { get; }

        public TimeSpan DirectDuration { get; }

        public TimeSpan PooledDuration { get; }

        public long DirectAllocatedBytes { get; }

        public long PooledAllocatedBytes { get; }

        public int PooledInstancesCreated { get; }

        public bool AllocationMeasurementAvailable { get; }

        public override string ToString()
        {
            var allocations =
                AllocationMeasurementAvailable
                    ? $"Direct allocation: {DirectAllocatedBytes} bytes{Environment.NewLine}" +
                      $"Pooled allocation: {PooledAllocatedBytes} bytes{Environment.NewLine}"
                    : $"Allocation measurement: unavailable{Environment.NewLine}";

            return
                $"Iterations: {Iterations}{Environment.NewLine}" +
                $"Direct duration: {DirectDuration.TotalMilliseconds:F3} ms{Environment.NewLine}" +
                $"Pooled duration: {PooledDuration.TotalMilliseconds:F3} ms{Environment.NewLine}" +
                allocations +
                $"Pooled processors created: {PooledInstancesCreated}";
        }
    }
}