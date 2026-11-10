using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Emission;
using WildsEngine.Core.Processing;
using WildsEngine.Tests.Registry;
using Xunit;

namespace WildsEngine.Core.Tests.Emission
{
    /// <summary>
    /// Gate 7 slice-one certification.
    ///
    /// eligible propagated fact
    ///     -> consumer-supplied resolver
    ///     -> existing IStateInstruction model
    ///
    /// Registration, processing, mutation, and QSL remain
    /// outside this focused resolution boundary.
    /// </summary>
    public sealed class EmitterGate7InstructionResolutionTests
    {
        [Fact]
        public void Eligible_fact_resolves_to_existing_instruction_model()
        {
            var eligibility =
                CreateEligibility(
                    0.25,
                    true);

            var resolver =
                new TestResolver(
                    "consumer-state.nocturnal-abyssal-response");

            var resolution =
                new EmissionInstructionResolution(
                    resolver);

            var instruction =
                resolution.Resolve(
                    eligibility);

            Assert.IsAssignableFrom<IStateInstruction>(
                instruction);

            var contextInstruction =
                Assert.IsAssignableFrom<ISetContextVariableInstruction>(
                    instruction);

            Assert.Same(
                eligibility,
                instruction.Source);

            Assert.Equal(
                "consumer-state.nocturnal-abyssal-response",
                contextInstruction.Key);

            Assert.Equal(
                eligibility.EvaluatedValue,
                Convert.ToDouble(
                    contextInstruction.Value),
                10);

            Assert.Equal(
                TestEmissionInstruction.IntentName,
                instruction.Intent);

            Assert.Equal(
                contextInstruction.Value,
                instruction.Payload);

            Assert.Equal(
                contextInstruction.Key,
                instruction.Arguments["key"]);

            Assert.Equal(
                contextInstruction.Value,
                instruction.Arguments["value"]);

            Assert.Equal(
                1,
                resolver.ResolveCount);
        }

        [Fact]
        public void Ineligible_fact_cannot_resolve_an_instruction()
        {
            var eligibility =
                CreateEligibility(
                    0.05,
                    false);

            var resolver =
                new TestResolver(
                    "consumer-state.blocked");

            var resolution =
                new EmissionInstructionResolution(
                    resolver);

            Assert.Throws<ArgumentException>(
                () =>
                    resolution.Resolve(
                        eligibility));

            Assert.Equal(
                0,
                resolver.ResolveCount);
        }

        [Fact]
        public void Resolver_must_return_an_instruction()
        {
            var eligibility =
                CreateEligibility(
                    0.25,
                    true);

            var resolution =
                new EmissionInstructionResolution(
                    new NullResolver());

            Assert.Throws<InvalidOperationException>(
                () =>
                    resolution.Resolve(
                        eligibility));
        }

        [Fact]
        public void Resolver_cannot_replace_eligibility_provenance()
        {
            var authoritative =
                CreateEligibility(
                    0.25,
                    true);

            var substituted =
                CreateEligibility(
                    0.50,
                    true);

            var resolution =
                new EmissionInstructionResolution(
                    new SubstitutingResolver(
                        substituted));

            Assert.Throws<InvalidOperationException>(
                () =>
                    resolution.Resolve(
                        authoritative));
        }

        [Fact]
        public void Resolution_does_not_process_or_mutate_state()
        {
            var eligibility =
                CreateEligibility(
                    0.25,
                    true);

            var resolver =
                new TestResolver(
                    "consumer-state.unprocessed");

            var resolution =
                new EmissionInstructionResolution(
                    resolver);

            var instruction =
                resolution.Resolve(
                    eligibility);

            Assert.NotNull(
                instruction);

            Assert.Equal(
                1,
                resolver.ResolveCount);

            Assert.Equal(
                0,
                resolver.ProcessCount);
        }

        [Fact]
        public void Resolution_rejects_null_dependencies_and_input()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                    new EmissionInstructionResolution(
                        null!));

            var resolution =
                new EmissionInstructionResolution(
                    new TestResolver(
                        "consumer-state.valid"));

