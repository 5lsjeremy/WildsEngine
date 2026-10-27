[CmdletBinding()]
param(
    [string] $Root = (Get-Location).Path
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$ExpectedRootName = "WildsEngine"
if ((Split-Path $Root -Leaf) -ne $ExpectedRootName) {
    throw "Run this audit from the WildsEngine repository root. Current path: $Root"
}

$SolutionPath = Join-Path $Root "WildsEngine.sln"
$CorePath = Join-Path $Root "WildsEngine.Core"
$TestsPath = Join-Path $Root "WildsEngine.Core.Tests"
$CoreProjectPath = Join-Path $CorePath "WildsEngine.Core.csproj"
$TestProjectPath = Join-Path $TestsPath "WildsEngine.Core.Tests.csproj"
$ReportDirectory = Join-Path $Root "Audit\Reports"

foreach ($RequiredPath in @($SolutionPath, $CoreProjectPath, $TestProjectPath)) {
    if (-not (Test-Path $RequiredPath -PathType Leaf)) {
        throw "Required file was not found: $RequiredPath"
    }
}

New-Item -ItemType Directory -Force -Path $ReportDirectory | Out-Null
$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$ReportPath = Join-Path $ReportDirectory "Wilds-P0-Processing-Audit-$Timestamp.md"

$Script:Passed = 0
$Script:Warnings = 0
$Script:Failed = 0
$Script:Findings = New-Object System.Collections.Generic.List[object]

function Get-Text {
    param([Parameter(Mandatory = $true)][string] $Path)

    if (-not (Test-Path $Path -PathType Leaf)) {
        return ""
    }

    return Get-Content -Path $Path -Raw
}

function Get-RelativePath {
    param([Parameter(Mandatory = $true)][string] $Path)

    $FullPath = [System.IO.Path]::GetFullPath($Path)

    if ($FullPath.StartsWith(
            $Root,
            [System.StringComparison]::OrdinalIgnoreCase)) {

        return $FullPath.Substring($Root.Length).TrimStart("\")
    }

    return $FullPath
}

function Add-Finding {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("PASS", "WARN", "FAIL")]
        [string] $Status,

        [Parameter(Mandatory = $true)]
        [string] $Area,

        [Parameter(Mandatory = $true)]
        [string] $Message,

        [string] $Evidence = ""
    )

    switch ($Status) {
        "PASS" { $Script:Passed++ }
        "WARN" { $Script:Warnings++ }
        "FAIL" { $Script:Failed++ }
    }

    $Script:Findings.Add([PSCustomObject]@{
        Status = $Status
        Area = $Area
        Message = $Message
        Evidence = $Evidence
    })
}

function Test-Pattern {
    param(
        [Parameter(Mandatory = $true)][string] $Text,
        [Parameter(Mandatory = $true)][string] $Pattern,
        [Parameter(Mandatory = $true)][string] $Area,
        [Parameter(Mandatory = $true)][string] $PassMessage,
        [Parameter(Mandatory = $true)][string] $FailMessage,
        [string] $Evidence = "",
        [ValidateSet("WARN", "FAIL")][string] $FailureStatus = "FAIL"
    )

    if ($Text -match $Pattern) {
        Add-Finding -Status "PASS" -Area $Area -Message $PassMessage -Evidence $Evidence
        return $true
    }

    Add-Finding -Status $FailureStatus -Area $Area -Message $FailMessage -Evidence $Evidence
    return $false
}

function Invoke-CapturedCommand {
    param(
        [Parameter(Mandatory = $true)][string] $Name,
        [Parameter(Mandatory = $true)][scriptblock] $Command
    )

    $PreviousPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"

    try {
        $Output = @(& $Command 2>&1)

        return [PSCustomObject]@{
            Name = $Name
            ExitCode = $LASTEXITCODE
            Output = $Output
        }
    }
    finally {
        $ErrorActionPreference = $PreviousPreference
    }
}

