# Roadmap to Launch — BRASSLIGHT (codename VAMP)

> Living document. Owner: `producer`. Store-specific dates are maintained by the
> publishing specialists. Store rules (fees, tester counts, API levels, SDK
> minimums) **must be re-verified against official sources** before each phase;
> values marked *(verify)* are known to change.

**Targets**: Steam (Win/macOS/Linux/Deck) · Google Play · Apple App Store
**Model**: Premium, no ads, no IAP
**Engine**: Unity 6.3 LTS + C# (ADR-0001)

---

## Phase 0 — Setup (now)

| # | Task | Agent(s) | Skill | Status |
|---|------|----------|-------|--------|
| 0.1 | Engine & tech preferences configured | technical-director | `/setup-engine` | ✅ done |
| 0.2 | Game concept draft (v0.2: Verne/Wells setting, Recruit + Engineering) | creative-director, game-designer | `/brainstorm` | ✅ draft — owner review |
| 0.3 | Store publishing agents created | — | — | ✅ done |
| 0.4 | Create Unity 6.3 LTS project (**Universal 3D** template) and run Brasslight > Setup Project — steps in `docs/BUILDING.md` | **owner** + unity-specialist | — | ⬜ (code ready) |
| 0.5 | Test framework + CI (GameCI: tests, Win/Linux/macOS/Android builds; iOS on macOS runner) | devops-engineer | `/test-setup` | ✅ workflows in `.github/workflows` (Unity part activates with secrets) |
| 0.6 | Accounts: Steamworks (Steam Direct fee), Google Play Console, Apple Developer Program | **owner**, checklists by publishing specialists | — | ⬜ |
| 0.7 | Title trademark search, public-domain check of sources (`design/ip-guidelines.md`), business/tax questions for Steuerberater | legal-compliance-specialist | — | ⬜ |

> **Status 2026-11-08**: title BRASSLIGHT chosen; the playable vertical slice code exists ahead of
> schedule (Phases 1 and 3 overlap): full simulation in `Vamp.Core`, Unity presentation in `Vamp.Game`
> with placeholder procedural art/audio, 4 worlds of data, 3 leaders, research, achievements, EN/DE.
> Store, legal and marketing drafts exist under `production/`. Open items are tracked in
> `docs/tech-debt-register.md`.

## Phase 1 — Prototype (validate the hook)

| # | Task | Agent(s) | Skill |
|---|------|----------|-------|
| 1.1 | Prototype: movement, auto-attack, horde, Recruit (companions) + Engineering (build plots) | prototyper, unity-specialist | `/prototype` |
| 1.2 | Perf + look-dev spike: 500 instanced sprite billboards + lights + DOF/bloom in a 3D diorama (ADR-0002) on Steam Deck + low/mid Android | performance-analyst, mobile-platform-programmer | `/perf-profile` |
| 1.3 | Playtest Recruit + Engineering (are both fun? readable on a phone?) | qa-lead, game-designer | `/playtest-report` |
| 1.4 | Go / pivot decision | creative-director, producer | `/gate-check` |

## Phase 2 — Pre-production

| # | Task | Agent(s) | Skill |
|---|------|----------|-------|
| 2.1 | Art bible (draft exists: 2D-3D diorama) + environment kit plan | art-director, technical-artist | `/art-bible` |
| 2.2 | Systems map + GDDs (movement, powers, horde, night clock, progression, save) | game-designer, systems-designer, economy-designer | `/map-systems`, `/design-system` |
| 2.3 | Architecture + ADRs (platform layer, horde sim, save/cloud, data format) | technical-director, lead-programmer | `/create-architecture`, `/architecture-decision` |
| 2.4 | UX: HUD, touch controls, menus, accessibility | ux-designer, accessibility-specialist | `/ux-design` |
| 2.5 | Sound bible (draft exists) + composer search/contract | audio-director, legal-compliance-specialist | `/team-audio` |
| 2.6 | Epics + stories, sprint 1 | producer | `/create-epics`, `/create-stories`, `/sprint-plan` |

## Phase 3 — Vertical slice (MVP scope from concept)

Implementation via `/dev-story` → `/code-review` → `/story-done`.
Platform layer (`IPlatformServices` + Null implementation) is built here so stores can plug in later.
**Steam "Coming Soon" page goes live at the end of this phase** (store-marketing-specialist + steam-publishing-specialist) — wishlists need months.

## Phase 4 — Production (content to 1.0 scope)

- Content: worlds, leaders, gadgets, enemies, recruitable elites, structures, bosses (`/team-combat`, `/team-level`, `/team-audio`)
- Localization EN + DE (`/localize`, localization-lead)
- Platform integration, in parallel:
  - **Steam**: Steamworks.NET, achievements, Steam Cloud, Steam Input, Deck checks — steam-publishing-specialist
  - **Android**: AAB, signing, target API *(verify)*, Play Games (optional) — android-publishing-specialist + mobile-platform-programmer
  - **iOS**: Xcode export, signing, privacy manifest, Game Center/iCloud (optional) — ios-publishing-specialist + mobile-platform-programmer
- Steam demo for **Steam Next Fest** (store-marketing-specialist)

## Phase 5 — Beta & store preparation

| Store | Required before submission | Agent |
|-------|----------------------------|-------|
| Steam | Store page review, build review, Deck review, pricing, release date set; Coming Soon page live for the required minimum period *(verify)* | steam-publishing-specialist |
| Google Play | Closed test with the required number of testers for the required number of days for new personal accounts *(verify)*; Data safety; IARC rating; listing; DSA trader status | android-publishing-specialist |
| App Store | TestFlight; App Privacy label; age rating; screenshots; review notes; Paid Apps agreement; DSA trader status | ios-publishing-specialist |
| All | Privacy policy URL, Impressum, EULA, third-party notices | legal-compliance-specialist |
| All | Full QA cycle, device matrix, soak test, accessibility pass | qa-lead (`/team-qa`, `/soak-test`) |
| All | Release checklist | release-manager (`/release-checklist`, `/launch-checklist`) |

## Phase 6 — Launch

`/team-release` → owner presses "Release" on each store. Steam first (or simultaneous), mobile together.
Day-one patch readiness (`/day-one-patch`), community announcements (community-manager), 72 h monitoring.

## Phase 7 — Post-launch

Patches (`/hotfix`, `/patch-notes`), free content updates, additional languages, seasonal sales.

---

## Costs to budget (owner) *(verify current amounts)*

- Steam Direct fee per app (recoupable after revenue threshold)
- Google Play one-time registration fee
- Apple Developer Program annual fee
- Mac for iOS builds (or macOS CI minutes)
- Unity license if revenue exceeds the Personal tier threshold
- Optional: Steuerberater, lawyer review of policies, trademark registration
