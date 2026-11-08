# BRASSLIGHT - Google Play (Android) Publishing Checklist

> Owner: `android-publishing-specialist`. Account-level actions (fees, tax/bank, signing agreements,
> "Submit for review", "Publish") are done by the **owner**. No secrets in this repo.
>
> **Last verified: 2026-11-08.** Re-verify every item marked (V) before the closed test starts and
> again before the production submission. The Unity project (`src/Brasslight/`) does not exist yet
> (roadmap 0.4), so all Unity settings below are planned, not yet applied.

## Verification log

| Item | Result | Source | Confidence |
|------|--------|--------|------------|
| Target API level | New apps and updates must target **Android 16 (API 36)** or higher since **2026-08-31**. Existing apps need API 35+ to stay available to new users on newer OS. Extension to 2026-11-01 existed and has expired. Next step expected around Aug 2027 (API 37), not yet announced. | https://developer.android.com/google/play/requirements/target-sdk (fetched 2026-11-08) | High (official) |
| 16 KB page size | Apps targeting API 35+ must support 16 KB memory pages on 64-bit devices. **From 2027-02-01 updates that do not comply cannot be released.** Third-party posts cite earlier dates (Nov 2025, May 2026), so treat the Play Console warning as the live authority. | https://developer.android.com/guide/practices/page-sizes (fetched 2026-11-08) | High (official) |
| Closed test, new personal accounts | Personal accounts created after 2023-11-13: closed test with **at least 12 testers, opted in continuously for 14 days**, before applying for production access. Older sources say 20 (outdated). Organisation accounts are exempt. | Google Help 14151465 (https://support.google.com/googleplay/android-developer/answer/14151465), read via search snippet only; the page itself was blocked from the build sandbox | Medium: owner must re-read the page |
| Unity 6.x and API 36 / 16 KB | Unity documents API 36 support from 6000.0.46f1 and 16 KB support. Forum reports exist of Unity 6000.3.0f1 AABs flagged for 4 KB alignment. No official 6.3-specific confirmation found. | Unity docs (Android requirements and compatibility, 6000.3) via search; forum threads | Medium: **test a real AAB early** |
| Android developer verification | Google is introducing developer identity verification for apps installed on certified devices. Enforcement starts 2026-09-30 in Brazil, Indonesia, Singapore, Thailand, wider in 2027. Secondary sources say Play-published apps were pre-registered and a $25 fee applies to distribution outside Play. | https://developer.android.com/developer-verification (landing page only); secondary press | Low: owner must check Play Console for any "verify identity / register app" prompts |
| Play Console registration fee | One-time fee, historically **USD 25**. Not re-confirmed from an official page today. | (V) Play Console sign-up page | Unverified |
| DSA trader status, IARC, Data safety form fields | Not re-fetched today. Follow the in-console wording at submission time. | (V) Play Console | Unverified |

## 0. Decisions needed from the owner

| # | Decision | Options | Recommendation |
|---|----------|---------|----------------|
| D1 | Account type | **Personal**: no D-U-N-S, but needs identity verification and the 12 testers / 14 days closed test. **Organisation**: D-U-N-S number (up to ~30 days), exempt from the closed-test rule, but needs a legal entity (UG/GmbH or a registered business). | If a business entity already exists or is planned for tax reasons (ask the Steuerberater, roadmap 0.7), use Organisation. Otherwise Personal and recruit testers early. Account type cannot be changed lightly later. |
| D2 | Trader status (EU DSA) | Declaring as trader publishes address, phone and email on the store page. | A paid game sold for profit by an individual in Germany will almost certainly be a **trader**. Confirm with `legal-compliance-specialist`. Use a business address or a mail-forwarding address that is legally sufficient, plus a dedicated support email. Do not use the private home address if avoidable. |
| D3 | Package name | `com.<owner>.brasslight` (permanent after first upload). | Use a domain/handle you control, lowercase, e.g. `com.<ownerhandle>.brasslight`. Never change it. Same bundle ID idea on iOS for consistency. |
| D4 | Price | Concept says EUR 3.99 mobile. | Set base price in EUR, let Play convert, review per-country prices. Premium apps cannot later become free-with-IAP without a new listing consideration, so decide deliberately. |
| D5 | Play Games Services | Optional achievements/cloud save behind `Vamp.Platform.Android`. | Defer until after 1.0 unless cloud save is a launch goal. If adopted, update `data-safety.md` first. |

## 1. Play Console account (owner)

- [ ] Choose account type (D1). Google Play Console sign-up with a dedicated Google account (not a daily-use one), 2-step verification on, recovery options set.
- [ ] Pay the registration fee (V: historically USD 25 one-time).
- [ ] Complete identity verification (government ID; organisation: D-U-N-S and organisation docs). Allow days to weeks.
- [ ] Verify contact phone and email. Developer contact email shown publicly: use a dedicated address.
- [ ] Check Play Console for any Android developer verification / app registration prompts (V, see log) and complete them.
- [ ] Declare DSA trader status (D2) and complete the address/contact verification.
- [ ] **Payments profile / merchant account** (needed to sell a paid app): create payments profile, bank account (IBAN), tax info. Germany: VAT/USt-IdNr. and Steuerberater input on how Google's EU VAT handling affects you (Google acts as merchant of record in many countries, verify). Owner executes, `legal-compliance-specialist` and Steuerberater advise.
- [ ] Accept the Paid Apps / merchant distribution agreement in Play Console (owner signs).
- [ ] Add team users with minimal roles if others will upload (release-manager, devops). CI uses a service account only if automated upload is adopted (see section 5); the JSON key is a CI secret, never in the repo.

## 2. Closed testing (new personal accounts)

Goal: be allowed to apply for production access. (V) Re-read https://support.google.com/googleplay/android-developer/answer/14151465 first.

- [ ] Create the app in Play Console (name, default language, App (not Game? choose **Game**), **Paid**). Free/paid cannot be switched from free to paid later; decide **Paid** at creation.
- [ ] Complete App content declarations needed for testing (privacy policy URL, ads: No, target audience, data safety, content rating) - they are required before production anyway.
- [ ] Create a Closed testing track, add a Google Group or email list of **at least 12 testers, ideally 20-25** (buffer for drop-outs).
- [ ] Testers opt in via the opt-in link and install from Play. They must **stay opted in for 14 continuous days** at the time you apply.
- [ ] Recruit early: start recruiting at the end of Phase 4 so the 14 days can run during Phase 5 beta. Sources: Steam wishlist community, Discord, friends, testers' community groups. Do not pay for or fake testers (policy risk).
- [ ] Ship at least one real build to the track and ship at least one update during the window; collect feedback (also feeds QA).
- [ ] Apply for production access in Play Console after day 14; Google review typically up to ~7 days (V). Plan 3 weeks of calendar time minimum from first opt-in.
- [ ] Internal testing (up to 100 testers, no review) is useful for CI smoke tests but does **not** count towards the requirement.
- If organisation account (D1): skip this section, but still use internal and closed tracks for QA.

## 3. Unity project settings (Unity 6.3 LTS, to apply when project exists)

Owner: `mobile-platform-programmer` + `devops-engineer`. Settings live in `ProjectSettings/` and Build Profiles, committed.

| Setting | Value |
|---------|-------|
| Build format | **Android App Bundle (AAB)** for Play. APK only for local debugging |
| Scripting backend | **IL2CPP** |
| API compatibility level | .NET Standard 2.1 (default) |
| Target architectures | **ARM64 only**. Add ARMv7 only with a documented device-share justification (ADR). Note Google Play requires 64-bit support; ARMv7-only is not allowed |
| Minimum API level | **26** (Android 8.0) per technical-preferences |
| Target API level | **36** (explicit, not "Automatic highest installed" so builds are reproducible). Re-check each August. Requires Unity version with API 36 support (6000.0.46f1+ documented; confirm 6.3 patch notes) |
| Orientation | Landscape only: allow Landscape Left and Right (auto-rotation between the two), disable Portrait and Upside Down |
| Package name | `com.<owner>.brasslight` (D3) |
| Version | `versionName` = semver (e.g. 0.1.0). `versionCode` = CI build number, monotonically increasing integer, never reused, stay below 2,100,000,000 |
| Graphics API | Vulkan primary with OpenGL ES 3.x fallback (check URP Forward+ requirements and device test results; ADR-0002) |
| Input | Input System package only |
| Custom Gradle | Use Unity's AGP; if custom templates are needed, keep them committed and reviewed |
| Split binary / PAD | Not needed while base AAB <= 150 MB. If exceeded, use Play Asset Delivery (install-time or fast-follow packs via Addressables-compatible setup, coordinate with `unity-addressables-specialist`) |
| Texture compression | ASTC (Android baseline with API 26+ GPUs; check device catalog) |
| Strip engine code / managed stripping | Medium or High, tested with IL2CPP link.xml for reflection |
| Debug symbols | Generate symbols (`public`/full) and upload with each release for native crash deobfuscation in Play vitals |
| Development build | Off for release |
| Android splash / app icon | Adaptive icon (foreground/background layers) plus legacy; round icon |
| Safe area | Respect cutouts (`Screen.safeArea`), render into display cutout mode "short edges" |
| Pause on focus loss | Required by technical-preferences |

### 16 KB page size
- [ ] Use a Unity 6.3 patch that documents 16 KB support. Keep Unity's bundled NDK / AGP; do not downgrade.
- [ ] After every build that changes native code, run an alignment check on the AAB's `.so` files (LOAD segments aligned to 0x4000 / 16384) with `zipalign -c -P 16 -v 4 <apk>` on the extracted universal APK and `llvm-readelf -l`. Add this as a CI step.
- [ ] Audit every plugin with native libs (Steamworks is not shipped on Android; Play Games plugin if adopted). Each must be rebuilt for 16 KB or upgraded.
- [ ] Test on a 16 KB Android 15+ emulator image and a real 16 KB device if available.
- [ ] Hard deadline: 2027-02-01 for updates (V). Do it from the first build, not at the end. Note forum reports of 6000.3.0f1 builds flagged by Play; if the first AAB upload shows "Does not support 16 KB", escalate to `unity-specialist` and check the Unity patch release notes.

## 4. Play App Signing and upload key (owner and CI)

- [ ] Enroll in **Play App Signing** (default for new apps; AAB required). Google holds the app signing key; you hold only the **upload key**.
- [ ] Owner generates the upload keystore **on their own machine** (`keytool -genkeypair -v -keystore upload.keystore -alias <alias> -keyalg RSA -keysize 2048 -validity 10000`). Never generated or pasted in chat or repo.
- [ ] Back up the keystore and passwords in the owner's password manager plus an offline copy. Losing the upload key is recoverable (upload key reset via Play support) but slow.
- [ ] `.gitignore` must contain `*.keystore`, `*.jks`, `*.p12`, `*.pem`, `google-play-service-account*.json`, `Library/`, `Logs/`, `UserSettings/` (devops to confirm).
- [ ] Base64-encode the keystore locally (`base64 -w0 upload.keystore`) and store in GitHub Actions secrets:

| Secret name | Content |
|-------------|---------|
| `ANDROID_KEYSTORE_BASE64` | Base64 of the upload keystore |
| `ANDROID_KEYSTORE_PASS` | Keystore password |
| `ANDROID_KEYALIAS_NAME` | Key alias |
| `ANDROID_KEYALIAS_PASS` | Key alias password |

- [ ] Optional later: `GOOGLE_PLAY_SERVICE_ACCOUNT_JSON` for automated upload to internal track only. Production release stays a manual owner action.
- [ ] CI: restrict secrets to the `main`/release workflow, not PRs from forks; mask values; delete the decoded keystore after the build step; use GitHub environment protection (required reviewer) for the release job.
- [ ] GameCI `game-ci/unity-builder` Android inputs: `androidExportType: androidAppBundle`, `androidKeystoreName`, `androidKeystoreBase64`, `androidKeystorePass`, `androidKeyaliasName`, `androidKeyaliasPass`, `androidTargetSdkVersion: AndroidApiLevel36`, `androidVersionCode: ${{ github.run_number }}` (verify input names against the current action version).
- [ ] The first AAB must be uploaded manually in Play Console to register the upload key certificate.

## 5. Manifest and permissions audit

- [ ] Goal: **no permissions** (game is offline). `INTERNET` is only added by Unity in some build configurations (e.g. development builds, or if Unity services/Addressables remote content is used). Decide: if remote Addressables or Play Games Services are not used, strip `INTERNET` and check merged manifest.
- [ ] Inspect merged manifest from the built AAB (`bundletool dump manifest`) in CI and fail the build on unexpected `uses-permission` entries.
- [ ] No `READ_EXTERNAL_STORAGE`, advertising ID (`AD_ID`), location, microphone, camera. Make sure no SDK adds `com.google.android.gms.permission.AD_ID`. If Unity adds it, remove it with a manifest `tools:node="remove"` entry and declare accordingly.
- [ ] Declare `android.hardware.touchscreen` not required (gamepad support), `android.hardware.screen.landscape` required.
- [ ] Save game in `Application.persistentDataPath` (app-private). Auto Backup: decide whether saves should be included in Android Auto Backup (recommended yes, with exclusions for caches) and set `allowBackup` and backup rules explicitly (dataExtractionRules for Android 12+). Mention in the privacy policy.

## 6. Store listing (coordinate with `store-marketing-specialist`)

- [ ] App name (max 30 chars): "BRASSLIGHT" or "BRASSLIGHT: Extraordinary Expeditions" (check length; trademark search pending, see game-concept).
- [ ] Short description (max 80 chars), full description (max 4000 chars). Written in EN and DE (localization).
- [ ] App icon 512 x 512 PNG (32-bit, max 1024 KB).
- [ ] Feature graphic 1024 x 500 (PNG or JPEG).
- [ ] Phone screenshots: at least 2 (up to 8), landscape for this game; 16:9 or 9:16, min 320 px, max 3840 px, max 2:1 ratio. Recommend 1920 x 1080.
- [ ] 7-inch and 10-inch tablet screenshots (strongly recommended, needed for tablet featuring).
- [ ] Optional: promo video (YouTube URL), Chromebook/large-screen screenshots.
- [ ] (V) Re-check exact asset specs in Play Console at the time of upload.
- [ ] Category: Games > Action (or Role Playing / Strategy). Tags: roguelite, survivor-like.
- [ ] Contact: support email (public), website (optional), **privacy policy URL (required)**; page must be public, non-PDF, and reachable. Hosted by `legal-compliance-specialist` plan, must match `data-safety.md`.
- [ ] Public-domain/IP notes: no Verne/Wells character names that conflict with `design/ip-guidelines.md`; no references to other games in the listing (policy: misleading content).
- [ ] Pricing: paid app, base price (D4) and countries. Availability: choose countries; consider excluding none initially.
- [ ] Play Pass: optional, skip for launch.

## 7. App content declarations (Play Console > Policy > App content)

- [ ] **Privacy policy** URL.
- [ ] **Ads**: "No, my app does not contain ads".
- [ ] **App access**: all functionality available without login.
- [ ] **Content rating (IARC)**: complete the questionnaire truthfully. Expected: fantasy/creature and machine violence without blood/gore, no gambling, no user-generated content, no chat, no location sharing, no purchases. Expected outcome: PEGI 7-12, ESRB E10+, USK 6-12 (matches the concept's target). Re-run if content changes.
- [ ] **Target audience and content**: select 13+ or 16+ age groups only (not designed for children, avoids the Families policy and its extra SDK/ads requirements). Avoid child-appealing cartoon marketing claims that could trigger a mismatch review.
- [ ] **Data safety**: use `production/platforms/android/data-safety.md`.
- [ ] **Government apps / financial features / health / news**: No.
- [ ] **Advertising ID**: declare not used.
- [ ] **Data deletion**: not required when no account/data collected (state accordingly).
- [ ] **Photo and video permissions / other declarations**: not applicable.
- [ ] **Encryption/export compliance**: none beyond standard (US export laws acknowledgement at upload).

## 8. Testing and release tracks

Track order: **Internal testing** (CI smoke, up to 100) -> **Closed testing** (12+ testers x 14 days) -> (optional **Open testing**) -> **Production** (staged rollout).

- [ ] Pre-launch report: enable and review on each closed-track build (crashes, ANR, accessibility, security, performance on Firebase Test Lab devices). Fix S1/S2 issues. Note the report may need a login-free path (our game has none).
- [ ] Device catalog: review supported devices; exclude only devices that fail the minimum spec (e.g. < 3 GB RAM or GPUs below the performance floor, 32-bit-only devices). Exclusions are decided after the perf spike on low/mid Android (roadmap 1.2).
- [ ] Android vitals thresholds to monitor (V: check current bad-behavior thresholds in Play Console; historically user-perceived crash rate 1.09% and ANR rate 0.47% overall, and 8% per device). Target well below these.
- [ ] Soak test (`/soak-test`) and device matrix done by `qa-lead`. **No submission without QA sign-off.**
- [ ] Production: first release as **staged rollout** (e.g. 20% -> 50% -> 100%) with the release notes in EN and DE. Owner presses "Send for review"/"Publish". Managed publishing can hold the release until the owner chooses the date, to launch together with Steam and iOS.
- [ ] Review time: allow up to ~7 days, longer for new accounts (V). Submit at least 7-10 days before the launch date.
- [ ] After launch: monitor Android vitals and reviews for 72 h; hotfix pipeline ready (`/hotfix`).

## 9. Policy and compliance quick list

- [ ] Target API 36 build (section 3). Re-check every August.
- [ ] 16 KB compatible (section 3), deadline 2027-02-01.
- [ ] No ads / IAP / analytics SDKs (anti-pillar). No Firebase Crashlytics unless the owner explicitly approves and `data-safety.md` is updated first. Rely on Play vitals / Android crash data for stability.
- [ ] Open-source and third-party notices in-game (credits screen) and in the repo, via `legal-compliance-specialist`.
- [ ] Impressum / trader contact in-store (DSA) and Impressum on the website (German law).
- [ ] Accessibility: touch targets >= 48dp, gamepad support.
- [ ] Payments policy: paid app, no external purchase links.
- [ ] Google Play Billing is not used (no IAP). Premium price is the only transaction.
- [ ] Keep the account in good standing: respond to policy emails within the stated time; watch Policy status page.

## 10. Hand-offs

| Task | Owner | Notes |
|------|-------|-------|
| Create account, pay fee, tax/bank, trader status, sign agreements, press Publish | Owner | This checklist, sections 1, 2, 8 |
| Unity settings, 16 KB audit, manifest | mobile-platform-programmer, unity-specialist | Section 3, 5 |
| CI Android job, secrets wiring, alignment and manifest checks | devops-engineer | Section 3, 4 |
| Listing copy and assets | store-marketing-specialist | Section 6 |
| Privacy policy, Impressum, DSA wording, VAT questions | legal-compliance-specialist (+ Steuerberater) | Sections 1, 7 |
| QA sign-off, device matrix, pre-launch report triage | qa-lead | Section 8 |
| Go/No-Go | release-manager | `/launch-checklist` |

## 11. Open questions

1. Personal or Organisation account (D1)? This drives the 12-tester / 14-day closed test.
2. Public contact address for the DSA trader declaration (D2)?
3. Package name owner handle (D3)?
4. Does 1.0 need any network feature (remote content, Play Games cloud save)? If no, ship without `INTERNET`.
5. Which Unity 6.3 patch version is pinned, and does its release notes confirm 16 KB and API 36 support?
