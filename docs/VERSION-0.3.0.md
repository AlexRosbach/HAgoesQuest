# Version 0.3.0

- Reines Passthrough-AR: keine gerenderten Handmodelle. Handtracking bleibt für Pinch und Zeiger aktiv.
- Offizieller Meta-Boundaryless-Modus im Android-Manifest. Die App hat keine eigene Bewegungsbegrenzung und keinen VR-Modus. Unterstützte Quest-Systemversion erforderlich; über Link gilt dieser Manifest-Eintrag nicht.
- Entitäten: Suche nach Anzeigename oder ID, kombinierbar mit Typ- und Home-Assistant-Raumfilter. Alle gültigen HA-Entitätsarten sind als Anzeige verfügbar. Nur `light` und `switch` senden Schaltbefehle; andere Arten bleiben reine Anzeigen.
- Raumzuordnungen werden über Home Assistants Template-API geladen, einschließlich der vom Gerät geerbten Raumzuordnung. Ohne Zuordnung erscheint die Entität unter „Ohne Raumzuordnung“. Bei fehlender Berechtigung bleiben Typfilter und Suche verfügbar; das Menü zeigt den Ladefehler.
- Platzierte Entitäten zeigen ein ca. 9 cm großes Icon ohne dauerhafte Karte. Name und Wert blenden bei höchstens 30 cm Abstand zwischen Headset und Icon ein und beim Entfernen wieder aus.
- Rechts mit der Hand auf eine Entität zeigen: ein dünner Strahl reicht vom Zeigefinger zum getroffenen Icon. Pinch schaltet nach kurzer Zielhaltezeit. Anzeigen lassen sich ebenfalls anvisieren. Controller bleiben als Alternative verfügbar.
- Eigenes generiertes Schalter-Icon.
- QR-Scan direkt über die Quest-Passthrough-Kamera, ausschließlich nach „Scan starten“ und Kamerafreigabe. Kamera wird beim Verlassen des Scanmenüs abgeschaltet. Die Bilder werden weder gespeichert noch hochgeladen.

## Token vom Handy übernehmen

Die Quest 3/3S benötigt Horizon OS 74 oder neuer für den Kamerazugriff. Zeige einen QR-Code mit dem Token als reinem Text oder diesem JSON:

```json
{"baseUrl":"https://ha.example.com","accessToken":"DEIN_LANG_LEBIGER_ZUGRIFFSTOKEN"}
```

Auch `url`/`token` sind als Feldnamen erlaubt. Scannen öffnet keine Links und stellt keine Verbindung her. Nach dem Scan die Adresse prüfen und „Verbindung prüfen & speichern“ wählen. Unverschlüsseltes HTTP muss weiterhin separat erlaubt werden.

Mit `tools/qr/index.html` und der beiliegenden `qrcode.min.js` kann der QR-Code lokal ohne Onlinedienst erzeugt werden. Beide Dateien zusammen auf das Handy übertragen oder am Computer öffnen. Der Code enthält den Token; anschließend mit „Code und Eingaben löschen“ schließen.

## Gerätetest

Auf echter Quest noch zu prüfen: Boundaryless-Verhalten, Sichtbarkeit der echten Hände, Fingerstrahl bei Pinch, Lesbarkeit der 30-cm-Anzeige, QR-Erkennung vom Handydisplay, Berechtigungsdialoge, räumliche Wiedererkennung und Android-Keystore. Editor- und Buildprüfungen ersetzen diese Prüfung nicht.

## Quellen

- [Meta Boundaryless](https://developers.meta.com/horizon/documentation/unity/unity-boundaryless/)
- [Meta Passthrough Camera API](https://developers.meta.com/horizon/documentation/unity/unity-pca-documentation/)
- [Home Assistant area_id](https://www.home-assistant.io/template-functions/area_id/)
- [Home Assistant REST API](https://developers.home-assistant.io/docs/api/rest/)
- ZXing.Net 0.16.11 (Apache-2.0), QRCode.js (MIT); Lizenzen liegen neben den jeweiligen Dateien.
