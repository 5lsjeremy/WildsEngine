$ErrorActionPreference = "Stop"

$Runner = Join-Path `
    $PSScriptRoot `
    "Invoke-E7Gate.ps1"

& $Runner `
    -Gate "E7-B" `
    -Title "Evidence Integrity" `
    -AssertionFilter `
        "FullyQualifiedName~EntityEvidenceCertificationTests" `
    -ReadableFilter `
        "FullyQualifiedName~EntityEvidenceReadableTests"

exit $LASTEXITCODE