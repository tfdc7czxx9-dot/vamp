# Asset Specs: Store Graphics

> Owner: `store-marketing-specialist`. Handed to `art-director`.
> **Verified: 2026-11-08** via web search of the official pages named per store. Some values come only from search summaries or third-party guides; those are marked *(confirm)*. **Download the official Steamworks templates and re-read each official page before final export**, because sizes changed in August 2024 and may change again.
> Style words on all art: "2D-3D diorama", "pixel art in 3D worlds". Never "HD-2D". No competitor names or logos. Art must read at thumbnail size: logo + brass/steam + creature crew, not a generic swarm.

## 1. Steam

Official sources: [Graphical Assets Overview](https://partner.steamgames.com/doc/store/assets), [Library Assets](https://partner.steamgames.com/doc/store/assets/libraryassets), [Graphical Asset Rules](https://partner.steamgames.com/doc/store/assets/rules). The Library Assets page was confirmed directly via search on 2026-11-08. Store capsule values come from the same documentation family plus multiple guides; the search snippet of the main Overview page still showed an old main-capsule size, so treat the main capsule value as *(confirm)* against the live page.

### 1.1 Store capsules

| Asset | Size (px) | Notes |
|-------|-----------|-------|
| Header capsule | 920 x 430 | Required. Shown on the store page, search, wishlists. Also used as the library header if none is set |
| Small capsule | 462 x 174 | Required. Steam generates 184x69 and 120x45 automatically. Logo must stay legible at those sizes |
| Main capsule | 1232 x 706 | Front-page carousel and featured spots *(confirm)* |
| Vertical capsule | 748 x 896 | Sale pages and vertical promo layouts *(confirm)* |

### 1.2 Library assets (confirmed)

| Asset | Size (px) | Rules |
|-------|-----------|-------|
| Library capsule | 600 x 900 | Required. Must include the logo; subtitle allowed; no other text. Steam auto-makes 300x450 |
| Library hero | 3840 x 1240 | Required. Artwork only, **no words at all**. Steam auto-makes 1920x620 |
| Library logo | up to 1280 wide and/or 720 tall | Required. Transparent PNG. Keep the safe area clear of the hero's focal point |
| Library header | 920 x 430 | Optional, falls back to header capsule |

### 1.3 Rules that fail reviews

- Capsule images: logo or title must be present and legible; no review quotes, prices, "discount", "new", awards or platform logos inside the art; PG-13 appropriate (`Graphical Asset Rules`).
- Demo banner: Steam may overlay a "demo" banner on the small and header capsules. Keep the title clear of the banner area *(per a developer forum report, confirm)*.
- Do not put text other than the title/subtitle on capsules.

### 1.4 Other Steam store content

| Asset | Spec | Note |
|-------|------|------|
| Page background | check Steamworks | Optional, subtle dark brass texture |
| Screenshots | at least 5 required to publish *(confirm)*, 1920x1080 recommended | Real in-game captures only. First 4 shown in the carousel. No UI mockups |
| Trailer(s) | 1080p or higher, MP4/WebM via Steamworks | See trailer brief in `launch-plan.md` |
| Capsule text | "Extraordinary Expeditions" subtitle allowed on library capsule | |
| Logo | Vector master of the BRASSLIGHT logo, transparent, light and dark variants | Needed by every store and the press kit |

## 2. Google Play

Official source: [Add preview assets to showcase your app](https://support.google.com/googleplay/android-developer/answer/9866151). Verified 2026-11-08 (search summary of the official page). The page itself was noted as old; confirm in Play Console under Grow > Store presence > Main store listing.

| Asset | Spec |
|-------|------|
| App icon | 512 x 512 px, 32-bit PNG with alpha, max 1024 KB. Google applies its own rounding/shadow: no pre-rounded corners, no drop shadow in the file |
| Feature graphic | 1024 x 500 px, JPEG or 24-bit PNG, **no alpha**. Keep key content in the centre (cropped on some surfaces). Text sparse; no store badges |
| Phone screenshots | Minimum 2 to publish, up to 8 per device type. JPEG or 24-bit PNG, no alpha. Each side 320 to 3840 px; long side at most 2x short side. Google recommends 4 or more at 1080 px minimum: landscape 1920 x 1080 (16:9). **VAMP is landscape-only, so supply 1920 x 1080** |
| Tablet screenshots (7 inch / 10 inch) | Optional but recommended for tablet ranking: same rules, 16:9 landscape, 1920x1080 or higher *(confirm in console)* |
| Promo video | Optional. YouTube URL, public or unlisted, monetization off. Spec for the listing video not confirmed *(confirm)* |
| Alt text | Play Console asks for alt text per graphic asset: prepare short EN and DE alt texts |
| Localization | Upload separate screenshots (with text) for EN and DE |
| Adaptive icon (in the app itself) | Not a store asset; handled by `android-publishing-specialist`: 108dp layers, 66dp safe zone *(confirm)* |

## 3. Apple App Store

Official source: [Screenshot specifications](https://developer.apple.com/help/app-store-connect/reference/app-information/screenshot-specifications/). Verified 2026-11-08 by search summary only; exact pixel lists must be re-read on Apple's page because App Store Connect rejects off-by-one sizes (reported by third-party guides).

| Asset | Spec |
|-------|------|
| App icon | 1024 x 1024 px, no transparency, no pre-rounded corners (Apple masks it). The official icon page was **not** found in this search *(confirm via Xcode asset catalog and Apple Human Interface Guidelines)* |
| iPhone screenshots | At least 1, up to 10 per size, JPEG/PNG. Required set: 6.9-inch display (accepted portrait sizes reported: 1320 x 2868, 1290 x 2796, 1260 x 2736; **landscape is the same pair swapped**, e.g. 2868 x 1320 or 2796 x 1290). Apple's page lists 1290 x 2796 as an example. A 6.5-inch set (1284 x 2778 or 1242 x 2688) is reported as an acceptable alternative *(confirm)*. Smaller device slots are scaled from the largest set *(confirm)* |
| iPad screenshots | Required if the app runs on iPad: 13-inch display, reported sizes 2064 x 2752 or 2048 x 2732 portrait; landscape swapped (e.g. 2752 x 2064). Apple's page example for landscape iPad: 2048 x 1496 *(confirm which sizes are current)* |
| App preview video | Optional, up to 3 per device size. Spec on [App preview specifications](https://developer.apple.com/help/app-store-connect/reference/app-information/app-preview-specifications/) *(not read, confirm)* |
| Product page optimization | Alternate icon and screenshot sets for A/B tests; alternate icons must also be 1024 x 1024 |
| Localization | Separate EN and DE screenshot sets with translated captions |

Decision for the owner: ship iPhone only at launch (smaller screenshot set) or iPhone + iPad (adds 13-inch set). Technical preferences say iOS / iPadOS, so iPad assets are needed unless the iPad is excluded in the Xcode device family.

## 4. Source art deliverables from `art-director`

To cover every store from one master, produce:

1. Logo: vector master + transparent PNG (light and dark).
2. Key art: one master illustration at 4096 x 2160 or larger with clean zones for crop to 920x430, 462x174, 1232x706, 748x896, 600x900, 3840x1240, 1024x500.
3. Icon master 1024 x 1024 (no text, one symbol: gear + lantern + creature silhouette). Test at 48 px.
4. Screenshot capture rig: 1920 x 1080 PC captures, and mobile captures at 2796 x 1290 and 1920 x 1080 (Play).
5. Platform-clean captures: no Steam overlay, no touch joystick on Steam shots; no keyboard prompts on mobile shots.
6. Alt texts (EN/DE) for each image.

## 5. Open items

- Re-read Steamworks main Overview page (main capsule value) and take the official template PSDs.
- Confirm Apple icon spec and the current iPad/iPhone accepted-size lists on Apple pages.
- Confirm the promo video specs on Play and Apple.
