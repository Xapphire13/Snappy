$ErrorActionPreference = "Stop"
$ASSEMBLY_INFO_PATH = Resolve-Path src\Properties\AssemblyInfo.cs

if (@($(git tag -l --points-at HEAD)).Contains("latest")) {
  exit # No changes
}

$currentVersion = $(Select-String -Pattern "AssemblyVersion\(`"(.+)`"\)" -Path $ASSEMBLY_INFO_PATH).Matches.Groups[1].Value.Split(".")
$bump = conventional-recommended-bump -p angular

switch ($bump) {
  "major" { $currentVersion[0] = 1 + $currentVersion[0] }
  "minor" { $currentVersion[1] = 1 + $currentVersion[1] }
  "patch" { $currentVersion[2] = 1 + $currentVersion[2] }
}

$newVersion = $currentVersion -join "."
(Get-Content $ASSEMBLY_INFO_PATH) -replace "AssemblyVersion\(.+\)","AssemblyVersion(`"$newVersion`")" | Set-Content -Encoding UTF8 -Path $ASSEMBLY_INFO_PATH
(Get-Content $ASSEMBLY_INFO_PATH) -replace "AssemblyFileVersion\(.+\)","AssemblyFileVersion(`"$newVersion`")" | Set-Content -Encoding UTF8 -Path $ASSEMBLY_INFO_PATH

"{`"version`": `"$newVersion`"}" | Set-Content package.json

standard-changelog
Remove-Item package.json
git add :/
git commit -m "release: v$newVersion"
git tag --force "v$newVersion"
git tag --force latest
