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

## Ergänzungen für 0.3.0

- [ ] Echte Hände im Passthrough sichtbar; keine virtuellen Handmodelle darüber.
- [ ] Frei durch mehrere erfasste Räume gehen; Boundaryless-Verhalten auf dem verwendeten Horizon OS prüfen. Anker bleiben am physischen Ort.
- [ ] Typ, HA-Raum und Suchtext kombinieren; leere Trefferliste und „Ohne Raumzuordnung“ prüfen. Danach Filter zurücksetzen.
- [ ] Lampe, Schalter und reine Anzeige platzieren. Schalter hat eigenes Icon. Andere Domänen (z. B. climate/lock) bleiben reine Anzeigen.
- [ ] Außerhalb von 30 cm nur Icon; innerhalb von 30 cm Name/Wert; beim Entfernen Text wieder weg.
- [ ] Fingerstrahl endet am tatsächlich anvisierten Icon, auch bei Sensoren. Pinch schaltet nur Lampen/Schalter, ohne Zielsprung oder Mehrfachauslösung.
- [ ] QR-Scan: Freigabe erlauben/ablehnen, Scan abbrechen, App pausieren, erneut starten. Kamera nur im aktiven Scan.
- [ ] Token-QR und JSON-QR vom Handy lesen; Adresse vor Verbinden prüfen. QR darf keine automatische Verbindung oder HTTP-Freigabe auslösen.
- [ ] Beschädigten QR, normalen Weblink und ungültigen Token prüfen; keine Zugangsdaten in Fehlermeldungen.

## Regressionen ab 0.3.2

- Quest-Systemdialog zum Bearbeiten der Räume öffnen und abschließen/abbrechen. Danach rechte Hand und linken Menü-Pinch prüfen; Menü muss wieder vor dem Kopf stehen.
- Annäherung an ein Icon: Kreis/Rand sichtbar, Größenanstieg zwischen 1,2 m und 30 cm; Text erst innerhalb von 30 cm.
- Platzierung per Zeiger und direkter Handposition jeweils mit einmaligem Pinch einfrieren, verschieben und bestätigen.
- Platzierung im Hauptmenü löschen, auch wenn Raumanker nicht gefunden oder HA-Entität nicht verfügbar ist; nach App-Neustart darf sie nicht zurückkehren.

## Magie ab 0.4.0

- Modus einschalten, App neu starten und gespeicherte Einstellung prüfen.
- Rechte offene Hand zum Icon richten, kurz halten und vorwärts stoßen. Zielrand, einmaligen Schaltbefehl, Feuerball vom Handteller, Schweif und Aufprall prüfen.
- Keine Auslösung durch Gehen, Zurückziehen des Kopfes, seitliches Winken, langsames Hinreichen, geschlossene Hand, Trackingverlust oder gehaltene Hand nach einem Stoß.
- Sensoren, reine Anzeigen, veraltete Zustände, Menüs, Platzierung und Raumscan dürfen keine Magie-Schaltbefehle auslösen.
- Nach Rücknahme/Schließen der Hand und Ablauf der Pause muss ein neuer Stoß möglich sein.

## Regressionen ab 0.4.1

- Räume im Quest-Systemdialog neu erstellen; anschließend mehrere Menüeinträge mit der rechten Hand auswählen.
- Mit rechtem Daumen/Zeigefinger lange die Quest-Neukalibrierung ausführen. Finger lösen; der rechte Zeiger muss weiter auf Icons und Menü zeigen und neue Pinches erkennen.
- Linker Pinch öffnet, nach Loslassen schließt ein weiterer Pinch auch Untermenüs. Gedrückthalten darf das Menü nicht wiederholt umschalten.
- Drei getrennte Vorwärtsstöße mit jeweiligem Zurücknehmen ausführen, ohne auf das Ende der Fluganimation zu warten. Pro Geste maximal ein Befehl, kein automatisch wiederholtes Schalten.

## Tracking und mehrere Räume ab 0.5.0

- Zwei Entitäten in unterschiedlichen Räumen platzieren. App vollständig schließen, in einem anderen Raum mit anderer Blickrichtung neu starten. Icons müssen erst nach verlässlichem Tracking an ihren ursprünglichen realen Orten erscheinen.
- Während des Starts Kameras kurz verdecken; keine Entität darf am Ursprung oder an einer veralteten Position sichtbar oder schaltbar sein. Nach Freigabe umsehen; fehlende Anker werden automatisch erneut gesucht.
- Nach Trackingverlust, langem rechten Kalibrierungs-Pinch sowie Abschluss/Abbruch der Raumerfassung müssen Icons kurz verborgen bleiben und anschließend wieder am realen Ort erscheinen.
- Platzierung einfrieren, dann kalibrieren oder den Systemdialog öffnen. Die alte Vorschau darf anschließend nicht gespeichert werden können; erneut platzieren.
- Durch die Wohnung laufen: räumlich gefundene Icons sollen stehen bleiben. Kopfbewegung darf keine Entitäten in einen anderen Raum verschieben.
- APK als Update installieren: vorhandener Token und Zuordnungen bleiben erhalten. Verlorene Anker dürfen keine neuen Zuordnungen erzeugen oder bestehende UUIDs überschreiben.

