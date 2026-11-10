using System;
using System.Collections.Generic;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Base
{
    public abstract class FactBase :
        IdentifierBase,
        IFact
    {
        private readonly List<IDiscovery> _discoveries =
            new List<IDiscovery>();

        protected FactBase(
            string slug,
            IEntity entity,
            string path,
            object? value)
            : base(slug)
        {
            Entity =
                entity ??
                throw new ArgumentNullException(
                    nameof(entity));

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "A Fact requires a path.",
                    nameof(path));
            }

            Path =
                path.Trim();

            Value =
                value;
        }

        protected FactBase(
            Guid id,
            string slug,
            IEntity entity,
            string path,
            object? value)
            : base(
                id,
                slug)
        {
            Entity =
                entity ??
                throw new ArgumentNullException(
                    nameof(entity));

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "A Fact requires a path.",
                    nameof(path));
            }

            Path =
                path.Trim();

            Value =
                value;
        }

        public IEntity Entity { get; }

        public string Path { get; }

        public object? Value { get; private set; }

        public IReadOnlyList<IDiscovery> Discoveries =>
            _discoveries;

        protected void SetValue(
            object? value)
        {
            Value =
                value;
        }

        protected bool AddDiscovery(
            IDiscovery discovery)
        {
            if (discovery == null)
            {
                throw new ArgumentNullException(
                    nameof(discovery));
            }

            if (!ReferenceEquals(
                    discovery.Fact,
                    this))
            {
                throw new ArgumentException(
                    "A Discovery must belong to this exact Fact.",
                    nameof(discovery));
            }

            if (_discoveries.Contains(
                    discovery))
            {
                return false;
            }

            _discoveries.Add(
                discovery);

            return true;
        }

        protected bool RemoveDiscovery(
            IDiscovery discovery)
        {
            if (discovery == null)
            {
                throw new ArgumentNullException(
                    nameof(discovery));
            }

            return _discoveries.Remove(
                discovery);
        }
    }
}