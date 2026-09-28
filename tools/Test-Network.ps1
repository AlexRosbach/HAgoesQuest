param([string]$Unity='U:\Unity\6000.0.66f2\Editor\Unity.exe')
$ErrorActionPreference='Stop'
$project=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$results=Join-Path $project 'TestResults'
New-Item -ItemType Directory -Force $results | Out-Null
$result=Join-Path $results 'network-result.txt'
if(Test-Path -LiteralPath $result){Remove-Item -LiteralPath $result}
$mock=Join-Path $project 'tests\ha-mock.cjs'
$server=Start-Process -FilePath (Get-Command node).Source -ArgumentList ('"'+$mock+'"') -WindowStyle Hidden -PassThru
$previous=$env:HA_QUEST_TEST_RESULT
try{
    $env:HA_QUEST_TEST_RESULT=$result
    $arguments='-batchmode -nographics -projectPath "'+$project+'" -executeMethod HAgoesQuest.Testing.PlaymodeProbe.Run -logFile "'+(Join-Path $results 'Unity-Network.log')+'"'
    $process=Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
    if(!$process.WaitForExit(600000)){
        Stop-Process -Id $process.Id
        throw 'Netzwerktest hat das Zeitlimit überschritten.'
    }
    if($process.ExitCode -ne 0 -or !(Test-Path -LiteralPath $result)){throw 'Unity-Netzwerktest fehlgeschlagen. TestResults/Unity-Network.log prüfen.'}
    Get-Content -LiteralPath $result
    if((Get-Content -Raw -LiteralPath $result) -notmatch '^PASS:'){throw 'Netzwerktest meldet Fehler.'}
}finally{
    $env:HA_QUEST_TEST_RESULT=$previous
    if(!$server.HasExited){Stop-Process -Id $server.Id}
}
