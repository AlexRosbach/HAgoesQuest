# Version 0.3.0

- Pure passthrough AR: no rendered hand models. Hand tracking remains active for pinching and pointing.
- Official Meta Boundaryless mode in the Android manifest. The app imposes no movement boundary and has no VR mode. Requires a supported Quest system version; this manifest entry does not apply over Link.
- Entities: search by display name or ID, combinable with type and Home Assistant room filters. All valid HA entity types are available as displays. Only `light` and `switch` send switching commands; other types remain display-only.
- Area assignments load through Home Assistant's Template API, including assignments inherited from a device. Unassigned entities appear under Unassigned. If permission is missing, type filters and search remain available; the menu shows the loading error.
- Placed entities show an approximately 9 cm icon without a permanent card. Name and value fade in within 30 cm between headset and icon, and fade out when moving away.
- Point the right hand at an entity: a thin ray reaches from the index finger to the hit icon. Pinching toggles after a short target hold. Displays can also be targeted. Controllers remain an alternative.
- Dedicated generated switch icon.
- QR scanning directly through the Quest passthrough camera, only after starting a scan and granting camera access. The camera stops when leaving the scan menu. Images are neither stored nor uploaded.

## Transfer a token from your phone

Quest 3/3S requires Horizon OS 74 or later for camera access. Display a QR code containing the token as plain text or this JSON:

```json
{"baseUrl":"https://ha.example.com","accessToken":"YOUR_LONG_LIVED_ACCESS_TOKEN"}
```

The field names `url`/`token` are also accepted. Scanning does not open links or establish a connection. Check the address after scanning and select the test-and-save action. Unencrypted HTTP must still be allowed separately.

Use `tools/qr/index.html` with the included `qrcode.min.js` to generate a QR code locally without an online service. Transfer both files together to the phone or open them on a computer. The code contains the token; finish by clearing the code and inputs.

## Device testing

Still to verify on a real Quest: Boundaryless behavior, visibility of real hands, finger ray during pinch, readability of the 30 cm display, QR recognition from a phone screen, permission dialogs, spatial recognition, and Android Keystore. Editor and build checks do not replace this testing.

## Sources

- [Meta Boundaryless](https://developers.meta.com/horizon/documentation/unity/unity-boundaryless/)
- [Meta Passthrough Camera API](https://developers.meta.com/horizon/documentation/unity/unity-pca-documentation/)
- [Home Assistant area_id](https://www.home-assistant.io/template-functions/area_id/)
- [Home Assistant REST API](https://developers.home-assistant.io/docs/api/rest/)
- ZXing.Net 0.16.11 (Apache-2.0), QRCode.js (MIT); licenses are located beside their respective files.
