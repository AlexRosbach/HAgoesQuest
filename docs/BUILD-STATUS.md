# Build and verification

Date: 2026-09-29. Target: Meta Quest 3, version 0.6.3.

| Check | Result |
| --- | --- |
| Unity 6000.0.66f2, Meta XR Core / MRUK / Interaction SDK 207 | Package import and C# compilation succeeded |
| Independent app core | 121 assertions passed |
| Unity Play Mode with local HA test server | 130 assertions passed |
| Menu preview | Rendered with Unity and visually checked |
| Android ARM64 / IL2CPP | APK built successfully |
| Local SDK developer data | Excluded from APK; packed asset report checked |
| Signature | Valid, identical to previous test versions |
| APK manifest | GameActivity, VR, hand tracking, passthrough, room, anchor, camera, Boundaryless, and system keyboard checked |
| HTTP opt-in | Network policy present; explicit permission in the app still required |
| Installation, gestures, and system keyboard on Quest | Device test outstanding |
| Multi-room localization / Android Keystore during updates | Device test outstanding |
| Performance and extended sessions | Device test outstanding |

New checks in 0.6.3: independent HA area catalog with empty rooms, area selection, Back without clicking through to the list, exclusive hand ownership and release barrier, captured target while looking away, rotation reference, and confirmation after head movement. Also checked: menu hierarchy and filter access, current room boundaries and commands blocked for other rooms, hand readiness and independently switchable subtitles, and mechanical adjustment ticks without jitter or repetition while holding. Checks retained from 0.6.1: watch position relative to a moving/rotating wrist, separate storage, preview without overwriting, limits, reset, invalid data, cancellation, and text contrast. Checks retained from 0.6.0: gaze selection independent of hand direction, gaze selection before a gesture begins, fist/pinch distinction, rotation and angle wraparound, repeated fist-open throws, no action on tracking loss, mirrored interaction hands, and proportional sensor labels. Other tests cover Meta SDK ray/poke selection, no repeated action while holding, recovery after tracking loss, two independently saved anchor UUIDs, rejection of old placement after recalibration, eight visible list entries and scrolling, slider gestures and HA brightness/volume calls, four HA area assignments, language switching, authenticated camera/image requests, and image decoding. Earlier checks for connection, QR, update credentials, filtering, deletion, markers, and fireballs remain included. No real Home Assistant instance was contacted.

This evidence does not establish correct localization in a real building. An anchor localized stably but incorrectly by the operating system cannot be reliably detected through tracking flags alone. The [Quest acceptance checklist](QUEST-TEST.md) is the required basis for that work.

## APK

- File: HAgoesQuest-0.6.3.apk
- Package: com.alexrosbach.hagoesquest
- Version: 0.6.3, Android code 12
- Size: 68173682 bytes
- SHA-256: ef95a28c3247c9a2aa51ac80d612fb3927ffd1f03e865d1e6499679ebd5732aa
- Minimum Android API: 32; ARM64; Unity IL2CPP; development build.
- Development signature SHA-256: 345caca35c4a1ad2eb8f2243a0ccda712945c87b9b089714f2778c007c76e5b2

The finished file was checked with apksigner and aapt. Package name, signature, Android Keystore alias, and stored data formats are retained for updates. Do not uninstall or clear app data. An actual update on Quest remains part of device acceptance.

Meta's preflight check may issue a GameActivity warning without a custom source manifest; the verified final APK manifest is authoritative. SDK warnings do not constitute Meta certification. GitHub Actions checks the .NET app core and does not build an APK.
