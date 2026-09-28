# HA goes Quest

Mixed-Reality-Prototyp für Meta Quest 3: Home-Assistant-Geräte räumlich zuordnen, per Kopf-Blickrichtung auswählen und mit einer Pinch-Geste bedienen. Sensorwerte erscheinen am gespeicherten Ort in der Wohnung.

**Stand: erster Quellcode-Prototyp. Noch kein Unity-Build, keine APK und kein Test auf einer Quest.** Die unabhängigen Tests der Fokus-/Pinch-Logik, Entitätsregeln und URL-Prüfung wurden unter Windows ausgeführt (36 Assertions). Das ersetzt keine Prüfung der SDK-Integration, Darstellung, Netzwerkkommunikation und Raumanker auf echter Hardware.

## Ablauf

1. Raum laden; falls noch nicht vorhanden, öffnet sich Metas Raumerfassung. „Raum neu scannen“ startet sie erneut.
2. Home-Assistant-Entität aus der Liste wählen.
3. Markierung auf einer erfassten Fläche oder in einem festen Abstand von 0,5 / 1 / 2 Metern platzieren. Bei freier Platzierung selbst die passende Position einnehmen.
4. Menü schließen, Markierung anvisieren, mindestens 0,35 Sekunden fokussieren und rechts Daumen und Zeigefinger zusammenführen.
5. Bei Lampen und Schaltern wird der Zustand geändert. Sensoren zeigen ihren Wert samt Einheit.

Die Quest 3 hat kein Eye-Tracking: „Anschauen“ bedeutet hier die Richtung des Headsets. Die Lampe wird über die ihr zugewiesene Markierung angesprochen. Ein Raumscan erkennt **nicht automatisch**, welche echte Lampe welcher Home-Assistant-Entität entspricht. Diese Zuordnung erfolgt einmal manuell.

## Enthalten

- Passthrough-Szene, Meta MR Utility Kit und lokale Spatial Anchors.
- Entitätsauswahl mit Seiten; Unterstützung für `light`, `switch`, `sensor`, `binary_sensor`.
- Kopf-Blickrichtung + rechter Pinch; rechter Controller-Trigger als Alternative.
- Linker Pinch oder X am linken Controller öffnet das Menü und bricht eine Platzierung ab.
- Lokales Speichern und Wiederfinden von bis zu 50 Zuordnungen pro Home-Assistant-Server. In einem anderen Raum ggf. „Gespeicherte Positionen erneut laden“ verwenden.
- Sensorwerte und Gerätezustände über REST-Polling, etwa alle 1,5 Sekunden plus Antwortzeit. Kein WebSocket-Livestream in dieser Version.
- Explizite `turn_on`-/`turn_off`-Aufrufe, keine automatische Wiederholung von Schaltbefehlen nach Fehlern. Anzeigen werden durch die nächste Statusabfrage bestätigt.
- Offline-/Veraltet-Anzeige; Schalten nur bei frischen Daten. Keine Aktion durch bloßes Anschauen oder dauerhaftes Pinchen.
- Demo mit einer Lampe und einem Temperatursensor, solange keine Verbindungsdatei vorliegt.

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

**Bisher unbestätigt:** Paketimport und vollständige C#-Kompilierung in Unity, Android-/IL2CPP-Build, OpenXR-Initialisierung, UI-Lesbarkeit, Gestenerkennung, lokale Netzwerkerreichbarkeit und Ankerpersistenz. Ein laufender Demo-Modus im Editor bestätigt diese Hardwarefunktionen nicht.

Im Editor: rechte Maustaste halten und Maus bewegen, um die Blickrichtung zu drehen; linke Maustaste bestätigt nach der Fokuszeit; Tab öffnet das Menü. Freie Platzierung ist eine flüchtige Vorschau. Raumerfassung und persistente Anker müssen auf der Quest getestet werden.

## Home Assistant verbinden

Die Quest muss die Home-Assistant-Adresse erreichen können, typischerweise im selben WLAN. In Home Assistant im Benutzerprofil einen **Long-Lived Access Token** erzeugen. Token nicht in GitHub, Chat oder Screenshots veröffentlichen.

Unter Windows nach der Installation:

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

**Entwicklungsstand der Anmeldung:** Token liegen im App-Dateiverzeichnis als Klartext. Android-Backup wird deaktiviert; die Datei ist aber für autorisiertes USB-Debugging lesbar. Eine produktionsreife Anmeldung mit OAuth und geschützter Speicherung ist noch nicht implementiert. Kein Token ist Bestandteil des Projekts oder der APK. Wird eine andere HA-URL eingetragen, verwendet die App eine getrennte Zuordnungsdatei.

## Überprüfung

```powershell
.\tests\Run-Tests.ps1
```

Oder plattformübergreifend mit .NET 8:

```text
dotnet run --project tests/CoreTests.csproj
```

Die Tests kompilieren den echten unabhängigen App-Kern. Sie prüfen Fokuszeit, gehaltenen Pinch, Zielwechsel, Trackingverlust, Wiederaufnahme, Wiederholungssperre, unterstützte Entitäten, ausdrückliche Schaltaktionen und URL-Validierung. GitHub Actions führt diese Tests ebenfalls aus; es wird dort **keine APK gebaut**.

Die notwendige Abnahme auf der Quest ist in [docs/QUEST-TEST.md](docs/QUEST-TEST.md) beschrieben.

## Nächste Ausbaustufen

- Auf echter Quest kompilieren, starten und die dokumentierten Prüfungen abschließen.
- Komfortable Anmeldung direkt in der Brille und geschützte Speicherung.
- WebSocket-Zustandsänderungen, Dimmen, Farben und weitere Entitätstypen.
- Bessere Suche/Filter bei vielen Entitäten und direktes Verschieben vorhandener Zuordnungen.
- Präzisere Platzierung über einen Handstrahl, Oberflächenvorschau und optische Verdeckung hinter Möbeln.
- Automatische Objekterkennung separat evaluieren; sie ist kein Bestandteil dieses Prototyps.

## Technische Quellen

- [Meta MRUK: Raumdaten verwalten](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-manage-scene-data/)
- [Meta Spatial Anchors: Starter-Beispiel](https://developers.meta.com/horizon/documentation/unity/unity-sample-starter-spatial-anchor/)
- [Meta: Eingabe und immersive Funktionen](https://developers.meta.com/horizon/essentials/horizon-os-immersive-features/)
- [Home Assistant REST API](https://developers.home-assistant.io/docs/api/rest/)

Meta- und Unity-Pakete behalten ihre jeweiligen Lizenzbedingungen.
