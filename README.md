# HA goes Quest

Mixed-Reality-Prototyp für Meta Quest 3: Home-Assistant-Geräte räumlich zuordnen, über einen Punkt in der Blickmitte auswählen und mit einer Pinch-Geste bedienen. Sensorwerte erscheinen am gespeicherten Ort in der Wohnung.

**Stand: installierbare Entwicklungs-APK 0.6.3 gebaut und geprüft; Quest-Hardwaretest noch offen.** Unity-Paketimport, C#-Kompilierung und Android-ARM64/IL2CPP-Build sind erfolgreich. Signatur, Paketinhalt und erforderliche Quest-Berechtigungen wurden an der fertigen APK geprüft. Unter Windows bestanden 121 Kernprüfungen und 130 Unity-Integrationsprüfungen gegen einen lokalen Home-Assistant-Testserver. Details: [Buildstatus](docs/BUILD-STATUS.md), [Quest-Abnahme](docs/QUEST-TEST.md).

Neu in 0.6.3: eigenständiger HA-Bereichskatalog, Zurück ohne Durchklicken, beide Hände mit exklusiver Aktionsübernahme, festgehaltenes Reglerziel beim Wegschauen und räumliche Lichtbögen an der echten Hand. [Anleitung](docs/VERSION-0.6.3.md).

Neu in 0.6.2: sichtbare Listenfilter, feste Menü-Zeichenreihenfolge, platzierte Entitäten nur im eigenen Quest-Raum, mechanische Drehgeräusche und grünes Bereitschaftsleuchten an der echten Hand. Gestenuntertitel separat schaltbar. [Anleitung](docs/VERSION-0.6.2.md).

Neu in 0.6.1: hellere Menüflächen, detailliertere texturierte 3D-Symbole und eine pro Handgelenk einstellbare Uhrenposition mit Live-Vorschau, Speichern, Abbrechen und Zurücksetzen. [Anleitung](docs/VERSION-0.6.1.md).

Neu in 0.6.0: Blickpunkt + Pinch, Faustdrehen für Helligkeit/Lautstärke, Faust öffnen zum Feuerballwerfen, größere entfernungsabhängige Sensorbeschriftung, überarbeitete Uhr mit Rechts-/Linkshändermodus und Menükacheln mit eigenen 3D-Symbolen. [Bedienung und Abnahme](docs/VERSION-0.6.0.md).

Neu in 0.5.0: Meta Interaction SDK für Zeigen und Antippen, leuchtender Strahl, acht sichtbare Entitäten mit Scrollen, Helligkeits-/Lautstärkeregler, Sensorwerte beim Hinschauen, HA-Uhr, Systemtastatur, Deutsch/Englisch, Klickgeräusche und Kamera-/Bilderrahmen. Gespeicherte Anker werden erst bei stabilem Tracking eingeblendet und parallel wiedergefunden. [Anleitung](docs/VERSION-0.5.0.md) · [English guide](docs/USER-GUIDE-EN.md) · [Meta-Recherche](docs/DESIGN-RESEARCH.md) · [Verkauf/Lizenzen](docs/COMMERCIAL-READINESS.md).

Neu in 0.4.1: Handzeiger bleiben bei Neukalibrierung im richtigen Koordinatenraum; linker Pinch öffnet und schließt das Menü; mehrere Feuerbälle nacheinander. [Details](docs/VERSION-0.4.1.md).

Neu in 0.4.0: optionaler Magie-Modus mit flacher rechter Hand, gezieltem Vorwärtsstoß und generiertem grünem Feuerball samt Schweif und Aufprall. [Anleitung](docs/VERSION-0.4.0.md) · [Grafik und Prompt](docs/ART-MAGIC.md).

Neu in 0.3.2: Bedienung nach Raumscan wiederherstellen, runde wachsende Icons, Platzieren mit einem Pinch und eigene Verwaltung zum Löschen. [Details](docs/VERSION-0.3.2.md).

Neu in 0.3.1: gespeicherten Token bei Updates und im Verbindungsmenü weiterverwenden; Signaturkontrolle für kompatible APKs. [Update-Anleitung](docs/VERSION-0.3.1.md).