function Add-CommandFinding {
    param(
        [Parameter(Mandatory = $true)][object] $Result,
        [Parameter(Mandatory = $true)][string] $Area,
        [Parameter(Mandatory = $true)][string] $SuccessMessage,
        [Parameter(Mandatory = $true)][string] $FailureMessage
    )

    $Evidence = ($Result.Output | Out-String).Trim()

    if ($Evidence.Length -gt 4000) {
        $Evidence = $Evidence.Substring($Evidence.Length - 4000)
    }

    if ($Result.ExitCode -eq 0) {
        Add-Finding -Status "PASS" -Area $Area -Message $SuccessMessage -Evidence $Evidence
    }
    else {
        Add-Finding -Status "FAIL" -Area $Area -Message $FailureMessage -Evidence $Evidence
    }
}

function Escape-MarkdownTableValue {
    param([AllowEmptyString()][string] $Value)

    if ($null -eq $Value) {
        return ""
    }

    return ($Value -replace "\|", "\|" -replace "`r?`n", "<br>")
}

$AbstractionsPath = Join-Path $CorePath "Abstractions"
$IBaseObjectPath = Join-Path $AbstractionsPath "IBaseObject.cs"
$IComponentPath = Join-Path $AbstractionsPath "IComponent.cs"
$IStatePath = Join-Path $AbstractionsPath "IState.cs"
$IStateInstructionPath = Join-Path $AbstractionsPath "IStateInstruction.cs"
$StateInstructionPath = Join-Path $CorePath "Processing\StateInstruction.cs"
$IContextPath = Join-Path $AbstractionsPath "IContext.cs"
$ISignalPath = Join-Path $AbstractionsPath "ISignal.cs"
$BaseObjectPath = Join-Path $CorePath "Objects\BaseObject.cs"
$BaseComponentPath = Join-Path $CorePath "Components\BaseComponent.cs"
$ArbitraryTestsPath = Join-Path $TestsPath "ArbitraryObjectProcessingTests.cs"
$NullTestsPath = Join-Path $TestsPath "NullValueContractTests.cs"

Write-Host ""
Write-Host "[Wilds] P0 Processing Audit" -ForegroundColor Cyan
Write-Host "[Wilds] Root: $Root" -ForegroundColor DarkGray
Write-Host "[Wilds] Report: $ReportPath" -ForegroundColor DarkGray

$RequiredFiles = @(
    $IBaseObjectPath
    $IComponentPath
    $IStatePath
    $IStateInstructionPath
    $StateInstructionPath
    $IContextPath
    $ISignalPath
    $BaseObjectPath
    $BaseComponentPath
    $ArbitraryTestsPath
    $NullTestsPath
)

foreach ($File in $RequiredFiles) {
    if (Test-Path $File -PathType Leaf) {
        Add-Finding -Status "PASS" -Area "Required files" -Message "Required file exists." -Evidence (Get-RelativePath $File)
    }
    else {
        Add-Finding -Status "FAIL" -Area "Required files" -Message "Required file is missing." -Evidence (Get-RelativePath $File)
    }
}

$IBaseObjectText = Get-Text $IBaseObjectPath
$IComponentText = Get-Text $IComponentPath
$IStateText = Get-Text $IStatePath
$IStateInstructionText = Get-Text $IStateInstructionPath
$StateInstructionText = Get-Text $StateInstructionPath
$IContextText = Get-Text $IContextPath
$BaseObjectText = Get-Text $BaseObjectPath
$BaseComponentText = Get-Text $BaseComponentPath
$CoreProjectText = Get-Text $CoreProjectPath
$ArbitraryTestsText = Get-Text $ArbitraryTestsPath
$NullTestsText = Get-Text $NullTestsPath

$TestCorpus =
$ArbitraryTestsText +
        "`n" +
        $NullTestsText +
        "`n" +
        $IStateInstructionText +
        "`n" +
        $StateInstructionText +
        "`n" +
        $IContextText

Test-Pattern $CoreProjectText "<TargetFramework>\s*netstandard2\.1\s*</TargetFramework>" "Target framework" "WildsEngine.Core targets netstandard2.1." "WildsEngine.Core does not target netstandard2.1." "WildsEngine.Core.csproj" | Out-Null

