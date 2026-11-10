[CmdletBinding()]
param(
    [string]$Root = (Get-Location).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root = [System.IO.Path]::GetFullPath($Root)
if (-not (Test-Path -LiteralPath $Root)) {
    throw "WildsEngine root does not exist: $Root"
}

$files = @{
    "WildsEngine.Core\Abstractions\Emissions\IEmissionStateInstruction.cs" = @'
namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Represents a Wilds state instruction resolved from one
    /// exact eligible propagated-emission decision.
    ///
    /// The source decision remains authoritative provenance.
    /// Resolving the instruction does not process it or mutate
    /// receiver state.
    /// </summary>
    public interface IEmissionStateInstruction :
        IStateInstruction
    {
        IEmissionEligibilityResult Source { get; }
    }
}
'@

    "WildsEngine.Core\Abstractions\Emissions\IEmissionInstructionResolver.cs" = @'
namespace WildsEngine.Core.Abstractions
{
    /// <summary>
    /// Maps one eligible propagated-emission decision into a
    /// consumer-defined Wilds state instruction.
    ///
    /// The resolver owns interpretation. Wilds owns validation
    /// of eligibility and exact source-provenance continuity.
    /// </summary>
    public interface IEmissionInstructionResolver
    {
        IEmissionStateInstruction Resolve(
            IEmissionEligibilityResult eligibility);
    }
}
'@

    "WildsEngine.Core\Emission\EmissionInstructionResolution.cs" = @'
using System;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Resolves an eligible propagated-emission fact into the
    /// existing Wilds state-instruction model.
    ///
    /// This boundary does not register, process, or execute the
    /// resulting instruction.
    /// </summary>
    public sealed class EmissionInstructionResolution
    {
        private readonly IEmissionInstructionResolver _resolver;

        public EmissionInstructionResolution(
            IEmissionInstructionResolver resolver)
        {
            _resolver =
                resolver ??
                throw new ArgumentNullException(
                    nameof(resolver));
        }

        public IEmissionStateInstruction Resolve(
            IEmissionEligibilityResult eligibility)
        {
            if (eligibility == null)
            {
                throw new ArgumentNullException(
                    nameof(eligibility));
            }

            if (!eligibility.IsEligible)
            {
                throw new ArgumentException(
                    "State-instruction resolution requires an " +
                    "eligible propagated-emission decision.",
                    nameof(eligibility));
            }

            var instruction =
                _resolver.Resolve(
                    eligibility);

            if (instruction == null)
            {
                throw new InvalidOperationException(
                    "The emission instruction resolver returned null.");
            }

            if (!ReferenceEquals(
                    eligibility,
                    instruction.Source))
            {
                throw new InvalidOperationException(
                    "The resolved instruction must preserve the " +
                    "exact eligibility-result reference.");
            }

            return instruction;
        }
    }
}
'@

    "WildsEngine.Core.Tests\Emission\EmitterGate7InstructionResolutionTests.cs" = @'
using System;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Emission;
using WildsEngine.Core.Processing;
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

            Assert.IsAssignableFrom<ISetContextVariableInstruction>(
                instruction);

            Assert.Same(
                eligibility,
                instruction.Source);

            Assert.Equal(
                "consumer-state.nocturnal-abyssal-response",
                instruction.Key);

            Assert.Equal(
                eligibility.EvaluatedValue,
                Convert.ToDouble(
                    instruction.Value),
                10);

            Assert.Equal(
                TestEmissionInstruction.IntentName,
                instruction.Intent);

            Assert.Equal(
                instruction.Value,
                instruction.Payload);

            Assert.Equal(
                instruction.Key,
                instruction.Arguments["key"]);

            Assert.Equal(
                instruction.Value,
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
                        null!));
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
'@
}

foreach ($relativePath in $files.Keys) {
    $path = Join-Path $Root $relativePath
    $directory = Split-Path -Parent $path

    if (-not (Test-Path -LiteralPath $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    [System.IO.File]::WriteAllText(
            $path,
            $files[$relativePath],
            [System.Text.UTF8Encoding]::new($false))

    Write-Host "[WROTE] $relativePath" -ForegroundColor Green
}

Write-Host ""
Write-Host "Gate 7 slice one installed." -ForegroundColor Cyan
Write-Host "Run:" -ForegroundColor Cyan
Write-Host 'dotnet test .\WildsEngine.Core.Tests\WildsEngine.Core.Tests.csproj -c Debug --filter "FullyQualifiedName~EmitterGate7InstructionResolutionTests" --logger "console;verbosity=detailed"'
