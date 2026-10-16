using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game.Visuals
{
    // F10: la rejilla de colisión del original encima de la sala, para el encargo de arte (docs/art/encargo/README.md):
    // las 32 × 20 celdas de 8 px, cada clase con su tinte (macizo, escalera, agua, lava, pasable en el combate), una
    // línea más fuerte cada 4 tiles (32 px) y la caja de contacto del jugador (13 × 13 px) en verde. Va en el mundo, no
    // en OnGUI, así sale también en las capturas a textura (VisualPreview).
    public class CollisionGridOverlay : MonoBehaviour
    {
        public GameBootstrap game;
        // Oculta por defecto (lo pidió el usuario); F10 la enseña: el sólido en rojo, como en la colisión de las salas de
        // Hollow Knight.
        public static bool Show;

        const int T = 32;                      // texels por tile (4 por píxel del original)
        SpriteRenderer grid, playerBox;
        RoomModel shown;
        int shownHash;
        Material material;

        public void Init(Material unlit) => material = unlit;

        void Update()
        {
            if (RemakeRules.DebugKeys && Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
            {
                Show = !Show;
                game?.Say(Show ? "Rejilla de colisión" : "Rejilla quitada");
            }
        }

        void LateUpdate()
        {
            bool on = Show && game != null && game.Motor != null && !game.AttractRunning && !game.EndingRunning;
            if (grid != null) grid.enabled = on;
            if (playerBox != null) playerBox.enabled = on;
            if (!on) return;
            if (grid == null)
            {
                grid = NewRenderer("Rejilla de colisión", 25);  // encima del escenario y del agua y la lava (20…24)
                playerBox = NewRenderer("Caja del jugador", 999);
                playerBox.sprite = BoxSprite();
            }
            var room = game.Motor.Room;
            int hash = AmbientOcclusion.Hash((c, r) => game.World.TileAt(room, c * 8 + 4, OriginalPhysics.RoomTopPx + r * 8 + 4) != TileClass.Empty, RoomCoords.Cols, RoomCoords.Rows);
            if (room != shown || hash != shownHash)
            {
                shown = room;
                shownHash = hash;
                if (grid.sprite != null) Destroy(grid.sprite.texture);
                grid.sprite = GridSprite(room);
                var o = RoomCoords.RoomOrigin(room);
                grid.transform.position = new Vector3(o.x, o.y, 0);
            }
            var m = game.Motor;
            var a = RoomCoords.PixelToWorld(room, m.X + OriginalPhysics.ContactLeft, m.Y + OriginalPhysics.ContactBottom + 1);
            playerBox.transform.position = new Vector3(a.x, a.y, 0);
        }

        SpriteRenderer NewRenderer(string name, int order)
        {
            // Suelto en el mundo: el objeto del ambiente se mueve con la cámara.
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            if (material != null) sr.sharedMaterial = material;
            return sr;
        }

        static Color Tint(TileClass c) => c switch
        {
            TileClass.Solid => new Color(0.85f, 0.08f, 0.12f, 0.3f),
            TileClass.Ladder => new Color(1f, 0.9f, 0.3f, 0.3f),
            TileClass.Water => new Color(0.3f, 0.9f, 1f, 0.25f),
            TileClass.Lava => new Color(1f, 0.5f, 0.2f, 0.3f),
            TileClass.BossPassable => new Color(1f, 0.4f, 1f, 0.25f),
            _ => Color.clear,
        };

        Sprite GridSprite(RoomModel room)
        {
            int w = RoomCoords.Cols * T, h = RoomCoords.Rows * T;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[w * h];
            var line = new Color(0.35f, 0f, 0.04f, 0.45f);
            var strong = new Color(0.25f, 0f, 0.03f, 0.7f);
            for (int r = 0; r < RoomCoords.Rows; r++)
                for (int c = 0; c < RoomCoords.Cols; c++)
                {
                    var tint = Tint(game.World.TileAt(room, c * 8 + 4, OriginalPhysics.RoomTopPx + r * 8 + 4));
                    int ox = c * T, oy = (RoomCoords.Rows - 1 - r) * T;
                    for (int y = 0; y < T; y++)
                        for (int x = 0; x < T; x++)
                        {
                            bool edge = x == 0 || y == 0;
                            bool big = x == 0 && c % 4 == 0 || y == 0 && (RoomCoords.Rows - r) % 4 == 0;
                            // La rejilla, solo en las celdas que no son aire (como la colisión marcada de Hollow Knight).
                            px[(oy + y) * w + ox + x] = tint.a <= 0 ? Color.clear : edge ? (big ? strong : line) : tint;
                        }
                }
            tex.SetPixels(px);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.zero, T / RoomCoords.TileSize, 0, SpriteMeshType.FullRect);
        }

        // Caja de contacto del jugador: 13 × 13 px (x − 6 … x + 6, y − 14 … y − 2), contorno verde de 1 px.
        static Sprite BoxSprite()
        {
            const int s = 4;
            int w = (OriginalPhysics.ContactRight - OriginalPhysics.ContactLeft + 1) * s;
            int h = (OriginalPhysics.ContactBottom - OriginalPhysics.ContactTop + 1) * s;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    px[y * w + x] = x < s || y < s || x >= w - s || y >= h - s ? new Color(0.2f, 1f, 0.3f, 0.9f) : new Color(0.2f, 1f, 0.3f, 0.12f);
            tex.SetPixels(px);
            tex.Apply(false, false);
            return Sprite.Create(tex, new Rect(0, 0, w, h), Vector2.zero, s * RoomCoords.PixelsPerUnit, 0, SpriteMeshType.FullRect);
        }
    }
}
