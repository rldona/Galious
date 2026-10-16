using System.Collections.Generic;
using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Agua y lava con el arte de Hollow Knight (Assets/Art/Resources/Hollow/liquidos, fuera de git; los importa
    // tools/hollow/liquidos.py), en la sala del jugador. El cuerpo del líquido lo pinta MapView (HollowScenery.WaterTile y
    // LavaTile); aquí va lo que se mueve, solo aspecto:
    //  - en la superficie de cada tramo, la línea ondulada animada de Hollow Knight (azulada el agua, naranja la lava);
    //  - agua: gotas que caen del techo de encima y salpican, y niebla flotando sobre ella (Hollow Knight no tiene burbujas
    //    en el agua: solo en el ácido, como señaló el usuario);
    //  - lava (Hollow Knight no tiene: su agua en naranja y la infección de los Cruces, las dos cosas): glóbulos naranjas
    //    que flotan meciéndose, burbujas que suben y estallan, ascuas que suben, humo y algún chorro de vapor;
    //  - al entrar o salir del líquido, una salpicadura.
    public class HollowLiquids : MonoBehaviour
    {
        public const string Folder = "Hollow/liquidos";
        public const float Fps = 10f;                         // la superficie (Ripple Water va a 20 con 70 fotogramas; aquí, 35)
        static readonly Color WaterLine = new Color(0.75f, 0.88f, 1f, 0.85f);
        static readonly Color LavaLine = new Color(1f, 0.72f, 0.3f, 0.95f);
        static readonly Color LavaBubble = new Color(1f, 0.6f, 0.25f, 0.9f);

        public GameBootstrap game;
        ArtLibrary lib;
        Material additive, normal;
        RoomModel shown;
        int shownHash;
        Transform root;

        class Surface { public SpriteRenderer sr; public int phase; }
        class Blob { public SpriteRenderer sr; public Vector2 at; public float phase; }
        class Particle
        {
            public SpriteRenderer sr; public Vector2 pos, vel; public float t, life, w; public FrameClip clip;
            public float surfaceY = float.NaN; public bool lava, drip; public float a0 = 1;
        }

        readonly List<Surface> surfaces = new List<Surface>();
        readonly List<SpriteRenderer> bodies = new List<SpriteRenderer>();
        Sprite[] waterGrad, lavaGrad;   // [d − 1]: las d filas de arriba del degradado (d = 1…LiquidDepths)
        readonly List<Blob> blobs = new List<Blob>();
        readonly List<Particle> live = new List<Particle>();
        readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>();
        // Tramos de superficie de la sala: (x0, x1, y de la superficie en px del original, lava).
        readonly List<(float x0, float x1, float y, bool lava)> runs = new List<(float, float, float, bool)>();
        // Techos sobre el agua (de donde caen gotas): (x, y del techo, y de la superficie).
        readonly List<(float x, float top, float y)> drips = new List<(float, float, float)>();
        float spawnTimer;
        bool wasIn;

        // Salpicadura pedida desde fuera (al caer en la lava el jugador vuelve a tierra antes de que se vea aquí).
        public static System.Action<RoomModel, int, bool> Splashed;

        // Orden de dibujo: el líquido va por delante de personajes y enemigos (translúcido, como en Hollow Knight: el
        // Caballero nadando queda medio dentro), y por detrás de las siluetas del primer plano (30).
        const int BodyOrder = 20, BubbleOrder = 21, LineOrder = 22, BlobOrder = 23, FxOrder = 24;

        public static bool Available => Resources.Load<Sprite>(Folder + "/superficie_00") != null;

        void Awake()
        {
            lib = ArtLibrary.Load(Folder);
            root = new GameObject("Líquidos de Hollow Knight").transform;
            // Nada de Shader.Find: en el compilado solo están los shaders de Resources.
            var add = Resources.Load<Shader>("Shaders/Additive");
            if (add != null) additive = new Material(add) { name = "Líquidos (aditivo)" };
            normal = Resources.Load<Material>("Materials/SpriteLit");
            Debug.Log($"[HollowLiquids] {lib.Count} clips");
            Splashed = (room, x, lava) => { if (room == shown) Splash(room, x, lava); };
        }

        void OnDestroy() { if (root != null) Destroy(root.gameObject); Splashed = null; }

        static Vector2 W(RoomModel room, float x, float y) =>
            RoomCoords.RoomOrigin(room) + new Vector2(x / RoomCoords.PixelsPerUnit, (RoomCoords.Rows * 8 - (y - RoomCoords.RoomTopPixel)) / RoomCoords.PixelsPerUnit);

        TileClass At(RoomModel room, int c, int r) =>
            c < 0 || c >= RoomCoords.Cols || r < 0 || r >= RoomCoords.Rows ? TileClass.Empty
                : game.World.TileAt(room, c * 8, OriginalPhysics.RoomTopPx + r * 8);

        static bool IsLiquid(TileClass t) => t == TileClass.Water || t == TileClass.Lava;

        SpriteRenderer Get(string name, int order, bool light)
        {
            var sr = pool.Count > 0 ? pool.Pop() : new GameObject("líquido").AddComponent<SpriteRenderer>();
            sr.transform.SetParent(root, false);
            sr.gameObject.SetActive(true);
            sr.sortingOrder = order;
            sr.sharedMaterial = light && additive != null ? additive : normal;
            sr.color = Color.white;
            sr.sprite = lib.Clip(name)?.Frames[0];
            sr.transform.rotation = Quaternion.identity;
            return sr;
        }

        void Release(SpriteRenderer sr) { sr.gameObject.SetActive(false); pool.Push(sr); }

        // Ancho de un sprite en px del original con escala 1.
        static float WidthPx(Sprite sp) => sp != null ? sp.bounds.size.x * RoomCoords.PixelsPerUnit : 1;

        void Rebuild(RoomModel room)
        {
            foreach (var s in surfaces) Release(s.sr);
            foreach (var b in bodies) Release(b);
            bodies.Clear();
            foreach (var b in blobs) Release(b.sr);
            foreach (var p in live) Release(p.sr);
            surfaces.Clear(); blobs.Clear(); live.Clear(); runs.Clear(); drips.Clear();
            var rng = new System.Random(room.id * 31 + 7);
            for (int r = 0; r < RoomCoords.Rows; r++)
            {
                int c = 0;
                while (c < RoomCoords.Cols)
                {
                    var t = At(room, c, r);
                    if (!IsLiquid(t) || IsLiquid(At(room, c, r - 1))) { c++; continue; }
                    int c0 = c;
                    while (c < RoomCoords.Cols && At(room, c, r) == t && !IsLiquid(At(room, c, r - 1))) c++;
                    float x0 = c0 * 8, x1 = c * 8, y = OriginalPhysics.RoomTopPx + r * 8;
                    bool lava = t == TileClass.Lava;
                    runs.Add((x0, x1, y, lava));
                    Body(room, c0, c, r, t);
                    // La línea de la superficie, una pieza tras otra, estirada para cubrir el tramo exacto.
                    var sp = lib.Clip("superficie")?.Frames[0];
                    float pw = WidthPx(sp);
                    int n = Mathf.Max(1, Mathf.RoundToInt((x1 - x0) / pw));
                    float k = (x1 - x0) / (n * pw);
                    for (int i = 0; i < n; i++)
                    {
                        var sr = Get("superficie", LineOrder, true);
                        sr.color = lava ? LavaLine : WaterLine;
                        var at = W(room, x0 + (i + 0.5f) * pw * k, y + 0.5f);
                        sr.transform.position = new Vector3(at.x, at.y, 0);
                        sr.transform.localScale = new Vector3(k, 1, 1);
                        surfaces.Add(new Surface { sr = sr, phase = rng.Next(35) });
                    }
                    if (lava)
                        for (float bx = x0 + 6; bx < x1 - 6; bx += 20 + rng.Next(20))
                        {
                            string name = rng.Next(3) switch { 0 => "globulo_a", 1 => "globulo_b", _ => "globulo_c" };
                            var sr = Get(name, BlobOrder, false);
                            float w = name == "globulo_a" ? 9 + rng.Next(5) : 4 + rng.Next(3);
                            float s = w / WidthPx(sr.sprite);
                            sr.transform.localScale = new Vector3(s, s, 1);
                            blobs.Add(new Blob { sr = sr, at = new Vector2(bx, y + w * 0.25f), phase = (float)rng.NextDouble() * 6 });
                        }
                    else
                        // Gotas: de los techos que haya encima del agua (el primer sólido subiendo), uno de cada 3 casillas.
                        for (int cc = c0; cc < c; cc += 3)
                        {
                            int rr = r - 1;
                            while (rr >= 0 && At(room, cc, rr) != TileClass.Solid) rr--;
                            if (rr >= 0 && r - rr > 2) drips.Add((cc * 8 + 4, OriginalPhysics.RoomTopPx + (rr + 1) * 8, y));
                        }
                }
            }
        }

        // Degradado del cuerpo, 1 texel por px del original: de la superficie (arriba) a LiquidDepths filas.
        Sprite[] Gradient(bool lava)
        {
            int h = HollowScenery.LiquidDepths * 8;
            var tex = new Texture2D(1, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++) tex.SetPixel(0, y, HollowScenery.LiquidColor(lava, (h - 1 - y) / (float)(h - 1)));
            tex.Apply(false, true);
            var sprites = new Sprite[HollowScenery.LiquidDepths];
            for (int d = 1; d <= sprites.Length; d++)
                sprites[d - 1] = Sprite.Create(tex, new Rect(0, h - d * 8, 1, d * 8), new Vector2(0.5f, 1f), RoomCoords.PixelsPerUnit, 0, SpriteMeshType.FullRect);
            return sprites;
        }

        // El cuerpo bajo un tramo de superficie: por columnas con la misma profundidad (hasta LiquidDepths filas), un trozo
        // del degradado de ese alto.
        void Body(RoomModel room, int c0, int c1, int r, TileClass t)
        {
            bool lava = t == TileClass.Lava;
            var grad = lava ? (lavaGrad ??= Gradient(true)) : (waterGrad ??= Gradient(false));
            int Depth(int c) { int d = 0; while (d < HollowScenery.LiquidDepths && At(room, c, r + d) == t) d++; return d; }
            int a = c0;
            while (a < c1)
            {
                int d = Depth(a), b = a + 1;
                while (b < c1 && Depth(b) == d) b++;
                if (d > 0)
                {
                    var sr = Get("superficie", BodyOrder, false);
                    sr.sprite = grad[d - 1];
                    var at = W(room, (a + b) * 4f, OriginalPhysics.RoomTopPx + r * 8);
                    sr.transform.position = new Vector3(at.x, at.y, 0);
                    sr.transform.localScale = new Vector3((b - a) * 8, 1, 1);
                    bodies.Add(sr);
                }
                a = b;
            }
        }

        void Spawn(RoomModel room, string name, Vector2 posPx, Vector2 velPx, float life, float widthPx, Color color,
            bool light, int order, bool anim = false)
        {
            var sr = Get(name, order, light);
            if (sr.sprite == null) { Release(sr); return; }
            sr.color = color;
            var clip = anim ? lib.Clip(name) : null;
            if (clip != null) { clip.Fps = 15; clip.Loop = false; if (life <= 0) life = clip.Duration; }
            live.Add(new Particle { sr = sr, pos = posPx, vel = velPx, life = life, w = widthPx, clip = clip, a0 = color.a });
            Place(room, live[live.Count - 1]);
        }

        void Place(RoomModel room, Particle p)
        {
            var at = W(room, p.pos.x, p.pos.y);
            p.sr.transform.position = new Vector3(at.x, at.y, 0);
            float s = p.w / WidthPx(p.sr.sprite);
            p.sr.transform.localScale = new Vector3(s, s, 1);
        }

        // Al entrar o salir del líquido: una salpicadura donde está el jugador, en la superficie.
        void Splash(RoomModel room, float x, bool lava)
        {
            float best = float.NaN;
            foreach (var r in runs)
                if (x >= r.x0 - 4 && x <= r.x1 + 4 && (float.IsNaN(best) || Mathf.Abs(r.y - game.Motor.Y) < Mathf.Abs(best - game.Motor.Y))) best = r.y;
            if (float.IsNaN(best)) return;
            Spawn(room, "salpicadura", new Vector2(x, best - 5), Vector2.zero, 0, 22, lava ? LavaLine : Color.white, true, FxOrder, true);
        }

        void LateUpdate()
        {
            if (game == null || game.Motor == null || game.World == null || lib == null || lib.Count == 0) return;
            var room = game.Motor.Room;
            bool on = !game.AttractRunning && !game.EndingRunning && room != null;
            root.gameObject.SetActive(on);
            if (!on) return;
            int hash = AmbientOcclusion.Hash((c, r) => IsLiquid(At(room, c, r)), RoomCoords.Cols, RoomCoords.Rows);
            if (room != shown || hash != shownHash) { shown = room; shownHash = hash; Rebuild(room); }
            float dt = Time.deltaTime, now = Time.time;

            // Superficie animada.
            var surf = lib.Clip("superficie");
            if (surf != null)
                foreach (var s in surfaces)
                    s.sr.sprite = surf.Frames[(int)(now * Fps + s.phase) % surf.Length];
            // Glóbulos que se mecen.
            foreach (var b in blobs)
            {
                var at = W(room, b.at.x + Mathf.Sin(now * 0.7f + b.phase) * 1.5f, b.at.y + Mathf.Sin(now * 1.3f + b.phase) * 1f);
                b.sr.transform.position = new Vector3(at.x, at.y, 0);
            }

            // Salpicadura al entrar o salir.
            bool inside = game.Motor.InLiquid;
            if (inside != wasIn) Splash(room, game.Motor.X, game.Motor.Liquid == TileClass.Lava || !inside && runs.Exists(r => r.lava));
            wasIn = inside;

            // Nuevas partículas.
            spawnTimer -= dt;
            if (spawnTimer <= 0 && runs.Count > 0)
            {
                spawnTimer = 0.12f;
                var run = runs[Random.Range(0, runs.Count)];
                float x = Random.Range(run.x0 + 2, run.x1 - 2);
                if (run.lava)
                {
                    float roll = Random.value;
                    if (roll < 0.35f)
                    {
                        // Burbuja que sube desde dentro y estalla en la superficie.
                        var p = new Vector2(x, run.y + Random.Range(6f, 20f));
                        Spawn(room, "burbuja", p, new Vector2(0, -Random.Range(8f, 14f)), 6, Random.Range(2.5f, 4.5f), LavaBubble, true, BubbleOrder);
                        live[live.Count - 1].surfaceY = run.y;
                        live[live.Count - 1].lava = true;
                    }
                    else if (roll < 0.7f)
                        Spawn(room, "ascua", new Vector2(x, run.y), new Vector2(Random.Range(-3f, 3f), -Random.Range(10f, 22f)),
                            Random.Range(1.2f, 2.2f), Random.Range(1.5f, 2.5f), new Color(1f, 0.6f, 0.2f, 1f), true, FxOrder);
                    else if (roll < 0.85f)
                        Spawn(room, "humo", new Vector2(x, run.y - 4), new Vector2(Random.Range(-2f, 2f), -Random.Range(3f, 6f)),
                            Random.Range(2.5f, 3.5f), Random.Range(14f, 22f), new Color(1f, 0.45f, 0.2f, 0.18f), false, FxOrder);
                    else if (roll < 0.9f)
                        Spawn(room, "vapor", new Vector2(x, run.y - 12), Vector2.zero, 0, 7, new Color(1f, 0.55f, 0.25f, 0.6f), true, FxOrder, true);
                }
                else if (Random.value < 0.3f)
                    Spawn(room, "niebla", new Vector2(x, run.y - Random.Range(2f, 6f)), new Vector2(Random.Range(-3f, 3f), 0),
                        Random.Range(4f, 6f), Random.Range(25f, 40f), new Color(0.75f, 0.85f, 1f, 0.12f), false, FxOrder);
            }
            // Gotas del techo sobre el agua, de vez en cuando.
            if (drips.Count > 0 && Random.value < dt * 0.6f)
            {
                var d = drips[Random.Range(0, drips.Count)];
                Spawn(room, "gota_cae", new Vector2(d.x, d.top + 2), new Vector2(0, 40), 10, 1.6f, new Color(0.8f, 0.9f, 1f, 0.9f), true, FxOrder);
                live[live.Count - 1].surfaceY = d.y;
                live[live.Count - 1].drip = true;
            }

            // Mover, animar y acabar.
            for (int i = live.Count - 1; i >= 0; i--)
            {
                var p = live[i];
                p.t += dt;
                p.pos += p.vel * dt;
                if (p.drip) p.vel.y += 60 * dt;
                if (!p.drip && p.clip == null && !float.IsNaN(p.surfaceY))
                    p.pos.x += Mathf.Sin(p.t * 6) * 4 * dt;          // las burbujas se mecen al subir
                bool reached = !float.IsNaN(p.surfaceY) && (p.drip ? p.pos.y >= p.surfaceY : p.pos.y <= p.surfaceY);
                if (reached)
                {
                    if (p.drip) Spawn(room, "salpicadura", new Vector2(p.pos.x, p.surfaceY - 3), Vector2.zero, 0, 9, new Color(1, 1, 1, 0.8f), true, FxOrder, true);
                    else Spawn(room, "burbuja_estalla", new Vector2(p.pos.x, p.surfaceY - 1), Vector2.zero, 0.25f, 6, LavaBubble, true, FxOrder, true);
                    Release(p.sr); live.RemoveAt(i); continue;
                }
                if (p.clip != null)
                {
                    int f = (int)(p.t * p.clip.Fps);
                    if (f >= p.clip.Length) { Release(p.sr); live.RemoveAt(i); continue; }
                    p.sr.sprite = p.clip.Frames[f];
                }
                else if (p.t >= p.life) { Release(p.sr); live.RemoveAt(i); continue; }
                var c = p.sr.color;
                float fade = p.clip != null ? 1 : Mathf.Clamp01(Mathf.Min(p.t / 0.3f, (p.life - p.t) / 0.6f));
                c.a = p.a0 * fade;
                p.sr.color = c;
                Place(room, p);
            }
        }
    }
}
