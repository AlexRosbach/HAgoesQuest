# HA goes Quest auf deiner Quest 3

## Installieren

1. Entwicklermodus für die Quest aktivieren und die Brille per USB mit deinem PC verbinden. USB-Debugging in der Brille erlauben. [Metas Anleitung](https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/).
2. Die bereitgestellte APK mit Meta Quest Developer Hub installieren. Alternativ: `adb install -r HAgoesQuest-0.2.0.apk`.
3. **HA goes Quest** unter den Apps aus unbekannten Quellen starten. Handtracking in den Quest-Einstellungen einschalten.

## Dein Zuhause einrichten

1. **Mit Home Assistant verbinden** öffnen. Basisadresse und einen in Home Assistant erzeugten langlebigen Zugriffstoken eingeben. Die Tastatur bietet **Einfügen** für die Zwischenablage der Quest. Dann **Verbindung prüfen & speichern** wählen.
2. Unter **Wohnung erfassen & Karte ansehen** die Quest-Raumerfassung starten oder bestehende Räume laden. Den Raumzugriff erlauben.
3. **Entität mit der Hand platzieren** öffnen, Gerät oder Sensor wählen. Lampen können als reine Anzeige oder zum Schalten platziert werden.
4. **Mit der Hand greifen & platzieren** wählen. Mit der rechten Hand pinchen, an die gewünschte Stelle bewegen und loslassen. Danach **Position speichern** bestätigen.
5. Menü schließen. Die Markierung etwa eine halbe Sekunde anschauen und rechts pinchen, um die Lampe zu schalten. Ein linker Pinch öffnet das Menü erneut.

Die Quest 3 verwendet dafür die Richtung deines Headsets. Die Verbindung zwischen einer echten Lampe und ihrer Home-Assistant-Entität weist du einmal selbst zu. Die Karte verwendet Metas Raumscan; die blaue Linie zeigt deinen Laufweg während dieser Sitzung.

Die App unterstützt Lampen, Schalter, Sensoren und Binärsensoren. Gespeicherte Positionen müssen von der Quest wiedererkannt werden. Falls eine Anzeige fehlt, im betreffenden Raum **Gespeicherte Positionen wiederfinden** wählen.

## Entwicklungsstand

Die Version ist für einen ersten Test auf deiner Quest vorgesehen. Der Start auf echter Hardware, Handtracking, Raumanker und geschützte Anmeldung sind noch nicht praktisch auf einer Quest geprüft. Alle dafür vorgesehenen Schritte stehen in `HAgoesQuest/docs/QUEST-TEST.md` im Quellcodearchiv.
