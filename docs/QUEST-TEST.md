# Quest 3 acceptance testing

Desktop checks are marked below. **All device tests remain outstanding.** Automated tests do not replace headset acceptance.

The version-specific sections preserve historical test procedures. Later sections supersede earlier gesture behavior; use the 0.6.3 rules for current hand ownership and looking away during adjustment.

## Build and first launch

- [x] Unity imports all pinned packages without compilation errors.
- [x] OpenXR build validation and Android ARM64/IL2CPP build succeed.
- [x] Signature and required entries in the finished APK manifest checked; see BUILD-STATUS.md.
- [ ] APK installs on Quest and launches as an immersive app.
- [ ] Real surroundings are visible in passthrough; UI is readable with both eyes.
- [ ] Without a saved connection, clearly marked demo data appears; real devices are not contacted.
- [ ] Left pinch/X opens a stationary menu; right pinch/trigger selects its buttons.

## Rooms and persistence

- [ ] Denying room permission produces an understandable message, not a crash.
- [ ] Load/first scan loads an existing room or starts capture.
- [ ] Rescan opens capture even when a room exists; cancellation works.
- [ ] Scan-surface placement offers no savable position without a hit.
- [ ] Free placement works one meter in front of the headset.
- [ ] Hand placement: grab, move, release, and confirm work. Tracking loss does not save a position.
- [ ] Apartment map shows room boundaries, current position, walking path, and localized devices.
- [ ] Generated symbols, focus animation, and state feedback are readable and smooth.
- [ ] Light and sensor mappings remain at their physical locations after app and headset restarts.
- [ ] A missing anchor produces no guessed replacement object; reloading in the correct room works.
- [ ] Deleting a mapping removes its display and saved anchor; it stays deleted after restart.
- [ ] Rescanning, recentering, and moving to another room do not cause incorrect device mappings.

## Home Assistant

- [ ] HTTPS connection with a valid certificate works.
- [ ] URL and token can be entered/pasted in the headset; the token stays hidden.
- [ ] Connection survives restart; only encrypted credentials reside in the app directory.
- [ ] Forgetting the connection deletes credentials and returns to demo mode.
- [ ] Changing servers shows only spatial mappings for that server.
- [ ] A light placed as display-only does not switch on a gesture.
- [ ] Local HTTP works only with explicit opt-in in configuration.
- [ ] Invalid tokens show an access error. No secrets appear in Unity/device logs.
- [ ] The actual device list appears; long names and large lists remain usable.
- [ ] Targeting a light/switch and then pinching causes exactly one correct service call.
- [ ] An early/held pinch, gaze changes while pinching, and recovery after tracking loss do not switch accidentally.
- [ ] Sensors show units and refreshed values. Sensors and unavailable devices cannot be switched.
- [ ] A light switched externally in HA updates after the next poll.
- [ ] Wi-Fi/HA outage: values are marked stale and commands blocked. Connection recovers without delayed switching commands.
- [ ] Lost command response: the command is not retried automatically; state is read again.
- [ ] Pausing/resuming the app and opening the system menu produce no unintended action.

Record results, Unity version, Quest OS version, date, and any errors during testing. Complete this checklist before using the app for daily home control.

## Additions for 0.3.0

- [ ] Real hands are visible in passthrough, with no virtual hand models over them.
- [ ] Walk freely through multiple captured rooms; check Boundaryless behavior on the installed Horizon OS. Anchors remain at physical locations.
- [ ] Combine type, HA room, and search text; check empty results and Unassigned. Then reset filters.
- [ ] Place a light, switch, and display-only entity. The switch has its own icon. Other domains, such as climate/lock, remain display-only.
- [ ] Beyond 30 cm, only the icon appears; within 30 cm, name/value appear; moving away hides text.
- [ ] Finger ray ends at the actual target icon, including sensors. Pinch switches only lights/switches without target jumps or repeated activation.
- [ ] QR scan: allow/deny permission, cancel scan, pause app, restart scan. Camera active only during scanning.
- [ ] Read token and JSON QR codes from a phone; check address before connecting. QR must not automatically connect or allow HTTP.
- [ ] Test damaged QR codes, ordinary web links, and invalid tokens; no credentials in errors.

