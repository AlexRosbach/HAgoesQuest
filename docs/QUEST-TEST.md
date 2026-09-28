# Abnahme auf der Quest 3

Status bei Erstellung: **alle folgenden Punkte noch offen**. Die automatischen Core-Tests ersetzen keinen dieser Schritte.

## Build und erster Start

- [ ] Unity importiert alle festgelegten Pakete ohne Compile-Fehler.
- [ ] Android/OpenXR Project Validation und erforderliche Meta-Project-Setup-Prüfungen sind erfolgreich.
- [ ] ARM64-IL2CPP-APK wird gebaut, installiert und als immersive App gestartet.
- [ ] Reale Umgebung ist im Passthrough sichtbar; UI ist mit beiden Augen gut lesbar.
- [ ] Fehlende Verbindungsdatei ergibt klar markierte Demo-Daten; echte Geräte werden nicht angesprochen.
- [ ] Linker Pinch/X öffnet ein im Raum stehendes Menü; Rechts-Pinch/Trigger wählt dessen Buttons.

## Raum und Persistenz

- [ ] Ablehnen der Raumberechtigung ergibt verständlichen Hinweis, keinen Absturz.
- [ ] „Raum laden / erstmals scannen“ lädt vorhandenen Raum oder startet die Erfassung.
- [ ] „Raum neu scannen“ öffnet die Erfassung auch bei vorhandenem Raum; Abbrechen ist möglich.
- [ ] Scanfläche-Platzierung zeigt bei fehlendem Treffer keine speicherbare Position.
- [ ] Freie Platzierung funktioniert bei 0,5 / 1 / 2 Metern.
- [ ] Lampen- und Sensorzuordnung bleiben nach App-Neustart sowie Headset-Neustart am physischen Ort.
- [ ] Bei nicht wiedergefundenem Anker erscheint kein geratenes Ersatzobjekt; erneutes Laden im richtigen Raum funktioniert.
- [ ] Löschen einer Zuordnung entfernt Anzeige und gespeicherten Anker; nach Neustart bleibt sie gelöscht.
- [ ] Erneuter Raumscan, Rezentrierung und Wechsel in einen anderen Raum verursachen keine falsche Gerätezuordnung.

## Home Assistant

- [ ] Verbindung über HTTPS mit gültigem Zertifikat funktioniert.
- [ ] Lokales HTTP funktioniert nur mit ausdrücklicher Freigabe in der Konfiguration.
- [ ] Ungültiger Token zeigt Zugriffsfehler. Keine Secrets stehen im Unity-/Gerätelog.
- [ ] Authentische Geräteliste erscheint; lange Namen und größere Listen bleiben bedienbar.
- [ ] Licht/Schalter anvisieren und danach pinchen führt zu genau einem korrekten Service-Aufruf.
- [ ] Früher/gehältener Pinch, Blickwechsel bei gehaltenem Pinch und Rückkehr nach Trackingverlust schalten nicht versehentlich.
- [ ] Sensoren zeigen Einheit und aktualisierte Werte. Sensoren und nicht verfügbare Geräte sind nicht schaltbar.
- [ ] Extern in HA geschaltete Lampe aktualisiert ihre Anzeige nach der nächsten Abfrage.
- [ ] WLAN/HA-Ausfall: Werte werden als veraltet angezeigt, Befehle gesperrt. Verbindung erholt sich ohne nachträgliche Schaltbefehle.
- [ ] Verlorene Antwort auf Schaltbefehl: Befehl wird nicht automatisch wiederholt; Status wird erneut gelesen.
- [ ] App pausieren/fortsetzen und Systemmenü öffnen erzeugt keine unbeabsichtigte Aktion.

Beim Test Ergebnisse, Unity-Version, Quest-OS-Version, Datum und etwaige Fehler ergänzen. Vor Nutzung als tägliche Haussteuerung diese Liste abschließen.
