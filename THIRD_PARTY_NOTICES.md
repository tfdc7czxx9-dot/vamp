# Third-Party Notices — BRASSLIGHT

> **Not legal advice.** Skeleton maintained by `legal-compliance-specialist`. Last updated: 2026-11-08.
> Every new dependency needs an ADR and a license check before it is added (`.claude/docs/technical-preferences.md`). Update this file in the same change that adds the dependency, and ship the final text in-game (Credits > Licenses) and on the store pages where required.

## 1. Engine

| Component | Version | License | Notes |
|-----------|---------|---------|-------|
| Unity Engine | 6.3 LTS | Proprietary, Unity Terms of Service / Unity Software Additional Terms | Not redistributed as source. Plan tier is tied to revenue/funding (see `production/legal/compliance-checklist.md`). Unity-bundled packages (URP, Input System, Addressables, Localization, Burst, Collections, 2D packages) are covered by Unity's package license terms (check each package's `LICENSE.md`/third-party notices file) |

## 2. Platform SDK wrappers

| Component | Version | License | Link |
|-----------|---------|---------|------|
| Steamworks.NET | [VERSION] | MIT | https://github.com/rlabrecque/Steamworks.NET |
| Steamworks SDK (Valve, native library redistributed with the game) | [VERSION] | Valve Steamworks SDK Access Agreement (proprietary; no separate attribution text required, confirm in the agreement) | https://partner.steamgames.com/ |
| Apple Unity Plug-ins (GameKit), optional | [VERSION] | [VERIFY: Apache-2.0 expected; check repository] | https://github.com/apple/unityplugins |
| Google Play Games plugin for Unity, optional | [VERSION] | [VERIFY: Apache-2.0 expected; check repository] | https://github.com/playgameservices/play-games-plugin-for-unity |

### Steamworks.NET — MIT license text (placeholder; paste exact text from the repository's LICENSE.txt for the shipped version)

```
The MIT License (MIT)

Copyright (c) [YEAR] [COPYRIGHT HOLDER as stated in the Steamworks.NET LICENSE.txt]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 3. Build and test tooling (not shipped in the game)

| Component | License | Shipped? |
|-----------|---------|----------|
| GameCI (game-ci/unity-builder, unity-test-runner) | MIT [VERIFY] | No |
| NUnit / Unity Test Framework | [VERIFY] | No |

## 4. Fonts

| Font | Author | License | Used for | Source URL |
|------|--------|---------|----------|-----------|
| _(none yet)_ | | | | |

Check: embedding and redistribution in apps is allowed; for CJK post-launch languages also check font license. SIL OFL fonts require the license text to be included.

## 5. Music and sound

| Asset | Creator | License / contract | Credit text required | Proof stored at |
|-------|---------|--------------------|----------------------|-----------------|
| _(none yet; score is to be original, composer TBD)_ | | | | |

For commissioned work: written agreement covering all platforms, worldwide, perpetual, buy-out or license scope, and soundtrack release rights.

## 6. Art, 3D models, textures, VFX

| Asset | Creator | License | Credit | Proof |
|-------|---------|---------|--------|-------|
| _(none yet)_ | | | | |

## 7. AI-generated assets

Policy: **none planned.** If any are introduced, record tool, version, date, prompt/output provenance, the tool's commercial-use terms, and how it is disclosed on Steam (Steam asks for AI-content disclosure; verify current form) and in any other store forms. Copyrightability of AI output is uncertain: get legal review first.

| Asset | Tool and version | Terms checked on | Disclosure made |
|-------|------------------|------------------|-----------------|
| _(none)_ | | | |

## 8. Public-domain sources and inspiration

"Inspired by the novels of Jules Verne and H. G. Wells." Details and exclusions: `design/ip-guidelines.md`. Public-domain original texts and period illustrations (for example Riou and de Neuville engravings, Alvim Corréa illustrations) require no license but record exact edition and source URL here when an image is used directly.

| Item | Source | Author death / publication | PD status verified (EU / US) on | Notes |
|------|--------|----------------------------|---------------------------------|-------|
| _(none used directly yet)_ | | | | |

## 9. Other libraries and code snippets

| Name | Version | License | URL | Added by / ADR |
|------|---------|---------|-----|----------------|
| _(none yet)_ | | | | |

## Attribution texts shown in-game (Credits > Licenses)

_[To be generated from the tables above before each release.]_
