# Building BRASSLIGHT

This guide takes the repository from a fresh clone to a playable build and then to
store uploads. Store-side steps (accounts, forms, review) are in
`production/platforms/{steam,android,ios}/checklist.md`.

## What is in the repository

| Path | Content |
|------|---------|
| `src/Brasslight/Assets/Scripts/Core` | Engine-independent game simulation (pure C#, no `UnityEngine`) |
| `src/Brasslight/Assets/Scripts/Game` | Unity presentation: rendering, input, HUD/menus, audio, save |
| `src/Brasslight/Assets/Scripts/Platform` | Store abstraction (`IPlatformServices`): offline + Steam |
| `src/Brasslight/Assets/Scripts/Editor` | `Brasslight` menu: project setup and command-line builds |
| `src/Brasslight/Assets/Tests/EditMode` | NUnit tests (run in Unity *and* in plain .NET) |
| `src/Brasslight/Assets/StreamingAssets` | `balance.json` (all tuning values), `loc/en.json`, `loc/de.json` |
| `tests/dotnet` | .NET harness that runs the same tests without Unity |
| `tests/unity-compile-check` | Compiles the Unity layer against reference assemblies (no editor needed) |

The Unity project folders `ProjectSettings/` and `Packages/` are **not** in the repository yet.
They are created once from Unity's official template, as described below, so their package
versions match your installed editor exactly.

## 1. Create the Unity project (one time, about 15 minutes)

1. Install **Unity Hub** and **Unity 6.3 LTS (6000.3.x)** with these modules:
   - Android Build Support, including OpenJDK and the Android SDK & NDK
   - iOS Build Support (on a Mac)
   - Linux Build Support (Mono) and Windows Build Support (Mono)
2. In Unity Hub, choose **New project → Universal 3D**. Name it `BrasslightTemplate` and create
   it anywhere *outside* the repository.
3. Close Unity. Copy these from the template into `src/Brasslight/`:
   - `ProjectSettings/`
   - `Packages/`
   - `Assets/Settings/` (the URP asset, renderer and default volume profile)
4. In Unity Hub, choose **Add → Add project from disk** and select `src/Brasslight`. Open it.
5. **Window → Package Manager**: make sure **Input System** is installed. The Unity 6 templates
   normally include it; if not, install it from the Unity Registry.
6. Run **Brasslight → Setup Project** from the menu. It:
   - sets the product name, landscape-only orientation, Linear colour space and the Input System
     input backend (Unity asks to restart; accept);
   - sets IL2CPP + ARM64 + min API 26 for Android, IL2CPP for iOS, and Mono for desktop;
   - creates the material templates in `Assets/Brasslight/Materials` (so the URP shaders are
     included in builds);
   - creates `Assets/Scenes/Boot.unity` and puts it first in the build settings.
7. Open `Assets/Scenes/Boot.unity` and press **Play**.
8. Commit everything, including the `.meta` files, `ProjectSettings/` and `Packages/`.

> Set your real application identifier before the first store upload. Setup writes the
> placeholder `com.yourname.brasslight` (Player Settings → Other Settings → Identification).

## 2. Run the tests

- **In Unity:** Window → General → Test Runner → EditMode → Run All.
- **Without Unity:** run `dotnet test tests/dotnet/Brasslight.Core.Tests.csproj`.
  This includes a full 15-minute simulated run with an autopilot bot, plus balance probes.
- **Compile check of the Unity layer:** run
  `dotnet build tests/unity-compile-check/UnityCompileCheck.csproj`.

## 3. Builds

Use **Brasslight → Build → …** in the editor, or call the same methods from the command line:

```bash
Unity -batchmode -quit -projectPath src/Brasslight -executeMethod Vamp.EditorTools.BuildScript.BuildAndroid
```

| Target | Method | Output |
|--------|--------|--------|
| Windows | `BuildWindows` | `build/Windows/Brasslight.exe` |
| Linux / Steam Deck | `BuildLinux` | `build/Linux/Brasslight.x86_64` |
| macOS | `BuildMac` | `build/macOS/Brasslight.app` |
| Android (AAB) | `BuildAndroid` | `build/Android/Brasslight.aab` |
| iOS | `BuildIOS` | `build/iOS` (Xcode project; sign and archive on a Mac) |

Environment variables: `BRASSLIGHT_VERSION` (for example `0.1.0`) and `BRASSLIGHT_BUILD_NUMBER`
(Android versionCode / iOS build number).

## 4. Continuous integration (GitHub Actions)

- `.github/workflows/core-tests.yml` runs on every push. It runs the gameplay tests, the Unity
  compile check and the localisation key check. No secrets are needed.
- `.github/workflows/unity-build.yml` runs Unity EditMode tests and builds Windows, Linux,
  Android, macOS and iOS with GameCI. It activates automatically once `src/Brasslight/ProjectSettings`
  exists and these repository secrets are set:

| Secret | Purpose |
|--------|---------|
| `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD` | Unity license activation (see the GameCI docs, "Activation") |
| `ANDROID_KEYSTORE_BASE64`, `ANDROID_KEYSTORE_PASS`, `ANDROID_KEYALIAS_NAME`, `ANDROID_KEYALIAS_PASS` | Upload key for Google Play (`production/platforms/android/checklist.md`) |
| `STEAM_USERNAME`, `STEAM_CONFIG_VDF` | Steam upload (`tools/ci/steam/README.md`) |
| `APPSTORE_ISSUER_ID`, `APPSTORE_KEY_ID`, `APPSTORE_P8` | App Store Connect upload (`production/platforms/ios/checklist.md`) |

Never commit keystores, certificates or `.p8` keys. `.gitignore` blocks the common file types.

## 5. Steam integration

1. Import [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) (MIT) via its UPM git URL.
2. Run **Brasslight → Platforms → Enable Steam**. This adds the define `BRASSLIGHT_STEAM`.
3. For local testing only, put a `steam_appid.txt` containing your App ID next to the executable.
   Never ship it.
4. Achievements: the API names are in `production/platforms/steam/achievements.md`, and the game
   reports them via `AchievementRules`. Create matching entries in Steamworks.
5. Steam Cloud: the game writes `brasslight_save.json` through Remote Storage. Alternatively,
   configure Auto-Cloud for `save.json` in the persistent data path.

## 6. Tuning without code

All gameplay numbers are in `src/Brasslight/Assets/StreamingAssets/balance.json`: enemies,
gadgets, passives, structures, worlds, leaders, research and spawn curves. Edit the file, then run
`dotnet test tests/dotnet` to validate it. The `test_shipped_balance_json_resolves_without_errors`
test catches broken references.

To regenerate the file from the code defaults, run
`BRASSLIGHT_WRITE_BALANCE=1 dotnet test tests/dotnet --filter Name~tool_write`.
