<div align="center">

# HA goes Quest

**Home Assistant throughout your home – mixed reality for Meta Quest 3**

[![Version](https://img.shields.io/badge/version-0.6.3-6366f1)](CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-6000.0.66f2-334155)](docs/documentation.md#development-and-build)
[![Status](https://img.shields.io/badge/status-development_build-f59e0b)](docs/BUILD-STATUS.md)

Place lights, switches, sensor readings, and picture frames in your scanned room. Select an entity with the gaze reticle and control it with a hand gesture.

[Installation](docs/INSTALLATION.md) · [Documentation](docs/README.md) · [Troubleshooting](docs/knowledgebase.md) · [Changelog](CHANGELOG.md) · [User guide](docs/USER-GUIDE-EN.md)

</div>

## Your home at a glance

- **Spatial entities:** Icons and sensor readings at positions you choose; camera and image entities as frames.
- **Gestures:** Gaze reticle + pinch to toggle; rotate a fist for brightness or volume. Either hand can be used, with one owning the action at a time.
- **Room visibility:** Placed entities appear only in the current scanned Quest room.
- **Find entities:** Scrollable list with current values, search, and combinable type and HA area filters.
- **Personal controls:** German/English, adjustable HA watch, sounds, and gesture hints. Optional magic mode with a fireball and practice target.
- **Retain your connection:** After successful setup, the token is stored encrypted on the Quest; compatible updates preserve credentials and placements.

**Development status:** APK 0.6.3 was built and its signature and manifest checked. The corresponding build passed 121 core checks and 130 Unity integration checks. Documented Quest hardware acceptance testing remains outstanding. See the [build report](docs/BUILD-STATUS.md).

## Quick start

### Prerequisites

- Meta Quest 3 with hand tracking enabled; developer mode and USB debugging for APK installation.
- A Home Assistant instance reachable from the Quest network and a Long-Lived Access Token.
- The supplied development APK; building it yourself also requires Unity with Android Build Support.

### 1. Install or update

Install using Meta Quest Developer Hub or:

```text
adb install -r HAgoesQuest-0.6.3.apk
```

Install over the existing app. **Do not uninstall or clear app data** if you want to retain the token and placements. APK uploads are currently manual; GitHub checks do not build an APK.

### 2. Connect and capture rooms

Start the app, enter the Home Assistant base URL and token, or scan the token as a QR code. Test and save the connection. Then load existing Quest rooms or open the system room-capture dialog.

### 3. Place and control an entity

Search for an entity, filter by type and HA area if needed, choose a placement method, and confirm its position. Close the menu, aim the gaze reticle at the icon, and pinch. For brightness/volume, briefly open your hand, make a fist, and rotate it; opening the hand commits the value.

Once the fist has captured the target, you can look away while the hand remains tracked. Briefly release both hands before switching hands. [Full instructions](docs/INSTALLATION.md).

## Currently supported features

| Feature | Behavior in 0.6.3 |
| --- | --- |
| Selection | Head direction; Quest 3 has no eye tracking |
| Lights and switches | Turn `light` / `switch` on and off; dim supported lights |
| Volume | `media_player` entities that report volume support |
| Other entities | Display states; no general commands for every HA domain |
| Camera / image | Refreshed snapshots, no video/audio stream |
| Room scan | Meta room data; physical lights are mapped to HA entities manually |
| Positions | Up to 50 mappings per HA server; local Spatial Anchors |
| States | REST polling; no WebSocket live stream |
| HA watch | Adjustable wrist position; opens a stationary menu |

## Development

Project baseline: **Unity 6000.0.66f2**, **Meta XR Core / Interaction SDK / MRUK 207.0.0**, Android ARM64 / IL2CPP. Unity loads SDK packages from the configured registries. Build instructions, data formats, and input flows are in the [technical reference](docs/documentation.md).

Core checks on Windows:

```powershell
.\tests\Run-Tests.ps1
```

Alternatively, with .NET 8:

```text
dotnet run --project tests/CoreTests.csproj
```

The [contribution guidelines](docs/CONTRIBUTING.md) describe commenting style, appropriate checks, and the distinction between editor checks and hardware acceptance.

## Documentation and licensing

- [Documentation index](docs/README.md): Find instructions by task.
- [Technical reference](docs/documentation.md): Architecture, HA requests, persistence, builds, and tests.
- [Knowledge base](docs/knowledgebase.md): Symptoms, causes, and practical troubleshooting.
- [Changelog](CHANGELOG.md): Current and historical release notes.
- [Commercial readiness and licensing](docs/COMMERCIAL-READINESS.md): Existing assessment and outstanding publication requirements.
- [Artwork provenance](docs/ART-PROMPTS.md) and [magic artwork](docs/ART-MAGIC.md): Assets and prompts.

Meta, Unity, and other dependencies retain their respective license terms. The documentation structure follows [LanLens](https://github.com/AlexRosbach/LanLens); this does not transfer its license or product commitments to HAgoesQuest.
