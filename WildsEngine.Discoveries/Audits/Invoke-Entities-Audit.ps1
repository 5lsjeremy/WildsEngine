$ErrorActionPreference = "Stop"

$Root = (Get-Location).Path
$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

$ReportDir = Join-Path $Root "Audits/Reports"
$Report = Join-Path $ReportDir "Entity-E7-Certification-Crawl-$Timestamp.txt"
$Latest = Join-Path $ReportDir "Entity-E7-Certification-Crawl-Latest.txt"

New-Item `
    -ItemType Directory `
    -Path $ReportDir `
    -Force |
        Out-Null

$Files =
Get-ChildItem `
        -Path $Root `
        -Recurse `
        -File `
        -Filter *.cs |
        Where-Object {
            $_.FullName -notmatch "\\bin\\" -and
                    $_.FullName -notmatch "\\obj\\" -and
                    $_.FullName -notmatch "\\Audit\\"
        } |
        Sort-Object FullName

function Write-Line
{
    param(
        [string]$Text = ""
    )

    $Text |
            Tee-Object `
            -FilePath $Report `
            -Append
}

function Write-Section
{
    param(
        [string]$Title
    )

    Write-Line ""
    Write-Line "============================================================"
    Write-Line $Title
    Write-Line "============================================================"
}

function Write-Matches
{
    param(
        [string]$Title,
        [string]$Pattern
    )

    Write-Line ""
    Write-Line "------------------------------------------------------------"
    Write-Line $Title
    Write-Line "------------------------------------------------------------"

    $Hits =
    $Files |
            Select-String `
            -Pattern $Pattern `
            -CaseSensitive:$false

    if (-not $Hits)
    {
        Write-Line "<none>"
        return
    }

    foreach ($Hit in $Hits)
    {
        $Relative =
        $Hit.Path.
                Substring($Root.Length).
                TrimStart("\")

        Write-Line (
        "{0}:{1}: {2}" -f
        $Relative,
        $Hit.LineNumber,
        $Hit.Line.Trim()
        )
    }
}

Write-Section "WILDS ENTITY E7 CERTIFICATION CRAWL"

Write-Line "Root: $Root"
Write-Line "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "C# source files: $($Files.Count)"

Write-Section "SOURCE INVENTORY"

foreach ($File in $Files)
{
    $Relative =
    $File.FullName.
            Substring($Root.Length).
            TrimStart("\")

    Write-Line $Relative
}

Write-Section "TYPE INVENTORY"

$Types = @()

foreach ($File in $Files)
{
    $Relative =
    $File.FullName.
            Substring($Root.Length).
            TrimStart("\")

    $Matches =
    Select-String `
            -Path $File.FullName `
            -Pattern `
                "public\s+(?:abstract\s+|sealed\s+)?(?:class|interface)\s+([A-Za-z0-9_]+)" `
            -AllMatches

    foreach ($MatchSet in $Matches)
    {
        foreach ($Match in $MatchSet.Matches)
        {
            $TypeName =
            $Match.Groups[1].Value

            $Types +=
            [pscustomobject]@{
                Type = $TypeName
                File = $Relative
            }

            Write-Line (
            "{0} -> {1}" -f
            $TypeName,
            $Relative
            )
        }
    }
}

Write-Section "DUPLICATE TYPE CHECK"

$Duplicates =
$Types |
        Group-Object Type |
        Where-Object {
            $_.Count -gt 1
        }

if (-not $Duplicates)
{
    Write-Line "[PASS] No duplicate public type names."
}
else
{
    foreach ($Duplicate in $Duplicates)
    {
        Write-Line ""
        Write-Line "[DUPLICATE] $($Duplicate.Name)"

        foreach ($Entry in $Duplicate.Group)
        {
            Write-Line "  $($Entry.File)"
        }
    }
}

Write-Section "E7 LEGACY MODEL CHECK"

Write-Matches `
    "Legacy Fact -> Discovery ownership" `
    "List\s*<\s*IDiscovery\s*>|IReadOnlyList\s*<\s*IDiscovery\s*>|Fact.*Discoveries|AddDiscovery|RemoveDiscovery"

Write-Matches `
    "Legacy single Discovery.Fact relationship" `
    "public\s+IFact\s+Fact\b|discovery\.Fact\b"

Write-Matches `
    "Legacy manual Discovery availability" `
    "MakeAvailable|MakeUnavailable"

Write-Matches `
    "Legacy manual Fact proof" `
    "MakeProven|MarkProven"

Write-Matches `
    "Legacy IsValid proof semantics" `
    "\bIsValid\b|\bisValid\b"

Write-Matches `
    "Mutable resolution counters" `
    "CurrentEvidenceCount\s*=|CurrentFactCount\s*=|EvidencePercentage\s*=|FactPercentage\s*="

Write-Section "E7 KNOWLEDGE PIPELINE"

Write-Matches `
    "Evidence" `
    "IDiscoveryEvidence|EvidenceBase|DiscoveryEvidence|AddEvidence|RemoveEvidence"

Write-Matches `
    "Proof" `
    "IProof\b|IProofResult|ProofBase|ProofResult"

Write-Matches `
    "Believability" `
    "\bBelievability\b|\bbelievability\b"

Write-Matches `
    "Truth resolution" `
    "\bIsTrue\b|\bisTrue\b"

Write-Matches `
    "Evidence validation" `
    "\bIsValidated\b|SetProofResult|ApplyProof"

Write-Matches `
    "Fact resolution" `
    "RequiredEvidenceCount|CurrentEvidenceCount|EvidencePercentage|IsProven"

Write-Matches `
    "Discovery resolution" `
    "RequiredFactCount|CurrentFactCount|FactPercentage|IsAvailable"

Write-Section "NATIVE ATTRIBUTE CAPTURE"

Write-Matches `
    "Native surface contracts" `
    "INativeAttribute|IEntityNativeSurface"

Write-Matches `
    "Native surface implementations" `
    "NativeAttribute|EntityNativeSurface|NativeAttributes"

Write-Matches `
    "Reflection capture" `
    "GetProperties|GetFields|BindingFlags|GetValue"

Write-Section "WILDS PROJECTION MODEL"

Write-Matches `
    "Generic projection" `
    "IWildsProjection|WildsProjectionBase"

Write-Matches `
    "Discovery projection" `
    "IWildsDiscovery|WildsDiscoveryBase|WildsDiscovery"

Write-Matches `
    "Fact projection" `
    "IWildsFact|WildsFactBase|WildsFact"

Write-Matches `
    "Evidence projection" `
    "IWildsEvidence|WildsEvidenceBase|WildsEvidence"

Write-Matches `
    "Speculative ProofResult projection" `
    "IWildsProofResult|WildsProofResult"

Write-Section "ENTITY OWNERSHIP"

Write-Matches `
    "Entity Facts" `
    "IReadOnlyList\s*<\s*IFact\s*>|AddFact|RemoveFact"

Write-Matches `
    "Entity Discoveries" `
    "IReadOnlyList\s*<\s*IDiscovery\s*>|AddDiscovery|RemoveDiscovery"

Write-Matches `
    "Exact Entity ownership checks" `
    "ReferenceEquals.*Entity|belong to this exact Entity|belong to the exact"

Write-Section "IDENTITY SURFACES"

Write-Matches `
    "Consumer / domain identity" `
    "IIdentifier|IdentifierBase|ExternalId|Slug"

Write-Matches `
    "Wilds runtime identity" `
    "IBaseObject|BaseObject|Guid id|Guid\.NewGuid"

Write-Section "NAMESPACE AUDIT"

foreach ($File in $Files)
{
    $Relative =
    $File.FullName.
            Substring($Root.Length).
            TrimStart("\")

    $Namespace =
    Select-String `
            -Path $File.FullName `
            -Pattern "^\s*namespace\s+([A-Za-z0-9_.]+)" |
            Select-Object `
            -First 1

    if ($Namespace)
    {
        Write-Line (
        "{0} -> {1}" -f
        $Relative,
        $Namespace.Matches[0].Groups[1].Value
        )
    }
    else
    {
        Write-Line (
        "[NO NAMESPACE] {0}" -f
        $Relative
        )
    }
}

