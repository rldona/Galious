using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MazeOfKnight.EditorTools
{
    // Compilado de prueba para macOS en unity/MazeOfKnight/Builds/MazeOfKnight.app, o en la ruta que siga a -salida (las
    // pruebas y capturas, en otra carpeta: recompilar encima del juego mientras se juega rompe la carga de los recursos).
    // Lleva dentro las salas importadas de la ROM del usuario: es para probar en local, no para distribuir.
    public static class GameBuild
    {
        const string Output = "Builds/MazeOfKnight.app";

        static string OutputPath()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-salida");
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : Output;
        }

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
                locationPathName = OutputPath(),
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[GameBuild] {report.summary.result} {Path.GetFullPath(OutputPath())} " +
                      $"({report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalErrors} errores)");
            if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }
    }
}
