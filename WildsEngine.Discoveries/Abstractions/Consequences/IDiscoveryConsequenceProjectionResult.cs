using System;using System.Collections.Generic;using WildsEngine.Discoveries.Abstractions.Truths;
namespace WildsEngine.Discoveries.Abstractions.Consequences
{
    public interface IDiscoveryConsequenceProjectionResult
    {
        Guid Id { get; }
        IDiscoveryTruthCongealmentResult TruthResult { get; }
        IDiscoveryConsequenceRegistration Registration { get; }
        IReadOnlyList<IConsequence> Consequences { get; }
        DateTimeOffset ProjectedAt { get; }
    }
}
