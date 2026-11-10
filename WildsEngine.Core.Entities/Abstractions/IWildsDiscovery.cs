using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Entities.Abstractions
{
    /// <summary>
    /// Represents the Wilds-compatible projection of one
    /// Entity-owned Discovery.
    ///
    /// The original Discovery remains available as authoritative
    /// provenance.
    /// </summary>
    public interface IWildsDiscovery :
        IBaseObject
    {
        IDiscovery Discovery { get; }
    }
}