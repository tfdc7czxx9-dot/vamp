using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vamp.Game;

namespace Vamp.EditorTools
{
    /// <summary>
    /// One-click project configuration: player settings for all stores, material templates
    /// (so URP shaders ship in builds) and the Boot scene. Menu: Brasslight > Setup Project.
    /// </summary>
    public static class ProjectSetup
    {
        private const string ScenePath = "Assets/Scenes/Boot.unity";
        private const string MaterialDir = "Assets/Brasslight/Materials";

        /// <summary>Runs all setup steps.</summary>
        [MenuItem("Brasslight/Setup Project", priority = 0)]
        public static void SetupAll()
        {
            ApplyPlayerSettings();
            CreateBootScene();
            Debug.Log("[Brasslight] Setup complete. Press Play in Assets/Scenes/Boot.unity.");
        }

        /// <summary>Applies identity, orientation, input and platform settings (see production/platforms/*).</summary>
        [MenuItem("Brasslight/Apply Player Settings", priority = 1)]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.productName = "BRASSLIGHT";
            if (string.IsNullOrEmpty(PlayerSettings.companyName) || PlayerSettings.companyName == "DefaultCompany")
                PlayerSettings.companyName = "Brasslight";
            if (string.IsNullOrEmpty(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "0.1")
                PlayerSettings.bundleVersion = "0.1.0";

            // Placeholder identifiers: replace "yourname" with the owner's reverse-DNS handle before store upload.
            const string id = "com.yourname.brasslight";
            foreach (var t in new[] { NamedBuildTarget.Android, NamedBuildTarget.iOS, NamedBuildTarget.Standalone })
            {
                string current = PlayerSettings.GetApplicationIdentifier(t);
                if (string.IsNullOrEmpty(current) || current.Contains("DefaultCompany")) PlayerSettings.SetApplicationIdentifier(t, id);
            }

            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.runInBackground = false;

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            // Desktop uses Mono: IL2CPP for Windows/macOS cannot be cross-compiled on Linux CI runners (ADR-0001).
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            SetActiveInputHandlingToInputSystem();
            AssetDatabase.SaveAssets();
            Debug.Log("[Brasslight] Player settings applied. Remember to set your real application identifier.");
        }

        private static void SetActiveInputHandlingToInputSystem()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset");
            if (assets == null || assets.Length == 0) return;
            var so = new SerializedObject(assets[0]);
            var prop = so.FindProperty("activeInputHandler");
            if (prop == null) return;
            if (prop.intValue != 1)
            {
                prop.intValue = 1; // 0 = legacy, 1 = Input System, 2 = both
                so.ApplyModifiedProperties();
                Debug.LogWarning("[Brasslight] Active Input Handling set to Input System. Unity will ask to restart.");
            }
        }

        /// <summary>Creates material templates and the Boot scene with a configured GameRoot.</summary>
        [MenuItem("Brasslight/Create Boot Scene", priority = 2)]
        public static void CreateBootScene()
        {
            EnsureFolder("Assets/Brasslight");
            EnsureFolder(MaterialDir);
            EnsureFolder("Assets/Scenes");
            var lit = CreateMaterial("SpriteLit", "Universal Render Pipeline/Simple Lit");
            var unlit = CreateMaterial("SpriteUnlit", "Universal Render Pipeline/Unlit");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("Brasslight");
            var root = go.AddComponent<GameRoot>();
            var so = new SerializedObject(root);
            so.FindProperty("_litTemplate").objectReferenceValue = lit;
            so.FindProperty("_unlitTemplate").objectReferenceValue = unlit;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        private static Material CreateMaterial(string name, string shader)
        {
            string path = $"{MaterialDir}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var s = Shader.Find(shader);
            if (s == null)
            {
                Debug.LogError($"[Brasslight] Shader '{shader}' not found. Is URP installed and the URP asset assigned in Graphics settings?");
                return null;
            }
            var m = new Material(s) { enableInstancing = true };
            m.SetFloat("_AlphaClip", 1f);
            m.EnableKeyword("_ALPHATEST_ON");
            m.SetFloat("_Cull", 0f);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>Enables the Steam backend (requires Steamworks.NET in the project).</summary>
        [MenuItem("Brasslight/Platforms/Enable Steam (Steamworks.NET installed)")]
        public static void EnableSteam() => PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, "BRASSLIGHT_STEAM");

        /// <summary>Disables the Steam backend.</summary>
        [MenuItem("Brasslight/Platforms/Disable Steam")]
        public static void DisableSteam() => PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, "");
    }
}
