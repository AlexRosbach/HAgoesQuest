# 0.4.1 – Handzeiger nach Neukalibrierung

## Rechte Hand

Metas Hand-Prefab hatte keinen PointerPose-Root. Das SDK schreibt die Zeigerpose in lokalen Tracking-Koordinaten; die App verwendete sie deshalb nach einem verschobenen/gedrehten Tracking-Bereich als falsche Weltkoordinaten. Beide Handzeiger sind jetzt explizit dem TrackingSpace des Kamerarigs zugeordnet, sowohl in der gespeicherten Szene als auch zur Laufzeit. Eine erneute Zuordnung erfolgt bei Bedarf automatisch.

Nach Neukalibrierung, wiedererlangtem Eingabefokus und Rückkehr aus der Raumerfassung werden Eingabegesten zurückgesetzt und das Menü neu ausgerichtet. Die eigentliche Systemgeste bleibt während ihrer Ausführung ausgenommen; nach Loslassen kann eine neue Pinch-Geste wieder bedienen.

## Linker Pinch

Ein neuer linker Pinch öffnet das Hauptmenü, der nächste schließt das aktuell offene Menü oder Untermenü. Dazwischen die Finger lösen. Gedrückthalten und ein nach Fokuswechsel noch gehaltener Pinch lösen keinen zusätzlichen Wechsel aus.

## Feuerbälle hintereinander

Eine neue Stoßgeste ist nach Zurücknehmen oder Schließen der Hand möglich, auch wenn der vorherige Feuerball noch fliegt. Die Rücknahme wird auch bei vorübergehend beschäftigtem HA-Client beobachtet. Zielhaltezeit 0,15 s, Mindestpause 0,35 s; bis zu vier Effekte können gleichzeitig bestehen. Gedrückthalten/ausgestrecktes Halten erzeugt weiterhin keine automatische Wiederholung. Schaltbefehle setzen weiterhin einen aktuellen Zustand und einen verfügbaren HA-Client voraus; es werden keine Befehle aufgestaut.

## Installation und Gerätetest

Version 0.4.1, Versionscode 7. Über die vorhandene APK installieren, nicht vorher deinstallieren; Anmeldung und Zuordnungen bleiben im bestehenden App-Speicher.

Auf der Quest besonders testen: Räume neu erfassen, rechts lange Daumen/Zeigefinger zur Neukalibrierung halten, Finger lösen und anschließend mehrere Menüeinträge mit rechts bedienen. Links mehrfach öffnen/schließen. Mehrere getrennte Stoßgesten ausführen. Diese realen Systemdialoge und Handbewegungen sind noch nicht auf Hardware bestätigt.