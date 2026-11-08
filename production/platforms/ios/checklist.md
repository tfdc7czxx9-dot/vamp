# iOS / iPadOS Publishing Checklist: BRASSLIGHT

> Owner: `ios-publishing-specialist` (reports to `release-manager`).
> Status: draft v0.1, written 2026-11-08. The Unity project (`src/Brasslight/`) does not exist yet, so Unity settings below are targets, not observed values.
> Rule: Apple requirements change. Items tagged **(V)** were verified on **2026-11-08** against the source given; re-verify before every submission. Items tagged **(U)** are unverified and must be checked.
> Account-level actions (paying, tax/bank, agreements, "Submit for Review", "Release") are done by the owner only. No keys, certificates or passwords in the repo or in chat.

## 0. Verified facts (2026-11-08)

| Topic | Fact | Source |
|-------|------|--------|
| SDK minimum for uploads | Uploads to App Store Connect (incl. TestFlight) must be built with **Xcode 26 or later** using the **iOS 26 / iPadOS 26 SDK**. Effective since 2026-04-28. (V) | https://developer.apple.com/news/upcoming-requirements/ (Apple, fetched directly; the 2026-04-28 date comes from secondary sources) |
| Deployment target | iOS/iPadOS apps uploaded must **target iOS 13 or later**. (V, Apple page; exact effective date 2026-09-09 from secondary sources) | same |
| Required-reason APIs | Approved reasons must be declared for listed APIs used by app code and third-party SDKs, or the upload is rejected. (V) | same |
| Program fee | **$99 USD per year** (V). Same for individual and organization per secondary sources (U). | https://developer.apple.com/programs/ |
| Age rating tiers | **4+, 9+, 13+, 16+, 18+** (12+ and 17+ removed). (V) | https://developer.apple.com/help/app-store-connect/reference/app-information/age-ratings-values-and-definitions/ ; announcement https://developer.apple.com/news/?id=ks775ehf |
| DSA trader status | Every developer must declare trader / non-trader status when submitting, even if not distributing in the EU. Trader contact data (address, phone, email) is verified and published on the EU product page. (V) | https://developer.apple.com/help/app-store-connect/manage-compliance-information/manage-european-union-digital-services-act-trader-requirements/ |

Not verifiable in this session (host blocked by proxy): Unity docs pages. See `privacy.md`.

## 1. Apple Developer Program (owner)

Decision: **Individual vs. Organization**

| | Individual | Organization |
|---|-----------|--------------|
| Seller name on App Store | Owner's legal personal name | Company legal name |
| Requirements | Apple ID with 2FA, legal age, legal name and address | D-U-N-S number matching legal entity, public website, authority to bind the company (U, secondary sources) |
| Lead time | Short | Days to ~2 weeks (U) |
| Fee | $99/yr | $99/yr |
| Liability / privacy | Personal name shown; if DSA trader, personal address and phone are published | Company data shown |

Recommendation: enrol as **Individual** now (owner is an individual in Germany, no company yet). Revisit if a GbR/UG/GmbH is founded; Apple allows moving an app between accounts via app transfer (U, confirm before relying on it). Ask the Steuerberater first whether game sales count as a trade (Gewerbe), because this decides DSA trader status and tax handling.

Checklist
- [ ] Apple ID with two-factor authentication, name matches ID document
- [ ] Enrol at https://developer.apple.com/programs/enroll/ (owner), pay annual fee
- [ ] Put annual renewal date in the production calendar (lapse removes the app from sale)
- [ ] Decide on a support email address that is not the private mailbox

## 2. App Store Connect business setup (owner)

- [ ] Agreements, Tax, and Banking: accept **Paid Apps Agreement** (required even for a one-time paid price)
- [ ] Bank account (IBAN) and tax forms; German VAT/USt-ID details as advised by Steuerberater
- [ ] **DSA trader status** (Business > Compliance): owner and legal-compliance-specialist decide; if "trader", address, phone, email are published on EU storefronts. Options: (a) declare trader and use a business address / Postfach if legally acceptable (U, check with lawyer), (b) non-trader only if genuinely not acting commercially, which is doubtful for a paid game. Recommendation: assume **trader**.
- [ ] Impressum / privacy policy URL ready (legal-compliance-specialist) before the app record is completed
- [ ] Pricing: tier for target EUR 3.99 (store-marketing-specialist validates); all territories decision

## 3. Identifiers

- Bundle ID suggestion: `com.<owner>.brasslight` (reverse-DNS of a domain the owner controls; owner name placeholder to be filled). It cannot be changed after the first upload, so decide before the Unity project is created and use the same value in Player Settings.
- [ ] Register explicit App ID in Certificates, Identifiers & Profiles
- [ ] Capabilities: none required for MVP. Add **Game Center** and/or **iCloud (key-value / CloudKit)** only when `Vamp.Platform.Apple` implements them (each needs an ADR + license check per technical preferences)
- [ ] Create App Store Connect app record: name "BRASSLIGHT" (check availability, name must be unique on the store; trademark search from roadmap 0.7 is still open), subtitle "Extraordinary Expeditions" (30 char limit, U), primary language EN, second DE, SKU, bundle ID

