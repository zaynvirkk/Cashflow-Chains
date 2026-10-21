# Release testing

Automated replay verifies behavior and presentation defects. It does not measure human enjoyment, retention or revenue. Keep initial release free while gathering actual player feedback; no analytics SDK or telemetry was introduced.

For a first human session, start a new local save and avoid giving instructions outside the game. Observe whether the player connects the first cafe, understands the farm arrow, distinguishes score from persistent city coins, and can explain why a changed layout produced a different forecast. Ask which action felt satisfying, which result was surprising, and where they felt stuck. Do not call scripted time-to-win a human usability result.

Run one desktop pointer replay at a time; each starts an isolated review save. The full campaign uses ordinary controls and legitimately earns its stars. Earned-feature tests copy that genuinely earned save byte-for-byte, then operate visible controls. Review instrumentation exports read-only state/geometry only when `-capture-dir` is supplied; normal builds have no player-facing debug console or level-solve shortcut.

The accepted release evidence must bind to current C# source and player bytes. Rebuilds or source changes require the relevant replay again. Failed runs remain local diagnostic evidence and do not count as passes.

Android checks use the signed release APK on a local Android 16 Google APIs emulator with 16 KB pages. Static audits inspect every ARM64 ELF load segment and APK ZIP alignment; the AAB is upload-key signed and checked by the official bundletool. Physical-device touch latency, haptic strength, thermals, battery consumption and Play-generated splits still need genuine device/Play evidence.

Google Play submission requires a public privacy-policy URL, accurate Data safety/content-rating/target-audience declarations and the account's applicable testing requirements. No public deployment, account creation or tester recruitment is part of the local build task.
