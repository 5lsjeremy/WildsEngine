using System;
using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    public interface IStateInstruction
    {
        Guid Id { get; }

        string Intent { get; }

        object? Payload { get; }

        IReadOnlyDictionary<string, object?> Arguments { get; }

        IReadOnlyDictionary<string, object?> Metadata { get; }
    }
}
