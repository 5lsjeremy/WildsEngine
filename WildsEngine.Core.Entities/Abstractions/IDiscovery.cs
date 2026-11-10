namespace WildsEngine.Core.Entities.Abstractions
{
    /// <summary>
    /// Represents one interpretation derived from an exact Fact.
    ///
    /// A Discovery remains Entity-owned knowledge until Surface
    /// deliberately promotes it into a Wilds-compatible form.
    /// </summary>
    public interface IDiscovery :
        IIdentifier
    {
        IFact Fact { get; }
    }
}