Test-Pattern $IBaseObjectText "\bGuid\s+Id\s*\{\s*get;\s*\}" "IBaseObject" "Canonical GUID identity is present." "Guid Id is missing." "IBaseObject.cs" | Out-Null
Test-Pattern $IBaseObjectText "\bstring\s+Slug\s*\{\s*get;\s*\}" "IBaseObject" "Slug identity is present." "Slug is missing." "IBaseObject.cs" | Out-Null
Test-Pattern $IBaseObjectText "IReadOnlyList\s*<\s*IBaseObject\s*>\s+Neighbors" "IBaseObject" "Neighbor exposure is read-only." "Neighbors contract is missing." "IBaseObject.cs" | Out-Null
Test-Pattern $IBaseObjectText "\bIComponent\s+Component\s*\{\s*get;\s*\}" "IBaseObject" "Component gateway is present." "Component gateway is missing." "IBaseObject.cs" | Out-Null
Test-Pattern $IBaseObjectText "\bstring\s+AppendSlug\s*\(\s*string\s+value\s*\)" "IBaseObject" "Appendable slug behavior is present." "AppendSlug is missing." "IBaseObject.cs" | Out-Null
Test-Pattern $IBaseObjectText "\bbool\s+AddNeighbor\s*\(\s*IBaseObject\s+neighbor\s*\)" "IBaseObject" "Neighbor addition reports topology change." "AddNeighbor contract is missing." "IBaseObject.cs" | Out-Null
Test-Pattern $IBaseObjectText "\bbool\s+RemoveNeighbor\s*\(\s*Guid\s+id\s*\)" "IBaseObject" "Neighbor removal uses GUID identity." "RemoveNeighbor contract is missing." "IBaseObject.cs" | Out-Null

foreach ($Forbidden in @(
    "\bIState\s+State\s*\{",
    "\bISignal\s+Signal\s*\{",
    "IDictionary\s*<[^>]+>\s+Metadata\s*\{"
)) {
    if ($IBaseObjectText -match $Forbidden) {
        Add-Finding -Status "FAIL" -Area "IBaseObject ownership" -Message "IBaseObject redeclares a component-owned shortcut." -Evidence $Forbidden
    }
    else {
        Add-Finding -Status "PASS" -Area "IBaseObject ownership" -Message "Component-owned shortcut is not redeclared on IBaseObject." -Evidence $Forbidden
    }
}

Test-Pattern $IComponentText "interface\s+IComponent\s*:\s*IBaseObject" "IComponent" "IComponent extends IBaseObject." "IComponent does not extend IBaseObject." "IComponent.cs" | Out-Null
Test-Pattern $IComponentText "\bIState\s+State\s*\{\s*get;\s*\}" "IComponent" "IComponent owns State." "IComponent does not own State." "IComponent.cs" | Out-Null
Test-Pattern $IComponentText "\bISignal\s+Signal\s*\{\s*get;\s*\}" "IComponent" "IComponent owns Signal." "IComponent does not own Signal." "IComponent.cs" | Out-Null
Test-Pattern $IComponentText "IDictionary\s*<\s*string\s*,\s*object\?\s*>\s+Metadata" "IComponent" "Component metadata explicitly supports nullable arbitrary values." "Component metadata is not explicitly nullable." "IComponent.cs" "WARN" | Out-Null

Test-Pattern $BaseObjectText "IState\s+State\s*=>\s*Component\.State" "BaseObject shortcuts" "State shortcut resolves through Component.State." "State shortcut is missing or incorrect." "BaseObject.cs" | Out-Null
Test-Pattern $BaseObjectText "ISignal\s+Signal\s*=>\s*Component\.Signal" "BaseObject shortcuts" "Signal shortcut resolves through Component.Signal." "Signal shortcut is missing or incorrect." "BaseObject.cs" | Out-Null
Test-Pattern $BaseObjectText "Metadata\s*=>\s*[\r\n\s]*Component\.Metadata" "BaseObject shortcuts" "Metadata shortcut resolves through Component.Metadata." "Metadata shortcut is missing or incorrect." "BaseObject.cs" | Out-Null

