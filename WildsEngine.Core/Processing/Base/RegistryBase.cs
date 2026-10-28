using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Objects;

namespace WildsEngine.Core.Processing
{
    public abstract class RegistryBase<T> :
        BaseObject,
        IRegistry<T>
        where T : class
    {
        private readonly ConcurrentDictionary<Guid, T> _items =
            new ConcurrentDictionary<Guid, T>();

        private readonly IComponent _component;
        private readonly Func<T, Guid> _identity;

        protected RegistryBase(
            IComponent component,
            string layer,
            Func<T, Guid> identity)
            : base(layer)
        {
            _component = component ??
                throw new ArgumentNullException(
                    nameof(component));

            _identity = identity ??
                throw new ArgumentNullException(
                    nameof(identity));
        }

        public sealed override IComponent Component =>
            _component;

        public int Count =>
            _items.Count;

        public IReadOnlyCollection<T> Items =>
            _items.Values.ToArray();

        public bool Register(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(
                    nameof(item));
            }

            var id =
                _identity(item);

            if (id == Guid.Empty)
                return false;

            return _items.TryAdd(
                id,
                item);
        }

        public bool Contains(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            return _items.ContainsKey(
                id);
        }

        public T? Get(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _items.TryGetValue(
                id,
                out var item)
                    ? item
                    : null;
        }

        public bool TryGet(
            Guid id,
            out T? item)
        {
            if (id == Guid.Empty)
            {
                item = null;

                return false;
            }

            return _items.TryGetValue(
                id,
                out item);
        }

        public bool Remove(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            return _items.TryRemove(
                id,
                out _);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool IsInitialized { get; private set; }

        public bool Initialize()
        {
            if (IsInitialized)
                return false;

            OnInitialize();

            IsInitialized = true;

            return true;
        }

        public bool Shutdown()
        {
            if (!IsInitialized)
                return false;

            OnShutdown();

            IsInitialized = false;

            return true;
        }

        protected abstract void OnInitialize();

        protected abstract void OnShutdown();
    }
}