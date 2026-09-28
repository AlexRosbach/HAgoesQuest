# HA goes Quest auf deiner Quest 3

## Installieren

1. Entwicklermodus für die Quest aktivieren und die Brille per USB mit deinem PC verbinden. USB-Debugging in der Brille erlauben. [Metas Anleitung](https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/).
2. Die bereitgestellte APK mit Meta Quest Developer Hub installieren. Alternativ: `adb install -r HAgoesQuest-0.3.0.apk`.
3. **HA goes Quest** unter den Apps aus unbekannten Quellen starten. Handtracking in den Quest-Einstellungen einschalten.

## Dein Zuhause einrichten

1. **Mit Home Assistant verbinden** öffnen. Basisadresse und einen in Home Assistant erzeugten langlebigen Zugriffstoken eingeben. Die Tastatur bietet **Einfügen** für die Zwischenablage der Quest. Dann **Verbindung prüfen & speichern** wählen.
2. Unter **Wohnung erfassen & Karte ansehen** die Quest-Raumerfassung starten oder bestehende Räume laden. Den Raumzugriff erlauben.
3. **Entität mit der Hand platzieren** öffnen, Gerät oder Sensor wählen. Lampen können als reine Anzeige oder zum Schalten platziert werden.
4. **Mit der Hand greifen & platzieren** wählen. Mit der rechten Hand pinchen, an die gewünschte Stelle bewegen und loslassen. Danach **Position speichern** bestätigen.
5. Menü schließen. Mit der rechten Hand etwa eine halbe Sekunde auf das Icon zeigen und rechts pinchen, um die Lampe zu schalten. Ein linker Pinch öffnet das Menü erneut.

Ein dünner Strahl zeigt das getroffene Icon. Innerhalb von 30 cm zum Headset erscheinen Name und Wert. Die Verbindung zwischen einer echten Lampe und ihrer Home-Assistant-Entität weist du einmal selbst zu. Die Karte verwendet Metas Raumscan; die blaue Linie zeigt deinen Laufweg während dieser Sitzung.

Die Suche unterstützt Namen und IDs, kombiniert mit Typ- und Raumfiltern. Alle Entitätsarten können angezeigt werden; Lampen und Schalter lassen sich zusätzlich schalten. Gespeicherte Positionen müssen von der Quest wiedererkannt werden. Falls eine Anzeige fehlt, im betreffenden Raum **Gespeicherte Positionen wiederfinden** wählen.

## Zugang per QR-Code

Im Verbindungsmenü **Token per QR-Code → Scan starten** wählen und Kamerazugriff erlauben. Der QR-Code kann den Token als Text oder ein JSON mit **baseUrl** und **accessToken** enthalten. Nach dem Scan Adresse prüfen und verbinden. Mit dem beiliegenden QR-Helfer lässt sich der Code lokal auf dem Handy oder Computer erzeugen. Beide Dateien aus dem ZIP zusammen entpacken und index.html öffnen. Quest 3/3S mit Horizon OS 74 oder neuer erforderlich.

## Entwicklungsstand

Die Version ist für einen ersten Test auf deiner Quest vorgesehen. Der Start auf echter Hardware, Handtracking, Raumanker und geschützte Anmeldung sind noch nicht praktisch auf einer Quest geprüft. Alle dafür vorgesehenen Schritte stehen in `HAgoesQuest/docs/QUEST-TEST.md` im Quellcodearchiv.