Neu in 0.3.0: keine virtuellen Hände, Boundaryless-AR, Typ-/Raumfilter und Suche, kompakte Icons mit Namen/Werten ab 30 cm Nähe, Fingerstrahl und QR-Zugang. [Details und QR-Anleitung](docs/VERSION-0.3.0.md).

## Ablauf

1. Im Verbindungsmenü die Home-Assistant-Adresse und einen Long-Lived Access Token eingeben. Die Bildschirmtastatur bietet auch Einfügen aus der Zwischenablage. Verbinden und speichern.
2. Raum laden; falls noch nicht vorhanden, öffnet sich Metas Raumerfassung. „Raum neu scannen“ startet sie erneut. Die Wohnungskarte zeigt erfasste Räume, Gerätepositionen und den Laufweg der aktuellen Sitzung.
3. Home-Assistant-Entität aus der Liste wählen; bei Lampen/Schaltern zwischen reiner Anzeige und Bedienung wählen.
4. Zeigerplatzierung wählen, auf den Wunschort zeigen und einmal pinchen. Vorschau bestätigen oder weiter verschieben. Alternativ direkt an der Hand oder auf einer Scanfläche platzieren; auch dort einmal pinchen und bestätigen.
5. Menü schließen, den Blickpunkt auf die Markierung richten und mit der Bedienhand pinchen. Für Helligkeit/Lautstärke kurz die Hand öffnen, Faust schließen und drehen; Öffnen bestätigt den Prozentwert. Die Schieberegler bleiben alternativ verfügbar.
6. Bei bedienbaren Lampen und Schaltern wird der Zustand geändert. Sensoren zeigen ihren Wert samt Einheit.

Die Quest 3 hat kein Eye-Tracking. Die Entitätsauswahl folgt der Kopfmitte, die Bestätigung der Bedienhand (standardmäßig rechts). Menüs unterstützen SDK-Zeiger und direktes Antippen. Der Linkshändermodus spiegelt die Hände. Die Lampe wird über die ihr zugewiesene Markierung angesprochen. Ein Raumscan erkennt **nicht automatisch**, welche echte Lampe welcher Home-Assistant-Entität entspricht. Diese Zuordnung erfolgt einmal manuell.

## Enthalten

- Passthrough-Szene, Meta MR Utility Kit und lokale Spatial Anchors.
- Generierte transparente Symbole für Lampe, Karte und Sensor; animiertes Einblenden, Fokus und Rückmeldung bei Zustandsänderung. [Bildprompts und Herkunft](docs/ART-PROMPTS.md).
- Wohnungskarte aus Metas Raumdaten. Die Erfassung erfolgt über den Quest-Systemdialog; der Laufweg ergänzt die Anzeige, erzeugt aber keine eigene kontinuierliche 3D-Rekonstruktion.
- Scrollbare Entitätsauswahl mit acht sichtbaren Einträgen und aktuellen Werten; Typ- und Raumfilter sowie Suche; alle Entitätsarten als Anzeige, `light`/`switch` zum Schalten.
- Blickpunkt + Pinch der Bedienhand; Controller-Trigger als Alternative. Fingerzeiger und Antippen bleiben für Menüs und Platzierung.
- Pinch der Uhrenhand öffnet/schließt das Menü und bricht eine Platzierung ab. Standardmäßig links; im Linkshändermodus rechts.
- Lokales Speichern und Wiederfinden von bis zu 50 Zuordnungen pro Home-Assistant-Server. Nicht erkannte Anker werden automatisch erneut gesucht; im betreffenden Raum kann „Gespeicherte Positionen wiederfinden“ zusätzlich angestoßen werden. HA-Raumfilter und physische Meta-Räume sind getrennte Zuordnungen.
- Sensorwerte und Gerätezustände über REST-Polling, etwa alle 1,5 Sekunden plus Antwortzeit. Kein WebSocket-Livestream in dieser Version.
- Explizite `turn_on`-/`turn_off`-Aufrufe, keine automatische Wiederholung von Schaltbefehlen nach Fehlern. Anzeigen werden durch die nächste Statusabfrage bestätigt.
- Offline-/Veraltet-Anzeige; Schalten nur bei frischen Daten. Keine Aktion durch bloßes Anschauen oder dauerhaftes Pinchen.
- Demo mit einer Lampe und einem Temperatursensor, solange keine Verbindung eingerichtet wurde.

