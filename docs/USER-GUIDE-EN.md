# HA goes Quest 0.6.0

Install the APK over your existing installation to retain your token and placements. Do not uninstall or clear application data.

1. Connect to your Home Assistant server. Enter the base URL and a long-lived token, or scan its QR code. Text entry uses the Quest system keyboard. Plain HTTP requires explicit opt-in.
2. Use Quest room setup to capture the rooms. Your Home Assistant areas organize the entity browser; they are separate from Meta's physical room records.
3. Search or combine type and room filters. The scrolling list shows eight complete entries with their current values. Pinch and drag vertically, or swipe directly with your index finger.
4. Select an entity, point to its position or move your hand there, pinch once to freeze the preview, and confirm. Sensors are display-only. Camera and image entities become fixed frames.
5. Aim the center dot at an entity and pinch with your control hand to select. Keep the other fingers loosely extended. Menus retain ray and direct index-finger touch. Pinch the watch hand to toggle the menu.
6. Look at an adjustable light or speaker, briefly open your hand until ready, close your fist and twist. Opening sends the displayed brightness or volume once. Looking away or losing tracking cancels. Sliders remain available as an alternative.
7. Look directly at a sensor icon to reveal its reading from a distance. Quest 3 uses head direction, not eye tracking, for this behavior. Text scales with distance between 0.45 and 8 meters.
8. Settings include English/German, sound, wrist-watch startup and handedness. Left-handed mode mirrors the watch and control hand. Touching the watch opens a stable panel in front of you; it does not follow the moving wrist.

Optional magic has a practice target that never controls devices. Look at the target and briefly open the control hand until ready. Close your fist, then open to cast. Twisting takes priority: opening then confirms the value and does not cast. Follow the live prompts. This experimental gesture is separate from standard pinch/poke input.

Camera frames show repeatedly refreshed snapshots, up to two requests per second plus network latency. They are not full-rate video streams. Static HA image entities refresh about every ten seconds. Only the configured HA server receives the access token; frames stop fetching when hidden.

After startup, recentering or room setup, icons remain hidden until their anchors are tracked and stable. Look around the corresponding room if an anchor has not been found. The app retries without creating replacement positions. Existing incorrectly localized anchors still require on-device diagnosis; desktop tests cannot validate your actual room map.

This APK is a development build. See `COMMERCIAL-READINESS.md` for licensing and production-release requirements.

## 0.6.1: contrast and wrist placement

Open Settings → Your HA watch → Adjust position & size. Keep the watch hand visible. Use − / + to move the dial in 5 mm steps, rotate it in 5° steps and resize it in 5% steps. Save position enables the watch and stores its pose relative to that wrist. Cancel or close the menu to discard changes. Left and right wrists are stored independently; Reset watch position restores the automatic placement for the current wrist.

Menu surfaces are now brighter and opaque. Start tiles use original detailed meshes and generated ceramic/brushed-metal texture maps. No external model/texture license was introduced. Headset validation is still required.

## 0.6.2: room scope and gesture feedback

Search & filters remain at the top of the entity list. Menus now share one rendering hierarchy so backgrounds cannot cover buttons when viewed from the side. Placed entities are visible and controllable only within the same scanned Quest room as your headset; room height is included. Leaving the room cancels adjustments. The full HA catalogue remains available for initial placement, with separate HA area/type/search filters.

Fist rotation and sliders produce quiet mechanical detents when values change. A translucent green glow follows your real tracked hand when the gesture is ready. Settings → Sound & gesture feedback offers independent switches for sounds, gesture captions and hand glow. No virtual hand model is added.
