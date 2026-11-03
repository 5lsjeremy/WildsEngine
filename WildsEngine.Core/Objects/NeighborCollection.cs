using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Topology;

namespace WildsEngine.Core.Objects
{
    /// <summary>
    /// Owns the directional Neighbor relationships belonging
    /// to one Wilds object.
    ///
    /// Relationship identity is defined by:
    ///
    ///     Neighbor.Id + Membership
    ///
    /// Distance describes relationship state and may therefore
    /// change without changing relationship identity.
    /// </summary>
    public sealed class NeighborCollection :
        INeighborCollection
    {
        private readonly IBaseObject _owner;

        private readonly object _sync =
            new object();

        private readonly List<INeighborRelationship> _relationships =
            new List<INeighborRelationship>();

        public NeighborCollection(
            IBaseObject owner)
        {
            _owner =
                owner ??
                throw new ArgumentNullException(
                    nameof(owner));
        }

        public int Count
        {
            get
            {
                lock (_sync)
                {
                    return _relationships.Count;
                }
            }
        }

        public IReadOnlyList<INeighborRelationship> Relationships
        {
            get
            {
                lock (_sync)
                {
                    return _relationships.ToArray();
                }
            }
        }

        public bool Add(
            IBaseObject neighbor,
            string membership,
            double distance)
        {
            if (neighbor == null)
            {
                throw new ArgumentNullException(
                    nameof(neighbor));
            }

            var normalizedMembership =
                ValidateMembership(
                    membership);

            NeighborDistance.Validate(
                distance,
                nameof(distance));

            if (neighbor.Id == _owner.Id)
            {
                return false;
            }

            lock (_sync)
            {
                if (FindIndex(
                        neighbor.Id,
                        normalizedMembership) >= 0)
                {
                    return false;
                }

                _relationships.Add(
                    new NeighborRelationship(
                        neighbor,
                        normalizedMembership,
                        distance));

                return true;
            }
        }

        public bool Contains(
            Guid neighborId,
            string membership)
        {
            if (neighborId == Guid.Empty)
            {
                return false;
            }

            var normalizedMembership =
                ValidateMembership(
                    membership);

            lock (_sync)
            {
                return FindIndex(
                           neighborId,
                           normalizedMembership) >= 0;
            }
        }

        public bool TryGet(
            Guid neighborId,
            string membership,
            out INeighborRelationship? relationship)
        {
            relationship = null;

            if (neighborId == Guid.Empty)
            {
                return false;
            }

            var normalizedMembership =
                ValidateMembership(
                    membership);

            lock (_sync)
            {
                var index =
                    FindIndex(
                        neighborId,
                        normalizedMembership);

                if (index < 0)
                {
                    return false;
                }

                relationship =
                    _relationships[index];

                return true;
            }
        }

        public bool SetDistance(
            Guid neighborId,
            string membership,
            double distance)
        {
            if (neighborId == Guid.Empty)
            {
                return false;
            }

            var normalizedMembership =
                ValidateMembership(
                    membership);

            NeighborDistance.Validate(
                distance,
                nameof(distance));

            lock (_sync)
            {
                var index =
                    FindIndex(
                        neighborId,
                        normalizedMembership);

                if (index < 0)
                {
                    return false;
                }

                var existing =
                    _relationships[index];

                if (existing.Distance == distance)
                {
                    return false;
                }

                _relationships[index] =
                    new NeighborRelationship(
                        existing.Neighbor,
                        existing.Membership,
                        distance);

                return true;
            }
        }

        public bool UpdateDistance(
            Guid neighborId,
            string membership,
            Func<double, double> update)
        {
            if (neighborId == Guid.Empty)
            {
                return false;
            }

            if (update == null)
            {
                throw new ArgumentNullException(
                    nameof(update));
            }

            var normalizedMembership =
                ValidateMembership(
                    membership);

            lock (_sync)
            {
                var index =
                    FindIndex(
                        neighborId,
                        normalizedMembership);

                if (index < 0)
                {
                    return false;
                }

                var existing =
                    _relationships[index];

                var updatedDistance =
                    update(
                        existing.Distance);

                NeighborDistance.Validate(
                    updatedDistance,
                    nameof(updatedDistance));

                if (existing.Distance ==
                    updatedDistance)
                {
                    return false;
                }

                _relationships[index] =
                    new NeighborRelationship(
                        existing.Neighbor,
                        existing.Membership,
                        updatedDistance);

                return true;
            }
        }

        public bool Remove(
            Guid neighborId,
            string membership)
        {
            if (neighborId == Guid.Empty)
            {
                return false;
            }

            var normalizedMembership =
                ValidateMembership(
                    membership);

            lock (_sync)
            {
                var index =
                    FindIndex(
                        neighborId,
                        normalizedMembership);

                if (index < 0)
                {
                    return false;
                }

                _relationships.RemoveAt(
                    index);

                return true;
            }
        }

        public int Remove(
            Guid neighborId)
        {
            if (neighborId == Guid.Empty)
            {
                return 0;
            }

            lock (_sync)
            {
                var removed = 0;

                for (var index =
                         _relationships.Count - 1;
                     index >= 0;
                     index--)
                {
                    if (_relationships[index]
                            .Neighbor
                            .Id != neighborId)
                    {
                        continue;
                    }

                    _relationships.RemoveAt(
                        index);

                    removed++;
                }

                return removed;
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _relationships.Clear();
            }
        }

        private int FindIndex(
            Guid neighborId,
            string membership)
        {
            for (var index = 0;
                 index < _relationships.Count;
                 index++)
            {
                var relationship =
                    _relationships[index];

                if (relationship.Neighbor.Id !=
                    neighborId)
                {
                    continue;
                }

                if (!string.Equals(
                        relationship.Membership,
                        membership,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                return index;
            }

            return -1;
        }

        private static string ValidateMembership(
            string membership)
        {
            if (string.IsNullOrWhiteSpace(
                    membership))
            {
                throw new ArgumentException(
                    "Neighbor relationships require a membership.",
                    nameof(membership));
            }

            return membership.Trim();
        }
    }
}