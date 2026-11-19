using System;using WildsEngine.Discoveries.Abstractions.Surface;
namespace WildsEngine.Discoveries.Abstractions.Consequences
{
    public interface IDiscoveryConsequenceRegistration : IWildsDiscoveriesRegistration
    { Guid ProjectionPolicyId { get; } }
}