## 4. Build, signing, CI

Approach: Unity exports an Xcode project; archive and upload on macOS (local Mac or GitHub macOS runner) via GameCI `game-ci/unity-builder` (targetPlatform iOS) followed by xcodebuild.

CI secrets (names fixed by owner; values only in GitHub Secrets):

| Secret | Content |
|--------|---------|
| `APPSTORE_ISSUER_ID` | App Store Connect API issuer ID |
| `APPSTORE_KEY_ID` | API key ID |
| `APPSTORE_P8` | contents of the `AuthKey_<id>.p8` (base64 if multi-line is a problem) |

Optional additional secrets only if fastlane match is chosen: `MATCH_PASSWORD`, certificate repo access token. Not needed with the preferred option below.

Signing options

| Option | Pros | Cons |
|--------|------|------|
| A. Automatic signing with API key (`xcodebuild -allowProvisioningUpdates -authenticationKeyPath/-KeyID/-IssuerID`) | No cert files to manage; only the three secrets | Cloud-managed certs; first run needs team ID; key needs App Manager (or Admin) role |
| B. fastlane match | Reproducible, explicit profiles | Extra private repo + passphrase to protect |

Recommendation: **A** for a solo project. Team ID is not secret but is set as a repo variable `APPLE_TEAM_ID` (U, suggested name).

- [ ] Owner creates the API key (App Store Connect > Users and Access > Integrations > App Store Connect API), role App Manager, downloads the .p8 once, stores it in a password manager and GitHub Secrets
- [ ] Workflow (with devops-engineer): macOS runner, Xcode 26.x selected explicitly (`xcode-select`), archive, export for App Store Connect, upload via `xcodebuild -exportArchive` with upload destination or fastlane `pilot` using the API key (U, confirm exact export options for Xcode 26)
- [ ] Build number auto-increments per upload (CFBundleVersion), marketing version from git tag
- [ ] Verify the macOS runner image actually ships Xcode 26 before the first upload (check runner image list at that time)
- [ ] Unity 6.3 iOS build support module compatible with Xcode 26: confirm in Unity release notes (U)

## 5. Unity iOS Player Settings (target values)

| Setting | Value |
|---------|-------|
| Company / Product Name, Bundle Identifier | owner / BRASSLIGHT, `com.<owner>.brasslight` |
| Target Device | iPhone + iPad |
| Target minimum iOS version | Unity 6.3 default minimum (U, read from Player Settings) and never below iOS 13 (V); technical preferences say "current Unity 6.3 minimum" |
| Architecture | ARM64 only |
| Graphics API | Metal only (remove OpenGLES) |
| Default Orientation | Auto Rotation with only Landscape Left and Landscape Right allowed |
| Requires Fullscreen (iPad) | On, because landscape only with no Split View/Slide Over support (U, validate on iPad; if off, Apple expects all orientations) |
| Hide Home Button / Defer system gestures | Defer on bottom edge so joystick swipes do not trigger home |
| Status bar | Hidden |
| Safe area | Use `Screen.safeArea`; test iPhone with notch/Dynamic Island and iPad |
| Target frame rate | 60 (ProMotion devices up to 120 via `maxFramePerSecond` in Info.plist only if needed; frame pacing test) |
| Scripting backend / stripping | IL2CPP, Managed stripping Medium or High, check Addressables/link.xml |
| Camera/Microphone/Photo usage strings | Leave empty; do not add permissions the game never uses (App Review rejects unused ones) |
| `ITSAppUsesNonExemptEncryption` | `NO` via post-process build script (see section 6) |
| Game controllers | Add `GCSupportsControllerUserInteraction`/controller support; controller optional, touch fully playable |
| App icon | 1024x1024 no alpha, no rounded corners; Unity 6 icon sets |
| Launch screen | Storyboard or Unity splash; no "Made with Unity" claim problems (Unity license tier) |
| Build size | Initial download target <= 200 MB (technical preferences) |
| Background behaviour | Auto-pause on `OnApplicationPause`/focus loss |

- [ ] Post-process script (editor, unity-specialist/mobile-platform-programmer) sets Info.plist keys and verifies `PrivacyInfo.xcprivacy` is in the Xcode target (see `privacy.md`)

## 6. Export compliance

- Game uses only standard OS HTTPS or no networking, no proprietary crypto: set `ITSAppUsesNonExemptEncryption = NO` in Info.plist. This avoids the per-build encryption question in TestFlight/App Store Connect.
- Caveat: if a plugin adds its own non-exempt encryption, re-assess. France distribution historically required documentation only for non-exempt encryption (U). Owner confirms the answer with legal-compliance-specialist.
- [ ] Info.plist key present in the exported project
- [ ] Re-check whenever a networking or save-sync package is added

## 7. TestFlight

