using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    public interface IRegistry<T> :
        IBaseObject
        where T : class
    {
        int Count { get; }

        IReadOnlyCollection<T> Items { get; }

        bool Register(T item);

        bool Contains(Guid id);

        T? Get(Guid id);

        bool TryGet(
            Guid id,
            out T? item);

        bool Remove(Guid id);

        void Clear();
    }
}