Test-Pattern $BaseComponentText "Component\s*=>\s*this" "BaseComponent" "BaseComponent resolves Component to itself." "BaseComponent does not resolve Component to itself." "BaseComponent.cs" | Out-Null
Test-Pattern $BaseComponentText "Dictionary\s*<\s*string\s*,\s*object\?\s*>\s+_metadata" "BaseComponent" "BaseComponent owns nullable component metadata." "BaseComponent metadata is not explicitly nullable." "BaseComponent.cs" "WARN" | Out-Null
Test-Pattern $BaseComponentText "abstract\s+override\s+IState\s+State" "BaseComponent" "Concrete components must provide State." "BaseComponent does not require State." "BaseComponent.cs" | Out-Null
Test-Pattern $BaseComponentText "abstract\s+override\s+ISignal\s+Signal" "BaseComponent" "Concrete components must provide Signal." "BaseComponent does not require Signal." "BaseComponent.cs" | Out-Null

Test-Pattern $IStateText "interface\s+IState\s*:\s*IBaseObject" "IState" "IState extends IBaseObject." "IState does not extend IBaseObject." "IState.cs" | Out-Null
Test-Pattern $IStateText "\bIContext\s+Context\s*\{\s*get;\s*\}" "IState" "IState owns Context." "IState does not expose Context." "IState.cs" | Out-Null
Test-Pattern $IStateText "\bbool\s+Process\s*\(\s*IStateInstruction\s+instruction\s*\)" "IState" "IState processes a Wilds-native instruction." "IState.Process has drifted." "IState.cs" | Out-Null

if ($IStateText -match "Process\s*\([^)]*IContext") {
    Add-Finding -Status "FAIL" -Area "IState" -Message "Context is incorrectly passed into Process()." -Evidence "IState.cs"
}
else {
    Add-Finding -Status "PASS" -Area "IState" -Message "Process does not receive duplicate Context." -Evidence "IState.cs"
}

Test-Pattern $IStateInstructionText "interface\s+IStateInstruction" "IStateInstruction" "Wilds-native instruction boundary exists." "IStateInstruction is missing." "IStateInstruction.cs" | Out-Null
Test-Pattern $IStateInstructionText "\bGuid\s+Id\s*\{\s*get;\s*\}" "IStateInstruction" "Instruction identity contract is present." "Instruction identity contract is missing." "IStateInstruction.cs" | Out-Null
Test-Pattern $IStateInstructionText "\bstring\s+Intent\s*\{\s*get;\s*\}" "IStateInstruction" "Instruction intent contract is present." "Instruction intent contract is missing." "IStateInstruction.cs" | Out-Null
Test-Pattern $IStateInstructionText "object\?\s+Payload\s*\{\s*get;\s*\}" "IStateInstruction" "Instruction payload is nullable and arbitrary." "Instruction payload contract is missing or not nullable." "IStateInstruction.cs" | Out-Null
Test-Pattern $IStateInstructionText "IReadOnlyDictionary\s*<\s*string\s*,\s*object\?\s*>\s+Arguments" "IStateInstruction" "Instruction arguments are read-only and nullable." "Instruction arguments contract is missing or not nullable." "IStateInstruction.cs" | Out-Null
Test-Pattern $IStateInstructionText "IReadOnlyDictionary\s*<\s*string\s*,\s*object\?\s*>\s+Metadata" "IStateInstruction" "Instruction metadata is read-only and nullable." "Instruction metadata contract is missing or not nullable." "IStateInstruction.cs" | Out-Null

