# HA goes Quest – user guide

Current behavior: **0.6.3**. [Documentation index](README.md) · [Troubleshooting](knowledgebase.md) · [Release history](../CHANGELOG.md).

## Install and connect

Install the APK over your existing installation to retain the token and placements. Do not uninstall or clear application data. Use Meta Quest Developer Hub or:

```text
adb install -r HAgoesQuest-0.6.3.apk
```

Connect to your Home Assistant base URL with a long-lived access token, or scan its QR code. Review the address before connecting. Text entry uses the Quest system keyboard. Plain HTTP requires explicit opt-in. An empty token field can reuse the saved login for the same validated server address.

## Capture rooms and place entities

1. Use Quest room setup to capture the rooms or load existing room data. Home Assistant areas organize the entity browser; they are separate from Meta's physical rooms.
2. Search by name or ID and combine type and area filters. The scrolling list shows eight complete entries with current values. Pinch and drag vertically, or swipe directly with your index finger.
3. Select an entity, point to a position or move your hand there, pinch to freeze the preview, then confirm. Sensors are display-only. Camera and image entities become fixed frames.
4. Close the menu and aim the center dot at an entity. Pinch to switch a supported light or switch. Keep the other fingers loosely extended. Quest 3 uses head direction, not eye tracking.

Placed entities are visible and controllable only in the same scanned Quest room as your headset. The full HA catalog remains available for initial placement. Returning to a room restores its visible markers once tracking is stable. Missing anchors are retried; use the saved-position recovery action in the relevant room if needed. Delete unwanted placements through the management menu.

## Both hands and adjustment gestures

Either hand can operate an entity. The first active hand owns the action; release both hands briefly before switching. The handedness preference breaks simultaneous ties and selects the watch side. Menus retain ray selection and direct index-finger touch.

For brightness or volume, look at a supported light or speaker, briefly open your hand until ready, close your fist and twist. The closed fist captures the target and rotation reference. **Looking away does not cancel the adjustment or change the value through head movement**, provided hand tracking continues. Opening the hand applies the displayed value once. Tracking loss, room changes or opening a menu still cancel. Sliders remain an alternative.

Navigation requires a released pinch or withdrawn finger before the next page accepts input. This prevents Back from also selecting a row on the next page.

## Watch, readings and feedback

Look at a sensor icon to reveal its reading from a distance. Text scales with distance between 0.45 and 8 meters. The app does not automatically identify which real lamp corresponds to an HA entity; assign it manually once.

Settings include English/German, watch startup and handedness. Open the watch settings to adjust position, rotation and size relative to the wrist. Save stores the pose for that wrist; Cancel discards the preview. Left and right wrist placements are independent. Reset restores the automatic position.

Tap the watch with the other hand to open a stable panel in front of you. The panel does not follow the moving wrist. The watch-hand pinch also toggles the menu when an entity or menu control is not being targeted.

Fist rotation and sliders produce quiet detent sounds as values change. Open emerald light contours follow the real hand when a gesture is ready; no virtual hand model is added. Sounds, gesture captions and hand effects have independent switches.

## Magic and media

Optional magic has a practice target that never controls devices. Look at the target, open your hand until ready, close your fist, then open it to cast. Twisting takes priority: an adjusted grip applies the level instead of casting.

Camera frames show repeatedly refreshed snapshots, with at least half a second between completed requests plus network latency. They are not video/audio streams. Static HA image entities refresh about every ten seconds. Hidden frames stop fetching. Only the configured HA server receives the token.

## Project status

The maintainer reports successful testing on Meta Quest. Active development is currently paused; see the [case study](../README.md#about-this-case-study).
