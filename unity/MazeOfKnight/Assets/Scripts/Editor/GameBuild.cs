using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MazeOfKnight.EditorTools
{
    // Compilado de prueba para macOS en unity/MazeOfKnight/Builds/MazeOfKnight.app (y para Linux y SteamOS, BuildLinux; para Windows, BuildWindows), o en la ruta que siga a -salida (las
    // pruebas y capturas, en otra carpeta: recompilar encima del juego mientras se juega rompe la carga de los recursos).
    // Lleva dentro las salas importadas de la ROM del usuario: es para probar en local, no para distribuir.
    public static class GameBuild
    {
        const string Output = "Builds/MazeOfKnight.app";

        static string OutputPath(string fallback = Output)
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-salida");
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }

        [MenuItem("Maze of Knight/Compilar para macOS", false, 40)]
        public static void BuildMac() => Build(BuildTarget.StandaloneOSX, OutputPath());

        // Para SteamOS (Steam Deck) y Linux: x86_64, en Builds/Linux/MazeOfKnight.x86_64 (o -salida). El plugin de vibración
        // es solo de macOS; en Linux, Rumble usa el Input System.
        [MenuItem("Maze of Knight/Compilar para Linux y SteamOS", false, 41)]
        public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, OutputPath("Builds/Linux/MazeOfKnight.x86_64"));

        // Para Windows: x86_64, en Builds/Windows/MazeOfKnight.exe (o -salida). La vibración, como en Linux, con el Input System.
        [MenuItem("Maze of Knight/Compilar para Windows", false, 42)]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, OutputPath("Builds/Windows/MazeOfKnight.exe"));

        static void Build(BuildTarget target, string path)
        {
            PlayerSettings.productName = "Maze of Knight";
            // A pantalla completa con la resolución nativa (GameSettings la vuelve a poner al arrancar; las capturas, en ventana).
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.macRetinaSupport = true;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 800;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.SplashScreen.show = false;        // sin la pantalla de "Made with Unity" al arrancar (Unity 6 lo permite)
            PlayerSettings.SplashScreen.showUnityLogo = false;
            PlayerSettings.enableFrameTimingStats = true;    // tiempos de procesador y gráfica (Visuals/PerfProbe)
            var options = new BuildPlayerOptions
            {
                scenes = new[] { GameSceneBuilder.ScenePath },
                locationPathName = path,
                target = target,
                options = BuildOptions.None,
            };
            var report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[GameBuild] {target} {report.summary.result} {Path.GetFullPath(path)} " +
                      $"({report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalErrors} errores)");
            if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }
    }
}
