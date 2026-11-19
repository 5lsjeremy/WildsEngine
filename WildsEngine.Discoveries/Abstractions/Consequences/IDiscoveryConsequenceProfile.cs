using WildsEngine.Discoveries.Abstractions.Surface;
namespace WildsEngine.Discoveries.Abstractions.Consequences
{
    public interface IDiscoveryConsequenceProfile : IWildsDiscoveriesProfile
    { IConsequenceProjectionPolicy ProjectionPolicy { get; } }
}
