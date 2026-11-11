$ErrorActionPreference = "Stop"

$Runner = Join-Path `
    $PSScriptRoot `
    "Invoke-E7Gate.ps1"

& $Runner `
    -Gate "E7-C" `
    -Title "Proof Truth and Believability" `
    -AssertionFilter `
        "FullyQualifiedName~EntityProofCertificationTests" `
    -ReadableFilter `
        "FullyQualifiedName~EntityProofReadableTests"

exit $LASTEXITCODE