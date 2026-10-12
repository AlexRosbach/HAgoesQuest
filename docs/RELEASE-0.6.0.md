# HA goes Quest 0.6.0

- Central gaze reticle replaces the finger ray for entity selection. Animated brightening and an intentional interaction-hand pinch toggle entities.
- Rotate a fist to set brightness or volume as a percentage; opening confirms once. Target changes and tracking loss cancel.
- Magic: look at the target, briefly hold an open hand ready, close a fist, then open. No thrust required. Rotation takes precedence and prevents a fireball on opening.
- Larger sensor labels with proportional distance scaling.
- Revised HA watch with dial, date, and connection status; tap with the opposite hand. Right-/left-handed mode mirrors watch and interaction.
- Larger menu tiles with original 3D symbols, a clear primary action, and click feedback. Design follows Meta panel, button, and hand-UI guidance.

99 core checks and 91 Unity checks passed. Menu and watch checked in the Unity renderer. Actual gesture recognition and multi-room localization require Quest testing.

Version 0.6.0 / Android code 9. Install over the existing installation without uninstalling or clearing app data. Signature, token, and room-anchor formats remain compatible. APK release upload is manual.

Instructions and acceptance steps: `docs/VERSION-0.6.0.md`.
