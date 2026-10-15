using System.Collections.Generic;
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

        // Hoja con todos los fotogramas: Popolon sin escudo y con el de oro, Aphrodite sin escudo y con el de plata
        // (logs/unity/personajes.png).
        [MenuItem("Maze of Knight/Hoja de personajes", false, 62)]
        public static void CharacterSheet()
        {
            var art = new CharacterArt();
            var cells = new List<(Character, CharacterArt.Anim, int, int)>();
            foreach (var (c, shield) in new[] { (Character.Popolon, 0), (Character.Popolon, 3), (Character.Aphrodite, 0), (Character.Aphrodite, 2) })
                foreach (var (a, n) in new[] { (CharacterArt.Anim.Idle, 2), (CharacterArt.Anim.Walk, 4), (CharacterArt.Anim.Prep, 1), (CharacterArt.Anim.Rise, 1),
                             (CharacterArt.Anim.Fall, 1), (CharacterArt.Anim.Ladder, 2), (CharacterArt.Anim.Attack, 2), (CharacterArt.Anim.Hurt, 1) })
                    for (int i = 0; i < n; i++) cells.Add((c, a, i, shield));
            var first = art.Get(Character.Popolon, CharacterArt.Anim.Idle, 0).texture;
            int cw = first.width, ch = first.height, cols = 14;
            int rows = cells.Count / cols;
            var sheet = new Texture2D(cw * cols, ch * rows, TextureFormat.RGBA32, false);
            var bg = new Color(0.23f, 0.28f, 0.38f);
            var fill = new Color[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++) fill[i] = bg;
            sheet.SetPixels(fill);
            for (int k = 0; k < cells.Count; k++)
            {
                var (c, a, i, shield) = cells[k];
                var px = art.Get(c, a, i, shield).texture.GetPixels();
                int ox = (k % cols) * cw, oy = (rows - 1 - k / cols) * ch;
                for (int y = 0; y < ch; y++)
                    for (int x = 0; x < cw; x++)
                    {
                        var p = px[y * cw + x];
                        sheet.SetPixel(ox + x, oy + y, Color.Lerp(bg, new Color(p.r, p.g, p.b), p.a));
                    }
            }
            sheet.Apply();
            Directory.CreateDirectory(OutDir);
            File.WriteAllBytes(Path.Combine(OutDir, "personajes.png"), sheet.EncodeToPNG());
            Debug.Log($"[VisualPreview] hoja de personajes en {OutDir}");
        }

        // Hoja con los dibujos de enemigos (los 2 fotogramas de cada arquetipo): logs/unity/enemigos.png.
        [MenuItem("Maze of Knight/Hoja de enemigos", false, 63)]
        public static void EnemySheet()
        {
            var art = new EnemyArt();
            var K = typeof(EnemyArt.Kind);
            var list = new List<(EnemyArt.Kind, EnemyArt.Mod, int)>
            {
                (EnemyArt.Kind.Beetle, EnemyArt.Mod.None, 2), (EnemyArt.Kind.Beetle, EnemyArt.Mod.Horns, 0), (EnemyArt.Kind.Beetle, EnemyArt.Mod.Shield, 8),
                (EnemyArt.Kind.Beetle, EnemyArt.Mod.Spear, 4), (EnemyArt.Kind.Beetle, EnemyArt.Mod.Skeleton | EnemyArt.Mod.Shield, 8),
                (EnemyArt.Kind.Beetle, EnemyArt.Mod.Wide | EnemyArt.Mod.Horns, 3), (EnemyArt.Kind.Worm, EnemyArt.Mod.None, 6),
                (EnemyArt.Kind.Winged, EnemyArt.Mod.None, 3), (EnemyArt.Kind.Fairy, EnemyArt.Mod.None, 8), (EnemyArt.Kind.Wisp, EnemyArt.Mod.None, 4),
                (EnemyArt.Kind.Wisp, EnemyArt.Mod.Head, 7), (EnemyArt.Kind.Orb, EnemyArt.Mod.None, 1), (EnemyArt.Kind.Ball, EnemyArt.Mod.Spiky, 5),
                (EnemyArt.Kind.Ball, EnemyArt.Mod.Shell, 2), (EnemyArt.Kind.Ball, EnemyArt.Mod.None, 7), (EnemyArt.Kind.Ball, EnemyArt.Mod.Rock, 0),
                (EnemyArt.Kind.Ball, EnemyArt.Mod.Bomb, 1), (EnemyArt.Kind.Fish, EnemyArt.Mod.None, 6), (EnemyArt.Kind.Flame, EnemyArt.Mod.None, 0),
                (EnemyArt.Kind.Flame, EnemyArt.Mod.Water, 4), (EnemyArt.Kind.Stone, EnemyArt.Mod.Piston, 2), (EnemyArt.Kind.Stone, EnemyArt.Mod.Mouth, 1),
                (EnemyArt.Kind.Stone, EnemyArt.Mod.Arrow, 5), (EnemyArt.Kind.Stone, EnemyArt.Mod.None, 3), (EnemyArt.Kind.Robe, EnemyArt.Mod.None, 3),
                (EnemyArt.Kind.Spikes, EnemyArt.Mod.None, 0), (EnemyArt.Kind.Bone, EnemyArt.Mod.None, 8), (EnemyArt.Kind.Dart, EnemyArt.Mod.None, 1),
                (EnemyArt.Kind.Plant, EnemyArt.Mod.None, 2), (EnemyArt.Kind.Loot, EnemyArt.Mod.Coins, 5), (EnemyArt.Kind.Loot, EnemyArt.Mod.Arrow, 5),
                (EnemyArt.Kind.Loot, EnemyArt.Mod.Keys, 5), (EnemyArt.Kind.Burst, EnemyArt.Mod.None, 5), (EnemyArt.Kind.Baby, EnemyArt.Mod.None, 7),
            };
            const int cell = 64, cols = 12;
            int n = list.Count * 2, rows = (n + cols - 1) / cols;
            var sheet = new Texture2D(cell * cols, cell * rows, TextureFormat.RGBA32, false);
            var bg = new Color(0.23f, 0.28f, 0.38f);
            var fill = new Color[sheet.width * sheet.height];
            for (int i = 0; i < fill.Length; i++) fill[i] = bg;
            sheet.SetPixels(fill);
            for (int k = 0; k < n; k++)
            {
                var (kind, mod, acc) = list[k / 2];
                var px = art.Get(kind, mod, acc, k & 1).texture.GetPixels();
                int ox = (k % cols) * cell, oy = (rows - 1 - k / cols) * cell;
                for (int y = 0; y < cell; y++)
                    for (int x = 0; x < cell; x++)
                    {
                        var p = px[y * cell + x];
                        sheet.SetPixel(ox + x, oy + y, Color.Lerp(bg, new Color(p.r, p.g, p.b), p.a));
                    }
            }
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(OutDir, "enemigos.png"), sheet.EncodeToPNG());
            Debug.Log($"[VisualPreview] hoja de enemigos en {OutDir}");
        }

        // Hoja de objetos (los 47 iconos) y, debajo, piedra, puertas y ascensor: logs/unity/objetos.png.
        [MenuItem("Maze of Knight/Hoja de objetos", false, 64)]
        public static void PropSheet()
        {
            var art = new PropArt();
            const int cell = 64, cols = 12;
            var sprites = new List<Sprite>();
            for (int t = 1; t <= 0x2F; t++) sprites.Add(art.Item(t));
            int itemRows = (sprites.Count + cols - 1) / cols;
            int w = cell * cols, h = cell * itemRows + 112;
            var sheet = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var bg = new Color(0.23f, 0.28f, 0.38f);
            var fill = new Color[w * h];
            for (int i = 0; i < fill.Length; i++) fill[i] = bg;
            sheet.SetPixels(fill);
            void Blit(Sprite sp, int ox, int oy)
            {
                var r = sp.textureRect;
                var px = sp.texture.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height);
                for (int y = 0; y < (int)r.height; y++)
                    for (int x = 0; x < (int)r.width; x++)
                    {
                        if (ox + x >= w || oy + y >= h) continue;
                        var p = px[y * (int)r.width + x];
                        sheet.SetPixel(ox + x, oy + y, Color.Lerp(sheet.GetPixel(ox + x, oy + y), new Color(p.r, p.g, p.b), p.a));
                    }
            }
            for (int k = 0; k < sprites.Count; k++)
                Blit(sprites[k], (k % cols) * cell, h - (k / cols + 1) * cell);
            void BlitTiles(UnityEngine.Tilemaps.TileBase[,] t, int ox)
            {
                for (int r = 0; r < t.GetLength(0); r++)
                    for (int c = 0; c < t.GetLength(1); c++)
                        Blit(((UnityEngine.Tilemaps.Tile)t[r, c]).sprite, ox + c * 32, 8 + (t.GetLength(0) - 1 - r) * 32);
            }
            BlitTiles(art.Rock, 8);
            BlitTiles(art.Door, 96);
            BlitTiles(art.KeyDoor, 248);
            BlitTiles(art.Revolving, 300);
            Blit(art.Lift, 360, 8);
            sheet.Apply();
            File.WriteAllBytes(Path.Combine(OutDir, "objetos.png"), sheet.EncodeToPNG());
            Debug.Log($"[VisualPreview] hoja de objetos en {OutDir}");
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
