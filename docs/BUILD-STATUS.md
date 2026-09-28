# Build und Prüfungen

Datum: 28. September 2026. Ziel: Meta Quest 3, Version 0.2.0.

| Prüfung | Ergebnis |
| --- | --- |
| Unity 6000.0.66f2 Paketimport | erfolgreich |
| Vollständige C#-Kompilierung im Editor | erfolgreich |
| Unabhängiger App-Kern | 46 Assertions erfolgreich |
| Unity-Netzwerkclient im Play Mode | 13 Assertions erfolgreich |
| Android ARM64 / IL2CPP | erfolgreich gebaut |
| APK-Signatur | gültige Entwicklungssignatur |
| APK-Inhalt | Paketname, Version, ARM64, IL2CPP, GameActivity, VR-Kategorie und Berechtigungen geprüft |
| HTTP-Opt-in | eigene Netzwerkpolicy in der fertigen APK bestätigt; App-Freigabe bleibt erforderlich |
| Installation und Start auf Quest | noch nicht geprüft |
| Handtracking, Raumscan, Ankerpersistenz | noch nicht auf Quest geprüft |
| Android-Keystore-Speicherung | kompiliert; Gerätetest offen |

Der Netzwerktest verwendet einen lokalen Testserver und den echten Unity-Netzwerkclient. Er prüft Anmeldung, JSON-Zustände und Einheit, einen Licht-Schaltbefehl, anschließende Statusaktualisierung, gesperrte Sensor-/Offline-Befehle, Wiederverbindung, ungültige Token und die ausdrückliche HTTP-Freigabe. Es wurde keine reale Home-Assistant-Instanz angesprochen.

Die Tests belegen keine Darstellung, Trackingqualität oder Persistenz auf einer echten Quest. Dafür gilt die [Quest-Abnahmeliste](QUEST-TEST.md).

## APK

- Datei: `HAgoesQuest-0.2.0.apk`
- Paket: `com.alexrosbach.hagoesquest`
- Version: 0.2.0 (Code 2)
- Größe: 79.464.546 Bytes
- SHA-256: `fa8b2d699eee6cd7b851f8b3777a90363f49d84d19b86b6a5c04effbeb39630a`
- Mindest-Android-API: 32; ARM64; Unity IL2CPP; Entwicklungsbuild.
- Android verwendet ausschließlich das neue Unity-Eingabesystem.

Die APK wurde mit `apksigner verify` und `aapt` geprüft. Die Bibliotheken `libil2cpp.so` und `libunity.so` sind enthalten. Das finale Manifest enthält den immersiven VR-Start, Passthrough sowie Handtracking-, Raum- und Ankerberechtigungen. Eine Meta-Vorprüfung meldet beim Build ohne eigenes Quellmanifest eine GameActivity-Warnung; das tatsächlich erzeugte und im APK geprüfte Manifest verwendet korrekt `UnityPlayerGameActivity`.
