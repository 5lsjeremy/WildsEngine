using System;
using System.Collections.Generic;
using QSL.Core;
using QSL.Core.Runtime.Context;
using WildsEngine.Core.Entities.Abstractions;

namespace WildsEngine.Qsl
{
    /// <summary>
    /// Connects Entity Surfaces and available Discoveries
    /// to the existing live QSL state lifecycle.
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

        public bool Receive(
            IEntitySurface surface,
            IDiscovery discovery)
        {
            RequireBound(
                surface);

            RequireAvailableOwnedDiscovery(
                surface,
                discovery);

            return _engine.ReceiveState(
                surface.Target,
                discovery.Fact.Path,
                discovery.Fact.Value);
        }

        public bool Update(
            IEntitySurface surface,
            IDiscovery discovery)
        {
            RequireBound(
                surface);

            RequireAvailableOwnedDiscovery(
                surface,
                discovery);

            return _engine.UpdateState(
                surface.Target,
                discovery.Fact.Path,
                discovery.Fact.Value);
        }

        public bool Remove(
            IEntitySurface surface,
            IDiscovery discovery)
        {
            RequireBound(
                surface);

            RequireOwnedDiscovery(
                surface,
                discovery);

            return _engine.RemoveState(
                surface.Target,
                discovery.Fact.Path);
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

        private static void RequireOwnedDiscovery(
            IEntitySurface surface,
            IDiscovery discovery)
        {
            if (discovery == null)
            {
                throw new ArgumentNullException(
                    nameof(discovery));
            }

            if (!ReferenceEquals(
                    discovery.Fact.Entity,
                    surface.Entity))
            {
                throw new ArgumentException(
                    "The Discovery must originate from a Fact " +
                    "owned by the exact Surface Entity.",
                    nameof(discovery));
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
                    "its Fact can enter live QSL state.");
            }
        }
    }
}