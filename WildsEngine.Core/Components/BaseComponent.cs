using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Components
{
    public abstract class BaseComponent : BaseObject, IComponent
    {
        private readonly Dictionary<string, object?> _metadata =
            new Dictionary<string, object?>();

        protected BaseComponent(string layer)
            : base(layer)
        {
        }

        protected BaseComponent(Guid id, string layer)
            : base(id, layer)
        {
        }

        public sealed override IComponent Component =>
            this;

        public abstract override IState State { get; }

        public abstract override ISignal Signal { get; }

        public sealed override IDictionary<string, object?> Metadata =>
            _metadata;
    }
}
