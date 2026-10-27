using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Objects
{
    public abstract class BaseObject : IBaseObject
    {
        private readonly List<IBaseObject> _neighbors =
            new List<IBaseObject>();

        protected BaseObject(string layer)
            : this(Guid.NewGuid(), layer)
        {
        }

        protected BaseObject(Guid id, string layer)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Wilds objects require a non-empty GUID.",
                    nameof(id));

            if (string.IsNullOrWhiteSpace(layer))
                throw new ArgumentException(
                    "Wilds objects require a layer.",
                    nameof(layer));

            Id = id;
            Slug = BuildSlug(id, layer);
        }

        public Guid Id { get; }

        public string Slug { get; private set; }

        public IReadOnlyList<IBaseObject> Neighbors =>
            _neighbors.AsReadOnly();

        public abstract IComponent Component { get; }

        // Component shortcuts.
        // These intentionally do NOT belong to IBaseObject.

        public virtual IState State =>
            Component.State;

        public virtual ISignal Signal =>
            Component.Signal;

        public virtual IDictionary<string, object?> Metadata =>
            Component.Metadata;

        public string AppendSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Slug;

            var segment = value.Trim().Trim('.');

            if (segment.Length == 0)
                return Slug;

            Slug = $"{Slug}.{segment}";

            return Slug;
        }

        public bool AddNeighbor(IBaseObject neighbor)
        {
            if (neighbor == null)
                throw new ArgumentNullException(nameof(neighbor));

            if (neighbor.Id == Id)
                return false;

            for (var i = 0; i < _neighbors.Count; i++)
            {
                if (_neighbors[i].Id == neighbor.Id)
                    return false;
            }

            _neighbors.Add(neighbor);

            return true;
        }

        public bool RemoveNeighbor(Guid id)
        {
            for (var i = 0; i < _neighbors.Count; i++)
            {
                if (_neighbors[i].Id != id)
                    continue;

                _neighbors.RemoveAt(i);

                return true;
            }

            return false;
        }

        private static string BuildSlug(Guid id, string layer)
        {
            var raw = id.ToString("N");

            var first = raw.Substring(0, 3);
            var last = raw.Substring(raw.Length - 3, 3);

            return $"{first}{last}-{layer.Trim()}";
        }
    }
}
