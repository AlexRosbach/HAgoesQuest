# Interaktion und Gestaltung: recherchierte Entscheidungen

Aktueller Stand: 0.6.0. Der Abschnitt „Änderung in 0.6.0“ ersetzt die frühere Entitätsauswahl per Handstrahl und die Stoßgeste. Die folgenden ursprünglichen Entscheidungen dokumentieren die Grundlage aus 0.5.0; die aktuelle Bedienanleitung steht in [VERSION-0.6.0.md](VERSION-0.6.0.md).

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

## Änderung in 0.6.0

Die neu gewünschte Entitätsbedienung verwendet einen sichtbaren Punkt in Kopf-Blickrichtung, bestätigt durch Pinch. Die Fingerzeiger bleiben ausschließlich für Menü und Platzierung, damit zwei Fernziel-Modi nicht gleichzeitig um dieselbe Entität konkurrieren. Eine kurze Klassifikationszeit und Fingerhaltung unterscheiden Pinch und Faust. Faustdrehen und Faustöffnen sind eigene App-Gesten, keine von Meta garantierten Standardgesten.

Die Hauptansicht ordnet Aktionen in große Kacheln mit einer primären Aktion. Selbst erstellte räumliche Symbole dienen als zusätzliche visuelle Hinweise; die beschriftete Kachel bleibt die unveränderte Trefferfläche. Die Uhr wird für Linkshänder gespiegelt und öffnet weiterhin ein ortsfestes Panel. Sensorbeschriftungen skalieren proportional zur Entfernung mit Unter-/Obergrenze.

Ergänzende Quellen: [Button-Hierarchie und Trefferflächen](https://developers.meta.com/horizon/design/buttons_bp/), [Button-Zustände](https://developers.meta.com/horizon/design/buttons_specs/), [winkelabhängige Skalierung](https://developers.meta.com/horizon/design/mr-design-guideline/), [Wrist button](https://developers.meta.com/horizon/design/wrist-buttons/). Die Plattformwerte in dp lassen sich nicht pauschal mit den Weltmetern der eigenen Unity-Oberfläche gleichsetzen. Lesbarkeit und Trefferflächen müssen deshalb zusätzlich in der Brille geprüft werden.

## 0.6.1: Lesbarkeit und Materialien

[Meta Accessibility](https://developers.meta.com/horizon/design/accessibility/), abgerufen am 29. September 2026: mindestens 4,5:1 für normalen Text, ausreichend große Ziele und sichtbare Fokusrückmeldung. Die drei Menütext-Paletten werden automatisiert gegen 4,5:1 geprüft. Die dunklen fast schwarzen Flächen wurden durch deckendes Schieferblau ersetzt. Unity-Vorschauen zeigen Startmenü und Uhreneditor; eine reale Quest-Abnahme ersetzt das nicht.

Die 3D-Modelle und 128×128-Materialtexturen entstehen vollständig aus eigenem Code in ButtonSculpture.cs. Die kamerabezogene Schattierung behält eine helle Grundbeleuchtung, sodass die Modelle unabhängig von realer oder virtueller Raumbeleuchtung lesbar bleiben. Pro Objekt werden Teile nach Material zusammengefasst. Der Uhreneditor verwendet große einzelne Tasten statt einer präzisen Ziehgeste; erst Speichern übernimmt die Anpassung.

## 0.6.2: Room boundaries and UI ordering

Meta MRUK 207 provides GetCurrentRoom and IsPositionInRoom with vertical-bounds checking: [MRUKRoom reference](https://developers.meta.com/horizon/reference/mruk/v207/class_meta_x_r_m_r_utility_kit_m_r_u_k_room/) and [scene data queries](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-manage-scene-data/). The app validates headset containment instead of trusting a stale current-room result, then checks entity positions against the same room. Missing rooms fail closed outside editor/demo mode.

The installed Unity uGUI source confirms RectMask2D clips descendants in their root canvas space. The fixed filter toolbar is outside that subtree. Menu surfaces now share a root canvas and the panel is the first sibling, removing independent per-card transparent distance sorting. Desktop renders at frontal and ±35° views supplement structural tests; Quest stereo testing remains necessary.
