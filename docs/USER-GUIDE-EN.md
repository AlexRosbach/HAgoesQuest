# HA goes Quest 0.5.0

Install the APK over your existing installation to retain your token and placements. Do not uninstall or clear application data.

1. Connect to your Home Assistant server. Enter the base URL and a long-lived token, or scan its QR code. Text entry uses the Quest system keyboard. Plain HTTP requires explicit opt-in.
2. Use Quest room setup to capture the rooms. Your Home Assistant areas organize the entity browser; they are separate from Meta's physical room records.
3. Search or combine type and room filters. The scrolling list shows eight complete entries with their current values. Pinch and drag vertically, or swipe directly with your index finger.
4. Select an entity, point to its position or move your hand there, pinch once to freeze the preview, and confirm. Sensors are display-only. Camera and image entities become fixed frames.
5. Extend your right index finger to see the aiming ray. Pinch to select; nearby buttons also support direct index-finger touch. Left pinch toggles the menu; release between gestures.
6. Point at an adjustable light or speaker and select the small sun/speaker control below it. Pinch the slider and drag sideways. Releasing sends the selected absolute brightness or volume. Unsupported devices do not offer the control.
7. Look directly at a sensor icon to reveal its reading from a distance. Quest 3 uses head direction, not eye tracking, for this behavior.
8. Settings include English/German, sound and wrist-watch startup. Touching the watch opens a stable panel in front of you; it does not follow the moving wrist.

Optional magic has a practice target that never controls devices. Open your right palm, aim toward the target, hold briefly until ready, then push forward about 12 cm. Pull back or close your hand before the next cast. Follow the live prompts. This experimental gesture is separate from standard pinch/poke input.

Camera frames show repeatedly refreshed snapshots, up to two requests per second plus network latency. They are not full-rate video streams. Static HA image entities refresh about every ten seconds. Only the configured HA server receives the access token; frames stop fetching when hidden.

After startup, recentering or room setup, icons remain hidden until their anchors are tracked and stable. Look around the corresponding room if an anchor has not been found. The app retries without creating replacement positions. Existing incorrectly localized anchors still require on-device diagnosis; desktop tests cannot validate your actual room map.

This APK is a development build. See `COMMERCIAL-READINESS.md` for licensing and production-release requirements.
