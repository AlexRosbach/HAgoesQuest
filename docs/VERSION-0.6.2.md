# Version 0.6.2 – Filters, room restrictions, and gesture feedback

## Entity list and menu

Search & filter is clearly visible at the top of the list again. Entity types, Home Assistant rooms, and name/ID search remain available there. The current type, room, and search filters are summarized below the list.

Menu surfaces now use a shared world-space canvas. The background draws first within this hierarchy. Previously, independently distance-sorted canvases could cover buttons at oblique angles. The filter bar and Back button sit outside the scroll viewport. Previews check the list, filters, and feedback settings at ±35° side angles as well.

## Only entities in the current room

Placed icons, sensor values, and camera/image frames appear only when the head and entity position are inside the same scanned Quest room. Vertical checking prevents assignment to the floor above or below. A stale or merely nearby room is insufficient. Markers stay hidden outside all recognized rooms.

This restriction also applies to commands: gaze/pinch, fist adjustment, and slider menus cannot control an entity in another room. Room changes discard active adjustments, gestures, and placements. New positions must be inside the current room. Existing anchors are not deleted and reappear on return once tracking is stable.

The complete HA list remains available for initial selection and placement; its HA room filters are independent of Quest room geometry. An HA room name is not automatically equated with a scanned Quest room ID. Saved mappings can still be removed through management. Editor previews without room data and demo mode are exempt.

## Clicks and green readiness

Fist rotation and sliders produce a short mechanical adjustment tick when the preview value changes sufficiently. Small fluctuations, holding, and end stops remain silent. Rate limiting prevents multiple overlapping clicks per tracking sample. Sounds are synthesized in the app.

A subtle green glow follows the real tracked interaction hand when the gesture is ready. It becomes slightly stronger with a closed fist. No custom hand model is shown. Readiness disappears on tracking loss, target change, menu opening, or room change.

Under Settings → Sound & gesture hints, sounds, gesture subtitles, and the green hand effect can be enabled independently. Choices persist across restarts. Left-handed mode is respected.

## Update and evidence

Version 0.6.2 / Android code 11. Package, signature, and token/anchor storage formats remain unchanged. Install over the existing app; do not uninstall or clear app data. Exclusion of local SDK developer data from 0.6.1 remains in place.

112 core checks and 119 Unity integration checks passed. New checks: filter access with type/room/search, shared menu hierarchy, filters outside the scroll mask, room changes and command eligibility, floor boundaries, invalid/missing current room, readiness effect without an interaction surface, hiding without tracking, independent hint switches, and adjustment ticks, jitter, end stops, and tracking gaps.

Real Quest acceptance remains outstanding, particularly room boundaries at doorways, relocalization, FOV/stereo presentation, and subjective glow strength. GitHub APK upload remains manual as agreed.
