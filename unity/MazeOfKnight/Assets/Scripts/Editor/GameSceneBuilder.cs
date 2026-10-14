using System.IO;
using MazeOfKnight.Data;
using MazeOfKnight.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MazeOfKnight.EditorTools
{
    // Crea Assets/Scenes/Game.unity: cámara por sala, mapa dibujado en tiempo de ejecución y jugador.
    // La escena solo referencia código y arte propio (tiles provisionales), así que se versiona; los datos de
    // las salas los carga GameBootstrap de Resources/Maps al darle a Play.
    public static class GameSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Game.unity";
        const string PlayerSpritePath = "Assets/Art/Placeholder/player.png";
        const string SwordSpritePath = "Assets/Art/Placeholder/sword.png";
        const string ItemSpritePath = "Assets/Art/Placeholder/item.png";
        const string EnemySpritePath = "Assets/Art/Placeholder/enemy.png";

        [MenuItem("Maze of Knight/Crear escena de juego", false, 20)]
        public static void Build()
        {
            GaliousImporter.EnsurePlaceholderTiles();
            var playerSprite = EnsurePlayerSprite();
            EnsureSprite(SwordSpritePath, 11, 2, SpriteAlignment.Center, (x, y) => Color.white);
            // Enemigo: cuadrado blanco de 16 x 16 con borde, pivote abajo a la izquierda (se escala a su caja).
            EnsureSprite(EnemySpritePath, 16, 16, SpriteAlignment.BottomLeft,
                (x, y) => x == 0 || y == 0 || x == 15 || y == 15 ? new Color(0.35f, 0.35f, 0.35f) : Color.white);
            EnsureSprite(ItemSpritePath, 12, 12, SpriteAlignment.BottomCenter,
                (x, y) => System.Math.Abs(x - 5.5f) + System.Math.Abs(y - 5.5f) <= 6 ? new Color(1f, 0.85f, 0.2f) : new Color(0, 0, 0, 0));
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // NewScene descarga los assets sin referencias: los tiles se cargan después.
            playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlayerSpritePath);
            var swordSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SwordSpritePath);
            var itemSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ItemSpritePath);
            var enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>(EnemySpritePath);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = RoomCoords.RoomSize.y / 2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            camGo.transform.position = new Vector3(0, 0, -10);
            var roomCamera = camGo.AddComponent<RoomCamera>();

            var game = new GameObject("Juego");
            var mapView = game.AddComponent<MapView>();
            mapView.background = GaliousImporter.LoadPlaceholder("room_background");
            mapView.backgroundAlt = GaliousImporter.LoadPlaceholder("room_background_alt");
            mapView.shrine = GaliousImporter.LoadPlaceholder("shrine");
            mapView.solid = GaliousImporter.LoadPlaceholder("solid");
            mapView.ladder = GaliousImporter.LoadPlaceholder("ladder");
            mapView.water = GaliousImporter.LoadPlaceholder("water");
            mapView.lava = GaliousImporter.LoadPlaceholder("lava");
            mapView.bossPassable = GaliousImporter.LoadPlaceholder("boss_passable");
            mapView.rock = GaliousImporter.LoadPlaceholder("rock");
            mapView.door = GaliousImporter.LoadPlaceholder("door");
            mapView.keyDoor = GaliousImporter.LoadPlaceholder("key_door");
            mapView.revolvingDoor = GaliousImporter.LoadPlaceholder("revolving_door");
            var bootstrap = game.AddComponent<GameBootstrap>();

            var playerGo = new GameObject("Jugador");
            var body = new GameObject("Cuerpo").AddComponent<SpriteRenderer>();
            body.transform.SetParent(playerGo.transform, false);
            body.sprite = playerSprite;
            body.sortingOrder = 10;
            var sword = new GameObject("Espada").AddComponent<SpriteRenderer>();
            sword.sprite = swordSprite;
            sword.sortingOrder = 11;
            sword.enabled = false;
            var player = playerGo.AddComponent<PlayerController>();
            player.body = body;
            player.sword = sword;

            bootstrap.mapView = mapView;
            bootstrap.player = player;
            bootstrap.itemSprite = itemSprite;
            bootstrap.enemySprite = enemySprite;
            var hud = game.AddComponent<DebugHud>();
            hud.bootstrap = bootstrap;
            hud.player = player;
            roomCamera.player = player;
            // Estilo del remake (luces 2D, capas, niebla, posproceso): necesita URP con el renderer 2D.
            RenderSetup.Setup();
            var atmosphere = camGo.AddComponent<Game.Visuals.Atmosphere>();
            atmosphere.game = bootstrap;
            atmosphere.player = player;
            atmosphere.mapView = mapView;

            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            EditorBuildSettings.scenes = scenes;
            Debug.Log($"[GameSceneBuilder] {ScenePath}");
        }

        static void EnsureSprite(string path, int w, int h, SpriteAlignment pivot, System.Func<int, int, Color> pixel)
        {
            if (!File.Exists(path))
            {
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                        tex.SetPixel(x, y, pixel(x, y));
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = RoomCoords.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        // Sprite provisional de 16 x 16 px con el pivote en los pies (abajo en el centro), blanco para
        // teñirlo por personaje: un cuerpo con casco y una franja de "visera".
        static Sprite EnsurePlayerSprite()
        {
            if (!File.Exists(PlayerSpritePath))
            {
                var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
                var clear = new Color(0, 0, 0, 0);
                for (int y = 0; y < 16; y++)
                    for (int x = 0; x < 16; x++)
                    {
                        bool inside = x >= 3 && x <= 12 && y <= 13;
                        bool edge = inside && (x == 3 || x == 12 || y == 0 || y == 13);
                        bool visor = x >= 7 && x <= 12 && y >= 9 && y <= 10;
                        tex.SetPixel(x, y, !inside ? clear : visor ? Color.black : edge ? new Color(0.75f, 0.75f, 0.75f) : Color.white);
                    }
                File.WriteAllBytes(PlayerSpritePath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(PlayerSpritePath);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(PlayerSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = RoomCoords.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(PlayerSpritePath);
        }

        // Batch: crea la escena y renderiza el primer fotograma (sin entrar en Play) a logs/unity/game_start.png.
        public static void BuildAndRenderBatch()
        {
            try
            {
                Build();
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
                var map = AssetDatabase.LoadAssetAtPath<MapData>($"{GaliousImporter.MapsRoot}/{bootstrap.mapName}.asset");
                var world = new RoomWorld(System.Linq.Enumerable.Select(map.rooms, r => r.room));
                bootstrap.mapView.Build(map, world);
                foreach (var room in world.Rooms)
                    foreach (var r in room.rocks)
                        bootstrap.mapView.ShowRock(room, r.x, r.y, true);
                var motor = new PlayerMotor(world, world.Room(bootstrap.startRoom), bootstrap.startX, bootstrap.startY);
                bootstrap.player.Init(motor);
                var cam = Camera.main;
                cam.aspect = 16f / 10f;
                var rc = cam.GetComponent<RoomCamera>();
                typeof(RoomCamera).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(rc, null);
                typeof(RoomCamera).GetMethod("LateUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(rc, null);
                bootstrap.player.transform.position = RoomCoords.PixelToWorld(motor.Room, motor.X, motor.Y);

                var rt = new RenderTexture(512, 320, 24);
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                var tex = new Texture2D(512, 320, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, 512, 320), 0, 0);
                tex.Apply();
                string png = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../logs/unity/game_start.png"));
                File.WriteAllBytes(png, tex.EncodeToPNG());
                RenderTexture.active = null;
                cam.targetTexture = null;
                Debug.Log($"[GameSceneBuilder] {png}");
                // No guardar: la escena versionada queda tal como la dejó Build().
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }
    }
}