Test-Pattern $StateInstructionText "public\s+abstract\s+class\s+StateInstruction\s*:\s*IStateInstruction" "StateInstruction" "StateInstruction is a public abstract IStateInstruction base." "StateInstruction is missing or does not implement IStateInstruction." "Processing\StateInstruction.cs" | Out-Null
Test-Pattern $StateInstructionText "protected\s+StateInstruction\s*\(" "StateInstruction" "External instruction sets can derive through a protected constructor." "StateInstruction does not expose a protected constructor." "Processing\StateInstruction.cs" | Out-Null
Test-Pattern $StateInstructionText "Id\s*=\s*Guid\.NewGuid\s*\(\s*\)" "StateInstruction" "StateInstruction creates canonical instruction identity." "StateInstruction does not create instruction identity." "Processing\StateInstruction.cs" | Out-Null
Test-Pattern $StateInstructionText "public\s+object\?\s+Payload\s*\{\s*get;\s*\}" "StateInstruction" "StateInstruction exposes nullable arbitrary payloads." "StateInstruction payload is missing or not nullable." "Processing\StateInstruction.cs" | Out-Null
Test-Pattern $StateInstructionText "protected\s+void\s+SetArgument\s*\(" "StateInstruction" "Derived instruction sets can populate arguments." "StateInstruction does not provide protected argument mutation." "Processing\StateInstruction.cs" | Out-Null
Test-Pattern $StateInstructionText "protected\s+void\s+SetMetadata\s*\(" "StateInstruction" "Derived instruction sets can populate metadata." "StateInstruction does not provide protected metadata mutation." "Processing\StateInstruction.cs" | Out-Null

Test-Pattern $IContextText "IReadOnlyDictionary\s*<\s*string\s*,\s*object\?\s*>\s+Variables" "IContext" "Variables explicitly support nullable arbitrary values." "Variables are not explicitly nullable." "IContext.cs" | Out-Null
Test-Pattern $IContextText "IReadOnlyDictionary\s*<\s*string\s*,\s*object\?\s*>\s+Metadata" "IContext" "Context Metadata explicitly supports nullable arbitrary values." "Context Metadata is not explicitly nullable." "IContext.cs" | Out-Null
Test-Pattern $IContextText "SetVariable\s*\(\s*string\s+key\s*,\s*object\?\s+value\s*\)" "IContext" "SetVariable accepts null." "SetVariable is not explicitly nullable." "IContext.cs" | Out-Null
Test-Pattern $IContextText "SetMetadata\s*\(\s*string\s+key\s*,\s*object\?\s+value\s*\)" "IContext" "SetMetadata accepts null." "SetMetadata is not explicitly nullable." "IContext.cs" | Out-Null

$CoreSourceFiles = @(
Get-ChildItem $CorePath -Filter "*.cs" -File -Recurse |
        Where-Object {
            $_.FullName -notmatch "[\\/](bin|obj|Audit)[\\/]"
        }
)

$QslReferences = @(
$CoreSourceFiles |
        Select-String `
            -Pattern "\bQSL\b|\bQsl[A-Za-z0-9_]*\b" `
            -CaseSensitive:$false
)

if ($QslReferences.Count -eq 0) {
    Add-Finding -Status "PASS" -Area "QSL isolation" -Message "WildsEngine.Core contains no direct QSL references." -Evidence "WildsEngine.Core"
}
else {
    $QslEvidence = (
    $QslReferences |
            Select-Object -First 10 |
            ForEach-Object {
                "$(Get-RelativePath $_.Path):$($_.LineNumber)"
            }
    ) -join "`n"

    Add-Finding -Status "FAIL" -Area "QSL isolation" -Message "WildsEngine.Core directly references QSL." -Evidence $QslEvidence
}

$TestRequirements = @(
    @{ Name = "Reference-type consumer object"; Pattern = "State_processes_external_reference_type_without_core_knowledge" },
    @{ Name = "Value-type consumer object"; Pattern = "State_processes_external_value_type_without_core_knowledge" },
    @{ Name = "Reference identity preservation"; Pattern = "Assert\.Same\s*\(\s*payload\s*,\s*stored\s*\)" },
    @{ Name = "Null present versus missing"; Pattern = "NullValue_IsDistinctFromMissingKey" },
    @{ Name = "State value-to-null transition"; Pattern = "State_processing_can_transition_existing_value_to_null" },
    @{ Name = "State null-to-value transition"; Pattern = "State_processing_can_transition_null_to_consumer_object" },
    @{ Name = "Nullable instruction payload"; Pattern = "object\?\s+Payload\s*\{\s*get;" },
    @{ Name = "Abstract instruction implementation"; Pattern = "abstract\s+class\s+StateInstruction" },
    @{ Name = "Externally derivable instruction base"; Pattern = "protected\s+StateInstruction\s*\(" },
    @{ Name = "Nullable context variables contract"; Pattern = "IReadOnlyDictionary\s*<\s*string\s*,\s*object\?\s*>\s+Variables" },
    @{ Name = "Nullable context variable setter contract"; Pattern = "SetVariable\s*\(\s*string\s+key\s*,\s*object\?\s+value\s*\)" }
)

