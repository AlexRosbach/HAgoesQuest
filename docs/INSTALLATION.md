# HA goes Quest on your Quest 3

## Installation

1. Enable developer mode for the Quest and connect it to your PC over USB. Allow USB debugging in the headset. [Meta instructions](https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/).
2. Install the supplied APK with Meta Quest Developer Hub. Alternatively: `adb install -r HAgoesQuest-0.6.3.apk`.
3. Launch **HA goes Quest** from Unknown Sources. Enable hand tracking in Quest settings.

## Updating without re-entering the token

Install the new APK over the existing app, for example with `adb install -r HAgoesQuest-0.6.3.apk`. **Do not uninstall first or clear app data.** The app automatically loads the protected token at startup; saved mappings also remain in app storage.

The connection menu indicates that the saved token is being used. For the same server address, the token field may remain empty. A different address requires a new token. A failed connection attempt does not replace the last confirmed credentials. A token revoked in Home Assistant must be replaced.

## Set up your home

1. Open **Connect to Home Assistant**. Enter the base URL and a Long-Lived Access Token generated in Home Assistant. The keyboard offers **Paste** from the Quest clipboard. Select **Test connection & save**.
2. Under the room capture/map option, start Quest room capture or load existing rooms. Grant room access.
3. Open entity placement and select a device or sensor. Lights can be placed for display only or for switching.
4. Choose **Point & pinch**. If needed, first set the distance between 0.5 and 2 meters. Point at the desired position and pinch once with the hand you want to use. The preview freezes: save the position or continue moving it. Alternatively, direct hand placement follows the hand until a single pinch.
5. Close the menu. Aim the gaze reticle at the icon and pinch with the interaction hand to toggle the light. A pinch of the watch hand opens or closes the menu when not interacting with an entity target or menu control. Alternatively, tap the watch. Briefly release both hands between hand switches.

The main menu's placed-entity management/deletion option can remove mappings even without a visible room anchor.

A small central gaze reticle remains visible for entity interaction; the selected icon becomes brighter. The hand ray remains available in menus and during placement. The circular background and subtle purple border match the menu. From 1.2 m to 30 cm, the icon smoothly grows to 1.8 times its size. Within 30 cm of the headset, its name and value appear. Sensor values also appear above the selected icon and grow with distance, using head direction. You manually associate each physical light with its Home Assistant entity once. The map uses Meta room capture; the blue line shows your walking path during the current session.

Search supports names and IDs, combined with type and room filters. All entity types can be displayed; lights and switches can also be toggled. The Quest must recognize saved positions again. If a display is missing, use the saved-position recovery action in the relevant room.

## QR credentials

In the connection menu, choose the token QR scan option and start scanning, then grant camera access. The QR code may contain the token as text or JSON with **baseUrl** and **accessToken**. Check the address after scanning and connect. The included QR helper creates the code locally on a phone or computer. Extract both files from the ZIP together and open index.html. Requires Quest 3/3S with Horizon OS 74 or later.

## Magic mode

Enable it in settings. Look at an entity, open your hand until ready, make a fist, then open it again. Rotating instead adjusts brightness/volume. For details, the watch, and right-/left-handed mode, see the [0.6.3 guide](VERSION-0.6.3.md).

## Development status

This version is intended for an initial test on your Quest. Startup on real hardware, hand tracking, room anchors, and protected credentials have not yet been practically verified on a Quest. The planned steps are in the [Quest acceptance checklist](QUEST-TEST.md).

## Sliders, watch, and media frames

Brightness and volume can be adjusted using the displayed slider or fist rotation. For the fist gesture, look at the target, hold your hand open until ready, make a fist, and rotate. Once the fist is recognized, you may look away while the hand remains tracked; opening commits the value. Room changes, tracking loss, and opening the menu cancel the action.

The list shows eight entities with values and rooms; pinch and drag or swipe directly. Settings include German/English, click sounds, and startup with the HA watch. The watch opens a stationary menu. Camera and image entities can be placed as frames; cameras show a sequence of current snapshots, not a smooth video/audio stream. See the [technical reference](documentation.md#areas-and-media).

## More help

[Knowledge base](knowledgebase.md) for token, room, and gesture problems · [Technical reference](documentation.md) · [Changelog](../CHANGELOG.md).
