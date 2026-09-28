# Build und Prüfungen

Datum: 28. September 2026. Ziel: Meta Quest 3, Version 0.4.1.

| Prüfung | Ergebnis |
| --- | --- |
| Unity 6000.0.66f2 Paketimport | erfolgreich |
| Vollständige C#-Kompilierung im Editor | erfolgreich |
| Unabhängiger App-Kern | 68 Assertions erfolgreich |
| Unity-Netzwerkclient im Play Mode | 54 Assertions erfolgreich |
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

- Datei: `HAgoesQuest-0.4.1.apk`
- Paket: `com.alexrosbach.hagoesquest`
- Version: 0.4.1 (Code 7)
- Größe: 65.134.367 Bytes
- SHA-256: `822434d28328aca084bba608f67588f46214bfc105741fd1adac746121369a6e`
- Mindest-Android-API: 32; ARM64; Unity IL2CPP; Entwicklungsbuild.
- Android verwendet ausschließlich das neue Unity-Eingabesystem.

Die APK wurde mit `apksigner verify` und `aapt` geprüft. Die Bibliotheken `libil2cpp.so` und `libunity.so` sind enthalten. Das finale Manifest enthält den immersiven VR-Start, Passthrough sowie Handtracking-, Raum- und Ankerberechtigungen. Eine Meta-Vorprüfung meldet beim Build ohne eigenes Quellmanifest eine GameActivity-Warnung; das tatsächlich erzeugte und im APK geprüfte Manifest verwendet korrekt `UnityPlayerGameActivity`.

Version 0.3.1 ergänzt Tests für den tatsächlichen QR-Codec (Erzeugen/Lesen), QR-Datenvalidierung, kombinierte Typ-/Raum-/Suchfilter, Raumzuordnungen nach Status-Polling und das Einblenden kompakter Marker. Die gespeicherte Unity-Szene wurde auf zwei unsichtbare Tracking-Hände, aktives Passthrough und die QR-Komponente geprüft. Das fertige APK enthält die Headset-Kameraberechtigung und `com.oculus.feature.BOUNDARYLESS_APP`.

Update: Wiederverwendung der letzten bestätigten Zugangsdaten, Schutz gegen Serverwechsel und Wiederverbindung mit leerem Tokenfeld im Integrationstest bestätigt. Echtes APK-Update mit Android-Keystore auf Quest noch nicht geprüft.

0.3.2: Zusätzliche Unity-Prüfungen für wachsende runde Marker, persistentes Löschen ohne lokalisierten Raumanker, Einfrieren/Korrigieren/Speichern einer Vorschau, Löschen aus dem Verwaltungsmenü und Neuausrichtung nach Fokuswechsel. Der Wiederholungsbuild beendet Unity erfolgreich; die erste APK-Erzeugung war erfolgreich, der erste Editor-Prozess meldete anschließend ein Beendigungs-Timeout. Finale APK und Signatur erneut geprüft. Der tatsächliche Quest-Systemdialog und die Handverfolgung sind weiterhin Hardwareprüfungen.

0.4.0: 62 Kernprüfungen inklusive Zielwechsel, Vorwärtsstoß, seitlichem/langsamem/rückwärtigem Bewegen, Tracking-Sprung, Latch und Wiederfreigabe. 48 Unity-Prüfungen inklusive OVR/OpenXR-Handgeometrie, gespeicherter Magie-Einstellung und tatsächlichen Feuerball-Objekten (Flug, Aufprallring, Aufräumen). Generierte PNG mit echtem Alphakanal geprüft; für Quest auf maximal 512 Pixel importiert. ARM64-Build und Signaturprüfung erfolgreich; Editor sauber beendet. Geste und Effektwirkung auf echter Quest bleiben offen.

0.4.1: 68 Kernprüfungen und 54 Unity-Prüfungen bestanden. Ungebundene Zeigerpose reproduziert den Koordinatenversatz; nach Zuordnung zum TrackingSpace trifft der Physics-Strahl nach Translation und Rotation weiterhin ein Weltmenü. Szenendatei enthält beide PointerPose-Root-Verweise. Menüwechsel, drei Stoßgesten nacheinander und gleichzeitig laufende Feuerbälle geprüft. APK-Build und kompatible Signatur erfolgreich. Reale Quest-Neukalibrierung weiterhin als Gerätetest offen.