- [ ] First build uploaded from CI, processing completes, no missing-manifest or required-reason emails
- [ ] **Internal testing**: up to 100 internal members with App Store Connect roles (U, verify limit); no Beta App Review
- [ ] **External testing**: needs Beta App Review for the first build of each version; provide test notes, contact, no login needed (game has no account)
- [ ] Builds expire after 90 days (U)
- [ ] Device matrix from `qa-lead`: oldest supported iPhone, current iPhone with Dynamic Island, small iPhone, iPad (A-series), iPad Pro (ProMotion), thermals/30 min session, memory under 1.0 GB, background/resume, interruptions (call, Control Centre), airplane mode, low storage
- [ ] Controller test: MFi/Xbox/PlayStation controller connect/disconnect mid-run
- [ ] QA sign-off recorded before any production submission (hard rule)

## 8. App Store listing assets (with store-marketing-specialist)

- [ ] Name (30 chars), subtitle (30), promotional text (170), description (4000), keywords (100 chars total, comma separated, no spaces after commas, no competitor names), What's New, support URL, marketing URL, privacy policy URL
- [ ] Screenshots: Apple's required sizes change; check App Store Connect > Screenshot specifications at upload time (U). Plan landscape 6.9" iPhone set and 13" iPad set; at least 3 and up to 10 each. Real in-game footage only, no placeholder art
- [ ] Optional App Preview video (15-30 s) per device class
- [ ] No mentions of Steam, Android, Google Play or other platforms in metadata or screenshots
- [ ] Localization: EN and DE metadata
- [ ] Copyright line, category Games > Action (primary), Arcade or Role Playing (secondary; decide), content rights: confirm no third-party content beyond public-domain sources (see `design/ip-guidelines.md`)
- [ ] Price tier set, availability territories, pre-order off (decision with owner)

## 9. Age rating

Apple's current tiers are 4+, 9+, 13+, 16+, 18+ (V). The questionnaire is answered in App Store Connect > App Information. Apple's own definitions (V): infrequent cartoon or fantasy violence leads to **9+**, **frequent** cartoon or fantasy violence leads to **13+**.

Risk: the concept's target (PEGI 7-12 / USK 6-12) maps to Apple 9+, but a survivor-like is continuous fantasy combat, so an honest "frequent" answer yields **13+**. Do not under-answer to get 9+: Apple can reject or re-rate. Plan for 13+ in store copy and revisit after reviewing the questionnaire with the real build.

- [ ] Answer all questions truthfully: violence (fantasy, no gore, no realistic violence), no horror beyond "fear themes" (assess monsters), no gambling/loot boxes, no UGC/chat, no web access, no parental controls
- [ ] Check regional questions (Australia, Brazil, Korea) shown in the questionnaire (V: these exist)
- [ ] Confirm the questionnaire version/deadline status in App Store Connect; secondary sources say new-question answers became mandatory for submissions in 2026 (U)
- [ ] Record the final rating here: ______

## 10. App Review risks and mitigations

| Guideline area | Risk for BRASSLIGHT | Mitigation |
|----------------|--------------------|------------|
| 2.1 App completeness | Placeholder art/text, crashes, broken links | Final content only; test on release build; working support and privacy URLs |
| 2.3 Accurate metadata | Screenshots not from the game; ratings mismatch; mention of other platforms | Real captures; truthful age answers; remove "Steam"/"Android" |
| 4.0 Design / 4.2 minimum functionality | Low risk for a full game | n/a |
| 5.1 Privacy | Manifest/label mismatch | `privacy.md`; no SDKs that collect data |
| Controller | Must not require a controller | Touch fully supported |
| 3.1 Payments | Premium paid, no IAP | Price is set in App Store Connect; no external purchase links or "buy on Steam" text |
| 1.x Safety (violence) | Fantasy combat | Rating honest; no gore |
| IP | Third-party trademarks | Only public-domain Verne/Wells material; see `design/ip-guidelines.md`; character and vehicle names that are also modern trademarks need a check by legal-compliance-specialist |
| Review notes | No login, no demo account needed | Explain controls, 60-second start, recruiting/building tutorial; no hidden features |

- [ ] Re-read the current App Review Guidelines (https://developer.apple.com/app-store/review/guidelines/) before submission and record the date: ______

## 11. Submission and release (owner)

- [ ] All of: QA sign-off, release-manager go/no-go, legal sign-off (privacy policy, Impressum, EULA choice: Apple standard EULA or custom)
- [ ] App Privacy answers published (`privacy.md`)
- [ ] Select build, fill review notes and contact info, export compliance, content rights, advertising identifier: **No** (no IDFA)
- [ ] Release option: choose **Manually release** so the owner can sync with Steam/Google Play
- [ ] Owner presses **Submit for Review**; no review time promised (typical durations vary; do not plan launch dates around them)
- [ ] After approval: owner presses **Release**, then 72 h monitoring (crash reports from Xcode Organizer / App Store Connect, no third-party crash SDK)
- [ ] Post-launch: yearly renewal, SDK minimum re-check each update, privacy label review per update

## 12. Open items

1. Owner name/domain for the bundle ID.
2. Individual vs. organization decision and trader status (Steuerberater + lawyer).
3. Unity project creation (roadmap 0.4); confirm Unity 6.3 iOS minimum and Xcode 26 compatibility.
4. Mac or macOS CI budget.
5. Re-verify (U) items listed above.
