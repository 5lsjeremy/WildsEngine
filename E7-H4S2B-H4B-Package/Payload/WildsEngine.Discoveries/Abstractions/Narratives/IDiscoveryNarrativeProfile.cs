using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Surface;
namespace WildsEngine.Discoveries.Abstractions.Narratives
{
 public interface IDiscoveryNarrativeProfile : IWildsDiscoveriesProfile
 { IDiscoveryNarrativeCompositionPolicy CompositionPolicy { get; } IDiscoveryNarrativeMetric Metric { get; } IReadOnlyList<IDiscoveryNarrativeRelationshipDefinition> Relationships { get; } }
}
