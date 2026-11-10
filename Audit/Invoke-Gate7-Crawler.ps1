[CmdletBinding()]
param(
    [string]$Root = "",
    [string]$OutputDirectory = "",
    [switch]$RunBuild,
    [switch]$RunTests,
    [switch]$RunSerializedTests,
    [switch]$IncludeReports
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# -----------------------------------------------------------------------------
# Root and output resolution
# -----------------------------------------------------------------------------
if ([string]::IsNullOrWhiteSpace($Root)) {
    $scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
    $Root = Split-Path -Parent $scriptDirectory
}

$Root = [System.IO.Path]::GetFullPath($Root)
if (-not (Test-Path -LiteralPath $Root)) {
    throw "WildsEngine root does not exist: $Root"
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $Root "Audit"
}

$OutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
if (-not (Test-Path -LiteralPath $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$reportPath = Join-Path $OutputDirectory "WildsEngine-Gate7-Crawl-$timestamp.txt"
$latestReportPath = Join-Path $OutputDirectory "WildsEngine-Gate7-Crawl-Latest.txt"

$coreRoot = Join-Path $Root "WildsEngine.Core"
$qslRoot = Join-Path $Root "WildsEngine.Qsl"
$testsRoot = Join-Path $Root "WildsEngine.Core.Tests"

$benchmarkRootCandidates = @(
    (Join-Path $Root "WildsEngine.Benchmarks"),
    (Join-Path $Root "WildsEngine.ConsoleTests"),
    (Join-Path $Root "Benchmarks")
)

$excludedDirectoryNames = @(
    ".git", ".vs", ".idea", "bin", "obj", "packages", "TestResults"
)

if (-not $IncludeReports) {
    $excludedDirectoryNames += @("Certification", "Reports", "Audit")
}

$report =
[System.Collections.Generic.List[string]]::new()

function Add-ReportLine {
    param([AllowEmptyString()][string]$Text = "")
    $script:report.Add($Text)
}

function Add-Section {
    param([Parameter(Mandatory = $true)][string]$Title)
    Add-ReportLine ""
    Add-ReportLine "============================================================"
    Add-ReportLine $Title
    Add-ReportLine "============================================================"
}

function Add-Subsection {
    param([Parameter(Mandatory = $true)][string]$Title)
    Add-ReportLine ""
    Add-ReportLine "------------------------------------------------------------"
    Add-ReportLine $Title
    Add-ReportLine "------------------------------------------------------------"
}

function Write-Status {
    param(
        [Parameter(Mandatory = $true)][string]$Label,
        [Parameter(Mandatory = $true)][string]$Message,
        [ConsoleColor]$Color = [ConsoleColor]::Gray
    )

    Write-Host ("[{0}] {1}" -f $Label, $Message) -ForegroundColor $Color
}

function Get-RelativePath {
    param(
        [Parameter(Mandatory = $true)][string]$BasePath,
        [Parameter(Mandatory = $true)][string]$TargetPath
    )

    $resolvedBase = [System.IO.Path]::GetFullPath($BasePath)
    $resolvedTarget = [System.IO.Path]::GetFullPath($TargetPath)
    $separator = [System.IO.Path]::DirectorySeparatorChar
    $alternate = [System.IO.Path]::AltDirectorySeparatorChar
    $baseUri = New-Object System.Uri (($resolvedBase.TrimEnd($separator, $alternate)) + $separator)
    $targetUri = New-Object System.Uri $resolvedTarget
    $relativeUri = $baseUri.MakeRelativeUri($targetUri)

    return [System.Uri]::UnescapeDataString($relativeUri.ToString()).Replace("/", $separator)
}

function Test-IsExcludedPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    foreach ($segment in ($Path -split "[\\/]")) {
        if ($excludedDirectoryNames -contains $segment) {
            return $true
        }
    }

    return $false
}

function Get-FilesByExtension {
    param(
        [Parameter(Mandatory = $true)][string]$SearchRoot,
        [Parameter(Mandatory = $true)][string[]]$Extensions
    )

    if (-not (Test-Path -LiteralPath $SearchRoot)) {
        return @()
    }

    return @(
    Get-ChildItem -LiteralPath $SearchRoot -File -Recurse |
            Where-Object {
                ($Extensions -contains $_.Extension.ToLowerInvariant()) -and
                        (-not (Test-IsExcludedPath -Path $_.FullName))
            } |
            Sort-Object FullName
    )
}

function Get-CSharpFiles {
    param([Parameter(Mandatory = $true)][string]$SearchRoot)
    return @(Get-FilesByExtension -SearchRoot $SearchRoot -Extensions @(".cs"))
}

function Get-ProjectFiles {
    param([Parameter(Mandatory = $true)][string]$SearchRoot)
    return @(Get-FilesByExtension -SearchRoot $SearchRoot -Extensions @(".csproj"))
}

function Get-ArchitectureFiles {
    param([Parameter(Mandatory = $true)][string]$SearchRoot)

    if (-not (Test-Path -LiteralPath $SearchRoot)) {
        return @()
    }

    return @(
    Get-ChildItem -LiteralPath $SearchRoot -File -Recurse |
            Where-Object {
                (-not (Test-IsExcludedPath -Path $_.FullName)) -and
                        (
                        $_.Extension -in @(".md", ".props", ".targets", ".json") -or
                                $_.Name -like "Directory.Build.*"
                        ) -and
                        (
                        $_.Name -match "(?i)(readme|architecture|milestone|gateway|emission|state|wilds|qsl|directory\.build|global\.json)" -or
                                $_.Extension -in @(".props", ".targets")
                        )
            } |
            Sort-Object FullName
    )
}

function Add-CommandOutput {
    param(
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][scriptblock]$Command
    )

    Add-Subsection $Title

    try {
        $output = & $Command 2>&1 | Out-String
        if ([string]::IsNullOrWhiteSpace($output)) {
            Add-ReportLine "<no output>"
        }
        else {
            foreach ($line in ($output -split "`r?`n")) {
                Add-ReportLine $line
            }
        }
    }
    catch {
        Add-ReportLine ("[ERROR] {0}: {1}" -f $_.Exception.GetType().Name, $_.Exception.Message)
    }
}

function Add-PatternAudit {
    param(
        [Parameter(Mandatory = $true)][string]$SectionTitle,
        [Parameter(Mandatory = $true)][string[]]$Patterns,
        [Parameter(Mandatory = $true)][System.IO.FileInfo[]]$Files,
        [switch]$Regex
    )

    Add-Section $SectionTitle

    foreach ($pattern in $Patterns) {
        Add-Subsection "Pattern: $pattern"
        $matchesFound = 0

        foreach ($file in $Files) {
            if ($Regex) {
                $lineMatches = Select-String -LiteralPath $file.FullName -Pattern $pattern
            }
            else {
                $lineMatches = Select-String -LiteralPath $file.FullName -SimpleMatch -Pattern $pattern
            }

            foreach ($lineMatch in $lineMatches) {
                $matchesFound++
                Add-ReportLine (
                "{0}:{1}: {2}" -f
                (Get-RelativePath -BasePath $Root -TargetPath $file.FullName),
                $lineMatch.LineNumber,
                $lineMatch.Line.Trim()
                )
            }
        }

        if ($matchesFound -eq 0) {
            Add-ReportLine "<no matches>"
        }
    }
}

function Add-CompleteFileCapture {
    param(
        [Parameter(Mandatory = $true)][string]$SectionTitle,
        [Parameter(Mandatory = $true)][System.IO.FileInfo[]]$Files
    )

    Add-Section $SectionTitle

    foreach ($file in $Files) {
        $relative = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
        Add-ReportLine ""
        Add-ReportLine "############################################################"
        Add-ReportLine "FILE: $relative"
        Add-ReportLine "############################################################"

        $lineNumber = 0
        foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
            $lineNumber++
            Add-ReportLine ("{0,5}: {1}" -f $lineNumber, $line)
        }
    }
}

