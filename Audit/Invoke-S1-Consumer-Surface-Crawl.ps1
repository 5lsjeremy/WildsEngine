param(
    [string]$RepoRoot = (Get-Location).Path
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$RepoRoot = (Resolve-Path $RepoRoot).Path
$DiscoveriesRoot = Join-Path $RepoRoot "WildsEngine.Discoveries"
$TestsRoot = Join-Path $RepoRoot "WildsEngine.Core.Tests"
$ReportRoot = Join-Path $RepoRoot "Audit\Reports\S1"
$Stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$Report = Join-Path $ReportRoot "Wilds-S1-Consumer-Surface-Crawl-$Stamp.txt"
$Latest = Join-Path $ReportRoot "Wilds-S1-Consumer-Surface-Crawl-Latest.txt"

if (-not (Test-Path (Join-Path $RepoRoot "WildsEngine.sln"))) {
    throw "WildsEngine.sln was not found under RepoRoot: $RepoRoot"
}

if (-not (Test-Path $DiscoveriesRoot)) {
    throw "WildsEngine.Discoveries was not found: $DiscoveriesRoot"
}

New-Item -ItemType Directory -Path $ReportRoot -Force | Out-Null

$includeDirectories = @(
    "Abstractions",
    "Services",
    "Registries",
    "Vectorization",
    "Vectors",
    "Motion",
    "Curves",
    "Clusters",
    "Policies",
    "Base",
    "Discoveries",
    "Facts",
    "Evidences",
    "Proofs"
)

$discoveriesFiles = foreach ($directory in $includeDirectories) {
    $path = Join-Path $DiscoveriesRoot $directory
    if (Test-Path $path) {
        Get-ChildItem $path -Recurse -File -Filter "*.cs"
    }
}

$rootFiles = Get-ChildItem $DiscoveriesRoot -File -Filter "*.cs"

$testPatterns = @(
    "*E7H1*",
    "*E7H2*",
    "*E7H3*"
)

$testFiles = foreach ($pattern in $testPatterns) {
    Get-ChildItem (Join-Path $TestsRoot "Entities\Certifications") `
        -Recurse -File -Filter "*.cs" |
        Where-Object { $_.FullName -like $pattern }
}

$runnerFiles = Get-ChildItem (Join-Path $TestsRoot "Entities\Runners") `
    -File -Filter "Invoke-E7H*.ps1"

$files = @($discoveriesFiles) + @($rootFiles) + @($testFiles) + @($runnerFiles) |
    Sort-Object FullName -Unique

$surfaceTerms = @(
    "public ",
    "interface ",
    "Options",
    "Policy",
    "Service",
    "Registry",
    "Catalog",
    "Mapping",
    "Profile",
    "Registration",
    "Result",
    "Factory",
    "Truth",
    "Consequence",
    "Cluster",
    "Vector"
)

function Write-Line {
    param([string]$Text = "")
    $Text | Out-File $Report -Append -Encoding utf8
}

Write-Line "============================================================"
Write-Line "WILDS DISCOVERIES S1 CONSUMER SURFACE CRAWL"
Write-Line "============================================================"
Write-Line "Generated: $(Get-Date -Format o)"
Write-Line "Repo root: $RepoRoot"
Write-Line "Discoveries root: $DiscoveriesRoot"
Write-Line "Selected files: $($files.Count)"
Write-Line ""

$coreStatus = @(git -C $RepoRoot status --short -- .\WildsEngine.Core)
Write-Line "============================================================"
Write-Line "CORE GUARD"
Write-Line "============================================================"
if ($coreStatus.Count -eq 0) {
    Write-Line "[PASS] WildsEngine.Core has no working-tree changes."
}
else {
    Write-Line "[WARN] WildsEngine.Core has working-tree changes:"
    $coreStatus | ForEach-Object { Write-Line $_ }
}
Write-Line ""

Write-Line "============================================================"
Write-Line "SOURCE INVENTORY"
Write-Line "============================================================"
foreach ($file in $files) {
    Write-Line $file.FullName.Substring($RepoRoot.Length).TrimStart('\')
}
Write-Line ""

Write-Line "============================================================"
Write-Line "SURFACE TERM INDEX"
Write-Line "============================================================"
foreach ($file in $files) {
    $matches = Select-String -Path $file.FullName -Pattern $surfaceTerms -SimpleMatch
    if ($matches) {
        Write-Line ""
        Write-Line "--- $($file.FullName.Substring($RepoRoot.Length).TrimStart('\'))"
        foreach ($match in $matches) {
            Write-Line ("{0,5}: {1}" -f $match.LineNumber, $match.Line.Trim())
        }
    }
}
Write-Line ""

Write-Line "============================================================"
Write-Line "FULL SOURCE BODIES"
Write-Line "============================================================"
foreach ($file in $files) {
    $relative = $file.FullName.Substring($RepoRoot.Length).TrimStart('\')
    Write-Line ""
    Write-Line "################################################################"
    Write-Line "FILE: $relative"
    Write-Line "################################################################"
    Get-Content $file.FullName | Out-File $Report -Append -Encoding utf8
}

Write-Line ""
Write-Line "============================================================"
Write-Line "S1 CLASSIFICATION WORKSHEET"
Write-Line "============================================================"
Write-Line "Classify each consumer-relevant type as one primary category:"
Write-Line "  OPTION       - instance/runtime configuration"
Write-Line "  DEFINITION   - consumer-authored structure or relationship"
Write-Line "  POLICY       - consumer-owned decision rule"
Write-Line "  SERVICE      - consumer or framework processing boundary"
Write-Line "  PROFILE      - typed composition for one capability/use case"
Write-Line "  REGISTRATION - immutable installed profile/configuration identity"
Write-Line "  RESULT       - attributable consumer-facing output"
Write-Line "  INTERNAL     - machinery hidden behind the Discoveries surface"
Write-Line ""
Write-Line "Required S1 outputs:"
Write-Line "  S1A - classified current surface inventory"
Write-Line "  S1B - strongly typed profile model and validation"
Write-Line "  S1C - immutable/versioned registration model"
Write-Line "  S1D - intentional WildsDiscoveries consumer surface"
Write-Line ""
Write-Line "[COMPLETE] S1 crawl captured current source bodies."

Copy-Item $Report $Latest -Force

Write-Host "S1 consumer-surface crawl complete." -ForegroundColor Green
Write-Host "Report: $Report"
Write-Host "Latest: $Latest"
