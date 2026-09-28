# 0.4.0 – Grüne Magie

Unter **Einstellungen · Magie & Positionen → Magie einschalten** aktivieren, anschließend das Menü schließen. Die Einstellung wird auf der Quest gespeichert und ist anfangs aus.

Die rechte Hand flach zum gewünschten Icon richten und kurz ruhig halten. Der Rand leuchtet grün, bei erkannter Bereitschaft heller. Dann die Hand etwa 12 cm zügig in Richtung der Entität stoßen. Hand anschließend zurücknehmen oder kurz schließen. Die Erkennung verlangt eine gezielte Vorwärtsbewegung; langsames Hinreichen, seitliche Bewegungen, Tracking-Sprünge und eine dauerhaft ausgestreckte Hand sollen nicht schalten. Nach einer Auslösung gilt zusätzlich eine Pause von 1,2 Sekunden.

Die Erkennung arbeitet nur bei geschlossenem Menü, gültigem Handtracking, aktuellen HA-Zuständen und einer schaltbaren, lokalisierten Platzierung. Sie ist während Raumerfassung, Platzierung und Systemdialogen deaktiviert. Pinch-Bedienung bleibt außerhalb des kurzen Feuerball-Effekts verfügbar. Es werden keine virtuellen Hände gerendert.

Der grüne Feuerball startet an der Hand, fliegt mit wirbelndem Schweif zur Entität und zerfällt in einen Ring und Funken. Die Grafik wurde mit dem eingebauten Imagegen-Werkzeug generiert; Flug, Rotation, Schweif und Aufprall werden von Unity animiert. Der Schaltbefehl wird einmal bei Erkennung gesendet. Die Animation visualisiert den Befehl, sie bestätigt keinen erfolgreichen HA-Schaltvorgang. Maßgeblich bleiben der zurückgemeldete Zustand und der Verbindungsstatus.

Paketname, Signatur und Token-Speicherung bleiben kompatibel. Version 0.4.0, Versionscode 6. Über die vorhandene App installieren, nicht deinstallieren.

## Gerätetest

Magie einschalten, Menü schließen, rechte flache Hand zum Lampen-Icon richten, kurz halten und nach vorn stoßen. Zielauswahl, Palmrichtung, Schwellenwerte, Effektgröße und Transparenz auf der Quest prüfen. Auch geschlossene Hand, Gehen, seitliches Winken, Trackingverlust, Sensoren, ausgeschaltete Magie und erneutes Öffnen des Menüs testen. Gesten und Darstellung sind noch nicht auf echter Quest-Hardware bestätigt.
