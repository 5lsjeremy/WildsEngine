$ErrorActionPreference = "Stop"

$Runner = Join-Path `
    $PSScriptRoot `
    "Invoke-E7Gate.ps1"

& $Runner `
    -Gate "E7-A" `
    -Title "Native Surface Capture" `
    -AssertionFilter `
        "FullyQualifiedName~EntityNativeSurfaceCertificationTests" `
    -ReadableFilter `
        "FullyQualifiedName~EntityNativeSurfaceReadableTests"

exit $LASTEXITCODE