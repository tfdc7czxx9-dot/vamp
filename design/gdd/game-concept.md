# Game Concept: VAMP (working title)

> **Status**: Draft v0.1 — proposed by the studio, awaiting owner review.
> Validate with `/design-review design/gdd/game-concept.md`. Everything here is
> original to this project: genre conventions (auto-attacking, horde survival,
> level-up choices) are used, but no characters, names, art or content from
> existing games.

## Elevator Pitch

You are a newly turned vampire with one night to feed. Drift through the town
while your powers strike automatically at hunting parties, drink the blood they
leave behind to grow stronger — and get back to your coffin before the sun rises.
Stay out longer for more blood, or flee early and live.

## Core Identity

| Field | Value |
|-------|-------|
| Genre | Survivor-like roguelite ("bullet heaven"), top-down 2D |
| Platforms | Steam (Win/macOS/Linux/Steam Deck), Android, iOS |
| Business model | Premium, one-time purchase. No ads, no IAP, no energy systems |
| Target price | Steam €4.99 / mobile €3.99 (to validate with `store-marketing-specialist`) |
| Session length | 10–20 minute runs; first run playable in < 60 s from launch |
| Controls | Movement only; attacks are automatic |
| Rating target | PEGI 12 / USK 12 / ESRB T (stylised, non-realistic blood) |

## Core Fantasy

Being the monster everyone is afraid of — starting fragile and hunted, ending
the night as an unstoppable storm of bats, blood and shadow, while the clock
toward dawn keeps you honest.

## Unique Hook

**The Dawn Gamble.** Each run is one night. At any time after midnight the player
can return to their coffin to *bank* the blood collected. As dawn approaches,
sunlight creeps in from the map edges (shrinking the safe area) and hunters get
stronger, but blood rewards multiply. Dying out in the open loses most of the
night's haul. Every run ends with a push-your-luck decision rather than a fixed
timer.

Secondary hooks:
- **Blood is everything**: the single resource is XP, healing and meta-currency at once — spending blood to heal slows your growth.
- **Thrall evolution**: certain combinations of powers turn defeated hunters into temporary thralls who fight for you.

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)

1. **Challenge** — survive escalating hunts
2. **Fantasy** — become the apex predator
3. **Discovery** — find power combinations and evolutions
4. **Sensation** — screen-filling, readable, satisfying effects

### Key Dynamics (Emergent player behaviors)

- Kiting hordes into choke points; routing to blood pools
- Risk calculation at the Dawn Gamble
- Build planning around evolutions and thralls

### Core Mechanics (Systems we build)

1. Movement & camera
2. Auto-attacking powers (weapons) with levels and evolutions
3. Horde spawner with wave/difficulty curve
4. Blood pickup → level-up choice (pick 1 of 3)
5. Night clock + dawn sunlight zone + coffin banking
6. Meta-progression (permanent upgrades, unlockable vampires, maps)
7. Save system with cloud sync

## Player Motivation Profile

### Primary Psychological Needs Served

- **Competence**: visible power growth within a single run
- **Autonomy**: build choices, when to bank
- **Relatedness**: light — leaderboards/achievements only

### Player Type Appeal (Bartle Taxonomy)

Achievers (unlocks, achievements) and Explorers (combinations) first; Killers via score chase.

### Flow State Design

Difficulty ramps with the night clock; level-up choices act as micro-rests;
the Dawn Gamble lets players self-select difficulty at the end of each run.

## Core Loop

### Moment-to-Moment (30 seconds)

Move, dodge, collect blood, watch powers fire.

### Short-Term (5-15 minutes)

Level up every 20–60 s, choose a power, push toward an evolution; mini-boss "Inquisitor" at midnight.

### Session-Level (30-120 minutes)

2–6 runs; spend banked blood on permanent upgrades between runs.

### Long-Term Progression

Unlock 6–8 vampires (each with a starting power and passive), 3–4 maps, evolutions, achievements, a challenge mode.

### Retention Hooks

Unlock list with visible silhouettes, achievement list, "one more night".
No daily login rewards or FOMO (premium game, respects the player's time).

## Game Pillars

### Pillar 1: One Thumb, Full Power

Every platform gets the same game: movement-only input, readable on a phone and a TV.

### Pillar 2: The Night Is a Gamble

Every run ends with a meaningful risk/reward decision, not just a timer.

### Pillar 3: Fair and Premium

No ads, no IAP, no dark patterns. Pay once, own everything, play offline.

### Anti-Pillars (What This Game Is NOT)

- Not a story-heavy RPG — lore is flavour text
- Not multiplayer (at launch)
- Not gacha / live-service
- Not gory-realistic — stylised blood only

## MVP Scope (Vertical Slice)

- 1 map, 1 vampire, 8 powers, 2 evolutions, 6 enemy types, 1 mini-boss
- Night clock + Dawn Gamble + coffin banking
- 5 permanent upgrades, local save
- Keyboard, gamepad and touch input

## Launch Scope (1.0)

- 3 maps, 6 vampires, ~30 powers, ~10 evolutions, ~25 enemy types, 3 bosses
- 40 Steam achievements (mirrored to Game Center / Play Games where enabled)
- Steam Cloud / iCloud / Play Games saved-game sync
- Languages: EN, DE at launch; FR, ES, PT-BR, JA, ZH-Hans, KO post-launch candidates

## Technical Considerations

See ADR-0001 and `.claude/docs/technical-preferences.md`. Horde performance is the
top technical risk and is validated in the first prototype.

## Risks and Open Questions

- Survivor-like market is crowded → the Dawn Gamble must be proven fun in the prototype
- Premium mobile pricing conversion → consider a free demo on Steam (Next Fest)
- Open: final title (trademark search needed), art style (pixel art vs. hand-drawn), music direction

## Next Steps

1. Owner review of this concept
2. `/prototype dawn-gamble` — validate hook and 500-enemy performance
3. `/art-bible`, `/map-systems`, then GDDs per system
4. Follow `production/roadmap-to-launch.md`
