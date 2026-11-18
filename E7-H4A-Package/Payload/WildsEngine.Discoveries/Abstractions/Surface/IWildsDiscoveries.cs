using WildsEngine.Discoveries.Abstractions.Services;

namespace WildsEngine.Discoveries.Abstractions.Surface
{
    public interface IWildsDiscoveries
    {
        IDiscoveryGeometrySurface Geometry { get; }
        IDiscoveryNarrativeSurface Narratives { get; }
        IWildsServiceExecutionJournal Executions { get; }
    }
}
