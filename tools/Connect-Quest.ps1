param(
    [string]$Adb = 'adb',
    [string]$Device,
    [string]$BaseUrl,
    [switch]$AllowInsecureHttp
)
$ErrorActionPreference = 'Stop'
$package = 'com.alexrosbach.hagoesquest'
if (!$BaseUrl) { $BaseUrl = Read-Host 'Home-Assistant-Basis-URL (z. B. https://ha.example.com)' }
$uri = $null
if (![Uri]::TryCreate($BaseUrl, [UriKind]::Absolute, [ref]$uri) -or
    $uri.Scheme -notin @('https', 'http') -or $uri.UserInfo -or $uri.Query -or $uri.Fragment -or $uri.AbsolutePath -ne '/') {
    throw 'Bitte die Basis-URL ohne Pfad, Zugangsdaten oder Query eingeben.'
}
if ($uri.Scheme -eq 'http' -and !$AllowInsecureHttp) {
    throw 'Für eine lokale HTTP-Verbindung erneut mit -AllowInsecureHttp starten. Dabei werden Zugangsdaten unverschlüsselt im Netzwerk übertragen.'
}
$deviceOutput = & $Adb devices
if ($LASTEXITCODE -ne 0) { throw 'ADB ist nicht verfügbar.' }
$devices = @($deviceOutput | Where-Object { $_ -match '^([^\s]+)\s+device$' } | ForEach-Object { ($_ -split '\s+')[0] })
if (!$Device) {
    if ($devices.Count -ne 1) { throw 'Genau eine entsperrte Quest per USB verbinden und USB-Debugging erlauben, oder -Device angeben.' }
    $Device = $devices[0]
}
if ($Device -notin $devices) { throw 'Das ausgewählte Gerät ist nicht für ADB autorisiert.' }
$installed = & $Adb -s $Device shell pm path $package
if ($LASTEXITCODE -ne 0 -or !($installed -match '^package:')) { throw 'Zuerst HAgoesQuest.apk installieren und einmal starten.' }
$secure = Read-Host 'Home Assistant Long-Lived Access Token (Eingabe verborgen)' -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
$temporary = Join-Path ([IO.Path]::GetTempPath()) ('hagoesquest-' + [Guid]::NewGuid().ToString('N') + '.json')
try {
    $token = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    if ([string]::IsNullOrWhiteSpace($token)) { throw 'Token ist leer.' }
    $settings = @{ baseUrl = $uri.AbsoluteUri; accessToken = $token; allowInsecureHttp = $AllowInsecureHttp.IsPresent }
    [IO.File]::WriteAllText($temporary, ($settings | ConvertTo-Json), (New-Object Text.UTF8Encoding($false)))
    $remote = '/sdcard/Android/data/' + $package + '/files'
    & $Adb -s $Device shell am force-stop $package | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'App konnte nicht angehalten werden.' }
    & $Adb -s $Device shell mkdir -p $remote
    if ($LASTEXITCODE -ne 0) { throw 'App-Verzeichnis konnte nicht geöffnet werden. App einmal starten.' }
    & $Adb -s $Device push $temporary ($remote + '/connection.json') | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Verbindungseinstellungen konnten nicht übertragen werden.' }
    & $Adb -s $Device shell monkey -p $package 1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Einstellungen übertragen. App bitte manuell starten.' }
    Write-Host 'Einstellungen übertragen. Verbindungsstatus in der Quest-App prüfen.'
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    $token = $null
    $settings = $null
    if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary }
    $secure.Dispose()
}
