using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MazeOfKnight.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace MazeOfKnight.EditorTools
{
    // Importa el paquete de salas (extracted/unity/<mapa>.json, de tools/extract/unity_bundle.py):
    //   - Assets/Generated/Resources/Maps/<mapa>.asset (MapData) y Assets/Generated/Rooms/<mapa>/room_NNN.asset (RoomData);
    //   - Assets/Generated/Scenes/<mapa>_overview.unity: todas las salas en su rejilla, una capa de Tilemap por
    //     clase de colisión, con los tiles provisionales de Assets/Art/Placeholder.
    // Lo generado sale de la ROM del usuario y no se versiona (ver .gitignore).
    public static class GaliousImporter
    {
        const string GeneratedRoot = "Assets/Generated";
        const string PlaceholderRoot = "Assets/Art/Placeholder";
        // Los mapas van en Resources para que el juego los cargue (Resources.Load<MapData>("Maps/<mapa>")).
        public const string MapsRoot = GeneratedRoot + "/Resources/Maps";

        static string BundleDir => Path.GetFullPath(Path.Combine(Application.dataPath, "../../../extracted/unity"));
        static string RepoLogs => Path.GetFullPath(Path.Combine(Application.dataPath, "../../../logs/unity"));

        static readonly (TileClass cls, string name, Color color)[] Palette =
        {
            (TileClass.Solid, "solid", new Color(0.62f, 0.62f, 0.66f)),
            (TileClass.Ladder, "ladder", new Color(0.85f, 0.65f, 0.25f)),
            (TileClass.Water, "water", new Color(0.20f, 0.40f, 0.95f)),
            (TileClass.Lava, "lava", new Color(0.90f, 0.20f, 0.10f)),
            (TileClass.BossPassable, "boss_passable", new Color(0.35f, 0.35f, 0.40f)),
        };

        [MenuItem("Maze of Knight/Importar salas", false, 0)]
        public static void ImportAll()
        {
            if (!Directory.Exists(BundleDir))
            {
                Debug.LogError($"No encuentro {BundleDir}. Genera el paquete con: python3 tools/extract/unity_bundle.py");
                return;
            }
            EnsurePlaceholderTiles();
            // Solo los mapas (castle, worldNN); enemies.json, weapons.json e items.json son catálogos.
            var files = Directory.GetFiles(BundleDir, "*.json")
                .Where(f => Path.GetFileName(f) == "castle.json" || Path.GetFileName(f).StartsWith("world"))
                .OrderBy(f => f).ToArray();
            // Las carpetas, antes de StartAssetEditing: dentro, IsValidFolder no ve las recién creadas y
            // CreateFolder acabaría haciendo duplicados ("Generated 1"...).
            EnsureFolder($"{MapsRoot}");
            EnsureFolder($"{GeneratedRoot}/Scenes");
            foreach (var file in files)
                EnsureFolder($"{GeneratedRoot}/Rooms/{Path.GetFileNameWithoutExtension(file)}");
            int rooms = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var file in files)
                    rooms += ImportMapData(JsonUtility.FromJson<MapBundle>(File.ReadAllText(file)));
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }
            foreach (var file in files)
                BuildOverviewScene(Path.GetFileNameWithoutExtension(file));
            ImportEnemyCatalog();
            Debug.Log($"[GaliousImporter] {files.Length} mapas, {rooms} salas importadas");
            if (!Application.isBatchMode)
                OpenMap("castle");
        }

        // ---------- abrir las vistas generales ----------

        [MenuItem("Maze of Knight/Abrir mapa/Castillo", false, 100)] static void OpenCastle() => OpenMap("castle");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 1", false, 101)] static void OpenWorld01() => OpenMap("world01");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 2", false, 102)] static void OpenWorld02() => OpenMap("world02");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 3", false, 103)] static void OpenWorld03() => OpenMap("world03");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 4", false, 104)] static void OpenWorld04() => OpenMap("world04");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 5", false, 105)] static void OpenWorld05() => OpenMap("world05");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 6", false, 106)] static void OpenWorld06() => OpenMap("world06");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 7", false, 107)] static void OpenWorld07() => OpenMap("world07");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 8", false, 108)] static void OpenWorld08() => OpenMap("world08");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 9", false, 109)] static void OpenWorld09() => OpenMap("world09");
        [MenuItem("Maze of Knight/Abrir mapa/Mundo 10", false, 110)] static void OpenWorld10() => OpenMap("world10");

        // Abre la vista general de un mapa y encuadra la vista de escena sobre él.
        public static void OpenMap(string mapName)
        {
            string path = $"{GeneratedRoot}/Scenes/{mapName}_overview.unity";
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Maze of Knight",
                    "Todavía no hay salas importadas. Usa Maze of Knight > Importar salas.", "Vale");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var background = GameObject.Find("Fondo");
            var view = SceneView.lastActiveSceneView;
            if (background != null && view != null)
            {
                view.in2DMode = true;
                view.Frame(background.GetComponent<Tilemap>().localBounds, false);
            }
        }

        // ---------- datos ----------

        static int ImportMapData(MapBundle bundle)
        {
            string roomDir = $"{GeneratedRoot}/Rooms/{bundle.map}";

            var map = LoadOrCreate<MapData>($"{MapsRoot}/{bundle.map}.asset");
            map.mapName = bundle.map;
            map.e041 = bundle.e041;
            map.bossRoom = bundle.bossRoom;
            map.bossWord = bundle.bossWord;
            map.rooms.Clear();
            foreach (var model in bundle.rooms)
            {
                var room = LoadOrCreate<RoomData>($"{roomDir}/room_{model.id:000}.asset");
                room.mapName = bundle.map;
                room.room = model;
                EditorUtility.SetDirty(room);
                map.rooms.Add(room);
            }
            EditorUtility.SetDirty(map);
            return bundle.rooms.Length;
        }

        // Catálogo de enemigos en Resources/Enemies.asset.
        static void ImportEnemyCatalog()
        {
            string json = Path.Combine(BundleDir, "enemies.json");
            if (!File.Exists(json))
                return;
            var data = JsonUtility.FromJson<EnemyCatalogJson>(File.ReadAllText(json));
            var catalog = LoadOrCreate<EnemyCatalog>($"{GeneratedRoot}/Resources/Enemies.asset");
            catalog.enemies = data.enemies.ToList();
            catalog.loot = data.loot ?? new LootTable();
            catalog.boxRects = (data.boxRects ?? new BoxRect[0]).ToList();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            // Armas secundarias, inventario y santuarios: el JSON tal cual, como TextAsset (Resources/Weapons, Resources/Items).
            foreach (var name in new[] { "weapons", "items", "shrines", "bosses", "ending" })
            {
                string src = Path.Combine(BundleDir, name + ".json");
                if (!File.Exists(src))
                    continue;
                string dst = $"{GeneratedRoot}/Resources/{char.ToUpper(name[0])}{name.Substring(1)}.json";
                File.Copy(src, Path.GetFullPath(Path.Combine(Application.dataPath, "..", dst)), true);
                AssetDatabase.ImportAsset(dst);
            }
        }

        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // ---------- tiles provisionales (arte propio, se versiona) ----------

        public static Dictionary<TileClass, TileBase> EnsurePlaceholderTiles()
        {
            EnsureFolder(PlaceholderRoot);
            var result = new Dictionary<TileClass, TileBase>();
            foreach (var (cls, name, color) in Palette)
                result[cls] = EnsureTile(name, color);
            result[TileClass.Empty] = EnsureTile("room_background", new Color(0.06f, 0.06f, 0.10f));
            EnsureTile("room_background_alt", new Color(0.10f, 0.10f, 0.16f));
            EnsureTile("shrine", new Color(0.45f, 0.20f, 0.55f));
            EnsureTile("rock", new Color(0.55f, 0.38f, 0.22f));
            EnsureTile("door", new Color(0.30f, 0.75f, 0.45f));
            EnsureTile("key_door", new Color(0.95f, 0.75f, 0.20f));
            EnsureTile("revolving_door", new Color(0.55f, 0.45f, 0.85f));
            return result;
        }

        public static Tile LoadPlaceholder(string name) =>
            AssetDatabase.LoadAssetAtPath<Tile>($"{PlaceholderRoot}/{name}.asset");

        static Tile EnsureTile(string name, Color color)
        {
            string png = $"{PlaceholderRoot}/{name}.png";
            if (!File.Exists(png))
            {
                // 8 x 8 px con un borde un poco más oscuro, para ver la rejilla.
                var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                var edge = Color.Lerp(color, Color.black, 0.25f);
                for (int y = 0; y < 8; y++)
                    for (int x = 0; x < 8; x++)
                        tex.SetPixel(x, y, x == 0 || y == 0 ? edge : color);
                File.WriteAllBytes(png, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(png);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(png);
            if (importer.textureType != TextureImporterType.Sprite || importer.spritePixelsPerUnit != RoomCoords.PixelsPerUnit)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = RoomCoords.PixelsPerUnit;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            string tilePath = $"{PlaceholderRoot}/{name}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, tilePath);
            }
            tile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(png);
            tile.colliderType = Tile.ColliderType.None;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        // ---------- escena de vista general ----------

        static void BuildOverviewScene(string mapName)
        {
            // NewScene descarga los assets sin referencias: cargar mapa y tiles después, no antes.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var map = AssetDatabase.LoadAssetAtPath<MapData>($"{MapsRoot}/{mapName}.asset");
            var tiles = EnsurePlaceholderTiles();

            var grid = new GameObject($"{mapName} (rejilla)").AddComponent<Grid>();
            grid.cellSize = new Vector3(RoomCoords.TileSize, RoomCoords.TileSize, 0);
            var background = NewLayer(grid, "Fondo", 0);
            var layers = Palette.ToDictionary(p => p.cls, p => NewLayer(grid, p.name, 1));

            var bgA = tiles[TileClass.Empty];
            var bgB = LoadPlaceholder("room_background_alt");
            var shrine = LoadPlaceholder("shrine");
            foreach (var data in map.rooms)
            {
                var room = data.room;
                var bg = room.shrine ? shrine : ((room.gridX + room.gridY) & 1) == 0 ? bgA : bgB;
                for (int row = 0; row < RoomCoords.Rows; row++)
                    for (int col = 0; col < RoomCoords.Cols; col++)
                    {
                        var cell = RoomCoords.Cell(room, col, row);
                        background.SetTile(cell, bg);
                        var cls = data.GetTile(col, row);
                        if (cls != TileClass.Empty)
                            layers[cls].SetTile(cell, tiles[cls]);
                    }
            }
            foreach (var tm in grid.GetComponentsInChildren<Tilemap>())
                tm.CompressBounds();

            var bounds = background.localBounds;
            var cam = new GameObject("Main Camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10);
            cam.orthographicSize = bounds.extents.y + 1;

            EditorSceneManager.SaveScene(scene, $"{GeneratedRoot}/Scenes/{mapName}_overview.unity");
        }

        static Tilemap NewLayer(Grid grid, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            var tm = go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder = order;
            return tm;
        }

        // ---------- batch ----------

        // Unity -batchmode -projectPath unity/MazeOfKnight -executeMethod MazeOfKnight.EditorTools.GaliousImporter.ImportAndRenderBatch -quit
        public static void ImportAndRenderBatch()
        {
            try
            {
                ImportAll();
                Directory.CreateDirectory(RepoLogs);
                foreach (var scenePath in Directory.GetFiles($"{GeneratedRoot}/Scenes", "*_overview.unity"))
                    RenderScene(scenePath.Replace('\\', '/'), 2);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorApplication.Exit(1);
            }
        }

        // Renderiza la vista general a logs/unity/<escena>.png con pxPerTile píxeles por tile.
        static void RenderScene(string scenePath, int pxPerTile)
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var background = GameObject.Find("Fondo").GetComponent<Tilemap>();
            var bounds = background.localBounds;
            int w = Mathf.CeilToInt(bounds.size.x / RoomCoords.TileSize) * pxPerTile;
            int h = Mathf.CeilToInt(bounds.size.y / RoomCoords.TileSize) * pxPerTile;
            var cam = Camera.main;
            cam.orthographicSize = bounds.extents.y;
            cam.aspect = (float)w / h;
            cam.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10);

            var rt = new RenderTexture(w, h, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            string png = Path.Combine(RepoLogs, Path.GetFileNameWithoutExtension(scenePath) + ".png");
            File.WriteAllBytes(png, tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);
            Debug.Log($"[GaliousImporter] {png} ({w}x{h})");
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
