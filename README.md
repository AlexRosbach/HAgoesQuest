# HA goes Quest

Mixed-Reality-Prototyp für Meta Quest 3: Home-Assistant-Geräte räumlich zuordnen, per Kopf-Blickrichtung auswählen und mit einer Pinch-Geste bedienen. Sensorwerte erscheinen am gespeicherten Ort in der Wohnung.

**Stand: Entwicklungsversion 0.2.0; Android-Build läuft, Quest-Hardwaretest noch offen.** Vollständiger Unity-Paketimport und C#-Kompilierung sind erfolgreich. Unter Windows bestanden 46 Kernprüfungen und 13 Unity-Netzwerkprüfungen gegen einen lokalen Home-Assistant-Testserver. Der Hardwaretest ist in [docs/QUEST-TEST.md](docs/QUEST-TEST.md) dokumentiert.

## Ablauf

1. Im Verbindungsmenü die Home-Assistant-Adresse und einen Long-Lived Access Token eingeben. Die Bildschirmtastatur bietet auch Einfügen aus der Zwischenablage. Verbinden und speichern.
2. Raum laden; falls noch nicht vorhanden, öffnet sich Metas Raumerfassung. „Raum neu scannen“ startet sie erneut. Die Wohnungskarte zeigt erfasste Räume, Gerätepositionen und den Laufweg der aktuellen Sitzung.
3. Home-Assistant-Entität aus der Liste wählen; bei Lampen/Schaltern zwischen reiner Anzeige und Bedienung wählen.
4. Handplatzierung wählen, rechts Daumen und Zeigefinger zusammenführen, zum gewünschten Ort bewegen und loslassen. Anschließend die Position bestätigen. Alternativ eine Scanfläche oder einen festen Abstand verwenden.
5. Menü schließen, Markierung anvisieren, mindestens 0,35 Sekunden fokussieren und rechts Daumen und Zeigefinger zusammenführen.
6. Bei bedienbaren Lampen und Schaltern wird der Zustand geändert. Sensoren zeigen ihren Wert samt Einheit.

Die Quest 3 hat kein Eye-Tracking: „Anschauen“ bedeutet hier die Richtung des Headsets. Die Lampe wird über die ihr zugewiesene Markierung angesprochen. Ein Raumscan erkennt **nicht automatisch**, welche echte Lampe welcher Home-Assistant-Entität entspricht. Diese Zuordnung erfolgt einmal manuell.

## Enthalten

- Passthrough-Szene, Meta MR Utility Kit und lokale Spatial Anchors.
- Generierte transparente Symbole für Lampe, Karte und Sensor; animiertes Einblenden, Fokus und Rückmeldung bei Zustandsänderung. [Bildprompts und Herkunft](docs/ART-PROMPTS.md).
- Wohnungskarte aus Metas Raumdaten. Die Erfassung erfolgt über den Quest-Systemdialog; der Laufweg ergänzt die Anzeige, erzeugt aber keine eigene kontinuierliche 3D-Rekonstruktion.
- Entitätsauswahl mit Seiten; Unterstützung für `light`, `switch`, `sensor`, `binary_sensor`.
- Kopf-Blickrichtung + rechter Pinch; rechter Controller-Trigger als Alternative.
- Linker Pinch oder X am linken Controller öffnet das Menü und bricht eine Platzierung ab.
- Lokales Speichern und Wiederfinden von bis zu 50 Zuordnungen pro Home-Assistant-Server. In einem anderen Raum ggf. „Gespeicherte Positionen erneut laden“ verwenden.
- Sensorwerte und Gerätezustände über REST-Polling, etwa alle 1,5 Sekunden plus Antwortzeit. Kein WebSocket-Livestream in dieser Version.
- Explizite `turn_on`-/`turn_off`-Aufrufe, keine automatische Wiederholung von Schaltbefehlen nach Fehlern. Anzeigen werden durch die nächste Statusabfrage bestätigt.
- Offline-/Veraltet-Anzeige; Schalten nur bei frischen Daten. Keine Aktion durch bloßes Anschauen oder dauerhaftes Pinchen.
- Demo mit einer Lampe und einem Temperatursensor, solange keine Verbindung eingerichtet wurde.

