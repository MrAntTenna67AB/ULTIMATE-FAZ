$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$games = Get-Content -LiteralPath (Join-Path $root 'sheets\games.json') -Raw | ConvertFrom-Json
$anim = Get-Content -LiteralPath (Join-Path $root 'sheets\animatronics.json') -Raw | ConvertFrom-Json
$attractions = Get-Content -LiteralPath (Join-Path $root 'sheets\attractions.json') -Raw | ConvertFrom-Json
$systems = Get-Content -LiteralPath (Join-Path $root 'sheets\systems.json') -Raw | ConvertFrom-Json
$gameIds = @($games.games | ForEach-Object id)
$errors = [System.Collections.Generic.List[string]]::new()
foreach($g in $games.games) {
  foreach($field in @('id','name','steamAppId','exe','role','use')) { if([string]::IsNullOrWhiteSpace([string]$g.$field)) { $errors.Add("games.$($g.id): missing $field") } }
  if($null -eq $g.animatronicUnlockCost -or $g.animatronicUnlockCost -lt 0) { $errors.Add("games.$($g.id): animatronic category unlock cost must be zero or greater") }
  if($g.role -notin @('secondary-unlisted','custom') -and [string]::IsNullOrWhiteSpace([string]$g.slug)) { $errors.Add("games.$($g.id): missing Melty slug") }
  if($g.role -eq 'custom') {
    if($g.steamAppId -ne 'custom' -or $null -ne $g.slug) { $errors.Add("games.$($g.id): custom roster groups must not claim a real game") }
  } elseif($g.steamAppId -notmatch '^\d+$') { $errors.Add("games.$($g.id): bad Steam app id") }
}
foreach($a in $anim.animatronics) {
  foreach($field in @('id','name','gameId','rule','action')) { if([string]::IsNullOrWhiteSpace([string]$a.$field)) { $errors.Add("animatronics.$($a.id): missing $field") } }
  if($a.gameId -notin $gameIds) { $errors.Add("animatronics.$($a.id): unresolved game $($a.gameId)") }
  if($a.cost -lt 0 -or ($a.action -ne 'puppet' -and ($a.firstAttackSeconds -lt 1 -or $a.warningSeconds -lt 1))) { $errors.Add("animatronics.$($a.id): invalid numeric column") }
  if($a.action -notin @('door','foxy','golden','tenna','mask','lure','listen','scan','mangle','puppet','debate')) { $errors.Add("animatronics.$($a.id): unknown action $($a.action)") }
  if($a.action -in @('door','listen') -and $a.doorSide -notin @('left','right')) { $errors.Add("animatronics.$($a.id): door threat must name left or right side") }
  foreach($field in @('atmosphere','healthSafety','entertainment','bonusRevenue')) { if($null -eq $a.$field -or ($field -ne 'healthSafety' -and $a.$field -lt 0)) { $errors.Add("animatronics.$($a.id): missing or invalid rating field $field") } }
  if(-not (Test-Path -LiteralPath (Join-Path $root "assets\portraits\$($a.id).png"))) { $errors.Add("animatronics.$($a.id): missing matching portrait asset assets/portraits/$($a.id).png") }
}
foreach($a in $attractions.attractions) {
  foreach($field in @('id','name','category','description','style')) { if([string]::IsNullOrWhiteSpace([string]$a.$field)) { $errors.Add("attractions.$($a.id): missing $field") } }
  if($a.category -notin @('dumpster','budget-tech','smiles','rare')) { $errors.Add("attractions.$($a.id): unknown catalog category $($a.category)") }
  if($a.cost -lt 0 -or $a.atmosphere -lt 0 -or $a.healthSafety -lt 0 -or $a.entertainment -lt 0 -or $a.bonusRevenue -lt 0 -or $a.liabilityRisk -lt 0) { $errors.Add("attractions.$($a.id): invalid cost or pizzeria rating") }
  if(-not (Test-Path -LiteralPath (Join-Path $root "assets\attractions\$($a.id).png"))) { $errors.Add("attractions.$($a.id): missing Catalog Mode gallery picture assets/attractions/$($a.id).png") }
}
foreach($s in $systems.systems) {
  foreach($field in @('id','behavior')) { if([string]::IsNullOrWhiteSpace([string]$s.$field)) { $errors.Add("systems.$($s.id): missing $field") } }
  foreach($ref in $s.gameRefs) { if($ref -notin $gameIds) { $errors.Add("systems.$($s.id): unresolved game $ref") } }
  if($s.fields.Count -eq 0) { $errors.Add("systems.$($s.id): no property columns") }
}
if(@($games.games | Where-Object role -ne 'custom').Count -ne 6) { $errors.Add('Expected six real game source rows.') }
if(@($games.games | Where-Object role -eq 'custom').Count -ne 1) { $errors.Add('Expected one custom animatronic roster group.') }
if($anim.defaultAiLevel -lt 0 -or $anim.defaultAiLevel -gt $anim.maximumAiLevel) { $errors.Add('Invalid default AI level bounds.') }
if($anim.startingFunds -ne 200) { $errors.Add('The agreed starting budget must be $200.') }
if('freddy' -notin @($anim.animatronics | ForEach-Object id)) { $errors.Add('The starter Freddy animatronic must remain in the roster.') }
foreach($required in @('source-scan','pizzeria-day','custom-night','power-and-vents','mask-and-listening','day-night-loop')) { if($required -notin @($systems.systems | ForEach-Object id)) { $errors.Add("Missing system sheet row $required") } }
if($errors.Count) { $errors | ForEach-Object { Write-Error $_ }; exit 1 }
Write-Host "Preflight clean: $($games.games.Count) game rows, $($anim.animatronics.Count) animatronics, $($systems.systems.Count) systems; all references resolve."
dotnet publish (Join-Path $root 'src\FazbearPizzeria.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o (Join-Path $root 'build')
if($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE" }
$publishSheets = Join-Path $root 'build\sheets'
New-Item -ItemType Directory -Force -Path $publishSheets | Out-Null
Copy-Item -Path (Join-Path $root 'sheets\*.json') -Destination $publishSheets -Force
foreach($sheet in @('games.json','animatronics.json','attractions.json','systems.json')) {
  if(-not (Test-Path -LiteralPath (Join-Path $publishSheets $sheet))) { throw "Published data sheet missing: $sheet" }
}
if(Test-Path -LiteralPath (Join-Path $root 'assets\portraits')) {
  $publishPortraits = Join-Path $root 'build\assets\portraits'
  New-Item -ItemType Directory -Force -Path $publishPortraits | Out-Null
  Copy-Item -Path (Join-Path $root 'assets\portraits\*') -Destination $publishPortraits -Force
}
if(Test-Path -LiteralPath (Join-Path $root 'assets\attractions')) {
  $publishAttractions = Join-Path $root 'build\assets\attractions'
  New-Item -ItemType Directory -Force -Path $publishAttractions | Out-Null
  Copy-Item -Path (Join-Path $root 'assets\attractions\*.png') -Destination $publishAttractions -Force
}
if(Test-Path -LiteralPath (Join-Path $root 'sound-bank')) {
  $publishSounds = Join-Path $root 'build\sound-bank'
  New-Item -ItemType Directory -Force -Path $publishSounds | Out-Null
  Copy-Item -Path (Join-Path $root 'sound-bank\*.wav') -Destination $publishSounds -Force
  foreach($requiredSound in @('CAMERA_VIDEO_LOA_60105303.wav','blip3.wav','main menu theme.wav','office ambience2.wav','on a cam.wav','put down monitor.wav','door closing.wav','door lights on sound.wav','animatronic moving from a cam.wav','animatronic spotted at the lights.wav','fnaf one jumpscare.wav','fnaf one golden freddy jumpscare.wav','when hitting 6 am (plays beffore the winning a night one).wav','winning a night.wav','Buzz_Fan_Florescent2 (plays at a low sound in the office).wav')) {
    if(-not (Test-Path -LiteralPath (Join-Path $publishSounds $requiredSound))) { throw "Published sound missing: $requiredSound" }
  }
}
foreach($a in $attractions.attractions) { if(-not (Test-Path -LiteralPath (Join-Path $root "build\assets\attractions\$($a.id).png"))) { throw "Published catalog picture missing: $($a.id).png" } }
Write-Host 'Published app, data sheets, portraits, attraction art, and sound bank to build/.'
