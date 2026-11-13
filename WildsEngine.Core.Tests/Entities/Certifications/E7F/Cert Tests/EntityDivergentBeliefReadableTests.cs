using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Discoveries.Abstractions;
using WildsEngine.Discoveries.Evidences;
using WildsEngine.Discoveries.Policies;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityDivergentBeliefReadableTests
    {
        [Fact]
        public void TomAndJerryResolveDifferentLocationsForSameFish()
        {
            TestLog.Section("E7-F - TOM AND JERRY DIVERGE");
            TestLog.Test("Tom and Jerry form resolved Facts about the same monster bass.");

            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomFact = CreateFact(tom, "Miller's Point");
            var jerryFact = CreateFact(jerry, "North Cove");
            var tomEvidence = CreateEvidence(tom, "Tom personally caught the fish", "Miller's Point", 1.0, true, "tom-direct");
            var jerryEvidence = CreateEvidence(jerry, "Tom told Jerry North Cove", "North Cove", 0.92, false, "jerry-tom");

            tomFact.AddEvidenceItem(tomEvidence);
            jerryFact.AddEvidenceItem(jerryEvidence);

            TestLog.State($"Tom Entity = {tom.Slug}");
            TestLog.State($"Tom Fact = {tomFact.Value} | proven={tomFact.IsProven}");
            TestLog.State($"Jerry Entity = {jerry.Slug}");
            TestLog.State($"Jerry Fact = {jerryFact.Value} | proven={jerryFact.IsProven}");
            TestLog.State($"Jerry Evidence = Believability {jerryEvidence.ProofResult.Believability}, IsTrue {jerryEvidence.ProofResult.IsTrue}");

            Assert.True(tomFact.IsProven);
            Assert.True(jerryFact.IsProven);
            Assert.NotEqual(tomFact.Value, jerryFact.Value);
            TestLog.Pass("Two Entities resolved contradictory Facts without sharing or corrupting ownership.");
        }

        [Fact]
        public void JerrysBeliefCanCollapseWithoutChangingTomsKnowledge()
        {
            TestLog.Section("E7-F - ONE PERSPECTIVE CHANGES, THE OTHER DOES NOT");

            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomEvidence = CreateEvidence(tom, "Tom direct catch", "Miller's Point", 1.0, true, "tom");
            var jerryEvidence = CreateEvidence(jerry, "Tom says North Cove", "North Cove", 0.92, false, "jerry");
            var tomFact = CreateFact(tom, "Miller's Point");
            var jerryFact = CreateFact(jerry, "North Cove");

            tomFact.AddEvidenceItem(tomEvidence);
            jerryFact.AddEvidenceItem(jerryEvidence);
            TestLog.State($"Before removal: Tom={tomFact.IsProven}, Jerry={jerryFact.IsProven}");

            jerryFact.RemoveEvidenceItem(jerryEvidence);
            TestLog.State($"After Jerry loses his report: Tom={tomFact.IsProven}, Jerry={jerryFact.IsProven}");
            TestLog.State($"Tom's location remains = {tomFact.Value}");

            Assert.True(tomFact.IsProven);
            Assert.False(jerryFact.IsProven);
            TestLog.Pass("Jerry becoming uncertain did not alter Tom's independent resolved Fact.");
        }

        [Fact]
        public void SharedReportIdentifierDoesNotCreateSharedKnowledge()
        {
            TestLog.Section("E7-F - SHARED LABEL IS NOT SHARED KNOWLEDGE");

            var tom = new TestEntity("tom");
            var jerry = new TestEntity("jerry");
            var tomEvidence = CreateEvidence(tom, "Tom account", "Miller's Point", 1.0, true, "report-001");
            var jerryEvidence = CreateEvidence(jerry, "Jerry account", "North Cove", 0.92, false, "report-001");

            TestLog.State($"ExternalId shared = {tomEvidence.ExternalId}");
            TestLog.State($"Tom Evidence owner = {tomEvidence.Entity.Slug}, outcome = {tomEvidence.Outcome}");
            TestLog.State($"Jerry Evidence owner = {jerryEvidence.Entity.Slug}, outcome = {jerryEvidence.Outcome}");

            Assert.Equal(tomEvidence.ExternalId, jerryEvidence.ExternalId);
            Assert.NotSame(tomEvidence, jerryEvidence);
            Assert.NotSame(tomEvidence.Entity, jerryEvidence.Entity);
            TestLog.Pass("Matching external identifiers did not merge Entity-owned Evidence or perspective.");
        }

        private static TestFact CreateFact(IEntity entity, string location)
        {
            return new TestFact(
                entity,
                "MonsterBassLocation",
                location,
                1,
                new MinimumBelievabilityPolicy(0.80));
        }

        private static DiscoveryEvidence CreateEvidence(
            IEntity entity,
            string narrative,
            string location,
            double believability,
            bool isTrue,
            string externalId)
        {
            var evidence = new DiscoveryEvidence(
                entity,
                new Dictionary<string, object?>
                {
                    { "ClaimedLocation", location },
                    { "Narrative", narrative }
                },
                location,
                1.0,
                1.0,
                new List<IBaseObject>(),
                null,
                externalId,
                narrative);

            var proof = new TestProof(believability, isTrue, narrative);
            Assert.True(evidence.ApplyProof(proof.Validate(evidence)));
            return evidence;
        }
    }
}
