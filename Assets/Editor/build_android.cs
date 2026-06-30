using System;
using System.IO;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityAgentBridge.Editor;

namespace UnityAgentBridge.Editor.CustomTools
{
    // Builds the Refugee Road Android APK from the open editor. BuildPlayer is synchronous and blocks the
    // bridge for the whole build, so the result is ALSO written to Builds/last_build.json - poll it.
    public static class build_android
    {
        [McpTool("build_android", "Build the Refugee Road Android APK to Builds/RefugeeRoad.apk from the open editor. Writes Builds/last_build.json (poll it; the call blocks for the whole build).")]
        public static object Invoke(
            string scene = "Assets/Game/Scenes/Game.unity",
            string output = "Builds/RefugeeRoad.apk",
            bool development = false)
        {
            const string resultPath = "Builds/last_build.json";
            Directory.CreateDirectory("Builds");
            if (File.Exists(resultPath)) File.Delete(resultPath);

            try
            {
                PlayerSettings.companyName = "Crossroads";
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.crossroads.refugeeroad");
                PlayerSettings.productName = "Refugee Road";
                if (string.IsNullOrEmpty(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "0.1")
                    PlayerSettings.bundleVersion = "0.1.0";

                EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);

                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

                var opts = new BuildPlayerOptions
                {
                    scenes = new[] { scene },
                    locationPathName = output,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = development ? BuildOptions.Development : BuildOptions.None,
                };

                BuildReport report = BuildPipeline.BuildPlayer(opts);
                var s = report.summary;
                long size = File.Exists(output) ? new FileInfo(output).Length : 0;
                bool ok = s.result == BuildResult.Succeeded;

                string json = "{"
                    + "\"ok\":" + (ok ? "true" : "false")
                    + ",\"result\":\"" + s.result + "\""
                    + ",\"errors\":" + s.totalErrors
                    + ",\"warnings\":" + s.totalWarnings
                    + ",\"output\":\"" + output + "\""
                    + ",\"sizeBytes\":" + size.ToString(CultureInfo.InvariantCulture)
                    + ",\"package\":\"" + PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) + "\""
                    + "}";
                File.WriteAllText(resultPath, json);
                return new { ok, result = s.result.ToString(), errors = s.totalErrors, output, sizeBytes = size };
            }
            catch (Exception e)
            {
                string json = "{\"ok\":false,\"result\":\"Exception\",\"error\":\""
                    + e.Message.Replace("\\", "/").Replace("\"", "'") + "\"}";
                File.WriteAllText(resultPath, json);
                return new { ok = false, error = e.Message };
            }
        }
    }
}
