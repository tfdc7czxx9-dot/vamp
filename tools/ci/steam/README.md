# Steam upload from CI (SteamPipe)

Rules and click-paths: `production/platforms/steam/checklist.md`.
Verified 2026-11-08 against the game-ci/steam-deploy README (https://github.com/game-ci/steam-deploy)
and Valve's SteamPipe doc (https://partner.steamgames.com/doc/sdk/uploading). Re-check both
before wiring the workflow; action inputs and versions change.

**No credentials in this repo.** Only these CI secrets exist, and only the owner creates them:

| Secret | Content |
|--------|---------|
| `STEAM_USERNAME` | Login of the dedicated builder account (not the owner's account) |
| `STEAM_CONFIG_VDF` | Base64 of the builder's `config.vdf` produced by a local steamcmd login (carries the Steam Guard session) |

Non-secret CI variables: `STEAM_APP_ID`, `STEAM_DEPOT_WINDOWS`, `STEAM_DEPOT_LINUX`, `STEAM_DEPOT_MACOS`
(plus the demo equivalents). The password is never stored in CI when using `STEAM_CONFIG_VDF`.

## Files

| File | Purpose |
|------|---------|
| `app_build.vdf.template` | App build; lists the three depots |
| `depot_build_windows.vdf.template` | Windows depot mapping and exclusions |
| `depot_build_linux.vdf.template` | Linux depot mapping and exclusions |
| `depot_build_macos.vdf.template` | macOS depot mapping and exclusions |

Placeholders: `${APPID}`, `${DEPOTID_WINDOWS}`, `${DEPOTID_LINUX}`, `${DEPOTID_MACOS}`, `${BUILD_DESC}`,
`${SETLIVE_BRANCH}`, `${CONTENT_ROOT}`, `${OUTPUT_DIR}`. Rendered files are build artifacts; never commit them.

## Recommended approach

Two options. Recommendation: **A (game-ci/steam-deploy)** because it matches our GameCI stack and
handles steamcmd install, login with `config.vdf`, and VDF generation.

### A. game-ci/steam-deploy action (recommended)

The action builds its own VDFs from inputs (root path, per-depot paths, branch), so the templates here
are the reference/fallback and the source for the exclusion list. Sketch (check current input names in
the action's `action.yml` before use):

```yaml
- uses: game-ci/steam-deploy@v3   # pin to a commit SHA in the real workflow
  with:
    username: ${{ secrets.STEAM_USERNAME }}
    configVdf: ${{ secrets.STEAM_CONFIG_VDF }}
    appId: ${{ vars.STEAM_APP_ID }}
    buildDescription: ${{ github.sha }}
    rootPath: build            # contains StandaloneWindows64 / StandaloneLinux64 / StandaloneOSX
    depot1Path: StandaloneWindows64
    depot1Id: ${{ vars.STEAM_DEPOT_WINDOWS }}
    depot2Path: StandaloneLinux64
    depot2Id: ${{ vars.STEAM_DEPOT_LINUX }}
    depot3Path: StandaloneOSX
    depot3Id: ${{ vars.STEAM_DEPOT_MACOS }}
    releaseBranch: internal    # never "default" from CI
```

Notes from the action README: use a builder account with only *Edit App Metadata* and
*Publish App Changes To Steam*; reports say the `default` branch cannot be set live by the action, so
promote to public by hand in Steamworks; v3 also supports a `totp` input if a shared secret is used
(we do not, to keep the Steam Guard handling with the owner).

### B. Raw steamcmd (fallback)

```bash
export APPID=... DEPOTID_WINDOWS=... DEPOTID_LINUX=... DEPOTID_MACOS=...
export BUILD_DESC="$GITHUB_SHA" SETLIVE_BRANCH=internal CONTENT_ROOT="$PWD/build" OUTPUT_DIR="$PWD/steam-output"
mkdir -p scripts && for f in app_build depot_build_windows depot_build_linux depot_build_macos; do
  envsubst < tools/ci/steam/$f.vdf.template > scripts/$f.vdf
done
steamcmd +login "$STEAM_USERNAME" +run_app_build "$PWD/scripts/app_build.vdf" +quit
```

`config.vdf` must be restored into steamcmd's config folder first (decode `STEAM_CONFIG_VDF`,
write to `<steamcmd>/config/config.vdf`) so no password or code prompt occurs.
`ContentRoot` must contain `windows/`, `linux/`, `macos/` folders matching the `LocalPath` entries.

## Owner one-time setup (Steam Guard)

1. Create the builder account and grant limited rights (checklist section K).
2. On the owner's machine run steamcmd, log in as the builder, enter the Steam Guard code.
3. Base64-encode `config/config.vdf` (Windows/Linux: relative to the steamcmd folder;
   macOS: `~/Library/Application Support/Steam/config/config.vdf`) and save it as `STEAM_CONFIG_VDF`.
4. If CI starts failing with a Steam Guard / auth error, run `steamcmd +set_steam_guard_code` locally,
   regenerate the file and replace the secret.

## Pre-upload CI assertions (devops-engineer)

- No `steam_appid.txt` in any build folder.
- No `*_BurstDebugInformation_DoNotShip` or `*_BackUpThisFolder_ButDontShipItWithYourGame` folders.
- macOS bundle is signed and notarized; Linux binary is executable.
- Upload only from tagged commits on `main`; branch `internal` by default, `beta` on release candidates,
  `default` only by the owner in Steamworks.
- Never print secrets; mask `STEAM_USERNAME` in logs.
