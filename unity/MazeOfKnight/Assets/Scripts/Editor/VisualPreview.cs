using System.IO;
using MazeOfKnight.Game;
using MazeOfKnight.Game.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MazeOfKnight.EditorTools
{
    // Capturas del estilo del remake sin abrir el juego: entra en Play con la escena de juego, pone al jugador en varias
    // salas del castillo con la paleta de cada mundo y guarda logs/unity/visual_<n>.png. En batch:
    //   Unity -batchmode -projectPath unity/MazeOfKnight -executeMethod MazeOfKnight.EditorTools.VisualPreview.CaptureBatch
    // (sin -quit: sale sola al terminar).
    [InitializeOnLoad]
    public static class VisualPreview
    {
        const string Flag = "MazeOfKnight.VisualPreview.Running";
        static readonly (int room, int x, int y, int palette)[] Shots =
        {
            (1, 192, 120, 0), (2, 120, 184, 0), (72, 120, 120, 0), (13, 128, 120, 1), (20, 128, 120, 2), (30, 128, 120, 3),
            (45, 128, 120, 4), (60, 128, 120, 5), (75, 128, 120, 6), (90, 128, 120, 7), (101, 128, 120, 8), (118, 128, 120, 9),
            (130, 128, 120, 10),
        };

        static int frame, shot;

        static string OutDir => Path.GetFullPath(Path.Combine(Application.dataPath, "../../../logs/unity"));

        [MenuItem("Maze of Knight/Capturas del estilo visual", false, 61)]
        public static void CaptureBatch()
        {
            EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath);
            var boot = Object.FindAnyObjectByType<GameBootstrap>();
            if (boot != null) boot.skipAttract = true;
            AttractMode.Current = AttractMode.Mode.Attract;
            SessionState.SetBool(Flag, true);
            EditorApplication.isPlaying = true;
        }

        static VisualPreview()
        {
            if (!SessionState.GetBool(Flag, false)) return;
            frame = shot = 0;
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            var boot = Object.FindAnyObjectByType<GameBootstrap>();
            var atmo = Object.FindAnyObjectByType<Atmosphere>();
            if (boot == null || boot.Motor == null || atmo == null) return;
            frame++;
            if (frame % 40 == 10 && shot < Shots.Length)
            {
                var s = Shots[shot];
                var room = boot.World.Room(s.room);
                if (room != null) boot.Motor.Place(boot.World, room, s.x, s.y);
                atmo.PreviewPalette(s.palette, true);
            }
            if (frame % 40 == 39 && shot < Shots.Length)
            {
                Capture(Camera.main, Path.Combine(OutDir, $"visual_{shot:00}.png"));
                shot++;
            }
            if (shot >= Shots.Length)
            {
                EditorApplication.update -= Tick;
                SessionState.SetBool(Flag, false);
                EditorApplication.isPlaying = false;
                Debug.Log($"[VisualPreview] {Shots.Length} capturas en {OutDir}");
                if (Application.isBatchMode) EditorApplication.delayCall += () => EditorApplication.Exit(0);
            }
        }

        static void Capture(Camera cam, string path)
        {
            const int w = 1280, h = 800;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var old = cam.targetTexture;
            cam.targetTexture = rt;
            cam.aspect = (float)w / h;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            cam.targetTexture = old;
            RenderTexture.active = null;
            cam.ResetAspect();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            rt.Release();
        }
    }
}
