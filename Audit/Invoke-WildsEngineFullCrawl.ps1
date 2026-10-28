param(
    [string]$Root = "",
    [string]$OutputDirectory = "",
    [switch]$RunBuild,
    [switch]$RunTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Resolve repository root
if ([string]::IsNullOrWhiteSpace($Root)) {
    $scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
    $Root = Split-Path -Parent $scriptDirectory
}
$Root = [System.IO.Path]::GetFullPath($Root)
if (-not (Test-Path -LiteralPath $Root)) { throw "WildsEngine root does not exist: $Root" }

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $Root "Audit"
}
if (-not (Test-Path -LiteralPath $OutputDirectory)) {
    New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$reportPath = Join-Path $OutputDirectory "WildsEngine-Full-Crawl-$timestamp.txt"
$latestReportPath = Join-Path $OutputDirectory "WildsEngine-Full-Crawl-Latest.txt"
$coreRoot = Join-Path $Root "WildsEngine.Core"
$testsRoot = Join-Path $Root "WildsEngine.Core.Tests"
$benchmarkRootCandidates = @(
    (Join-Path $Root "WildsEngine.Benchmarks"),
    (Join-Path $Root "WildsEngine.ConsoleTests"),
    (Join-Path $Root "Benchmarks")
)
$excludedDirectoryNames = @(".git", ".vs", ".idea", "bin", "obj", "packages", "TestResults")

$report = New-Object System.Collections.Generic.List[string]
function Add-ReportLine { param([string]$Text = "") $script:report.Add($Text) }
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
    $baseUri = New-Object System.Uri (($resolvedBase.TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)) + [System.IO.Path]::DirectorySeparatorChar)
    $targetUri = New-Object System.Uri $resolvedTarget
    $relativeUri = $baseUri.MakeRelativeUri($targetUri)
    return [System.Uri]::UnescapeDataString($relativeUri.ToString()).Replace("/", [System.IO.Path]::DirectorySeparatorChar)
}
function Test-IsExcludedPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    foreach ($segment in ($Path -split "[\\/]")) {
        if ($excludedDirectoryNames -contains $segment) { return $true }
    }
    return $false
}
function Get-CSharpFiles {
    param([Parameter(Mandatory = $true)][string]$SearchRoot)
    if (-not (Test-Path -LiteralPath $SearchRoot)) { return @() }
    return @(Get-ChildItem -LiteralPath $SearchRoot -Filter "*.cs" -File -Recurse |
            Where-Object { -not (Test-IsExcludedPath -Path $_.FullName) } |
            Sort-Object FullName)
}
function Get-ProjectFiles {
    param([Parameter(Mandatory = $true)][string]$SearchRoot)
    if (-not (Test-Path -LiteralPath $SearchRoot)) { return @() }
    return @(Get-ChildItem -LiteralPath $SearchRoot -Filter "*.csproj" -File -Recurse |
            Where-Object { -not (Test-IsExcludedPath -Path $_.FullName) } |
            Sort-Object FullName)
}
function Add-CommandOutput {
    param(
        [Parameter(Mandatory = $true)][string]$Title,
        [Parameter(Mandatory = $true)][scriptblock]$Command
    )
    Add-Subsection $Title
    try {
        $output = & $Command 2>&1 | Out-String
        if ([string]::IsNullOrWhiteSpace($output)) { Add-ReportLine "<no output>" }
        else { foreach ($line in ($output -split "`r?`n")) { Add-ReportLine $line } }
    }
    catch {
        Add-ReportLine ("[ERROR] {0}: {1}" -f $_.Exception.GetType().Name, $_.Exception.Message)
    }
}
function Add-PatternAudit {
    param(
        [Parameter(Mandatory = $true)][string]$SectionTitle,
        [Parameter(Mandatory = $true)][string[]]$Patterns,
        [Parameter(Mandatory = $true)][System.IO.FileInfo[]]$Files
    )
    Add-Section $SectionTitle
    foreach ($pattern in $Patterns) {
        Add-Subsection "Pattern: $pattern"
        $matchesFound = 0
        foreach ($file in $Files) {
            $lineMatches = Select-String -LiteralPath $file.FullName -SimpleMatch -Pattern $pattern
            foreach ($lineMatch in $lineMatches) {
                $matchesFound++
                Add-ReportLine ("{0}:{1}: {2}" -f (Get-RelativePath -BasePath $Root -TargetPath $file.FullName), $lineMatch.LineNumber, $lineMatch.Line.Trim())
            }
        }
        if ($matchesFound -eq 0) { Add-ReportLine "<no matches>" }
    }
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " WildsEngine Full Crawler Audit" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host ""
Write-Status -Label "ROOT" -Message $Root -Color Cyan

$coreFiles = @(Get-CSharpFiles -SearchRoot $coreRoot)
$testFiles = @(Get-CSharpFiles -SearchRoot $testsRoot)
$benchmarkFiles = New-Object System.Collections.Generic.List[System.IO.FileInfo]
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
$allSourceFiles = @(Get-CSharpFiles -SearchRoot $Root)
$projectFiles = @(Get-ProjectFiles -SearchRoot $Root)

Write-Status -Label "FOUND" -Message "$($coreFiles.Count) core C# files" -Color Green
Write-Status -Label "FOUND" -Message "$($testFiles.Count) test C# files" -Color Green
Write-Status -Label "FOUND" -Message "$($benchmarkFiles.Count) benchmark/console C# files" -Color Green
Write-Status -Label "FOUND" -Message "$($projectFiles.Count) project files" -Color Green

Add-Section "WILDSENGINE FULL CRAWLER AUDIT"
Add-ReportLine "Root: $Root"
Add-ReportLine "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss K')"
Add-ReportLine "Core: $coreRoot"
Add-ReportLine "Tests: $testsRoot"
Add-ReportLine "Core C# files: $($coreFiles.Count)"
Add-ReportLine "Test C# files: $($testFiles.Count)"
Add-ReportLine "Benchmark C# files: $($benchmarkFiles.Count)"
Add-ReportLine "All repository C# files: $($allSourceFiles.Count)"
Add-ReportLine "Project files: $($projectFiles.Count)"

Add-Section "REPOSITORY TREE"
Add-CommandOutput -Title "Repository tree" -Command {
    Push-Location $Root
    try { tree /f /a }
    finally { Pop-Location }
}

Add-Section "PROJECT INVENTORY"
foreach ($project in $projectFiles) {
    Add-ReportLine (Get-RelativePath -BasePath $Root -TargetPath $project.FullName)
}

Add-Section "SOURCE INVENTORY"
$inventoryGroups = @(
    @{ Name = "Abstractions"; Path = Join-Path $coreRoot "Abstractions" },
    @{ Name = "Objects"; Path = Join-Path $coreRoot "Objects" },
    @{ Name = "Processing"; Path = Join-Path $coreRoot "Processing" },
    @{ Name = "Pooling"; Path = Join-Path $coreRoot "Pooling" },
    @{ Name = "Signals"; Path = Join-Path $coreRoot "Signals" },
    @{ Name = "Core root"; Path = $coreRoot; RootOnly = $true },
    @{ Name = "Tests"; Path = $testsRoot }
)
foreach ($group in $inventoryGroups) {
    Add-Subsection $group.Name
    if (-not (Test-Path -LiteralPath $group.Path)) { Add-ReportLine "<directory not found>"; continue }
    if ($group.ContainsKey("RootOnly") -and $group.RootOnly) {
        $files = @(Get-ChildItem -LiteralPath $group.Path -Filter "*.cs" -File | Sort-Object Name)
    }
    else { $files = @(Get-CSharpFiles -SearchRoot $group.Path) }
    if ($files.Count -eq 0) { Add-ReportLine "<no C# files>"; continue }
    foreach ($file in $files) { Add-ReportLine (Get-RelativePath -BasePath $Root -TargetPath $file.FullName) }
}

Add-Section "DECLARATION INVENTORY"
$declarationPattern = '^\s*(?:public|internal|protected|private)?\s*(?:sealed\s+|abstract\s+|static\s+|partial\s+)*(class|interface|struct|record|enum)\s+([A-Za-z_][A-Za-z0-9_]*)'
$declarations = New-Object System.Collections.Generic.List[object]
foreach ($file in $allSourceFiles) {
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($file.FullName)) {
        $lineNumber++
        $match = [regex]::Match($line, $declarationPattern)
        if (-not $match.Success) { continue }
        $declarations.Add([pscustomobject]@{
            Kind = $match.Groups[1].Value
            Name = $match.Groups[2].Value
            File = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
            Line = $lineNumber
            Declaration = $line.Trim()
        })
    }
}
foreach ($declaration in ($declarations | Sort-Object Kind, Name, File, Line)) {
    Add-ReportLine ("{0,-10} {1,-40} {2}:{3}" -f $declaration.Kind, $declaration.Name, $declaration.File, $declaration.Line)
}

