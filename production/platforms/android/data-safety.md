# BRASSLIGHT - Google Play Data safety form: answers and rationale

> Owner: `android-publishing-specialist`. Reviewed by `legal-compliance-specialist` before submission.
> **Last verified: 2026-11-08** against the project's own design only. The Play Console form wording
> was **not** re-fetched; compare each answer with the live form at submission time (V).
> The Unity project does not exist yet, so every answer below is a **design commitment** that must be
> confirmed against the real merged manifest and SDK list of the release build.

## Target outcome

"**No data collected**" and "**No data shared**". This holds only while the following stay true.

## Preconditions (any change requires updating this file first)

1. No ads SDK, no IAP / billing library, no analytics, no crash-reporting SDK that sends data off-device (anti-pillar, `technical-preferences.md`).
2. No user accounts, no login, no cloud backend of our own.
3. Saves are stored locally in app-private storage. Android Auto Backup (Google's own backup of app data to the user's Google Drive) may copy the save; this is a user-controlled OS feature, not collection by us, and Google's form guidance treats it as not collection by the developer (V: confirm in the form's help text).
4. Unity engine itself: standard Unity runtime without Unity Analytics / Unity Gaming Services / Unity Ads. Verify in the release build: Unity 6 may include engine-level connectivity or data features depending on packages and settings. Run the following checks before each release:
   - Package Manager lists none of: Analytics, Ads/Advertisement, In-App Purchasing, Cloud Diagnostics, Unity Gaming Services packages.
   - Merged manifest has no `INTERNET` permission, or if it has one, every network call is documented here.
   - Network capture on a test device during a full session shows no outbound traffic (Wireshark / mitmproxy / a firewall app).
5. Play Games Services is **not** used at launch. If added later, answers change (see "Future changes").

## Form answers

| Form question | Answer | Rationale |
|---------------|--------|-----------|
| Does your app collect or share any of the required user data types? | **No** | Offline game, no SDK sending data (preconditions 1-4). |
| Is all of the user data collected by your app encrypted in transit? | Not applicable (shown only when data is collected) | No data leaves the device. |
| Do you provide a way for users to request data deletion? | Not applicable | No account or server-side data. Uninstalling removes local data. |
| Data types (location, personal info, financial info, health, messages, photos/videos, audio, files, calendar, contacts, app activity, web browsing, app info and performance, device or other IDs) | None selected | Nothing collected. |
| Data shared with third parties | None | Nothing shared. |
| Security practices: independent security review | No | Not performed; optional and not required. |
| Privacy policy URL | Public URL (required even with "no data collected") | Hosted by `legal-compliance-specialist`; text must match this file. |

## Related declarations (consistency check)

| Declaration | Answer |
|-------------|--------|
| Ads | No ads |
| Advertising ID | Not used (ensure `AD_ID` permission is removed from the merged manifest) |
| Target audience | 13+ / 16+ (not designed for children) |
| Content rating | IARC questionnaire: no user-generated content, no chat, no location sharing, no digital purchases |
| Permissions | None requested at runtime |

## Rationale notes

- **Crash reporting**: Google Play's Android vitals gathers crash and ANR data from users who opted into sharing diagnostics with Google. That is Google's collection, not ours, and does not need to be declared. We do not add Firebase Crashlytics, Sentry or similar. If post-launch crash visibility proves insufficient, the owner must decide explicitly; the form would then change to "App info and performance: Crash logs / Diagnostics", collected, not shared, optional or not, with processing by a service provider (V: service-provider exemption wording in the form).
- **Network access**: the game is designed offline (premium, no live service). Local-only leaderboards or achievements are acceptable. Online leaderboards are out of scope.
- **Steam/iOS parity**: the same "no data collected" stance should appear in the Apple App Privacy label (`ios-publishing-specialist`) and the Steam privacy policy, so one privacy policy covers all stores.
- **GDPR**: with no personal data processing by the developer, obligations are minimal, but the policy still has to state what is processed (nothing), the contact (Impressum), and the Auto Backup behaviour. `legal-compliance-specialist` finalises the text.
- **Accuracy matters**: Google can suspend apps whose declared data safety answers do not match actual behaviour, so the CI checks below back this file.

## CI / release gates

- [ ] `bundletool dump manifest` on the release AAB: list of `uses-permission` is empty or matches this file.
- [ ] Dependency scan: no analytics / ads / billing / Firebase / Facebook / Adjust / AppsFlyer / Unity Analytics classes (`apkanalyzer dex packages` or grep of the dex).
- [ ] Network capture of a 15-minute session: zero outbound connections.
- [ ] Privacy policy text reviewed against this file for this release.
- [ ] Owner submits the form; keep a screenshot of the submitted answers in `production/platforms/android/` (without personal data).

## Future changes that trigger a rewrite

| Change | Likely effect on the form |
|--------|---------------------------|
| Play Games Services (achievements, cloud save) | Collects "Personal info: user IDs" and "App activity" (via Google Play services); verify Google's current guidance on whether PGS needs declaration |
| Crash reporting SDK | "App info and performance: crash logs, diagnostics" |
| Any analytics | "App activity", "Device or other IDs", and likely sharing |
| Online features or accounts | Many categories; deletion mechanism required |
| Remote Addressables CDN | Network requests expose IP address to the host; check if that counts as collected data |
