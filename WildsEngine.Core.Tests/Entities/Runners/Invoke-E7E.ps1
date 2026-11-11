$ErrorActionPreference = "Stop"

$Runner = Join-Path `
    $PSScriptRoot `
    "Invoke-E7Gate.ps1"

& $Runner `
    -Gate "E7-E" `
    -Title "Fact Resolution" `
    -AssertionFilter `
        "FullyQualifiedName~EntityFactResolutionCertificationTests" `
    -ReadableFilter `
        "FullyQualifiedName~EntityFactResolutionReadableTests"

exit $LASTEXITCODE
