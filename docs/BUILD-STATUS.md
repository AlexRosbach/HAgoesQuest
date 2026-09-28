# Build und Prüfungen

Datum: 28. September 2026. Ziel: Meta Quest 3, Version 0.2.0.

| Prüfung | Ergebnis |
| --- | --- |
| Unity 6000.0.66f2 Paketimport | erfolgreich |
| Vollständige C#-Kompilierung im Editor | erfolgreich |
| Unabhängiger App-Kern | 46 Assertions erfolgreich |
| Unity-Netzwerkclient im Play Mode | 13 Assertions erfolgreich |
| Android ARM64 / IL2CPP | Build läuft |
| Installation und Start auf Quest | noch nicht geprüft |
| Handtracking, Raumscan, Ankerpersistenz | noch nicht auf Quest geprüft |
| Android-Keystore-Speicherung | kompiliert; Gerätetest offen |

Der Netzwerktest verwendet einen lokalen Testserver und den echten Unity-Netzwerkclient. Er prüft Anmeldung, JSON-Zustände und Einheit, einen Licht-Schaltbefehl, anschließende Statusaktualisierung, gesperrte Sensor-/Offline-Befehle, Wiederverbindung, ungültige Token und die ausdrückliche HTTP-Freigabe. Es wurde keine reale Home-Assistant-Instanz angesprochen.

Die Tests belegen keine Darstellung, Trackingqualität oder Persistenz auf einer echten Quest. Dafür gilt die [Quest-Abnahmeliste](QUEST-TEST.md).
