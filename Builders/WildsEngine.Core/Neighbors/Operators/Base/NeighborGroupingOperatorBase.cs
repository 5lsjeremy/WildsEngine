using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Base class for operators that group relationships under
    /// deterministic string keys.
    /// </summary>
    public abstract class NeighborGroupingOperatorBase :
        NeighborOperatorBase<
            IReadOnlyDictionary<
                string,
                IReadOnlyList<INeighborRelationship>>>,
        INeighborGroupingOperator
    {
        protected static IReadOnlyDictionary<
            string,
            IReadOnlyList<INeighborRelationship>> Freeze(
                Dictionary<
                    string,
                    List<INeighborRelationship>> groups)
        {
            var frozen =
                new Dictionary<
                    string,
                    IReadOnlyList<INeighborRelationship>>(
                        groups.Count,
                        System.StringComparer.Ordinal);

            foreach (var pair in groups)
            {
                frozen.Add(
                    pair.Key,
                    pair.Value.ToArray());
            }

            return frozen;
        }
    }
}
