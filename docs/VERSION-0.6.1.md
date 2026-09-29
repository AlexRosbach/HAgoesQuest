# Version 0.6.1 – lesbareres Menü und anpassbare Uhr

Das Menü verwendet jetzt hellere, vollständig deckende Schieferblau-Flächen, weiße Haupttexte und klar getrennte Überschriften und Beschreibungen. Die Textfarben der normalen, primären und Hinweisflächen erreichen rechnerisch mindestens 4,5:1 Kontrast. Die tatsächliche Lesbarkeit im Headset bei unterschiedlichem Passthrough-Licht bleibt Teil der Geräteabnahme.

Die vier Startkacheln haben detailliertere eigene 3D-Objekte: Glühbirne mit Schraubfassung, eingerichteter Raumgrundriss, Verbindungshub und Zahnrad. Keramik- und gebürstete Metalltexturen werden aus eigenem Code erzeugt. Die Beleuchtung richtet sich nach der Kamera und braucht keine Raumbeleuchtung; Teile werden nach Material zusammengefasst. Es wurden keine fremden Modelle oder Texturen hinzugefügt.

## Uhr positionieren

1. Einstellungen → Deine HA-Uhr → Position & Größe anpassen.
2. Uhrenhand sichtbar vor dich halten. Die Uhr wird während der Anpassung auch angezeigt, wenn der Uhrenmodus zuvor ausgeschaltet war.
3. Mit den großen − / + Tasten seitlich, hoch/runter und in der Tiefe verschieben (5 mm). Die Richtungen beziehen sich auf das Zifferblatt. Neigung, seitliches Kippen und Drehung ändern sich um 5°; die Größe um 5 Prozentpunkte.
4. Position speichern übernimmt die Vorschau und aktiviert die Uhr. Die Uhr folgt danach dem Handgelenk, einschließlich dessen Drehung; sie richtet sich nicht weiter automatisch zur Kopfmitte aus.
5. Abbrechen oder Menü schließen verwirft die Vorschau. Position zurücksetzen stellt die bisherige automatische Ausrichtung wieder her.

Linkes und rechtes Handgelenk haben getrennte gespeicherte Einstellungen. Der Rechts-/Linkshändermodus wählt die passende Uhr. Der Abstand zum Handgelenk ist auf 20 cm begrenzt, die Größe auf 65–160 %. Ohne sichtbare Uhrenhand startet die Anpassung nicht; Speichern ist ebenfalls nur mit erkanntem Handtracking möglich. Während der Anpassung öffnet ein Tippen auf das Zifferblatt nicht versehentlich ein anderes Menü.

## Update und Prüfung

Version 0.6.1, Android-Code 10. Paketname und Entwicklungssignatur bleiben gleich. APK über die bestehende App installieren; weder deinstallieren noch App-Daten löschen. Token, HA-Verbindung und Entitätsanker verwenden weiterhin die bisherigen Speicherformate.

99 Kernprüfungen und 104 Unity-Integrationsprüfungen bestanden. Neue Prüfungen: lokale Uhrenpose nach Bewegung und Drehung des Handgelenks, Speichern und erneutes Laden, Trennung beider Handgelenke, nicht gespeicherte Vorschau, Verschiebungsrichtung, Grenzen, Zurücksetzen, ungültige Speicherdaten, Abbruch beim Schließen und Textkontrast. Unity-Vorschauen für Startmenü und Uhrenanpassung werden separat geprüft. Installation und Bedienung auf einer echten Quest stehen aus.

APK-Upload auf GitHub erfolgt wie vereinbart manuell. Quellcode und Dokumentation werden über einen PR zusammengeführt.

## Ausschluss lokaler SDK-Daten

Die Buildkontrolle hat festgestellt, dass Meta XR SDK 207 bei Entwicklungsbuilds automatisch lokale Editor-/AgentBridge-Verbindungsdaten in ein Resources-Asset einträgt. Der Build verschiebt dieses Asset nun nach den SDK-Vorbereitungsschritten vorübergehend in einen Editor-only-Ordner. Der gepackte Asset-Bericht muss bestätigen, dass es nicht in der APK enthalten ist; andernfalls schlägt der Build fehl. Danach wird die lokale Datei wiederhergestellt, auch bei Buildfehlern. Weder Dateiinhalte noch Zugangsdaten werden dafür ausgegeben oder in Git übernommen. Diese Daten gehören zum Unity-Entwicklungswerkzeug, nicht zur Home-Assistant-Verbindung. Frühere Entwicklungs-APKs konnten diese SDK-Daten enthalten; für die Weitergabe den korrigierten Build verwenden.