            Assert.Throws<ArgumentNullException>(
                () =>
                    resolution.Resolve(
                        (IEmissionEligibilityResult)null!));
        }
        
        [Fact]
        public void Resolved_instruction_is_compatible_with_existing_instruction_registry()
        {
            var eligibility =
                CreateEligibility(
                    0.25,
                    true);

            var resolver =
                new TestResolver(
                    "consumer-state.registry-compatible");

            var resolution =
                new EmissionInstructionResolution(
                    resolver);

            var instruction =
                resolution.Resolve(
                    eligibility);

            var component =
                new TestComponent(
                    "gate7.registry.component");

            var registry =
                new TestInstructionsRegistry(
                    component,
                    "gate7.registry");

            registry.Initialize();

            try
            {
                Assert.True(
                    registry.Register(
                        instruction));

                Assert.Single(
                    registry.Items);

                Assert.True(
                    registry.TryGet(
                        instruction.Id,
                        out var retrieved));

                Assert.NotNull(
                    retrieved);

                Assert.Same(
                    instruction,
                    retrieved);

                Assert.Equal(
                    instruction.Id,
                    retrieved!.Id);

                Assert.Same(
                    eligibility,
                    instruction.Source);
            }
            finally
            {
                registry.Shutdown();
            }
        }
        

        private static IEmissionEligibilityResult CreateEligibility(
            double value,
            bool isEligible)
        {
            var emission =
                new TestPropagatedEmission(
                    value);

            return new EmissionEligibilityResult(
                emission,
                value,
                isEligible);
        }
        
        private sealed class TestInstructionsRegistry :
            InstructionsRegistryBase
        {
            public TestInstructionsRegistry(
                IComponent component,
                string layer)
                : base(
                    component,
                    layer)
            {
            }

            protected override void OnInitialize()
            {
            }

            protected override void OnShutdown()
            {
                Clear();
            }
        }

        private sealed class TestResolver :
            IEmissionInstructionResolver
        {
            private readonly string _key;

            public TestResolver(string key)
            {
                _key = key;
            }

            public int ResolveCount { get; private set; }

            public int ProcessCount { get; private set; }

            public IEmissionStateInstruction Resolve(
                IEmissionEligibilityResult eligibility)
            {
                ResolveCount++;

                return new TestEmissionInstruction(
                    eligibility,
                    _key,
                    eligibility.EvaluatedValue);
            }
        }

        private sealed class NullResolver :
            IEmissionInstructionResolver
        {
            public IEmissionStateInstruction Resolve(
                IEmissionEligibilityResult eligibility)
            {
                return null!;
            }
        }

        private sealed class SubstitutingResolver :
            IEmissionInstructionResolver
        {
            private readonly IEmissionEligibilityResult _substitute;

            public SubstitutingResolver(
                IEmissionEligibilityResult substitute)
            {
                _substitute = substitute;
            }

            public IEmissionStateInstruction Resolve(
                IEmissionEligibilityResult eligibility)
            {
                return new TestEmissionInstruction(
                    _substitute,
                    "consumer-state.substituted",
                    _substitute.EvaluatedValue);
            }
        }

        private sealed class TestEmissionInstruction :
            ContextVariableInstructionBase,
            IEmissionStateInstruction
        {
            public const string IntentName =
                "certification.emission.context-variable.set";

            public TestEmissionInstruction(
                IEmissionEligibilityResult source,
                string key,
                object? value)
                : base(
                    IntentName,
                    key,
                    value)
            {
                Source =
                    source ??
                    throw new ArgumentNullException(
                        nameof(source));
            }

            public IEmissionEligibilityResult Source { get; }
        }

        private sealed class TestPropagatedEmission :
            IPropagatedEmission
        {
            public TestPropagatedEmission(double value)
            {
                Value = value;
            }

            public IResolvedEmission Emission =>
                null!;

            public INeighborRelationship Relationship =>
                null!;

            public double PropagationDistance =>
                0.0;

            public double ResolvedValue =>
                Value;

            public double? NormalizedValue =>
                null;

            public double Value { get; }
        }
    }
}