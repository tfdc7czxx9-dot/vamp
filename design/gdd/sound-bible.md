# Sound Bible: IMPOSSIBLE VOYAGES

> Version 0.1. Music direction set by owner on 2026-11-08. Owner: audio-director.
> Sections marked *TODO* are completed via `/team-audio`.

## Audio Vision

### Sonic Identity

**A classic JRPG orchestral score meets Victorian brass and steam.** Memorable
melodies and leitmotifs in the tradition of the great Japanese RPG soundtracks,
orchestrated for an expedition into the impossible.

### Audio Pillars

1. **Melody first**: every world has a theme you can hum after one run
2. **Leitmotifs**: the expedition theme returns in every world, boss and victory screen
3. **Readable chaos**: with 500 enemies on screen, important cues (Elite spawn, recruit complete, structure built, low HP) must always cut through

### Reference Games / Media

Style references only, used for mood and orchestration. **No melody, chord
progression or signature motif may be imitated**; see Music Rules.

- Classic and modern Japanese RPG orchestral scores (lyrical themes, heroic battle music, character leitmotifs)
- Late-romantic orchestral music (Dvořák, Saint-Saëns, Holst — public-domain compositions; their own recordings are not)
- Victorian brass-band and music-hall colors

## Music Direction

### Style and Genre

Orchestral JRPG score: strings, woodwinds, brass, harp, piano and choir for
bosses, plus a world-specific "voyage color":

| Track | Character | Voyage color instruments |
|-------|-----------|--------------------------|
| Main theme ("The Expedition") | heroic, longing, adventurous | full orchestra, brass band |
| Hollow Earth | wonder → primal drive | marimba, log drums, cave-reverb choir |
| Ocean depths | mysterious, flowing | harp, celesta, glass harmonica, pipe organ |
| Martian invasion | dread, defiance | low brass, anvils, theremin-like synth, snare marches |
| Lunar caverns | alien, serene | music box, vibraphone, solo violin |
| Boss themes | epic | choir, timpani, organ, quotes of the world theme |
| Camp / menu | warm, nostalgic | piano, accordion, solo cello |
| Victory / level-up stings | triumphant | our *own* fanfare motif based on the main theme |

### Instrumentation Palette

Orchestral core plus steampunk textures: brass band, accordion, music box,
harmonium, ratchets and gears as percussion, steam hisses as rhythm elements.

### Adaptive Music System

- **Vertical layering**: 3–4 intensity layers per world track (exploration → swarm → elite/danger → boss), crossfaded by swarm size and HP
- **Stingers**: Elite spawn, recruit complete, structure built and level-up are short musical stings in the key of the current track
- **Implementation**: *TODO* (Unity Audio Mixer + custom layer controller, or middleware; decide via ADR)

### Music Rules

- Every piece is **original**: no soundalikes, no quoting or paraphrasing of melodies from Final Fantasy or other games and films
- No AI-generated music trained on copyrighted soundtracks without a clear commercial license (legal-compliance-specialist checks)
- Commission a composer with a written work-for-hire contract (full buyout incl. streaming and soundtrack-release rights)
- Recordings of public-domain classical pieces are **not** free. If used, record our own.
- Loops must be seamless. Each track at least 2:30 before looping.

## Sound Effects

### SFX Palette

*TODO (`/team-audio`)*. Direction: mechanical (gears, valves, steam), electric arcs, creature voices per world.

### Audio Feedback Priority

1. Player damage / low HP
2. Elite spawn and boss telegraphs
3. Recruit complete, structure built, level-up
4. Gadget fire (heavily voice-limited)
5. Enemy hits/deaths (pooled, max simultaneous voices)
6. Ambience

### SFX Rules

*TODO*. Voice limits per category for 500+ enemies.

## Mixing

### Mix Bus Structure

*TODO*. Master → Music / SFX (Player, Enemies, UI) / Ambience.

### Mixing Rules

*TODO*.

### Dynamic Range

Mobile: narrower dynamic range, speaker-safe low end.

## Technical Specifications

### Format Requirements

Music streamed (Vorbis on PC/Android, AAC on iOS). SFX decompressed on load if short.

### Naming Convention

*TODO*.

### Memory Budget

*TODO*. Target ≤ 60 MB audio on mobile.

## Accessibility

Separate volume sliders (music, SFX, UI), visual equivalents for critical audio cues, mono audio option.
