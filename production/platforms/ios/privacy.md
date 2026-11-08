# iOS Privacy: App Privacy Label and Privacy Manifest, BRASSLIGHT

> Owner: `ios-publishing-specialist`. Draft v0.1, 2026-11-08. Unity project does not exist yet; this is the plan and must be re-checked against the real Xcode project.
> Tags: **(V)** verified 2026-11-08 at the source named; **(U)** unverified. Unity documentation hosts (docs.unity3d.com) were blocked from this session, so Unity's exact required-reason list is only partly verified (search-result snippets of Unity's Apple privacy manifest policy page).

## 1. App Privacy label ("nutrition label")

Design basis: premium game, no ads, no IAP, no analytics, no accounts, no network services (technical preferences forbid ads SDKs and analytics that collect personal data).

Answer in App Store Connect > App Privacy:

| Question | Answer |
|----------|--------|
| Do you or your third-party partners collect data from this app? | **No** -> label shows **Data Not Collected** |
| Privacy Policy URL | Required (published by legal-compliance-specialist); state plainly that no data is collected |
| Tracking (ATT) | None. No `NSUserTrackingUsageDescription`, no IDFA, "Advertising identifier" = No at submission |

"Collect" means data transmitted off the device in a way that lets you or a partner access it. Local save files and PlayerPrefs that never leave the device do not count. Apple definition to re-read when answering: https://developer.apple.com/app-store/app-privacy-details/ (U, not fetched).

This answer becomes false if any of the following is added, so each needs a re-review before shipping:
- Game Center (Apple processes it; typically still not "collected by you", but confirm the current Apple guidance (U))
- iCloud/CloudKit saves (data in the user's own iCloud; confirm treatment (U))
- Crash reporting or analytics SDKs (currently forbidden)
- Any web request carrying device identifiers (e.g. Unity services). Verify Unity Analytics / Cloud Diagnostics / Services are **not** enabled in the project (U).

## 2. Privacy manifest (PrivacyInfo.xcprivacy)

Apple requirement (V, https://developer.apple.com/news/upcoming-requirements/): approved reasons for listed "required reason" APIs, including those used by third-party SDKs, must be declared or the upload to App Store Connect is rejected.

What the app-level manifest must contain
- `NSPrivacyTracking` = false
- `NSPrivacyTrackingDomains` = empty
- `NSPrivacyCollectedDataTypes` = empty array (consistent with Data Not Collected)
- `NSPrivacyAccessedAPITypes` = every required-reason API category used by game code, plugins and the engine, with approved reason codes

### Required-reason API categories commonly hit by Unity games

Unity documents that the engine calls required-reason APIs and declares them (Unity 6 "Apple privacy manifest policy" page, seen only as search snippets; (U) for full list). Snippets showed:

| Category (Apple API type) | Reason code | Unity use (per Unity doc snippet) |
|---|---|---|
| User defaults (`NSPrivacyAccessedAPICategoryUserDefaults`) | CA92.1 | PlayerPrefs |
| System boot time (`...SystemBootTime`) | 35F9.1 | time between events |
| Disk space (`...DiskSpace`) | E174.1 | AssetBundle / disk space checks |
| File timestamp (`...FileTimestamp`) | 0A2A.1 (per 2023.2 page) | C# file timestamp APIs |

The reason codes above are from search snippets of Unity documentation, not from Apple's reason list; confirm each code against Apple's list (https://developer.apple.com/documentation/bundleresources/describing-use-of-required-reason-api) (U, not fetched) before relying on it.

### What Unity 6 ships itself (U)

- Unity states the engine declares its own categories and that the project must declare usage by its own code and third-party plug-ins. Older Unity docs (2023.2) told developers to create a manifest and place it in `Assets/Plugins`. Whether Unity 6.3 merges or generates the manifest in the Xcode project automatically was **not settled** by what I could read.
- Verification procedure (required, cheap, definitive):
  1. Build iOS from the real project and open the Xcode project.
  2. Search for every `PrivacyInfo.xcprivacy` (`find . -name PrivacyInfo.xcprivacy`) and check that each is a member of the app or framework target (UnityFramework).
  3. In Xcode: Product > Archive > Distribute > generate **Privacy Report**; it aggregates all manifests.
  4. Upload a TestFlight build; Apple emails ITMS-91053 style warnings for missing reasons. Resolve all before App Review (code number is (U)).
- Decision for the owner/unity-specialist: if Unity does not inject the engine's declarations, add a project-level `Assets/Plugins/iOS/PrivacyInfo.xcprivacy` and a build post-process step that guarantees it ends up in the app target. Do not hand-edit the generated Xcode project (it is overwritten each build).

### Game-code and plugin audit

| Source | Check | Status |
|---|---|---|
| `Vamp.*` C# (save system: file read/write, `File.GetLastWriteTime`, free-space check) | Each timestamp or disk-space call adds a required-reason category | open |
| PlayerPrefs for settings | UserDefaults category (CA92.1) | open |
| Input System, URP, Addressables, Localization, Burst/Collections | Unity packages; check for their own manifests in the package folders | open |
| Apple Unity Plug-ins (GameKit), if adopted | ships its own manifest? check | open, optional feature |
| Any new dependency | needs ADR + license check + manifest review | rule |

Practical rule: add a CI step (devops-engineer) that fails the iOS job if the exported project contains no `PrivacyInfo.xcprivacy`, and that lists all manifests found.

## 3. Other privacy-related items

- No permission prompts: leave camera, microphone, photos, location, tracking usage strings empty; unused usage strings can cause review questions.
- Privacy policy: required URL even with no collection; must be reachable, and available in DE and EN. Impressum separately (legal-compliance-specialist).
- Children: with a likely 13+ rating the game is not in the Kids Category; do not select Kids.
- GDPR: no personal data processed by the developer; the policy states this. Owner/lawyer confirm.
- Re-review this file at every update and whenever the Unity version, a package or a plugin changes.

## 4. Sign-off

- [ ] Real Xcode project inspected for manifests
- [ ] TestFlight upload without privacy warnings
- [ ] Label answered "Data Not Collected" by owner in App Store Connect
- [ ] legal-compliance-specialist reviewed privacy policy text

Verification date for this draft: 2026-11-08.