## In Unity öffnen und bauen

Benötigt: **Unity 6000.0.66f2**, Android Build Support inklusive SDK/NDK/OpenJDK und eine aktivierte Unity-Lizenz. Die Versionsbasis entspricht dem offiziellen [MRUK-Beispielprojekt](https://github.com/oculus-samples/Unity-MRUtilityKitSample). Meta XR Core und MRUK sind auf **207.0.0** festgelegt. Die Paketdateien selbst sind nicht in diesem Repository; Unity lädt sie aus dem Meta-Registry und dem Unity-Registry.

1. Diesen Projektordner in Unity Hub hinzufügen und öffnen. Paketimport vollständig abwarten. Bei der Meta-Abfrage das Meta-XR-Feature-Set aktivieren.
2. **File → Build Profiles → Android → Switch Platform**. Neukompilierung abwarten.
3. **HA goes Quest → 1 - Projekt konfigurieren** ausführen.
4. **HA goes Quest → 2 - Startszene erstellen** ausführen. Erzeugt `Assets/HAgoesQuest/Scenes/Home.unity` mit Camera Rig, Händen, Passthrough, MRUK und App. Eine vorhandene Szene wird geöffnet und nicht ersetzt.
5. **Project Settings → XR Plug-in Management → OpenXR** für Android prüfen: OpenXR-Loader, Meta-XR-Feature-Set und Oculus-Touch-Interaction-Profile müssen aktiv sein. **Project Validation** und Metas **Project Setup Tool** öffnen und alle erforderlichen Probleme beheben. Das Projekt nutzt das Built-in-Render-Pipeline-Setup; URP ist für diese UI nicht notwendig.
6. **HA goes Quest → 3 - Entwicklungs-APK bauen**. Ergebnis bei erfolgreichem Build: `Builds/HAgoesQuest.apk`.
7. Quest-Entwicklermodus aktivieren, per USB verbinden, USB-Debugging in der Brille erlauben. Mit Metas Developer Hub installieren oder `adb install -r Builds/HAgoesQuest.apk` verwenden.
8. App auf der Quest einmal starten. Raumzugriff beim Scan erlauben und Handtracking in den Quest-Einstellungen aktivieren.

**Bisher unbestätigt:** Android-/IL2CPP-Build, OpenXR-Initialisierung auf der Quest, UI-Lesbarkeit in der Brille, Gestenerkennung, Erreichbarkeit des eigenen Home Assistant und Ankerpersistenz. Die erfolgreichen Editorprüfungen bestätigen diese Hardwarefunktionen nicht.

Im Editor: rechte Maustaste halten und Maus bewegen, um die Blickrichtung zu drehen; linke Maustaste bestätigt nach der Fokuszeit; Tab öffnet das Menü. Freie Platzierung ist eine flüchtige Vorschau. Raumerfassung und persistente Anker müssen auf der Quest getestet werden.

## Home Assistant verbinden

Die Quest muss die Home-Assistant-Adresse erreichen können, typischerweise im selben WLAN. In Home Assistant im Benutzerprofil einen **Long-Lived Access Token** erzeugen. Token nicht in GitHub, Chat oder Screenshots veröffentlichen.

Die reguläre Einrichtung erfolgt direkt im Verbindungsmenü der App. Der Token wird verdeckt eingegeben und nach erfolgreicher Prüfung mit einem Schlüssel aus dem Android Keystore verschlüsselt gespeichert. Die Verschlüsselung muss noch auf echter Quest geprüft werden. Kein Token ist Bestandteil des Projekts oder der APK. Eine andere HA-Adresse verwendet getrennte räumliche Zuordnungen.

Optionaler USB-Einrichtungsweg unter Windows:

```powershell
.\tools\Connect-Quest.ps1 -BaseUrl 'https://dein-home-assistant.example'
```

Das Skript fragt den Token verdeckt ab und überträgt die Konfiguration auf genau eine per USB autorisierte Quest. Falls ADB nicht im Suchpfad liegt, `-Adb 'C:\Pfad\zu\adb.exe'` ergänzen. Bei mehreren Geräten `-Device 'SERIAL'` angeben.

Für eine lokale Home-Assistant-Installation ohne HTTPS:

```powershell
.\tools\Connect-Quest.ps1 -BaseUrl 'http://192.168.1.10:8123' -AllowInsecureHttp
```

HTTP überträgt auch den Token unverschlüsselt; deshalb verlangt der Prototyp die ausdrückliche Freigabe. Zertifikatsprüfung wird bei HTTPS nicht deaktiviert. HTTP-Weiterleitungen werden nicht verfolgt.

Alternativ eine Kopie von `docs/connection.example.json` als `connection.json` in `Application.persistentDataPath` ablegen. Auf der Quest ist das normalerweise `/sdcard/Android/data/com.alexrosbach.hagoesquest/files/`. Danach App neu starten. Ohne Datei ist Demo aktiv; eine fehlerhafte Datei führt zu einer Fehlermeldung und wird nicht stillschweigend als Demo behandelt.

Die optionale USB-Konfigurationsdatei enthält zunächst Klartext. Nach erfolgreicher Prüfung und verschlüsselter Speicherung löscht die App diese alte Datei. Android-Backup ist deaktiviert. OAuth-Anmeldung ist noch nicht implementiert.

## Überprüfung

```powershell
.\tests\Run-Tests.ps1
```

Oder plattformübergreifend mit .NET 8:

```text
dotnet run --project tests/CoreTests.csproj
```

Die Tests kompilieren den echten unabhängigen App-Kern. Sie prüfen Fokuszeit, gehaltenen Pinch, Zielwechsel, Trackingverlust, Wiederaufnahme, Wiederholungssperre, unterstützte Entitäten, ausdrückliche Schaltaktionen und URL-Validierung. GitHub Actions führt diese Tests ebenfalls aus; es wird dort **keine APK gebaut**.

`tools/Test-Network.ps1` startet einen ausschließlich auf `127.0.0.1` erreichbaren Testserver und prüft den tatsächlichen Unity-Netzwerkclient im Play Mode: Anmeldung, Zustandsdaten samt Einheit, Schaltbefehl und Aktualisierung, keine Sensor-/Offline-Befehle, Wiederverbindung, Ablehnung ungültiger Token und HTTP-Freigabe. Dafür werden keine echten Zugangsdaten verwendet.

Die notwendige Abnahme auf der Quest ist in [docs/QUEST-TEST.md](docs/QUEST-TEST.md) beschrieben.

## Nächste Ausbaustufen

- Auf echter Quest kompilieren, starten und die dokumentierten Prüfungen abschließen.
- WebSocket-Zustandsänderungen, Dimmen, Farben und weitere Entitätstypen.
- Bessere Suche/Filter bei vielen Entitäten und direktes Verschieben vorhandener Zuordnungen.
- Optische Verdeckung hinter Möbeln.
- Automatische Objekterkennung separat evaluieren; sie ist kein Bestandteil dieses Prototyps.

## Technische Quellen

- [Meta MRUK: Raumdaten verwalten](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-manage-scene-data/)
- [Meta Spatial Anchors: Starter-Beispiel](https://developers.meta.com/horizon/documentation/unity/unity-sample-starter-spatial-anchor/)
- [Meta: Eingabe und immersive Funktionen](https://developers.meta.com/horizon/essentials/horizon-os-immersive-features/)
- [Home Assistant REST API](https://developers.home-assistant.io/docs/api/rest/)

Meta- und Unity-Pakete behalten ihre jeweiligen Lizenzbedingungen.