function Get-DeclarationInventory {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo[]]$Files
    )

    $pattern =
    '^\s*(?:public|internal|protected|private)?\s*' +
            '(?:readonly\s+|ref\s+|sealed\s+|abstract\s+|' +
            'static\s+|partial\s+)*' +
            '(class|interface|struct|record|enum)\s+' +
            '([A-Za-z_][A-Za-z0-9_]*)'

    foreach ($file in $Files) {
        $lineNumber = 0

        foreach ($line in
        [System.IO.File]::ReadLines(
                $file.FullName)) {

            $lineNumber++

            $match =
            [regex]::Match(
                    $line,
                    $pattern)

            if (-not $match.Success) {
                continue
            }

            [pscustomobject]@{
                Kind =
                $match.Groups[1].Value

                Name =
                $match.Groups[2].Value

                File =
                Get-RelativePath `
                        -BasePath $Root `
                        -TargetPath $file.FullName

                Line =
                $lineNumber

                Declaration =
                $line.Trim()
            }
        }
    }
}

# -----------------------------------------------------------------------------
# Collection
# -----------------------------------------------------------------------------
Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " WildsEngine Gate 7 Architecture Crawler" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host ""
Write-Status -Label "ROOT" -Message $Root -Color Cyan

$coreFiles = @(Get-CSharpFiles -SearchRoot $coreRoot)
$qslFiles = @(Get-CSharpFiles -SearchRoot $qslRoot)
$testFiles = @(Get-CSharpFiles -SearchRoot $testsRoot)
$allSourceFiles = @(Get-CSharpFiles -SearchRoot $Root)
$projectFiles = @(Get-ProjectFiles -SearchRoot $Root)
$architectureFiles = @(Get-ArchitectureFiles -SearchRoot $Root)

