# Wissensdatenbank

Hilfe zum aktuellen Stand **0.6.3**. Zurück zur [Dokumentationsübersicht](README.md).

## Nach einem Update wird wieder ein Token verlangt

**Symptom:** Das Verbindungsmenü bietet keine gespeicherte Anmeldung an.

**Bedeutung:** App-Daten fehlen, der Keystore-Schlüssel ist nicht verfügbar oder die Serveradresse wurde geändert. Ein widerrufener HA-Token kann ebenfalls nicht weiterverwendet werden.

**Prüfen:** APK über die vorhandene Installation installieren, ohne Deinstallation oder Datenlöschung. Dieselbe Basisadresse einschließlich Schema und Port verwenden. Bei derselben bestätigten Adresse darf das Tokenfeld leer bleiben. Schlägt bereits die Installation wegen einer Signaturabweichung fehl, die passende APK verwenden; eine Deinstallation würde die gespeicherten Daten entfernen.

## Home Assistant ist nicht erreichbar oder meldet 401/403

**Symptom:** Verbindungsprüfung scheitert, Zustände werden als veraltet angezeigt.

**Bedeutung:** 401/403 weist auf Zugangsdaten oder Berechtigungen hin. Timeout, DNS- oder Zertifikatsfehler betreffen die Erreichbarkeit aus dem Quest-Netzwerk.

**Prüfen:** Basisadresse ohne `/api`, Pfad, eingebettete Zugangsdaten oder Query verwenden. Token im HA-Benutzerprofil prüfen. WLAN, DNS und HTTPS-Zertifikat kontrollieren. Lokales HTTP benötigt die ausdrückliche Freigabe in der App. Bei unbestätigtem Schaltbefehl erst den aktuellen Gerätezustand prüfen: Die App wiederholt Befehle nicht automatisch.

## Räume oder Bereiche fehlen im Filter

**Symptom:** Nur „Alle Räume“ und „Ohne Raumzuordnung“ sind sichtbar oder eine Zuordnung fehlt.

**Bedeutung:** Der HA-Bereichskatalog ist noch nicht geladen, die Template-Anfrage ist fehlgeschlagen oder die Entität hat in HA keine Bereichszuordnung.

**Prüfen:** Unter **Suchen & Filtern → Raumzuordnungen neu laden** erneut abrufen und die Statusmeldung beachten. In HA die Bereichszuordnung der Entität bzw. ihres Geräts kontrollieren. Version 0.6.3 zeigt auch leere Bereiche und aktualisiert eine offene Auswahl nach dem Laden. Die Quest-Raumerfassung erzeugt keine HA-Bereiche.

## Ein Icon fehlt oder erscheint im falschen Raum

**Symptom:** Nach dem Start oder beim Raumwechsel wird eine gespeicherte Entität nicht richtig angezeigt.

**Bedeutung:** Die App wartet auf einen stabilen Anker oder der Quest-Raum wurde nicht korrekt lokalisiert. Nur Entitäten im aktuellen physischen Raum werden angezeigt.

**Prüfen:** Im zugehörigen Raum umsehen und kurz warten. Bei Bedarf **Gespeicherte Positionen wiederfinden** verwenden. Raumzugriff und gespeicherte System-Raumdaten prüfen. Bleibt eine Position falsch, den Ablauf nach [QUEST-TEST.md](QUEST-TEST.md) nachvollziehen; eine dauerhaft falsche Betriebssystem-Lokalisierung wird durch Desktop-Tests nicht ausgeschlossen. Eine Zuordnung lässt sich über die Verwaltung entfernen und anschließend neu platzieren.

## Nach „Zurück“ reagiert die nächste Seite zunächst nicht

**Symptom:** Direkt nach einem Seitenwechsel wird ein gehaltener Pinch oder anliegender Finger ignoriert.

**Bedeutung:** Die Loslass-Sperre verhindert einen unbeabsichtigten Klick auf die neue Seite.

**Lösung:** Pinch lösen bzw. den Finger von der Menüfläche zurückziehen, kurz warten und neu auswählen. Falls trotzdem eine darunterliegende Entität geöffnet wird, Version prüfen und für einen Fehlerbericht festhalten, ob Strahl/Pinch oder direktes Antippen benutzt wurde.

## Die andere Hand übernimmt nicht sofort

**Symptom:** Während einer Aktion bleibt die zweite Hand ohne Wirkung.

**Bedeutung:** Es darf nur eine Hand gleichzeitig bedienen. Eine bereits gehaltene Geste der zweiten Hand übernimmt die erste nicht.

**Lösung:** Laufende Aktion beenden und beide Hände kurz lösen, dann mit der gewünschten Hand neu beginnen. Bei gleichzeitigem Beginn entscheidet die eingestellte Handpräferenz. Nach einem Quest-Systemdialog beide Hände wieder ins Tracking bringen und eine frische Geste beginnen. Weitere Hintergründe: [Tracking-Wiederherstellung](TRACKING-RECOVERY.md).

## Dimmen oder Lautstärke bricht beim Wegschauen ab

**Symptom:** Der Vorschauwert verschwindet, ohne übernommen zu werden.

**Bedeutung:** Ab 0.6.3 bleibt das Ziel nach erkannter Faust festgehalten. Vorheriger Zielwechsel, Verlust des Handtrackings, Raumwechsel oder Menüöffnung brechen jedoch ab.

**Lösung:** Ziel ansehen, offene Hand bis zur Bereitschaft halten, Faust schließen, dann drehen. Erst nach der erkannten Faust wegschauen und die Hand im Trackingbereich halten. Öffnen übernimmt. Die Entität muss Dimmen bzw. Lautstärke laut ihren HA-Fähigkeiten unterstützen.

## Magie löst nicht aus oder regelt stattdessen einen Wert

**Symptom:** Kein Feuerball beim Öffnen oder Helligkeits-/Lautstärkeregler statt Wurf.

**Bedeutung:** Die aktuelle Geste ist „offene Hand → Faust → öffnen“. Die frühere Stoßgeste gehört zur Versionshistorie. Faustdrehen hat Vorrang vor dem Wurf.

**Lösung:** Magie aktivieren und zunächst das Übungsziel verwenden. Bereitschaft abwarten, Faust schließen und ohne Drehbewegung öffnen. Nach jedem Wurf neu beginnen. Gestenuntertitel können zur Diagnose eingeschaltet werden.

## Kamera zeigt Einzelbilder oder bleibt stehen

**Symptom:** Kein flüssiger Video-/Audiostream.

**Bedeutung:** Medienrahmen verwenden HA-Proxy-Einzelbilder. Kameras warten mindestens 0,5 s zwischen abgeschlossenen Anfragen; verborgene oder entfernte Rahmen werden pausiert.

**Prüfen:** Kamera bzw. Bild in HA öffnen, Berechtigung und Verbindung prüfen und den Rahmen ansehen. Große oder nicht dekodierbare Bilder werden abgewiesen. Externe Bild-URLs werden nicht mit dem HA-Token aufgerufen.

## Welche Angaben helfen bei einem Fehlerbericht?

App-Version, Horizon-OS-Version, genaue Schritte, verwendete Hand, Eingabeart, Zeitpunkt eines Raum-/Systemdialogwechsels und die sichtbare Statusmeldung nennen. Bei Bedarf einen bereinigten Screenshot ergänzen. Token, QR-Zugangscodes, private Konfigurationsdateien und unbereinigte SDK-Zugangsdaten nicht veröffentlichen.
