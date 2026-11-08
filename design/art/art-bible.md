# Art Bible: IMPOSSIBLE VOYAGES

## Document Status

- **Version**: 0.1 (direction set by owner 2026-11-08). Sections marked *TODO* are filled via `/art-bible` (retrofit mode).
- **Owner**: art-director · **Technical owner**: technical-artist, unity-shader-specialist
- **Technical basis**: ADR-0002 (`docs/architecture/adr-0002-rendering-diorama-2d3d.md`)

## Visual Identity Summary

**Pixel-art characters in lit 3D diorama worlds.** Hand-pixelled sprites stand in
low-poly 3D environments textured with pixel art. Real-time lights, depth of
field (tilt-shift), bloom, fog and light shafts make every world look like a
glowing miniature model of a 19th-century engraving brought to life. The goal is
the polish of current premium JRPG remakes in this style, built on our own
Victorian retro-sci-fi identity.

> **Naming**: in all public and store materials we call the style
> **"2D-3D diorama"** or "pixel art in 3D worlds". **"HD-2D" is a Square Enix
> trademark** and must not be used for our game (see `design/ip-guidelines.md`).

## Reference Board

Style references (study the techniques, never copy assets, characters or compositions):

- Modern JRPG remakes in the 2D-3D style: sprite-in-3D staging, tilt-shift DOF, bloom, lantern lighting
- Engravings by Riou and de Neuville from the Hetzel editions of Verne (public domain): shapes, machinery, creature design language
- Alvim Corréa's 1906 *War of the Worlds* illustrations (public domain): Martian machines
- Victorian technical drawings, brass instruments, gaslight, museum dioramas

## Color Palette

### Primary Palette

*TODO (`/art-bible`)*. Direction: warm brass, copper and gaslight amber for the
expedition; each world gets its own cool counter-palette.

### Emotional Color Mapping

| World | Base mood | Key colors |
|-------|-----------|------------|
| Hollow Earth | wonder, primeval | deep teal caverns, bioluminescent green, magma orange |
| Ocean depths | mystery, pressure | ink blue, cyan light shafts, pearl white |
| Martian invasion | dread, defiance | soot grey, red weed crimson, heat-ray white |
| Lunar caverns | alien calm | silver, violet, cold blue crystal |

## Art Style

### Rendering Style

- **Characters, enemies, companions, pickups, projectiles**: pixel-art sprites rendered as camera-facing billboards in 3D. Point filtering, lit by scene lights, casting soft blob or sprite shadows.
- **Environments**: modular low-poly 3D meshes with pixel-art textures (point filtering, consistent texel density), real-time point and spot lights (lanterns, lava, heat rays), height fog, light shafts, water reflections.
- **Post-processing (per quality tier)**: tilt-shift depth of field (top and bottom of the screen soft), bloom, color grading per world, vignette, subtle film grain. Optional screen-space AO on PC.
- **Camera**: perspective with a narrow FOV (~25–30°) and a fixed pitch (~50°) for the diorama look. The angle is steeper than in story JRPGs so the swarm stays readable.

### Proportions

- Hero and leaders: about 48 px tall. Standard enemies: 24–40 px. Elites and companions: 48–64 px. Bosses: multi-tile sprites or 3D/sprite hybrids.
- Chibi-leaning proportions (head about 1/3 of height) for readability at phone size.

### Level of Detail

Environments carry the detail. Gameplay sprites stay clean, with strong silhouettes and a 1 px dark outline option for readability.

### Visual Hierarchy

1. Player and companions (rim light + team color)
2. Enemy projectiles and danger telegraphs
3. Elites, Remnant recruit rings, build plots
4. Standard enemies
5. Pickups
6. Environment (desaturated near gameplay, detail toward the blurred edges)

## Character Art Standards

*TODO (`/art-bible`)*. 8-direction sprites for the hero, 4-direction for
enemies, idle/walk/hit/death minimum, normal maps optional on PC tier.

## Environment Art Standards

*TODO (`/art-bible`)*. Modular kit per world, texel density standard,
light-probe and lightmap rules for mobile.

## UI Art Standards

*TODO (`/art-bible`)*. Direction: brass-and-parchment frames, engraved icons,
JRPG-style crisp menus. Touch targets per `technical-preferences.md`.

## VFX Standards

*TODO (`/art-bible`)*. Direction: steam, sparks, electric arcs and bioluminescence.
Additive effects capped on the mobile tier.

## Asset Production Standards

### Naming Convention

*TODO*. Follows `technical-preferences.md` (PascalCase prefabs, folder-by-type).

### Texture Standards

- Pixel art: power-of-two atlases, point filtering, no mipmaps on sprites, ASTC on mobile (verify that pixel art holds up under compression)

### Animation Standards

*TODO*. Sprite flipbooks, 8–12 fps for characters.

## Accessibility

- Option to reduce or disable DOF, bloom, film grain and screen shake
- Colorblind-safe telegraph colors, shapes in addition to color
- High-contrast outline toggle for all gameplay sprites
