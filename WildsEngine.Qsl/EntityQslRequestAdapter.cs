using System;
using System.Collections.Generic;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Packages;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Qsl;

namespace WildsEngine.Qsl
{
    public sealed class EntityQslRequestAdapter
    {
        public WildsQslRequest CreateRequest(
            string source,
            IReadOnlyList<IEntitySurface> surfaces,
            IReadOnlyDictionary<
                IEntitySurface,
                IReadOnlyList<IFact>> attachedFacts)
        {
            if (string.IsNullOrWhiteSpace(
                    source))
            {
                throw new ArgumentException(
                    "QSL source cannot be empty.",
                    nameof(source));
            }

            if (surfaces == null)
            {
                throw new ArgumentNullException(
                    nameof(surfaces));
            }

            if (attachedFacts == null)
            {
                throw new ArgumentNullException(
                    nameof(attachedFacts));
            }

            var consumers =
                new List<QslConsumerBinding>(
                    surfaces.Count);

            var packages =
                new List<QslStatePackage>();

            for (var index = 0;
                 index < surfaces.Count;
                 index++)
            {
                var surface =
                    surfaces[index] ??
                    throw new ArgumentException(
                        "Surfaces cannot contain null.",
                        nameof(surfaces));

                consumers.Add(
                    new QslConsumerBinding(
                        surface.Alias,
                        surface.ConsumerType,
                        surface.Target));

                if (!attachedFacts.TryGetValue(
                        surface,
                        out var facts) ||
                    facts.Count == 0)
                {
                    continue;
                }

                var properties =
                    new Dictionary<string, object?>(
                        StringComparer.Ordinal);

                for (var factIndex = 0;
                     factIndex < facts.Count;
                     factIndex++)
                {
                    var fact =
                        facts[factIndex] ??
                        throw new ArgumentException(
                            "Attached Facts cannot contain null.",
                            nameof(attachedFacts));

                    if (!ReferenceEquals(
                            fact.Entity,
                            surface.Entity))
                    {
                        throw new ArgumentException(
                            "Attached Facts must belong to the " +
                            "exact Surface Entity.",
                            nameof(attachedFacts));
                    }

                    properties.Add(
                        fact.Path,
                        fact.Value);
                }

                packages.Add(
                    new QslStatePackage(
                        surface.Target,
                        properties));
            }

            return new WildsQslRequest(
                source.Trim(),
                consumers,
                packages);
        }
    }
}