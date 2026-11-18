using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Abstractions.Vectorization;
using WildsEngine.Discoveries.Vectors;

namespace WildsEngine.Discoveries.Vectorization
{
    internal sealed class DiscoveryVectorizer : IDiscoveryVectorizer
    {
        private readonly IDiscoveryVectorizationService _service;
        private readonly IDiscoveryVectorMetric _metric;

        public DiscoveryVectorizer(
            IDiscoveryVectorizationService service,
            IDiscoveryVectorMetric metric)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _metric = metric ?? throw new ArgumentNullException(nameof(metric));
        }

        public async Task<IDiscoveryVectorizationResult> VectorizeAsync(
            IDiscoveryVectorInput input,
            object? provenance = null,
            Guid? parentExecutionId = null,
            Guid? pulseId = null,
            CancellationToken cancellationToken = default)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            var serviceResult = await _service.ExecuteAsync(
                new DiscoveryVectorizationRequest(input),
                provenance,
                parentExecutionId,
                pulseId,
                cancellationToken);

            var contributions = serviceResult.Value.Contributions;
            ValidateExactInput(input, contributions);
            var components = Aggregate(contributions);
            var vector = new DiscoveryVector(
                components,
                contributions,
                new List<IDiscoveryVectorMotion>(),
                _metric,
                DateTimeOffset.UtcNow);
            return new DiscoveryVectorizationResult(serviceResult.ExecutionId, vector);
        }

        private static void ValidateExactInput(
            IDiscoveryVectorInput input,
            IReadOnlyList<IDiscoveryVectorContribution> contributions)
        {
            if (contributions.Count == 0)
                throw new InvalidOperationException("Vectorization service returned no contributions.");
            for (var index = 0; index < contributions.Count; index++)
                if (!ReferenceEquals(input, contributions[index].Input))
                    throw new InvalidOperationException(
                        "Every vectorization contribution must reference the exact requested input.");
        }

        private static IReadOnlyList<IDiscoveryVectorComponent> Aggregate(
            IReadOnlyList<IDiscoveryVectorContribution> contributions)
        {
            var totals = new SortedDictionary<string, double>(StringComparer.Ordinal);
            for (var index = 0; index < contributions.Count; index++)
            {
                var contribution = contributions[index];
                if (string.IsNullOrWhiteSpace(contribution.Dimension))
                    throw new ArgumentException("Contribution dimension cannot be empty.", nameof(contributions));
                if (double.IsNaN(contribution.Value) || double.IsInfinity(contribution.Value))
                    throw new ArgumentOutOfRangeException(nameof(contributions), "Contribution value must be finite.");
                if (double.IsNaN(contribution.Weight) || double.IsInfinity(contribution.Weight) || contribution.Weight < 0.0)
                    throw new ArgumentOutOfRangeException(nameof(contributions), "Contribution weight must be finite and non-negative.");
                var weighted = contribution.Value * contribution.Weight;
                if (double.IsNaN(weighted) || double.IsInfinity(weighted))
                    throw new ArgumentOutOfRangeException(nameof(contributions), "Weighted contribution must be finite.");
                totals.TryGetValue(contribution.Dimension, out var current);
                var next = current + weighted;
                if (double.IsNaN(next) || double.IsInfinity(next))
                    throw new ArgumentOutOfRangeException(nameof(contributions), "Aggregated component must be finite.");
                totals[contribution.Dimension] = next;
            }
            var components = new List<IDiscoveryVectorComponent>(totals.Count);
            foreach (var pair in totals)
                components.Add(new DiscoveryVectorComponent(pair.Key, pair.Value));
            return components;
        }
    }
}
