# Steam Checklist: BRASSLIGHT (codename VAMP)

> Owner: `steam-publishing-specialist`. Reports to `release-manager`.
> **Rules verified: 2026-11-08** via WebSearch of Valve Steamworks pages
> (the sandbox proxy blocked direct fetches of partner.steamgames.com, so
> wording below comes from search excerpts of official pages).
> Items tagged **(verify live)** could not be confirmed word-for-word and must
> be re-read on the linked Steamworks page by the owner before acting.
> Re-verify everything again at the start of Phase 5 (see `production/roadmap-to-launch.md`).
>
> Roles: **[OWNER]** = account-level action only the owner may do (payments,
> tax/bank, signing, "Publish"/"Submit"). **[AGENT]** = prepared or done by agents.
> No credentials in the repo: Steam partner logins, builder password and
> `config.vdf` live only in the CI secret store / owner's machine.

Official sources:
- Steam Direct: https://partner.steamgames.com/steamdirect
- App fee: https://partner.steamgames.com/doc/gettingstarted/appfee
- Store releasing: https://partner.steamgames.com/doc/store/releasing
- Coming Soon: https://partner.steamgames.com/doc/store/coming_soon
- Review process: https://partner.steamgames.com/doc/store/review_process
- Steam Deck compat: https://partner.steamgames.com/doc/steamdeck/compat
- Next Fest 2027 editions: https://partner.steamgames.com/doc/marketing/upcoming_events/nextfest/feb_2027
- SteamPipe upload: https://partner.steamgames.com/doc/sdk/uploading

---

## A. Timeline rules at a glance (verified 2026-11-08)

| Rule | Value | Source / status |
|------|-------|-----------------|
| Steam Direct fee | **USD 100 per app** (or local equivalent), paid by a Steamworks account with Admin permission; non-refundable but **recoupable** once the app has >= USD 1,000 adjusted gross revenue | App fee doc, verified |
| Wait between paying the fee and release | **30 days** | Steam Direct page excerpt; not in the appfee doc. **(verify live)** |
| Paperwork (bank, tax, identity) | Required before app can ship; Valve page quotes 1-5 days to process (older versions said 15-30 days). Start early | Steam Direct page, **(verify live)** |
| Coming Soon page | Must be publicly live **>= 2 weeks before release** for a new product | Coming Soon doc, verified |
| Store page review | Typically 3-5 business days; **submit >= 7 days** before wanted live date | Review process doc, verified |
| Build review | Typically 3-5 business days; **plan >= 7 business days**. Store page must be submitted before the build | Review process doc, verified |
| Real-world delays | Trade press reported pre-release reviews up to 12 business days; build rejections restart the clock | Third party, advisory |
| Release date lock | Once Coming Soon is live and release is within 14 days, date cannot be changed without contacting Valve | Search excerpt **(verify live)** |
| Next Fest | A game can appear in Next Fest **once**; a playable demo must be public when the fest starts. Next dates on the official pages: Feb 2027 edition (registration deadline 10 Jan 2027, 11:59pm PST), then June 2027 | Next Fest pages, verified for Feb 2027 deadline |

**Planning rule of thumb for launch date D:** pay fee and finish paperwork
>= D-75 days; Coming Soon page live (and wishlisting) >= D-6 months ideally (roadmap
says end of Phase 3); store page approved >= D-45; build submitted for review
>= D-30 (first-pass rejection buffer included); date announced >= D-21.

---

## B. Phase 0: Account and legal setup [OWNER, agent prepares]

Dependencies: legal-compliance-specialist (entity, tax questions for the Steuerberater, roadmap task 0.7).

- [ ] Decide the legal entity that signs with Valve (individual vs. company). Ask the Steuerberater first; it affects tax forms and bank account.
- [ ] Create a Steamworks partner account at https://partner.steamgames.com/ using a dedicated owner email with 2FA (Steam Guard Mobile). Never share this login.
- [ ] Sign the Steam Distribution Agreement online. [OWNER only]
- [ ] Complete the company/personal information, then the **paperwork** (all in Steamworks "Complete your paperwork"):
  - [ ] Identity verification (government ID; may require a photo/KYC step). Name must match bank and tax data.
  - [ ] Bank account (IBAN/SWIFT for EU) with a name matching the legal entity.
  - [ ] Tax interview (US W-8BEN / W-8BEN-E for non-US persons or entities). Provide the foreign tax ID; correct treaty claims reduce US withholding. Have the Steuerberater review the answers.
  - [ ] EU VAT / DSA trader details if requested (Valve asks for trader status under the EU DSA) **(verify live)**.
