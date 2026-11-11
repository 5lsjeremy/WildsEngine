$ErrorActionPreference = "Stop"

$Runner = Join-Path `
    $PSScriptRoot `
    "Invoke-E7Gate.ps1"

& $Runner `
    -Gate "E7-G" `
    -Title "Discovery Resolution" `
    -AssertionFilter `
        "FullyQualifiedName~EntityDiscoveryResolutionCertificationTests" `
    -ReadableFilter `
        "FullyQualifiedName~EntityDiscoveryResolutionReadableTests"

exit $LASTEXITCODE