# Forking and contributions

HA goes Quest is a completed personal case study. Active development is paused, but forks and further experiments are welcome. No project-wide license for the original code has been selected; preserve all third-party license texts, copyright statements, and notices.

Use the [technical reference](documentation.md#development-and-build) to open and build the Unity project. Keep documentation, identifiers, and technical comments in English; the app supports German and English. Explain non-obvious state transitions and update the user guides when behavior changes.

The repository does not include an automated test suite or test CI. For changes to the app, check compilation in Unity and try the affected behavior on a Quest. Keep local documentation links valid and Unity assets paired with their `.meta` files.

Tokens, signing keys, private SDK settings, and personal configuration files do not belong in Git. Keep the build safeguard that excludes local SDK settings. Changes to the package name, signature, Keystore alias, or saved data formats need an explicit update/migration plan.
