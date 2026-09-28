# Interaktion und Gestaltung: recherchierte Entscheidungen

Recherche: 29. September 2026; Primärquellen von Meta und Home Assistant.

## Metas Empfehlungen und Umsetzung

Die App verwendet eine dominante rechte Hand für Zeigen und Pinchen; im Nahbereich ist Antippen mit dem Zeigefinger vorgesehen. Die Auswahl benutzt die vom Meta-Runtime gelieferte Hand-Aim-Pose, ergänzt durch Glättung und eine kurze Stabilisierung beim Pinch-Beginn. Der frühere zusätzliche Fokus-Zeitfilter vor dem rechten Pinch entfällt. Das soll verpasste Auswahl reduzieren; die tatsächliche Zuverlässigkeit muss auf Quest geprüft werden.

Der sichtbare Strahl ist beim Zeigen auch ohne Treffer vorhanden. Ein Treffer auf ein Entity-Icon verstärkt Helligkeit und Leuchtrand. Schaltgeräusche folgen einer vom Server angenommenen Aktion, nicht einem bloßen Blickkontakt. Der resultierende Gerätezustand wird weiterhin separat aus HA gelesen.

Listen zeigen acht vollständig bedienbare Einträge und angeschnittene Folgeeinträge. Pinch-and-drag und direktes Wischen dienen zum Scrollen; ein kleiner Scrollbalken ist kein nötiges Eingabeziel. Pinch-und-Ziehen auf einem beschrifteten Regler verändert Helligkeit oder Lautstärke. Beim Loslassen wird genau der gewählte absolute Wert angefragt. Normale Bedienung erfordert keine frei erfundenen Richtungszeichen oder unbestätigte automatische Blick-Schaltbefehle.

Große Menüs bleiben ortsfest und stehen in etwa 1,4 m Abstand. Die optionale Uhr am linken Handgelenk dient nur als Öffner; das Panel folgt dem Arm nicht. Das entspricht der späteren Nutzerentscheidung für Metas Empfehlungen. Keine erzeugten Hände verdecken die echten Passthrough-Hände.

Quellen: [Hands UI best practices](https://developers.meta.com/horizon/design/hands-ui-best-practices/), [Input mappings](https://developers.meta.com/horizon/design/interactions-input-mappings/), [Ray casting](https://developers.meta.com/horizon/design/raycasting_usage/), [Input modalities](https://developers.meta.com/horizon/design/interactions-input-modalities/).

## Tastatur, Blickrichtung und Magie

Texteingaben auf Quest verwenden `TouchScreenKeyboard` mit Metas System-Overlay; der eigene Tastatur-Renderer bleibt nur für den Editor. Fokusverlust beim Öffnen des Overlays ist vorgesehen und darf die Handsteuerung anschließend nicht blockieren. [Meta: Enable Keyboard Overlay](https://developers.meta.com/horizon/documentation/unity/unity-keyboard-overlay/).

Quest 3 hat kein Eye-Tracking. „Anschauen“ bezeichnet hier die mittlere Kopf-Blickrichtung. Sensorwerte können dadurch aus der Entfernung eingeblendet werden. Bloßes Anschauen sendet keinen HA-Befehl.

Magie bleibt eine ausdrücklich optionale, experimentelle Zusatzgeste, kein von Meta standardisiertes Bedienmuster. Anleitung, Live-Hinweise und ein Übungsziel erklären Hand öffnen, Ziel finden, ruhig halten, vorwärts stoßen und erneut vorbereiten. Die Standardbedienung bleibt verfügbar. Die visuelle Fluganimation allein bestätigt keinen physischen Gerätezustand.

## Grenzen

Die eigene Welt-UI ist über `RayInteractable` und `PokeInteractable` an Metas Interaction SDK 207 angebunden. Ein Root-Treiber versorgt `RayInteractor` und `PokeInteractor` mit der Runtime-Aim-Pose, dem Pinchzustand und der Fingerspitze. Auswahl und Antippen verwenden die SDK-Zustandsübergänge; Scrollen und Regler interpretieren die anschließende Bewegung. Es ist kein unverändert übernommenes Meta-UI-Set. Dokumentierte Designentscheidungen und Rechnerprüfungen sind keine Meta-Zulassung.
