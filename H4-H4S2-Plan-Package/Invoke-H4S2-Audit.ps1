param([string]$RepoRoot=(Get-Location).Path)
$ErrorActionPreference="Stop";Set-StrictMode -Version Latest
$RepoRoot=(Resolve-Path $RepoRoot).Path;$D=Join-Path $RepoRoot "WildsEngine.Discoveries";$T=Join-Path $RepoRoot "WildsEngine.Core.Tests";$Out=Join-Path $T "Entities\Reports\H4S2-Surface-Requirements-Latest.txt"
if(@(git -C $RepoRoot status --short -- .\WildsEngine.Core).Count -gt 0){throw "Core has changes."}
$files=Get-ChildItem $D -Recurse -File -Filter *.cs|Sort-Object FullName
"H4S2 SURFACE REQUIREMENTS AUDIT"|Out-File $Out -Encoding utf8
"Repo: $RepoRoot"|Out-File $Out -Append -Encoding utf8
"Classification: DERIVED | SUPPLIED_DEFINITION | SUPPLIED_POLICY | SUPPLIED_METRIC | SUPPLIED_SERVICE | OPTION | RESULT | INTERNAL"|Out-File $Out -Append -Encoding utf8
foreach($f in $files){$m=Select-String -Path $f.FullName -Pattern @("Narrative","Population","Recognition","Profile","Policy","Metric","Relationship","Surface","Registration","Truth","Consequence") -SimpleMatch;if($m){"`nFILE: $($f.FullName.Substring($RepoRoot.Length).TrimStart('\'))"|Out-File $Out -Append -Encoding utf8;Get-Content $f.FullName|Out-File $Out -Append -Encoding utf8}}
"`nREQUIRED H4S2 QUESTIONS"|Out-File $Out -Append -Encoding utf8
"1. Which H4 values are derivable from certified geometry?"|Out-File $Out -Append -Encoding utf8
"2. Which H4 meanings/relationships must be supplied by the designer?"|Out-File $Out -Append -Encoding utf8
"3. Which decisions require policies?"|Out-File $Out -Append -Encoding utf8
"4. Which measurements require consumer metrics?"|Out-File $Out -Append -Encoding utf8
"5. Which supplied items require profile + immutable registration identity?"|Out-File $Out -Append -Encoding utf8
"6. Which results must retain source recognition and configuration provenance?"|Out-File $Out -Append -Encoding utf8
Write-Host "H4S2 audit: $Out" -ForegroundColor Green
