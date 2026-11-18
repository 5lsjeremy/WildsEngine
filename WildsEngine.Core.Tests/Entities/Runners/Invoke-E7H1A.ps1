$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest
$RunnersRoot = $PSScriptRoot
$EntitiesRoot = Split-Path -Parent $RunnersRoot
$TestsRoot = Split-Path -Parent $EntitiesRoot
$RepoRoot = Split-Path -Parent $TestsRoot
$Project = Join-Path $TestsRoot "WildsEngine.Core.Tests.csproj"
$ReportDir = Join-Path $EntitiesRoot "Reports"
$Timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$Report = Join-Path $ReportDir "E7-H1A-Service-Context-Certification-$Timestamp.txt"
$Latest = Join-Path $ReportDir "E7-H1A-Service-Context-Certification-Latest.txt"
New-Item -ItemType Directory -Path $ReportDir -Force | Out-Null
function Write-Line([string]$Text = "") { Write-Host $Text; $Text | Out-File -LiteralPath $Report -Append -Encoding utf8 }
function Invoke-Logged([string]$Title, [scriptblock]$Command) {
    Write-Line ""; Write-Line "============================================================"; Write-Line $Title; Write-Line "============================================================"
    & $Command 2>&1 | ForEach-Object { Write-Line ([string]$_) }
    if ($LASTEXITCODE -ne 0) { throw "$Title failed with exit code $LASTEXITCODE." }
}
Write-Line "============================================================"
Write-Line "WILDS DISCOVERIES E7-H1A SERVICE / CONTEXT CERTIFICATION"
Write-Line "============================================================"
Write-Line "Started : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
$core = @(git -C $RepoRoot status --short -- .\WildsEngine.Core)
if ($core.Count -gt 0) { $core | ForEach-Object { Write-Line $_ }; throw "Core has changes." }
Write-Line "Core guard: PASS"
Invoke-Logged "E7-H1A RELEASE BUILD" { dotnet build $Project -c Release }
Invoke-Logged "E7-H1A ASSERTION TESTS" { dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~WildsServiceContextCertificationTests" }
Invoke-Logged "E7-H1A READABLE TESTS" { dotnet test $Project -c Release --no-build --filter "FullyQualifiedName~WildsServiceContextReadableTests" --logger "console;verbosity=detailed" }
$coreAfter = @(git -C $RepoRoot status --short -- .\WildsEngine.Core)
if ($coreAfter.Count -gt 0) { $coreAfter | ForEach-Object { Write-Line $_ }; throw "Core changed during H1A." }
Write-Line ""; Write-Line "RESULT: PASS"; Write-Line "Core unchanged. Service/context foundation certified."
Copy-Item -LiteralPath $Report -Destination $Latest -Force
Write-Host "Report : $Report"; Write-Host "Latest : $Latest"
