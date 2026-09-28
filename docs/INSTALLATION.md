# HA goes Quest auf deiner Quest 3

## Installieren

1. Entwicklermodus für die Quest aktivieren und die Brille per USB mit deinem PC verbinden. USB-Debugging in der Brille erlauben. [Metas Anleitung](https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/).
2. Die bereitgestellte APK mit Meta Quest Developer Hub installieren. Alternativ: `adb install -r HAgoesQuest-0.4.1.apk`.
3. **HA goes Quest** unter den Apps aus unbekannten Quellen starten. Handtracking in den Quest-Einstellungen einschalten.

## Update ohne erneute Token-Eingabe

Die neue APK über die vorhandene App installieren, beispielsweise mit `adb install -r HAgoesQuest-0.4.1.apk`. **Nicht vorher deinstallieren und keine App-Daten löschen.** Die App lädt den geschützten Token beim Start automatisch; auch die gespeicherten Zuordnungen bleiben im App-Speicher.

Das Verbindungsmenü zeigt „Gespeicherter Token wird verwendet“. Für dieselbe Serveradresse darf das Tokenfeld leer bleiben. Bei einer anderen Adresse ist ein neuer Token erforderlich. Ein fehlgeschlagener Verbindungsversuch ersetzt die zuletzt bestätigte Anmeldung nicht. Ein von Home Assistant widerrufener Token muss erneuert werden.

## Dein Zuhause einrichten

1. **Mit Home Assistant verbinden** öffnen. Basisadresse und einen in Home Assistant erzeugten langlebigen Zugriffstoken eingeben. Die Tastatur bietet **Einfügen** für die Zwischenablage der Quest. Dann **Verbindung prüfen & speichern** wählen.
2. Unter **Wohnung erfassen & Karte ansehen** die Quest-Raumerfassung starten oder bestehende Räume laden. Den Raumzugriff erlauben.
3. **Entität mit der Hand platzieren** öffnen, Gerät oder Sensor wählen. Lampen können als reine Anzeige oder zum Schalten platziert werden.
4. **Zeigen & pinchen** wählen. Vorher bei Bedarf den Abstand zwischen 0,5 und 2 Metern einstellen. Auf die gewünschte Stelle zeigen und einmal rechts pinchen. Die Vorschau bleibt stehen: **Position speichern** oder **Weiter verschieben** wählen. Alternativ folgt **Direkt an der Hand platzieren** der Hand bis zum einmaligen Pinch.
5. Menü schließen. Mit der rechten Hand etwa eine halbe Sekunde auf das Icon zeigen und rechts pinchen, um die Lampe zu schalten. Ein linker Pinch öffnet das Menü, der nächste schließt es wieder. Dazwischen die Finger lösen.

Im Hauptmenü unter **Platzierte Entitäten verwalten / löschen** lassen sich Zuordnungen auch ohne sichtbaren Raumanker entfernen.

Ein dünner Strahl zeigt das getroffene Icon. Der runde Hintergrund und türkise Rand passen zum Menü. Von 1,2 m bis 30 cm vergrößert sich das Icon weich auf das 1,8-Fache. Innerhalb von 30 cm zum Headset erscheinen Name und Wert. Die Verbindung zwischen einer echten Lampe und ihrer Home-Assistant-Entität weist du einmal selbst zu. Die Karte verwendet Metas Raumscan; die blaue Linie zeigt deinen Laufweg während dieser Sitzung.

Die Suche unterstützt Namen und IDs, kombiniert mit Typ- und Raumfiltern. Alle Entitätsarten können angezeigt werden; Lampen und Schalter lassen sich zusätzlich schalten. Gespeicherte Positionen müssen von der Quest wiedererkannt werden. Falls eine Anzeige fehlt, im betreffenden Raum **Gespeicherte Positionen wiederfinden** wählen.

## Zugang per QR-Code

Im Verbindungsmenü **Token per QR-Code → Scan starten** wählen und Kamerazugriff erlauben. Der QR-Code kann den Token als Text oder ein JSON mit **baseUrl** und **accessToken** enthalten. Nach dem Scan Adresse prüfen und verbinden. Mit dem beiliegenden QR-Helfer lässt sich der Code lokal auf dem Handy oder Computer erzeugen. Beide Dateien aus dem ZIP zusammen entpacken und index.html öffnen. Quest 3/3S mit Horizon OS 74 oder neuer erforderlich.

## Magie-Modus

Im Hauptmenü **Einstellungen · Magie & Positionen → Magie einschalten**, dann das Menü schließen. Rechte Hand flach zum Lampen- oder Schalter-Icon richten, kurz ruhig halten und bei grünem Zielrand etwa zwölf Zentimeter nach vorn stoßen. Ein grüner Feuerball fliegt von der Hand zum Icon. Für weitere Feuerbälle die Hand zurücknehmen oder kurz schließen und erneut stoßen; der vorige Effekt darf noch laufen. Die Einstellung bleibt gespeichert; anfangs ist sie aus. Der Effekt zeigt den ausgelösten Befehl, der zurückgemeldete Entitätszustand zeigt das tatsächliche Ergebnis.

## Entwicklungsstand

Die Version ist für einen ersten Test auf deiner Quest vorgesehen. Der Start auf echter Hardware, Handtracking, Raumanker und geschützte Anmeldung sind noch nicht praktisch auf einer Quest geprüft. Alle dafür vorgesehenen Schritte stehen in `HAgoesQuest/docs/QUEST-TEST.md` im Quellcodearchiv.
