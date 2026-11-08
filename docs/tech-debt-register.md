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

## Design content (1.0 scope from the concept)

| ID | Item | Status |
|----|------|--------|
| C-01 | Gadget **evolutions** + crew/structure **synergies** | ✅ 6 evolutions (max level + partner passive); synergies "Charged" (Lightning Mast → chain attacks) and "Overclocked" (Steam Turret → +50 % attack speed) |
| C-02 | Leaders 4–6 | ✅ 6 leaders (sprites are recoloured placeholders) |
| C-03 | Bestiary | ✅ Screen + save tracking (seen / recruited); per-creature lore texts still to write (writer) |
| C-04 | Boss attack patterns | ✅ Radial volleys + telegraphed charges; unique per-boss patterns still desirable |
| C-05 | Ranged enemies | ✅ 4 shooter types across the worlds |
| C-06 | Languages | ✅ EN, DE, FR (CI checks key parity); content-name translations for enemies pending |
| C-07 | Unique visuals per evolution (currently same sprite with glow) | Open (art) |
| C-08 | Balance pass with real players (bot dies at ~10–11 min without research; target: first win after ~5–8 runs) | Open (`/balance-check`, playtests) |

## Known simplifications in the simulation

- Companions attack with a generic melee/zap; per-species attack patterns (C-01/C-05) would add identity.
- Structures share one destructible-HP model; mine fields are indestructible by design.
- Enemy separation checks at most 8 neighbours for performance; dense clumps can overlap slightly.
