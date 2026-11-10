param(
    [switch]$SkipBuild,
    [switch]$IncludeCoreSource
)

$ErrorActionPreference = "Stop"

$EntitiesRoot = Split-Path -Parent $PSScriptRoot
$RepoRoot = Split-Path -Parent $EntitiesRoot
$CoreRoot = Join-Path $RepoRoot "WildsEngine.Core"
$QslRoot = Join-Path $RepoRoot "WildsEngine.Qsl"
$AuditRoot = $PSScriptRoot

$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$ReportPath = Join-Path $AuditRoot "WildsEngine-Entities-Audit-$Timestamp.txt"
$LatestPath = Join-Path $AuditRoot "WildsEngine-Entities-Audit-Latest.txt"

$Builder = New-Object System.Text.StringBuilder

function Write-ReportLine {
    param([string]$Text = "")
    [void]$Builder.AppendLine($Text)
}

function Write-Section {
    param([string]$Title)
    Write-ReportLine
    Write-ReportLine ("=" * 72)
    Write-ReportLine $Title
    Write-ReportLine ("=" * 72)
}

function Get-SourceFiles {
    param([string]$Root)
    if (-not (Test-Path $Root)) { return @() }
    return @(
    Get-ChildItem -Path $Root -Recurse -File -Filter "*.cs" |
            Where-Object {
                $_.FullName -notmatch "\\bin\\" -and
                        $_.FullName -notmatch "\\obj\\" -and
                        $_.FullName -notmatch "\\Audits\\"
            } |
            Sort-Object FullName
    )
}

