param([string]$Unity = 'U:\Unity\6000.0.66f2\Editor\Unity.exe', [string]$CacheRoot = '')
$ErrorActionPreference='Stop'
if ($CacheRoot) {
    New-Item -ItemType Directory -Force $CacheRoot | Out-Null
    $env:GRADLE_USER_HOME = Join-Path $CacheRoot 'Gradle'
    $env:TEMP = Join-Path $CacheRoot 'Temp'
    $env:TMP = $env:TEMP
    New-Item -ItemType Directory -Force $env:TEMP | Out-Null
}
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if(!(Test-Path -LiteralPath $Unity)){throw 'Unity-Editor nicht gefunden. -Unity mit dem vollständigen Pfad angeben.'}
$androidTools=Join-Path (Split-Path $Unity) 'Data/PlaybackEngines/AndroidPlayer'
$env:JAVA_HOME=Join-Path $androidTools 'OpenJDK'
$expectedCertificate='345caca35c4a1ad2eb8f2243a0ccda712945c87b9b089714f2778c007c76e5b2'
$signingKey=Join-Path $env:USERPROFILE '.android/debug.keystore'
if (!(Test-Path -LiteralPath $signingKey)) { throw 'Vorhandener Signaturschlüssel fehlt. Aus privater Sicherung wiederherstellen; keinen neuen Schlüssel erzeugen.' }
$certificate=& (Join-Path $env:JAVA_HOME 'bin/keytool.exe') -exportcert -keystore $signingKey -storepass android -alias androiddebugkey -rfc 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Signaturschlüssel kann nicht geprüft werden.' }
$encoded=($certificate -join '') -replace '-----BEGIN CERTIFICATE-----|-----END CERTIFICATE-----|\s',''
$digest=[BitConverter]::ToString([Security.Cryptography.SHA256]::Create().ComputeHash([Convert]::FromBase64String($encoded))).Replace('-','').ToLowerInvariant()
if ($digest -ne $expectedCertificate) { throw 'Signatur passt nicht zu 0.2.0/0.3.0. Build abgebrochen, damit Updates kompatibel bleiben.' }
$results=Join-Path $project 'TestResults'
New-Item -ItemType Directory -Force $results | Out-Null
foreach($step in @(@('Prepare','HAgoesQuest.Editor.ProjectSetup.PrepareBatch'),@('Build','HAgoesQuest.Editor.ProjectSetup.Build'))){
    $log=Join-Path $results ('Unity-'+$step[0]+'.log')
    $arguments='-batchmode -nographics -quit -buildTarget Android -projectPath "'+$project+'" -executeMethod '+$step[1]+' -logFile "'+$log+'"'
    $process=Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
    $process.WaitForExit()
    if($process.ExitCode -ne 0){throw ('Unity '+$step[0]+' fehlgeschlagen. Log: '+$log)}
}
$apk=Join-Path $project 'Builds\HAgoesQuest.apk'
if(!(Test-Path -LiteralPath $apk)){throw 'Unity meldet keinen Fehler, aber die APK fehlt.'}
$apksigner=Join-Path $androidTools 'SDK/build-tools/36.0.0/apksigner.bat'
$signature=& $apksigner verify --print-certs $apk
if ($LASTEXITCODE -ne 0 -or ($signature -join "`n") -notmatch [regex]::Escape($expectedCertificate)) { throw 'APK hat keine gültige, kompatible Update-Signatur.' }
Write-Output $apk
