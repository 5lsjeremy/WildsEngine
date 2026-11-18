using WildsEngine.Discoveries.Abstractions.Services;

namespace WildsEngine.Discoveries.Abstractions.Surface
{
    public interface IWildsDiscoveries
    {
        IDiscoveryGeometrySurface Geometry { get; }
        IWildsServiceExecutionJournal Executions { get; }
    }
}
