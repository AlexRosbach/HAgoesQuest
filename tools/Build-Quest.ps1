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
Write-Output $apk
