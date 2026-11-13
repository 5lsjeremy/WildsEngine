namespace WildsEngine.Discoveries.Abstractions
{
    /// <summary>
    /// Determines whether one validated Evidence item contributes
    /// toward proving a particular Fact.
    ///
    /// The policy may consider truth, Believability, Evidence
    /// strength, conditions, outcome, source, or any other
    /// consumer-facing information.
    ///
    /// Wilds imposes no universal relationship between
    /// Believability and truth.
    /// </summary>
    public interface IEvidenceContributionPolicy
    {
        bool Contributes(
            IDiscoveryEvidence evidence);
    }
}