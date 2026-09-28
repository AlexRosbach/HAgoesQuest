# Abnahme auf der Quest 3

Desktop-Prüfungen sind unten markiert. **Alle Gerätetests sind noch offen.** Die automatischen Tests ersetzen die Abnahme in der Brille nicht.

## Build und erster Start

- [x] Unity importiert alle festgelegten Pakete ohne Compile-Fehler.
- [x] OpenXR-Buildvalidierung und Android-ARM64/IL2CPP-Build sind erfolgreich.
- [x] Signatur und erforderliche Einträge im fertigen APK-Manifest wurden geprüft; siehe BUILD-STATUS.md.
- [ ] APK wird auf der Quest installiert und als immersive App gestartet.
- [ ] Reale Umgebung ist im Passthrough sichtbar; UI ist mit beiden Augen gut lesbar.
- [ ] Ohne gespeicherte Verbindung erscheinen klar markierte Demo-Daten; echte Geräte werden nicht angesprochen.
- [ ] Linker Pinch/X öffnet ein im Raum stehendes Menü; Rechts-Pinch/Trigger wählt dessen Buttons.

## Raum und Persistenz

- [ ] Ablehnen der Raumberechtigung ergibt verständlichen Hinweis, keinen Absturz.
- [ ] „Raum laden / erstmals scannen“ lädt vorhandenen Raum oder startet die Erfassung.
- [ ] „Raum neu scannen“ öffnet die Erfassung auch bei vorhandenem Raum; Abbrechen ist möglich.
- [ ] Scanfläche-Platzierung zeigt bei fehlendem Treffer keine speicherbare Position.
- [ ] Freie Platzierung funktioniert einen Meter vor dem Headset.
- [ ] Handplatzierung: Greifen, Bewegen, Loslassen und Bestätigen funktionieren. Trackingverlust speichert keine Position.
- [ ] Wohnungskarte zeigt Raumgrenzen, aktuelle Position, Laufweg und lokalisierte Geräte.
- [ ] Generierte Symbole, Fokusanimation und Zustandsrückmeldung sind gut lesbar und flüssig.
- [ ] Lampen- und Sensorzuordnung bleiben nach App-Neustart sowie Headset-Neustart am physischen Ort.
- [ ] Bei nicht wiedergefundenem Anker erscheint kein geratenes Ersatzobjekt; erneutes Laden im richtigen Raum funktioniert.
- [ ] Löschen einer Zuordnung entfernt Anzeige und gespeicherten Anker; nach Neustart bleibt sie gelöscht.
- [ ] Erneuter Raumscan, Rezentrierung und Wechsel in einen anderen Raum verursachen keine falsche Gerätezuordnung.

## Home Assistant

- [ ] Verbindung über HTTPS mit gültigem Zertifikat funktioniert.
- [ ] URL und Token lassen sich in der Brille eingeben/einfügen; der Token bleibt verdeckt.
- [ ] Verbindung bleibt nach Neustart erhalten; nur verschlüsselte Zugangsdaten liegen im App-Verzeichnis.
- [ ] Verbindung vergessen löscht Zugangsdaten und schaltet zurück zur Demo.
- [ ] Serverwechsel zeigt nur die zu diesem Server gehörenden räumlichen Zuordnungen.
- [ ] Als reine Anzeige platzierte Lampe schaltet bei einer Geste nicht.
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
