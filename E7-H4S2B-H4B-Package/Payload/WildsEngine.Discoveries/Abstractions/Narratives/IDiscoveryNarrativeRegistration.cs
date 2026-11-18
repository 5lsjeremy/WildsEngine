using System;
using System.Collections.Generic;
using WildsEngine.Discoveries.Abstractions.Surface;
namespace WildsEngine.Discoveries.Abstractions.Narratives
{
 public interface IDiscoveryNarrativeRegistration : IWildsDiscoveriesRegistration
 { Guid CompositionPolicyId { get; } Guid MetricId { get; } IReadOnlyList<Guid> RelationshipIds { get; } }
}
