# Version 0.6.2 – Filter, Raumbegrenzung und Gestenrückmeldung

## Entitätenliste und Menü

Suchen & Filtern steht wieder deutlich sichtbar oben in der Liste. Dort bleiben Entitätsarten, Home-Assistant-Räume und Suche nach Name/ID verfügbar. Der aktuelle Typ-, Raum- und Suchfilter wird unter der Liste zusammengefasst.

Die Menüflächen verwenden jetzt einen gemeinsamen World-Space-Canvas. Der Hintergrund wird innerhalb dieser Hierarchie zuerst gezeichnet. Zuvor konnten unabhängig nach Entfernung sortierte Canvas-Flächen bei schräger Ansicht Buttons überdecken. Filterleiste und Zurück-Taste liegen außerhalb des Scroll-Ausschnitts. Die Vorschau prüft Liste, Filter und Rückmeldungseinstellungen auch bei ±35° seitlicher Ansicht.

## Nur Entitäten im aktuellen Raum

Platzierte Icons, Sensorwerte und Kamera-/Bildrahmen werden nur angezeigt, wenn der Kopf und die Entitätsposition innerhalb desselben gescannten Quest-Raums liegen. Die Höhenprüfung verhindert eine Zuordnung zum Stockwerk darüber oder darunter. Ein veralteter oder nur nahegelegener Raum reicht nicht aus. Außerhalb aller erkannten Räume bleiben die Markierungen verborgen.

Die Begrenzung gilt auch für Befehle: Blick/Pinch, Faustregler und Regler-Menü können keine Entität in einem anderen Raum bedienen. Beim Raumwechsel werden laufende Regelungen, Gesten und Platzierungen verworfen. Neue Positionen müssen im aktuellen Raum liegen. Bestehende Anker werden nicht gelöscht und erscheinen beim Zurückkehren wieder, sobald das Tracking stabil ist.

Die vollständige HA-Liste bleibt für die erstmalige Auswahl und Platzierung verfügbar; ihre HA-Raumfilter sind unabhängig von Quest-Raumgeometrien. Ein HA-Raumname wird nicht automatisch einer gescannten Quest-Raum-ID gleichgesetzt. Entfernen von gespeicherten Zuordnungen bleibt über die Verwaltung möglich. Editor-Vorschau ohne Raumdaten und Demo-Modus sind ausgenommen.

## Klicks und grüne Bereitschaft

Beim Faustdrehen und beim Schieberegler ertönt ein kurzes mechanisches Rasterklicken, sobald sich der Vorschauwert ausreichend ändert. Kleine Schwankungen, Halten und Endanschläge bleiben lautlos. Eine Ratenbegrenzung verhindert mehrere übereinanderliegende Klicks pro Tracking-Sample. Die Klänge sind selbst synthetisiert.

Ein dezenter grüner Leuchteffekt folgt der echten getrackten Bedienhand, sobald die Geste bereit ist. Mit geschlossener Faust wird er etwas stärker. Es wird kein eigenes Handmodell dargestellt. Bei Trackingverlust, Zielwechsel, Menüöffnung oder Raumwechsel verschwindet die Bereitschaft.

Unter Einstellungen → Ton & Gestenhinweise sind Geräusche, Gesten-Untertitel und grüner Hand-Effekt unabhängig schaltbar. Die Auswahl bleibt beim Neustart erhalten. Der Linkshändermodus wird berücksichtigt.

## Update und Nachweise

Version 0.6.2 / Android-Code 11. Paket, Signatur und Speicherformate für Token und Anker bleiben gleich. Über die vorhandene App installieren; nicht deinstallieren oder App-Daten löschen. Der Ausschluss lokaler SDK-Entwicklerdaten aus 0.6.1 bleibt bestehen.

112 Kernprüfungen und 119 Unity-Integrationsprüfungen bestanden. Neu geprüft: Filterzugang mit Art/Raum/Suche, gemeinsame Menühierarchie, Filter außerhalb der Scrollmaske, Raumwechsel und Befehlsberechtigung, Stockwerkgrenzen, ungültiger/fehlender aktueller Raum, Bereitschaftseffekt ohne Interaktionsfläche, Ausblenden ohne Tracking, unabhängige Hinweisschalter sowie Rasterklicken, Jitter, Endanschläge und Trackingpausen.

Die echte Quest-Abnahme – insbesondere Raumgrenzen an Türen, Wiederlokalisierung, FOV-/Stereo-Darstellung und subjektive Stärke des Leuchteffekts – steht aus. GitHub-Upload der APK bleibt wie vereinbart manuell.
