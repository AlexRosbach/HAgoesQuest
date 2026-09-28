# Tracking beim Start und in mehreren Räumen (ab 0.5.0)

Gespeicherte Icons wurden bisher bereits bei `OVRSpatialAnchor.Localized` eingeblendet. Im verwendeten SDK bedeutet das, dass die Locatable-Komponente aktiv ist. Es bestätigt nicht, dass Position und Orientierung gerade verfolgt werden. Außerdem wurden Anker unmittelbar nach Laden der HA-Anmeldung angefordert, auch vor einem gültigen Kopftracking.

- Laden wartet auf aktives Kopftracking, Fokus und Kamera. Nach spätestens 20 Sekunden wird der Versuch freigegeben; fehlende Anker werden bei verfügbarem Tracking alle 15 Sekunden erneut gesucht.
- Anzeige verlangt zusätzlich `IsTracked` und eine stabile Position über 0,75 Sekunden. Bei einem Positionssprung über 3 cm bzw. einer Rotation über 3 Grad beginnt diese Wartezeit neu. Die Prüfung erfolgt nach den SDK-Updates; Sichtbarkeit wird vor dem Rendern aktualisiert.
- Fehlende Anker ergeben keine sichtbare Ersatzposition. Neue Marker bleiben zunächst unsichtbar. Nicht sichtbare Marker können weder per Zeiger noch durch Magie angesprochen werden.
- Raumscan, Fokuswechsel und Neukalibrierung verwerfen die Tracking-Freigabe sowie offene Platzierungsvorschauen. Neue Positionen werden nur bei stabilem Tracking gespeichert; wechselt das Tracking während des Speicherns, wird die Zuordnung zurückgerollt.
- Bestehende UUID-Zuordnungen, Paketname, Signaturschlüssel und verschlüsselter Token bleiben erhalten. Die APK ist Version 0.5.0, Android-Versionscode 8.
- Mehrere Anker werden parallel lokalisiert. Ein nicht sichtbarer Raum darf die Wiederherstellung anderer Räume nicht nacheinander um je zehn Sekunden verzögern. Die manuelle Wiederherstellung blockiert nicht mehr die gesamte Menübedienung.

Die SDK-Methode `UnboundAnchor.TryGetPose` liefert bereits Weltkoordinaten; diese werden bewusst nicht ein zweites Mal umgerechnet. MRUK und räumliche Anker bleiben für die Ortsbestimmung zuständig. Ein von der Quest stabil, aber falsch lokalisierter Anker lässt sich allein anhand dieser Tracking-Flags nicht erkennen oder automatisch korrigieren. Deshalb bleibt der Neustart in verschiedenen realen Räumen Teil der Geräteabnahme.

Referenz: [Meta Spatial Anchors](https://developers.meta.com/horizon/documentation/unity/unity-spatial-anchors-persist-content/); Verhalten von `Localized` und `IsTracked` zusätzlich im installierten Meta-SDK-Quellcode geprüft.