foreach ($Requirement in $TestRequirements) {
    if ($TestCorpus -match $Requirement.Pattern) {
        Add-Finding -Status "PASS" -Area "P0 test matrix" -Message "$($Requirement.Name) evidence was found." -Evidence $Requirement.Pattern
    }
    else {
        Add-Finding -Status "FAIL" -Area "P0 test matrix" -Message "$($Requirement.Name) evidence was not found." -Evidence $Requirement.Pattern
    }
}

Write-Host "[Wilds] Building solution..." -ForegroundColor Yellow

$BuildResult = Invoke-CapturedCommand `
    -Name "dotnet build" `
    -Command {
    dotnet build $SolutionPath --nologo
}

Add-CommandFinding `
    $BuildResult `
    "Build" `
    "WildsEngine.sln builds successfully." `
    "WildsEngine.sln failed to build."

Write-Host "[Wilds] Testing WildsEngine.Core.Tests..." -ForegroundColor Yellow

$TestResult = Invoke-CapturedCommand `
    -Name "dotnet test" `
    -Command {
    dotnet test $TestProjectPath --no-build --nologo
}

Add-CommandFinding `
    $TestResult `
    "Tests" `
    "WildsEngine.Core.Tests passed." `
    "WildsEngine.Core.Tests failed."

$BuildEvidence = ($BuildResult.Output | Out-String)
$TestEvidence = ($TestResult.Output | Out-String)

$WarningsFound = [regex]::Matches(
        $BuildEvidence,
        "(?im)^.*warning\s+[A-Z]+\d+.*$"
)

if ($WarningsFound.Count -eq 0) {
    Add-Finding -Status "PASS" -Area "Compiler warnings" -Message "Solution build contains no compiler warnings." -Evidence "dotnet build"
}
else {
    Add-Finding -Status "FAIL" -Area "Compiler warnings" -Message "Compiler warnings were found." -Evidence (($WarningsFound.Value) -join "`n")
}

$TestSummaryPattern =
"Failed:\s*0,\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+),\s*Total:\s*(\d+)"

$TestSummaryMatch = [regex]::Match(
        $TestEvidence,
        $TestSummaryPattern
)

if ($TestSummaryMatch.Success) {
    $PassedTests =
    [int] $TestSummaryMatch.Groups[1].Value

    $SkippedTests =
    [int] $TestSummaryMatch.Groups[2].Value

    $TotalTests =
    [int] $TestSummaryMatch.Groups[3].Value

    if (
    $PassedTests -eq $TotalTests -and
            $SkippedTests -eq 0
    ) {
        Add-Finding `
            -Status "PASS" `
            -Area "Test count" `
            -Message "All $TotalTests processing tests passed." `
            -Evidence "$PassedTests passed, 0 failed, 0 skipped"
    }
    else {
        Add-Finding `
            -Status "WARN" `
            -Area "Test count" `
            -Message "The processing test suite did not complete with every test passing." `
            -Evidence ($TestEvidence.Trim())
    }
}
else {
    Add-Finding `
        -Status "WARN" `
        -Area "Test count" `
        -Message "The test summary was not detected; inspect test output." `
        -Evidence ($TestEvidence.Trim())
}

if ($Script:Failed -gt 0) {
    $Certification = "FAILED"
    $CertificationReason =
    "One or more required architecture, build, warning, or test checks failed."
}
elseif ($Script:Warnings -gt 0) {
    $Certification = "P0 READY WITH WARNINGS"
    $CertificationReason =
    "All required checks passed, but non-blocking audit warnings remain."
}
else {
    $Certification = "P0 READY"
    $CertificationReason =
    "Contracts, instruction extensibility, nullability, build, and processing tests support arbitrary consumer-owned object and instruction processing."
}