## In Unity öffnen und bauen

Benötigt: **Unity 6000.0.66f2**, Android Build Support inklusive SDK/NDK/OpenJDK und eine aktivierte Unity-Lizenz. Die Versionsbasis entspricht dem offiziellen [MRUK-Beispielprojekt](https://github.com/oculus-samples/Unity-MRUtilityKitSample). Meta XR Core, Interaction SDK und MRUK sind auf **207.0.0** festgelegt. Die Paketdateien selbst sind nicht in diesem Repository; Unity lädt sie aus dem Meta-Registry und dem Unity-Registry.

1. Diesen Projektordner in Unity Hub hinzufügen und öffnen. Paketimport vollständig abwarten. Bei der Meta-Abfrage das Meta-XR-Feature-Set aktivieren.
2. **File → Build Profiles → Android → Switch Platform**. Neukompilierung abwarten.
3. **HA goes Quest → 1 - Projekt konfigurieren** ausführen.
4. **HA goes Quest → 2 - Startszene erstellen** ausführen. Erzeugt `Assets/HAgoesQuest/Scenes/Home.unity` mit Camera Rig, Händen, Passthrough, MRUK und App. Eine vorhandene Szene wird geöffnet und nicht ersetzt.
5. **Project Settings → XR Plug-in Management → OpenXR** für Android prüfen: OpenXR-Loader, Meta-XR-Feature-Set und Oculus-Touch-Interaction-Profile müssen aktiv sein. **Project Validation** und Metas **Project Setup Tool** öffnen und alle erforderlichen Probleme beheben. Das Projekt nutzt das Built-in-Render-Pipeline-Setup; URP ist für diese UI nicht notwendig.
6. **HA goes Quest → 3 - Entwicklungs-APK bauen**. Ergebnis bei erfolgreichem Build: `Builds/HAgoesQuest.apk`.
7. Quest-Entwicklermodus aktivieren, per USB verbinden, USB-Debugging in der Brille erlauben. Mit Metas Developer Hub installieren oder `adb install -r Builds/HAgoesQuest.apk` verwenden.
8. App auf der Quest einmal starten. Raumzugriff beim Scan erlauben und Handtracking in den Quest-Einstellungen aktivieren.

**Bisher unbestätigt:** Installation und OpenXR-Initialisierung auf der Quest, UI-Lesbarkeit in der Brille, Gestenerkennung, Erreichbarkeit des eigenen Home Assistant, Android-Keystore und Ankerpersistenz. Die erfolgreichen Build- und Editorprüfungen bestätigen diese Hardwarefunktionen nicht.

Im Editor: rechte Maustaste halten und Maus bewegen, um die Blickrichtung zu drehen; linke Maustaste bestätigt; Tab öffnet das Menü. Freie Platzierung ist eine flüchtige Vorschau. Raumerfassung und persistente Anker müssen auf der Quest getestet werden.

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

- Die erstellte APK auf einer echten Quest installieren, starten und die dokumentierten Prüfungen abschließen.
- WebSocket-Zustandsänderungen, Farbauswahl und weitere steuerbare Entitätstypen.
- Suche bei sehr großen Installationen weiter erproben und direktes Verschieben vorhandener Zuordnungen ergänzen.
- Optische Verdeckung hinter Möbeln.
- Automatische Objekterkennung separat evaluieren; sie ist kein Bestandteil dieses Prototyps.

## Technische Quellen

- [Meta MRUK: Raumdaten verwalten](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-manage-scene-data/)
- [Meta Spatial Anchors: Starter-Beispiel](https://developers.meta.com/horizon/documentation/unity/unity-sample-starter-spatial-anchor/)
- [Meta: Eingabe und immersive Funktionen](https://developers.meta.com/horizon/essentials/horizon-os-immersive-features/)
- [Home Assistant REST API](https://developers.home-assistant.io/docs/api/rest/)

Meta- und Unity-Pakete behalten ihre jeweiligen Lizenzbedingungen.


