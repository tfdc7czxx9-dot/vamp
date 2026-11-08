# Compliance Checklist — BRASSLIGHT

> **Not legal or tax advice.** Prepared by `legal-compliance-specialist`. Items marked **[LAWYER]** or **[STEUERBERATER]** need professional review. Account-level actions (fees, tax forms, agreements, Submit/Publish) are done by the owner only.
> **Date verified: 2026-11-08** by web search. Verification quality is noted per item: "V" = confirmed by at least one search source this session (mostly secondary sources), "NV" = not verified this session, check the official page before acting. Re-verify everything before submission; store rules change often.

## 0. Global facts this checklist assumes

Premium, offline, no ads, no IAP, no analytics, no accounts, no crash-reporting SDK. Local save plus optional platform cloud saves. Any change to these invalidates sections 2, 3 and the privacy policy.

Pre-submission technical proof (owner + `security-engineer`):
- [ ] SDK/package audit: list every package in `Packages/manifest.json` and every plugin under `Assets/Plugins`; none may collect data. Disable/strip Unity Analytics, Unity Ads, Unity IAP, Cloud Diagnostics, Unity Gaming Services packages if present.
- [ ] Network capture of a release build on each platform shows no traffic except platform services.
- [ ] No advertising ID usage (Android `AD_ID` permission removed from merged manifest; check `AndroidManifest` in the AAB).

## 1. Privacy policy URL (all stores)

- [ ] Replace placeholders in `production/legal/privacy-policy.md` ([NAME], [ADDRESS], [EMAIL], authority) and get a review **[LAWYER]**
- [ ] Publish EN and DE at a stable public URL (HTTPS, no login, not a PDF-only). Same URL pattern for all stores
- [ ] Enter URL: Steam (store page / Steamworks app admin), Google Play (App content > Privacy policy), App Store Connect (App Information > Privacy Policy URL)
- [ ] Google Play: a privacy policy is required to complete the Data safety form and have its info shown (V: Play Console help 10787469)
- [ ] Link the Impressum (`impressum-template.md`) from the same site

## 2. Google Play

### 2.1 Data safety (V: https://support.google.com/googleplay/android-developer/answer/10787469, form is reviewed with the app; Google may flag undisclosed collection by SDKs)

Answers (expected):

| Question | Answer | Rationale |
|----------|--------|-----------|
| Does your app collect or share any of the required user data types? | **No** | No data leaves the device to us or third parties by our code or SDKs |
| Is all user data encrypted in transit? | n/a (shown only if data collected) | |
| Do you provide a way to request deletion? | n/a | |
| Data collected / shared lists | Empty ("No data collected", "No data shared") | |

Caveats to settle before ticking "No":
- Google's definition of "collect" excludes data processed only on-device and data sent to Google-provided services acting on the user's behalf? **NV: confirm in the help page** whether Google Play Games saved games / sign-in need a declaration. If the Play Games plugin is used, check the plugin's own data safety guidance; the form may require declaring data handled by it (for example user IDs/game activity). Decide after the plugin is integrated. Fallback answer if required: declare "App activity / Other user-generated content" or "Device or other IDs", collected: no for developer, but follow Google's guidance
- Local save is not "collection"
- Unity engine itself: audit the final AAB with Android vitals / pre-launch report for flagged SDKs

### 2.2 Other Play Console declarations
- [ ] App content: privacy policy, Ads = **No ads**, App access (all functionality available, no login), Target audience (select 13+ or "not primarily for children"; do not opt into Designed for Families without a separate review **[LAWYER]**), Government app = no, Financial features = none, Health = none
- [ ] Content rating via IARC (see section 4)
- [ ] Target API level: **NV, check current requirement** at https://developer.android.com/google/play/requirements/target-sdk before upload; handled by `android-publishing-specialist`
- [ ] Payments profile / tax: see section 7

### 2.3 DSA trader status (see section 6)

## 3. Apple App Store

### 3.1 App Privacy label (App Store Connect > App Privacy)
- Answer: **"Data Not Collected"** (rationale: no data transmitted off device by us or third-party SDKs; Game Center / iCloud data is handled by Apple and not collected by the developer). **NV: confirm Apple's wording for Game Center** (Apple's definition: data is "collected" only if transmitted off device in a way accessible to you or your third-party partners).
- [ ] Privacy Policy URL set (required)