- [ ] Pay the **USD 100** Steam Direct fee for the BRASSLIGHT app (Admin account; credit is tied to the paying user). [OWNER only]
- [ ] Record the date the fee was paid in `production/roadmap-to-launch.md` (starts the 30-day counter **(verify live)**).
- [ ] Create the Steamworks groups/users: owner (Admin), builder account (see section K), optional marketing user. Least privilege everywhere.
- [ ] Note the **App ID**. Later: **Demo App ID** (section I) and optionally **Playtest App ID**. Store IDs in `tools/ci/steam/README.md` only as non-secret values via CI variables.
- [ ] Trademark search (EUIPO/DPMA/USPTO) for "BRASSLIGHT" completed before the page goes public (see `design/gdd/game-concept.md`). Block on legal-compliance-specialist.

---

## C. Phase 3: Store page ("Coming Soon") [store-marketing-specialist drafts, OWNER submits]

### C.1 Assets (sizes must be re-checked on the Steamworks "Graphical Assets" page, **verify live**)
- [ ] Capsules: header, small, main, vertical, plus library assets (hero, logo, header); text in capsules must be the logo only, no review quotes/prices/"free"/"new" badges.
- [ ] Screenshots: minimum five, 1920x1080 or larger, real in-game footage (no mock-ups); include one gamepad/Deck-style frame.
- [ ] Trailer(s) with in-game gameplay in the first seconds; no price or release-date text burned in.
- [ ] Page background, icons (app icon, client icon, community icon).
- [ ] Short description, long description, "About this game" with feature GIFs, tags, genre, categories (Single-player, Steam Achievements, Steam Cloud, Full controller support, Steam Deck-friendly flags only if true).
- [ ] System requirements (min / recommended) from `tests/performance` results, including Linux and macOS.
- [ ] Languages: EN, DE, FR interface at launch (per concept). Mark audio/subtitles accurately.
- [ ] Content survey: no gore, creature combat. Target PEGI 7-12 / USK 6-12 (IARC questionnaire in Steamworks).
- [ ] **AI content disclosure** form completed truthfully if any pre-generated or live-generated AI content is used **(verify live)**.
- [ ] Legal: privacy policy URL, EULA/terms if used, Impressum for EU, third-party notices.
- [ ] Public-domain source credit on page is optional; keep IP rules in `design/ip-guidelines.md` (no protected-adaptation elements).
- [ ] Price recommendation prepared (concept target EUR 4.99). **Owner decides.** Review Steam regional pricing suggestions and launch discount rules (a launch discount of up to 40% lasting 7-14 days is the common range **(verify live)**). Do not set price in docs without owner decision.

### C.2 Submit and publish
- [ ] Store page "Release" section: set app type Game, release state "Coming Soon", tentative release date or "To be announced".
- [ ] Submit store presence for review [OWNER]. Allow >= 7 days; fix rejections within the same cycle.
- [ ] After approval, **publish the Coming Soon page** [OWNER]. Wishlisting starts. Calendar the 2-week minimum and the target >= 6 months of visibility.
- [ ] Add wishlist-tracking review into producer weekly cadence.

---

## D. Phase 4: Integration (Unity) [unity-specialist implements, steam-publishing-specialist reviews]

Rules: Steamworks.NET only behind `Vamp.Platform.IPlatformServices`; assembly `Vamp.Platform.Steam` compiled only with `STEAMWORKS` define on Standalone; no `Steamworks.*` calls outside it.