function Get-RelativePath {
    param([string]$Path)

    # Windows PowerShell 5.1 compatible.
    # System.IO.Path.GetRelativePath is not available on the .NET Framework
    # runtime used by Windows PowerShell 5.1.
    $Base = $RepoRoot.TrimEnd('\')
    $Target = $Path

    if ($Target.StartsWith(
            $Base + '\',
            [System.StringComparison]::OrdinalIgnoreCase))
    {
        return $Target.Substring($Base.Length + 1)
    }

    if ([string]::Equals(
            $Target,
            $Base,
            [System.StringComparison]::OrdinalIgnoreCase))
    {
        return '.'
    }

    # Fallback for a path outside the repository. Keep the absolute path
    # rather than failing the audit.
    return $Target
}

function Write-SourceFile {
    param([System.IO.FileInfo]$File)
    $Relative = Get-RelativePath $File.FullName
    Write-ReportLine
    Write-ReportLine ("#" * 72)
    Write-ReportLine "FILE: $Relative"
    Write-ReportLine ("#" * 72)
    $LineNumber = 0
    foreach ($Line in Get-Content $File.FullName) {
        $LineNumber++
        Write-ReportLine ("{0,5}: {1}" -f $LineNumber, $Line)
    }
}

function Write-PatternAudit {
    param(
        [string]$Title,
        [string[]]$Patterns,
        [System.IO.FileInfo[]]$Files
    )
    Write-Section $Title
    foreach ($Pattern in $Patterns) {
        Write-ReportLine
        Write-ReportLine "Pattern: $Pattern"
        Write-ReportLine ("-" * 72)
        $Matches = @($Files | Select-String -Pattern $Pattern -CaseSensitive:$false)
        if ($Matches.Count -eq 0) {
            Write-ReportLine "<no matches>"
            continue
        }
        foreach ($Match in $Matches) {
            Write-ReportLine ("{0}:{1}: {2}" -f (Get-RelativePath $Match.Path), $Match.LineNumber, $Match.Line.Trim())
        }
    }
}

function Write-TypeAudit {
    param([System.IO.FileInfo[]]$Files)
    Write-Section "TYPE DECLARATION AUDIT"
    $Declarations = @()
    foreach ($File in $Files) {
        $Matches = Select-String -Path $File.FullName -Pattern "\b(class|interface|struct|record)\s+([A-Za-z_][A-Za-z0-9_]*)" -AllMatches
        foreach ($Match in $Matches) {
            foreach ($Token in $Match.Matches) {
                $Declarations += [PSCustomObject]@{
                    Type = $Token.Groups[1].Value
                    Name = $Token.Groups[2].Value
                    File = Get-RelativePath $File.FullName
                    Line = $Match.LineNumber
                }
            }
        }
    }
    foreach ($Declaration in $Declarations | Sort-Object Name, File) {
        Write-ReportLine ("{0,-10} {1,-40} {2}:{3}" -f $Declaration.Type, $Declaration.Name, $Declaration.File, $Declaration.Line)
    }
    Write-Section "DUPLICATE TYPE DECLARATIONS"
    $Duplicates = @($Declarations | Group-Object Name | Where-Object Count -gt 1 | Sort-Object Name)
    if ($Duplicates.Count -eq 0) {
        Write-ReportLine "[PASS] No duplicate type names found."
        return
    }
    foreach ($Duplicate in $Duplicates) {
        Write-ReportLine
        Write-ReportLine ("[REVIEW] {0} appears {1} times" -f $Duplicate.Name, $Duplicate.Count)
        foreach ($Item in $Duplicate.Group) {
            Write-ReportLine ("    {0}:{1}" -f $Item.File, $Item.Line)
        }
    }
}

Write-Section "WILDSENGINE CORE.ENTITIES AUDIT"
Write-ReportLine "Generated: $(Get-Date -Format o)"
Write-ReportLine "Repository: $RepoRoot"
Write-ReportLine "Entities:   $EntitiesRoot"
Write-ReportLine "Core:       $CoreRoot"
Write-ReportLine "QSL:        $QslRoot"

$EntityFiles = Get-SourceFiles $EntitiesRoot
$CoreFiles = Get-SourceFiles $CoreRoot

Write-ReportLine
Write-ReportLine "Entities C# files: $($EntityFiles.Count)"
Write-ReportLine "Core C# files:     $($CoreFiles.Count)"

$ProjectFile = Join-Path $EntitiesRoot "WildsEngine.Core.Entities.csproj"
Write-Section "PROJECT FILE"
if (Test-Path $ProjectFile) {
    foreach ($Line in Get-Content $ProjectFile) { Write-ReportLine $Line }
} else {
    Write-ReportLine "[MISSING] WildsEngine.Core.Entities.csproj"
}

Write-Section "PROJECT REFERENCES"
if (Test-Path $ProjectFile) {
    $References = @(Select-String -Path $ProjectFile -Pattern "ProjectReference|PackageReference")
    if ($References.Count -eq 0) {
        Write-ReportLine "<no project/package references>"
    } else {
        foreach ($Reference in $References) { Write-ReportLine $Reference.Line.Trim() }
    }
}

Write-Section "ENTITIES SOURCE TREE"
foreach ($File in $EntityFiles) { Write-ReportLine (Get-RelativePath $File.FullName) }

Write-PatternAudit -Title "ENTITY CONTRACT AUDIT" -Patterns @(
    "\bIIdentifier\b", "\bIEntity\b", "\bIFact\b", "\bIDiscovery\b", "\bIWildsDiscovery\b",
    "\bIBaseObject\b", "\bBaseObject\b"
) -Files $EntityFiles

Write-PatternAudit -Title "CORE FUNDAMENTAL USAGE AUDIT" -Patterns @(
    "\bIComponent\b", "\bIState\b", "\bIContext\b", "\bIStateInstruction\b", "\bIRegistry\b",
    "\bRegistryBase\b", "\bINeighborCollection\b", "\bINeighborOperator\b"
) -Files $EntityFiles

Write-PatternAudit -Title "BOUNDARY LEAK AUDIT" -Patterns @(
    "\bQSL\.", "\bQslEngine\b", "\bQslRuntimeContext\b", "\bQslConsumerBinding\b", "\bQslStatePackage\b",
    "\bWildsQslRequest\b", "\bWildsQslExecutor\b", "\bWildsEngine\.Qsl\b"
) -Files $EntityFiles

Write-PatternAudit -Title "IMPLEMENTATION RISK AUDIT" -Patterns @(
    "TODO", "FIXME", "NotImplementedException", "NotSupportedException", "null!", "throw new Exception",
    "Guid\.Empty", "Guid\.NewGuid", "new Dictionary", "new ConcurrentDictionary"
) -Files $EntityFiles

Write-PatternAudit -Title "IDENTITY AND PROVENANCE AUDIT" -Patterns @(
    "\bGuid\b", "\bId\b", "\bSlug\b", "\bPath\b", "\bEntity\b", "\bFact\b", "\bDiscovery\b"
) -Files $EntityFiles

Write-TypeAudit -Files $EntityFiles

Write-Section "RELEVANT CORE CONTRACT LOCATIONS"
$CorePatterns = @(
    "interface IBaseObject", "abstract class BaseObject", "interface IComponent", "abstract class BaseComponent",
    "interface IContext", "class WildsContext", "interface IState", "interface IStateInstruction",
    "interface IRegistry", "class RegistryBase", "interface IEmitter", "interface IEmissionStateInstruction"
)
foreach ($Pattern in $CorePatterns) {
    Write-ReportLine
    Write-ReportLine "Pattern: $Pattern"
    Write-ReportLine ("-" * 72)
    $Matches = @($CoreFiles | Select-String -Pattern $Pattern -CaseSensitive:$false)
    if ($Matches.Count -eq 0) {
        Write-ReportLine "<no matches>"
        continue
    }
    foreach ($Match in $Matches) {
        Write-ReportLine ("{0}:{1}: {2}" -f (Get-RelativePath $Match.Path), $Match.LineNumber, $Match.Line.Trim())
    }
}

if ($IncludeCoreSource) {
    Write-Section "FULL RELEVANT CORE SOURCE"
    $RelevantCoreFiles = @($CoreFiles | Where-Object {
        $_.Name -in @(
            "IBaseObject.cs", "BaseObject.cs", "IComponent.cs", "BaseComponent.cs", "IContext.cs",
            "WildsContext.cs", "IState.cs", "IStateInstruction.cs", "IRegistry.cs", "RegistryBase.cs"
        )
    } | Sort-Object FullName)
    foreach ($File in $RelevantCoreFiles) { Write-SourceFile $File }
}

Write-Section "FULL ENTITIES SOURCE"
foreach ($File in $EntityFiles) { Write-SourceFile $File }

$BuildExitCode = $null
if (-not $SkipBuild) {
    Write-Section "BUILD"
    $Solution = Join-Path $RepoRoot "WildsEngine.sln"
    if (Test-Path $Solution) {
        $BuildOutput = & dotnet build $Solution --configuration Debug --no-restore 2>&1
        $BuildExitCode = $LASTEXITCODE
        foreach ($Line in $BuildOutput) { Write-ReportLine ([string]$Line) }
        Write-ReportLine
        if ($BuildExitCode -eq 0) {
            Write-ReportLine "[PASS] Solution build succeeded."
        } else {
            Write-ReportLine "[FAIL] Solution build returned exit code $BuildExitCode."
        }
    } else {
        Write-ReportLine "[MISSING] WildsEngine.sln"
    }
}

Write-Section "AUDIT SUMMARY"
Write-ReportLine "Entities files: $($EntityFiles.Count)"

$QslLeaks = @($EntityFiles | Select-String -Pattern "QSL\.|QslEngine|QslRuntimeContext|QslConsumerBinding|QslStatePackage|WildsQslRequest|WildsQslExecutor|WildsEngine\.Qsl" -CaseSensitive:$false)
if ($QslLeaks.Count -eq 0) {
    Write-ReportLine "[PASS] No direct QSL integration references found in Core.Entities."
} else {
    Write-ReportLine "[REVIEW] Direct QSL-related references found: $($QslLeaks.Count)"
}

$WildsBaseUsage = @($EntityFiles | Select-String -Pattern "\bIBaseObject\b|\bBaseObject\b")
Write-ReportLine "[INFO] Wilds base-object references found: $($WildsBaseUsage.Count)"
Write-ReportLine
Write-ReportLine "Report: $ReportPath"

$Builder.ToString() | Set-Content -Path $ReportPath -Encoding UTF8
Copy-Item -Path $ReportPath -Destination $LatestPath -Force

Write-Host
Write-Host "===================================================="
Write-Host " WILDSENGINE CORE.ENTITIES AUDIT"
Write-Host "===================================================="
Write-Host
Write-Host "Report:"
Write-Host "  $ReportPath"
Write-Host
Write-Host "Latest:"
Write-Host "  $LatestPath"
Write-Host
if (-not $SkipBuild -and $null -ne $BuildExitCode) {
    if ($BuildExitCode -eq 0) {
        Write-Host "Build: PASS" -ForegroundColor Green
    } else {
        Write-Host "Build: FAIL" -ForegroundColor Red
    }
} elseif ($SkipBuild) {
    Write-Host "Build: SKIPPED" -ForegroundColor Yellow
}
