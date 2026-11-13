using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Discoveries.Base
{
    public abstract class WildsProjectionBase<TSource> :
        BaseObject,
        IWildsProjection<TSource>
        where TSource : class
    {
        private readonly IComponent _component;

        protected WildsProjectionBase(
            TSource source,
            IComponent component,
            Guid id,
            string layer)
            : base(
                id,
                layer)
        {
            Source =
                source ??
                throw new ArgumentNullException(
                    nameof(source));

            _component =
                component ??
                throw new ArgumentNullException(
                    nameof(component));
        }

        public TSource Source { get; }

        public sealed override IComponent Component =>
            _component;
    }
}
