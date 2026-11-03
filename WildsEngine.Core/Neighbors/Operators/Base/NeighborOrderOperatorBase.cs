using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Neighbors.Operators
{
    /// <summary>
    /// Orders a copied relationship snapshot through a reusable
    /// comparer and never mutates the source sequence.
    /// </summary>
    public abstract class NeighborOrderOperatorBase :
        NeighborSequenceOperatorBase
    {
        private readonly INeighborComparer _comparer;

        protected NeighborOrderOperatorBase(
            INeighborComparer comparer)
        {
            _comparer =
                comparer ??
                throw new ArgumentNullException(
                    nameof(comparer));
        }

        protected sealed override
            IReadOnlyList<INeighborRelationship> ApplyCore(
                IBaseObject subject,
                IReadOnlyList<INeighborRelationship> relationships)
        {
            var result =
                new List<INeighborRelationship>(
                    relationships.Count);

            for (var index = 0;
                 index < relationships.Count;
                 index++)
            {
                result.Add(
                    relationships[index]);
            }

            result.Sort(
                (left, right) =>
                    _comparer.Compare(
                        subject,
                        left,
                        right));

            return Snapshot(
                result);
        }
    }
}
