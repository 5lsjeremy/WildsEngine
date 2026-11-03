using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class FarthestNeighbors : NeighborSamplerBase
    {
        private static readonly NeighborDistanceDescendingComparer Comparer = new NeighborDistanceDescendingComparer();
        public FarthestNeighbors(int count) : base(count) { }

        protected override IReadOnlyList<INeighborRelationship> SampleCore(IBaseObject subject, IReadOnlyList<INeighborRelationship> relationships, int count)
        {
            var copy = new List<INeighborRelationship>(relationships.Count);
            for (var i = 0; i < relationships.Count; i++) copy.Add(relationships[i]);
            copy.Sort((a, b) => Comparer.Compare(subject, a, b));
            if (copy.Count > count) copy.RemoveRange(count, copy.Count - count);
            return copy.ToArray();
        }
    }
}
