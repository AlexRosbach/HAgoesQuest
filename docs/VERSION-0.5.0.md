# 0.5.0 – Bedienung, Räume und Medien

Diese Version bündelt die Trackingkorrektur und die anschließenden Erweiterungswünsche. Die unveröffentlichte Bezeichnung 0.4.2 entfällt. Android-Versionscode: 8; Paketname und Entwicklungssignatur bleiben unverändert.

## Bedienung

- **Rechte Hand:** Zeigefinger ausstrecken. Der Strahl bleibt auch ohne Treffer sichtbar; bei einem Entity-Treffer leuchtet er heller. Pinchen wählt das Ziel. Im Nahbereich lässt es sich mit der Fingerspitze antippen. Ray- und Poke-Auswahl verwenden Meta Interaction SDK 207.
- **Linke Hand:** Ein Pinch öffnet das Menü, nach Loslassen schließt ein weiterer Pinch es. Lange Systemgesten und die Meta-Systemoberfläche werden nicht umgangen.
- **Listen:** acht vollständig sichtbare Einträge mit Name, Wert und Raum; weitere Einträge durch Pinch-und-Ziehen oder direktes Wischen. Suche, Art und Raum lassen sich kombinieren.
- **Dimmen / Lautstärke:** Auf ein unterstütztes Icon zeigen; unter dem Icon erscheint das Sonnen-/Lautsprecher-Symbol. Damit den Regler öffnen. Pinchen und seitlich ziehen oder direkt am Regler ziehen; Loslassen sendet den Wert. Auch über die Details einer platzierten Entität erreichbar. HA muss Helligkeit bzw. `VOLUME_SET` anbieten. Sensoren werden dadurch nicht schaltbar.
- **Sensoren:** Neben der bisherigen Nahansicht erscheinen Werte oberhalb des Icons beim direkten Hinschauen aus der Ferne. Auf Quest 3 wird hierfür die Kopf-Blickrichtung verwendet.

## Oberfläche und Start

Dunkle neutrale Panels, helle Schrift, dezente violette Auswahl, einheitliche geometrische Icons und eine kurze Startanimation. In den Einstellungen zwischen Deutsch und Englisch wechseln sowie Schaltgeräusche ein-/ausschalten. Die bestätigte Annahme eines Lampenbefehls erzeugt einen leichten Klick; ein Schalter einen mechanischeren Klang. Der HA-Gerätezustand wird anschließend separat abgefragt.

Optional startet die App mit einer HA-Uhr am linken Handgelenk. Antippen öffnet ein **ortsfestes** Panel vor dem Nutzer. Es folgt nicht dem Arm, entsprechend der nachträglichen Vorgabe, Metas Empfehlungen einzuhalten. Ein linker Pinch bleibt als Alternative verfügbar. Bei fehlender Anmeldung bleibt der Zugang zum Verbindungsmenü erhalten.

Auf Quest erscheint bei Adresse, Token und Suche die Meta-Systemtastatur. Im Unity-Editor bleibt eine Ersatztastatur verfügbar. Das Öffnen der Systemtastatur kann kurz den Eingabefokus übernehmen.

## Magie lernen

Unter Einstellungen → Magie das Übungsziel starten. Es schaltet keine HA-Geräte. Hand öffnen, Handfläche zum Ziel richten, auf den grünen Bereitschaftshinweis warten und etwa zwölf Zentimeter vorwärts stoßen. Anschließend zurückziehen oder die Hand kurz schließen. Der zulässige Zielwinkel und die Bereitschaftszeit sind größer als zuvor; Live-Hinweise zeigen fehlende Schritte an. Mehrere Fluganimationen können überlappen. Magie bleibt experimentell und ist kein standardisiertes Meta-Eingabemuster.

## Bilder und Kameras

`image.*` und `camera.*` können als Rahmen platziert werden. Die beim Platzieren gewählte Orientierung wird im räumlichen Anker gespeichert, damit der Rahmen nicht ständig zum Kopf gedreht wird. Bild-Entitäten aktualisieren sich ungefähr alle zehn Sekunden; Kameras liefern eine Folge aktueller Einzelbilder mit bis zu zwei Abrufen pro Sekunde plus Netzwerklaufzeit. **Kein flüssiger Video-/Audiostream.** Zeitstempel und Fehlermeldungen unterscheiden aktuelle von veralteten Bildern. Maximal zwei parallele Abrufe; große oder nicht unterstützte Antworten werden verworfen. Verborgene Rahmen stellen ihre Abrufe ein und geben das Bild frei.

Abrufe verwenden ausschließlich den konfigurierten HA-Server und seine authentifizierten Proxy-Endpunkte. Fremde `entity_picture`-URLs und HTTP-Weiterleitungen erhalten keinen HA-Token. Die App speichert Kamerabilder nicht dauerhaft.

## Räume und Updates

Jede Entität behält ihre eigene gespeicherte Anker-UUID. HA-Räume dienen der Auswahl und sind nicht automatisch dieselben Datensätze wie die von Meta erfassten physischen Räume. Ein Raumwechsel darf bestehende Zuordnungen nicht neu am Startpunkt anlegen. Icons erscheinen erst nach stabilem, aktuell verfolgtem Tracking; nicht gefundene Anker werden erneut gesucht. [Trackingdetails](TRACKING-RECOVERY.md).

Die APK über die vorhandene Installation installieren. Nicht deinstallieren und keine App-Daten löschen: Token und Zuordnungen liegen im bisherigen App-Speicher.

## Dokumentation und Abnahme

[Recherche](DESIGN-RESEARCH.md) · [Mehrraum- und Quest-Abnahme](QUEST-TEST.md) · [Verkauf und Lizenzen](COMMERCIAL-READINESS.md) · [Buildstatus](BUILD-STATUS.md).

Automatisierte Prüfungen können Zustandslogik, HA-Befehle, UI-Aufbau und gespeicherte Zuordnungen prüfen. Sie belegen nicht, dass die Quest im echten Gebäude korrekt relokalisiert, alle Gesten zuverlässig erkennt oder die erforderliche Bildrate hält. Diese Gerätetests bleiben vor einer Verkaufsfreigabe erforderlich.
