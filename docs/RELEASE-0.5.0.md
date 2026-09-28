# HA goes Quest 0.5.0

Mehr Orientierung, zuverlässigere Auswahl und eine umfangreichere Oberfläche für Home Assistant auf Quest 3.

- Meta Interaction SDK für Zeigen, Pinchen und Antippen. Leuchtender Fingerstrahl mit deutlichem Trefferfeedback; kein zusätzlicher Wartefilter vor der Auswahl.
- Scrollbare Auswahl mit acht sichtbaren Entitäten, aktuellen Werten, Raum-/Typfiltern und Suche. Neue geometrische Icons, dunkle neutrale Panels, Startanimation und eigene Klickgeräusche.
- Helligkeit und Lautstärke über unterstützte HA-Entitäten einstellen: Regler öffnen, mit Pinch seitlich ziehen und loslassen. Sensorwerte beim direkten Hinschauen auch aus der Ferne.
- Optionale HA-Uhr am linken Arm zum Öffnen eines ortsfesten Menüs, Meta-Systemtastatur und Deutsch/Englisch.
- Magie mit Übungsziel und Live-Anleitung; wiederholte Feuerbälle weiterhin möglich.
- Gespeicherte Anker erst bei stabilem Tracking anzeigen, parallel wiederfinden und bei Raumwechsel erneut suchen. Fokusverlust und Neukalibrierung verwerfen alte Eingaben und eingefrorene Platzierungen.
- Bild- und Kameraentitäten als räumliche Rahmen. Kameras zeigen eine aktuelle Einzelbildfolge (bis zu zwei Abrufe pro Sekunde zuzüglich Netzwerkzeit), keinen flüssigen Video-/Audiostream.

## Installieren

`HAgoesQuest-0.5.0.apk` über die vorhandene App installieren, beispielsweise mit `adb install -r HAgoesQuest-0.5.0.apk`. Nicht vorher deinstallieren oder App-Daten löschen. Paketname, Signatur und Speicherformat bleiben für Token und Zuordnungen kompatibel. Android-Versionscode: 8.

## Geprüft und offen

77 Kernprüfungen und 82 Unity-Integrationsprüfungen erfolgreich, darunter SDK-Auswahl, Scrollen, Regler, vier HA-Raumzuordnungen, gespeicherte Anker-IDs und authentifizierte Bilder. Menü im Unity-Renderer geprüft. Diese Tests verwenden einen lokalen HA-Testserver.

Installation, Systemtastatur, reale Mehrraum-Lokalisierung, Handgesten und Performance müssen auf Quest abgenommen werden. Diese Entwicklungsversion ist nicht als verkaufsfertig freigegeben. Die Dokumentation enthält eine Meta-Designrecherche, eine Abnahmeliste und eine kommerzielle Lizenzvorprüfung, insbesondere zur Unity-Einordnung als Nicht-Spiel-Anwendung.

Die APK ist als manuell hochzuladendes Release-Asset vorgesehen. Die Quellcode-Tests in GitHub Actions bauen keine APK.
