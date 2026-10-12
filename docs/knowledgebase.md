# Knowledge base

Help for the current version, **0.6.3**. Return to the [documentation index](README.md).

## A token is requested again after an update

**Symptom:** The connection menu does not offer saved credentials.

**Meaning:** App data is missing, the Keystore key is unavailable, or the server address changed. A revoked HA token cannot be reused either.

**Check:** Install the APK over the existing installation without uninstalling or clearing data. Use the same base URL, including scheme and port. The token field may remain empty for the same confirmed address. If installation fails because of a signature mismatch, use the matching APK; uninstalling would remove the saved data.

## Home Assistant is unreachable or returns 401/403

**Symptom:** The connection check fails and states appear stale.

**Meaning:** 401/403 indicates credentials or permissions. Timeouts, DNS errors, and certificate errors concern connectivity from the Quest network.

**Check:** Use a base URL without `/api`, a path, embedded credentials, or a query. Check the token in the HA user profile. Check Wi-Fi, DNS, and the HTTPS certificate. Local HTTP requires explicit opt-in in the app. If a switching command is unconfirmed, check the current device state first: the app does not retry commands automatically.

## Rooms or areas are missing from the filter

**Symptom:** Only All rooms and Unassigned are visible, or an assignment is missing.

**Meaning:** The HA area catalog has not loaded, the template request failed, or the entity has no HA area assignment.

**Check:** Reload area assignments under **Search & filter** and read the status message. Check the entity's or device's area assignment in HA. Version 0.6.3 also shows empty areas and refreshes an open selector after loading. Quest room capture does not create HA areas.

## An icon is missing or appears in the wrong room

**Symptom:** A saved entity is displayed incorrectly after startup or a room change.

**Meaning:** The app is waiting for a stable anchor, or the Quest room was not localized correctly. Only entities in the current physical room are displayed.

**Check:** Look around in the corresponding room and wait briefly. Use saved-position recovery if needed. Check room permission and saved system room data. If the position remains incorrect, reproduce the flow in [QUEST-TEST.md](QUEST-TEST.md); desktop tests cannot rule out stable but incorrect operating-system localization. Remove the mapping through management and place it again if needed.

## The next page initially ignores input after Back

**Symptom:** A held pinch or touching finger is ignored immediately after navigation.

**Meaning:** The release barrier prevents an accidental click on the new page.

**Solution:** Release the pinch or withdraw your finger from the menu surface, wait briefly, then select again. If an entity underneath is still opened, check the version and record whether ray/pinch or direct touch was used in the bug report.

## The other hand does not take over immediately

**Symptom:** The second hand has no effect during an action.

**Meaning:** Only one hand can interact at a time. An already-held gesture on the second hand cannot take over.

**Solution:** Finish the current action and briefly release both hands, then start again with the desired hand. The configured hand preference decides simultaneous starts. After a Quest system dialog, bring both hands back into tracking and start a fresh gesture. See [tracking recovery](TRACKING-RECOVERY.md).

## Dimming or volume adjustment cancels when looking away

**Symptom:** The preview value disappears without being committed.

**Meaning:** From 0.6.3, the target remains captured once the fist is recognized. A target change before that point, hand tracking loss, room change, or menu opening still cancels the action.

**Solution:** Look at the target, hold an open hand until ready, make a fist, then rotate. Look away only after fist recognition and keep the hand within tracking range. Opening commits the value. The entity must report brightness or volume support in HA.

## Magic does not trigger or adjusts a value instead

**Symptom:** No fireball appears when opening the hand, or a brightness/volume slider appears instead.

**Meaning:** The current gesture is open hand → fist → open. The earlier thrust gesture belongs to the version history. Fist rotation takes precedence over throwing.

**Solution:** Enable magic and use the practice target first. Wait until ready, make a fist, and open it without rotating. Start a fresh gesture after each throw. Gesture subtitles can be enabled for diagnosis.

## A camera shows snapshots or stops updating

**Symptom:** There is no smooth video/audio stream.

**Meaning:** Media frames use HA proxy snapshots. Cameras wait at least 0.5 s between completed requests; hidden or distant frames are paused.

**Check:** Open the camera or image in HA, check permissions and connectivity, and look at the frame. Large or undecodable images are rejected. External image URLs are not requested with the HA token.

## What helps in a bug report?

Include app version, Horizon OS version, exact steps, hand used, input method, timing of room/system dialog changes, and the visible status message. Add a sanitized screenshot if useful. Do not publish tokens, credential QR codes, private configuration files, or unsanitized SDK credentials.