Add-Section "DUPLICATE TYPE DECLARATION AUDIT"
$duplicateDeclarations = @($declarations | Group-Object Name | Where-Object { $_.Count -gt 1 } | Sort-Object Name)
if ($duplicateDeclarations.Count -eq 0) { Add-ReportLine "[PASS] No duplicate type names found." }
else {
    foreach ($duplicate in $duplicateDeclarations) {
        Add-ReportLine ""
        Add-ReportLine ("[REVIEW] {0} appears {1} times" -f $duplicate.Name, $duplicate.Count)
        foreach ($entry in $duplicate.Group) { Add-ReportLine ("    {0} at {1}:{2}" -f $entry.Kind, $entry.File, $entry.Line) }
    }
}

Add-Section "NAMESPACE AUDIT"
$namespacePattern = '^\s*namespace\s+([A-Za-z_][A-Za-z0-9_.]*)'
$namespaceRecords = New-Object System.Collections.Generic.List[object]
foreach ($file in $allSourceFiles) {
    $namespaceMatches = @()
    foreach ($line in (Get-Content -LiteralPath $file.FullName)) {
        $match = [regex]::Match($line, $namespacePattern)
        if ($match.Success) { $namespaceMatches += $match.Groups[1].Value }
    }
    $relative = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
    if ($namespaceMatches.Count -eq 0) {
        $namespaceRecords.Add([pscustomobject]@{ File = $relative; Namespace = "<global namespace>" })
    }
    else {
        foreach ($namespaceName in $namespaceMatches) {
            $namespaceRecords.Add([pscustomobject]@{ File = $relative; Namespace = $namespaceName })
        }
    }
}
foreach ($record in ($namespaceRecords | Sort-Object Namespace, File)) {
    Add-ReportLine ("{0,-55} {1}" -f $record.Namespace, $record.File)
}
$globalNamespaceFiles = @($namespaceRecords | Where-Object { $_.Namespace -eq "<global namespace>" })
Add-Subsection "Global namespace files"
if ($globalNamespaceFiles.Count -eq 0) { Add-ReportLine "[PASS] No C# files use the global namespace." }
else { foreach ($record in $globalNamespaceFiles) { Add-ReportLine ("[REVIEW] {0}" -f $record.File) } }