$benchmarkFiles =
[System.Collections.Generic.List[System.IO.FileInfo]]::new()
$benchmarkSeen = @{}
foreach ($candidate in $benchmarkRootCandidates) {
    foreach ($file in (Get-CSharpFiles -SearchRoot $candidate)) {
        $key = $file.FullName.ToLowerInvariant()
        if (-not $benchmarkSeen.ContainsKey($key)) {
            $benchmarkSeen[$key] = $true
            $benchmarkFiles.Add($file)
        }
    }
}

$declarations = @(Get-DeclarationInventory -Files $allSourceFiles)
$duplicateDeclarations = @(
$declarations |
        Group-Object Name |
        Where-Object { $_.Count -gt 1 } |
        Sort-Object Name
)

Write-Status -Label "FOUND" -Message "$($coreFiles.Count) Core C# files" -Color Green
Write-Status -Label "FOUND" -Message "$($qslFiles.Count) Wilds/QSL C# files" -Color Green
Write-Status -Label "FOUND" -Message "$($testFiles.Count) test C# files" -Color Green
Write-Status -Label "FOUND" -Message "$($projectFiles.Count) project files" -Color Green
Write-Status -Label "FOUND" -Message "$($architectureFiles.Count) architecture/config files" -Color Green

# -----------------------------------------------------------------------------
# Report header and tree
# -----------------------------------------------------------------------------
Add-Section "WILDSENGINE GATE 7 ARCHITECTURE CRAWL"
Add-ReportLine "Root: $Root"
Add-ReportLine "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss K')"
Add-ReportLine "Core: $coreRoot"
Add-ReportLine "Wilds/QSL: $qslRoot"
Add-ReportLine "Tests: $testsRoot"
Add-ReportLine "Core C# files: $($coreFiles.Count)"
Add-ReportLine "Wilds/QSL C# files: $($qslFiles.Count)"
Add-ReportLine "Test C# files: $($testFiles.Count)"
Add-ReportLine "All repository C# files: $($allSourceFiles.Count)"
Add-ReportLine "Project files: $($projectFiles.Count)"
Add-ReportLine "Architecture/config files: $($architectureFiles.Count)"
Add-ReportLine "Include reports: $IncludeReports"

