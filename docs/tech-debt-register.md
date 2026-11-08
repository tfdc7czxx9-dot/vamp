# Tech Debt & Open Work Register — BRASSLIGHT

> Last updated 2026-11-08. Items are ordered by priority within each group. Placeholders were
> chosen deliberately so the game is fully playable before final assets and polish exist.

## Must fix before Early Access / launch

| ID | Item | Why it exists | Owner | Effort |
|----|------|---------------|-------|--------|
| TD-01 | **First open in the Unity editor**: the Unity layer is compile-checked against 2021.3 reference assemblies plus verified stubs, but has never run in Unity 6.3. Expect small fixes (shader keywords, material setup, IMGUI scaling). | No Unity editor in the authoring environment | unity-specialist | S |
| TD-02 | **Final art**: procedural placeholder sprites (`PixelArt.cs`) → hand-made pixel art and 3D environment kits per `design/art/art-bible.md` | Art production not started | art-director, technical-artist | XL |
| TD-03 | **Final audio**: synthesised SFX and placeholder theme (`SoundBank.cs`) → commissioned score and SFX per `design/gdd/sound-bible.md` | Composer not contracted | audio-director | L |
| TD-04 | **UI**: IMGUI HUD/menus (`Hud*.cs`) → UI Toolkit implementation with proper fonts, icons and accessibility (text scaling, colour-blind options) | Fast, input-agnostic first version | unity-ui-specialist, ux-designer | L |
| TD-05 | **Device performance pass**: 500 enemies + DOF/bloom on Steam Deck and low-end Android (ADR-0002 validation criteria) | Needs real hardware | performance-analyst, mobile-platform-programmer | M |
| TD-06 | **Steam Deck verification pass**: glyphs per controller, text size at 1280×800, suspend/resume | Needs hardware | qa-lead, steam-publishing-specialist | S |
| TD-07 | **iOS privacy manifest & Game Center / iCloud**: `IPlatformServices` has only Offline + Steam backends | Store accounts not yet created | ios-publishing-specialist | M |
| TD-08 | **Google Play Games (optional)**: achievements / saved games backend | Optional for premium launch | android-publishing-specialist | M |
| TD-09 | **Application identifier** placeholder `com.yourname.brasslight` | Owner's handle unknown | owner | XS |

## Design content still to build (1.0 scope from the concept)

| ID | Item | Status |
|----|------|--------|
| C-01 | Gadget **evolutions** incl. crew+structure synergies (e.g. lightning tripod) | Not implemented; achievements referencing it are not evaluated |
| C-02 | Leaders 4–6 (concept lists 6; 3 exist) | Data only (add to `balance.json` + sprites) |
| C-03 | Bestiary screen (discovered/recruited creatures) | Save data tracks recruited species; no screen yet |
| C-04 | Unique boss attack patterns (bosses currently use standard chase + contact damage) | Simulation extension |
| C-05 | Ranged enemies / telegraphed attacks | Simulation extension |
| C-06 | Additional languages (FR at launch per concept) | Add `loc/fr.json`; CI checks key parity |

## Known simplifications in the simulation

- Companions attack with a generic melee/zap; per-species attack patterns (C-01/C-05) would add identity.
- Structures share one destructible-HP model; mine fields are indestructible by design.
- Enemy separation checks at most 8 neighbours for performance; dense clumps can overlap slightly.
