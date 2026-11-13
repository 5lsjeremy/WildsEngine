using System.Collections.Generic;

namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Exposes what an Entity saw on the exact consumer-owned target at surface
    /// creation without becoming an alternative authority for live state.
    /// </summary>
    public interface IEntityNativeSurface
    {
        IEntity Entity { get; }

        object Target { get; }

        IReadOnlyList<INativeAttribute> NativeAttributes { get; }
    }
}