## Bedienung und Oberfläche ab 0.5.0

- Rechter ausgestreckter Zeigefinger: Strahl auch ohne Treffer sichtbar, bei Entity-Treffer stärker. Pinchen nach schnellem Zielen, Gedrückthalten, Zielwechsel und verlorenes Tracking prüfen; keine doppelten Befehle.
- ISDK-Antippen von vorne und schräg mit der rechten Fingerspitze; andere Finger und Annäherung von der Rückseite dürfen keine unbeabsichtigten Aktionen auslösen.
- Mindestens 30 Entitäten aus vier HA-Räumen: acht vollständige Zeilen, letzte Zeile erreichbar, laufende Werte aktuell. Pinch-and-drag und direktes Wischen dürfen nicht versehentlich eine Zeile öffnen. Kombinierte Filter prüfen.
- Uhrmodus speichern, App neu starten, Uhr antippen. Panel muss ruhig vor dem Kopf stehen und nicht dem bewegten Arm folgen. Linker Pinch bleibt verwendbar.
- DE/EN umschalten und neu starten; Menüs, Statusmeldungen, Suche, Magie-Hinweise und Sensorzustände prüfen. Individuelle HA-Namen unverändert lassen.
- Adresse, Token und Suche in der Meta-Systemtastatur bearbeiten, bestätigen und abbrechen. Nach jedem Fokuswechsel rechte Hand weiter verwendbar; Token bleibt verdeckt.
- Sensor aus mehreren Metern mittig anschauen: Wert über Icon sichtbar, Wegschauen blendet ihn aus. Reiner Blick darf keine Geräte steuern.
- Dimmbare Lampe und Lautsprecher separat testen: richtigen Regler greifen, seitlich ziehen, loslassen. HA-Wert muss folgen. Ein/Aus-Lampe ohne Dimmfähigkeit darf keinen Helligkeitsregler erhalten.
- Übungsziel für Magie nutzen: keine HA-Befehle; grüne Bereitschaft sichtbar; mehrere Stöße nacheinander. Danach echte Lampe und Schalter prüfen.
- Kamera- und Bildrahmen platzieren, App aus anderem Raum neu starten: Position und Wandorientierung erhalten. HA offline schalten, Berechtigung entziehen und zurückkehren: keine stillschweigend als aktuell ausgegebenen alten Bilder.
- Sounds unterscheiden Lampe/Schalter, lassen sich abschalten und dürfen nach einem fehlgeschlagenen HA-Befehl keine bestätigte Aktion vortäuschen.
- Bildrate und Speicherverbrauch mit vielen Icons und zwei sichtbaren Kamerarahmen über mindestens 30 Minuten auf der echten Quest messen.

## Bedienung ab 0.6.0 (ersetzt die älteren Gestenabläufe)

- [ ] Entität mit dem Punkt in der Kopfmitte auswählen. Handrichtung verändern: Das Ziel darf sich dadurch nicht ändern. Beim Wegschauen muss die Aufhellung verschwinden.
- [ ] Bewusster Pinch schaltet einmal. Eine Faust langsam und schnell schließen: kein unbeabsichtigter Pinch-Befehl. Gehaltene Finger und Fokusverlust erzeugen keine Wiederholung.
- [ ] Lampe und Lautsprecher ansehen, Hand kurz öffnen, Faust schließen und in beide Richtungen drehen. Prozentvorschau prüfen. Erst Öffnen übernimmt den Wert.
- [ ] Während der Drehung wegschauen, Systemmenü öffnen oder Hand verdecken: kein Wert darf durch verlorenes Tracking bestätigt werden.
- [ ] Magie: Bereitschaftsring ansehen, Faust schließen und öffnen. Drei Würfe hintereinander; pro Geste nur ein Befehl. Bei vorheriger Drehung nur Wertübernahme, kein Feuerball.
- [ ] Sensoren aus 0,5 / 1 / 3 / 6 Metern ansehen. Schrift soll ungefähr dieselbe Winkelgröße behalten, lesbar sein und benachbarte Icons nicht dauerhaft verdecken.
- [ ] Uhr links mit rechter Fingerspitze öffnen/schließen, dann Linkshändermodus wählen: Uhr rechts, Tippen/Pinch/Faustbedienung links. Präferenz nach Neustart erhalten.
- [ ] Uhrzeit, Datum und Verbindungsanzeige ohne Überlappung; beim Armbewegen bleibt das geöffnete Menü ortsfest.
- [ ] Hauptkacheln, 3D-Symbole und Beschriftung mit beiden Augen prüfen: keine Doppelbilder, fehlenden Objekte oder Tiefenkonflikte. Hover und Klickfeedback prüfen.
- [ ] Zwei Räume, Kaltstart im anderen Raum und Systemkalibrierung wie oben erneut prüfen. Neue Gesten dürfen die räumliche Zuordnung nicht verändern.
