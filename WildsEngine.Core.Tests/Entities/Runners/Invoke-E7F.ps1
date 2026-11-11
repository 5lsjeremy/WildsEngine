$ErrorActionPreference = "Stop"

$Runner = Join-Path `
    $PSScriptRoot `
    "Invoke-E7Gate.ps1"

& $Runner `
    -Gate "E7-F" `
    -Title "Divergent Belief" `
    -AssertionFilter `
        "FullyQualifiedName~EntityDivergentBeliefCertificationTests" `
    -ReadableFilter `
        "FullyQualifiedName~EntityDivergentBeliefReadableTests"

exit $LASTEXITCODE