Add-Section "REPOSITORY TREE"
Add-CommandOutput -Title "Repository tree" -Command {
    Push-Location $Root
    try {
        tree /f /a
    }
    finally {
        Pop-Location
    }
}

# -----------------------------------------------------------------------------
# Project and source inventories
# -----------------------------------------------------------------------------
Add-Section "PROJECT INVENTORY"
foreach ($project in $projectFiles) {
    Add-ReportLine (Get-RelativePath -BasePath $Root -TargetPath $project.FullName)
}

Add-Section "PROJECT CONFIGURATION AUDIT"
$projectPatterns = @(
    "<TargetFramework>",
    "<TargetFrameworks>",
    "<ProjectReference",
    "<PackageReference",
    "<Nullable>",
    "<ImplicitUsings>",
    "<LangVersion>"
)
foreach ($project in $projectFiles) {
    Add-Subsection (Get-RelativePath -BasePath $Root -TargetPath $project.FullName)
    foreach ($pattern in $projectPatterns) {
        $matches = @(Select-String -LiteralPath $project.FullName -SimpleMatch -Pattern $pattern)
        foreach ($match in $matches) {
            Add-ReportLine ("{0}: {1}" -f $match.LineNumber, $match.Line.Trim())
        }
    }
}

Add-Section "SOURCE INVENTORY"
$inventoryGroups = @(
    @{ Name = "Core abstractions"; Path = Join-Path $coreRoot "Abstractions" },
    @{ Name = "Core emission"; Path = Join-Path $coreRoot "Emission" },
    @{ Name = "Core state"; Path = Join-Path $coreRoot "State" },
    @{ Name = "Core topology"; Path = Join-Path $coreRoot "Topology" },
    @{ Name = "Core processing"; Path = Join-Path $coreRoot "Processing" },
    @{ Name = "Core objects"; Path = Join-Path $coreRoot "Objects" },
    @{ Name = "Core pooling"; Path = Join-Path $coreRoot "Pooling" },
    @{ Name = "Core signals"; Path = Join-Path $coreRoot "Signals" },
    @{ Name = "Wilds/QSL"; Path = $qslRoot },
    @{ Name = "Tests"; Path = $testsRoot },
    @{ Name = "Benchmarks/console"; Files = @($benchmarkFiles) }
)

foreach ($group in $inventoryGroups) {
    Add-Subsection $group.Name

    if ($group.ContainsKey("Files")) {
        $files = @($group.Files)
    }
    elseif (-not (Test-Path -LiteralPath $group.Path)) {
        Add-ReportLine "<directory not found>"
        continue
    }
    else {
        $files = @(Get-CSharpFiles -SearchRoot $group.Path)
    }

    if ($files.Count -eq 0) {
        Add-ReportLine "<no C# files>"
        continue
    }

    foreach ($file in $files) {
        Add-ReportLine (Get-RelativePath -BasePath $Root -TargetPath $file.FullName)
    }
}

# -----------------------------------------------------------------------------
# Declaration and namespace audits
# -----------------------------------------------------------------------------
Add-Section "DECLARATION INVENTORY"
foreach ($declaration in ($declarations | Sort-Object Kind, Name, File, Line)) {
    Add-ReportLine (
    "{0,-10} {1,-45} {2}:{3}" -f
    $declaration.Kind,
    $declaration.Name,
    $declaration.File,
    $declaration.Line
    )
}

