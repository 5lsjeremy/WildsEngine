using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class DeterministicNeighborSample : NeighborSamplerBase
    {
        public DeterministicNeighborSample(int count, int seed) : base(count) => Seed = seed;
        public int Seed { get; }

        protected override IReadOnlyList<INeighborRelationship> SampleCore(IBaseObject subject, IReadOnlyList<INeighborRelationship> relationships, int count)
        {
            var copy = new List<INeighborRelationship>(relationships.Count);
            for (var i = 0; i < relationships.Count; i++) copy.Add(relationships[i]);
            var random = new Random(Seed);
            for (var i = copy.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                var temp = copy[i]; copy[i] = copy[j]; copy[j] = temp;
            }
            if (copy.Count > count) copy.RemoveRange(count, copy.Count - count);
            return copy.ToArray();
        }
    }
}
