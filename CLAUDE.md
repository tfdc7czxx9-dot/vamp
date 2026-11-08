# Claude Code Game Studios -- Game Studio Agent Architecture

Indie game development managed through 55 coordinated Claude Code subagents.

**Project**: IMPOSSIBLE VOYAGES (working title, codename VAMP) — a Victorian retro-sci-fi survivor-like roguelite in public-domain Jules Verne / H. G. Wells worlds, with two signature mechanics: recruiting defeated elites into your crew, and building structures on the battlefield.
**IP rules**: `design/ip-guidelines.md`
**Targets**: Steam (Windows/macOS/Linux + Steam Deck), Google Play (Android), Apple App Store (iOS).
**Business model**: Premium (one-time purchase) on every store — no ads, no IAP.
**Concept**: `design/gdd/game-concept.md` · **Roadmap**: `production/roadmap-to-launch.md`
Each agent owns a specific domain, enforcing separation of concerns and quality.

## Technology Stack

- **Engine**: Unity 6.3 LTS (see ADR-0001)
- **Language**: C#
- **Version Control**: Git with trunk-based development (Git LFS for binary assets)
- **Build System**: Unity Build Profiles + GameCI (GitHub Actions) for Windows/macOS/Linux, Android (AAB), iOS (Xcode project)
- **Asset Pipeline**: Unity Addressables, Sprite Atlases, URP Universal Renderer (3D) with instanced sprite billboards — 2D-3D diorama look (ADR-0002)
- **Art / Audio direction**: `design/art/art-bible.md`, `design/gdd/sound-bible.md`

> **Note**: This project uses the **Unity** specialist set (`unity-specialist`,
> `unity-ui-specialist`, `unity-addressables-specialist`, `unity-shader-specialist`,
> `unity-dots-specialist`). Godot/Unreal agents are unused.
>
> **Store publishing agents**: `steam-publishing-specialist`,
> `android-publishing-specialist`, `ios-publishing-specialist`,
> `mobile-platform-programmer`, `store-marketing-specialist`,
> `legal-compliance-specialist` — see `.claude/docs/agent-roster.md`.

## Project Structure

@.claude/docs/directory-structure.md

## Engine Version Reference

@docs/engine-reference/unity/VERSION.md

## Technical Preferences

@.claude/docs/technical-preferences.md

## Coordination Rules

@.claude/docs/coordination-rules.md

## Collaboration Protocol

**User-driven collaboration, not autonomous execution.**
Every task follows: **Question -> Options -> Decision -> Draft -> Approval**

- Agents MUST ask "May I write this to [filepath]?" before using Write/Edit tools
- Agents MUST show drafts or summaries before requesting approval
- Multi-file changes require explicit approval for the full changeset
- No commits without user instruction

See `docs/COLLABORATIVE-DESIGN-PRINCIPLE.md` for full protocol and examples.

> **First session?** If the project has no engine configured and no game concept,
> run `/start` to begin the guided onboarding flow.

## Coding Standards

@.claude/docs/coding-standards.md

## Context Management

@.claude/docs/context-management.md