Add-Section "REQUIRED TYPE AUDIT"
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
foreach ($requiredType in $requiredTypes) {
    $matches = @($declarations | Where-Object { $_.Name -eq $requiredType })
    if ($matches.Count -eq 0) { Add-ReportLine ("[MISSING] {0}" -f $requiredType) }
    elseif ($matches.Count -eq 1) { Add-ReportLine ("[FOUND]   {0,-35} {1}:{2}" -f $requiredType, $matches[0].File, $matches[0].Line) }
    else {
        Add-ReportLine ("[DUPLICATE] {0} appears {1} times" -f $requiredType, $matches.Count)
        foreach ($match in $matches) { Add-ReportLine ("            {0}:{1}" -f $match.File, $match.Line) }
    }
}

Add-Section "INSTRUCTION NAMING AUDIT"
$instructionNamingGroups = @(
    @{ Label = "Registry interface"; Names = @("IInstructionRegistry", "IInstructionsRegistry") },
    @{ Label = "Processor interface"; Names = @("IInstructionProcessor", "IInstructionsProcessor") },
    @{ Label = "Processor class"; Names = @("InstructionProcessor", "InstructionsProcessor") },
    @{ Label = "Registry base"; Names = @("InstructionRegistryBase", "InstructionsRegistryBase") },
    @{ Label = "Default registry"; Names = @("DefaultInstructionRegistry", "DefaultInstructionsRegistry") }
)
foreach ($group in $instructionNamingGroups) {
    $present = @()
    foreach ($name in $group.Names) {
        if (@($declarations | Where-Object { $_.Name -eq $name }).Count -gt 0) { $present += $name }
    }
    if ($present.Count -eq 0) { Add-ReportLine ("[MISSING] {0}: neither naming variant exists." -f $group.Label) }
    elseif ($present.Count -eq 1) { Add-ReportLine ("[INFO] {0}: using {1}." -f $group.Label, $present[0]) }
    else { Add-ReportLine ("[REVIEW] {0}: both variants exist: {1}." -f $group.Label, ($present -join ", ")) }
}

