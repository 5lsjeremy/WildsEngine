$ErrorActionPreference = "Stop"

$Root = (Get-Location).Path
$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"

$AuditRoot = Join-Path $Root "Audits"
$ReportRoot = Join-Path $AuditRoot "Reports"

$Report = Join-Path `
    $ReportRoot `
    "Entity-E7-Test-Crawl-$Timestamp.txt"

$Latest = Join-Path `
    $ReportRoot `
    "Entity-E7-Test-Crawl-Latest.txt"

New-Item `
    -ItemType Directory `
    -Path $ReportRoot `
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
                    $_.FullName -notmatch "\\Audits\\" -and
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

Write-Section "WILDS ENTITY E7 TEST CERTIFICATION CRAWL"

Write-Line "Root: $Root"
Write-Line "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
Write-Line "C# source files: $($Files.Count)"

Write-Section "TEST SOURCE INVENTORY"

foreach ($File in $Files)
{
    $Relative =
    $File.FullName.
            Substring($Root.Length).
            TrimStart("\")

    Write-Line $Relative
}

Write-Section "TEST CLASS AND METHOD INVENTORY"

$TestClasses = @()
$TestMethods = @()

foreach ($File in $Files)
{
    $Relative =
    $File.FullName.
            Substring($Root.Length).
            TrimStart("\")

    $ClassMatches =
    Select-String `
            -Path $File.FullName `
            -Pattern `
                "public\s+(?:sealed\s+|abstract\s+)?class\s+([A-Za-z0-9_]+)" `
            -AllMatches

    foreach ($ClassMatchSet in $ClassMatches)
    {
        foreach ($Match in $ClassMatchSet.Matches)
        {
            $TestClasses +=
            [pscustomobject]@{
                Name = $Match.Groups[1].Value
                File = $Relative
            }

            Write-Line (
            "CLASS  {0} -> {1}" -f
            $Match.Groups[1].Value,
            $Relative
            )
        }
    }

    $MethodMatches =
    Select-String `
            -Path $File.FullName `
            -Pattern `
                "public\s+(?:async\s+)?(?:void|Task|ValueTask)\s+([A-Za-z0-9_]+)\s*\(" `
            -AllMatches

    foreach ($MethodMatchSet in $MethodMatches)
    {
        foreach ($Match in $MethodMatchSet.Matches)
        {
            $TestMethods +=
            [pscustomobject]@{
                Name = $Match.Groups[1].Value
                File = $Relative
            }

            Write-Line (
            "METHOD {0} -> {1}" -f
            $Match.Groups[1].Value,
            $Relative
            )
        }
    }
}

Write-Section "ENTITY AND QSL TEST FILES"

$RelevantFiles =
$Files |
        Where-Object {
            $Content =
            Get-Content `
                -Path $_.FullName `
                -Raw

            $Content -match `
            "IEntity|EntityBase|EntitySurface|IFact|FactBase|" +
                    "IDiscovery|DiscoveryBase|IDiscoveryEvidence|" +
                    "IProof|ProofResult|IWildsEntity|IWildsFact|" +
                    "IWildsDiscovery|IWildsEvidence|EntityQsl|" +
                    "WildsQsl|QslStatePackage"
        }

