using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Objects
{
    public abstract class BaseObject :
        IBaseObject
    {
        private readonly NeighborCollection _neighbors;

        protected BaseObject(
            string layer)
            : this(
                Guid.NewGuid(),
                layer)
        {
        }

        protected BaseObject(
            Guid id,
            string layer)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Wilds objects require a non-empty GUID.",
                    nameof(id));
            }

            if (string.IsNullOrWhiteSpace(layer))
            {
                throw new ArgumentException(
                    "Wilds objects require a layer.",
                    nameof(layer));
            }

            Id = id;

            Slug =
                BuildSlug(
                    id,
                    layer);

            _neighbors =
                new NeighborCollection(
                    this);
        }

        public Guid Id { get; }

        public string Slug { get; private set; }

        public INeighborCollection Neighbors =>
            _neighbors;

        public abstract IComponent Component { get; }

        // Component shortcuts.
        // These intentionally do NOT belong to IBaseObject.

        public virtual IState State =>
            Component.State;

        public virtual ISignal Signal =>
            Component.Signal;

        public virtual IDictionary<string, object?> Metadata =>
            Component.Metadata;

        public string AppendSlug(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Slug;
            }

            var segment =
                value
                    .Trim()
                    .Trim('.');

            if (segment.Length == 0)
            {
                return Slug;
            }

            Slug =
                $"{Slug}.{segment}";

            return Slug;
        }

        public TResult ApplyNeighborOperator<TResult>(
            INeighborOperator<TResult> neighborOperator)
        {
            if (neighborOperator == null)
            {
                throw new ArgumentNullException(
                    nameof(neighborOperator));
            }

            return neighborOperator.Apply(
                this,
                _neighbors.Relationships);
        }

        private static string BuildSlug(
            Guid id,
            string layer)
        {
            var raw =
                id.ToString(
                    "N");

            var first =
                raw.Substring(
                    0,
                    3);

            var last =
                raw.Substring(
                    raw.Length - 3,
                    3);

            return
                $"{first}{last}-{layer.Trim()}";
        }
    }
}