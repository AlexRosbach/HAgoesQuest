# Build und Prüfungen

Datum: 2026-09-29. Ziel: Meta Quest 3, Version 0.6.1.

| Prüfung | Ergebnis |
| --- | --- |
| Unity 6000.0.66f2, Meta XR Core / MRUK / Interaction SDK 207 | Paketimport und C#-Kompilierung erfolgreich |
| Unabhängiger App-Kern | 99 Assertions erfolgreich |
| Unity Play Mode mit lokalem HA-Testserver | 104 Assertions erfolgreich |
| Menüvorschau | mit Unity gerendert und visuell geprüft |
| Android ARM64 / IL2CPP | APK erfolgreich gebaut |
| Lokale SDK-Entwicklerdaten | Aus APK ausgeschlossen; gepackter Asset-Bericht geprüft |
| Signatur | gültig, identisch mit vorherigen Testversionen |
| APK-Manifest | GameActivity, VR, Handtracking, Passthrough, Raum, Anker, Kamera, Boundaryless und Systemtastatur geprüft |
| HTTP-Opt-in | Netzwerkpolicy vorhanden, ausdrückliche Freigabe in der App weiterhin nötig |
| Installation, Gesten und Systemtastatur auf Quest | Gerätetest offen |
| Mehrraum-Lokalisierung / Android-Keystore beim Update | Gerätetest offen |
| Performance und längere Sitzungen | Gerätetest offen |

Neu in 0.6.1 geprüft: Uhrenposition relativ zum bewegten/gedrehten Handgelenk, getrennte Speicherung, Vorschau ohne Überschreiben, Grenzen, Reset, ungültige Daten, Abbruch sowie Textkontrast. Aus 0.6.0 weiterhin geprüft: Blickauswahl ohne Handrichtung, Abbruch beim Wegsehen, Faust/Pinch-Unterscheidung, Drehung und Winkelüberlauf, wiederholte Faust-Öffnen-Würfe, keine Aktion bei Trackingverlust, gespiegelte Bedienhände sowie proportionale Sensorbeschriftung. Die übrigen Tests prüfen unter anderem Meta-SDK-Ray-/Poke-Auswahl, keine wiederholte Aktion beim Halten, Wiederaufnahme nach Trackingverlust, zwei unabhängige gespeicherte Anker-UUIDs, verworfene alte Platzierung nach Neukalibrierung, acht sichtbare Listeneinträge und Scrollen, Slider-Geste und HA-Helligkeits-/Lautstärkeaufrufe, vier HA-Raumzuordnungen, Sprachwechsel sowie authentifizierte Kamera-/Bildabfragen und Bilddekodierung. Frühere Prüfungen für Verbindung, QR, Update-Zugangsdaten, Filtern, Löschen, Marker und Feuerbälle bleiben enthalten. Es wurde keine reale Home-Assistant-Instanz angesprochen.

Diese Nachweise bestätigen keine korrekte Lokalisierung im echten Gebäude. Ein vom Betriebssystem stabil, aber falsch erkannter Anker lässt sich allein über Trackingflags nicht zuverlässig erkennen. Dafür ist die [Quest-Abnahmeliste](QUEST-TEST.md) verbindliche Arbeitsgrundlage.

## APK

- Datei: HAgoesQuest-0.6.1.apk
- Paket: com.alexrosbach.hagoesquest
- Version: 0.6.1, Android-Code 10
- Größe: 68236022 Bytes
- SHA-256: da16dbd6c5cddcfd500a1a906597e93c6bf2448a257e49c3d4fb6010ba642762
- Mindest-Android-API: 32; ARM64; Unity IL2CPP; Entwicklungsbuild.
- Entwicklungssignatur SHA-256: 345caca35c4a1ad2eb8f2243a0ccda712945c87b9b089714f2778c007c76e5b2

Die fertige Datei wurde mit apksigner und aapt geprüft. Paketname, Signatur, Android-Keystore-Alias und gespeicherte Datenformate bleiben für ein Update erhalten. Nicht deinstallieren oder App-Daten löschen. Ein tatsächlich durchgeführtes Update auf Quest ist weiterhin Teil der Geräteabnahme.

Die Meta-Vorprüfung kann ohne eigenes Quellmanifest eine GameActivity-Warnung ausgeben; maßgeblich ist das geprüfte finale APK-Manifest. SDK-Warnungen sind keine Meta-Zertifizierung. GitHub Actions prüft den .NET-App-Kern und baut keine APK.