[CmdletBinding()]
param(
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$Root = [System.IO.Path]::GetFullPath($Root)

$coreProject = Join-Path $Root "WildsEngine.Core\WildsEngine.Core.csproj"
$testProject = Join-Path $Root "WildsEngine.Core.Tests\WildsEngine.Core.Tests.csproj"
if (-not (Test-Path -LiteralPath $coreProject) -or -not (Test-Path -LiteralPath $testProject)) {
    throw "WildsEngine repository root not found at: $Root"
}

$files = @{
    "WildsEngine.Core\Emission\EmissionInstructionResolution.cs" = @'
using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;

namespace WildsEngine.Core.Emission
{
    /// <summary>
    /// Resolves eligible propagated-emission facts into the
    /// existing Wilds state-instruction model.
    ///
    /// Resolution does not register, process, or execute the
    /// resulting instructions.
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

            ValidateResolvedInstruction(
                eligibility,
                instruction);

            return instruction;
        }

        public IReadOnlyList<IEmissionStateInstruction> Resolve(
            IEmissionEligibilityVector eligibility)
        {
            if (eligibility == null)
            {
                throw new ArgumentNullException(
                    nameof(eligibility));
            }

            var resolved =
                new List<IEmissionStateInstruction>(
                    eligibility.EligibleCount);

            for (var index = 0;
                 index < eligibility.Dimension;
                 index++)
            {
                var decision =
                    eligibility.Emissions[index];

                if (!decision.IsEligible)
                {
                    continue;
                }

                resolved.Add(
                    Resolve(
                        decision));
            }

            return resolved.AsReadOnly();
        }

        private static void ValidateResolvedInstruction(
            IEmissionEligibilityResult eligibility,
            IEmissionStateInstruction instruction)
        {
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
        }
    }
}
'@

    "WildsEngine.Core.Tests\Emission\EmitterGate7Slice2Tests.cs" = @'
using System;
using System.Collections.Generic;
using WildsEngine.Core.Abstractions;
using WildsEngine.Core.Emission;
using WildsEngine.Core.Processing;
using Xunit;

namespace WildsEngine.Core.Tests.Emission
{
    /// <summary>
    /// Gate 7 slice-two certification.
    ///
    /// Proves ordered vector-component resolution, filtering at
    /// the Gate 7 boundary, exact eligibility provenance, and
    /// zero implicit IState.Process calls during resolution.
    /// </summary>
    public sealed class EmitterGate7Slice2Tests
    {
        [Fact]
        public void Vector_resolution_returns_only_eligible_components_in_source_order()
        {
            var first = CreateEligibility(0.40, true);
            var second = CreateEligibility(0.20, false);
            var third = CreateEligibility(0.30, true);
            var fourth = CreateEligibility(0.10, false);

            var vector =
                new TestEligibilityVector(
                    first,
                    second,
                    third,
                    fourth);

            var resolver = new RecordingResolver();
            var resolution =
                new EmissionInstructionResolution(
                    resolver);

            var instructions =
                resolution.Resolve(
                    vector);

            Assert.Equal(2, instructions.Count);
            Assert.Equal(2, resolver.Sources.Count);

            Assert.Same(first, resolver.Sources[0]);
            Assert.Same(third, resolver.Sources[1]);

            Assert.Same(first, instructions[0].Source);
            Assert.Same(third, instructions[1].Source);

            Assert.DoesNotContain(second, resolver.Sources);
            Assert.DoesNotContain(fourth, resolver.Sources);
        }

        [Fact]
        public void Vector_with_no_eligible_components_returns_empty_without_invoking_resolver()
        {
            var vector =
                new TestEligibilityVector(
                    CreateEligibility(0.10, false),
                    CreateEligibility(0.20, false));

            var resolver = new RecordingResolver();
            var resolution =
                new EmissionInstructionResolution(
                    resolver);

            var instructions =
                resolution.Resolve(
                    vector);

            Assert.Empty(instructions);
            Assert.Empty(resolver.Sources);
        }

        [Fact]
        public void Vector_resolution_preserves_exact_eligibility_reference_per_instruction()
        {
            var first = CreateEligibility(0.25, true);
            var second = CreateEligibility(0.50, true);

            var vector =
                new TestEligibilityVector(
                    first,
                    second);

            var resolution =
                new EmissionInstructionResolution(
                    new RecordingResolver());

            var instructions =
                resolution.Resolve(
                    vector);

            Assert.Equal(2, instructions.Count);
            Assert.Same(first, instructions[0].Source);
            Assert.Same(second, instructions[1].Source);
        }