## Regressions from 0.3.2

- Open the Quest room-editing system dialog and complete/cancel it. Then check the right hand and left menu pinch; the menu must return in front of the head.
- Approach an icon: circle/border visible, size increases between 1.2 m and 30 cm; text only within 30 cm.
- Freeze pointer and direct-hand placement with a single pinch, move again, and confirm.
- Delete a placement from the main menu even when its room anchor is missing or HA entity unavailable; it must not return after restart.

## Magic from 0.4.0

- Enable the mode, restart the app, and check the saved setting.
- Aim an open right hand at the icon, briefly hold, then thrust forward. Check target border, single switching command, fireball from the palm, trail, and impact.
- No activation from walking, moving the head backward, sideways waving, slow reaching, closed hand, tracking loss, or holding after a thrust.
- Sensors, display-only entities, stale states, menus, placement, and room capture must not trigger magic switching commands.
- After retracting/closing the hand and waiting for cooldown, another thrust must be possible.

## Regressions from 0.4.1

- Recreate rooms in the Quest system dialog; afterward select several menu entries with the right hand.
- Perform a long Quest recalibration hold with the right thumb/index finger. Release; the right pointer must still target icons and menus and recognize new pinches.
- Left pinch opens; after release, another pinch closes submenus too. Holding must not repeatedly toggle the menu.
- Perform three separate forward thrusts, retracting each time, without waiting for the flight animation to end. At most one command per gesture, no automatic repeated switching.

## Tracking and multiple rooms from 0.5.0

- Place two entities in different rooms. Fully close the app, then restart in another room facing a different direction. Icons must appear at their original physical locations only after reliable tracking.
- Briefly cover cameras during startup; no entity may be visible or switchable at the origin or a stale position. Uncover and look around; missing anchors are retried automatically.
- After tracking loss, a long right calibration pinch, or completing/canceling room capture, icons must briefly stay hidden, then reappear at their real locations.
- Freeze a placement, then recalibrate or open the system dialog. The old preview must no longer be savable; place it again.
- Walk through the apartment: spatially localized icons should stay fixed. Head movement must not move entities into another room.
- Install the APK as an update: existing token and mappings remain. Missing anchors must not create new mappings or overwrite existing UUIDs.

## Interaction and interface from 0.5.0

- Extend the right index finger: ray visible without a hit, stronger on entity hit. Test pinching after rapid targeting, holding, target changes, and tracking loss; no duplicate commands.
- ISDK touch from the front and at an angle with the right fingertip; other fingers and rear approaches must not trigger unintended actions.
- At least 30 entities across four HA rooms: eight complete rows, final row reachable, live values current. Pinch-and-drag and direct swiping must not accidentally open a row. Check combined filters.
- Save watch mode, restart, and tap the watch. Panel must remain steady in front of the head and not follow the moving arm. Left pinch remains usable.
- Switch DE/EN and restart; check menus, status messages, search, magic hints, and sensor states. Preserve custom HA names.
- Edit, confirm, and cancel address, token, and search in the Meta system keyboard. Right hand remains usable after every focus change; token stays hidden.
- Look directly at a sensor from several meters: value visible above icon; looking away hides it. Gaze alone must not control devices.
- Test a dimmable light and speaker separately: grab the correct slider, drag sideways, release. HA value must follow. An on/off light without dimming must not receive a brightness slider.
- Use the magic practice target: no HA commands, visible green readiness, repeated thrusts. Then test a real light and switch.
- Place camera/image frames, then restart from another room: position and wall orientation retained. Take HA offline, revoke permission, and return: stale images must not silently appear current.
- Sounds distinguish lights/switches, can be disabled, and must not imply a confirmed action after a failed HA command.
- Measure frame rate and memory use on a real Quest for at least 30 minutes with many icons and two visible camera frames.

