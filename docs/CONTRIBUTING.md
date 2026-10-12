# Entwicklung und Kommentarstil

## Vorbild und Sprache

Die Struktur orientiert sich an [LanLens](https://github.com/AlexRosbach/LanLens/tree/b8fa87166ed39ff6f35305fbca6dec6ead7eda20): ein schneller Einstieg in der README, eine technische Referenz, eine symptomorientierte Wissensdatenbank sowie kurze englische Modul-/Funktionsbeschreibungen. Beispiele dafür sind dort `docs/documentation.md`, `docs/knowledgebase.md`, `backend/services/scan_targets.py` und `backend/services/crypto.py`.

HAgoesQuest behält deutsche Benutzer- und Projektdokumentation sowie die englische Bedienungsanleitung. Code-Bezeichner und technische Kommentare sind Englisch. Python-Docstrings werden sinngemäß als C#-XML-Kommentare umgesetzt; fremde Implementierungen und Lizenzangaben werden nicht kopiert.

## Was kommentiert wird

- Klassenbeschreibung: Verantwortung und relevante Grenzen eines Moduls.
- Öffentliche oder schwierige Schnittstelle: Eingaben, Rückgabebedeutung, Einheiten, Fehlerverhalten oder Seiteneffekte, soweit nicht aus der Signatur erkennbar.
- Inline-Kommentar: Warum ein Schutz, eine Reihenfolge oder eine ungewöhnliche Entscheidung nötig ist.
- Zustandsautomaten: Wer setzt zurück, wer hält das Ziel, wann entsteht ein einmaliges Ereignis?

Einfache Zuweisungen und selbsterklärende Getter benötigen keine Nacherzählung. Kommentare beschreiben den tatsächlichen Code, keine geplante Funktion. Beispiel:

```csharp
/// <summary>Returns true once navigation has settled and the previous input is released.</summary>
/// <param name="active">Whether a pinch, trigger or near-menu touch is still active.</param>
/// <param name="time">Monotonic unscaled time in seconds.</param>
public bool Step(bool active, double time)
```

Ein sinnvoller Inline-Kommentar erklärt etwa, warum ein Timeout keinen zweiten HA-Befehl auslösen darf. „Set busy to false“ neben `Busy = false` bringt keinen zusätzlichen Nutzen.

## Dokumentation pflegen

| Änderung | Zu aktualisieren |
| --- | --- |
| Sichtbare Bedienung | `INSTALLATION.md`, `USER-GUIDE-EN.md`, bei Bedarf Wissensdatenbank |
| Schnittstelle, Persistenz oder Zustandsablauf | `documentation.md` und betroffene Code-Kommentare |
| Reproduzierbarer Fehlerfall | `knowledgebase.md` mit Symptom, Bedeutung und Prüfschritten |
| Release | `CHANGELOG.md`, Versions-/Releasehinweise und Buildnachweise |
| Nur Kommentare oder Texte | Eintrag unter „Unveröffentlicht“; keine neue APK behaupten |

Historische Versionsdateien bleiben als Historie erhalten. Die README zeigt den aktuellen Einstieg und verweist für ältere Änderungen auf den Changelog. Einen früheren Buildbericht nicht so umschreiben, als sei ein neuer Hardwaretest durchgeführt worden.

## Geeignete Prüfungen

Kernlogik mit `tests/Run-Tests.ps1` oder .NET 8 prüfen. Für geänderte Unity-Eingabe-/Netzwerkabläufe `tools/Test-Network.ps1` verwenden. Darstellungsänderungen im Editor rendern und auf der Quest prüfen. Die Befehle und Grenzen stehen in der [technischen Referenz](documentation.md#prüfungen).

Bei reinen Kommentaränderungen prüfen, dass die C#-Tokens außerhalb von Kommentaren und Formatierung gleich geblieben sind. Markdown-Verweise auf lokale Dateien müssen auflösbar sein. Ein vollständiger APK-Neubau ist dafür nicht nötig.

## Daten und Veröffentlichung

Token, Signaturschlüssel, private SDK-Einstellungen und persönliche Konfigurationsdateien gehören nicht ins Repository. Bei Änderungen an Paketname, Signatur, Keystore-Alias oder Ankerformaten die Update-Folgen und eine nötige Migration ausdrücklich beschreiben. `PrivateBuildSettings` muss die lokalen SDK-Einstellungen weiterhin aus dem Player ausschließen. Die bestehende [Einschätzung zu Verkauf und Lizenzen](COMMERCIAL-READINESS.md) bleibt maßgeblich; der Dokumentationsstil eines anderen Projekts überträgt keine Lizenz.
