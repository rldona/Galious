using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MazeOfKnight.EditorTools
{
    // Compilado del juego: macOS en unity/MazeOfKnight/Builds/Galious.app, Linux y SteamOS en Builds/Linux/Galious.x86_64,
    // Windows en Builds/Windows/Galious.exe y web (WebGL) en Builds/Web, o en la ruta que siga a -salida (las pruebas y
    // capturas, en otra carpeta: recompilar encima del juego mientras se juega rompe la carga de los recursos). Lo usan el
    // menú de Unity y el CI (.github/workflows/builds.yml).
    // Lleva dentro las salas importadas de la ROM y el arte de Hollow Knight: es para el usuario, no para distribuir.
    public static class GameBuild
    {
        public const string Product = "Galious - The Hollow Kingdom";
        public const string Description =
            "Remake de Knightmare II: The Maze of Galious (Konami, 1987) en Unity con el arte de Hollow Knight. Proyecto educativo.";
        const string Output = "Builds/Galious.app";
        const string Icon = "Assets/Art/Icono/icono.png";
        const string SteamArt = "../../assets/steam";        // portada, cubierta, héroe, logo e icono para la biblioteca de Steam

        static string OutputPath(string fallback = Output)
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-salida");
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : fallback;
        }

        [MenuItem("Maze of Knight/Compilar para macOS", false, 40)]
        public static void BuildMac() => Build(BuildTarget.StandaloneOSX, OutputPath());

        // Para SteamOS (Steam Deck, Legion Go) y Linux: x86_64. El plugin de vibración es solo de macOS; en Linux, Rumble usa
        // el Input System.
        [MenuItem("Maze of Knight/Compilar para Linux y SteamOS", false, 41)]
        public static void BuildLinux() => Build(BuildTarget.StandaloneLinux64, OutputPath("Builds/Linux/Galious.x86_64"));

        // Para Windows: x86_64. La vibración, como en Linux, con el Input System.
        [MenuItem("Maze of Knight/Compilar para Windows", false, 42)]
        public static void BuildWindows() => Build(BuildTarget.StandaloneWindows64, OutputPath("Builds/Windows/Galious.exe"));

        // Para el navegador (WebGL): una carpeta con index.html. Comprimida con Brotli y con descompresión en el navegador si
        // el servidor no manda las cabeceras (GitHub Pages, un servidor local).
        [MenuItem("Maze of Knight/Compilar para la web", false, 43)]
        public static void BuildWeb()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.initialMemorySize = 512;
            PlayerSettings.WebGL.maximumMemorySize = 4096;
            Build(BuildTarget.WebGL, OutputPath("Builds/Web"));
        }

        static void Build(BuildTarget target, string path)
        {
            PlayerSettings.productName = Product;
            PlayerSettings.companyName = "rldona";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.rldona.galious");
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(Icon);
            if (icon != null)
            {
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Application);
                PlayerSettings.SetIcons(NamedBuildTarget.Standalone, new[] { icon }, IconKind.Application);
            }
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
            if (report.summary.result == BuildResult.Succeeded) Extras(target, path);
            if (Application.isBatchMode && report.summary.result != BuildResult.Succeeded)
                EditorApplication.Exit(1);
        }

        // Junto al juego: el arte de Steam (para la biblioteca de SteamOS: "Añadir un juego que no es de Steam" y luego
        // Propiedades → Personalizar), en Linux un .desktop con el nombre, la descripción y el icono, y en macOS la
        // descripción en el Info.plist.
        static void Extras(BuildTarget target, string path)
        {
            string dir = target == BuildTarget.WebGL ? path : Path.GetDirectoryName(Path.GetFullPath(path));
            if (target != BuildTarget.WebGL && Directory.Exists(SteamArt))
            {
                string to = Path.Combine(dir, "steam");
                Directory.CreateDirectory(to);
                foreach (var f in Directory.GetFiles(SteamArt, "*.png")) File.Copy(f, Path.Combine(to, Path.GetFileName(f)), true);
            }
            if (target == BuildTarget.StandaloneLinux64)
            {
                string exe = Path.GetFileName(path);
                File.WriteAllText(Path.Combine(dir, "Galious.desktop"),
                    "[Desktop Entry]\nType=Application\nName=Galious: The Hollow Kingdom\n" +
                    $"Comment={Description}\nExec=sh -c 'cd \"$(dirname \"%k\")\" && ./{exe}'\nIcon=steam/icono_1024.png\n" +
                    "Terminal=false\nCategories=Game;ActionGame;\n");
            }
            if (target == BuildTarget.StandaloneOSX)
            {
                string plist = Path.Combine(Path.GetFullPath(path), "Contents/Info.plist");
                if (File.Exists(plist))
                {
                    string s = File.ReadAllText(plist);
                    if (!s.Contains("NSHumanReadableCopyright"))
                        s = s.Replace("</dict>\n</plist>",
                            $"\t<key>NSHumanReadableCopyright</key>\n\t<string>{Description}</string>\n" +
                            "\t<key>LSApplicationCategoryType</key>\n\t<string>public.app-category.action-games</string>\n</dict>\n</plist>");
                    File.WriteAllText(plist, s);
                }
            }
        }
    }
}