Add-Section "DUPLICATE TYPE DECLARATION AUDIT"
if ($duplicateDeclarations.Count -eq 0) {
    Add-ReportLine "[PASS] No duplicate type names found."
}
else {
    foreach ($duplicate in $duplicateDeclarations) {
        Add-ReportLine ""
        Add-ReportLine ("[REVIEW] {0} appears {1} times" -f $duplicate.Name, $duplicate.Count)
        foreach ($entry in $duplicate.Group) {
            Add-ReportLine ("    {0} at {1}:{2}" -f $entry.Kind, $entry.File, $entry.Line)
        }
    }
}

Add-Section "NAMESPACE AUDIT"
$namespacePattern = '^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)'
$namespaceRecords =
[System.Collections.Generic.List[object]]::new()

foreach ($file in $allSourceFiles) {
    $matches = @()
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $match = [regex]::Match($line, $namespacePattern)
        if ($match.Success) {
            $matches += $match.Groups[1].Value
        }
    }

    $relative = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
    if ($matches.Count -eq 0) {
        $namespaceRecords.Add([pscustomobject]@{
            File = $relative
            Namespace = "<global namespace>"
        })
    }
    else {
        foreach ($name in $matches) {
            $namespaceRecords.Add([pscustomobject]@{
                File = $relative
                Namespace = $name
            })
        }
    }
}

foreach ($record in ($namespaceRecords | Sort-Object Namespace, File)) {
    Add-ReportLine ("{0,-60} {1}" -f $record.Namespace, $record.File)
}

$globalNamespaceFiles = @(
$namespaceRecords |
        Where-Object { $_.Namespace -eq "<global namespace>" }
)

# -----------------------------------------------------------------------------
# Gate 7 targeted audits
# -----------------------------------------------------------------------------
$stateBoundaryPatterns = @(
    "interface IState",
    "class State",
    "IStateInstruction",
    "StateInstruction",
    "StateOperation",
    "StateChange",
    "UpdateState",
    "AddState",
    "RemoveState",
    "SetState",
    "TryGetState",
    "Receive(",
    "Receiver",
    "Target",
    "StatePackage",
    "QslStatePackage",
    "Payload",
    "Binding"
)
Add-PatternAudit `
    -SectionTitle "GATE 7 STATE BOUNDARY AUDIT" `
    -Patterns $stateBoundaryPatterns `
    -Files $allSourceFiles

$emissionTypeNames = @(
    "IResolvedEmission",
    "IResolvedEmissionVector",
    "INormalizedEmission",
    "INormalizedEmissionVector",
    "IPropagatedEmission",
    "IPropagatedEmissionVector",
    "IEmissionEligibilityResult",
    "IEmissionEligibilityVector",
    "IEmissionNormalizationPolicy",
    "ResolvedEmission",
    "ResolvedEmissionVector",
    "NormalizedEmission",
    "NormalizedEmissionVector",
    "PropagatedEmission",
    "PropagatedEmissionVector",
    "EmissionEligibilityResult",
    "EmissionEligibilityVector",
    "EmissionPropagation",
    "EmissionVectorPropagation",
    "EmissionEligibility",
    "EmissionVectorEligibility",
    "ProportionalEmissionNormalization"
)

Add-Section "GATES 4-6 EMISSION TYPE INVENTORY"
foreach ($name in $emissionTypeNames) {
    $matches = @($declarations | Where-Object { $_.Name -eq $name })
    if ($matches.Count -eq 0) {
        Add-ReportLine ("[MISSING] {0}" -f $name)
    }
    elseif ($matches.Count -eq 1) {
        Add-ReportLine ("[FOUND]   {0,-40} {1}:{2}" -f $name, $matches[0].File, $matches[0].Line)
    }
    else {
        Add-ReportLine ("[DUPLICATE] {0} appears {1} times" -f $name, $matches.Count)
        foreach ($match in $matches) {
            Add-ReportLine ("            {0}:{1}" -f $match.File, $match.Line)
        }
    }
}

$consumerIdentityPatterns = @(
    "object Target",
    "IBaseObject Target",
    "Target { get;",
    "Consumer",
    "ConsumerBindings",
    "ConsumerBinding",
    "QslStatePackage",
    "StatePackage",
    "RuntimeObject",
    "ReferenceEquals(",
    "Payload",
    "Result",
    "Neighbor.Id",
    "Relationship.Neighbor"
)
Add-PatternAudit `
    -SectionTitle "CONSUMER TARGET IDENTITY AND PROVENANCE AUDIT" `
    -Patterns $consumerIdentityPatterns `
    -Files $allSourceFiles

