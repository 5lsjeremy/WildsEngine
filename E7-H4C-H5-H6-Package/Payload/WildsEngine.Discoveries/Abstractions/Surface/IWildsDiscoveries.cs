using WildsEngine.Discoveries.Abstractions.Outcomes;using WildsEngine.Discoveries.Abstractions.Services;
namespace WildsEngine.Discoveries.Abstractions.Surface { public interface IWildsDiscoveries { IDiscoveryGeometrySurface Geometry{get;}IDiscoveryNarrativeSurface Narratives{get;}IDiscoveryOutcomeSurface Outcomes{get;}IWildsServiceExecutionJournal Executions{get;} } }
