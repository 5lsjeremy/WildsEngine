using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WildsEngine.Discoveries.Abstractions.Vectorization
{
    public sealed class DiscoveryVectorizationServiceResult
    {
        public DiscoveryVectorizationServiceResult(
            IReadOnlyList<IDiscoveryVectorContribution> contributions)
        {
            if (contributions == null) throw new ArgumentNullException(nameof(contributions));
            var snapshot = new IDiscoveryVectorContribution[contributions.Count];
            for (var index = 0; index < contributions.Count; index++)
                snapshot[index] = contributions[index] ?? throw new ArgumentException(
                    "Vectorization results cannot contain null contributions.", nameof(contributions));
            Contributions = new ReadOnlyCollection<IDiscoveryVectorContribution>(snapshot);
        }
        public IReadOnlyList<IDiscoveryVectorContribution> Contributions { get; }
    }
}
