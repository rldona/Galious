using System.Collections.Generic;
using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MazeOfKnight.Game.Visuals
{
    // Escenario de Hollow Knight sobre la colisión del original (Assets/Art/Resources/Hollow/escenario, fuera de git; lo
    // importa tools/hollow/escenario.py con las piezas de los Cruces Olvidados). Como en Hollow Knight, la colisión no se
    // ve: el sólido se pinta negro (el interior de la roca) y las piezas visten sus bordes, sin pasarse mucho de ellos
    // para que lo que se ve sea lo que se pisa:
    //  - arriba de cada sólido con aire encima, bordes de suelo, uno tras otro, con el labio 2 px por encima;
    //  - debajo de cada sólido con aire debajo, techos colgando y, a veces, una estalactita si hay sitio;
    //  - en los lados con aire, trozos de pared, apilados;
    //  - detrás, arcos, estatuas y rocas lejanas, apagados, sobre un azul muy oscuro.
    // Fuera de la sala se cuenta como la celda del borde (la sala sigue en la de al lado): sin bordes en los límites.
    // Las piedras rompibles no cuentan como sólido (tienen su dibujo). Todo sale de una semilla por sala: siempre igual.
    public class HollowScenery
    {
        public const float Lip = 2f;                 // px que asoma el suelo por encima de donde se pisa
        public const float Overlap = 0.8f;           // cada pieza empieza al 80 % de la anterior
        static readonly Color BackTop = new Color(0.05f, 0.06f, 0.09f), BackLow = new Color(0.16f, 0.20f, 0.28f);
        static readonly Color Deep = new Color(0.17f, 0.21f, 0.30f, 0.85f);   // rocas del fondo, más lejos que lo de Far
        public static readonly Color Rock = new Color(0.02f, 0.02f, 0.03f);
        static readonly Color Far = new Color(0.36f, 0.43f, 0.58f, 0.7f);
        Sprite gradient;

        readonly Dictionary<string, List<Sprite>> roles = new Dictionary<string, List<Sprite>>();
        readonly Dictionary<RoomModel, GameObject> built = new Dictionary<RoomModel, GameObject>();
        Material material;
        public TileBase FillTile { get; private set; }

        public static HollowScenery Load()
        {
            var all = Resources.LoadAll<Sprite>("Hollow/escenario");
            if (all.Length == 0) return null;
            var s = new HollowScenery();
            foreach (var sp in all)
            {
                int cut = sp.name.IndexOf("__");
                if (cut <= 0) continue;
                string role = sp.name.Substring(0, cut);
                if (!s.roles.TryGetValue(role, out var list)) s.roles[role] = list = new List<Sprite>();
                list.Add(sp);
            }
            foreach (var list in s.roles.Values) list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            s.FillTile = SolidTile(Rock);
            s.material = Resources.Load<Material>("Materials/SpriteLit");
            Debug.Log($"[HollowScenery] {all.Length} piezas de escenario");
            return s;
        }

        static TileBase SolidTile(Color c)
        {
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color[64];
            for (int i = 0; i < 64; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply(false, true);
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8 / RoomCoords.TileSize);
            tile.color = c;
            tile.colliderType = Tile.ColliderType.None;
            return tile;
        }

        // ¿Se viste como roca? Sólido (o pasable solo en el combate), sin contar las piedras rompibles. Fuera de la sala,
        // la celda del borde.
        public static bool Solid(RoomWorld world, RoomModel room, int col, int row)
        {
            col = Mathf.Clamp(col, 0, RoomCoords.Cols - 1);
            row = Mathf.Clamp(row, 0, RoomCoords.Rows - 1);
            var t = world.TileAt(room, col * 8, OriginalPhysics.RoomTopPx + row * 8);
            if (t != TileClass.Solid && t != TileClass.BossPassable) return false;
            foreach (var r in room.rocks)
            {
                int rc = r.x >> 3, rr = (r.y - OriginalPhysics.RoomTopPx) >> 3;
                if (col >= rc && col < rc + 2 && row >= rr && row < rr + 2) return false;
            }
            return true;
        }

        // Rehace el escenario de una sala (al construir el mapa y cuando cambia su colisión).
        public void BuildRoom(RoomWorld world, RoomModel room, Transform parent)
        {
            if (built.TryGetValue(room, out var old) && old != null) Object.Destroy(old);
            var root = new GameObject($"Escenario sala {room.id}");
            root.transform.SetParent(parent, false);
            built[room] = root;
            var rng = new System.Random(room.id * 7919 + 17);
            bool S(int c, int r) => Solid(world, room, c, r);

            Background(room, root.transform, rng, S);
            // Suelos: tramos con aire encima.
            for (int r = 0; r < RoomCoords.Rows; r++)
                foreach (var (c0, c1) in Runs(c => S(c, r) && !S(c, r - 1)))
                    Row(room, root.transform, rng, "suelo", c0 * 8, c1 * 8, OriginalPhysics.RoomTopPx + r * 8 - Lip, -4);
            // Techos: tramos con aire debajo; alguna estalactita si quedan 5 filas libres.
            for (int r = 0; r < RoomCoords.Rows; r++)
                foreach (var (c0, c1) in Runs(c => S(c, r) && !S(c, r + 1)))
                {
                    float y = OriginalPhysics.RoomTopPx + (r + 1) * 8 - 3;
                    Row(room, root.transform, rng, "techo", c0 * 8, c1 * 8, y, -5);
                    if (c1 - c0 >= 3 && rng.Next(3) == 0)
                    {
                        int c = rng.Next(c0 + 1, c1 - 1), free = 0;
                        while (free < 12 && !S(c, r + 1 + free)) free++;
                        if (free >= 5) Hang(room, root.transform, rng, (c * 8 + 4), y, free * 8 * 0.5f);
                    }
                }
            // Paredes: tramos verticales con aire a un lado.
            for (int c = 0; c < RoomCoords.Cols; c++)
            {
                int cc = c;
                foreach (var (r0, r1) in Runs(r => S(cc, r) && !S(cc - 1, r), RoomCoords.Rows))
                    Column(room, root.transform, rng, cc * 8, r0, r1, true);
                foreach (var (r0, r1) in Runs(r => S(cc, r) && !S(cc + 1, r), RoomCoords.Rows))
                    Column(room, root.transform, rng, cc * 8 + 8, r0, r1, false);
            }
        }

        // Sala oscura (sin la Candle): no se ve.
        public void SetVisible(RoomModel room, bool visible)
        {
            if (built.TryGetValue(room, out var go) && go != null) go.SetActive(visible);
        }

        public void Clear()
        {
            foreach (var go in built.Values) if (go != null) Object.Destroy(go);
            built.Clear();
        }

        // Tramos [a, b) donde se cumple la condición.
        static IEnumerable<(int, int)> Runs(System.Func<int, bool> on, int n = RoomCoords.Cols)
        {
            int start = -1;
            for (int i = 0; i <= n; i++)
            {
                bool v = i < n && on(i);
                if (v && start < 0) start = i;
                if (!v && start >= 0) { yield return (start, i); start = -1; }
            }
        }

        Sprite Pick(string role, System.Random rng) =>
            roles.TryGetValue(role, out var l) && l.Count > 0 ? l[rng.Next(l.Count)] : null;

        // Una pieza: su esquina de arriba a la izquierda en (x, y) px del original, a escala k, volteada o no.
        SpriteRenderer Put(RoomModel room, Transform parent, Sprite sp, float x, float y, float k, bool flip, int order, Color color)
        {
            var go = new GameObject(sp.name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sp;
            sr.sortingOrder = order;
            sr.color = color;
            if (material != null) sr.sharedMaterial = material;
            var size = sp.bounds.size * RoomCoords.PixelsPerUnit * k;   // px
            // El pivote está en el centro: el centro de la pieza.
            var at = PixelToWorldF(room, x + size.x / 2, y + size.y / 2);
            go.transform.position = new Vector3(at.x, at.y, 0);
            go.transform.localScale = new Vector3(flip ? -k : k, k, 1);
            return sr;
        }

        static Vector2 PixelToWorldF(RoomModel room, float x, float y) =>
            RoomCoords.RoomOrigin(room) + new Vector2(x / RoomCoords.PixelsPerUnit, (RoomCoords.Rows * 8 - (y - RoomCoords.RoomTopPixel)) / RoomCoords.PixelsPerUnit);

        static Vector2 SizePx(Sprite sp) => sp.bounds.size * RoomCoords.PixelsPerUnit;

        // Una fila de piezas de x0 a x1 con su borde de arriba en y, solapadas, sin pasarse de los extremos más de 2 px.
        void Row(RoomModel room, Transform parent, System.Random rng, string role, float x0, float x1, float y, int order)
        {
            float x = x0 - 2, end = x1 + 2;
            int guard = 0;
            while (x < end - 1 && guard++ < 40)
            {
                var sp = Pick(role, rng);
                if (sp == null) return;
                var size = SizePx(sp);
                float k = 1;
                float span = end - x0 + 2;
                if (size.x > span) k = Mathf.Max(0.5f, span / size.x);
                float w = size.x * k;
                float px = x + w > end ? end - w : x;
                Put(room, parent, sp, px, y, k, rng.Next(2) == 0, order, Color.white);
                x = px + w * Overlap;
                if (px + w >= end) break;
            }
        }

        // Una estalactita colgando de (cx, y), de hasta maxH px de largo.
        void Hang(RoomModel room, Transform parent, System.Random rng, float cx, float y, float maxH)
        {
            var sp = Pick("estalactita", rng);
            if (sp == null) return;
            var size = SizePx(sp);
            float k = Mathf.Min(1, maxH / size.y);
            if (k < 0.3f) return;
            Put(room, parent, sp, cx - size.x * k / 2, y, k, rng.Next(2) == 0, -5, Color.white);
        }

        // Un lado de pared en x (la cara del sólido), de la fila r0 a r1; airLeft: el aire a la izquierda.
        void Column(RoomModel room, Transform parent, System.Random rng, float x, int r0, int r1, bool airLeft)
        {
            float y0 = OriginalPhysics.RoomTopPx + r0 * 8, y1 = OriginalPhysics.RoomTopPx + r1 * 8;
            float y = y0;
            int guard = 0;
            while (y < y1 - 1 && guard++ < 40)
            {
                var sp = airLeft ? Pick("pared_izq", rng) : Pick("pared_der", rng) ?? Pick("pared_izq", rng);
                if (sp == null) return;
                bool mirrored = !airLeft && !sp.name.StartsWith("pared_der");
                var size = SizePx(sp);
                float k = 1, span = y1 - y0;
                if (size.y > span) k = Mathf.Max(0.35f, span / size.y);
                float w = size.x * k, h = size.y * k;
                float py = y + h > y1 ? y1 - h : y;
                // La pared asoma 2 px al aire; el resto, dentro del sólido.
                float px = airLeft ? x - 2 : x + 2 - w;
                Put(room, parent, sp, px, py, k, mirrored, -5, Color.white);
                y = py + h * Overlap;
                if (py + h >= y1) break;
            }
        }

        // Degradado de la sala: oscuro arriba y con niebla azulada abajo, como las cuevas de los Cruces.
        Sprite Gradient()
        {
            if (gradient != null) return gradient;
            const int h = 64;
            var tex = new Texture2D(1, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);                    // 0 abajo, 1 arriba
                tex.SetPixel(0, y, Color.Lerp(BackLow, BackTop, Mathf.SmoothStep(0, 1, t)));
            }
            tex.Apply(false, true);
            // Del tamaño de la sala: 1 texel de ancho = 256 px, 64 de alto = 160 px.
            return gradient = Sprite.Create(tex, new Rect(0, 0, 1, h), new Vector2(0.5f, 0.5f), 1f / RoomCoords.RoomSize.x, 0, SpriteMeshType.FullRect);
        }

        // Fondo: el degradado, 2 rocas grandes muy lejos y 3 o 4 piezas lejanas apagadas, apoyadas en un suelo de la sala
        // (o abajo), donde haya aire.
        void Background(RoomModel room, Transform parent, System.Random rng, System.Func<int, int, bool> solid)
        {
            var g = new GameObject("Degradado").AddComponent<SpriteRenderer>();
            g.transform.SetParent(parent, false);
            g.sprite = Gradient();
            g.sortingOrder = -11;
            var center = RoomCoords.RoomOrigin(room) + RoomCoords.RoomSize / 2;
            g.transform.position = new Vector3(center.x, center.y, 0);
            g.transform.localScale = new Vector3(1, RoomCoords.RoomSize.y / (64 * RoomCoords.RoomSize.x), 1);
            for (int i = 0; i < 2; i++)
            {
                var sp = Pick("masa", rng);
                if (sp == null) break;
                float k = 0.6f + (float)rng.NextDouble() * 0.3f;
                var size = SizePx(sp) * k;
                float x = (i + (float)rng.NextDouble()) * 128 - size.x / 2;
                float y = OriginalPhysics.RoomTopPx + 30 + (float)rng.NextDouble() * 90 - size.y / 2;
                Put(room, parent, sp, x, y, k, rng.Next(2) == 0, -10, Deep);
            }
            int n = 3 + rng.Next(2);
            for (int i = 0; i < n; i++)
            {
                var sp = Pick("fondo", rng);
                if (sp == null) return;
                int c = (int)((i + 0.2f + rng.NextDouble() * 0.6f) * RoomCoords.Cols / n);
                // El primer suelo bajando desde arriba con aire encima en esa columna.
                int r = 1;
                while (r < RoomCoords.Rows && !(solid(c, r) && !solid(c, r - 1))) r++;
                float k = 0.55f + (float)rng.NextDouble() * 0.25f;
                var size = SizePx(sp) * k;
                float floor = OriginalPhysics.RoomTopPx + r * 8;
                Put(room, parent, sp, c * 8 + 4 - size.x / 2, floor - size.y + 2, k, rng.Next(2) == 0, -9, Far);
            }
        }
    }
}
