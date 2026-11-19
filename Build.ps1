param([string]$ServerDir = "$env:ProgramFiles\Jellyfin\Server")
$ErrorActionPreference = 'Stop'
$ServerDir = (Resolve-Path $ServerDir).Path
$config = Get-Content (Join-Path $ServerDir 'jellyfin.runtimeconfig.json') -Raw | ConvertFrom-Json
$target = $config.runtimeOptions.tfm
if (!$target) { throw 'Could not determine the server target framework.' }
if (!(Get-Command dotnet -ErrorAction SilentlyContinue)) { throw "Install the .NET SDK for $target first." }
& dotnet build "$PSScriptRoot\Tracker.csproj" -c Release "-p:ServerDir=$ServerDir" "-p:ServerFramework=$target" -o "$PSScriptRoot\output"
if ($LASTEXITCODE -ne 0) { throw 'Build failed. No installation was performed.' }
Write-Host "Plugin DLL: $PSScriptRoot\output\Dreamstreaming.SubtitleTracker.dll"