$registryPatterns = @(
    "interface IRegistry", "interface IInstructionRegistry", "interface IInstructionsRegistry",
    "interface ITaskRegistry", "class RegistryBase", "class TaskRegistryBase",
    "class InstructionsRegistryBase", "class DefaultTaskRegistry", "class DefaultInstructionsRegistry",
    "ConcurrentDictionary", "Dictionary<", "Func<T, Guid>", "Register(", "Contains(",
    "TryGet(", "Get(", "Remove(", "Clear(", "Count", "Items", "Initialize(",
    "Shutdown(", "OnInitialize(", "OnShutdown("
)
Add-PatternAudit -SectionTitle "REGISTRY CONTRACT AND STORAGE AUDIT" -Patterns $registryPatterns -Files $coreFiles

Add-Section "REGISTRY STORAGE SHAPE AUDIT"
$registryBaseCandidates = @($coreFiles | Where-Object { $_.Name -eq "RegistryBase.cs" })
if ($registryBaseCandidates.Count -eq 0) {
    Add-ReportLine "[MISSING] RegistryBase.cs"
}
else {
    foreach ($file in $registryBaseCandidates) {
        $relative = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
        $text = [System.IO.File]::ReadAllText($file.FullName)
        Add-ReportLine "Registry base: $relative"
        Add-ReportLine $(if ($text -match 'ConcurrentDictionary\s*<\s*Guid\s*,') { "[PASS] GUID-keyed ConcurrentDictionary storage detected." } else { "[REVIEW] GUID-keyed ConcurrentDictionary storage not detected." })
        Add-ReportLine $(if ($text -match 'Func\s*<\s*T\s*,\s*Guid\s*>') { "[PASS] Generic identity selector detected." } else { "[INFO] Generic identity selector not detected." })
        foreach ($method in @("Register", "Contains", "Get", "TryGet", "Remove", "Clear")) {
            Add-ReportLine $(if ($text -match ("\b" + [regex]::Escape($method) + "\s*\(")) { "[PASS] $method present." } else { "[MISSING] $method not found." })
        }
    }
}

$processingPatterns = @(
    "class ProcessorBase", "class InstructionsProcessor", "class TaskProcessor",
    "IObjectPool<ITaskProcessor>", "IObjectPool<IInstructionsProcessor>", "ObjectLease<",
    "Rent()", "RentAsync(", "Return(", "TryReset(", "OnReset(", "ExecuteTaskAsync",
    "ExecuteInstructionAsync", "MaximumRetained", "MaximumConcurrent", "AvailableCount",
    "ActiveCount", "CreatedCount", "SemaphoreSlim", "ConcurrentQueue"
)
Add-PatternAudit -SectionTitle "PROCESSOR AND POOLING AUDIT" -Patterns $processingPatterns -Files $coreFiles

Add-Section "COMPONENT IMPLEMENTATION AUDIT"
$componentMatches = @(
foreach ($file in $coreFiles) {
    Select-String -LiteralPath $file.FullName -Pattern "IComponent\s+Component" | ForEach-Object {
        [pscustomobject]@{
            File = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
            Line = $_.LineNumber
            Text = $_.Line.Trim()
        }
    }
}
)
if ($componentMatches.Count -eq 0) { Add-ReportLine "[MISSING] No Component property declarations found." }
else { foreach ($match in $componentMatches) { Add-ReportLine ("{0}:{1}: {2}" -f $match.File, $match.Line, $match.Text) } }

Add-Subsection "Potential uninitialized getter-only Component properties"
$uninitializedComponentPattern = 'public\s+(?:abstract\s+|override\s+|virtual\s+)*IComponent\s+Component\s*\{\s*get;\s*\}'
$uninitializedComponentMatches = @(
foreach ($file in $coreFiles) {
    Select-String -LiteralPath $file.FullName -Pattern $uninitializedComponentPattern | ForEach-Object {
        [pscustomobject]@{
            File = Get-RelativePath -BasePath $Root -TargetPath $file.FullName
            Line = $_.LineNumber
            Text = $_.Line.Trim()
        }
    }
}
)
if ($uninitializedComponentMatches.Count -eq 0) { Add-ReportLine "<none found>" }
else { foreach ($match in $uninitializedComponentMatches) { Add-ReportLine ("[REVIEW] {0}:{1}: {2}" -f $match.File, $match.Line, $match.Text) } }

$incompletePatterns = @("NotImplementedException", "NotSupportedException", "TODO", "FIXME", "HACK", "null!", "throw new Exception", "return default", "return null", "NotImplemented")
Add-PatternAudit -SectionTitle "PLACEHOLDER AND INCOMPLETE IMPLEMENTATION AUDIT" -Patterns $incompletePatterns -Files $allSourceFiles

