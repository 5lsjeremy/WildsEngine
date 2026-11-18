using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;

namespace WildsEngine.Discoveries.Narratives
{
    public sealed class DiscoveryNarrativeCompositionResult : IDiscoveryNarrativeCompositionResult
    {
        public DiscoveryNarrativeCompositionResult(
            Guid id,
            IDiscoveryRecognizedGeometryPopulation population,
            IDiscoveryNarrativeRegistration registration,
            IReadOnlyList<IDiscoveryNarrativeGroup> groups,
            IReadOnlyList<IDiscoveryNarrativeCompositionDecision> decisions,
            DateTimeOffset composedAt)
        {
            if (id == Guid.Empty) throw new ArgumentException("Composition identity is required.", nameof(id));
            Population = population ?? throw new ArgumentNullException(nameof(population));
            Registration = registration ?? throw new ArgumentNullException(nameof(registration));
            if (!string.Equals(population.Scope, registration.Scope, StringComparison.Ordinal))
                throw new ArgumentException("Population and narrative registration scopes must match.");
            if (groups == null) throw new ArgumentNullException(nameof(groups));
            if (groups.Count == 0) throw new ArgumentException("At least one narrative group is required.", nameof(groups));
            if (decisions == null) throw new ArgumentNullException(nameof(decisions));

            var source = new HashSet<IDiscoveryClusterRecognitionResult>(Ref.Instance);
            foreach (var member in population.Members) source.Add(member);
            var assigned = new HashSet<IDiscoveryClusterRecognitionResult>(Ref.Instance);
            var groupCopy = new IDiscoveryNarrativeGroup[groups.Count];
            for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                var group = groups[groupIndex] ?? throw new ArgumentException("Groups cannot contain null.", nameof(groups));
                foreach (var member in group.Members)
                {
                    if (!source.Contains(member)) throw new ArgumentException("Every group member must come from the exact source population.", nameof(groups));
                    if (!assigned.Add(member)) throw new ArgumentException("A source member cannot appear in more than one group.", nameof(groups));
                }
                groupCopy[groupIndex] = group;
            }
            if (assigned.Count != source.Count)
                throw new ArgumentException("Groups must form an exact partition of the source population.", nameof(groups));

            var decisionCopy = new IDiscoveryNarrativeCompositionDecision[decisions.Count];
            for (var index = 0; index < decisions.Count; index++)
            {
                var decision = decisions[index] ?? throw new ArgumentException("Decisions cannot contain null.", nameof(decisions));
                if (!source.Contains(decision.Left) || !source.Contains(decision.Right))
                    throw new ArgumentException("Every decision must reference exact source-population members.", nameof(decisions));
                decisionCopy[index] = decision;
            }

            Id = id;
            Groups = new ReadOnlyCollection<IDiscoveryNarrativeGroup>(groupCopy);
            Decisions = new ReadOnlyCollection<IDiscoveryNarrativeCompositionDecision>(decisionCopy);
            ComposedAt = composedAt;
        }

        public Guid Id { get; }
        public IDiscoveryRecognizedGeometryPopulation Population { get; }
        public IDiscoveryNarrativeRegistration Registration { get; }
        public IReadOnlyList<IDiscoveryNarrativeGroup> Groups { get; }
        public IReadOnlyList<IDiscoveryNarrativeCompositionDecision> Decisions { get; }
        public DateTimeOffset ComposedAt { get; }

        private sealed class Ref : IEqualityComparer<IDiscoveryClusterRecognitionResult>
        {
            public static readonly Ref Instance = new Ref();
            public bool Equals(IDiscoveryClusterRecognitionResult? x, IDiscoveryClusterRecognitionResult? y) => ReferenceEquals(x, y);
            public int GetHashCode(IDiscoveryClusterRecognitionResult obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
