# ADR-0002: Rendering — 2D sprites in 3D diorama worlds (URP Universal Renderer)

## Status

Accepted

## Date

2026-11-08

## Last Verified

2026-11-08 against `docs/engine-reference/unity/` (Unity 6.3 LTS). Instancing and
post-processing APIs must be checked in the engine reference before implementation.

## Decision Makers

Project owner (art direction decision), technical-director, art-director.

## Summary

The owner chose a premium JRPG-style **2D-3D diorama look**: pixel-art sprites in
3D environments with real-time lights and tilt-shift depth of field. This
replaces the URP **2D Renderer** from ADR-0001 with the URP **Universal Renderer
(3D, Forward+)**. Gameplay runs on the XZ plane, and all gameplay entities are
camera-facing sprite billboards drawn with GPU instancing.

## Engine Compatibility

| Field | Value |
|-------|-------|
| Engine | Unity 6.3 LTS |
| Pipeline | URP, Universal Renderer, Forward+ rendering path |
| Post-processing | URP Volume system (Depth of Field, Bloom, Color Adjustments, Vignette, Film Grain); custom tilt-shift and light-shaft passes as Renderer Features if needed |
| Knowledge-gap risk | HIGH: Unity 6 render-graph-based Renderer Feature API differs from 2022 LTS. Check `docs/engine-reference/unity/modules/rendering.md` first. |

## ADR Dependencies

- Amends **ADR-0001**: "URP 2D Renderer" → "URP Universal Renderer". Everything else in ADR-0001 stands.

## Context

### Problem Statement

Reach a polished, premium 2D-3D visual identity while keeping 500 enemies at
60 fps on Steam Deck and acceptable frame rates on mid-range phones.

### Constraints

- Survivor-likes need a wide, readable view; classic diorama games use low camera angles
- Mobile GPUs struggle with full-resolution DOF, many real-time lights and overdraw
- Hundreds of animated sprites cannot each be a SpriteRenderer GameObject

### Requirements

- Sprites lit by scene lights; shadows; DOF; bloom
- Quality tiers: Low / Medium / High / Ultra
- Same gameplay readability on all tiers

## Decision

### Architecture

- **World**: 3D, gameplay on the XZ plane. Perspective camera, fixed pitch about 50°, FOV about 25–30°, follows the player with smoothing.
- **Gameplay entities**: quads facing the camera (Y-axis-locked billboard). Sprite-sheet flipbook animation in the shader. Rendered via **GPU instancing** (indirect/instanced draw or BatchRendererGroup; choose after the prototype benchmark). Per-instance data: position, frame, tint, flash.
- **Hero, companions, bosses**: may use regular renderers with the same billboard shader (low count).
- **Lighting**: Forward+ for many point lights on PC. Mobile: baked lightmaps + light probes + max 4–8 real-time lights.
- **Shadows**: blob shadows (instanced decal quads) for swarms; real shadow casting only for hero, companions, bosses and structures on Medium and above.
- **Post-processing**: one global Volume per world + quality-tier overrides.

### Quality tiers (data-driven, `assets/data/quality-tiers.json`)

| Feature | Low (old phones) | Medium (mid phones, Deck) | High (PC) | Ultra (PC) |
|---------|------------------|---------------------------|-----------|------------|
| Render scale | 0.75 | 0.9–1.0 | 1.0 | 1.0 |
| DOF | off or cheap Gaussian tilt-shift | Gaussian tilt-shift | Bokeh | Bokeh |
| Bloom | low | on | on | high quality |
| Real-time lights | 4 | 8 | Forward+ many | Forward+ many |
| Shadows | blob only | blob + hero | full | full + soft |
| Light shafts / fog | off | fog | both | both |
| SSAO | off | off | on | on |

### Key Interfaces

```csharp
namespace Vamp.Gameplay.Rendering
{
    /// <summary>Draws all pooled swarm entities in instanced batches each frame.</summary>
    public interface ISwarmRenderer
    {
        void Submit(in SwarmRenderData data); // no allocations; data owned by the simulation
    }
}
```

### Implementation Guidelines

- Gameplay code never touches renderers. The simulation writes into structs, and `ISwarmRenderer` draws them.
- Pixel-art textures use point filtering with no mipmaps on sprites. Environment textures may use mipmaps.
- All tier values live in data, not code.

## Alternatives Considered

### Alternative 1: Keep URP 2D Renderer, fake depth with parallax layers

Cheaper, but cannot deliver real 3D lighting, perspective and DOF. Rejected: it does not meet the owner's visual target.

### Alternative 2: Full 3D characters

Loses the pixel-art identity and costs far more per character. Rejected.

## Consequences

### Positive

- Distinctive premium look; strong store screenshots and trailer
- Environments reusable across worlds via modular kits

### Negative

- Two art pipelines (pixel sprites + 3D environments) mean higher art cost
- Mobile performance risk from post-processing and overdraw
- Custom instanced sprite rendering must be built

### Neutral

- The physics plane changes from XY to XZ; the spatial hash is unchanged in principle

## Risks

| Risk | Mitigation |
|------|------------|
| DOF/bloom too expensive on mobile | Tier system; prototype benchmark on a low-end Android device in Phase 1 |
| Readability lost in blur and bloom | DOF only on screen edges; gameplay band always sharp; accessibility toggle |
| Instancing API complexity in Unity 6 | Spike in Phase 1; unity-shader-specialist + engine reference |

## Performance Implications

- Swarm rendering: 1–3 instanced draws for all standard enemies (shared atlas)
- Post-processing target: ≤ 3 ms on Medium tier on Steam Deck
- Budgets in `.claude/docs/technical-preferences.md` updated accordingly

## Migration Plan

No code exists yet. Create the Unity project from the **Universal 3D** template, not Universal 2D.

## Validation Criteria

- Prototype scene: 500 animated billboards + 8 lights + DOF + bloom → ≥ 60 fps Steam Deck (Medium), ≥ 30 fps on a low-end reference Android (Low)
- Art-director sign-off on a look-dev screenshot of the Hollow Earth world

## GDD Requirements Addressed

`design/gdd/game-concept.md` (art style), `design/art/art-bible.md` (rendering style).

## Related

ADR-0001, `design/art/art-bible.md`, `design/ip-guidelines.md`
