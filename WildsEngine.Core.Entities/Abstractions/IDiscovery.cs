using WildsEngine.Core.Entities.Abstractions;

public interface IDiscovery :
    IIdentifier
{
    IFact Fact { get; }

    bool IsAvailable { get; }
}