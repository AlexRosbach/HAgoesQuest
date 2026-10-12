# Version 0.6.3 – Safe navigation and both hands

## Rooms and entity list

Room selection loads a separate Home Assistant area catalog. Areas without supported entities are selectable too. An already-open room menu refreshes when loading completes. On error, previously loaded assignments remain available; reload them under Search & filter. HA areas and scanned Quest rooms remain distinct.

After changing pages, release any pinch or touch first. A short quiet period prevents the same click from selecting an entity on the new page. When targeting the toolbar, the pinch ray takes precedence over incidental finger proximity to the list. Canceled list movements do not select a row.

## Both hands, one active action

The first active hand owns interaction. The second cannot take over an ongoing gesture; briefly release both hands between switches. The preferred interaction hand in settings decides simultaneous starts. Watch position is independent of this.

Look at an entity and pinch with either hand to toggle it. For brightness or volume, briefly open the hand, then make a fist and rotate. Once the fist is recognized, the entity remains captured. Looking away does not stop adjustment, and head movement does not change its rotation reference. Open the hand to commit the value. Tracking loss, room changes, and menu opening still cancel. Magic mode likewise retains the throw target until opening.

The watch-hand menu pinch is available outside an entity target or menu control. While controlling an entity, its gesture takes precedence. Alternatively, tap the watch with the other hand.

## Readiness effect

Three open emerald light arcs with soft edges follow the palm in three dimensions. They leave the real hand visible and become more pronounced with a closed fist. No artificial hand or interaction surface. The effect can still be disabled separately.

## Update and acceptance

Version 0.6.3 / Android code 12. Package name, signature, token storage, and anchor formats remain unchanged. Install over the existing installation; do not uninstall or clear data. APK upload remains manual.

Automated evidence is in [BUILD-STATUS.md](BUILD-STATUS.md). Still to check on Quest: both hands and rapid switching, Back through pinch and touch, asynchronously loaded HA areas, looking away during adjustment, moving through doorways, and the new effect's strength and spatial position.

Area queries follow the official [Home Assistant documentation](https://www.home-assistant.io/template-functions/area_entities/).