$mutationPatterns = @(
    ".Health",
    ".Curiosity",
    ".Fatigue",
    "UpdateState",
    "RemoveState",
    "SetState",
    "AddState",
    "ReceivePackage",
    "StatePackage",
    "PropertyInfo",
    "SetValue(",
    "TrySet",
    "TryRemove",
    "RemoveProperty"
)
Add-PatternAudit `
    -SectionTitle "CONSUMER STATE MUTATION USAGE AUDIT" `
    -Patterns $mutationPatterns `
    -Files $testFiles

$gate7CandidatePatterns = @(
    "ResolvedStateOperation",
    "IResolvedStateOperation",
    "StateOperationKind",
    "IEmissionStateOperationResolver",
    "EligibilityResult",
    "IsEligible",
    "EvaluatedValue",
    "PropagationDistance",
    "Exact target",
    "state operation"
)
Add-PatternAudit `
    -SectionTitle "GATE 7 EXISTING CANDIDATE AND COLLISION AUDIT" `
    -Patterns $gate7CandidatePatterns `
    -Files $allSourceFiles

$publicSurfacePatterns = @(
    "public interface",
    "public sealed class",
    "public abstract class",
    "public enum",
    "public async",
    "public Task",
    "ExecuteAsync(",
    "Process(",
    "Receive("
)
Add-PatternAudit `
    -SectionTitle "PUBLIC SURFACE AUDIT" `
    -Patterns $publicSurfacePatterns `
    -Files @($coreFiles + $qslFiles)

# -----------------------------------------------------------------------------
# Existing architecture audits retained from the full crawler
# -----------------------------------------------------------------------------
$requiredTypes = @(
    "IBaseObject", "IComponent", "IContext", "ISignal", "IState",
    "IBus", "ITaskBus", "IStateInstruction", "ITask", "IRegistry",
    "ITaskRegistry", "IInstructionsProcessor", "ITaskProcessor",
    "IObjectPool", "IResettablePoolObject", "BaseObject", "ProcessorBase",
    "RegistryBase", "TaskRegistryBase", "InstructionsRegistryBase",
    "TaskProcessor", "InstructionsProcessor", "DefaultTaskRegistry",
    "DefaultInstructionsRegistry", "ObjectLease", "ObjectPool",
    "WildsEngine", "WildsEngineOptions"
)

Add-Section "REQUIRED TYPE AUDIT"
foreach ($requiredType in $requiredTypes) {
    $matches = @($declarations | Where-Object { $_.Name -eq $requiredType })
    if ($matches.Count -eq 0) {
        Add-ReportLine ("[MISSING] {0}" -f $requiredType)
    }
    elseif ($matches.Count -eq 1) {
        Add-ReportLine ("[FOUND]   {0,-35} {1}:{2}" -f $requiredType, $matches[0].File, $matches[0].Line)
    }
    else {
        Add-ReportLine ("[DUPLICATE] {0} appears {1} times" -f $requiredType, $matches.Count)
        foreach ($match in $matches) {
            Add-ReportLine ("            {0}:{1}" -f $match.File, $match.Line)
        }
    }
}

$registryPatterns = @(
    "interface IRegistry", "IInstructionRegistry", "IInstructionsRegistry",
    "ITaskRegistry", "class RegistryBase", "TaskRegistryBase",
    "InstructionsRegistryBase", "DefaultTaskRegistry", "DefaultInstructionsRegistry",
    "ConcurrentDictionary", "Dictionary<", "Register(", "Contains(",
    "TryGet(", "Get(", "Remove(", "Clear(", "Count", "Items"
)
Add-PatternAudit `
    -SectionTitle "REGISTRY CONTRACT AND STORAGE AUDIT" `
    -Patterns $registryPatterns `
    -Files $coreFiles

