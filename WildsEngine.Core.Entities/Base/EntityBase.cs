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

        protected EntityBase(
            string slug)
            : base(slug)
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

            if (_facts.Contains(fact))
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
    }
}