Add-Section "SOURCE STRUCTURE AUDIT"
$sourceStructureIssueCount = 0
foreach ($file in $allSourceFiles) {
    $content = Get-Content -LiteralPath $file.FullName
    $namespaceSeen = $false
    $namespaceDepth = 0
    $lineNumber = 0
    foreach ($line in $content) {
        $lineNumber++
        if ($line -match '^\s*namespace\s+') { $namespaceSeen = $true; continue }
        if ($namespaceSeen -and $line -match '^\s*using\s+[A-Za-z_]') {
            $sourceStructureIssueCount++
            Add-ReportLine ("[REVIEW] using directive after namespace declaration: {0}:{1}: {2}" -f (Get-RelativePath -BasePath $Root -TargetPath $file.FullName), $lineNumber, $line.Trim())
        }
    }
}
if ($sourceStructureIssueCount -eq 0) { Add-ReportLine "[PASS] No suspicious using placement found." }

Add-Section "COMPLETE SOURCE CAPTURE"
foreach ($file in $allSourceFiles) {
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

Add-Section "BUILD AUDIT"
if ($RunBuild) {
    Add-CommandOutput -Title "dotnet build" -Command {
        Push-Location $Root
        try { dotnet build }
        finally { Pop-Location }
    }
}
else { Add-ReportLine "Build not requested. Run with -RunBuild to include build output." }

Add-Section "TEST AUDIT"
if ($RunTests) {
    if (Test-Path -LiteralPath $testsRoot) {
        Add-CommandOutput -Title "dotnet test" -Command {
            Push-Location $testsRoot
            try { dotnet test --logger "console;verbosity=normal" }
            finally { Pop-Location }
        }
    }
    else { Add-ReportLine "[MISSING] Test project directory not found: $testsRoot" }
}
else { Add-ReportLine "Tests not requested. Run with -RunTests to include test output." }

Add-Section "AUDIT SUMMARY"
Add-ReportLine "Core files: $($coreFiles.Count)"
Add-ReportLine "Test files: $($testFiles.Count)"
Add-ReportLine "Benchmark/console files: $($benchmarkFiles.Count)"
Add-ReportLine "Declarations: $($declarations.Count)"
Add-ReportLine "Duplicate declaration groups: $($duplicateDeclarations.Count)"
Add-ReportLine "Global namespace files: $($globalNamespaceFiles.Count)"
Add-ReportLine "Potential uninitialized Component properties: $($uninitializedComponentMatches.Count)"
Add-ReportLine "Source structure issues: $sourceStructureIssueCount"
Add-ReportLine "Build requested: $RunBuild"
Add-ReportLine "Tests requested: $RunTests"
Add-ReportLine "Report: $reportPath"

$reportText = $report -join [Environment]::NewLine
[System.IO.File]::WriteAllText($reportPath, $reportText, [System.Text.UTF8Encoding]::new($false))
[System.IO.File]::WriteAllText($latestReportPath, $reportText, [System.Text.UTF8Encoding]::new($false))

Write-Host ""
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host " WildsEngine Full Crawl Complete" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor DarkGray
Write-Host ""
Write-Status -Label "REPORT" -Message $reportPath -Color Green
Write-Status -Label "LATEST" -Message $latestReportPath -Color Green
Write-Status -Label "CORE" -Message "$($coreFiles.Count) files" -Color Cyan
Write-Status -Label "TESTS" -Message "$($testFiles.Count) files" -Color Cyan
Write-Status -Label "TYPES" -Message "$($declarations.Count) declarations" -Color Cyan

if ($duplicateDeclarations.Count -gt 0) { Write-Status -Label "REVIEW" -Message "$($duplicateDeclarations.Count) duplicate declaration groups" -Color Yellow }
else { Write-Status -Label "PASS" -Message "No duplicate declaration groups" -Color Green }

if ($globalNamespaceFiles.Count -gt 0) { Write-Status -Label "REVIEW" -Message "$($globalNamespaceFiles.Count) global-namespace files" -Color Yellow }
else { Write-Status -Label "PASS" -Message "No global-namespace files" -Color Green }

if ($uninitializedComponentMatches.Count -gt 0) { Write-Status -Label "REVIEW" -Message "$($uninitializedComponentMatches.Count) getter-only Component properties" -Color Yellow }
else { Write-Status -Label "PASS" -Message "No getter-only Component properties detected" -Color Green }

Write-Host ""
