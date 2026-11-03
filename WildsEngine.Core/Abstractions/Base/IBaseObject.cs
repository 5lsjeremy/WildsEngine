using System;

namespace WildsEngine.Core.Abstractions
{
    public interface IBaseObject
    {
        Guid Id { get; }

        string Slug { get; }

        INeighborCollection Neighbors { get; }

        IComponent Component { get; }

        string AppendSlug(
            string value);

        TResult ApplyNeighborOperator<TResult>(
            INeighborOperator<TResult> neighborOperator);
    }
}