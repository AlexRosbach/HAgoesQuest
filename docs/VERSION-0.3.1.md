# 0.3.1 – Anmeldung bei Updates behalten

Die App verwendet weiterhin denselben Paketnamen, denselben Android-Keystore-Alias und dieselbe verschlüsselte Anmeldedatei wie 0.2.0 und 0.3.0. Der Versionscode steigt auf 4. Die vorhandene Anmeldung wird beim Start automatisch geladen.

Im Verbindungsmenü wird der hinterlegte Token angezeigt, ohne seinen Inhalt offenzulegen. Ein leeres Tokenfeld verwendet die zuletzt bestätigten Zugangsdaten ausschließlich für dieselbe normalisierte Serveradresse (einschließlich Protokoll und Port). Ein abgewiesener Ersatz-Token überschreibt die bestätigte Anmeldung nicht. Die HTTP-Freigabe wird beim Öffnen übernommen. „Anmeldung entfernen“ löscht auch die Wiederverwendung im Arbeitsspeicher.

## Installation

`adb install -r HAgoesQuest-0.3.1.apk`

Vorherige App nicht deinstallieren und ihre Daten nicht löschen. Die APK wird als Update installiert. Ohne vorhandene Anmeldedaten oder mit widerrufenem Token ist eine neue Anmeldung erforderlich. Hardwaretest für das tatsächliche Update auf Quest steht noch aus.

## Signatur beibehalten

Build-Quest.ps1 prüft vor dem Build den bisherigen lokalen Signaturschlüssel und nach dem Build das APK-Zertifikat. Fingerabdruck SHA-256:

`345caca35c4a1ad2eb8f2243a0ccda712945c87b9b089714f2778c007c76e5b2`

Bei einer Abweichung wird abgebrochen. Den bisherigen Entwicklungsschlüssel privat sichern und auf einem neuen Build-Rechner unter dem bisherigen Android-Debug-Keystore-Pfad wiederherstellen. Schlüssel niemals ins Repository oder ins Release hochladen. Ein neuer Schlüssel wäre nicht mit den vorhandenen Installationen kompatibel. [Android-App-Signaturen](https://developer.android.com/studio/publish/app-signing).

## Gerätetest

1. In 0.2.0 oder 0.3.0 verbinden und eine Zuordnung speichern.
2. App beenden, 0.3.1 mit `adb install -r` darüber installieren.
3. Starten: Entitäten ohne Token-Eingabe laden; gespeicherte Position wiederfinden.
4. Verbindungsmenü öffnen und mit leerem Tokenfeld erneut prüfen.
5. Quest neu starten und automatische Anmeldung erneut prüfen.
