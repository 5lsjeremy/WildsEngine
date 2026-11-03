using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Observes or transforms a snapshot of Neighbor
    /// relationships belonging to a Wilds object.
    ///
    /// Operators do not own or mutate the source collection.
    /// </summary>
    public interface INeighborOperator<TResult>
    {
        TResult Apply(
            IBaseObject subject,
            IReadOnlyList<INeighborRelationship> relationships);
    }
}
