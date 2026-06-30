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
    // Builds the Refugee Road WebGL player to a folder (index.html) for itch.io browser play. BuildPlayer is
    // synchronous and blocks the bridge for the whole build, so the result is ALSO written to
    // Builds/last_build.json - poll that file even if the call times out.
    public static class build_webgl
    {
        [McpTool("build_webgl", "Build the Refugee Road WebGL player to a folder (default Builds/WebGL) for itch.io browser play. Gzip + decompression fallback so it runs on itch's plain hosting. Writes Builds/last_build.json.")]
        public static object Invoke(
            string scene = "Assets/Game/Scenes/Game.unity",
            string output = "Builds/WebGL",
            bool development = false)
        {
            const string resultPath = "Builds/last_build.json";
            Directory.CreateDirectory("Builds");
            if (File.Exists(resultPath)) File.Delete(resultPath);

            try
            {
                PlayerSettings.companyName = "Crossroads";
                PlayerSettings.productName = "Refugee Road";
                if (string.IsNullOrEmpty(PlayerSettings.bundleVersion) || PlayerSettings.bundleVersion == "0.1")
                    PlayerSettings.bundleVersion = "0.1.0";

                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
                PlayerSettings.WebGL.decompressionFallback = true;
                PlayerSettings.WebGL.nameFilesAsHashes = false;
                PlayerSettings.WebGL.dataCaching = true;
                PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
                PlayerSettings.runInBackground = true;

                EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);

                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

                var opts = new BuildPlayerOptions
                {
                    scenes = new[] { scene },
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    targetGroup = BuildTargetGroup.WebGL,
                    options = development ? BuildOptions.Development : BuildOptions.None,
                };

                BuildReport report = BuildPipeline.BuildPlayer(opts);
                var s = report.summary;
                bool ok = s.result == BuildResult.Succeeded;
                bool hasIndex = File.Exists(Path.Combine(output, "index.html"));

                string json = "{"
                    + "\"ok\":" + (ok ? "true" : "false")
                    + ",\"result\":\"" + s.result + "\""
                    + ",\"errors\":" + s.totalErrors
                    + ",\"warnings\":" + s.totalWarnings
                    + ",\"output\":\"" + output + "\""
                    + ",\"hasIndexHtml\":" + (hasIndex ? "true" : "false")
                    + ",\"sizeBytes\":" + s.totalSize.ToString(CultureInfo.InvariantCulture)
                    + "}";
                File.WriteAllText(resultPath, json);
                return new { ok, result = s.result.ToString(), errors = s.totalErrors, output, hasIndexHtml = hasIndex, sizeBytes = s.totalSize };
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
