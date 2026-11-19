using WildsEngine.Discoveries.Abstractions.Surface;
namespace WildsEngine.Discoveries.Abstractions.Truths
{
    public interface IDiscoveryTruthProfile : IWildsDiscoveriesProfile
    {
        ITruthCongealmentPolicy CongealmentPolicy { get; }
    }
}
