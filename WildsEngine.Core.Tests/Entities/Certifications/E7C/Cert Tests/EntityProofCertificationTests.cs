using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Entities.Abstractions;
using WildsEngine.Core.Entities.Evidences;
using WildsEngine.Core.Entities.Proofs;
using WildsEngine.Core.Tests.Entities.Fixtures;
using Xunit;

namespace WildsEngine.Core.Tests.Entities.Certifications
{
    public sealed class EntityProofCertificationTests
    {
        [Theory]
        [InlineData(0.95, true)]
        [InlineData(0.95, false)]
        [InlineData(0.15, true)]
        [InlineData(0.15, false)]
        public void TruthAndBelievabilityRemainIndependent(double believability, bool isTrue)
        {
            var evidence = CreateEvidence();
            var proof = new TestProof(believability, isTrue, "controlled proof");
            var result = proof.Validate(evidence);

            Assert.Same(evidence, result.Evidence);
            Assert.Same(proof, result.Proof);
            Assert.Equal(believability, result.Believability);
            Assert.Equal(isTrue, result.IsTrue);
            Assert.Equal("controlled proof", result.Reason);
        }

        [Fact]
        public void ApplyingExactProofResultValidatesEvidence()
        {
            var evidence = CreateEvidence();
            var proof = new TestProof(0.91, false, "Tom sounded convincing.");
            var result = proof.Validate(evidence);

            Assert.False(evidence.IsValidated);
            Assert.True(evidence.ApplyProof(result));
            Assert.True(evidence.IsValidated);
            Assert.Same(result, evidence.ProofResult);
            Assert.Same(evidence, evidence.ProofResult.Evidence);
        }

        [Fact]
        public void EvidenceRejectsProofResultForDifferentEvidence()
        {
            var target = CreateEvidence("target");
            var foreign = CreateEvidence("foreign");
            var proof = new TestProof(0.80, true);
            var foreignResult = proof.Validate(foreign);

            Assert.Throws<ArgumentException>(() => target.ApplyProof(foreignResult));
            Assert.False(target.IsValidated);
        }

        [Fact]
        public void EvidenceCannotSilentlyReplaceEstablishedProofResult()
        {
            var evidence = CreateEvidence();
            var firstProof = new TestProof(0.90, false, "first");
            var secondProof = new TestProof(0.25, true, "second");
            var first = firstProof.Validate(evidence);
            var second = secondProof.Validate(evidence);

            Assert.True(evidence.ApplyProof(first));
            Assert.False(evidence.ApplyProof(second));
            Assert.Same(first, evidence.ProofResult);
            Assert.Equal(0.90, evidence.ProofResult.Believability);
            Assert.False(evidence.ProofResult.IsTrue);
            Assert.Equal("first", evidence.ProofResult.Reason);
        }

        [Fact]
        public void ProofRejectsNullEvidence()
        {
            var proof = new TestProof(0.5, true);
            Assert.Throws<ArgumentNullException>(() => proof.Validate(null!));
        }

        [Theory]
        [InlineData(-0.01)]
        [InlineData(1.01)]
        public void ProofRejectsOutOfRangeBelievability(double believability)
        {
            var proof = new TestProof(believability, true);
            Assert.Throws<InvalidOperationException>(() => proof.Validate(CreateEvidence()));
        }

        [Fact]
        public void ProofRejectsNaNBelievability()
        {
            var proof = new TestProof(double.NaN, true);
            Assert.Throws<InvalidOperationException>(() => proof.Validate(CreateEvidence()));
        }

        [Fact]
        public void ProofRejectsInfiniteBelievability()
        {
            var positive = new TestProof(double.PositiveInfinity, true);
            var negative = new TestProof(double.NegativeInfinity, false);

            Assert.Throws<InvalidOperationException>(() => positive.Validate(CreateEvidence("positive")));
            Assert.Throws<InvalidOperationException>(() => negative.Validate(CreateEvidence("negative")));
        }

        [Fact]
        public void ProofResultConstructorDefendsItsOwnBelievabilityBoundary()
        {
            var evidence = CreateEvidence();
            var proof = new TestProof(0.5, true);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ProofResult(evidence, proof, -0.01, true));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new ProofResult(evidence, proof, 1.01, false));
        }

        private static DiscoveryEvidence CreateEvidence(string externalId = "tom-report")
        {
            return new DiscoveryEvidence(
                new TestEntity("jerry"),
                new Dictionary<string, object?>
                {
                    { "ClaimedLocation", "North Cove" },
                    { "PriorReport", "North Cove closed for kelp removal" }
                },
                "Tom reports a large bass at North Cove",
                1.0,
                1.0,
                new List<IBaseObject>(),
                null,
                externalId,
                "Tom fishing report");
        }
    }
}