- [ ] Steamworks.NET (MIT) added with ADR + `legal-compliance-specialist` license check (required for any new dependency).
- [ ] `SteamAPI.RestartAppIfNecessary(appId)` at startup; on init failure fall back to `NullPlatformServices`; game still runs offline.
- [ ] `steam_appid.txt` used for local dev only, **excluded from builds and depots** (CI step asserts absence).
- [ ] `SteamAPI.RunCallbacks()` pumped each frame; `SteamAPI.Shutdown()` on quit.
- [ ] Overlay activation (`GameOverlayActivated_t`) pauses the game.
- [ ] Achievements + stats via `IPlatformServices`; IDs mirror `assets/data/achievements.json` (see `production/platforms/steam/achievements.md`). `StoreStats` after unlock and at run end, not every frame.
- [ ] Steam Cloud: decide Auto-Cloud (config in Steamworks, no code) vs. API; recommended Auto-Cloud for the save folder with conflict handling by `lastModified` + run counter and a user prompt on divergence. Define root path, file patterns and quota in Steamworks Cloud settings.
- [ ] Rich Presence (world + "In expedition" localized token).
- [ ] Steam Input: action sets (Menu, Gameplay), default configs, glyph retrieval via Steam Input or Input System glyph map; Big Picture/Deck on-screen keyboard (`ShowFloatingGamepadTextInput`) for any text entry (e.g. profile name).
- [ ] Pause on focus loss (technical preferences) also covers Deck sleep/resume.
- [ ] Tests (EditMode): offline init fallback, achievement unlock queue when offline, cloud conflict resolution (required by technical preferences).
- [ ] No Steam DRM wrapper required; if used, test offline start.

---

## E. Depots and builds (SteamPipe) [devops-engineer + steam-publishing-specialist]

- [ ] Depots: one per OS: Windows, Linux, macOS (+ separate depot per Demo App). IDs recorded as CI variables, not hard-coded secrets.
- [ ] Launch options in Steamworks: Windows exe, Linux executable (flag as Linux), macOS .app (universal/Apple Silicon; notarization recommended so it opens without Gatekeeper warnings).
- [ ] Branches: `internal` (team), `beta` (testers, password protected), `default` (public). Never auto-upload to `default` from CI; owner sets live. Note: reports that `default` cannot be set via the upload action's `setlive`; use `internal`/`beta` and promote by hand.
- [ ] Templates: `tools/ci/steam/*.vdf.template`, flow in `tools/ci/steam/README.md`.
- [ ] Build contents audit: no `steam_appid.txt`, no `*_BurstDebugInformation_DoNotShip`, no `*_BackUpThisFolder_ButDontShipItWithYourGame`, no debug symbols unless intended, no dev scenes.
- [ ] Build size within budget (PC memory ceiling 2 GB, see technical preferences).
- [ ] Clean-machine install test from Steam (not just local run) on Windows, Linux, macOS.

---

## F. Steam Deck compatibility [qa-lead runs the test pass]

Request the review from the app landing page, Technical Tools, "Steam Deck Compatibility Review". Valve reviews against a published checklist and reports point by point; developer may update and request re-review. Typical turnaround quoted as about a week (advisory). Re-read the checklist on the Steamworks page before testing **(verify live)**.

Checklist (the official categories are Input, Display, Seamlessness, System Support; items below from the doc excerpts plus project requirements, **verify live** against the current wording):
- [ ] **Input**: full controller support, all menus and gameplay reachable by gamepad, no mouse-only UI; correct Deck/Steam glyphs; default controller configuration that works without any setup.
- [ ] **Text entry**: on-screen keyboard appears automatically when text is needed (Steamworks API or own in-game keyboard).
- [ ] **Display**: supports 1280x800 (and 1280x720), readable text at that size (project rule: smallest text legible at 800p handheld), good defaults chosen by the game on first launch.
- [ ] **Seamlessness**: no launcher or any launcher fully controller navigable (we ship none); no compatibility warnings on first launch; no crash at sleep/resume; no unexpected EULA dialogs that need a mouse.
- [ ] **System support**: runs through Proton/native Linux without unsupported anti-cheat or middleware; no extra components requiring install.
- [ ] Performance: target 60 fps on Deck, accept 30 fps floor; post-processing <= 3 ms on Medium tier; gameplay sim <= 4 ms with 500 enemies (technical preferences). Default graphics tier on Deck = Medium; detected automatically.
- [ ] Battery/thermal check: 30-minute run, no throttling collapse.
- [ ] Cloud saves sync between PC and Deck.
- [ ] Test on real hardware (Deck LCD and OLED if available), with `beta` branch.
- [ ] Mark Linux build support in store categories accurately.

