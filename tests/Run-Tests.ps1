$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$output = Join-Path $projectRoot 'TestResults'
New-Item -ItemType Directory -Force $output | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'Windows .NET Framework C# compiler not found. Use dotnet run --project tests/CoreTests.csproj instead.' }
$exe = Join-Path $output 'CoreTests.exe'
$core = @(Get-ChildItem (Join-Path $projectRoot 'Assets/HAgoesQuest/Core') -Filter '*.cs' | ForEach-Object FullName)
& $compiler /nologo "/out:$exe" $core (Join-Path $PSScriptRoot 'CoreTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Core test compilation failed.' }
& $exe
if ($LASTEXITCODE -ne 0) { throw 'Core tests failed.' }