$processingPatterns = @(
    "class ProcessorBase", "class InstructionsProcessor", "class TaskProcessor",
    "IObjectPool<ITaskProcessor>", "IObjectPool<IInstructionsProcessor>", "ObjectLease<",
    "Rent()", "RentAsync(", "Return(", "TryReset(", "OnReset(",
    "ExecuteTaskAsync", "ExecuteInstructionAsync", "MaximumRetained",
    "MaximumConcurrent", "AvailableCount", "ActiveCount", "CreatedCount",
    "SemaphoreSlim", "ConcurrentQueue"
)
Add-PatternAudit `
    -SectionTitle "PROCESSOR AND POOLING AUDIT" `
    -Patterns $processingPatterns `
    -Files $coreFiles

$incompletePatterns = @(
    "NotImplementedException", "NotSupportedException", "TODO", "FIXME", "HACK",
    "null!", "throw new Exception", "return default", "return null", "NotImplemented"
)
Add-PatternAudit `
    -SectionTitle "PLACEHOLDER AND INCOMPLETE IMPLEMENTATION AUDIT" `
    -Patterns $incompletePatterns `
    -Files $allSourceFiles

Add-Section "COMPONENT IMPLEMENTATION AUDIT"
$componentMatches = @(
foreach ($file in $coreFiles) {
    Select-String -LiteralPath $file.FullName -Pattern "IComponent\s+Component" |
            ForEach-Object {
                [pscustomobject]@{
                    File = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
                    Line = $_.LineNumber
                    Text = $_.Line.Trim()
                }
            }
}
)

if ($componentMatches.Count -eq 0) {
    Add-ReportLine "[MISSING] No Component property declarations found."
}
else {
    foreach ($match in $componentMatches) {
        Add-ReportLine ("{0}:{1}: {2}" -f $match.File, $match.Line, $match.Text)
    }
}

Add-Section "SOURCE STRUCTURE AUDIT"
$sourceStructureIssueCount = 0
foreach ($file in $allSourceFiles) {
    $namespaceSeen = $false
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNumber++
        if ($line -match '^\s*namespace\s+') {
            $namespaceSeen = $true
            continue
        }

        if ($namespaceSeen -and $line -match '^\s*using\s+[A-Za-z_]') {
            $sourceStructureIssueCount++
            Add-ReportLine (
            "[REVIEW] using directive after namespace declaration: {0}:{1}: {2}" -f
            (Get-RelativePath -BasePath $Root -TargetPath $file.FullName),
            $lineNumber,
            $line.Trim()
            )
        }
    }
}

if ($sourceStructureIssueCount -eq 0) {
    Add-ReportLine "[PASS] No suspicious using placement found."
}

# -----------------------------------------------------------------------------
# Complete captures
# -----------------------------------------------------------------------------
Add-CompleteFileCapture -SectionTitle "COMPLETE C# SOURCE CAPTURE" -Files $allSourceFiles
Add-CompleteFileCapture -SectionTitle "COMPLETE PROJECT FILE CAPTURE" -Files $projectFiles
Add-CompleteFileCapture -SectionTitle "ARCHITECTURE AND CONFIGURATION CAPTURE" -Files $architectureFiles

# -----------------------------------------------------------------------------
# Optional build and tests
# -----------------------------------------------------------------------------
Add-Section "BUILD AUDIT"
if ($RunBuild) {
    Add-CommandOutput -Title "dotnet build" -Command {
        Push-Location $Root
        try {
            dotnet build
        }
        finally {
            Pop-Location
        }
    }
}
else {
    Add-ReportLine "Build not requested. Run with -RunBuild to include build output."
}

