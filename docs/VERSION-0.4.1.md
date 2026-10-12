# 0.4.1 – Hand pointers after recalibration

## Right hand

Meta's hand prefab lacked a PointerPose root. The SDK writes the pointer pose in local tracking coordinates; after moving/rotating the tracking space, the app therefore treated it as incorrect world coordinates. Both hand pointers are now explicitly assigned to the camera rig's TrackingSpace in the saved scene and at runtime. Assignment is refreshed automatically when needed.

Recalibration, restored input focus, and returning from room capture reset input gestures and realign the menu. The system gesture itself remains excluded while active; after release, a fresh pinch can interact again.

## Left pinch

A fresh left pinch opens the main menu; the next closes the currently open menu or submenu. Release the fingers between pinches. Holding, or retaining a pinch after a focus change, does not trigger another toggle.

## Repeated fireballs

A new thrust gesture is possible after pulling back or closing the hand, even while the previous fireball is still flying. Retraction is observed even when the HA client is temporarily busy. Target hold time: 0.15 s; minimum cooldown: 0.35 s; up to four effects may coexist. Holding the hand extended still does not repeat automatically. Switching commands still require fresh state and an available HA client; commands are not queued.

## Installation and device test

Version 0.4.1, version code 7. Install over the existing APK without uninstalling first; credentials and mappings remain in existing app storage.

On Quest, specifically test: capture rooms again, hold the right thumb/index finger together for recalibration, release, then operate several menu entries with the right hand. Repeatedly open/close with the left hand. Perform multiple separate thrust gestures. These real system dialogs and hand movements have not yet been confirmed on hardware.
