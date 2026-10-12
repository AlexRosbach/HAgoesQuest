# 0.6.0 – Gaze reticle, fist gestures, and new interface

## Controlling entities

A small dot remains in the center of the field of view. Aim it at an icon: the icon smoothly brightens. Pinch with the right interaction hand to toggle the selected light or switch. Keep the other fingers loosely extended. Very short pinches do not trigger: 120 ms distinguishes an intentional pinch from the start of a fist. Holding does not send a second command.

Quest 3 has no eye tracking. The reticle follows the center of the head, not eye movement. Selection tolerates a small 1.7° angle from the icon center. Looking alone never triggers an automatic action. The hand ray is disabled for entity interaction; SDK pointers and direct touch remain available in menus and placement.

## Brightness and volume

1. Look at a dimmable light or speaker.
2. Briefly open the interaction hand until Ready appears.
3. Close a fist and rotate it like a dial. After a 12° dead zone, the new percentage appears. Movement is measured relative to the initial pose; 120° spans the entire value range.
4. Open the hand: the selected value is sent to HA once. During rotation, it is a preview.

Existing sliders remain an alternative. Gaze changes, opening the menu, focus loss, or tracking loss discard the current gesture. Unsupported entities do not offer adjustment.

## Magic

Enable magic in settings or start the practice target. Look at the target and briefly open the hand: a pulsing green ring indicates readiness. Close a fist, then open without rotating: the fireball flies from the hand to the target. A forward thrust is no longer needed. After a short pause, another fist-open gesture is possible; multiple fireballs may fly simultaneously.

**Rotation takes precedence:** If the fist adjusted a value, opening confirms it and does not throw a fireball. Sensors, display-only entities, and unavailable devices are not switched. Practice does not switch real devices.

## Sensors

The selected sensor icon shows its name and value above it. Text is larger and grows proportionally with distance to remain approximately the same angular size. Scaling is clamped between 0.45 and 8 meters and is independent of the icon's enlargement when approached.

## Watch and menu

Under Settings → Watch & interaction hand, enable the watch and select right- or left-handed mode. Default: watch left, interaction right. Left-handed mode puts the watch on the right and assigns selection, rotation, magic, and menu interaction to the left hand. Tap the dial with the opposite hand to open or close the menu. The watch-hand pinch remains an alternative.

The watch has a dial rim with ticks, time, date, and connection status. The menu still opens stationary in front of the user. The main view has larger tiles with original 3D symbols, a prominent primary action, separate management/close actions, and a shared panel background. The entity list remains scrollable.

## Design and acceptance

The design follows Meta guidance: clear primary/secondary actions, adequate hit targets and spacing, recognizable selection states, direct index-finger touch, and angular text scaling. The custom fist gesture is a project-specific extension, not a certified Meta interaction pattern. Sources: [Buttons](https://developers.meta.com/horizon/design/buttons/), [Button recommendations](https://developers.meta.com/horizon/design/buttons_bp/), [Panels](https://developers.meta.com/horizon/design/panels/), [Hand UI](https://developers.meta.com/horizon/design/hands-ui-best-practices/), [Angular scaling](https://developers.meta.com/horizon/design/mr-design-guideline/), [Wrist buttons](https://developers.meta.com/horizon/design/wrist-buttons/).

Before using on Quest, check pinch/fist distinction, right-/left-handed mode, watch tapping, rotation in both directions, opening after tracking loss, repeated throws, text across rooms/distances, and frame rate. Desktop tests verify state logic and structure, not actual hand recognition.

## Update

Version 0.6.0 / Android code 9. Install over the existing installation; do not uninstall or clear app data. Signature, token storage, and room mappings remain compatible.
