using System;
using System.Collections.Generic;
using System.Globalization;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NeighborDistanceBandGroups : NeighborGroupingOperatorBase
    {
        public NeighborDistanceBandGroups(double bandSize)
        {
            BandSize = NeighborDistance.Validate(bandSize, nameof(bandSize));
            if (BandSize == 0d) throw new ArgumentOutOfRangeException(nameof(bandSize), "Distance band size must be greater than zero.");
        }
        public double BandSize { get; }

        protected override IReadOnlyDictionary<string, IReadOnlyList<INeighborRelationship>> ApplyCore(IBaseObject subject, IReadOnlyList<INeighborRelationship> relationships)
        {
            var groups = new Dictionary<string, List<INeighborRelationship>>(StringComparer.Ordinal);
            for (var i = 0; i < relationships.Count; i++)
            {
                var r = relationships[i];
                var band = Math.Floor(r.Distance / BandSize);
                var min = band * BandSize;
                var max = min + BandSize;
                var key = min.ToString("R", CultureInfo.InvariantCulture) + ".." + max.ToString("R", CultureInfo.InvariantCulture);
                List<INeighborRelationship> group;
                if (!groups.TryGetValue(key, out group!)) { group = new List<INeighborRelationship>(); groups.Add(key, group); }
                group.Add(r);
            }
            return Freeze(groups);
        }
    }
}
