using System;using WildsEngine.Discoveries.Abstractions.Truths;
namespace WildsEngine.Discoveries.Abstractions.Consequences
{
    public interface IDiscoveryConsequenceSurface
    {
        IDiscoveryConsequenceRegistration Register(IDiscoveryConsequenceProfile profile);
        IDiscoveryConsequenceProjectionResult Project(IDiscoveryTruthCongealmentResult truthResult,Guid registrationId);
    }
}
