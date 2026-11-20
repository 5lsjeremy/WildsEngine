using Xunit;

namespace WildsEngine.Core.Tests
{
    /// <summary>
    /// Serializes Wilds/QSL integration certifications.
    ///
    /// Individual tests may still create and exercise multiple
    /// independent runtimes concurrently or sequentially.
    ///
    /// This collection prevents unrelated integration test methods
    /// from overlapping in the same test process while they exercise
    /// QSL/Wilds runtime infrastructure.
    /// </summary>
    [CollectionDefinition(
        Name,
        DisableParallelization = true)]
    
    public sealed class WildsQslIntegrationTestCollection
    {
        public const string Name =
            "Wilds / QSL Integration Certification";
    }
}