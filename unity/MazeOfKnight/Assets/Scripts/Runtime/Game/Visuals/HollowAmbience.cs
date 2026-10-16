using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MazeOfKnight.Game.Visuals
{
    // El ambiente de cada mundo como lo pone Team Cherry en Hollow Knight (RemakeRules.HollowAmbience; lo pidió el usuario).
    // En Hollow Knight cada escena lleva un SceneManager (Assembly-CSharp/SceneManager.cs) con:
    //   - la saturación y las curvas de color de la cámara (ColorCorrectionCurves) y la luz ambiente (defaultColor);
    //   - el color de la luz suave que lleva el Caballero (heroLightColor, HeroController.heroLight);
    //   - el nivel de oscuridad (darknessLevel, de −1 a 2): la viñeta Knight/Vignette deja un hueco alrededor del
    //     Caballero de escala 10,5 / 5,5 / 2,2 / 0,8 (FSM "Darkness Control"), que la linterna de luciérnagas abre a 3;
    //   - y la cámara enciende las partículas de su zona (SceneParticlesController: polvo, hojas, goteo, lluvia…).
    // Aquí, por mundo (su zona de Hollow Knight, la de HollowScenery.Looks): la gradación con un Volume de URP (saturación
    // y filtro de color), la luz del Caballero (light_effect_v02 aditiva), la oscuridad con su hueco donde Hollow Knight
    // la tiene (los Canales Reales, el Abismo y el Nido Profundo son de nivel 1 en la mayoría de sus salas; la Vela del
    // original hace de linterna) y las partículas de la zona con sus texturas (tools/hollow/ambiente.py). Los niveles de
    // oscuridad y las partículas salen de los datos del juego. La saturación, el filtro de color y el color de la bruma
    // se han medido (tools/hollow/medir_ambiente.py) en las imágenes de las salas de cada zona (hollow-knight-mundos,
    // reconstruidas desde sus ficheros de nivel) frente a nuestras salas sin gradación: las de Hollow Knight son más
    // luminosas y menos saturadas, con bruma entre los planos. A la saturación medida se le suman los 17 puntos que la
    // cámara de Hollow Knight añade siempre (SceneManager.AdjustSaturationForPlatform), que las imágenes no llevan. Por eso cada sala lleva además, como pidió el usuario,
    // oclusión ambiental (AmbientOcclusion: sombra en el aire junto a la roca), una bruma del color de la zona entre el
    // fondo y el plano de juego, más espesa abajo, y jirones de niebla (Fog.png de Hollow Knight) que pasan despacio.
    public class HollowAmbience : MonoBehaviour
    {
        public GameBootstrap game;

        public static bool Available => Resources.Load<Sprite>("Hollow/ambiente/luz") != null;
        // -ambienteNeutro: sin gradación (para medir el color de nuestras salas con tools/hollow/medir_ambiente.py).
        static readonly bool Neutral = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-ambienteNeutro") >= 0;

        // Partículas de una zona: textura (Hollow/ambiente), dibujos de su tira, aditiva, ritmo (por segundo), vida (s),
        // tamaño (unidades; la sala mide 16 × 10), velocidad inicial, gravedad, viento (x), giro (grados/s), color y si
        // salen de arriba (lluvia, goteo, hojas) o de toda la sala.
        struct Particles
        {
            public string Tex;
            public int Frames;
            public bool Additive, FromTop, Stretch;
            public float Rate, LifeMin, LifeMax, SizeMin, SizeMax, Speed, Gravity, Wind, Spin;
            public Color Color;
        }

        struct Look
        {
            public int Darkness;                 // −1 claro, 0 normal, 1 oscuro, 2 muy oscuro (darknessLevel)
            public float Saturation;             // −100 … 100
            public Color Filter, Hero;           // filtro de color de la cámara y luz del Caballero
            public Color Haze;                   // color medio de las salas de la zona en Hollow Knight (la bruma)
            public Particles Main, Extra;
        }

        static Particles P(string tex, int frames, bool add, float rate, float life0, float life1, float size0, float size1, Color c,
            float speed = 0, float gravity = 0, float wind = 0, float spin = 0, bool top = false, bool stretch = false) => new Particles
        {
            Tex = tex, Frames = frames, Additive = add, Rate = rate, LifeMin = life0, LifeMax = life1, SizeMin = size0, SizeMax = size1,
            Color = c, Speed = speed, Gravity = gravity, Wind = wind, Spin = spin, FromTop = top, Stretch = stretch,
        };

        static Color C(float r, float g, float b, float a = 1) => new Color(r, g, b, a);

        // Índice 0 = castillo (Cruces Olvidados), 1–10 = mundos, en el orden de HollowScenery.Looks.
        static readonly Look[] Looks =
        {
            new Look { Darkness = 0, Saturation = 1, Filter = C(1.00f, 1.01f, 0.93f), Haze = C(0.19f, 0.21f, 0.29f), Hero = C(0.75f, 0.85f, 1f),
                Main = P("mota", 1, true, 7, 6, 10, 0.1f, 0.26f, C(0.8f, 0.88f, 1f, 0.55f), speed: 0.05f, gravity: -0.004f) },
            new Look { Darkness = 0, Saturation = 40, Filter = C(0.78f, 1.05f, 1.15f), Haze = C(0.12f, 0.17f, 0.22f), Hero = C(0.8f, 1f, 0.85f),
                Main = P("hoja", 3, false, 2.2f, 7, 10, 0.3f, 0.5f, C(0.85f, 1f, 0.85f, 0.85f), gravity: 0.02f, wind: 0.35f, spin: 60, top: true) },
            new Look { Darkness = 1, Saturation = 20, Filter = C(0.71f, 1.09f, 0.96f), Haze = C(0.11f, 0.19f, 0.26f), Hero = C(0.7f, 1f, 0.9f),
                Main = P("gota", 1, false, 9, 1.2f, 1.6f, 0.14f, 0.22f, C(0.75f, 0.95f, 0.9f, 0.7f), gravity: 1.2f, top: true),
                Extra = P("niebla", 1, false, 0.5f, 12, 16, 5, 8, C(0.55f, 0.75f, 0.75f, 0.2f), speed: 0.12f) },
            new Look { Darkness = 0, Saturation = -19, Filter = C(0.89f, 1.04f, 0.94f), Haze = C(0.24f, 0.19f, 0.14f), Hero = C(1f, 0.88f, 0.6f),
                Main = P("hexagono", 1, false, 1.6f, 12, 16, 0.22f, 0.4f, C(1f, 0.85f, 0.5f, 0.55f), speed: 0.12f, spin: 20) },
            new Look { Darkness = 0, Saturation = 3, Filter = C(0.87f, 1.05f, 0.90f), Haze = C(0.26f, 0.22f, 0.34f), Hero = C(1f, 0.82f, 1f),
                Main = P("cristal", 3, false, 2, 10, 14, 0.28f, 0.5f, C(1f, 0.85f, 1f, 0.85f), gravity: -0.02f, spin: 25) },
            new Look { Darkness = 0, Saturation = 2, Filter = C(0.89f, 1.05f, 0.87f), Haze = C(0.16f, 0.21f, 0.30f), Hero = C(0.7f, 0.8f, 1f),
                Main = P("lluvia", 1, false, 55, 0.6f, 0.8f, 0.05f, 0.08f, C(0.75f, 0.85f, 1f, 0.5f), speed: 16, top: true, stretch: true) },
            new Look { Darkness = 0, Saturation = 14, Filter = C(0.81f, 1.07f, 0.90f), Haze = C(0.14f, 0.21f, 0.26f), Hero = C(0.7f, 1f, 0.85f),
                Main = P("espora", 1, true, 6, 6, 10, 0.12f, 0.28f, C(0.75f, 1f, 0.6f, 0.7f), speed: 0.08f, gravity: -0.006f) },
            new Look { Darkness = 1, Saturation = -23, Filter = C(0.84f, 1.07f, 0.77f), Haze = C(0.18f, 0.20f, 0.23f), Hero = C(0.9f, 0.9f, 1f),
                Main = P("humo", 5, false, 0.9f, 9, 13, 3.5f, 6.5f, C(0.05f, 0.05f, 0.08f, 0.45f), speed: 0.12f, spin: 6) },
            new Look { Darkness = 1, Saturation = -18, Filter = C(0.65f, 1.11f, 1.04f), Haze = C(0.15f, 0.18f, 0.22f), Hero = C(0.8f, 0.8f, 0.92f),
                Main = P("telarana", 4, false, 1.2f, 9, 12, 0.35f, 0.6f, C(0.8f, 0.8f, 0.85f, 0.45f), gravity: 0.01f, spin: 10),
                Extra = P("mota", 1, true, 4, 6, 9, 0.08f, 0.18f, C(0.7f, 0.7f, 0.8f, 0.35f), speed: 0.04f) },
            new Look { Darkness = 0, Saturation = 40, Filter = C(0.83f, 1.04f, 1.07f), Haze = C(0.21f, 0.23f, 0.28f), Hero = C(1f, 0.95f, 0.9f),
                Main = P("ceniza", 4, true, 10, 5, 7, 0.14f, 0.28f, C(0.9f, 0.9f, 0.9f, 0.8f), gravity: 0.04f, wind: -1.1f, spin: 90, top: true) },
            new Look { Darkness = -1, Saturation = 1, Filter = C(0.80f, 1.04f, 1.17f), Haze = C(0.26f, 0.28f, 0.35f), Hero = C(1f, 1f, 1f),
                Main = P("petalo", 3, false, 2, 8, 11, 0.28f, 0.42f, C(1f, 1f, 1f, 0.9f), gravity: 0.03f, wind: 0.3f, spin: 70, top: true),
                Extra = P("mota", 1, true, 5, 6, 9, 0.1f, 0.22f, C(1f, 1f, 1f, 0.5f), speed: 0.05f) },
        };

        // Oscuridad (hueco interior y exterior en unidades, opacidad fuera) por nivel; con la Vela, como con la linterna.
        // Más suave que en Hollow Knight (la sala entera está en pantalla y hay que ver por dónde se va).
        static (float inner, float outer, float alpha) DarknessShape(int level, bool lantern) =>
            level <= 0 ? (0, 0, 0) : lantern ? (5f, 10f, 0.55f) : level == 1 ? (3.4f, 8f, 0.72f) : (2f, 5f, 0.88f);

        ParticleSystem main, extra;
        SpriteRenderer heroLight, darkness, occlusion, haze;
        readonly System.Collections.Generic.List<(SpriteRenderer sr, float speed)> wisps = new System.Collections.Generic.List<(SpriteRenderer, float)>();
        int occlusionHash;
        ColorAdjustments grade;
        Material additive, alpha;
        string shownMap;
        RoomModel shownRoom;
        (float, float, float) shownDark = (-1, -1, -1);

        void Start()
        {
            var add = Resources.Load<Shader>("Shaders/Additive");
            var blend = Resources.Load<Shader>("Shaders/AlphaBlend");
            additive = add != null ? new Material(add) { name = "Ambiente (aditivo)" } : null;
            alpha = blend != null ? new Material(blend) { name = "Ambiente (normal)" } : null;
            var root = new GameObject("Ambiente de Hollow Knight").transform;
            main = NewParticles(root, "Partículas de la zona");
            extra = NewParticles(root, "Partículas de la zona (2)");
            heroLight = new GameObject("Luz del Caballero").AddComponent<SpriteRenderer>();
            heroLight.transform.SetParent(root, false);
            heroLight.sprite = Resources.Load<Sprite>("Hollow/ambiente/luz");
            heroLight.sortingOrder = 2;
            if (additive != null) heroLight.sharedMaterial = additive;
            darkness = new GameObject("Oscuridad").AddComponent<SpriteRenderer>();
            darkness.transform.SetParent(root, false);
            darkness.sortingOrder = 40;                          // encima de la sala y los personajes, debajo del marcador
            if (alpha != null) darkness.sharedMaterial = alpha;
            occlusion = NewLayer(root, "Oclusión ambiental", -7);
            haze = NewLayer(root, "Bruma", -8);
            haze.sprite = HazeSprite();
            for (int i = 0; i < 3; i++)
            {
                var w = NewLayer(root, "Jirón de niebla", -8);
                w.sprite = Resources.Load<Sprite>("Hollow/ambiente/niebla");
                wisps.Add((w, 0.08f + 0.07f * i));
            }
            var vol = new GameObject("Gradación de la zona").AddComponent<Volume>();
            vol.transform.SetParent(root, false);
            vol.isGlobal = true;
            vol.priority = 50;                                   // el brillo de Opciones (100) sigue mandando en la exposición
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            grade = profile.Add<ColorAdjustments>(false);    // solo lo que se cambia (saturación y filtro)
            vol.sharedProfile = profile;
        }

        SpriteRenderer NewLayer(Transform root, string name, int order)
        {
            var sr = new GameObject(name).AddComponent<SpriteRenderer>();
            sr.transform.SetParent(root, false);
            sr.sortingOrder = order;
            if (alpha != null) sr.sharedMaterial = alpha;
            return sr;
        }

        // La bruma: un degradado vertical, más espeso abajo (como la niebla del suelo de las salas de Hollow Knight).
        static Sprite HazeSprite()
        {
            const int H = 64;
            var tex = new Texture2D(1, H, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < H; y++)
            {
                float t = y / (H - 1f);                                   // 0 abajo, 1 arriba
                tex.SetPixel(0, y, new Color(1, 1, 1, Mathf.Lerp(0.16f, 0.03f, Mathf.SmoothStep(0, 1, t))));
            }
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, 1, H), new Vector2(0.5f, 0.5f), 1);
        }

        // Oclusión ambiental de la sala (Visuals/AmbientOcclusion), rehecha si cambia su colisión (muros que se abren).
        void UpdateOcclusion(RoomModel room)
        {
            var w = game.World;
            bool Solid(int c, int r) => w.TileAt(room, c * 8 + 4, OriginalPhysics.RoomTopPx + r * 8 + 4) == TileClass.Solid;
            int hash = AmbientOcclusion.Hash(Solid, RoomCoords.Cols, RoomCoords.Rows);
            if (room == shownRoom && hash == occlusionHash && occlusion.sprite != null) return;
            occlusionHash = hash;
            if (occlusion.sprite != null) Destroy(occlusion.sprite.texture);
            var a = AmbientOcclusion.Compute(Solid, RoomCoords.Cols, RoomCoords.Rows);
            var tex = AmbientOcclusion.ToTexture(a, RoomCoords.Cols, RoomCoords.Rows);
            occlusion.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero,
                AmbientOcclusion.TexelsPerTile / RoomCoords.TileSize, 0, SpriteMeshType.FullRect);
            var o = RoomCoords.RoomOrigin(room);
            occlusion.transform.position = new Vector3(o.x, o.y, 0);
        }

        static ParticleSystem NewParticles(Transform root, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var m = ps.main;
            m.playOnAwake = false;
            m.loop = true;
            m.simulationSpace = ParticleSystemSimulationSpace.World;
            m.maxParticles = 300;
            return ps;
        }

        static int Index(string map) =>
            map != null && map.StartsWith("world") && int.TryParse(map.Substring(5), out int w) ? Mathf.Clamp(w, 0, 10) : 0;

        void LateUpdate()
        {
            var motor = game != null ? game.Motor : null;
            bool on = RemakeRules.HollowAmbience && motor != null && motor.Room != null && !game.AttractRunning && !game.EndingRunning;
            heroLight.enabled = on;
            occlusion.enabled = haze.enabled = on;
            foreach (var (w, _) in wisps) w.enabled = on && w.sprite != null;
            if (!on)
            {
                darkness.enabled = false;
                grade.active = false;
                if (main.isPlaying) main.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                if (extra.isPlaying) extra.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                shownMap = null;
                return;
            }
            var look = Looks[Index(game.mapName)];
            if (shownMap != game.mapName)
            {
                shownMap = game.mapName;
                grade.active = !Neutral;
                grade.saturation.Override(look.Saturation);
                grade.colorFilter.Override(look.Filter);
                heroLight.color = new Color(look.Hero.r, look.Hero.g, look.Hero.b, 0.28f);
                // La bruma y la niebla, del color medio de la zona aclarado (es luz que flota en el aire).
                var hz = new Color(Mathf.Min(1, look.Haze.r * 1.8f), Mathf.Min(1, look.Haze.g * 1.8f), Mathf.Min(1, look.Haze.b * 1.8f));
                haze.color = hz;
                foreach (var (w, _) in wisps) w.color = new Color(hz.r, hz.g, hz.b, 0.09f);
                Setup(main, look.Main);
                Setup(extra, look.Extra);
                shownRoom = null;
            }
            // Las partículas, en la sala del jugador (las de la anterior se borran al cambiar).
            UpdateOcclusion(motor.Room);
            if (shownRoom != motor.Room)
            {
                shownRoom = motor.Room;
                var center = RoomCoords.RoomOrigin(motor.Room) + RoomCoords.RoomSize / 2;
                haze.transform.position = new Vector3(center.x, center.y, 0);
                haze.transform.localScale = new Vector3(RoomCoords.RoomSize.x, RoomCoords.RoomSize.y / 64f, 1);
                var rng = new System.Random(motor.Room.id * 31 + 7);
                foreach (var (w, _) in wisps)
                {
                    if (w.sprite == null) continue;
                    float ws = (7f + (float)rng.NextDouble() * 5f) / w.sprite.bounds.size.x;
                    w.transform.localScale = new Vector3(ws, ws, 1);
                    w.transform.position = new Vector3(center.x + ((float)rng.NextDouble() - 0.5f) * RoomCoords.RoomSize.x,
                        center.y + ((float)rng.NextDouble() - 0.6f) * RoomCoords.RoomSize.y * 0.8f, 0);
                }
                Place(main, look.Main, center);
                Place(extra, look.Extra, center);
            }
            // Los jirones de niebla pasan despacio y vuelven a entrar por el otro lado de la sala.
            var room0 = RoomCoords.RoomOrigin(motor.Room);
            foreach (var (w, speed) in wisps)
            {
                if (w.sprite == null) continue;
                var pos = w.transform.position;
                float half = w.sprite.bounds.size.x * w.transform.localScale.x / 2;
                pos.x += speed * Time.deltaTime;
                if (pos.x - half > room0.x + RoomCoords.RoomSize.x) pos.x = room0.x - half;
                w.transform.position = pos;
            }
            // La luz y el hueco de la oscuridad, con el Caballero (a media altura).
            var p = RoomCoords.PixelToWorld(motor.Room, motor.X, motor.Y - 11) + PlayerController.DrawOffset;
            heroLight.transform.position = new Vector3(p.x, p.y, 0);
            float k = 5.5f / Mathf.Max(0.01f, heroLight.sprite != null ? heroLight.sprite.bounds.size.x : 1);
            heroLight.transform.localScale = new Vector3(k, k, 1);
            var shape = DarknessShape(look.Darkness, game.Inventory.Has(Inventory.Candle));
            darkness.enabled = shape.alpha > 0;
            if (darkness.enabled)
            {
                if (shape != shownDark) { shownDark = shape; darkness.sprite = DarknessSprite(shape.inner, shape.outer, shape.alpha); }
                darkness.transform.position = new Vector3(p.x, p.y, 0);
            }
        }

        void Setup(ParticleSystem ps, Particles d)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (string.IsNullOrEmpty(d.Tex)) return;
            var tex = Resources.Load<Sprite>("Hollow/ambiente/" + d.Tex);
            var mat = d.Additive ? additive : alpha;
            if (tex == null || mat == null) return;
            var r = ps.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = new Material(mat) { mainTexture = tex.texture };
            r.sortingOrder = d.Tex == "niebla" || d.Tex == "humo" ? 12 : 15;
            r.renderMode = d.Stretch ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            r.velocityScale = d.Stretch ? 0.05f : 0;
            r.lengthScale = d.Stretch ? 2f : 1;
            var m = ps.main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(d.LifeMin, d.LifeMax);
            m.startSize = new ParticleSystem.MinMaxCurve(d.SizeMin, d.SizeMax);
            m.startSpeed = d.Speed;
            m.gravityModifier = d.Gravity;
            m.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            m.startColor = d.Color;
            var e = ps.emission;
            e.rateOverTime = d.Rate;
            var vel = ps.velocityOverLifetime;
            vel.enabled = d.Wind != 0;
            vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(d.Wind * 0.7f, d.Wind * 1.3f);
            vel.y = new ParticleSystem.MinMaxCurve(0, 0);
            vel.z = new ParticleSystem.MinMaxCurve(0, 0);
            var rot = ps.rotationOverLifetime;
            rot.enabled = d.Spin != 0;
            rot.z = new ParticleSystem.MinMaxCurve(-d.Spin * Mathf.Deg2Rad, d.Spin * Mathf.Deg2Rad);
            var noise = ps.noise;
            noise.enabled = !d.Stretch && d.Gravity < 0.5f;
            noise.strength = 0.25f;
            noise.frequency = 0.3f;
            noise.scrollSpeed = 0.15f;
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.15f), new GradientAlphaKey(1, 0.8f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var sheet = ps.textureSheetAnimation;
            sheet.enabled = d.Frames > 1;
            if (d.Frames > 1)
            {
                sheet.mode = ParticleSystemAnimationMode.Grid;
                sheet.numTilesX = 1;
                sheet.numTilesY = d.Frames;
                sheet.animation = ParticleSystemAnimationType.WholeSheet;
                sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0, 0);
                sheet.startFrame = new ParticleSystem.MinMaxCurve(0, d.Frames - 0.01f);   // un dibujo al azar, fijo
                sheet.cycleCount = 1;
            }
        }

        static void Place(ParticleSystem ps, Particles d, Vector2 center)
        {
            if (string.IsNullOrEmpty(d.Tex)) return;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            var size = RoomCoords.RoomSize;
            if (d.FromTop)
            {
                // Una franja encima de la sala; lo que cae hacia abajo (la lluvia, con su velocidad hacia abajo).
                ps.transform.position = new Vector3(center.x, center.y + size.y / 2 + 0.3f, 0);
                shape.scale = new Vector3(size.x * 1.2f, 0.2f, 0.1f);
                ps.transform.rotation = d.Speed > 0 ? Quaternion.Euler(90, 0, 0) : Quaternion.identity;
            }
            else
            {
                bool low = d.Tex == "niebla";
                ps.transform.position = new Vector3(center.x, center.y - (low ? size.y * 0.3f : 0), 0);
                shape.scale = new Vector3(size.x * 1.1f, low ? size.y * 0.35f : size.y, 0.1f);
                ps.transform.rotation = Quaternion.identity;
            }
            ps.Play();
            // Que la sala no empiece vacía: lo que habría a los pocos segundos.
            ps.Simulate(Mathf.Min(d.LifeMax, 6f), true, true);
            ps.Play();
        }

        // Un velo negro con un hueco suave en el centro (radio interior → exterior, en unidades), de 48 unidades de lado
        // para cubrir la pantalla vaya donde vaya el Caballero.
        static Sprite DarknessSprite(float inner, float outer, float alpha)
        {
            const int N = 256;
            const float Side = 48f;
            var tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[N * N];
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float dx = (x + 0.5f) / N - 0.5f, dy = (y + 0.5f) / N - 0.5f;
                    float r = Mathf.Sqrt(dx * dx + dy * dy) * Side;
                    float t = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(inner, outer, r));
                    px[y * N + x] = new Color32(0, 0, 0, (byte)(t * alpha * 255));
                }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N / Side);
        }
    }
}
