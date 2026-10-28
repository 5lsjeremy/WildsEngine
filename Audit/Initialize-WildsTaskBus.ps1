$InstructionFiles = @(
    "StateInstruction.cs",
    "ContextVariableInstruction.cs",
    "SetContextVariableInstruction.cs",
    "AddNeighborInstruction.cs",
    "RemoveNeighborInstruction.cs",
    "SetComponentMetadataInstruction.cs",
    "SetContextMetadataInstruction.cs",
    "ConsumerSetVariableInstruction.cs"
)

$InstructionSearchPaths = @(
    $ProcessingPath,
    (Join-Path $ProcessingPath "Base"),
    (Join-Path $ProcessingPath "Instructions")
)

foreach ($InstructionFile in $InstructionFiles) {
    $MatchingFiles = @(
    foreach ($SearchPath in $InstructionSearchPaths) {
        if (Test-Path $SearchPath -PathType Container) {
            Get-ChildItem `
                    -Path $SearchPath `
                    -Filter $InstructionFile `
                    -File `
                    -Recurse `
                    -ErrorAction SilentlyContinue
        }
    }
    )

    if ($MatchingFiles.Count -eq 0) {
        Write-Host `
            "[NOT FOUND] Existing instruction file: $InstructionFile" `
            -ForegroundColor Yellow

        continue
    }

    foreach ($MatchingFile in $MatchingFiles) {
        Write-Host `
            "[UNCHANGED] $($MatchingFile.FullName)" `
            -ForegroundColor DarkGray
    }
}

# ============================================================
# Verification
# ============================================================

$RequiredFiles = @(
    $IBusPath,
    $ITaskPath,
    $ITaskBusPath,
    $BaseTaskPath
)

if (Test-Path $WildsSignalPath -PathType Leaf) {
    $RequiredFiles += $WildsSignalPath
}

$MissingFiles = @(
foreach ($RequiredFile in $RequiredFiles) {
    if (-not (Test-Path $RequiredFile -PathType Leaf)) {
        $RequiredFile
    }
}
)

if ($MissingFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "Task-bus initialization is incomplete." `
        -ForegroundColor Red

    foreach ($MissingFile in $MissingFiles) {
        Write-Host `
            "[MISSING] $MissingFile" `
            -ForegroundColor Red
    }

    exit 1
}

Write-Host ""
Write-Host "============================================================" `
    -ForegroundColor DarkGray
Write-Host " Wilds Task-Bus Initialization" `
    -ForegroundColor Cyan
Write-Host "============================================================" `
    -ForegroundColor DarkGray
Write-Host "IBus       : $IBusPath" `
    -ForegroundColor Green
Write-Host "ITask      : $ITaskPath" `
    -ForegroundColor Green
Write-Host "ITaskBus   : $ITaskBusPath" `
    -ForegroundColor Green
Write-Host "BaseTask   : $BaseTaskPath" `
    -ForegroundColor Green

if (Test-Path $WildsSignalPath -PathType Leaf) {
    Write-Host "WildsSignal: $WildsSignalPath" `
        -ForegroundColor Green
}
else {
    Write-Host "WildsSignal: not found; update was skipped" `
        -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Task-bus foundation initialized successfully." `
    -ForegroundColor Green
Write-Host ""
Write-Host "No instruction implementations were modified." `
    -ForegroundColor DarkGray
Write-Host "No task execution or task-signal contract was introduced." `
    -ForegroundColor DarkGray
Write-Host ""

exit 0