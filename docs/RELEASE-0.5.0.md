# HA goes Quest 0.5.0

Better orientation, more reliable selection, and an expanded Home Assistant interface on Quest 3.

- Meta Interaction SDK for pointing, pinching, and touch. Glowing finger ray with clear hit feedback; no extra waiting filter before selection.
- Scrollable selection with eight visible entities, current values, room/type filters, and search. New geometric icons, dark neutral panels, startup animation, and original click sounds.
- Adjust brightness and volume on supported HA entities: open the slider, pinch and drag sideways, then release. Sensor values also appear from a distance when viewed directly.
- Optional left-arm HA watch opens a stationary menu; Meta system keyboard and German/English.
- Magic with practice target and live guidance; repeated fireballs remain possible.
- Show saved anchors only after stable tracking, recover them in parallel, and retry on room changes. Focus loss and recalibration discard old inputs and frozen placements.
- Image and camera entities as spatial frames. Cameras show current snapshots, up to two requests per second plus network time, not a smooth video/audio stream.

## Installation

Install `HAgoesQuest-0.5.0.apk` over the existing app, for example with `adb install -r HAgoesQuest-0.5.0.apk`. Do not uninstall first or clear app data. Package name, signature, and storage formats remain compatible for tokens and mappings. Android version code: 8.

## Verified and outstanding

77 core checks and 82 Unity integration checks passed, including SDK selection, scrolling, sliders, four HA area assignments, saved anchor IDs, and authenticated images. Menu checked in the Unity renderer. These tests use a local HA test server.

Installation, system keyboard, real multi-room localization, hand gestures, and performance require Quest acceptance. Documentation includes Meta design research and a Quest acceptance checklist.

The APK is intended as a manually uploaded release asset. GitHub Actions source-code tests do not build an APK.
