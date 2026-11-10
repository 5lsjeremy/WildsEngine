using System;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Core.Entities.Base
{
    public abstract class IdentifierBase :
        IIdentifier
    {
        protected IdentifierBase(
            string slug)
            : this(
                Guid.NewGuid(),
                slug)
        {
        }

        protected IdentifierBase(
            Guid id,
            string slug)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Identifiers require a non-empty GUID.",
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(slug))
            {
                throw new ArgumentException(
                    "Identifiers require a slug.",
                    nameof(slug));
            }

            Id =
                id;

            Slug =
                slug.Trim();
        }

        public Guid Id { get; }

        public string Slug { get; }
    }
}