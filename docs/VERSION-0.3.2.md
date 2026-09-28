# 0.3.2 – Handbedienung, Platzieren und Löschen

- Die Raumerfassung blockiert nicht mehr das gesamte Menü während ihrer Rückmeldung. Nach Rückkehr aus dem Quest-Systemdialog richtet sich das Menü wieder vor dem Kopf aus. Ausbleibende Rückmeldungen haben ein Zeitlimit. Handzeiger verwenden getrackte Hände ohne die bisherige globale High-Confidence-Sperre; Systemgesten bleiben ausgenommen.
- Icons sitzen auf einem dunklen Kreis mit türkisem Rand im Menüdesign. Zwischen 1,2 m und 30 cm wachsen sie weich auf die 1,8-fache Größe. Name und Wert erscheinen weiterhin erst innerhalb von 30 cm.
- Standardplatzierung: Zeigen und einmal pinchen, Vorschau prüfen, speichern oder weiter verschieben. Abstand vorab zwischen 0,5 und 2 m wählen. Die direkte Handplatzierung benötigt ebenfalls nur einen Pinch, kein längeres Festhalten. Die Handposition wird geglättet; die Vorschau bleibt klein.
- „Platzierte Entitäten verwalten / löschen“ im Hauptmenü zeigt auch in Home Assistant nicht mehr verfügbare Entitäten. Löschen benötigt keinen lokalisierten Raumanker. Die Zuordnung wird zuerst dauerhaft entfernt; die separate Bereinigung des Systemankers darf das Löschen nicht rückgängig machen. Bei einer fehlgeschlagenen Bereinigung kann ein ungenutzter Systemanker zurückbleiben.
- Paketname, Signatur und geschützte Anmeldung bleiben updatekompatibel. Version 0.3.2, Versionscode 5. APK über die vorhandene App installieren, nicht deinstallieren.

## Quest-Prüfung

1. Raumkarte bearbeiten, Systemdialog abschließen und danach mit rechts Menü und Entitäten bedienen. Auch Abbruch prüfen.
2. Auf ein Icon zugehen: runder Hintergrund, Rand und kontinuierliche Vergrößerung prüfen; Text erst ab 30 cm.
3. Entität per Zeiger platzieren: pinchen, erneut verschieben, bestätigen. Direkte Handplatzierung mit einmaligem Pinch ebenfalls testen.
4. Im Verwaltungsmenü eine sichtbare und eine nicht lokalisierte Platzierung löschen. App neu starten und sicherstellen, dass beide entfernt bleiben.
5. Update über 0.3.1 installieren und automatische Anmeldung prüfen.

Diese Änderungen sind noch nicht auf echter Quest-Hardware bestätigt.