foreach ($File in $RelevantFiles)
{
    $Relative =
    $File.FullName.
            Substring($Root.Length).
            TrimStart("\")

    Write-Line $Relative
}

Write-Section "BROKEN SINGLE-FACT DISCOVERY MODEL"

Write-Matches `
    "Discovery.Fact references" `
    "\bdiscovery\.Fact\b|\bDiscovery\.Fact\b"

Write-Matches `
    "Singular Discovery Fact declarations" `
    "public\s+IFact\s+Fact\b|IDiscovery\s+\w+\s*,\s*IFact"

Write-Matches `
    "Old Discovery constructors receiving one Fact" `
    "new\s+Discovery\s*\([^;]*\bfact\b"

Write-Matches `
    "Old available flag construction" `
    "new\s+Discovery\s*\([^;]*(true|false)\s*\)"

Write-Matches `
    "Manual availability lifecycle" `
    "MakeAvailable|MakeUnavailable|MarkAvailable|SetAvailable"

Write-Section "REMOVED FACT IDENTITY ASSUMPTIONS"

Write-Matches `
    "Fact.Path references" `
    "\bfact\.Path\b|\bFact\.Path\b"

Write-Matches `
    "Fact.Id references" `
    "\bfact\.Id\b|\bFact\.Id\b"

Write-Matches `
    "Fact.Slug references" `
    "\bfact\.Slug\b|\bFact\.Slug\b"

Write-Matches `
    "Old Fact constructors using Guid or slug" `
    "new\s+Fact\s*\([^;]*(Guid|slug)"

Write-Matches `
    "Discovery.Id references" `
    "\bdiscovery\.Id\b|\bDiscovery\.Id\b"

Write-Matches `
    "Discovery.Slug references" `
    "\bdiscovery\.Slug\b|\bDiscovery\.Slug\b"

Write-Section "OLD PROOF SEMANTICS"

Write-Matches `
    "IsValid proof semantics" `
    "\bIsValid\b|\bisValid\b"

Write-Matches `
    "ValidateEvidence override" `
    "\bValidateEvidence\s*\("

Write-Matches `
    "Old ProofResult constructors" `
    "new\s+ProofResult\s*\([^;]*(true|false)\s*,\s*[^,)]*\)"

Write-Matches `
    "Truth hard-coded as universal Fact contribution" `
    "ProofResult\.IsTrue"

Write-Section "CURRENT BELIEF MODEL"

Write-Matches `
    "Believability" `
    "\bBelievability\b|\bbelievability\b"

Write-Matches `
    "Truth conclusion" `
    "\bIsTrue\b|\bisTrue\b"

Write-Matches `
    "Evidence contribution policy" `
    "IEvidenceContributionPolicy|EvidenceContributionPolicyBase"

Write-Matches `
    "Truth policies" `
    "TrueEvidencePolicy|FalseEvidencePolicy"

Write-Matches `
    "Believability policies" `
    "MinimumBelievabilityPolicy|" +
"MaximumBelievabilityPolicy|" +
        "BelievabilityRangePolicy"

Write-Matches `
    "Deception and mixed-state policies" `
    "BelievableTrueEvidencePolicy|" +
"BelievableFalseEvidencePolicy|" +
        "TrueButUnbelievableEvidencePolicy"

Write-Matches `
    "Composite policies" `
    "AllEvidencePolicies|" +
"AnyEvidencePolicies|" +
        "NotEvidencePolicy|" +
        "DelegateEvidencePolicy"

Write-Section "CURRENT KNOWLEDGE PIPELINE COVERAGE"

Write-Matches `
    "Native surface capture" `
    "IEntityNativeSurface|EntityNativeSurface|NativeAttribute"

Write-Matches `
    "Evidence creation" `
    "DiscoveryEvidence|EvidenceBase"

Write-Matches `
    "Proof creation and application" `
    "ProofBase|ProofResult|ApplyProof|SetProofResult"

Write-Matches `
    "Evidence to Fact lifecycle" `
    "ReceiveEvidence|AddEvidence|RemoveEvidenceItem|IsProven"

Write-Matches `
    "Fact to Discovery lifecycle" `
    "ReceiveFact|AddFact|RemoveFactItem|IsAvailable"

Write-Matches `
    "Entity ownership" `
    "AddDiscovery|RemoveDiscovery|Discoveries|ReferenceEquals"

Write-Matches `
    "Wilds projection" `
    "WildsEntity|WildsEvidence|WildsFact|WildsDiscovery"

Write-Matches `
    "Private versus promoted knowledge" `
    "IWildsProjection|WildsProjectionBase|Source"

Write-Section "QSL ADAPTER COVERAGE"

Write-Matches `
    "Entity QSL state adapter" `
    "EntityQslStateAdapter"

Write-Matches `
    "Entity QSL request adapter" `
    "EntityQslRequestAdapter"

Write-Matches `
    "Multi-property package receipt" `
    "QslStatePackage|ReceiveState"

Write-Matches `
    "Property updates" `
    "UpdateState"

Write-Matches `
    "Property removal" `
    "RemoveState"

Write-Matches `
    "Fact.Name vocabulary" `
    "fact\.Name|Fact\.Name"

Write-Matches `
    "Proven Fact filtering" `
    "fact\.IsProven|Fact\.IsProven"

Write-Matches `
    "Discovery Facts collection" `
    "discovery\.Facts|Discovery\.Facts"

Write-Section "E7 RECERTIFICATION TARGET COVERAGE"

$Targets = @(
    @{
        Name = "E7-A Native Capture"
        Pattern = "EntityNativeSurface|NativeAttribute"
    },
    @{
        Name = "E7-B Evidence Snapshot"
        Pattern = "DiscoveryEvidence|Conditions|Neighbors"
    },
    @{
        Name = "E7-C Proof Semantics"
        Pattern = "Believability|IsTrue|ProofResult"
    },
    @{
        Name = "E7-D Contribution Policies"
        Pattern = "IEvidenceContributionPolicy|EvidencePolicy"
    },
    @{
        Name = "E7-E Fact Threshold"
        Pattern = "RequiredEvidenceCount|CurrentEvidenceCount|EvidencePercentage|IsProven"
    },
    @{
        Name = "E7-F Competing Belief Standards"
        Pattern = "BelievableFalse|MinimumBelievability|TrueEvidencePolicy"
    },
    @{
        Name = "E7-G Discovery Threshold"
        Pattern = "RequiredFactCount|CurrentFactCount|FactPercentage|IsAvailable"
    },
    @{
        Name = "E7-H Ownership and Traceability"
        Pattern = "ReferenceEquals|exact Entity|foreign"
    },
    @{
        Name = "E7-I Wilds Promotion"
        Pattern = "IWildsEntity|IWildsEvidence|IWildsFact|IWildsDiscovery"
    },
    @{
        Name = "E7-J Private Knowledge Boundary"
        Pattern = "private knowledge|unpromoted|promotion"
    },
    @{
        Name = "E7-K Native and Discovered QSL Vocabulary"
        Pattern = "EntityQslStateAdapter|EntityQslRequestAdapter|QslStatePackage"
    },
    @{
        Name = "E7-L Believable Lie"
        Pattern = "BelievableFalse|believable lie|IsTrue\s*=\s*false"
    }
)

foreach ($Target in $Targets)
{
    $Hits =
    $Files |
            Select-String `
            -Pattern $Target.Pattern `
            -CaseSensitive:$false

    if ($Hits)
    {
        $FileCount =
        $Hits |
                Select-Object `
                -ExpandProperty Path `
                -Unique |
                Measure-Object |
                Select-Object `
                -ExpandProperty Count

        Write-Line (
        "[PRESENT] {0} ({1} matching files)" -f
        $Target.Name,
        $FileCount
        )
    }
    else
    {
        Write-Line (
        "[MISSING] {0}" -f
        $Target.Name
        )
    }
}

Write-Section "POTENTIAL TEST SUITE REWRITE SIGNALS"

$LegacyPatterns = @(
    "\bdiscovery\.Fact\b",
    "\bfact\.Path\b",
    "\bfact\.Id\b",
    "\bfact\.Slug\b",
    "\bIsValid\b",
    "MakeAvailable",
    "MakeProven"
)

foreach ($Pattern in $LegacyPatterns)
{
    $Hits =
    $Files |
            Select-String `
            -Pattern $Pattern `
            -CaseSensitive:$false

    $Count =
    @($Hits).Count

    Write-Line (
    "{0} -> {1} matches" -f
    $Pattern,
    $Count
    )
}

Write-Section "PROJECT AND BUILD REFERENCES"

$ProjectFiles =
Get-ChildItem `
        -Path $Root `
        -File `
        -Filter *.csproj

foreach ($ProjectFile in $ProjectFiles)
{
    Write-Line "FILE: $($ProjectFile.Name)"

    Get-Content `
        -Path $ProjectFile.FullName |
            ForEach-Object {
                Write-Line $_
            }
}

Write-Section "FULL RELEVANT SOURCE"

foreach ($File in $RelevantFiles)
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

Copy-Item `
    -Path $Report `
    -Destination $Latest `
    -Force

Write-Section "CRAWL COMPLETE"

Write-Line "Report: $Report"
Write-Line "Latest: $Latest"

Write-Host ""
Write-Host "============================================================"
Write-Host " ENTITY E7 TEST CRAWL COMPLETE"
Write-Host "============================================================"
Write-Host "Files         : $($Files.Count)"
Write-Host "Relevant files: $($RelevantFiles.Count)"
Write-Host "Report        : $Report"
Write-Host "Latest        : $Latest"
Write-Host "============================================================"