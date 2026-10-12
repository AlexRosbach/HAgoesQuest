# Interaction and design: researched decisions

Version covered by the original decisions: 0.6.0. The “Changes in 0.6.0” section supersedes earlier entity selection using a hand ray and the thrust gesture. The original decisions below record the 0.5.0 baseline; the corresponding instructions are in [VERSION-0.6.0.md](VERSION-0.6.0.md). Later sections record subsequent revisions; use [INSTALLATION.md](INSTALLATION.md) for current operation.

Research: September 29, 2026; primary sources from Meta and Home Assistant.

## Meta recommendations and implementation

The app uses a dominant right hand for pointing and pinching; near interaction supports index-finger touch. Selection uses the hand aim pose supplied by the Meta runtime, supplemented by smoothing and brief stabilization at pinch onset. The earlier additional focus-time filter before a right pinch is removed. This aims to reduce missed selections; actual reliability requires Quest testing.

The visible ray remains present while pointing, even without a hit. Hitting an entity icon increases brightness and the glowing border. Switching sounds follow a server-accepted action, not mere gaze contact. The resulting device state is still read separately from HA.

Lists show eight fully interactive entries and partially visible following entries. Pinch-and-drag and direct swiping scroll the list; a small scrollbar is not a required input target. Pinch-and-drag on a labeled slider changes brightness or volume. Release requests exactly the chosen absolute value. Normal interaction does not require invented directional signals or unconfirmed automatic gaze commands.

Large menus remain stationary at approximately 1.4 m. The optional left-wrist watch only opens the panel; the panel does not follow the arm. This follows the user's later decision to adhere to Meta recommendations. No generated hands obscure real passthrough hands.

Sources: [Hands UI best practices](https://developers.meta.com/horizon/design/hands-ui-best-practices/), [Input mappings](https://developers.meta.com/horizon/design/interactions-input-mappings/), [Ray casting](https://developers.meta.com/horizon/design/raycasting_usage/), [Input modalities](https://developers.meta.com/horizon/design/interactions-input-modalities/).

## Keyboard, gaze direction, and magic

Text input on Quest uses `TouchScreenKeyboard` with Meta's system overlay; the custom keyboard renderer remains only for the editor. Focus loss when opening the overlay is expected and must not leave hand controls blocked afterward. [Meta: Enable Keyboard Overlay](https://developers.meta.com/horizon/documentation/unity/unity-keyboard-overlay/).

Quest 3 has no eye tracking. “Looking” here means central head direction. This allows sensor values to appear from a distance. Looking alone sends no HA command.

Magic remains an explicitly optional, experimental gesture, not a Meta-standardized interaction pattern. Instructions, live hints, and a practice target explain opening the hand, finding the target, holding still, thrusting forward, and preparing again. Standard controls remain available. The flight animation alone does not confirm a physical device state.

## Limitations

The custom world UI connects to Meta Interaction SDK 207 through `RayInteractable` and `PokeInteractable`. A root driver supplies `RayInteractor` and `PokeInteractor` with the runtime aim pose, pinch state, and fingertip. Selection and touch use SDK state transitions; scrolling and sliders interpret subsequent movement. This is not an unmodified Meta UI kit. Documented design choices and desktop checks do not constitute Meta approval.

## Changes in 0.6.0

The newly requested entity controls use a visible head-direction reticle, confirmed by a pinch. Finger pointers remain exclusive to menus and placement so two distant-targeting modes do not compete for the same entity. A brief classification interval and finger posture distinguish pinch from fist. Fist rotation and opening are custom app gestures, not Meta-guaranteed standard gestures.

The main view arranges actions in large tiles with one primary action. Original spatial symbols provide additional visual cues; the labeled tile remains the same hit target. The watch is mirrored for left-handed users and still opens a stationary panel. Sensor labels scale proportionally with distance within lower and upper limits.

Additional sources: [Button hierarchy and hit targets](https://developers.meta.com/horizon/design/buttons_bp/), [Button states](https://developers.meta.com/horizon/design/buttons_specs/), [Angular scaling](https://developers.meta.com/horizon/design/mr-design-guideline/), [Wrist button](https://developers.meta.com/horizon/design/wrist-buttons/). Platform values in dp cannot be universally equated with world meters in the custom Unity interface. Readability and hit targets therefore require additional headset testing.

## 0.6.1: Readability and materials

[Meta Accessibility](https://developers.meta.com/horizon/design/accessibility/), accessed September 29, 2026: at least 4.5:1 for normal text, sufficiently large targets, and visible focus feedback. The three menu text palettes are automatically tested against 4.5:1. Nearly black surfaces were replaced with opaque slate blue. Unity previews show the startup menu and watch editor; this does not replace real Quest acceptance.

The 3D models and 128×128 material textures are generated entirely by original code in ButtonSculpture.cs. Camera-relative shading retains bright ambient lighting so models remain readable independently of real or virtual room lighting. Parts are combined by material per object. The watch editor uses large individual buttons instead of a precise dragging gesture; adjustments are committed only on Save.

## 0.6.2: Room boundaries and UI ordering

Meta MRUK 207 provides GetCurrentRoom and IsPositionInRoom with vertical-bounds checking: [MRUKRoom reference](https://developers.meta.com/horizon/reference/mruk/v207/class_meta_x_r_m_r_utility_kit_m_r_u_k_room/) and [scene data queries](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-manage-scene-data/). The app validates headset containment instead of trusting a stale current-room result, then checks entity positions against the same room. Missing rooms fail closed outside editor/demo mode.

The installed Unity uGUI source confirms RectMask2D clips descendants in their root canvas space. The fixed filter toolbar is outside that subtree. Menu surfaces now share a root canvas and the panel is the first sibling, removing independent per-card transparent distance sorting. Desktop renders at frontal and ±35° views supplement structural tests; Quest stereo testing remains necessary.