## Interaction from 0.6.0 (supersedes older gesture flows)

- [ ] Select an entity with the head-center dot. Changing hand direction must not change the target. Looking away removes the highlight.
- [ ] Intentional pinch switches once. Close a fist slowly and quickly: no accidental pinch command. Held fingers and focus loss do not repeat.
- [ ] Look at a light and speaker, briefly open the hand, make a fist, and rotate both ways. Check percentage preview. Only opening commits.
- [ ] Look away, open the system menu, or cover the hand during rotation: no value may be confirmed through tracking loss.
- [ ] Magic: observe readiness ring, close and open fist. Three consecutive throws; one command per gesture. After rotation, only commit the value, with no fireball.
- [ ] View sensors from 0.5 / 1 / 3 / 6 meters. Text should retain approximately the same angular size, remain readable, and not permanently obscure neighboring icons.
- [ ] Open/close the left watch with the right fingertip, then select left-handed mode: watch right, touch/pinch/fist interaction left. Preference survives restart.
- [ ] Time, date, and connection status do not overlap; moving the arm leaves the opened menu stationary.
- [ ] Check main tiles, 3D symbols, and labels with both eyes: no double images, missing objects, or depth conflicts. Check hover and click feedback.
- [ ] Repeat two-room, cold-start-in-another-room, and system calibration tests above. New gestures must not change spatial mappings.

## Version 0.6.1

- Check the menu under bright and dark passthrough: all submenus, focus/pressed states, and text readable in both eyes; no missing materials on 3D symbols.
- Adjust the watch on both arms; test every movement direction and rotation axis, minimum and maximum size. Rotating the arm moves the saved watch accordingly.
- Save, restart, and switch handedness: correct position retained. Canceling and closing the menu do not change saved values. Reset affects only the selected wrist.
- Lose and restore watch-hand tracking during adjustment; preview must not appear in the wrong location or save unintentionally.
- Update 0.6.0 → 0.6.1 without uninstalling: token and saved entities retained.

## Version 0.6.2

- Check entity list and filters head-on and from strong side angles with both eyes. Background must not cover buttons; Search & filter and Back stay visible and usable while only the list scrolls.
- Combine search, type, and HA room filters, reset them, and reopen.
- Walk from room A to B: A markers, including camera frames, disappear; B markers appear after stable localization. Return; mappings remain. Check rooms on different floors.
- Walk through a doorway during fist adjustment and with an open slider: no commit to the old target. Outside the scanned home, no placed entities may be visible or interactive.
- Fist rotation for brightness/volume: adjustment ticks on value changes, no repeats while holding or at 0/100%. With sound off, all clicks are silent.
- Check readiness glow for both interaction hands; tracking loss or target change removes it. No artificial hands.
- Toggle subtitles and hand effect independently; settings survive restart.

## Version 0.6.3

- Open rooms while HA areas are still loading; afterward check selectable areas, including empty ones. Change area, combine filters, and reset.
- Back by pinch and touch: underlying entities must not open details. Withdraw the finger after navigation and tap again.
- Switch, place, scroll, and adjust sequentially with left and right hands. Simultaneous gestures send at most one command. An already-held second hand cannot take over the action.
- Close a fist on a dimmable light, turn the head sideways, and continue adjusting. Opening commits to exactly the original target. Repeat for volume; head movement alone must not change the value.
- Tracking loss and room change during fist adjustment: no confirmation; start again.
- View both palms and fists from different angles: open light arcs follow the bones, leave the real hand visible, and do not flicker.
- Test menu pinch away from entities and watch tapping with the watch on either side.
- Update 0.6.2 → 0.6.3 without uninstalling: token, anchors, and settings retained.
