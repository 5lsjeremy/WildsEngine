[CmdletBinding()]
param(
    [string] $Root = (Get-Location).Path
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$ExpectedRootName = "WildsEngine"

if ((Split-Path $Root -Leaf) -ne $ExpectedRootName) {
    throw "Run this crawler from the WildsEngine repository root. Current path: $Root"
}

$CorePath =
Join-Path $Root "WildsEngine.Core"

$ProcessingPath =
Join-Path $CorePath "Processing"

$AbstractionsPath =
Join-Path $CorePath "Abstractions"

$SignalsPath =
Join-Path $CorePath "Signals"

$ReportDirectory =
Join-Path $Root "Audit\Reports"

if (-not (Test-Path $CorePath -PathType Container)) {
    throw "WildsEngine.Core was not found: $CorePath"
}

if (-not (Test-Path $ProcessingPath -PathType Container)) {
    throw "Processing directory was not found: $ProcessingPath"
}

New-Item `
    -ItemType Directory `
    -Force `
    -Path $ReportDirectory |
        Out-Null

$Timestamp =
Get-Date -Format "yyyyMMdd-HHmmss"

$ReportPath =
Join-Path `
        $ReportDirectory `
        "Wilds-Processing-Crawl-$Timestamp.txt"

$Lines =
New-Object System.Collections.Generic.List[string]

function Add-Line {
    param(
        [AllowEmptyString()]
        [string] $Value = ""
    )

    $Lines.Add($Value)
}

function Add-Separator {
    Add-Line "============================================================"
}

function Add-SourceFile {
    param(
        [Parameter(Mandatory = $true)]
        [System.IO.FileInfo] $File,

        [Parameter(Mandatory = $true)]
        [string] $SectionRoot
    )

    $RelativePath =
    $File.FullName.Substring(
            $SectionRoot.Length
    ).TrimStart(
            [System.IO.Path]::DirectorySeparatorChar,
            [System.IO.Path]::AltDirectorySeparatorChar
    )

    Add-Line ""
    Add-Line "############################################################"
    Add-Line "# FILE: $RelativePath"
    Add-Line "############################################################"

    $Content =
    Get-Content `
            -Path $File.FullName `
            -Raw

    if ([string]::IsNullOrEmpty($Content)) {
        Add-Line "<empty file>"
        return
    }

    foreach ($ContentLine in ($Content -split "`r?`n")) {
        Add-Line $ContentLine
    }
}

function Add-DirectoryTree {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $TreeOutput =
    @(
        & tree $Path /F /A 2>&1
    )

    foreach ($TreeLine in $TreeOutput) {
        Add-Line ([string] $TreeLine)
    }
}

$ProcessingFiles =
@(
Get-ChildItem `
            -Path $ProcessingPath `
            -Filter "*.cs" `
            -File `
            -Recurse |
        Sort-Object FullName
)

$RelevantAbstractionNames = @(
    "IBaseObject.cs"
    "IBus.cs"
    "IComponent.cs"
    "IContext.cs"
    "ISignal.cs"
    "IState.cs"
    "IStateInstruction.cs"
    "ITask.cs"
    "ITaskBus.cs"
)

$AbstractionFiles =
@(
foreach ($Name in $RelevantAbstractionNames) {
    $Path =
    Join-Path $AbstractionsPath $Name

    if (Test-Path $Path -PathType Leaf) {
        Get-Item $Path
    }
}
)

$SignalFiles =
@()

if (Test-Path $SignalsPath -PathType Container) {
    $SignalFiles =
    @(
    Get-ChildItem `
                -Path $SignalsPath `
                -Filter "*.cs" `
                -File `
                -Recurse |
            Sort-Object FullName
    )
}

Add-Separator
Add-Line " WildsEngine.Core Processing Crawl"
Add-Separator
Add-Line "Root: $Root"
Add-Line "Core: $CorePath"
Add-Line "Processing: $ProcessingPath"
Add-Line "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Add-Line "Processing files: $($ProcessingFiles.Count)"
Add-Line "Relevant abstraction files: $($AbstractionFiles.Count)"
Add-Line "Signal files: $($SignalFiles.Count)"
Add-Separator

Add-Line ""
Add-Line "PROCESSING TREE"
Add-Separator

Add-DirectoryTree -Path $ProcessingPath

Add-Line ""
Add-Line "PROCESSING FILE INVENTORY"
Add-Separator

if ($ProcessingFiles.Count -eq 0) {
    Add-Line "<no Processing C# files found>"
}
else {
    foreach ($File in $ProcessingFiles) {
        $RelativePath =
        $File.FullName.Substring(
                $ProcessingPath.Length
        ).TrimStart(
                [System.IO.Path]::DirectorySeparatorChar,
                [System.IO.Path]::AltDirectorySeparatorChar
        )

        Add-Line $RelativePath
    }
}

Add-Line ""
Add-Line "PROCESSING SOURCE"
Add-Separator

foreach ($File in $ProcessingFiles) {
    Add-SourceFile `
        -File $File `
        -SectionRoot $ProcessingPath
}

Add-Line ""
Add-Line "RELATED ABSTRACTIONS"
Add-Separator

if ($AbstractionFiles.Count -eq 0) {
    Add-Line "<no related abstraction files found>"
}
else {
    foreach ($File in $AbstractionFiles) {
        Add-SourceFile `
            -File $File `
            -SectionRoot $AbstractionsPath
    }
}

Add-Line ""
Add-Line "SIGNAL SOURCE"
Add-Separator

if ($SignalFiles.Count -eq 0) {
    Add-Line "<no Signals C# files found>"
}
else {
    foreach ($File in $SignalFiles) {
        Add-SourceFile `
            -File $File `
            -SectionRoot $SignalsPath
    }
}

Add-Line ""
Add-Separator
Add-Line " Crawl complete"
Add-Separator

[System.IO.File]::WriteAllLines(
        $ReportPath,
        $Lines,
        [System.Text.UTF8Encoding]::new($false)
)

Write-Host ""
Write-Host "============================================================" `
    -ForegroundColor DarkGray
Write-Host " WildsEngine.Core Processing Crawl" `
    -ForegroundColor Cyan
Write-Host "============================================================" `
    -ForegroundColor DarkGray
Write-Host "Processing files : $($ProcessingFiles.Count)" `
    -ForegroundColor Green
Write-Host "Abstraction files: $($AbstractionFiles.Count)" `
    -ForegroundColor Green
Write-Host "Signal files     : $($SignalFiles.Count)" `
    -ForegroundColor Green
Write-Host "Report           : $ReportPath" `
    -ForegroundColor Cyan
Write-Host ""