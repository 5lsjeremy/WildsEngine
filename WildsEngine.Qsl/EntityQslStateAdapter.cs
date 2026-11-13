using System;
using System.Collections.Generic;
using QSL.Core;
using QSL.Core.Runtime.Context;
using QSL.Core.Runtime.Packages;
using WildsEngine.Discoveries.Abstractions;

namespace WildsEngine.Qsl
{
    /// <summary>
    /// Connects Entity Surfaces and available Discoveries
    /// to the existing live QSL state lifecycle.
    ///
    /// One available Discovery may expose multiple proven
    /// Facts as one coherent QSL state package.
    ///
    /// This adapter owns no runtime state.
    ///
    /// QSL remains authoritative for:
    ///
    /// - consumer binding,
    /// - attached-state receipt,
    /// - attached-state updates,
    /// - attached-state removal,
    /// - consumer unbinding,
    /// - duplicate vocabulary rules,
    /// - runtime isolation.
    /// </summary>
    public sealed class EntityQslStateAdapter
    {
        private readonly QslEngine _engine;

        private readonly Dictionary<
            IEntitySurface,
            QslConsumerBinding> _bindings =
                new Dictionary<
                    IEntitySurface,
                    QslConsumerBinding>();

        public EntityQslStateAdapter(
            QslEngine engine)
        {
            _engine =
                engine ??
                throw new ArgumentNullException(
                    nameof(engine));
        }

        public QslEngine Engine =>
            _engine;

        public bool IsBound(
            IEntitySurface surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(
                    nameof(surface));
            }

            return _bindings.ContainsKey(
                surface);
        }

