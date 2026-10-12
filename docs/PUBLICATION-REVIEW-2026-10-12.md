# Publication readiness review — October 12, 2026

## Decision

**Do not treat the existing 0.6.3 APK as ready for a paid Meta Horizon Store release. A public GitHub test APK also needs the licensing and distribution issues below resolved; “free” does not remove them.**

This is a technical and preliminary licensing assessment, not legal clearance or Meta approval. It covers both requested routes: a free public GitHub beta and a paid Meta Horizon Store app. No store submission, license acceptance, release removal, signing-key change, or runtime change was performed.

| Route | Decision for the inspected artifact |
| --- | --- |
| Existing personal testing | Continue controlled testing; this does not establish distribution rights |
| Public GitHub beta | Hold broader distribution until Unity rights, developer tooling, privacy/notices, and a documented device smoke test are addressed; clearly label experimental builds |
| Paid Meta Horizon Store | No-go: confirmed packaging failures plus licensing, privacy, and hardware acceptance gaps |
| Public source repository | Separate from binary distribution. Decide rights for original code; retain third-party terms and exclude secrets. Publishing source is not equivalent to an open-source license |

The [0.6.3 GitHub release](https://github.com/AlexRosbach/HAgoesQuest/releases/tag/0.6.3) already contains the APK and is **not** marked as a prerelease. Its GitHub asset digest matches the locally inspected file. Recommendations here do not change that existing release.

## Scope and evidence

- Source baseline: `aa660a7dabbc9d29092223df72ac04f6d82bf2a2`.
- Compared 55 tracked source, tool, test, package, and project-setting files against GitHub blob hashes: no mismatches.
- APK: `HAgoesQuest-0.6.3.apk`, 68,173,682 bytes, package `com.alexrosbach.hagoesquest`, version code 12.
- SHA-256: `ef95a28c3247c9a2aa51ac80d612fb3927ffd1f03e865d1e6499679ebd5732aa`.
- Fresh checks: APK signature, final compiled manifest, native library inventory, selected security/privacy code paths, license notices, release metadata, and 121 passing core assertions.
- Existing 130 Unity integration assertions are historical evidence, not a newly executed integration run.
- No attached-headset test, live HA test, network capture, store-dashboard inspection, full Git-history secret audit, or complete native-dependency audit was performed. Local SDK credential asset contents were not opened.

The machine-readable [artifact findings](PUBLICATION-EVIDENCE-2026-10-12.json) record exact values. The SHA-256 in that file and the GitHub asset digest identify the artifact.

## Confirmed packaging failures and production risks

### P1 — Debug build and development signing

The final APK has `android:debuggable=true`. Its certificate subject is `C=US, O=Android, CN=Android Debug`. APK Signature Scheme v2 validation passes, so the problem is not an unsigned or corrupt file.

`ProjectSetup.cs` builds with `BuildOptions.Development`; `tools/Build-Quest.ps1` pins the existing debug-keystore certificate. This is deliberate test-update compatibility, not a production signing process.

Meta requires a non-debug release manifest. Create a separate release configuration and privately managed production key. Do not silently replace the existing key: ordinary updates require a compatible signing identity. Plan and test migration before promising that existing testers retain tokens and anchors. A different package installs separately; it does not inherit Android Keystore data. [Meta manifest rules](https://developers.meta.com/vr/resources/publish-mobile-manifest/), [Meta signing](https://developers.meta.com/vr/documentation/native/android/mobile-application-signing-quest/), [Android signing](https://developer.android.com/studio/publish/app-signing).

### P2 — Manifest differs from current immersive-app requirements

| Field | Inspected APK | Required action |
| --- | --- | --- |
| Target SDK | 36 | Pin 34 for the current immersive-app submission rules |
| Minimum / compile SDK | 32 / 36 | Minimum is within the documented range; compile may exceed target |
| Install location | 2, preferExternal | Use auto, value 0 |
| Launch activity excludeFromRecents | Absent | Set true |
| Supported devices | quest2, questpro, quest3, quest3s, stanley | Restrict to actually supported/tested devices and current canonical identifiers |
| Architecture / entry | ARM64, GameActivity, VR category | Present; retain and verify on the release build |

The app is immersive MR, not a 2D panel app. The Meta manifest page updated September 30, 2026 lists target 32–34 for immersive apps and target 34 for newly created apps. Merely compiling with a newer SDK does not establish store compatibility. `stanley` is not in that page's canonical list. Quest 2/Pro must not be advertised solely because SDK defaults include them. [Current manifest specification](https://developers.meta.com/vr/resources/publish-mobile-manifest/).

### P3 — Developer operator components remain in the APK

The APK contains `libXrApiLayer_METAX_operator.so` and its implicit OpenXR layer JSON. Its manifest includes:

- `com.meta.agenticxr.AgenticMediaProjectionActivity`
- `com.meta.agenticxr.AgenticMediaProjectionService`
- `FOREGROUND_SERVICE` and `FOREGROUND_SERVICE_MEDIA_PROJECTION`
- Optional `com.oculus.experimental.enabled`

The local manifest-merger blame report attributes the projection components/permissions to `XrApiLayer_METAX_operator_unity_android`. Both projection components are non-exported. This finding demonstrates bundled development functionality, **not** a demonstrated remote exploit, active screen capture, or leaked credentials.

The existing build log confirms exclusion of `DevAgentSettings.asset`. That safeguard does not remove the operator binary. Exclude the operator layer and unnecessary development components from a release build, then recheck the final manifest, native libraries, and network behavior. Meta requires minimal necessary permissions; do not assume projection is prohibited in all apps, but this app has no documented user-facing need for it. [Security and input VRCs](https://developers.meta.com/vr/resources/publish-quest-req/).

## Licensing and rights

### L1 — Unity distribution authorization is unresolved for both binary routes

The reviewed Unity terms are dated June 30, 2026. Section 2.2.1 makes Industry runtime distribution conditional on Unity's express authorization. The definition includes uses not solely for games or entertainment; Unity decides classification. Home control makes this relevant, but this review cannot make Unity's binding classification.

An activated editor license alone does not prove permission to distribute this app. Obtain written confirmation covering the actual accepted terms, plan, financial eligibility, this home-control use, free GitHub APKs, and paid store distribution. Do not assume the Personal revenue threshold or the absence of a Unity 6 Runtime Fee resolves section 2.2.1. No invoice, contract, revenue information, or Unity authorization was inspected. [Unity terms](https://unity.com/legal/editor-terms-of-service/software), [Unity eligibility FAQ](https://unity.com/legal/editor-terms-of-service/software/faq).

### L2 — Existing notices are useful, but component obligations need completion

The app has an accessible About/licenses screen in `QuestExperience.cs`, loading `Resources/Legal/ThirdPartyNotices.txt`. This is a positive finding; notices are not missing entirely.

| Component | Evidence and remaining work |
| --- | --- |
| Meta XR Core, Interaction SDK, MRUK 207 | Local notices refer to Meta SDK terms. Verify applicable notices and distribution conditions for the exact resolved packages |
| Unity wrappers and runtime | Multiple Unity package licenses are included. Map runtime/native obligations as well as package wrappers |
| ZXing.Net | Apache-2.0 license and notice present |
| Newtonsoft.Json | MIT notice present in the collected notices |
| QRCode.js helper | MIT file beside the helper; retain it when distributing the helper |
| MRUK transitive components | Notices include Boost, ISC, MIT, BSD, Apache, and MPL-2.0 text. Map covered components to exact source/version and any required source-availability notice |

MPL text in the notice collection does **not** prove that the whole app must become open source, nor does printing the text alone prove compliance for an included MPL component. Establish which covered software ships and how recipients obtain its corresponding source where required. [Mozilla MPL FAQ](https://www.mozilla.org/en-US/MPL/2.0/FAQ/).

Meta SDK terms permit application development/distribution subject to restrictions, including approved platforms and separate terms for marked materials. They do not automatically forbid selling this app. Do not relicense SDK material under a blanket project license. [Meta SDK agreement](https://developers.meta.com/vr/licenses/oculussdk/).

No root license for original project code was found. Choose an explicit license or proprietary source-available terms with appropriate dependency exclusions. Missing a root license is not itself proof that the owner cannot sell their own work, but readers do not automatically receive general reuse rights. [GitHub licensing guidance](https://docs.github.com/en/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/licensing-a-repository).

### L3 — Branding and generated content

Artwork provenance is recorded in [ART-PROMPTS.md](ART-PROMPTS.md) and [ART-MAGIC.md](ART-MAGIC.md); procedural symbols and sounds are implemented in project code. This supports provenance, not a trademark clearance or guarantee against third-party claims.

The in-app About text correctly states that the app is independent of Meta and Home Assistant. Keep that distinction in listings. Review the name “HA goes Quest” and proposed logos before commercial use; no trademark-register clearance was performed. Do not use official partnership/certification badges without permission. [Home Assistant terms and marks](https://www.home-assistant.io/tos/).

## Privacy and security

### S1 — Short in-app disclosure is not a publication-ready privacy policy

The About screen describes encrypted credentials, HA requests, local QR processing, and Meta spatial services. No full public privacy-policy document or policy URL was found in the inspected repository/app code. An external policy may exist, but none was supplied or verified.

Provide a stable public policy, link it in-app and in store submission, and identify the responsible publisher/contact. Describe actual data categories, destinations, retention/deletion, support data, and platform SDK processing. Meta requires an up-to-date public privacy policy; where GDPR applies, determine roles, legal bases, disclosures, and rights rather than assuming every local operation makes the publisher a controller. [Meta data-use policy](https://developers.meta.com/vr/policy/data-use/), [GDPR](https://eur-lex.europa.eu/eli/reg/2016/679/oj/eng/).

| Data | Observed path | Remaining publication work |
| --- | --- | --- |
| HA base URL/token | AES-GCM file, non-exportable Android Keystore key | Real update/restart/forget test; explain token revocation |
| State and service requests | Configured HA server, bearer authorization | Prefer HTTPS; disclose explicit HTTP opt-in sends credentials unencrypted |
| QR frames | On-demand camera, local decoding, stop on leaving scan/pause | Permission-denial and lifecycle tests; document permission purpose |
| Camera/image frames | HA proxy snapshots, transient textures | Verify network behavior; no blanket zero-telemetry claim |
| Entity/anchor mappings | Per-server JSON plus backup and platform anchors | Define complete deletion behavior across servers, backups, and failed anchor cleanup |
| SDK behavior | Includes telemetry-named and operator libraries | Inventory/configure actual runtime behavior; names alone do not prove data is sent |

Forgetting a connection deletes credentials/key and returns to demo mode; it does not establish deletion of all mapping backups or platform anchor records. Avoid describing it as “delete all data.” Individual anchor cleanup may fail after local mapping removal.

### S2 — Protections found, with remaining limits

Verified in source: normal HTTPS validation, redirects disabled, eight-second request timeout, no automatic command retries, stale-state gating, restricted light/switch commands, encrypted persisted token, no editor token persistence, and final `allowBackup=false`. No custom certificate-validation bypass was found in the inspected request path.

Media limits apply **after** a buffered download, and pixel dimensions are checked **after** `LoadImage`. A large or malformed response from the configured server can therefore consume memory before rejection. Add a bounded streaming download and pre-decode dimension checks; apply similar response limits to state/template requests. This is a resilience finding, not a demonstrated external compromise.

A `switch` domain is not inherently a harmless lamp: it may control plugs or other equipment. Consider explicit user opt-in for each controllable entity and clearly exclude safety-critical use. Gesture readiness and successful HTTP acceptance do not prove the physical device reached the intended state.

Unity 6000.0.66f2 is newer than the 6000.0.58f2 fix listed for CVE-2025-59489; that advisory does not identify this configured version as an older vulnerable version. This is one advisory check, not a complete vulnerability scan. [Unity advisory](https://unity.com/security/sept-2025-01).

## Store and hardware acceptance

- Complete [QUEST-TEST.md](QUEST-TEST.md) on each advertised headset/OS. User feedback is valuable, but no completed evidence record was found for cold starts in multiple rooms, system room dialogs, recalibration, hand switching, token-preserving updates, or 30-minute performance.
- Validate startup, passthrough, focus loss, system gestures, controller/hand transitions, permissions, offline recovery, and accurate supported-device declarations against current required VRCs.
- First-launch language currently defaults to German via `Get("english")`, not headset language. Implement supported-language detection and English fallback; VRC Functional.13 is currently a recommendation, not a required failure.
- No platform entitlement check was found in app code. Security.1 is currently recommended, not a hard blocker; decide the paid-store entitlement strategy.
- Prepare developer organization/app record, verified submission identity, screenshots, description, rating, support contact, pricing, review instructions, and reproducible reviewer access/demo. Dashboard status was not inspected.
- Document the QR camera purpose for permissions review. Do not infer approval from runtime permission grant. [Review-requiring permissions](https://developers.meta.com/vr/resources/permissions-review-required/).

Required versus recommended distinctions above follow the [current VRC table](https://developers.meta.com/vr/resources/publish-quest-req/). A unit-test pass is not a VRC pass.

For paid German/EU consumer distribution, review publisher information, applicable withdrawal rules for digital content, statutory conformity/security updates, and the store's allocation of responsibilities. A generic “prototype/no warranty” label does not settle these obligations. [DDG §5](https://www.gesetze-im-internet.de/ddg/__5.html), [BGB §327f](https://www.gesetze-im-internet.de/bgb/__327f.html), [BGB §356](https://www.gesetze-im-internet.de/bgb/__356.html).

## Existing public releases

The repository currently exposes 0.6.3, 0.6.2, 0.6.1, 0.6.0, 0.5.0, 0.4.1, 0.3.2, 0.3.0, and 0.2.0 APK releases. Several, including 0.6.3, are marked as normal releases rather than prereleases.

Earlier documentation says pre-0.6.1 development APKs could include local SDK settings. This review did not inspect those older asset contents and does not establish a leak. Triage those binaries privately; if exposed credentials are confirmed, rotate the affected credentials and remove/replace affected artifacts. Merely deleting a download would not revoke credentials. Do not publish secret values in an issue or report.

Recommended immediate release hygiene: mark experimental releases clearly, link this review, and stop presenting an unaccepted development APK as stable. These are recommendations; this review did not change release metadata or remove downloads.

## Completion plan

| Priority | Action | Evidence required to close |
| --- | --- | --- |
| 1 | Resolve Unity classification/distribution rights | Written applicable authorization/confirmation covering both routes |
| 1 | Audit older public APKs privately | Sanitized finding record and remediation if exposure is confirmed |
| 1 | Create a production build configuration | Non-debug APK, corrected manifest, no operator layer, v2 signature, artifact hash |
| 1 | Decide signing and tester migration | Secure key backup and tested upgrade/migration path; no unsupported token-retention promise |
| 1 | Publish accurate privacy policy and complete notices | Working public URL, app link, component/source mapping, deletion instructions |
| 2 | Bound downloads and verify SDK behavior | Targeted failure tests and device network evidence |
| 2 | Run hardware acceptance | Dated results, headset/OS, multi-room and update tests, performance measurements |
| 2 | Prepare store and consumer-facing materials | Dashboard validation, reviewer instructions, support and commercial terms |
| 3 | Improve language defaults and paid entitlement handling | Documented behavior and regression checks |

A cleaned and legally cleared beta can precede a paid release. The existing APK should not be promoted as store-ready, secure by audit, or certified.
