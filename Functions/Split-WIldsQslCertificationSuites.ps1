[CmdletBinding()]
param(
    [Parameter()]
    [string]$RepositoryRoot = (Get-Location).Path,

    [Parameter()]
    [string]$CertificationRelativePath = "WildsEngine.Core.Tests\Certification"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$certificationPath = Join-Path $RepositoryRoot $CertificationRelativePath
$sourcePath = Join-Path $certificationPath "qsl_statement_testing.json"
$corePath = Join-Path $certificationPath "qsl_core_statement_testing.json"
$consumerDirectory = Join-Path $certificationPath "Consumer"
$consumerPath = Join-Path $consumerDirectory "qsl_consumer_statement_testing.json"
$backupPath = Join-Path $certificationPath "qsl_statement_testing.pre-split.json"

if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw "Certification suite was not found: $sourcePath"
}

New-Item -ItemType Directory -Path $consumerDirectory -Force | Out-Null

if (-not (Test-Path -LiteralPath $backupPath -PathType Leaf)) {
    Copy-Item -LiteralPath $sourcePath -Destination $backupPath
}

$suite = Get-Content -LiteralPath $sourcePath -Raw | ConvertFrom-Json
$tests = @($suite.tests)

if ($tests.Count -eq 0) {
    throw "Certification suite contains no tests."
}

$coreTests = @(
$tests | Where-Object {
    $dependencyProperty =
    $_.PSObject.Properties["dependency"]

    $dependencyProperty -eq $null -or
            [string]::IsNullOrWhiteSpace(
                    [string]$dependencyProperty.Value)
}
)

$consumerTests = @(
$tests | Where-Object {
    $dependencyProperty =
    $_.PSObject.Properties["dependency"]

    $dependencyProperty -ne $null -and
            -not [string]::IsNullOrWhiteSpace(
                    [string]$dependencyProperty.Value)
}
)

if (($coreTests.Count + $consumerTests.Count) -ne $tests.Count) {
    throw "Split accounting failed. Core and consumer totals do not match the source total."
}

if ($consumerTests.Count -eq 0) {
    throw "No dependency-declared consumer tests were found."
}

$coreDocument = [ordered]@{
    suite = "WildsQslCore"
    tests = $coreTests
}

$consumerDocument = [ordered]@{
    suite = "WildsQslConsumer"
    dependencies = @(
    $consumerTests |
            ForEach-Object {
                [string]$_.PSObject.Properties["dependency"].Value
            } |
            Sort-Object -Unique
    )
    tests = $consumerTests
}

$coreJson = $coreDocument | ConvertTo-Json -Depth 100
$consumerJson = $consumerDocument | ConvertTo-Json -Depth 100

Set-Content -LiteralPath $corePath -Value $coreJson -Encoding utf8
Set-Content -LiteralPath $consumerPath -Value $consumerJson -Encoding utf8

$coreVerification = Get-Content -LiteralPath $corePath -Raw | ConvertFrom-Json
$consumerVerification = Get-Content -LiteralPath $consumerPath -Raw | ConvertFrom-Json

$coreCount = @($coreVerification.tests).Count
$consumerCount = @($consumerVerification.tests).Count

if ($coreCount -ne $coreTests.Count) {
    throw "Core-suite verification failed. Expected $($coreTests.Count), found $coreCount."
}

if ($consumerCount -ne $consumerTests.Count) {
    throw "Consumer-suite verification failed. Expected $($consumerTests.Count), found $consumerCount."
}

Write-Host ""
Write-Host "Wilds / QSL certification suites split successfully." -ForegroundColor Green
Write-Host "Source total:     $($tests.Count)"
Write-Host "Core tests:       $coreCount"
Write-Host "Consumer tests:   $consumerCount"
Write-Host ""
Write-Host "Core suite:       $corePath"
Write-Host "Consumer suite:   $consumerPath"
Write-Host "Source backup:    $backupPath"
Write-Host ""
Write-Host "Consumer dependencies:"
@($consumerVerification.dependencies) | ForEach-Object {
    Write-Host "  - $_"
}
