using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries
{
    /// <summary>
    /// Projects selected Entity-owned Facts through the
    /// existing Wilds IEmissionDataSource contract.
    ///
    /// Facts remain authoritative. Values are read from
    /// the exact Fact when requested.
    ///
    /// This type performs no emission processing.
    /// </summary>
    public sealed class EntityEmissionDataSource :
        IEmissionDataSource
    {
        private readonly IEntity _entity;

        private readonly Dictionary<string, IFact> _properties =
            new Dictionary<string, IFact>(
                StringComparer.Ordinal);

        private readonly Dictionary<
            string,
            Dictionary<string, IFact>> _groups =
                new Dictionary<
                    string,
                    Dictionary<string, IFact>>(
                        StringComparer.Ordinal);

        public EntityEmissionDataSource(
            IEntity entity)
        {
            _entity =
                entity ??
                throw new ArgumentNullException(
                    nameof(entity));
        }

        public IEntity Entity =>
            _entity;

        public bool ExposeProperty(
            string propertyName,
            IFact fact)
        {
            RequireName(
                propertyName,
                nameof(propertyName));

            RequireOwnedFact(
                fact);

            return _properties.TryAdd(
                propertyName.Trim(),
                fact);
        }

        public bool ExposeGroupProperty(
            string groupName,
            string propertyName,
            IFact fact)
        {
            RequireName(
                groupName,
                nameof(groupName));

            RequireName(
                propertyName,
                nameof(propertyName));

            RequireOwnedFact(
                fact);

            var normalizedGroup =
                groupName.Trim();

            if (!_groups.TryGetValue(
                    normalizedGroup,
                    out var group))
            {
                group =
                    new Dictionary<string, IFact>(
                        StringComparer.Ordinal);

                _groups.Add(
                    normalizedGroup,
                    group);
            }

            return group.TryAdd(
                propertyName.Trim(),
                fact);
        }

        public bool TryGetProperty(
            string propertyName,
            out object? value)
        {
            if (string.IsNullOrWhiteSpace(
                    propertyName))
            {
                value =
                    null;

                return false;
            }

            if (_properties.TryGetValue(
                    propertyName,
                    out var fact))
            {
                value =
                    fact.Value;

                return true;
            }

            value =
                null;

            return false;
        }

        public bool TryGetGroup(
            string groupName,
            out IReadOnlyDictionary<string, object?> values)
        {
            if (string.IsNullOrWhiteSpace(
                    groupName) ||
                !_groups.TryGetValue(
                    groupName,
                    out var group))
            {
                values =
                    new Dictionary<string, object?>();

                return false;
            }

            var snapshot =
                new Dictionary<string, object?>(
                    StringComparer.Ordinal);

            foreach (var pair in group)
            {
                snapshot.Add(
                    pair.Key,
                    pair.Value.Value);
            }

            values =
                snapshot;

            return true;
        }

        private void RequireOwnedFact(
            IFact fact)
        {
            if (fact == null)
            {
                throw new ArgumentNullException(
                    nameof(fact));
            }

            if (!ReferenceEquals(
                    fact.Entity,
                    _entity))
            {
                throw new ArgumentException(
                    "Emission Facts must belong to the exact Entity.",
                    nameof(fact));
            }
        }

        private static void RequireName(
            string value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                throw new ArgumentException(
                    "Emission vocabulary cannot be empty.",
                    parameterName);
            }
        }
    }
}