# Build und Prüfungen

Datum: 28. September 2026. Ziel: Meta Quest 3, Version 0.3.1.

| Prüfung | Ergebnis |
| --- | --- |
| Unity 6000.0.66f2 Paketimport | erfolgreich |
| Vollständige C#-Kompilierung im Editor | erfolgreich |
| Unabhängiger App-Kern | 48 Assertions erfolgreich |
| Unity-Netzwerkclient im Play Mode | 32 Assertions erfolgreich |
| Android ARM64 / IL2CPP | erfolgreich gebaut |
| APK-Signatur | gültige Entwicklungssignatur; identisch mit 0.2.0 und 0.3.0 |
| APK-Inhalt | Paketname, Version, ARM64, IL2CPP, GameActivity, VR-Kategorie und Berechtigungen geprüft |
| HTTP-Opt-in | eigene Netzwerkpolicy in der fertigen APK bestätigt; App-Freigabe bleibt erforderlich |
| Installation und Start auf Quest | noch nicht geprüft |
| Handtracking, Raumscan, Ankerpersistenz | noch nicht auf Quest geprüft |
| Android-Keystore-Speicherung | kompiliert; Gerätetest offen |

Der Netzwerktest verwendet einen lokalen Testserver und den echten Unity-Netzwerkclient. Er prüft Anmeldung, JSON-Zustände und Einheit, einen Licht-Schaltbefehl, anschließende Statusaktualisierung, gesperrte Sensor-/Offline-Befehle, Wiederverbindung, ungültige Token und die ausdrückliche HTTP-Freigabe. Es wurde keine reale Home-Assistant-Instanz angesprochen.

Die Tests belegen keine Darstellung, Trackingqualität oder Persistenz auf einer echten Quest. Dafür gilt die [Quest-Abnahmeliste](QUEST-TEST.md).

## APK

- Datei: `HAgoesQuest-0.3.1.apk`
- Paket: `com.alexrosbach.hagoesquest`
- Version: 0.3.1 (Code 4)
- Größe: 65.030.093 Bytes
- SHA-256: `a247aa1b6e0bb16bb62aaaf900f81a36e1d606543850deffc2da900cc6462126`
- Mindest-Android-API: 32; ARM64; Unity IL2CPP; Entwicklungsbuild.
- Android verwendet ausschließlich das neue Unity-Eingabesystem.

Die APK wurde mit `apksigner verify` und `aapt` geprüft. Die Bibliotheken `libil2cpp.so` und `libunity.so` sind enthalten. Das finale Manifest enthält den immersiven VR-Start, Passthrough sowie Handtracking-, Raum- und Ankerberechtigungen. Eine Meta-Vorprüfung meldet beim Build ohne eigenes Quellmanifest eine GameActivity-Warnung; das tatsächlich erzeugte und im APK geprüfte Manifest verwendet korrekt `UnityPlayerGameActivity`.

Version 0.3.1 ergänzt Tests für den tatsächlichen QR-Codec (Erzeugen/Lesen), QR-Datenvalidierung, kombinierte Typ-/Raum-/Suchfilter, Raumzuordnungen nach Status-Polling und das Einblenden kompakter Marker. Die gespeicherte Unity-Szene wurde auf zwei unsichtbare Tracking-Hände, aktives Passthrough und die QR-Komponente geprüft. Das fertige APK enthält die Headset-Kameraberechtigung und `com.oculus.feature.BOUNDARYLESS_APP`.

Update: Wiederverwendung der letzten bestätigten Zugangsdaten, Schutz gegen Serverwechsel und Wiederverbindung mit leerem Tokenfeld im Integrationstest bestätigt. Echtes APK-Update mit Android-Keystore auf Quest noch nicht geprüft.
