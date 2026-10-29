$ErrorActionPreference = 'Stop'

$Root = 'C:\Users\jmatt\source\WildsEngine'
$Project = Join-Path $Root 'WildsEngine.Qsl'

$folders = @(
    $Project,
    (Join-Path $Project 'Adapters'),
    (Join-Path $Project 'Abstractions'),
    (Join-Path $Project 'Processing')
)

foreach ($folder in $folders) {
    if (-not (Test-Path $folder)) {
        New-Item -ItemType Directory -Path $folder | Out-Null
        Write-Host "[CREATE] $folder"
    }
    else {
        Write-Host "[EXISTS] $folder"
    }
}

Write-Host ''
Write-Host 'WildsEngine.Qsl companion folder structure is ready.'
Write-Host 'No Core files were modified.'
Write-Host 'No project references were added.'