$ReportLines =
New-Object System.Collections.Generic.List[string]

$ReportLines.Add("# Wilds P0 Processing Audit")
$ReportLines.Add("")
$ReportLines.Add("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$ReportLines.Add("")
$ReportLines.Add("Repository: ``$Root``")
$ReportLines.Add("")
$ReportLines.Add("## Certification")
$ReportLines.Add("")
$ReportLines.Add("**$Certification**")
$ReportLines.Add("")
$ReportLines.Add($CertificationReason)
$ReportLines.Add("")
$ReportLines.Add("## Summary")
$ReportLines.Add("")
$ReportLines.Add("- Passed: $Script:Passed")
$ReportLines.Add("- Warnings: $Script:Warnings")
$ReportLines.Add("- Failed: $Script:Failed")
$ReportLines.Add("")
$ReportLines.Add("## Findings")
$ReportLines.Add("")
$ReportLines.Add("| Status | Area | Finding | Evidence |")
$ReportLines.Add("|---|---|---|---|")

foreach ($Finding in $Script:Findings) {
    $ReportLines.Add(
            "| $(Escape-MarkdownTableValue $Finding.Status) | " +
                    "$(Escape-MarkdownTableValue $Finding.Area) | " +
                    "$(Escape-MarkdownTableValue $Finding.Message) | " +
                    "$(Escape-MarkdownTableValue $Finding.Evidence) |"
    )
}

$ReportLines.Add("")
$ReportLines.Add("## Ownership Checkpoint")
$ReportLines.Add("")
$ReportLines.Add("- ``IBaseObject`` owns identity, slug, neighbors, and the Component gateway.")
$ReportLines.Add("- ``IComponent`` owns State, Signal, and component Metadata.")
$ReportLines.Add("- ``BaseObject`` supplies State, Signal, and Metadata shortcuts through Component.")
$ReportLines.Add("- ``IState`` owns Context and processes ``IStateInstruction``.")
$ReportLines.Add("- ``IStateInstruction`` defines the external instruction contract.")
$ReportLines.Add("- ``StateInstruction`` provides the public abstract base for internal and consumer-owned instruction sets.")
$ReportLines.Add("- ``IStateInstruction`` and ``StateInstruction`` preserve nullable arbitrary Payload, Arguments, and Metadata values.")
$ReportLines.Add("- ``IContext`` owns nullable Variables and contextual Metadata.")
$ReportLines.Add("- Context is not passed into ``IState.Process()``.")
$ReportLines.Add("- WildsEngine.Core does not depend directly on QSL.")
$ReportLines.Add("")
$ReportLines.Add("## Build Output")
$ReportLines.Add('```text')

foreach ($Line in $BuildResult.Output) {
    $ReportLines.Add([string] $Line)
}

$ReportLines.Add('```')
$ReportLines.Add("")
$ReportLines.Add("## Test Output")
$ReportLines.Add("")
$ReportLines.Add('```text')

foreach ($Line in $TestResult.Output) {
    $ReportLines.Add([string] $Line)
}

$ReportLines.Add('```')

[System.IO.File]::WriteAllLines(
        $ReportPath,
        $ReportLines,
        [System.Text.UTF8Encoding]::new($false)
)

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " Wilds P0 Processing Audit" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host "Passed   : $Script:Passed" -ForegroundColor Green
Write-Host "Warnings : $Script:Warnings" -ForegroundColor Yellow
Write-Host "Failed   : $Script:Failed" -ForegroundColor Red

switch ($Certification) {
    "P0 READY" {
        Write-Host "Status   : $Certification" -ForegroundColor Green
    }

    "P0 READY WITH WARNINGS" {
        Write-Host "Status   : $Certification" -ForegroundColor Yellow
    }

    default {
        Write-Host "Status   : $Certification" -ForegroundColor Red
    }
}

Write-Host "Report   : $ReportPath" -ForegroundColor Cyan
Write-Host ""

if ($Script:Failed -gt 0) {
    exit 1
}

exit 0