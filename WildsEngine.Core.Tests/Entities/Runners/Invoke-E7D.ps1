$ErrorActionPreference = "Stop"

$Runner = Join-Path `
    $PSScriptRoot `
    "Invoke-E7Gate.ps1"

& $Runner `
    -Gate "E7-D" `
    -Title "Evidence Contribution Policies" `
    -AssertionFilter `
        "(FullyQualifiedName~EntityFactPolicyCertificationTests|FullyQualifiedName~EntityProductionPolicyMatrixCertificationTests)" `
    -ReadableFilter `
        "FullyQualifiedName~EntityFactPolicyReadableTests"

exit $LASTEXITCODE