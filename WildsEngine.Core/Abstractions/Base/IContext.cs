using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    public interface IContext : IBaseObject
    {
        IReadOnlyDictionary<string, object?> Variables { get; }

        IReadOnlyDictionary<string, object?> Metadata { get; }

        void SetVariable(string key, object? value);

        void SetMetadata(string key, object? value);
    }
}
