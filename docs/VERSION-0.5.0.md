# 0.5.0 – Interaction, rooms, and media

This version combines the tracking fix and subsequent feature requests. The unpublished 0.4.2 designation is dropped. Android version code: 8; package name and development signature remain unchanged.

## Interaction

- **Right hand:** Extend the index finger. The ray remains visible without a hit and brightens when it hits an entity. Pinching selects the target. At close range, tap it with a fingertip. Ray and poke selection use Meta Interaction SDK 207.
- **Left hand:** A pinch opens the menu; after release, another pinch closes it. Long system gestures and the Meta system interface are not bypassed.
- **Lists:** Eight fully visible entries with name, value, and room; reach further entries by pinch-and-drag or direct swiping. Search, type, and room filters can be combined.
- **Dimming / volume:** Point at a supported icon; a sun/speaker symbol appears beneath it. Use this to open the slider. Pinch and drag sideways or drag directly on the slider; release sends the value. Also available through placed-entity details. HA must report brightness or `VOLUME_SET` support. This does not make sensors switchable.
- **Sensors:** In addition to the existing close view, values appear above the icon when looking directly at it from a distance. Quest 3 uses head direction for this.

## Interface and startup

Dark neutral panels, bright text, subtle purple selection, consistent geometric icons, and a brief startup animation. Settings switch between German and English and enable/disable switching sounds. Confirmed acceptance of a light command produces a soft click; a switch produces a more mechanical sound. The HA device state is queried separately afterward.

Optionally, the app starts with an HA watch on the left wrist. Tapping opens a **stationary** panel in front of the user. It does not follow the arm, reflecting the later requirement to follow Meta guidance. A left pinch remains available as an alternative. Access to the connection menu remains available without saved credentials.

Address, token, and search fields use the Meta system keyboard on Quest. A fallback keyboard remains in the Unity editor. Opening the system keyboard may briefly take input focus.

## Learning magic

Start the practice target under Settings → Magic. It does not switch HA devices. Open the hand, face the palm toward the target, wait for the green readiness hint, then thrust approximately twelve centimeters forward. Pull back or briefly close the hand afterward. The allowed target angle and readiness interval are more forgiving than before; live hints show missing steps. Multiple flight animations may overlap. Magic remains experimental and is not a standardized Meta interaction pattern.

## Images and cameras

`image.*` and `camera.*` can be placed as frames. Placement orientation is stored in the Spatial Anchor so the frame does not constantly turn toward the head. Image entities update approximately every ten seconds; cameras provide a sequence of current snapshots with up to two requests per second plus network time. **No smooth video/audio stream.** Timestamps and errors distinguish current from stale images. At most two requests run concurrently; large or unsupported responses are discarded. Hidden frames stop fetching and release their image.

Requests use only the configured HA server and its authenticated proxy endpoints. External `entity_picture` URLs and HTTP redirects do not receive the HA token. The app does not persist camera images.

## Rooms and updates

Each entity retains its own saved anchor UUID. HA rooms serve selection and are not automatically the same records as Meta's captured physical rooms. A room change must not recreate existing mappings at the startup position. Icons appear only after stable, currently tracked localization; missing anchors are retried. [Tracking details](TRACKING-RECOVERY.md).

Install the APK over the existing installation. Do not uninstall or clear app data: the token and mappings reside in existing app storage.

## Documentation and acceptance

[Research](DESIGN-RESEARCH.md) · [Multi-room and Quest acceptance](QUEST-TEST.md) · [Build status](BUILD-STATUS.md).

Automated checks can verify state logic, HA commands, UI structure, and stored mappings. They do not establish that Quest correctly relocalizes in a real building, recognizes all gestures reliably, or maintains the required frame rate. Device tests remain necessary before approval for sale.