        [Fact]
        public void Vector_resolution_rejects_null_input()
        {
            var resolution =
                new EmissionInstructionResolution(
                    new RecordingResolver());

            Assert.Throws<ArgumentNullException>(
                () =>
                    resolution.Resolve(
                        (IEmissionEligibilityVector)null!));
        }

        [Fact]
        public void Resolution_performs_zero_state_process_calls()
        {
            var state = new CountingState();
            var resolver =
                new RecordingResolver();

            var resolution =
                new EmissionInstructionResolution(
                    resolver);

            var eligibility =
                CreateEligibility(
                    0.25,
                    true);

            var instruction =
                resolution.Resolve(
                    eligibility);

            Assert.NotNull(instruction);
            Assert.Equal(1, resolver.Sources.Count);
            Assert.Equal(0, state.ProcessCount);
        }

        private static IEmissionEligibilityResult CreateEligibility(
            double value,
            bool eligible)
        {
            var emission =
                new TestPropagatedEmission(
                    value);

            return new EmissionEligibilityResult(
                emission,
                value,
                eligible);
        }

        private sealed class RecordingResolver :
            IEmissionInstructionResolver
        {
            private readonly List<IEmissionEligibilityResult> _sources =
                new List<IEmissionEligibilityResult>();

            public IReadOnlyList<IEmissionEligibilityResult> Sources =>
                _sources;

            public IEmissionStateInstruction Resolve(
                IEmissionEligibilityResult eligibility)
            {
                _sources.Add(
                    eligibility);

                return new TestEmissionInstruction(
                    eligibility,
                    "consumer-state." + _sources.Count,
                    eligibility.EvaluatedValue);
            }
        }

        private sealed class TestEmissionInstruction :
            ContextVariableInstructionBase,
            IEmissionStateInstruction
        {
            public TestEmissionInstruction(
                IEmissionEligibilityResult source,
                string key,
                object? value)
                : base(
                    "certification.emission.context-variable.set",
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

        private sealed class TestEligibilityVector :
            IEmissionEligibilityVector
        {
            private readonly IReadOnlyList<IEmissionEligibilityResult> _emissions;

            public TestEligibilityVector(
                params IEmissionEligibilityResult[] emissions)
            {
                _emissions =
                    Array.AsReadOnly(
                        emissions ??
                        throw new ArgumentNullException(
                            nameof(emissions)));

                var eligibleCount = 0;
                for (var index = 0;
                     index < _emissions.Count;
                     index++)
                {
                    if (_emissions[index].IsEligible)
                    {
                        eligibleCount++;
                    }
                }

                EligibleCount = eligibleCount;
            }

            public IPropagatedEmissionVector Source => null!;

            public IReadOnlyList<IEmissionEligibilityResult> Emissions =>
                _emissions;

            public int Dimension => _emissions.Count;

            public int EligibleCount { get; }

            public bool AnyEligible => EligibleCount > 0;

            public bool AllEligible => EligibleCount == Dimension;
        }

        private sealed class CountingState :
            IState
        {
            public int ProcessCount { get; private set; }

            public IContext Context => null!;

            public Guid Id => Guid.Empty;

            public string Slug => "gate7.counting-state";

            public INeighborCollection Neighbors => null!;

            public IComponent Component => null!;

            public string AppendSlug(string value) =>
                Slug + "." + value;

            public TResult ApplyNeighborOperator<TResult>(
                INeighborOperator<TResult> neighborOperator)
            {
                throw new NotSupportedException();
            }

            public bool Process(
                IStateInstruction instruction)
            {
                ProcessCount++;
                return true;
            }
        }

        private sealed class TestPropagatedEmission :
            IPropagatedEmission
        {
            public TestPropagatedEmission(double value)
            {
                Value = value;
            }

            public IResolvedEmission Emission => null!;

            public INeighborRelationship Relationship => null!;

            public double PropagationDistance => 0.0;

            public double ResolvedValue => Value;

            public double? NormalizedValue => null;

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
Write-Host "Gate 7 Slice 2 installed at: $Root" -ForegroundColor Cyan
Write-Host 'Run: dotnet test .\WildsEngine.Core.Tests\WildsEngine.Core.Tests.csproj -c Debug --filter "FullyQualifiedName~EmitterGate7" --logger "console;verbosity=detailed"'
