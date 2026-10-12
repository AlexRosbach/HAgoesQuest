# 0.3.1 – Retain credentials across updates

The app continues using the same package name, Android Keystore alias, and encrypted credential file as 0.2.0 and 0.3.0. The version code increases to 4. Existing credentials load automatically at startup.

The connection menu indicates a stored token without exposing its contents. An empty token field uses the last confirmed credentials only for the same normalized server address, including protocol and port. A rejected replacement token does not overwrite confirmed credentials. HTTP permission is restored when opening the menu. Removing credentials also clears reuse in memory.

## Installation

`adb install -r HAgoesQuest-0.3.1.apk`

Do not uninstall the previous app or clear its data. The APK installs as an update. Missing credentials or a revoked token require a new login. Hardware testing of an actual Quest update remains outstanding.

## Preserve the signature

Build-Quest.ps1 checks the existing local signing key before building and the APK certificate afterward. SHA-256 fingerprint:

`345caca35c4a1ad2eb8f2243a0ccda712945c87b9b089714f2778c007c76e5b2`

A mismatch aborts the process. Back up the existing development key privately and restore it at the existing Android debug-keystore path on a new build machine. Never upload keys to the repository or release. A new key would be incompatible with existing installations. [Android app signing](https://developer.android.com/studio/publish/app-signing).

## Device test

1. Connect in 0.2.0 or 0.3.0 and save a mapping.
2. Exit the app and install 0.3.1 over it with `adb install -r`.
3. Start: load entities without entering a token and recover the saved position.
4. Open the connection menu and test again with an empty token field.
5. Restart the Quest and check automatic login again.
