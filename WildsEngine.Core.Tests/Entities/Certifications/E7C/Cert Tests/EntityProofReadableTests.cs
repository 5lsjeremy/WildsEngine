using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Evidences;
using WildsEngine.Core.Tests.Entities.Fixtures;
using WildsEngine.Tests.Logging;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityProofReadableTests
    {
        [Fact]
        public void TomCanTellJerrySomethingFalseYetBelievable()
        {
            TestLog.Section("E7-C - TOM'S BELIEVABLE LIE");
            TestLog.Test("Tom tells Jerry that the monster bass came from North Cove.");

            var evidence = CreateEvidence("Tom says: North Cove", "Jerry heard North Cove was closed for kelp removal");
            var proof = new TestProof(0.92, false, "Tom is convincing, but the claim is false.");
            var result = proof.Validate(evidence);

            TestLog.State($"Claim = {evidence.Outcome}");
            TestLog.State($"Believability = {result.Believability}");
            TestLog.State($"IsTrue = {result.IsTrue}");
            TestLog.State($"Reason = {result.Reason}");

            Assert.Equal(0.92, result.Believability);
            Assert.False(result.IsTrue);
            TestLog.Pass("A false statement can independently resolve as highly believable.");
        }

        [Fact]
        public void TomCanTellJerryTheTruthAndStillSoundUnbelievable()
        {
            TestLog.Section("E7-C - TOM'S UNBELIEVABLE TRUTH");
            TestLog.Test("Tom truthfully reports North Cove while Jerry has conflicting prior information.");

            var evidence = CreateEvidence("Tom says: North Cove", "Jerry heard North Cove was closed for kelp removal");
            var proof = new TestProof(0.15, true, "The claim is true, but Jerry has strong reason to doubt it.");
            var result = proof.Validate(evidence);

            TestLog.State($"Prior information = {evidence.Conditions["PriorReport"]}");
            TestLog.State($"Believability = {result.Believability}");
            TestLog.State($"IsTrue = {result.IsTrue}");
            TestLog.State($"Reason = {result.Reason}");

            Assert.Equal(0.15, result.Believability);
            Assert.True(result.IsTrue);
            TestLog.Pass("A true statement can independently resolve as poorly believable.");
        }

        [Fact]
        public void FourProofQuadrantsRemainLegitimate()
        {
            TestLog.Section("E7-C - TRUTH AND BELIEVABILITY QUADRANTS");

            var cases = new[]
            {
                new { Label = "believable truth", Believability = 0.95, IsTrue = true },
                new { Label = "believable falsehood", Believability = 0.95, IsTrue = false },
                new { Label = "unbelievable truth", Believability = 0.15, IsTrue = true },
                new { Label = "unbelievable falsehood", Believability = 0.15, IsTrue = false }
            };

            foreach (var item in cases)
            {
                var evidence = CreateEvidence(item.Label, "Jerry's prior information remains separate");
                var proof = new TestProof(item.Believability, item.IsTrue, item.Label);
                var result = proof.Validate(evidence);

                TestLog.State($"{item.Label} | Believability={result.Believability} | IsTrue={result.IsTrue}");
                Assert.Equal(item.Believability, result.Believability);
                Assert.Equal(item.IsTrue, result.IsTrue);
            }

            TestLog.Pass("Truth and believability occupy independent dimensions.");
        }

        [Fact]
        public void ValidationAttachesOneExactHistoricalProofResult()
        {
            TestLog.Section("E7-C - EXACT PROOF RESULT ATTACHMENT");

            var evidence = CreateEvidence("Tom says: North Cove", "Jerry heard North Cove was closed");
            var proof = new TestProof(0.92, false, "First evaluation");
            var replacementProof = new TestProof(0.10, true, "Attempted replacement");
            var first = proof.Validate(evidence);
            var replacement = replacementProof.Validate(evidence);

            TestLog.State($"Before ApplyProof: IsValidated={evidence.IsValidated}");
            var accepted = evidence.ApplyProof(first);
            var replaced = evidence.ApplyProof(replacement);
            TestLog.State($"First result accepted = {accepted}");
            TestLog.State($"Replacement accepted = {replaced}");
            TestLog.State($"Stored Believability = {evidence.ProofResult.Believability}");
            TestLog.State($"Stored IsTrue = {evidence.ProofResult.IsTrue}");

            Assert.True(accepted);
            Assert.False(replaced);
            Assert.Same(first, evidence.ProofResult);
            Assert.Same(evidence, evidence.ProofResult.Evidence);
            TestLog.Pass("Evidence retained its first exact ProofResult and refused silent replacement.");
        }

        private static DiscoveryEvidence CreateEvidence(string outcome, string priorReport)
        {
            return new DiscoveryEvidence(
                new TestEntity("jerry"),
                new Dictionary<string, object?>
                {
                    { "PriorReport", priorReport },
                    { "ClaimedLocation", "North Cove" }
                },
                outcome,
                1.0,
                1.0,
                new List<IBaseObject>(),
                null,
                "tom-report",
                "Tom fishing report");
        }
    }
}
