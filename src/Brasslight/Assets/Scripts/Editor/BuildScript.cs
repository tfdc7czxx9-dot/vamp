using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Vamp.EditorTools
{
    /// <summary>
    /// Command-line builds for CI (e.g. GameCI buildMethod: Vamp.EditorTools.BuildScript.BuildAndroid).
    /// Output goes to ../../build/&lt;platform&gt; relative to the Unity project. Version from env BRASSLIGHT_VERSION,
    /// build number from env BRASSLIGHT_BUILD_NUMBER (Android versionCode / iOS build).
    /// </summary>
    public static class BuildScript
    {
        [MenuItem("Brasslight/Build/Windows")] public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, "Windows/Brasslight.exe");
        [MenuItem("Brasslight/Build/Linux (Steam Deck)")] public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, "Linux/Brasslight.x86_64");
        [MenuItem("Brasslight/Build/macOS")] public static void BuildMac() => Build(BuildTarget.StandaloneOSX, "macOS/Brasslight.app");
        [MenuItem("Brasslight/Build/Android (AAB)")] public static void BuildAndroid() { EditorUserBuildSettings.buildAppBundle = true; Build(BuildTarget.Android, "Android/Brasslight.aab"); }
        [MenuItem("Brasslight/Build/iOS (Xcode project)")] public static void BuildIOS() => Build(BuildTarget.iOS, "iOS");

        private static void Build(BuildTarget target, string relativeOutput)
        {
            string version = Environment.GetEnvironmentVariable("BRASSLIGHT_VERSION");
            if (!string.IsNullOrEmpty(version)) PlayerSettings.bundleVersion = version;
            if (int.TryParse(Environment.GetEnvironmentVariable("BRASSLIGHT_BUILD_NUMBER"), out int build))
            {
                PlayerSettings.Android.bundleVersionCode = build;
                PlayerSettings.iOS.buildNumber = build.ToString();
            }
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) { ProjectSetup.CreateBootScene(); scenes = new[] { "Assets/Scenes/Boot.unity" }; }
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = System.IO.Path.Combine("..", "..", "build", relativeOutput),
                target = target,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[Build] {target}: {report.summary.result}, {report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalErrors} errors");
            if (Application.isBatchMode) EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
