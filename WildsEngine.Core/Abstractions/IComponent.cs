using System.Collections.Generic;

namespace WildsEngine.Core.Abstractions
{
    public interface IComponent : IBaseObject
    {
        IState State { get; }

        ISignal Signal { get; }

        IDictionary<string, object?> Metadata { get; }
    }
}
