using System.Collections.Generic;
using System.Linq;
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
        public const float TechoMax = 12f;
        public const float SueloMax = 9f;            // y un suelo (los escombros de los Canales son gruesos)           // px que cuelga un techo como mucho (lo largo, las estalactitas)
        // Capas, de atrás adelante (como Team Cherry): 4, el fondo lejano (un trozo de una sala de Hollow Knight, apagado);
        // 3, el degradado con niebla, rocas grandes y arcos y estatuas; 2, el juego (el sólido y sus bordes); 1, siluetas
        // negras por delante de los personajes, asomando por los bordes de la sala.
        public const int FarOrder = -13, ForeOrder = 30;
        static readonly Color Silhouette = new Color(0.008f, 0.008f, 0.014f);
        public static readonly Color Rock = new Color(0.02f, 0.02f, 0.03f);

        // Zona de Hollow Knight de cada mapa (su kit en Hollow/escenario/<zona>) y sus colores: el tinte del fondo lejano,
        // el degradado (arriba, abajo con niebla), las rocas grandes del fondo (Deep) y los arcos y estatuas (Far).
        public class Look
        {
            public string Zone;
            public Color FarTint, BackTop, BackLow, Deep, Far;
            public int DetailEvery = 6;     // un adorno de cada tantas casillas de suelo
        }
        static Look L(string zone, string farTint, string top, float topA, string low, float lowA, string deep, string far) => new Look
        {
            Zone = zone, FarTint = Hex(farTint, 1), BackTop = Hex(top, topA), BackLow = Hex(low, lowA), Deep = Hex(deep, 0.45f), Far = Hex(far, 0.7f),
        };
        static Look With(Look l, int detailEvery) { l.DetailEvery = detailEvery; return l; }
        static Color Hex(string h, float a) { ColorUtility.TryParseHtmlString(h, out var c); c.a = a; return c; }
        // Índice 0 = castillo, 1–10 = mundos (el reparto acordado con el usuario el 16 de octubre de 2026). Sin su kit
        // importado, una zona usa la de los Cruces.
        public static readonly Look[] Looks =
        {
            L("cruces", "#6b7899", "#0a0d14", 0.8f, "#29334a", 0.45f, "#2b354d", "#5c6e94"),      // castillo
            With(L("greenpath", "#7a9993", "#08100f", 0.8f, "#1f3d38", 0.45f, "#1f3833", "#527a73"), 4),   // 1 cueva verde agua (hierba)
            With(L("canales", "#6b8a99", "#070d12", 0.8f, "#1f3542", 0.45f, "#1d313d", "#4f7385"), 16),   // 2 arrecife (tarros, pocos)
            With(L("colmena", "#99876b", "#120d06", 0.8f, "#4a3a1f", 0.45f, "#3d301b", "#8a7449"), 14),   // 3 guarida ámbar
            With(L("cristal", "#8a7a99", "#0e0914", 0.8f, "#3a2a4d", 0.45f, "#30243f", "#73609a"), 10),   // 4 cristal violeta
            L("ciudad", "#6b7d99", "#070b14", 0.8f, "#1f2d4a", 0.45f, "#1d2840", "#4f6494"),      // 5 ruinas hundidas
            With(L("fungicos", "#7f97ab", "#060b10", 0.8f, "#1f364a", 0.45f, "#1c2f40", "#55748f"), 8),   // 6 bosque de hongos (azul, como los Páramos)
            With(L("abismo", "#5a6280", "#030408", 0.85f, "#141a2e", 0.45f, "#121726", "#3d4a73"), 18),   // 7 abismo luminoso
            With(L("nido", "#7a7f94", "#06070a", 0.8f, "#2a2e3d", 0.5f, "#222633", "#5a6075"), 16),   // 8 magma (el Nido Profundo)
            L("confines", "#8c8c8c", "#0d0d0d", 0.8f, "#3a3a3a", 0.45f, "#2e2e2e", "#7a7a7a"),    // 9 osario
            With(L("palacio", "#a3a3b3", "#0f0f14", 0.75f, "#4d4d5c", 0.45f, "#3d3d4a", "#9494a8"), 14),   // 10 trono carmesí (el Palacio Blanco)
        };
        public static Look LookFor(string mapName)
        {
            int i = mapName != null && mapName.StartsWith("world") && int.TryParse(mapName.Substring(5), out int w) ? w : 0;
            return Looks[Mathf.Clamp(i, 0, Looks.Length - 1)];
        }

        Look look = Looks[0];
        Dictionary<string, List<Sprite>> roles = new Dictionary<string, List<Sprite>>();
        readonly Dictionary<string, Dictionary<string, List<Sprite>>> kits = new Dictionary<string, Dictionary<string, List<Sprite>>>();

        // Cambia de zona (al construir un mapa): su kit, o el de los Cruces si no está importado.
        public void UseMap(string mapName)
        {
            look = LookFor(mapName);
            roles = Kit(look.Zone);
            if (roles.Count == 0) roles = Kit(Looks[0].Zone);
        }

        Dictionary<string, List<Sprite>> Kit(string zone)
        {
            if (kits.TryGetValue(zone, out var k)) return k;
            k = new Dictionary<string, List<Sprite>>();
            foreach (var sp in Resources.LoadAll<Sprite>($"Hollow/escenario/{zone}"))
            {
                int cut = sp.name.IndexOf("__");
                if (cut <= 0) continue;
                string role = sp.name.Substring(0, cut);
                if (!k.TryGetValue(role, out var list)) k[role] = list = new List<Sprite>();
                list.Add(sp);
            }
            foreach (var list in k.Values) list.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            if (k.Count > 0) Debug.Log($"[HollowScenery] zona {zone}: {k.Values.Sum(l => l.Count)} piezas");
            return kits[zone] = k;
        }
        Sprite gradient;
        string gradientZone;
        readonly Dictionary<RoomModel, GameObject> built = new Dictionary<RoomModel, GameObject>();
        Material material;
        public TileBase FillTile { get; private set; }
        // Cuerpo del agua y de la lava (HollowLiquids pone la superficie y lo que se mueve), por profundidad: la fila de
        // arriba más clara y cada una más oscura hasta la cuarta.
        public const int LiquidDepths = 4;
        TileBase[] water, lava;
        public TileBase Liquid(bool isLava, int depth) => (isLava ? lava : water)[Mathf.Clamp(depth, 0, LiquidDepths - 1)];

        public static HollowScenery Load()
        {
            var s = new HollowScenery();
            if (s.Kit(Looks[0].Zone).Count == 0) return null;
            s.UseMap(null);
            s.FillTile = SolidTile(Rock);
            s.water = new TileBase[LiquidDepths];
            s.lava = new TileBase[LiquidDepths];
            for (int d = 0; d < LiquidDepths; d++)
            {
                float t = d / (float)(LiquidDepths - 1);
                s.water[d] = SolidTile(Color.Lerp(new Color(0.22f, 0.36f, 0.55f, 0.6f), new Color(0.03f, 0.06f, 0.13f, 0.85f), t));
                s.lava[d] = SolidTile(Color.Lerp(new Color(1f, 0.52f, 0.14f, 0.95f), new Color(0.3f, 0.04f, 0.03f, 0.97f), Mathf.Sqrt(t)));
            }
            s.material = Resources.Load<Material>("Materials/SpriteLit");
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
                {
                    float surface = OriginalPhysics.RoomTopPx + r * 8;
                    Row(room, root.transform, rng, "suelo", c0 * 8, c1 * 8, surface - Lip, -4, SueloMax);
                    // Adornos encima (hierba, huesos, tarros): uno de cada Look.DetailEvery casillas, de 12 px de alto como mucho.
                    for (int c = c0; c < c1; c++)
                        if (rng.Next(look.DetailEvery) == 0 && Pick("detalle", rng) is Sprite d)
                        {
                            var size = SizePx(d);
                            float k = Mathf.Min(1, 12f / size.y);
                            Put(room, root.transform, d, c * 8 + 4 - size.x * k / 2, surface - Lip + 1 - size.y * k, k, rng.Next(2) == 0, -4, Color.white);
                        }
                }
            // Techos: tramos con aire debajo; alguna estalactita si quedan 5 filas libres.
            for (int r = 0; r < RoomCoords.Rows; r++)
                foreach (var (c0, c1) in Runs(c => S(c, r) && !S(c, r + 1)))
                {
                    float y = OriginalPhysics.RoomTopPx + (r + 1) * 8 - 3;
                    Row(room, root.transform, rng, "techo", c0 * 8, c1 * 8, y, -5, TechoMax);
                    if (c1 - c0 >= 3 && rng.Next(3) == 0)
                    {
                        int c = rng.Next(c0 + 1, c1 - 1), free = 0;
                        while (free < 12 && !S(c, r + 1 + free)) free++;
                        if (free >= 5) Hang(room, root.transform, rng, (c * 8 + 4), y, free * 8 * 0.5f);
                    }
                }
            Foreground(room, root.transform, rng, S);
            // Paredes: tramos verticales con aire a un lado.
            for (int c = 0; c < RoomCoords.Cols; c++)
            {
                int cc = c;
                foreach (var (r0, r1) in Runs(r => S(cc, r) && !S(cc - 1, r), RoomCoords.Rows))
                    Column(room, root.transform, rng, cc * 8, r0, r1, true, Thickness(S, cc, r0, r1, 1));
                foreach (var (r0, r1) in Runs(r => S(cc, r) && !S(cc + 1, r), RoomCoords.Rows))
                    Column(room, root.transform, rng, cc * 8 + 8, r0, r1, false, Thickness(S, cc, r0, r1, -1));
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
        // maxH: alto máximo (los techos de enredaderas de Greenpath son cortinas largas).
        void Row(RoomModel room, Transform parent, System.Random rng, string role, float x0, float x1, float y, int order, float maxH = 999)
        {
            float x = x0 - 2, end = x1 + 2;
            int guard = 0;
            while (x < end - 1 && guard++ < 40)
            {
                var sp = Pick(role, rng);
                if (sp == null) return;
                var size = SizePx(sp);
                float k = Mathf.Min(1, maxH / size.y);
                float span = end - x0 + 2;
                if (size.x * k > span) k = Mathf.Max(0.5f * k, span / size.x);
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
        // Grosor en px del sólido desde la columna c hacia dentro (dir +1 a la derecha, −1 a la izquierda), el menor de las
        // filas r0…r1: una pared no puede ser más ancha (asomaría por el otro lado de un muro fino).
        static float Thickness(System.Func<int, int, bool> solid, int c, int r0, int r1, int dir)
        {
            int min = RoomCoords.Cols;
            for (int r = r0; r < r1; r++)
            {
                int n = 0;
                while (n < RoomCoords.Cols && c + dir * n >= 0 && c + dir * n < RoomCoords.Cols && solid(c + dir * n, r)) n++;
                if (c + dir * n < 0 || c + dir * n >= RoomCoords.Cols) n = RoomCoords.Cols;   // sigue en la sala de al lado
                min = Mathf.Min(min, n);
            }
            return min * 8;
        }

        void Column(RoomModel room, Transform parent, System.Random rng, float x, int r0, int r1, bool airLeft, float thick)
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
                if (size.x * k > thick + 4) k = Mathf.Max(0.35f, (thick + 4) / size.x);
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
            if (gradient != null && gradientZone == look.Zone) return gradient;
            gradientZone = look.Zone;
            const int h = 64;
            var tex = new Texture2D(1, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);                    // 0 abajo, 1 arriba
                tex.SetPixel(0, y, Color.Lerp(look.BackLow, look.BackTop, Mathf.SmoothStep(0, 1, t)));
            }
            tex.Apply(false, true);
            // Del tamaño de la sala: 1 texel de ancho = 256 px, 64 de alto = 160 px.
            return gradient = Sprite.Create(tex, new Rect(0, 0, 1, h), new Vector2(0.5f, 0.5f), 1f / RoomCoords.RoomSize.x, 0, SpriteMeshType.FullRect);
        }

        // Primer plano: 1 o 2 siluetas negras desde el borde de abajo y quizá 1 colgando del de arriba, más cerca (más
        // grandes) que el juego. Asoman al aire como mucho Poke px desde el primer suelo (o techo) de su columna, y no
        // salen de la sala (la de al lado se vería por debajo).
        const float Poke = 10f, PokeUp = 18f;   // por abajo poco, que no tape a los que andan; colgando, más
        void Foreground(RoomModel room, Transform parent, System.Random rng, System.Func<int, int, bool> solid)
        {
            float top = OriginalPhysics.RoomTopPx, bottom = top + RoomCoords.Rows * 8;
            int below = 1 + rng.Next(2);
            for (int i = 0; i < below; i++)
            {
                var sp = Pick("pp_abajo", rng);
                if (sp == null) break;
                int c = below == 1 ? rng.Next(4, RoomCoords.Cols - 4) : i == 0 ? rng.Next(0, 10) : rng.Next(RoomCoords.Cols - 10, RoomCoords.Cols);
                int r = RoomCoords.Rows - 1;
                while (r > 0 && solid(c, r)) r--;              // el aire más bajo de la columna
                if (r < RoomCoords.Rows - 8) continue;           // suelo muy alto: taparía el juego
                float floor = top + (r + 1) * 8;
                float poke = 4 + (float)rng.NextDouble() * (Poke - 4);
                float k = 1.5f + (float)rng.NextDouble() * 0.5f;
                var size = SizePx(sp) * k;
                // A lo ancho, dentro del suelo donde se apoya (más 4 px por lado): no sobresale en el aire.
                int left = c, right = c;
                while (left > 0 && solid(left - 1, r + 1) && !solid(left - 1, r)) left--;
                while (right < RoomCoords.Cols - 1 && solid(right + 1, r + 1) && !solid(right + 1, r)) right++;
                float runW = (right - left + 1) * 8 + 8;
                if (size.x > runW) { k *= runW / size.x; size = SizePx(sp) * k; }
                int half = Mathf.CeilToInt(size.x / 16);
                c = Mathf.Clamp(c, left + half - 1, right - half + 1);
                // A lo alto, solo la roca de debajo de ese suelo, la más fina de las columnas que cubre (si hay aire más
                // abajo, no se mete en él).
                float limit = bottom;
                for (int cc = Mathf.Max(0, c - half); cc <= Mathf.Min(RoomCoords.Cols - 1, c + half); cc++)
                {
                    int depth = 0;
                    while (r + 1 + depth < RoomCoords.Rows && solid(cc, r + 1 + depth)) depth++;
                    if (r + 1 + depth < RoomCoords.Rows) limit = Mathf.Min(limit, floor + depth * 8);
                }
                if (size.y > limit - (floor - poke)) { k *= (limit - (floor - poke)) / size.y; size = SizePx(sp) * k; }
                if (k < 1f) continue;                            // no cabe: más pequeño ya no parecería más cerca
                Put(room, parent, sp, c * 8 + 4 - size.x / 2, floor - poke, k, rng.Next(2) == 0, ForeOrder, Silhouette);
            }
            if (rng.Next(2) == 0 && Pick("pp_arriba", rng) is Sprite hang)
            {
                int c = rng.Next(0, RoomCoords.Cols);
                int r = 0;
                while (r < RoomCoords.Rows - 1 && solid(c, r)) r++;   // el aire más alto
                if (r <= 6)
                {
                    float ceiling = top + r * 8;
                    float poke = 8 + (float)rng.NextDouble() * (PokeUp - 8);
                    float k = 1.4f + (float)rng.NextDouble() * 0.4f;
                    var size = SizePx(hang) * k;
                    // Igual que abajo: a lo ancho, dentro del techo donde cuelga; a lo alto, la roca más fina de encima.
                    int left = c, right = c;
                    while (left > 0 && solid(left - 1, r - 1) && !solid(left - 1, r)) left--;
                    while (right < RoomCoords.Cols - 1 && solid(right + 1, r - 1) && !solid(right + 1, r)) right++;
                    if (r == 0) { left = 0; right = RoomCoords.Cols - 1; }
                    float runW = (right - left + 1) * 8 + 8;
                    if (size.x > runW) { k *= runW / size.x; size = SizePx(hang) * k; }
                    int half = Mathf.CeilToInt(size.x / 16);
                    c = Mathf.Clamp(c, left + half - 1, right - half + 1);
                    float limit = top;
                    for (int cc = Mathf.Max(0, c - half); cc <= Mathf.Min(RoomCoords.Cols - 1, c + half); cc++)
                    {
                        int depth = 0;
                        while (r - 1 - depth >= 0 && solid(cc, r - 1 - depth)) depth++;
                        if (r - 1 - depth >= 0) limit = Mathf.Max(limit, ceiling - depth * 8);
                    }
                    if (size.y > ceiling + poke - limit) { k *= (ceiling + poke - limit) / size.y; size = SizePx(hang) * k; }
                    if (size.y > 4) Put(room, parent, hang, c * 8 + 4 - size.x / 2, ceiling + poke - size.y, k, rng.Next(2) == 0, ForeOrder, Silhouette);
                }
            }
        }

        // Fondo: el degradado, 2 rocas grandes muy lejos y 3 o 4 piezas lejanas apagadas, apoyadas en un suelo de la sala
        // (o abajo), donde haya aire.
        void Background(RoomModel room, Transform parent, System.Random rng, System.Func<int, int, bool> solid)
        {
            if (Pick("lejano", rng) is Sprite far)
            {
                var f = new GameObject(far.name).AddComponent<SpriteRenderer>();
                f.transform.SetParent(parent, false);
                f.sprite = far;
                f.sortingOrder = FarOrder;
                f.color = look.FarTint;
                var c = RoomCoords.RoomOrigin(room) + RoomCoords.RoomSize / 2;
                f.transform.position = new Vector3(c.x, c.y, 0);
                float k = RoomCoords.RoomSize.x / far.bounds.size.x;
                f.transform.localScale = new Vector3(rng.Next(2) == 0 ? -k : k, RoomCoords.RoomSize.y / far.bounds.size.y, 1);
            }
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
                Put(room, parent, sp, x, y, k, rng.Next(2) == 0, -10, look.Deep);
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
                Put(room, parent, sp, c * 8 + 4 - size.x / 2, floor - size.y + 2, k, rng.Next(2) == 0, -9, look.Far);
            }
        }
    }
}