---

## G. Pre-release gate (D-45 to D-30)

- [ ] QA sign-off (`/team-qa`), soak test, zero S1/S2.
- [ ] Release candidate uploaded to `beta`, all three OS verified from a clean install.
- [ ] Achievements: all 40 defined in Steamworks, icons (locked and unlocked) uploaded, tested end to end; hidden ones flagged.
- [ ] Steam Cloud configuration published in Steamworks.
- [ ] Store page up to date with final screenshots and trailer; price set by owner; release date entered.
- [ ] Package and pricing: base package for the app, regional prices reviewed.
- [ ] Review keys: generate Steam keys for press and creators (owner) **(verify per-title key request limits live)**.
- [ ] Publish all pending Steamworks changes (Publish tab) [OWNER].

---

## H. Build review and release

- [ ] Submit the **release build** for review after store page approval [OWNER presses Submit]. Allow >= 7 business days; do not move the date inside 14 days.
- [ ] On pass: keep the reviewed build identical to what goes live. Changes after review can be pushed as a normal update.
- [ ] Final day-before: set the build live on `default` branch [OWNER], verify install on a fresh account.
- [ ] Release day: press "Release" in the Store Presence (Release) tab [OWNER]. Monitor: reviews, forums, crash reports, Cloud sync issues for 72 h (`/day-one-patch` ready).
- [ ] Post-launch: patch notes through Steam Events, regional-price check, apply for seasonal sale eligibility **(verify live)**.

---

## I. Demo and Steam Next Fest

- [ ] Create the Demo App from the main app in Steamworks (Demo App ID; separate store page, free, links to the full game, shares wishlist audience). Needs its own depots/builds; use the same CI pipeline with a different APPID.
- [ ] Demo content scope: first world (Hollow Earth) with one leader, saved progress isolated from full game (separate save slot or marked demo).
- [ ] Demo store page and build pass the same page/build reviews (allow the same 7+ day buffers).
- [ ] Pick the Next Fest edition. Per official pages: June 2026 and October 2026 have passed; **next registration deadline is 10 Jan 2027 (Feb 2027 edition); June 2027 edition follows**. Remember **one Next Fest per game**: choose the edition that matches a polished demo.
- [ ] Registration in Steamworks by the deadline; demo public **before fest start**; fest page assets submitted by the stated review date (October 2026 had a ~1 month pre-fest review submission date; assume similar **(verify live)**).
- [ ] Prepare livestream/developer-stream plan with store-marketing-specialist.
- [ ] After the fest: keep demo live or take down per marketing plan.

---

## J. Optional: Playtest app

- [ ] Steam Playtest (separate Playtest App ID) for closed beta signups, controlled by owner. Same build pipeline. Hide until ready.

---

## K. CI upload account [OWNER, devops-engineer]

- [ ] Create a dedicated Steam builder account (not the owner's). Grant only *Edit App Metadata* and *Publish App Changes To Steam* on the Steamworks group, per game-ci/steam-deploy recommendation.
- [ ] Owner runs `steamcmd` locally once, logs in, completes Steam Guard, base64-encodes `config/config.vdf`, stores it as CI secret `STEAM_CONFIG_VDF`; username as `STEAM_USERNAME`. Never paste these in chat or the repo.
- [ ] Re-do when the token expires (Steam Guard challenge appears in CI logs).
- Full flow: `tools/ci/steam/README.md`.

---

## L. Open decisions for the owner

1. Legal entity and bank (Steuerberater input).
2. Price (concept target EUR 4.99) and launch discount percent.
3. Target launch date D (drives all dates above) and Next Fest edition (Feb or June 2027 vs. later).
4. Auto-Cloud vs. API Cloud (recommended: Auto-Cloud).
5. Whether to run a Playtest app before the demo.
