using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    public interface IBaseObject
    {
        Guid Id { get; }

        string Slug { get; }

        IReadOnlyList<IBaseObject> Neighbors { get; }

        IComponent Component { get; }

        string AppendSlug(string value);

        bool AddNeighbor(IBaseObject neighbor);

        bool RemoveNeighbor(Guid id);
    }
}
