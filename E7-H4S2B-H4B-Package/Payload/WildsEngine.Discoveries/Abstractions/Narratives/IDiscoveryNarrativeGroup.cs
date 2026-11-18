using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Clusters;
namespace WildsEngine.Discoveries.Abstractions.Narratives
{
 public interface IDiscoveryNarrativeGroup
 { IReadOnlyList<IDiscoveryClusterRecognitionResult> Members { get; } }
}
