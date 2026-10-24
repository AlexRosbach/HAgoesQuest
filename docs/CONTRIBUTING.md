# Development and commenting style

## Reference and language

The structure follows [LanLens](https://github.com/AlexRosbach/LanLens/tree/b8fa87166ed39ff6f35305fbca6dec6ead7eda20): a quick introduction in the README, a technical reference, a symptom-oriented knowledge base, and short English module/function descriptions. Examples there include `docs/documentation.md`, `docs/knowledgebase.md`, `backend/services/scan_targets.py`, and `backend/services/crypto.py`.

HAgoesQuest uses English for repository documentation, code identifiers, and technical comments. The app retains its German and English localization. Python docstrings are adapted conceptually as C# XML comments; third-party implementations and licensing statements are not copied.

## What to comment

- Class descriptions: The module's responsibility and relevant limitations.
- Public or complex interfaces: Inputs, return meaning, units, error behavior, or side effects where these are not apparent from the signature.
- Inline comments: Why a safeguard, ordering, or unusual decision is necessary.
- State machines: Who resets the state, who owns the target, and when does a one-time event occur?

Simple assignments and self-explanatory getters do not need narration. Comments describe actual code, not planned functionality. Example:

```csharp
/// <summary>Returns true once navigation has settled and the previous input is released.</summary>
/// <param name="active">Whether a pinch, trigger or near-menu touch is still active.</param>
/// <param name="time">Monotonic unscaled time in seconds.</param>
public bool Step(bool active, double time)
```

A useful inline comment might explain why a timeout must not trigger a second HA command. “Set busy to false” beside `Busy = false` adds no value.

## Maintaining documentation

| Change | Update |
| --- | --- |
| Visible interaction | `INSTALLATION.md`, `USER-GUIDE-EN.md`, and the knowledge base if needed |
| Interface, persistence, or state flow | `documentation.md` and affected code comments |
| Reproducible failure | `knowledgebase.md` with symptoms, meaning, and checking steps |
| Release | `CHANGELOG.md`, version/release notes, and build evidence |
| Comments or text only | An Unreleased entry; do not claim a new APK |

Historical version files remain as records. The README provides the current entry point and links to the changelog for older changes. Do not rewrite an earlier build report as though a new hardware test had been performed.

## Appropriate checks

Check core logic with `tests/Run-Tests.ps1` or .NET 8. For changed Unity input/network flows, use `tools/Test-Network.ps1`. Render presentation changes in the editor and check them on the Quest. Commands and limitations are in the [technical reference](documentation.md#verification).

For comment-only changes, verify that C# tokens outside comments and formatting remain unchanged. Markdown links to local files must resolve. A complete APK rebuild is unnecessary for these changes.

## Data and publication

Tokens, signing keys, private SDK settings, and personal configuration files do not belong in the repository. Changes to the package name, signature, Keystore alias, or anchor formats must explicitly document update consequences and any required migration. `PrivateBuildSettings` must continue excluding local SDK settings from the player. Preserve third-party license texts, copyright statements, and notices; another project's documentation style does not transfer a license.