Write-Section "EXPECTED E7 TYPE INVENTORY"

$Expected = @(
    "IEntity",
    "IEntitySurface",
    "IEntityNativeSurface",
    "INativeAttribute",

    "IDiscoveryEvidence",
    "IProof",
    "IProofResult",

    "IFact",
    "IDiscovery",

    "IResolutionThreshold",

    "IWildsProjection",
    "IWildsFact",
    "IWildsDiscovery",
    "IWildsEvidence",

    "IdentifierBase",
    "ResolutionThresholdBase",
    "EvidenceBase",
    "ProofBase",
    "FactBase",
    "DiscoveryBase",

    "WildsProjectionBase",
    "WildsFactBase",
    "WildsDiscoveryBase",
    "WildsEvidenceBase",

    "NativeAttribute",
    "EntityNativeSurface",
    "DiscoveryEvidence",
    "ProofResult",
    "Fact",
    "Discovery",

    "WildsFact",
    "WildsDiscovery",
    "WildsEvidence"
)

foreach ($ExpectedType in $Expected)
{
    $Found =
    $Types |
            Where-Object {
                $_.Type -eq $ExpectedType
            }

    if ($Found)
    {
        Write-Line "[FOUND]   $ExpectedType"
    }
    else
    {
        Write-Line "[MISSING] $ExpectedType"
    }
}

Write-Section "FULL SOURCE"

foreach ($File in $Files)
{
    $Relative =
    $File.FullName.
            Substring($Root.Length).
            TrimStart("\")

    Write-Line ""
    Write-Line "------------------------------------------------------------"
    Write-Line "FILE: $Relative"
    Write-Line "------------------------------------------------------------"

    Get-Content `
        -Path $File.FullName |
            ForEach-Object {
                Write-Line $_
            }
}

Write-Section "CRAWL COMPLETE"

Write-Line "Report: $Report"
Write-Line "Latest: $Latest"

Copy-Item `
    -Path $Report `
    -Destination $Latest `
    -Force

Write-Host ""
Write-Host "============================================================"
Write-Host " ENTITY E7 CERTIFICATION CRAWL COMPLETE"
Write-Host "============================================================"
Write-Host "Files : $($Files.Count)"
Write-Host "Report: $Report"
Write-Host "Latest: $Latest"
Write-Host "============================================================"