using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MazeOfKnight.EditorTools
{
    // Compilado de prueba para macOS en unity/MazeOfKnight/Builds/MazeOfKnight.app.
    // Lleva dentro las salas importadas de la ROM del usuario: es para probar en local, no para distribuir.
    public static class GameBuild
    {
        const string Output = "Builds/MazeOfKnight.app";

        [MenuItem("Maze of Knight/Compilar para macOS", false, 40)]
        public static void BuildMac()
        {
            PlayerSettings.productName = "Maze of Knight";
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 800;
            PlayerSettings.resizableWindow = true;
            var options = new BuildPlayerOptions
            {
                scenes = new[] { GameSceneBuilder.ScenePath },
                locationPathName = Output,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[GameBuild] {report.summary.result} {Path.GetFullPath(Output)} " +
                      $"({report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalErrors} errores)");
            if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }
    }
}
