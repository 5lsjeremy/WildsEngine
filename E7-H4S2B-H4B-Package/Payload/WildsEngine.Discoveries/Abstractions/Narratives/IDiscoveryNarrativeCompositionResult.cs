using System;
using System.Collections.Generic;
namespace WildsEngine.Discoveries.Abstractions.Narratives
{
 public interface IDiscoveryNarrativeCompositionResult
 { Guid Id { get; } IDiscoveryRecognizedGeometryPopulation Population { get; } IDiscoveryNarrativeRegistration Registration { get; } IReadOnlyList<IDiscoveryNarrativeGroup> Groups { get; } DateTimeOffset ComposedAt { get; } }
}
