using System.IO;
using UnityEditor;
using UnityEngine;

namespace MazeOfKnight.EditorTools
{
    // Importa el arte dibujado por nombre (Runtime/Game/Visuals/ArtLibrary.cs; docs/art/encargo/README.md):
    //   - todo PNG bajo Assets/Art/Resources/Dibujado/ es un sprite suelto a 8 texels por píxel del original
    //     (128 texels por unidad), bilineal, sin mipmaps y sin comprimir;
    //   - pivote en los pies (abajo, centro) para personajes, enemigos, jefes y objetos de sala; en el centro para lo demás;
    //   - <nombre>_n.png es el mapa de normales de <nombre>.png: se importa como dato lineal y se engancha como textura
    //     secundaria _NormalMap, que las luces 2D usan si el material es Sprite-Lit.
    public class ArtImporter : AssetPostprocessor
    {
        public const string Root = "Assets/Art/Resources/Dibujado/";
        // Arte de Hollow Knight importado con tools/hollow/import_hollow.py (fuera de git): mismas reglas.
        public const string HollowRoot = "Assets/Art/Resources/Hollow/";
        public const float TexelsPerPixel = 8f;
        public const float PixelsPerUnit = TexelsPerPixel * Data.RoomCoords.PixelsPerUnit;   // 128

        static bool Ours(string path)
        {
            path = path.Replace('\\', '/');
            return (path.StartsWith(Root) || path.StartsWith(HollowRoot)) && path.EndsWith(".png");
        }
        static bool IsNormal(string path) => Path.GetFileNameWithoutExtension(path).EndsWith("_n");

        // Carpetas cuyo pivote va en los pies.
        static bool FeetPivot(string path)
        {
            string folder = Path.GetFileName(Path.GetDirectoryName(path)).ToLowerInvariant();
            return folder is "personajes" or "enemigos" or "jefes" or "objetos_sala" or "bancos" or "cornifer";
        }

        void OnPreprocessTexture()
        {
            if (!Ours(assetPath)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = !IsNormal(assetPath);
            importer.sRGBTexture = !IsNormal(assetPath);
            importer.wrapMode = TextureWrapMode.Clamp;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)(FeetPivot(assetPath) ? SpriteAlignment.BottomCenter : SpriteAlignment.Center);
            importer.SetTextureSettings(settings);
            if (!IsNormal(assetPath))
            {
                string normalPath = Path.ChangeExtension(assetPath, null) + "_n.png";
                var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                importer.secondarySpriteTextures = normal != null
                    ? new[] { new SecondarySpriteTexture { name = "_NormalMap", texture = normal } }
                    : new SecondarySpriteTexture[0];
            }
        }

        // Al llegar un mapa de normales, reimporta su sprite para que lo enganche.
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (var path in imported)
            {
                if (!Ours(path) || !IsNormal(path)) continue;
                string sprite = path.Substring(0, path.Length - "_n.png".Length) + ".png";
                if (File.Exists(sprite)) AssetDatabase.ImportAsset(sprite);
            }
        }

        [MenuItem("Maze of Knight/Reimportar arte dibujado", false, 66)]
        public static void Reimport()
        {
            if (Directory.Exists(Root)) AssetDatabase.ImportAsset(Root.TrimEnd('/'), ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            var lib = Game.Visuals.ArtLibrary.Load();
            Debug.Log($"[ArtImporter] {lib.Count} clips dibujados: {string.Join(", ", lib.Names)}");
        }
    }
}
