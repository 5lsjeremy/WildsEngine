using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    public sealed class NeighborMembershipGroups : NeighborGroupingOperatorBase
    {
        protected override IReadOnlyDictionary<string, IReadOnlyList<INeighborRelationship>> ApplyCore(IBaseObject subject, IReadOnlyList<INeighborRelationship> relationships)
        {
            var groups = new Dictionary<string, List<INeighborRelationship>>(System.StringComparer.Ordinal);
            for (var i = 0; i < relationships.Count; i++)
            {
                var r = relationships[i];
                List<INeighborRelationship> group;
                if (!groups.TryGetValue(r.Membership, out group!))
                {
                    group = new List<INeighborRelationship>();
                    groups.Add(r.Membership, group);
                }
                group.Add(r);
            }
            return Freeze(groups);
        }
    }
}
