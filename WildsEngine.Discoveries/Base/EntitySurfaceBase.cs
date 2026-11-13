using System;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Discoveries.Base
{
    public abstract class EntitySurfaceBase :
        IEntitySurface
    {
        protected EntitySurfaceBase(
            IEntity entity,
            object target,
            string alias,
            string consumerType)
        {
            Entity =
                entity ??
                throw new ArgumentNullException(
                    nameof(entity));

            Target =
                target ??
                throw new ArgumentNullException(
                    nameof(target));

            Alias =
                RequireVocabulary(
                    alias,
                    nameof(alias));

            ConsumerType =
                RequireVocabulary(
                    consumerType,
                    nameof(consumerType));
        }

        public IEntity Entity { get; }

        public object Target { get; }

        public string Alias { get; }

        public string ConsumerType { get; }

        protected void RequireOwnedFact(
            IFact fact)
        {
            if (fact == null)
            {
                throw new ArgumentNullException(
                    nameof(fact));
            }

            if (!ReferenceEquals(
                    fact.Entity,
                    Entity))
            {
                throw new ArgumentException(
                    "The Fact must belong to the exact Surface Entity.",
                    nameof(fact));
            }
        }

        protected void RequireOwnedDiscovery(
            IDiscovery discovery)
        {
            if (discovery == null)
            {
                throw new ArgumentNullException(
                    nameof(discovery));
            }

            if (!ReferenceEquals(
                    discovery.Entity,
                    Entity))
            {
                throw new ArgumentException(
                    "The Discovery must belong to the exact Surface Entity.",
                    nameof(discovery));
            }

            for (var index = 0;
                 index < discovery.Facts.Count;
                 index++)
            {
                RequireOwnedFact(
                    discovery.Facts[index]);
            }
        }

        private static string RequireVocabulary(
            string value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Surface vocabulary cannot be empty.",
                    parameterName);
            }

            return value.Trim();
        }
    }
}
