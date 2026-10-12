# Tracking at startup and across rooms (from 0.5.0)

Saved icons previously appeared as soon as `OVRSpatialAnchor.Localized` was true. In the SDK used here, that means the Locatable component is active. It does not confirm that position and orientation are currently tracked. Anchors were also requested immediately after loading HA credentials, even before valid head tracking.

- Loading waits for active head tracking, focus, and a camera. The attempt releases its hold after at most 20 seconds; missing anchors are retried every 15 seconds when tracking is available.
- Visibility additionally requires `IsTracked` and a stable position for 0.75 seconds. A position jump over 3 cm or rotation over 3 degrees restarts this wait. Checks run after SDK updates; visibility updates before rendering.
- Missing anchors do not produce a visible fallback position. New markers start hidden. Invisible markers cannot be targeted by pointer or magic.
- Room capture, focus changes, and recalibration invalidate tracking readiness and pending placement previews. New positions are saved only with stable tracking; if tracking changes while saving, the mapping is rolled back.
- Existing UUID mappings, package name, signing key, and encrypted token are retained. This APK is version 0.5.0, Android version code 8.
- Multiple anchors localize in parallel. An unseen room must not delay recovery of other rooms sequentially by ten seconds each. Manual recovery no longer blocks all menu interaction.

The SDK method `UnboundAnchor.TryGetPose` already returns world coordinates; these are deliberately not converted a second time. MRUK and Spatial Anchors remain responsible for localization. An anchor that the Quest localizes stably but incorrectly cannot be detected or automatically corrected through these tracking flags alone. Startup in different real rooms therefore remains part of device acceptance.

Reference: [Meta Spatial Anchors](https://developers.meta.com/horizon/documentation/unity/unity-spatial-anchors-persist-content/); the behavior of `Localized` and `IsTracked` was also checked in the installed Meta SDK source.