### 3.2 Privacy manifest (PrivacyInfo.xcprivacy)
- V (Unity docs, https://docs.unity3d.com/Manual/apple-privacy-manifest-policy.html): Unity Editor versions from 2021.3.35f1 / 2022.3.18f1 / 2023.2.7f1 onward automatically include required-reason declarations for engine APIs (e.g. file timestamp, user defaults for PlayerPrefs). Unity 6.3 should therefore include them, but **the developer is responsible for checking accuracy** and for third-party plugins.
- [ ] Inspect generated Xcode project for `PrivacyInfo.xcprivacy`; `NSPrivacyTracking` = false; `NSPrivacyTrackingDomains` empty; `NSPrivacyCollectedDataTypes` empty
- [ ] Every plugin/xcframework (Apple Unity Plug-ins/GameKit, any native lib) ships its own manifest or you add declarations; fix App Store Connect "ITMS-91053"-style warnings if received (**NV: code number, check email text**)
- [ ] No App Tracking Transparency prompt, no `NSUserTrackingUsageDescription`
- [ ] Export compliance: app uses only exempt/standard OS encryption → `ITSAppUsesNonExemptEncryption = NO` **[LAWYER if unsure]**

### 3.3 Age rating
- V: Apple replaced its tiers in July 2025 with 4+, 9+, 13+, 16+, 18+ and a new questionnaire; developers had to answer it by 31 January 2026 (secondary sources, e.g. https://www.iphoneincanada.ca/2025/07/25/updated-app-store-age-ratings). **NV: exact current question wording; read in App Store Connect.**
- Draft answers: Cartoon/Fantasy violence = **Infrequent/Mild** (or "Frequent/Intense" is NOT expected); Realistic violence = None; Prolonged graphic/sadistic realistic violence = None; Horror/fear themes = None to Infrequent (giant creatures, Martian machines; tone adventure); Profanity = None; Sexual content/nudity = None; Alcohol/tobacco/drugs = None; Medical/wellness = None; Gambling and simulated gambling = None; Contests = None; Unrestricted web access = No; User-generated content = No; Messaging/chat = No; Parental controls / age assurance capabilities = No; Loot boxes = No
- Expected result: **4+ or 9+**. Compare with IARC/USK and adjust if "Frequent" fantasy violence triggers 9+. Not a problem commercially.

### 3.4 Other
- [ ] Standard Apple EULA (default) unless custom needed; see section 8
- [ ] App Review notes: "Offline game; no login. Game Center optional."
- [ ] DSA trader status (section 6)

## 4. Age ratings: IARC (Google Play), USK/PEGI, Steam

### 4.1 IARC questionnaire (draft answers; **NV: IARC wording changes, read live**) 
| Topic | Draft answer |
|-------|--------------|
| Violence | Yes: fantasy/cartoon-style violence against fictional creatures and machines; no human victims; no blood; no gore; no dismemberment; no realistic depiction; no sadistic/torture |
| Blood | No |
| Fear / horror | Mild: large monsters and alien machines, no jump-scare horror |
| Sex / nudity | No |
| Language | No profanity |
| Controlled substances | No |
| Gambling | No (no loot boxes, no simulated gambling) |
| User interaction / UGC / chat | None |
| Shares user location | No |
| Digital purchases | No (premium; the price itself is the purchase) |
| Unrestricted internet | No |
- Expected: **PEGI 7** (fantasy violence, non-realistic), **USK 6 (possibly 12)**, ESRB E10+, IARC Generic 7+ (outcomes are computed by IARC; not guaranteed).
- Keep the game's tone: no body horror (Moreau world is post-launch; re-run ratings when adding it). Update questionnaires per DLC/world.
- [ ] Do not mention rating in marketing before official certificates are issued
- [ ] If a physical or direct USK certificate is needed for other channels, note: digital stores use IARC; USK direct is optional.

### 4.2 Steam content survey (Steamworks > Store Page > Content Survey; **NV: re-read form**)
- Violence frequency/severity: mild-moderate, fantasy; no gore; no nudity; no sexual content
- [ ] AI-generated content disclosure: answer truthfully (see `THIRD_PARTY_NOTICES.md` section 7: currently none)

## 5. EU consumer / misc store obligations
- [ ] Price display, withdrawal right: platforms handle consumer terms as merchant of record **(NV for each store)**
- [ ] Accessibility Act (EAA, in force 28 June 2025): applies to certain consumer e-commerce services; pure game software is largely out of scope, but the store/website selling may be. Microenterprises are exempt for services. **NV: confirm with [LAWYER]**
- [ ] Mention source credits (Verne and Wells) in Credits screen

## 6. EU DSA trader status

**What it is:** Under the EU Digital Services Act, stores must verify and show contact details of "traders" to EU consumers. Selling a paid game for money makes you a trader in practice, even as a single developer (V: Apple: developers who earn money through the App Store are treated as traders; Google guide says details become public for EU users). Declaring "non-trader" while selling commercially is risky **[LAWYER]**.

**What gets published (V, secondary sources):**
- Apple App Store: your address, phone number and email are shown on the EU product page. Verification of phone/email is required. Anecdotal 2026 forum reports (https://developer.apple.com/forums/thread/847098) show verification "In Review" for weeks, blocking EU availability: start early, before the first App Review submission.
- Google Play: Account details > declare status; the contact details on the Contact tab become public to EU users; address verification may be requested; a P.O. box may fail (V: third-party guide fdaytalk.com; the stated February 2027 deadline is **NV** and must be checked at support.google.com/googleplay).
- Steam: Valve collects trader info in Steamworks "Store Presence / Trader Information" and displays it on EU store pages **NV: check Steamworks Documentation for EU DSA**.

**Options for the public address:**
| Option | Pros | Cons |
|--------|------|------|
| A. Home address | Free, simplest | Home address public worldwide-ish for EU listing; privacy/safety risk |
| B. Business address (Gewerbe in a rented office/co-working) | Real address, professional | Cost; must be where you can receive mail |
| C. c/o / virtual office / registered "Impressum service" address with mail forwarding | Keeps home private, usually 10-40 EUR/month | Stores may reject some virtual addresses; Impressum needs a "ladungsfähige Anschrift" and the provider must really receive/forward mail. **[LAWYER]** |
| D. Dedicated phone number (VoIP/mobile with voicemail) and role email (support@yourdomain) | Avoids private phone/email in public | Apple verifies via code to the phone: must be able to receive SMS/call |
| E. Form a UG/GmbH and use company data | Liability separation and address of company | Cost and admin; question for **[STEUERBERATER]** |
**Recommendation:** Option C or B for the address, D for phone and role-based email. Use the same address in the Impressum, so the public identity is consistent. Decide before creating developer accounts.

## 7. Tax and payouts (owner-only actions; all NV unless stated; confirm with [STEUERBERATER])

### 7.1 Steam (Steamworks)
- [ ] Complete Steamworks onboarding: identity/bank (owner), W-8BEN (individual) or W-8BEN-E (entity) in the Steamworks tax interview. German tax ID (Steuer-ID/ Steuernummer) will be requested. Claim US tax treaty benefits (Germany-US treaty; royalty withholding is commonly reduced to 0%) **NV: check the Steam tax interview guidance** 
- [ ] Steam is the seller of record and handles VAT/sales tax collection; you receive revenue share. Confirm with Steuerberater how to report (Umsatzsteuer: place of supply, reverse charge, ZM)
- [ ] Steam app fee (one-time per app, currently USD 100 recoupable at USD 1,000 revenue; **NV: verify current amount**)

### 7.2 Apple
- [ ] Apple Developer Program (annual fee, **NV: current price**) as individual or organization; D-U-N-S only needed for organizations
- [ ] Agreements, Tax and Banking: Paid Apps Agreement; US tax forms (W-8BEN for non-US individuals; Apple asks for a US tax form even for non-US developers **NV**), plus German VAT info. Apple is merchant of record in most regions (and acts as "commissionaire" in some, e.g. certain countries where you charge VAT yourself: **NV: check "Tax" table in App Store Connect**)
- [ ] Apple Small Business Program (15% commission under USD 1M/yr proceeds): apply yourself **NV: verify eligibility**

### 7.3 Google Play
- [ ] Google Play Console registration fee (one-time USD 25, **NV**). New personal accounts may need closed testing with testers before production access **NV: check current requirement**; plan for it in the schedule (`android-publishing-specialist`)
- [ ] Payments profile: tax info (US W-8BEN or form asked in "Payments profile" > "Tax info"), EU VAT: Google generally is merchant of record for app sales; **NV**
- [ ] Google service fee reduced tier (15% for first USD 1M) **NV**

### 7.4 Kleinunternehmer / VAT questions for the Steuerberater
1. Is § 19 UStG Kleinunternehmerregelung usable when payouts come from Valve/Apple/Google (foreign entities; reverse charge; "Leistungsort")? Does it hurt to be without a USt-IdNr. when stores ask for a VAT ID?
2. Do you need a USt-IdNr. (for platform onboarding, EU B2B Reverse Charge)? Is an EU "Zusammenfassende Meldung" needed?
3. Gewerbeanmeldung vs. freiberuflich; Gewerbesteuer, Einkommensteuer, Vorauszahlungen, Buchführung (EÜR), Umsatzsteuervoranmeldung
4. Handling of US withholding tax and treaty benefits (Steuerbescheinigung, Anrechnung)
5. Effect of the Wachstumschancengesetz e-invoice and other 2025/2026 changes on reporting
6. Business form (Einzelunternehmen vs. UG/GmbH) and liability
7. Home office, software and hardware deductions; Unity Pro cost if needed

## 8. EULA / terms
- Steam: Steam Subscriber Agreement applies by default; custom EULA optional and not needed for this game. Recommendation: **none**; keeps things simple.
- Apple: standard Apple EULA by default; custom EULA optional. Recommendation: default.
- Google Play: no EULA field required.
- [ ] If a custom EULA is ever used, review under German AGB law (§§ 305 ff. BGB) **[LAWYER]** (German consumers: many typical US EULA clauses are invalid)

## 9. Licensing / third-party audit
- [ ] Maintain `THIRD_PARTY_NOTICES.md` (skeleton exists); every new dependency → ADR + license check
- [ ] Ship licenses in-game Credits screen
- [ ] AI-generated assets: none planned; any addition → legal review and disclosure
- [ ] Music/sfx/fonts: written license or contract on file in a private location (not in the public repo if it contains personal data)

### 9.1 Unity license tier check
- V (2026-11-08, https://unity.com/blog/news/plan-pricing-and-packaging-updates and secondary reports): Runtime Fee was cancelled; for Unity 6, **Unity Personal** is free up to **USD 200,000** in annual revenue and funding (trailing 12 months); above that, **Unity Pro** (about USD 2,200 per seat per year, **NV: current price**) is required. Splash screen is optional in Unity 6 for Personal.
- [ ] Check the numbers on https://unity.com/pricing and the Unity Terms of Service at each release
- [ ] Revenue includes all gross revenue and funding of the entity (not just this game). Plan a tier check at 150k USD trailing revenue **[STEUERBERATER]**
- [ ] Verify that Unity 6.3 LTS is covered by the current licensing page; ADR-0001 should record the tier

## 10. Public domain (IP) and trademark

### 10.1 IP
- Follow `design/ip-guidelines.md`. Re-verify PD status per territory before launch **[LAWYER]**. Never reuse Disney Nautilus, 2005 tripods, etc. Credit line "Inspired by the novels of Jules Verne and H. G. Wells."

### 10.2 Trademark search for "BRASSLIGHT" (do before announcing, before Steam "Coming Soon" page)
Preliminary web search by the owner found nothing (game concept v0.2). A formal search was not performed here. Steps:
1. **EUIPO eSearch plus**: https://euipo.europa.eu/eSearch/ — search "BRASSLIGHT", variants "BRASS LIGHT", "BRASSLITE", phonetic variants; filter classes below. Use "similar" mode
2. **DPMA register** (DPMAregister): https://register.dpma.de/ — word mark search incl. similar marks
3. **USPTO Trademark Search**: https://tmsearch.uspto.gov/ — "BRASSLIGHT" and variants; look at live and dead
4. **WIPO Global Brand Database**: https://branddb.wipo.int/ — international registrations (Madrid), UK, JP, CN, KR, etc.; also UKIPO search
5. **Nice classes**: **9** (downloadable game software), **41** (entertainment, online/non-downloadable games, esports); consider class 28 (toys, merchandise) and 25 (apparel) if merchandise is planned; class 35 only if relevant
6. Also search: Steam store, Google Play, App Store, itch.io, Google, domain (brasslight.com/.de/.games), social handles, Wikipedia; "Brasslight" as a company name (Handelsregister, Companies House, OpenCorporates) and in other industries (lighting products named Brasslight in class 11/21 are not game-relevant but note them)
7. Record results (date, database, query, hits, risk) in a table in this file and decide: proceed / adjust title / seek **[LAWYER]** clearance
8. Optional: file own trademark (EUIPO word mark, classes 9 and 41; fee about EUR 850 for one class base **NV**) after clearance **[LAWYER]**. Priority: title first, logo later
9. Check main-character names the same way
10. Do not use "HD-2D" or competitor names in metadata (see ip-guidelines)

Search log:
| Date | Database | Query | Result | Risk |
|------|----------|-------|--------|------|
| | | | | |

## 11. Release gate summary (blocking before first store submission)
- [ ] Privacy policy published EN+DE, reviewed
- [ ] Impressum live
- [ ] Data safety / App Privacy / Steam survey filled consistently
- [ ] Privacy manifest verified in Xcode project
- [ ] Trader status verified on Apple (can take weeks), Google, Steam
- [ ] Tax forms accepted on all three stores
- [ ] Trademark search logged
- [ ] Third-party notices complete and in-game
- [ ] Unity tier recorded
- [ ] Steuerberater consulted (Kleinunternehmer, VAT, Gewerbe)
