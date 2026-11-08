# ADR-0001: Engine — Unity 6.3 LTS with C#

## Status

Accepted

## Date

2026-11-08

## Last Verified

2026-11-08 — against `docs/engine-reference/unity/VERSION.md` (Unity 6.3 LTS).
Store-side requirements (Android target API, Xcode/iOS SDK minimums) change
yearly and must be re-verified by the publishing specialists before each submission.

## Decision Makers

Project owner (decision), technical-director (recommendation record).

## Summary

VAMP is built in Unity 6.3 LTS with C#, URP 2D Renderer and the Input System,
because one codebase must ship to Steam (Windows/macOS/Linux/Steam Deck), Google
Play and the Apple App Store, and Unity has the most mature mobile export,
store-plugin ecosystem and CI tooling (GameCI) for that combination.

## Engine Compatibility

| Field | Value |
|-------|-------|
| Engine | Unity 6.3 LTS |
| Render pipeline | URP — amended by ADR-0002: Universal Renderer (3D) instead of 2D Renderer |
| Scripting backend | IL2CPP on Android (ARM64) and iOS (required). Mono on desktop: IL2CPP for Windows/macOS cannot be cross-compiled on Linux CI runners; revisit if profiling shows a need (amended 2026-11-08) |
| Android | ARM64, AAB output, Play Asset Delivery if > 150 MB |
| iOS | Xcode project export, built and signed on macOS (local or CI runner) |
| Knowledge-gap risk | HIGH — Unity 6.x APIs post-date model training; check `docs/engine-reference/unity/` before using APIs |

## ADR Dependencies

None (foundation ADR). Future ADRs depending on this: platform abstraction layer,
horde simulation architecture, save system, data format for balance values.

## Context

### Problem Statement

Choose an engine and language that can ship a premium 2D survivor-like roguelite
to three storefronts with one codebase and a small team.

### Current State

Fresh project — no code, no engine configured.

### Constraints

- Must export to Windows, macOS, Linux, Android and iOS from one project
- Must hold 500–1000 on-screen enemies at 60 fps on Steam Deck and mid-range phones
- Small team: prefer mature tooling, documentation and store plugins
- Premium model: no ads/IAP SDK requirements

### Requirements

- Steamworks integration (achievements, cloud saves, Steam Input / Deck)
- Google Play and App Store compliant builds (signing, privacy declarations)
- Automated CI builds and tests

## Decision

Use Unity 6.3 LTS, C#, URP (renderer per ADR-0002), Input System, Addressables, Localization package,
Unity Test Framework, IL2CPP.

### Architecture

Layered assemblies (asmdef):

- `Vamp.Core` — pure C# (no UnityEngine dependency where possible): rules, formulas, RNG, data models. Fully EditMode-testable.
- `Vamp.Gameplay` — MonoBehaviours / systems: player, enemies, weapons, pickups, run flow.
- `Vamp.Platform` — `IPlatformServices` abstraction (achievements, cloud save, overlay/pause, store links) with implementations `Steam`, `Apple`, `GooglePlay`, `Null` (editor/offline).
- `Vamp.UI` — menus/HUD (UI Toolkit).
- `Vamp.Tests.EditMode` / `Vamp.Tests.PlayMode`.

### Key Interfaces

```csharp
namespace Vamp.Platform
{
    /// <summary>Store/platform services used by gameplay. Gameplay never calls a store SDK directly.</summary>
    public interface IPlatformServices
    {
        void UnlockAchievement(string achievementId);
        bool TryLoadCloudSave(out byte[] data);
        void WriteCloudSave(byte[] data);
        event System.Action<bool> OverlayToggled; // pause game while store overlay is open
    }
}
```

### Implementation Guidelines

- Platform-specific code is compiled only via scripting define symbols / asmdef platform filters.
- Balance numbers live in ScriptableObjects or JSON under `assets/data/` mirrored into the Unity project — never in code.

## Alternatives Considered

### Alternative 1: Godot 4.6 + GDScript

Free and royalty-free, lightweight 2D. Rejected by the project owner in favour of
Unity's larger mobile/store plugin ecosystem and team familiarity.

### Alternative 2: Unreal Engine 5

Strong 3D, but heavy for a 2D mobile title; larger builds and slower iteration.

## Consequences

### Positive

- Mature Android/iOS export, GameCI pipelines, Steamworks.NET
- Large hiring/asset/documentation pool

### Negative

- Unity licensing terms must be monitored (revenue thresholds for Unity Personal/Pro)
- Unity CI builds need a Unity license secret; iOS builds need a macOS runner

### Neutral

- C# chosen over visual scripting for testability

## Risks

| Risk | Mitigation |
|------|------------|
| Unity license/pricing changes | `legal-compliance-specialist` reviews terms at each milestone |
| Model knowledge gap on Unity 6 APIs | Always consult `docs/engine-reference/unity/` |
| Horde performance on low-end Android | Pooling + spatial hash first; Jobs/Burst if profiling requires (separate ADR) |

## Performance Implications

See budgets in `.claude/docs/technical-preferences.md`.

## Migration Plan

N/A (initial choice).

## Validation Criteria

- Empty project builds for all five targets in CI
- Prototype with 500 enemies holds 60 fps on Steam Deck and a mid-range Android device

## GDD Requirements Addressed

Platform and performance requirements from `design/gdd/game-concept.md`.

## Related

- `design/gdd/game-concept.md`
- `production/roadmap-to-launch.md`
