<div align="center">

# HA goes Quest

**Home Assistant in deiner Wohnung – Mixed Reality für Meta Quest 3**

[![Version](https://img.shields.io/badge/version-0.6.3-6366f1)](CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-6000.0.66f2-334155)](docs/documentation.md#entwicklung-und-build)
[![Status](https://img.shields.io/badge/status-Entwicklungsbuild-f59e0b)](docs/BUILD-STATUS.md)

Platziere Lampen, Schalter, Sensorwerte und Bilderrahmen im gescannten Raum. Wähle eine Entität mit dem Blickpunkt und bediene sie mit einer Handgeste.

[Installation](docs/INSTALLATION.md) · [Dokumentation](docs/README.md) · [Fehlerhilfe](docs/knowledgebase.md) · [Änderungen](CHANGELOG.md) · [English guide](docs/USER-GUIDE-EN.md)

</div>

## Dein Zuhause auf einen Blick

- **Räumliche Entitäten:** Icons und Sensorwerte an selbst gewählten Positionen; Kamera- und Bildentitäten als Rahmen.
- **Gesten:** Blickpunkt + Pinch zum Schalten, Faustdrehen für Helligkeit oder Lautstärke. Beide Hände sind nutzbar, jeweils eine übernimmt die Aktion.
- **Raumbezogene Anzeige:** Platzierte Entitäten erscheinen nur im aktuellen gescannten Quest-Raum.
- **Entitäten finden:** Scrollbare Liste mit aktuellen Werten, Suche sowie kombinierbaren Typ- und HA-Bereichsfiltern.
- **Persönliche Bedienung:** Deutsch/Englisch, einstellbare HA-Uhr, Geräusche und Gestenhinweise. Optionaler Magiemodus mit Feuerball und Übungsziel.
- **Verbindung behalten:** Nach erfolgreicher Einrichtung wird der Token auf der Quest verschlüsselt gespeichert; kompatible Updates erhalten Anmeldung und Platzierungen.

**Entwicklungsstand:** APK 0.6.3 wurde gebaut und ihre Signatur sowie das Manifest geprüft. 121 Kernprüfungen und 130 Unity-Integrationsprüfungen bestanden beim zugehörigen Build. Der dokumentierte Quest-Hardwaretest steht aus. Details im [Buildbericht](docs/BUILD-STATUS.md).

## Schnellstart

### Voraussetzungen

- Meta Quest 3 mit aktiviertem Handtracking; Entwicklermodus und USB-Debugging für die APK-Installation.
- Eine aus dem Quest-Netzwerk erreichbare Home-Assistant-Instanz und ein Long-Lived Access Token.
- Die bereitgestellte Entwicklungs-APK; bei eigenem Build zusätzlich Unity mit Android Build Support.

### 1. Installieren oder aktualisieren

Mit Meta Quest Developer Hub installieren oder:

```text
adb install -r HAgoesQuest-0.6.3.apk
```

Über die vorhandene App installieren. **Nicht deinstallieren oder App-Daten löschen**, wenn Token und Platzierungen erhalten bleiben sollen. Der APK-Upload erfolgt derzeit manuell; die GitHub-Prüfung baut keine APK.

### 2. Verbinden und Räume erfassen

App starten, Home-Assistant-Basisadresse und Token eingeben oder den Token als QR-Code scannen. Verbindung prüfen und speichern. Anschließend bestehende Quest-Räume laden oder den Systemdialog zur Raumerfassung öffnen.

### 3. Entität platzieren und bedienen

Entität suchen, bei Bedarf nach Art und HA-Bereich filtern, eine Platzierungsmethode wählen und die Position bestätigen. Menü schließen, Blickpunkt auf das Icon richten und pinchen. Für Helligkeit/Lautstärke kurz die Hand öffnen, Faust schließen und drehen; Öffnen übernimmt den Wert.

Sobald die Faust das Ziel festhält, darfst du wegschauen, solange die Hand weiter getrackt wird. Zwischen Handwechseln beide Hände kurz lösen. [Vollständige Bedienung](docs/INSTALLATION.md).

## Was die App derzeit unterstützt

| Funktion | Verhalten in 0.6.3 |
| --- | --- |
| Auswahl | Kopf-Blickrichtung; die Quest 3 hat kein Eye-Tracking |
| Lampen und Schalter | `light` / `switch` ein- und ausschalten; unterstützte Lampen dimmen |
| Lautstärke | `media_player` mit gemeldeter Lautstärkefunktion |
| Weitere Entitäten | Zustände anzeigen; keine allgemeinen Befehle für alle HA-Domänen |
| Kamera / Bild | Aktualisierte Einzelbilder, kein Video-/Audiostream |
| Raumscan | Meta-Raumdaten; Zuordnung einer echten Lampe zur HA-Entität erfolgt manuell |
| Positionen | Bis zu 50 Zuordnungen pro HA-Server; lokale Spatial Anchors |
| Zustände | REST-Polling; kein WebSocket-Livestream |
| HA-Uhr | Einstellbare Position am Handgelenk; öffnet ein ortsfestes Menü |

## Entwicklung

Projektbasis: **Unity 6000.0.66f2**, **Meta XR Core / Interaction SDK / MRUK 207.0.0**, Android ARM64 / IL2CPP. Unity lädt die SDK-Pakete über die konfigurierten Registries. Die Build-Anleitung, Datenformate und Eingabeabläufe stehen in der [technischen Referenz](docs/documentation.md).

Kernprüfungen unter Windows:

```powershell
.\tests\Run-Tests.ps1
```

Alternativ mit .NET 8:

```text
dotnet run --project tests/CoreTests.csproj
```

Die [Entwicklungsrichtlinien](docs/CONTRIBUTING.md) beschreiben Kommentarstil, passende Prüfungen und den Unterschied zwischen Editor- und Hardwareabnahme.

## Dokumentation und Lizenzen

- [Dokumentationsübersicht](docs/README.md): Einstieg nach Aufgabe.
- [Technische Referenz](docs/documentation.md): Architektur, HA-Anfragen, Persistenz, Build und Tests.
- [Wissensdatenbank](docs/knowledgebase.md): Symptome, Ursachen und konkrete Lösungsschritte.
- [Änderungsübersicht](CHANGELOG.md): aktuelle und historische Versionshinweise.
- [Verkauf und Lizenzen](docs/COMMERCIAL-READINESS.md): bestehende Einschätzung und offene Punkte vor einer Veröffentlichung.
- [Grafikherkunft](docs/ART-PROMPTS.md) und [Magiegrafik](docs/ART-MAGIC.md): Assets und Prompts.

Meta-, Unity- und weitere Abhängigkeiten behalten ihre jeweiligen Lizenzbedingungen. Die Dokumentationsstruktur orientiert sich an [LanLens](https://github.com/AlexRosbach/LanLens); dessen Lizenz- oder Produktversprechen werden damit nicht auf HAgoesQuest übertragen.
