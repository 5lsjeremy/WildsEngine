$ErrorActionPreference = "Stop"

$Root = (Get-Location).Path

$ProjectFile = Join-Path $Root "WildsEngine.Core.Tests.csproj"

if (-not (Test-Path $ProjectFile)) {
    throw "Run this script from the WildsEngine.Core.Tests directory."
}

Write-Host ""
Write-Host "============================================"
Write-Host " WildsEngine Test Project Organizer"
Write-Host "============================================"
Write-Host ""
Write-Host "Root: $Root"
Write-Host ""

# ------------------------------------------------------------
# Target folders
# ------------------------------------------------------------

$Folders = @(
    "Certification",
    "Emission",
    "EndToEnd",
    "Fixtures",
    "Integration",
    "Neighbors",
    "Pooling",
    "Processing",
    "Propagation",
    "Qsl",
    "Registry",
    "Temporal",
    "Units"
)

foreach ($Folder in $Folders) {
    $Path = Join-Path $Root $Folder

    if (-not (Test-Path $Path)) {
        New-Item `
            -ItemType Directory `
            -Path $Path | Out-Null

        Write-Host "[CREATE] $Folder"
    }
    else {
        Write-Host "[EXISTS] $Folder"
    }
}

Write-Host ""

# ------------------------------------------------------------
# Move helper
# ------------------------------------------------------------

function Move-TestFile {
    param(
        [Parameter(Mandatory = $true)]
        [string] $FileName,

        [Parameter(Mandatory = $true)]
        [string] $DestinationFolder
    )

    $Source = Join-Path $Root $FileName
    $DestinationDirectory =
        Join-Path $Root $DestinationFolder

    $Destination =
        Join-Path $DestinationDirectory $FileName

    if (-not (Test-Path $Source)) {
        Write-Host "[SKIP]   $FileName"
        return
    }

    if (Test-Path $Destination) {
        Write-Host "[KEEP]   $FileName -> $DestinationFolder"
        return
    }

    Move-Item `
        -Path $Source `
        -Destination $DestinationDirectory

    Write-Host "[MOVE]   $FileName -> $DestinationFolder"
}

# ------------------------------------------------------------
# Shared fixtures
# ------------------------------------------------------------

Move-TestFile "TestComponent.cs" "Fixtures"
Move-TestFile "TestContext.cs" "Fixtures"
Move-TestFile "TestSignal.cs" "Fixtures"
Move-TestFile "TestState.cs" "Fixtures"

# Keep test logging together with shared test infrastructure.
Move-TestFile "TestLog.cs" "Fixtures"

# Registry fixture is currently registry-specific.
Move-TestFile "RegistryTestFixture.cs" "Registry"

# ------------------------------------------------------------
# Certification
# ------------------------------------------------------------

Move-TestFile "CertificationLogger.cs" "Certification"
Move-TestFile "CertificationTests.cs" "Certification"
Move-TestFile "ExtremeInputSafetyCertificationTests.cs" "Certification"
Move-TestFile "WildsEngineCertification.cs" "Certification"

# ------------------------------------------------------------
# End-to-end
# ------------------------------------------------------------

Move-TestFile "EndToEndProcessingTests.cs" "EndToEnd"

# ------------------------------------------------------------
# Neighbors / topology
# ------------------------------------------------------------

Move-TestFile "NeighborTopologyTests.cs" "Neighbors"

# ------------------------------------------------------------
# Pooling
# ------------------------------------------------------------

Move-TestFile "ObjectPoolTests.cs" "Pooling"

# ------------------------------------------------------------
# Processing / integration
# ------------------------------------------------------------

Move-TestFile "WildsEngineExecuteIntegrationTests.cs" "Integration"
Move-TestFile "WildsEngineDecompositionIntegrationTests.cs" "Integration"
Move-TestFile "WIldsEngineActiveCancellationTests.cs" "Integration"

# ------------------------------------------------------------
# Registry
# ------------------------------------------------------------

Move-TestFile "RegistryTests.cs" "Registry"
Move-TestFile "TaskRegistryTests.cs" "Registry"

# ------------------------------------------------------------
# Temporal / Pulse / WorkUnit
# ------------------------------------------------------------

Move-TestFile "PulseCoordinatorCertificationTests.cs" "Temporal"
Move-TestFile "PulseDecompositionCertificationTests.cs" "Temporal"
Move-TestFile "PulseOutcomeCertificationTests.cs" "Temporal"
Move-TestFile "TemporalConcurrencyOrderingCertificationTests.cs" "Temporal"
Move-TestFile "WorkUnitRaceCertificationTests.cs" "Temporal"

# ------------------------------------------------------------
# Unit model
# ------------------------------------------------------------

Move-TestFile "UnitCertificationTests.cs" "Units"

# ------------------------------------------------------------
# Wilds / QSL integration
# ------------------------------------------------------------

Move-TestFile "QslPackageLifecycleCertificationTests.cs" "Qsl"
Move-TestFile "WildsQslEndToEndTests.cs" "Qsl"
Move-TestFile "WildsQslProvenanceIntegrationTests.cs" "Qsl"
Move-TestFile "WildsQslRequestCertification.cs" "Qsl"
Move-TestFile "WildsQslRuntimeIsolationTests.cs" "Qsl"

# ------------------------------------------------------------
# Existing specialized folders are intentionally preserved:
#
#   Certification\Fixtures
#   Certification\Consumer
#   Emission
#   Propagation
#   Release
#
# bin / obj are intentionally ignored.
# ------------------------------------------------------------

Write-Host ""
Write-Host "============================================"
Write-Host " Organization complete"
Write-Host "============================================"
Write-Host ""

Write-Host "Root files remaining:"
Write-Host ""

Get-ChildItem `
    -Path $Root `
    -File |
    Sort-Object Name |
    ForEach-Object {
        Write-Host "  $($_.Name)"
    }

Write-Host ""
Write-Host "Run certification with:"
Write-Host ""
Write-Host "  dotnet test .\WildsEngine.Core.Tests.csproj -c Debug"
Write-Host ""
Write-Host "Cheese status: structurally accommodated."
Write-Host ""