Add-Section "TEST AUDIT"
if ($RunTests) {
    if (Test-Path -LiteralPath $testsRoot) {
        Add-CommandOutput -Title "dotnet test" -Command {
            Push-Location $Root
            try {
                dotnet test .\WildsEngine.Core.Tests\WildsEngine.Core.Tests.csproj -c Debug --logger "console;verbosity=normal"
            }
            finally {
                Pop-Location
            }
        }
    }
    else {
        Add-ReportLine "[MISSING] Test project directory not found: $testsRoot"
    }
}
else {
    Add-ReportLine "Tests not requested. Run with -RunTests to include normal test output."
}

if ($RunSerializedTests) {
    if (Test-Path -LiteralPath $testsRoot) {
        Add-CommandOutput -Title "dotnet test - serialized xUnit execution" -Command {
            Push-Location $Root
            try {
                dotnet test .\WildsEngine.Core.Tests\WildsEngine.Core.Tests.csproj -c Debug --no-build -- -parallel none
            }
            finally {
                Pop-Location
            }
        }
    }
    else {
        Add-ReportLine "[MISSING] Test project directory not found: $testsRoot"
    }
}
else {
    Add-ReportLine "Serialized tests not requested. Run with -RunSerializedTests to include them."
}

# -----------------------------------------------------------------------------
# Summary and output
# -----------------------------------------------------------------------------
Add-Section "AUDIT SUMMARY"
Add-ReportLine "Core files: $($coreFiles.Count)"
Add-ReportLine "Wilds/QSL files: $($qslFiles.Count)"
Add-ReportLine "Test files: $($testFiles.Count)"
Add-ReportLine "Benchmark/console files: $($benchmarkFiles.Count)"
Add-ReportLine "Declarations: $($declarations.Count)"
Add-ReportLine "Duplicate declaration groups: $($duplicateDeclarations.Count)"
Add-ReportLine "Global namespace files: $($globalNamespaceFiles.Count)"
Add-ReportLine "Source structure issues: $sourceStructureIssueCount"
Add-ReportLine "Build requested: $RunBuild"
Add-ReportLine "Tests requested: $RunTests"
Add-ReportLine "Serialized tests requested: $RunSerializedTests"
Add-ReportLine "Report: $reportPath"

$reportText = $report -join [Environment]::NewLine
$utf8 = [System.Text.UTF8Encoding]::new($false)
[System.IO.File]::WriteAllText($reportPath, $reportText, $utf8)
[System.IO.File]::WriteAllText($latestReportPath, $reportText, $utf8)

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " WildsEngine Gate 7 Crawl Complete" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host ""
Write-Status -Label "REPORT" -Message $reportPath -Color Green
Write-Status -Label "LATEST" -Message $latestReportPath -Color Green
Write-Status -Label "CORE" -Message "$($coreFiles.Count) files" -Color Cyan
Write-Status -Label "QSL" -Message "$($qslFiles.Count) files" -Color Cyan
Write-Status -Label "TESTS" -Message "$($testFiles.Count) files" -Color Cyan
Write-Status -Label "TYPES" -Message "$($declarations.Count) declarations" -Color Cyan

if ($duplicateDeclarations.Count -gt 0) {
    Write-Status -Label "REVIEW" -Message "$($duplicateDeclarations.Count) duplicate declaration groups" -Color Yellow
}
else {
    Write-Status -Label "PASS" -Message "No duplicate declaration groups" -Color Green
}

if ($globalNamespaceFiles.Count -gt 0) {
    Write-Status -Label "REVIEW" -Message "$($globalNamespaceFiles.Count) global-namespace files" -Color Yellow
}
else {
    Write-Status -Label "PASS" -Message "No global-namespace files" -Color Green
}

Write-Host ""