        public bool TryGetBinding(
            IEntitySurface surface,
            out QslConsumerBinding? binding)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(
                    nameof(surface));
            }

            if (_bindings.TryGetValue(
                    surface,
                    out var resolved))
            {
                binding =
                    resolved;

                return true;
            }

            binding =
                null;

            return false;
        }

        public QslConsumerBinding Bind(
            IEntitySurface surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(
                    nameof(surface));
            }

            if (_bindings.TryGetValue(
                    surface,
                    out var existing))
            {
                return existing;
            }

            var binding =
                new QslConsumerBinding(
                    surface.Alias,
                    surface.ConsumerType,
                    surface.Target);

            _engine.BindConsumer(
                binding);

            _bindings.Add(
                surface,
                binding);

            return binding;
        }

        /// <summary>
        /// Receives all proven Facts from one available
        /// Discovery as one coherent QSL state package.
        /// </summary>
        public bool Receive(
            IEntitySurface surface,
            IDiscovery discovery)
        {
            RequireBound(
                surface);

            RequireAvailableOwnedDiscovery(
                surface,
                discovery);

            var properties =
                CreateProvenFactProperties(
                    discovery);

            var package =
                new QslStatePackage(
                    surface.Target,
                    properties);

            var receivedCount =
                _engine.ReceiveState(
                    package);

            return receivedCount ==
                   properties.Count;
        }

        /// <summary>
        /// Updates every proven Fact currently exposed by
        /// the available Discovery.
        ///
        /// QSL remains responsible for rejecting updates to
        /// vocabulary that has not already been attached.
        /// </summary>
        public bool Update(
            IEntitySurface surface,
            IDiscovery discovery)
        {
            RequireBound(
                surface);

            RequireAvailableOwnedDiscovery(
                surface,
                discovery);

            var properties =
                CreateProvenFactProperties(
                    discovery);

            var updatedAll =
                true;

            foreach (var property in properties)
            {
                if (!_engine.UpdateState(
                        surface.Target,
                        property.Key,
                        property.Value))
                {
                    updatedAll =
                        false;
                }
            }

            return updatedAll;
        }

        /// <summary>
        /// Removes every Fact vocabulary member contributed
        /// by the Discovery.
        ///
        /// Removal does not require the Discovery to remain
        /// available because previously attached state must
        /// remain removable after its source falls below its
        /// resolution threshold.
        /// </summary>
        public bool Remove(
            IEntitySurface surface,
            IDiscovery discovery)
        {
            RequireBound(
                surface);

            RequireOwnedDiscovery(
                surface,
                discovery);

            if (discovery.Facts.Count == 0)
            {
                return false;
            }

            var removedAny =
                false;

            var removedAll =
                true;

            var names =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < discovery.Facts.Count;
                 index++)
            {
                var fact =
                    discovery.Facts[index] ??
                    throw new InvalidOperationException(
                        "A Discovery cannot contain a null Fact.");

                RequireOwnedFact(
                    surface,
                    fact);

                var name =
                    RequireFactName(
                        fact);

                if (!names.Add(
                        name))
                {
                    throw new InvalidOperationException(
                        "A Discovery cannot expose duplicate " +
                        "Fact names to QSL.");
                }

                var removed =
                    _engine.RemoveState(
                        surface.Target,
                        name);

                removedAny |=
                    removed;

                removedAll &=
                    removed;
            }

            return removedAny &&
                   removedAll;
        }

        public bool Remove(
            IEntitySurface surface)
        {
            RequireBound(
                surface);

            return _engine.RemoveState(
                surface.Target);
        }

        public bool Unbind(
            IEntitySurface surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(
                    nameof(surface));
            }

            if (!_bindings.TryGetValue(
                    surface,
                    out var binding))
            {
                return false;
            }

            var unbound =
                _engine.UnbindConsumer(
                    binding);

            if (!unbound)
            {
                return false;
            }

            _bindings.Remove(
                surface);

            return true;
        }

        private void RequireBound(
            IEntitySurface surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(
                    nameof(surface));
            }

            if (!_bindings.ContainsKey(
                    surface))
            {
                throw new InvalidOperationException(
                    "The Entity Surface must be bound before " +
                    "using the live QSL state lifecycle.");
            }
        }

        private static Dictionary<string, object?>
            CreateProvenFactProperties(
                IDiscovery discovery)
        {
            var properties =
                new Dictionary<string, object?>(
                    StringComparer.Ordinal);

            for (var index = 0;
                 index < discovery.Facts.Count;
                 index++)
            {
                var fact =
                    discovery.Facts[index] ??
                    throw new InvalidOperationException(
                        "A Discovery cannot contain a null Fact.");

                if (!fact.IsProven)
                {
                    continue;
                }

                var name =
                    RequireFactName(
                        fact);

                if (!properties.TryAdd(
                        name,
                        fact.Value))
                {
                    throw new InvalidOperationException(
                        "A Discovery cannot expose duplicate " +
                        "proven Fact names to QSL.");
                }
            }

            if (properties.Count == 0)
            {
                throw new InvalidOperationException(
                    "An available Discovery must expose at " +
                    "least one proven Fact.");
            }

            return properties;
        }

        private static void RequireOwnedDiscovery(
            IEntitySurface surface,
            IDiscovery discovery)
        {
            if (surface == null)
            {
                throw new ArgumentNullException(
                    nameof(surface));
            }

            if (discovery == null)
            {
                throw new ArgumentNullException(
                    nameof(discovery));
            }

            if (!ReferenceEquals(
                    discovery.Entity,
                    surface.Entity))
            {
                throw new ArgumentException(
                    "The Discovery must belong to the exact " +
                    "Surface Entity.",
                    nameof(discovery));
            }

            for (var index = 0;
                 index < discovery.Facts.Count;
                 index++)
            {
                var fact =
                    discovery.Facts[index] ??
                    throw new InvalidOperationException(
                        "A Discovery cannot contain a null Fact.");

                RequireOwnedFact(
                    surface,
                    fact);
            }
        }

        private static void RequireAvailableOwnedDiscovery(
            IEntitySurface surface,
            IDiscovery discovery)
        {
            RequireOwnedDiscovery(
                surface,
                discovery);

            if (!discovery.IsAvailable)
            {
                throw new InvalidOperationException(
                    "The Discovery must be available before " +
                    "its proven Facts can enter live QSL state.");
            }
        }

        private static void RequireOwnedFact(
            IEntitySurface surface,
            IFact fact)
        {
            if (!ReferenceEquals(
                    fact.Entity,
                    surface.Entity))
            {
                throw new ArgumentException(
                    "Every Discovery Fact must belong to the " +
                    "exact Surface Entity.",
                    nameof(fact));
            }
        }

        private static string RequireFactName(
            IFact fact)
        {
            if (string.IsNullOrWhiteSpace(
                    fact.Name))
            {
                throw new InvalidOperationException(
                    "Facts exposed to QSL require a name.");
            }

            return fact.Name.Trim();
        }
    }
}