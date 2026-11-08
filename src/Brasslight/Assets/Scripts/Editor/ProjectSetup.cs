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
            var variants = (MaterialVariant[])System.Enum.GetValues(typeof(MaterialVariant));
            var templates = new Material[variants.Length];
            foreach (var v in variants) templates[(int)v] = CreateTemplate(v);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Fog and flat ambient are set at runtime; saving them in the scene keeps the fog shader
            // variants from being stripped ("Fog Modes: Automatic" only keeps modes used by build scenes).
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.06f, 0.1f, 0.11f);
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 90f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.3f, 0.32f, 0.35f);

            var go = new GameObject("Brasslight");
            var root = go.AddComponent<GameRoot>();
            var so = new SerializedObject(root);
            var arr = so.FindProperty("_materialTemplates");
            arr.arraySize = templates.Length;
            for (int i = 0; i < templates.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = templates[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        private static Material CreateTemplate(MaterialVariant v)
        {
            string path = $"{MaterialDir}/Template_{v}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                MaterialFactory.ConfigureTemplate(existing, v);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            string shader = MaterialFactory.ShaderName(v);
            var s = Shader.Find(shader);
            if (s == null)
            {
                Debug.LogError($"[Brasslight] Shader '{shader}' not found. Is URP installed and the URP asset assigned in Graphics settings?");
                return null;
            }
            var m = new Material(s);
            MaterialFactory.ConfigureTemplate(m, v);
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
        public static void EnableSteam() => SetDefine("BRASSLIGHT_STEAM", true);

        /// <summary>Disables the Steam backend.</summary>
        [MenuItem("Brasslight/Platforms/Disable Steam")]
        public static void DisableSteam() => SetDefine("BRASSLIGHT_STEAM", false);

        private static void SetDefine(string define, bool on)
        {
            var list = new System.Collections.Generic.List<string>(
                PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone).Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
            list.Remove(define);
            if (on) list.Add(define);
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, string.Join(";", list));
        }
    }
}
