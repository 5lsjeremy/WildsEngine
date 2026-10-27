using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Contexts
{
    public abstract class WildsContext :
        BaseObject,
        IContext
    {
        private readonly Dictionary<string, object?> _variables =
            new Dictionary<string, object?>();

        private readonly Dictionary<string, object?> _contextMetadata =
            new Dictionary<string, object?>();

        private readonly IComponent _component;

        protected WildsContext(
            IComponent component,
            string layer)
            : base(layer)
        {
            _component = component
                ?? throw new ArgumentNullException(
                    nameof(component));
        }

        protected WildsContext(
            Guid id,
            IComponent component,
            string layer)
            : base(id, layer)
        {
            _component = component
                ?? throw new ArgumentNullException(
                    nameof(component));
        }

        public sealed override IComponent Component =>
            _component;

        public IReadOnlyDictionary<string, object?> Variables =>
            _variables;

        IReadOnlyDictionary<string, object?> IContext.Metadata =>
            _contextMetadata;

        public virtual void SetVariable(
            string key,
            object? value)
        {
            ValidateKey(key);

            _variables[key] = value;
        }

        public virtual void SetMetadata(
            string key,
            object? value)
        {
            ValidateKey(key);

            _contextMetadata[key] = value;
        }

        protected bool RemoveVariable(string key)
        {
            ValidateKey(key);

            return _variables.Remove(key);
        }

        protected bool RemoveMetadata(string key)
        {
            ValidateKey(key);

            return _contextMetadata.Remove(key);
        }

        protected bool TryGetVariable(
            string key,
            out object? value)
        {
            ValidateKey(key);

            return _variables.TryGetValue(
                key,
                out value);
        }

        protected bool TryGetMetadata(
            string key,
            out object? value)
        {
            ValidateKey(key);

            return _contextMetadata.TryGetValue(
                key,
                out value);
        }

        protected static void ValidateKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException(
                    "Context keys cannot be empty.",
                    nameof(key));
        }
    }
}
