$ErrorActionPreference = "Stop"

$Runner = Join-Path `
    $PSScriptRoot `
    "Invoke-E7Gate.ps1"

& $Runner `
    -Gate "E7-L" `
    -Title "Believable Lie End-to-End Certification" `
    -AssertionFilter `
        "FullyQualifiedName~EntityE7EndToEndCertificationTests" `
    -ReadableFilter `
        "FullyQualifiedName~EntityE7EndToEndReadableTests"

exit $LASTEXITCODE