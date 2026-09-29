# Version 0.6.3 – sichere Navigation und beide Hände

## Räume und Entitätenliste

Die Raumauswahl lädt einen eigenen Home-Assistant-Bereichskatalog. Auch Bereiche ohne unterstützte Entitäten sind auswählbar. Ein bereits geöffnetes Raummenü aktualisiert sich, sobald der Abruf fertig ist. Bei einem Fehler bleiben zuvor geladene Zuordnungen erhalten; unter „Suchen & Filtern“ lassen sie sich erneut laden. HA-Bereiche und gescannte Quest-Räume bleiben unterschiedliche Dinge.

Nach einem Seitenwechsel müssen Pinch oder Berührung erst gelöst werden. Eine kurze Ruhephase verhindert, dass derselbe Klick eine Entität auf der neuen Seite auslöst. Beim Zielen auf die Werkzeugleiste hat der Pinch-Strahl Vorrang vor einem zufällig nahen Finger an der Liste. Abgebrochene Listenbewegungen wählen keine Zeile aus.

## Beide Hände, eine laufende Aktion

Die erste aktive Hand übernimmt die Bedienung. Die zweite Hand kann eine laufende Geste nicht übernehmen; beide Hände müssen zwischen einem Wechsel kurz loslassen. Bei gleichzeitigem Beginn entscheidet die bevorzugte Bedienhand aus den Einstellungen. Die Position der Uhr bleibt davon unabhängig.

Eine Entität ansehen und mit links oder rechts pinchen schaltet sie. Für Helligkeit oder Lautstärke zunächst die Hand kurz öffnen, dann eine Faust machen und drehen. Sobald die Faust erkannt ist, bleibt die Entität fest gewählt. Wegschauen beendet die Regelung nicht, und Kopfbewegungen verändern nicht deren Drehreferenz. Hand öffnen übernimmt den Wert. Trackingverlust, Raumwechsel oder Menüöffnung brechen weiterhin ab. Im Magiemodus bleibt auch das Wurfziel bis zum Öffnen festgehalten.

Der Menü-Pinch der Uhrenhand ist außerhalb eines Entitätsziels bzw. Menübedienelements verfügbar. Beim Bedienen einer Entität hat deren Geste Vorrang. Alternativ die Uhr mit der anderen Hand antippen.

## Bereitschaftseffekt

Drei offene smaragdgrüne Lichtbögen mit weichen Rändern folgen der Handfläche in drei Dimensionen. Sie lassen die echte Hand frei sichtbar und werden bei einer geschlossenen Faust deutlicher. Keine künstliche Hand und keine Interaktionsfläche. Der Effekt bleibt separat abschaltbar.

## Update und Abnahme

Version 0.6.3 / Android-Code 12. Paketname, Signatur, Token-Speicher und Ankerformate bleiben unverändert. Über die vorhandene Installation installieren; nicht deinstallieren oder Daten löschen. APK-Upload bleibt manuell.

Automatisierte Nachweise stehen in [BUILD-STATUS.md](BUILD-STATUS.md). Auf der Quest noch zu prüfen: beide Hände und schneller Wechsel, Zurück per Pinch und Antippen, asynchron geladene HA-Bereiche, Wegschauen während der Regelung, Bewegung durch Türen sowie Stärke und räumliche Lage des neuen Effekts.

Bereichsabfrage nach der offiziellen [Home-Assistant-Dokumentation](https://www.home-assistant.io/template-functions/area_entities/).
