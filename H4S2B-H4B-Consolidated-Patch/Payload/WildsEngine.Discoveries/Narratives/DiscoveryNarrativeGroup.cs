using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WildsEngine.Discoveries.Abstractions.Clusters;
using WildsEngine.Discoveries.Abstractions.Narratives;

namespace WildsEngine.Discoveries.Narratives
{
    public sealed class DiscoveryNarrativeGroup : IDiscoveryNarrativeGroup
    {
        public DiscoveryNarrativeGroup(
            IReadOnlyList<IDiscoveryClusterRecognitionResult> members)
        {
            if (members == null) throw new ArgumentNullException(nameof(members));
            if (members.Count == 0)
                throw new ArgumentException("A narrative group requires at least one member.", nameof(members));
            var copy = new IDiscoveryClusterRecognitionResult[members.Count];
            var unique = new HashSet<IDiscoveryClusterRecognitionResult>(ReferenceEqualityComparer<IDiscoveryClusterRecognitionResult>.Instance);
            for (var index = 0; index < members.Count; index++)
            {
                var member = members[index] ??
                    throw new ArgumentException("Narrative group members cannot contain null.", nameof(members));
                if (!unique.Add(member))
                    throw new ArgumentException("Narrative group members must be unique references.", nameof(members));
                copy[index] = member;
            }
            Members = new ReadOnlyCollection<IDiscoveryClusterRecognitionResult>(copy);
        }

        public IReadOnlyList<IDiscoveryClusterRecognitionResult> Members { get; }

        private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
            where T : class
        {
            public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();
            public bool Equals(T? x, T? y) => ReferenceEquals(x, y);
            public int GetHashCode(T obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
