using System;
using System.Collections.Generic;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Base
{
    public abstract class EntityBase :
        IdentifierBase,
        IEntity
    {
        private readonly List<IFact> _facts =
            new List<IFact>();

        private readonly List<IDiscovery> _discoveries =
            new List<IDiscovery>();

        protected EntityBase(
            string slug)
            : base(
                slug)
        {
        }

        protected EntityBase(
            Guid id,
            string slug)
            : base(
                id,
                slug)
        {
        }

        public IReadOnlyList<IFact> Facts =>
            _facts;

        public IReadOnlyList<IDiscovery> Discoveries =>
            _discoveries;

        protected bool AddFact(
            IFact fact)
        {
            if (fact == null)
            {
                throw new ArgumentNullException(
                    nameof(fact));
            }

            if (!ReferenceEquals(
                    fact.Entity,
                    this))
            {
                throw new ArgumentException(
                    "A Fact must belong to this exact Entity.",
                    nameof(fact));
            }

            if (_facts.Contains(
                    fact))
            {
                return false;
            }

            _facts.Add(
                fact);

            return true;
        }

        protected bool RemoveFact(
            IFact fact)
        {
            if (fact == null)
            {
                throw new ArgumentNullException(
                    nameof(fact));
            }

            return _facts.Remove(
                fact);
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
                    discovery.Entity,
                    this))
            {
                throw new ArgumentException(
                    "A Discovery must belong to this exact Entity.",
                    nameof(discovery));
            }

            for (var index = 0;
                 index < discovery.Facts.Count;
                 index++)
            {
                if (!ReferenceEquals(
                        discovery.Facts[index].Entity,
                        this))
                {
                    throw new ArgumentException(
                        "Every Discovery Fact must belong to " +
                        "this exact Entity.",
                        nameof(discovery));
                }
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