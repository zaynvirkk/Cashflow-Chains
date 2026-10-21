# Cashflow Chains: remaining release risks

21 October 2026. The full offline local edition replaces the initial three-puzzle prototype. Current evidence and downloadable artifacts are in MOBILE_ACCEPTANCE.md and Builds/Deliverables/DELIVERY.json.

- Real-player visual approval, first-session comprehension, satisfaction, voluntary replay, retention and revenue are unmeasured. Automated winning solutions do not prove a good difficulty curve or a profitable game.
- Physical Android phones, vibration feel, notches, battery, thermal behavior, accessibility services and interruption variety still need device acceptance. Emulator runs use ARM64 translation on API 35 and cannot establish phone performance.
- Native text, saved music/effects/haptics switches, reduced motion and high contrast are implemented. English is the supported language; translated catalogs, TalkBack and a complete accessibility audit remain release work.
- Local store configuration is absent. Unity IAP 5.4.4 code validates before granting and fails closed, but genuine pending/deferred/failed/restore/refund transactions, server revocation and store compliance are unverified. No real checkout, account, publishing, ads or analytics uploader is active.
- APKs are local ARM64 IL2CPP playtests signed with the local debug key. Store signing, AAB submission, privacy/data labels and licensed closed tests are separate. Steam, Windows, iOS, Steam Deck and controller releases are unverified.
- Saves use atomic writes, validation and backup recovery. Local checksums detect corruption; they do not establish a server-authoritative anti-cheat system. Uninstalling can remove local progress.
- First-time font/menu work causes short frame spikes. See measured frame distributions in the actual reports. No low-end phone or universal 60 fps claim.
- The earlier laptop crash was a global out-of-memory event while multiple Unity processes and an emulator ran together. Current scripts cap workers; use one Unity process at a time. The final test runs completed with that resource limit.

No unresolved P0 or P1 defect was observed in the completed local acceptance flows. These external and subjective release boundaries remain open. No commercial outcome or future store approval is promised.
