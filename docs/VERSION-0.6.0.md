# 0.6.0 – Blickpunkt, Faustgesten und neue Oberfläche

## Entitäten bedienen

Ein kleiner Punkt bleibt in der Mitte des Sichtfelds. Richte ihn auf ein Icon: Das Icon wird weich heller. Ein Pinch mit der rechten Bedienhand schaltet die ausgewählte Lampe oder den Schalter. Die übrigen Finger dabei locker ausgestreckt lassen. Ein sehr kurzer Pinch wird nicht ausgelöst: 120 ms unterscheiden den bewussten Pinch vom Beginn einer Faust. Gedrückthalten erzeugt keinen zweiten Befehl.

Quest 3 besitzt kein Eye-Tracking. Der Blickpunkt folgt der Kopfmitte, nicht der Augenbewegung. Die Auswahl toleriert einen kleinen Winkel von 1,7° zur Iconmitte. Es gibt keine automatische Aktion allein durch Anschauen. Während der Entitätsbedienung ist der Handstrahl aus; im Menü und beim Platzieren bleiben SDK-Zeiger und direktes Antippen verfügbar.

## Helligkeit und Lautstärke

1. Eine dimmbare Lampe oder einen Lautsprecher ansehen.
2. Die Bedienhand kurz öffnen, bis „Bereit“ erscheint.
3. Faust schließen und wie einen Drehknopf drehen. Nach einer Totzone von 12° erscheint der neue Prozentwert. Die Bewegung wird relativ zur Anfangshaltung gemessen; 120° entsprechen dem gesamten Wertebereich.
4. Hand öffnen: Der gewählte Wert wird einmal an HA gesendet. Während des Drehens ist der Wert eine Vorschau.

Die bisherigen Schieberegler bleiben als Alternative erhalten. Blickwechsel, Menüaufruf, Fokus- oder Trackingverlust verwerfen die laufende Geste. Bei einer nicht unterstützten Entität erscheint keine Regelung.

## Magie

Magie in den Einstellungen einschalten oder das Übungsziel starten. Ziel ansehen, Hand kurz öffnen: Ein pulsierender grüner Ring zeigt die Bereitschaft. Faust schließen und wieder öffnen, ohne zu drehen: Der Feuerball fliegt aus der Hand zum Ziel. Ein Vorwärtsstoß ist nicht mehr nötig. Nach kurzer Pause ist die nächste Faust-Öffnen-Geste möglich; mehrere Feuerbälle dürfen gleichzeitig fliegen.

**Drehen hat Vorrang:** Wurde mit der Faust ein Wert eingestellt, bestätigt das Öffnen diesen Wert und wirft keinen Feuerball. Sensoren, reine Anzeigen und nicht verfügbare Geräte werden nicht geschaltet. Die Übung schaltet keine echten Geräte.

## Sensoren

Das ausgewählte Sensor-Icon zeigt Name und Wert darüber. Die Schrift ist größer und wächst proportional zur Entfernung, damit sie im Sichtfeld ungefähr gleich groß bleibt. Die Skalierung ist zwischen 0,45 und 8 Metern begrenzt und unabhängig vom Größenwechsel des Icons bei Annäherung.

## Uhr und Menü

Unter Einstellungen → Uhr & Bedienhand lässt sich die Uhr einschalten und zwischen Rechts- und Linkshänder wählen. Standard: Uhr links, Bedienung rechts. Im Linkshändermodus ist die Uhr rechts und die linke Hand übernimmt Auswahl, Drehung, Magie und Menübedienung. Auf das Zifferblatt mit der jeweils anderen Hand tippen, um das Menü zu öffnen oder zu schließen. Alternativ bleibt der Pinch der Uhrenhand verfügbar.

Die Uhr besitzt einen Zifferblattrand mit Teilstrichen, Uhrzeit, Datum und Verbindungsanzeige. Das Menü öffnet weiterhin ortsfest vor dem Nutzer. Die Hauptansicht hat größere Kacheln mit selbst erstellten 3D-Symbolen, eine hervorgehobene Hauptaktion, getrennte Verwaltungs-/Schließen-Aktionen und einen gemeinsamen Panelhintergrund. Die Entitätsliste bleibt scrollbar.

## Gestaltung und Abnahme

Orientierung an Metas Empfehlungen: klare primäre/sekundäre Aktionen, ausreichende Trefferflächen und Abstände, erkennbare Auswahlzustände, direktes Antippen mit dem Zeigefinger sowie winkelabhängige Skalierung von Text. Die eigene Faustgeste ist eine projektspezifische Erweiterung und kein zertifiziertes Meta-Eingabemuster. Quellen: [Buttons](https://developers.meta.com/horizon/design/buttons/), [Button-Empfehlungen](https://developers.meta.com/horizon/design/buttons_bp/), [Panels](https://developers.meta.com/horizon/design/panels/), [Hand-UI](https://developers.meta.com/horizon/design/hands-ui-best-practices/), [Angular scaling](https://developers.meta.com/horizon/design/mr-design-guideline/), [Wrist buttons](https://developers.meta.com/horizon/design/wrist-buttons/).

Vor dem Einsatz auf Quest prüfen: Pinch gegen Faust abgrenzen, Rechts-/Linkshändermodus, Uhr antippen, Drehung in beide Richtungen, Öffnen nach Trackingverlust, mehrere Würfe, Schrift in verschiedenen Räumen/Entfernungen und Bildrate. Desktoptests prüfen Zustandslogik und Aufbau, nicht die reale Handerkennung.

## Update

Version 0.6.0 / Android-Code 9. Über die vorhandene Installation installieren; nicht deinstallieren und keine App-Daten löschen. Signatur, Token-Speicherung und Raumzuordnungen bleiben kompatibel.
