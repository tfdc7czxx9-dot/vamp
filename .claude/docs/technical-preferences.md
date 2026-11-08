# Technical Preferences

## Engine & Language

- **Engine**: Unity 6.3 LTS (ADR-0001 — `docs/architecture/adr-0001-engine-unity6.md`)
- **Language**: C# (Unity's supported C# version; nullable reference types off in runtime assemblies)
- **Rendering**: URP Universal Renderer (3D, Forward+) — pixel-art sprite billboards in 3D diorama environments, perspective camera, tilt-shift DOF/bloom, quality tiers (ADR-0002)
- **Physics**: Gameplay on the XZ plane. No rigidbody physics for enemies — custom spatial-hash collision for hordes; no Physics2D

## Input & Platform

- **Target Platforms**: Steam (Windows, macOS, Linux, Steam Deck), Google Play (Android 8.0+ / API 26+ minimum, target API = current Google Play requirement), Apple App Store (iOS / iPadOS, minimum = current Unity 6.3 iOS minimum)
- **Input Methods**: Keyboard/Mouse, Gamepad, Touch
- **Primary Input**: Movement only (8-direction / analog). Attacks are automatic — this keeps every platform equal.
- **Gamepad Support**: Full (required for Steam Deck Verified; also MFi/Xbox/PS controllers on mobile)
- **Touch Support**: Full (floating virtual joystick, all menus touch-sized ≥ 48dp / 44pt)
- **Platform Notes**:
  - Unity Input System package only (legacy Input Manager disabled)
  - Respect safe areas / notches on mobile (`Screen.safeArea`)
  - Pause automatically on focus loss / app backgrounding
  - Steam Deck: 1280×800 readable text, no mouse-only UI, on-screen keyboard for text entry
  - Portrait not supported — landscape only on mobile

## Naming Conventions

- **Classes / Structs / Enums**: PascalCase (`EnemySpawner`, `WeaponDefinition`)
- **Interfaces**: `I` prefix (`IDamageable`)
- **Public members / properties / methods**: PascalCase
- **Private fields**: `_camelCase`; serialized private fields use `[SerializeField] private` + `_camelCase`
- **Local variables / parameters**: camelCase
- **Signals/Events**: C# events / `Action` named as past-tense verbs prefixed with `On` for handlers (`EnemyKilled` event, `OnEnemyKilled` handler)
- **Files**: one public type per file, file name = type name (`EnemySpawner.cs`)
- **Scenes/Prefabs**: PascalCase (`Run_Graveyard.unity`, `Enemy_Villager.prefab`)
- **ScriptableObject assets**: `SO_` prefix not used — folder by type instead (`Data/Gadgets/SteamCannon.asset`)
- **Constants**: PascalCase `const` / `static readonly`
- **Namespaces**: `Vamp.<Layer>.<System>` (e.g. `Vamp.Gameplay.Weapons`)

## Performance Budgets

- **Target Framerate**: 60 fps on PC/Steam Deck/modern mobile; 30 fps floor on low-tier Android (user-selectable)
- **Frame Budget**: 16.6 ms total; gameplay simulation ≤ 4 ms with 500 active enemies on Steam Deck
- **Draw Calls**: ≤ 150 batches mobile / ≤ 300 PC (swarm via instanced billboards in 1–3 draws, SRP Batcher for environments)
- **Post-processing**: ≤ 3 ms on Medium tier (Steam Deck); tiers Low/Medium/High/Ultra data-driven (ADR-0002)
- **Memory Ceiling**: 1.0 GB on mobile, 2 GB on PC
- **Enemy cap**: 500 simultaneous on mobile, 1000 on PC (data-driven cap)
- **Build size**: Android AAB base ≤ 150 MB (Play Asset Delivery beyond); iOS ≤ 200 MB initial download
- **GC**: zero per-frame allocations in the hot loop (pooling for enemies, projectiles, pickups, damage numbers)

## Testing

- **Framework**: Unity Test Framework (NUnit) — EditMode for pure logic, PlayMode for integration
- **Minimum Coverage**: 80% of pure-logic assemblies (`Vamp.Core`, `Vamp.Gameplay.Rules`)
- **Required Tests**: Balance formulas, gameplay systems, save/load, platform service fallbacks (offline Steam, no Game Center)

## Forbidden Patterns

- Legacy `UnityEngine.Input` API
- `GameObject.Find*` / `FindObjectOfType` at runtime
- `Resources/` folder for content (use Addressables)
- Hardcoded gameplay numbers in MonoBehaviours (use ScriptableObject/JSON data)
- Platform SDK calls outside the `Vamp.Platform` abstraction layer (no `Steamworks.*` or GameKit calls from gameplay code)
- Ads SDKs, IAP, analytics that collect personal data — the game is premium and data-minimal
- Instantiating/destroying per frame in hot paths (use pooling)
- One SpriteRenderer GameObject per swarm enemy (use the instanced `ISwarmRenderer`)

## Allowed Libraries / Addons

- Unity packages: Input System, URP, Addressables, Localization, Test Framework, 2D Sprite/Animation, Burst + Collections (for the enemy simulation)
- Steam: Steamworks.NET (MIT) — wrapped by `Vamp.Platform.Steam`
- iOS: Apple Unity Plug-ins (GameKit) — optional, wrapped by `Vamp.Platform.Apple`
- Android: Google Play Games plugin for Unity — optional, wrapped by `Vamp.Platform.Android`
- CI: GameCI (game-ci/unity-builder, unity-test-runner)
- Any new dependency requires an ADR and a license check by `legal-compliance-specialist`

## Architecture Decisions Log

- ADR-0001 — Engine: Unity 6.3 LTS + C# (`docs/architecture/adr-0001-engine-unity6.md`)
- ADR-0002 — Rendering: 2D sprites in 3D diorama worlds, URP Universal Renderer (`docs/architecture/adr-0002-rendering-diorama-2d3d.md`)

## Engine Specialists

- **Primary**: unity-specialist
- **Language/Code Specialist**: unity-specialist (C# idioms), lead-programmer for reviews
- **Shader Specialist**: unity-shader-specialist
- **UI Specialist**: unity-ui-specialist
- **Additional Specialists**: unity-addressables-specialist (content/loading), unity-dots-specialist (only for the horde simulation if Jobs/Burst are adopted), mobile-platform-programmer (touch, safe area, thermals)
- **Routing Notes**: Store/platform SDK work routes to the matching publishing specialist (`steam-publishing-specialist`, `android-publishing-specialist`, `ios-publishing-specialist`)

### File Extension Routing

| File Extension / Type | Specialist to Spawn |
|-----------------------|---------------------|
| Game code (`*.cs`) | unity-specialist |
| Shader / material files (`*.shader`, `*.shadergraph`, `*.mat`) | unity-shader-specialist |
| UI / screen files (`*.uxml`, `*.uss`, UI prefabs) | unity-ui-specialist |
| Scene / prefab / level files (`*.unity`, `*.prefab`) | unity-specialist |
| Native extension / plugin files (`Assets/Plugins/**`) | matching publishing specialist |
| Build/platform settings (`ProjectSettings/**`, Build Profiles) | devops-engineer + matching publishing specialist |
| General architecture review | unity-specialist |
