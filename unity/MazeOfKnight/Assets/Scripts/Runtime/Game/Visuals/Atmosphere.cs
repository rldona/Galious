using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game.Enemies;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

namespace MazeOfKnight.Game.Visuals
{
    // Ambiente del remake al estilo de Hollow Knight (docs/design_bible/12_estilo_visual.md). Solo es aspecto: no toca
    // la lógica, la colisión ni los tiempos del original.
    //  - Capas por sala: cielo en degradado, siluetas lejanas, niebla, siluetas medias, niebla baja, (sala), motas y un
    //    primer plano en silueta. Con la cámara fija, la profundidad sale de un parallax según la posición del jugador.
    //  - Luces 2D sobre la sala: luz global de la paleta, una luz que acompaña al jugador y farolas en las paredes.
    //    Rayos de luz y halos aditivos que el bloom recoge. Viñeta y algo de contraste.
    //  - Fundido desde negro al cambiar de sala (dura lo que la pausa de 11 ticks del original) y más largo al cambiar de
    //    mapa.
    //  - Personajes, enemigos y objetos con contorno de tinta y destello al recibir un golpe.
    //  - F7 activa o quita el estilo; F6 prueba la paleta de otro mundo en la sala actual.
    public class Atmosphere : MonoBehaviour
    {
        public GameBootstrap game;
        public PlayerController player;
        public MapView mapView;
        public bool styled = true;

        // Material de personajes (contorno + destello); null con el estilo quitado o sin el shader.
        public static Material CharacterMaterial { get; private set; }
        // Dibujos de enemigos y armas; null con el estilo quitado.
        public static EnemyArt EnemyArt { get; private set; }
        // Objetos y ascensores; null con el estilo quitado.
        public static PropArt Props { get; private set; }
        public static CharacterArt Characters { get; private set; }
        // El estilo del remake está puesto (lo mira la interfaz).
        public static bool Styled { get; private set; }
        PropArt props;
        EnemyArt enemyArt;

        // Entre salas no hay fundido: el corte seco del original es más fluido. Solo al entrar o salir de un mundo.
        const float MapFadeSeconds = 0.8f;
        const float ParallaxFollow = 0.08f;          // cuánto "seguiría" una cámara virtual al jugador

        Camera cam;
        Transform root;
        SpriteRenderer sky, far, mid, near, fogFar, fogLow, front, fade;
        readonly List<SpriteRenderer> shafts = new List<SpriteRenderer>();
        readonly List<(SpriteRenderer glow, Light2D light)> lamps = new List<(SpriteRenderer, Light2D)>();
        ParticleSystem motes;
        Light2D globalLight, playerLight, bossLight;
        BossEffects bossFx;
        float darkness;
        readonly Dictionary<SpriteRenderer, Color> baseColors = new Dictionary<SpriteRenderer, Color>();
        Volume volume;
        Bloom bloom;
        Vignette vignette;
        ColorAdjustments grade;
        Material unlit, additive, defaultSprite;
        Sprite dot, shaft, white;
        CharacterArt characters;
        SpriteRenderer slash;
        readonly Dictionary<(Theme, ShapePainter.Depth, int), Sprite> layerCache = new Dictionary<(Theme, ShapePainter.Depth, int), Sprite>();
        readonly Dictionary<int, Sprite> fogCache = new Dictionary<int, Sprite>();
        readonly Dictionary<WorldPalette, Sprite> skyCache = new Dictionary<WorldPalette, Sprite>();
        MapView.StyleTiles tiles;
        Tile ladderRightTile;
        Tile solidTile, solidInnerTile, solidDeepTile, rimTile, ladderTile, waterTile, lavaTile, bossTile;
        MapView.StyleTiles placeholderTiles;

        string shownMap;
        RoomModel shownRoom;
        int paletteOverride = -1;
        WorldPalette palette, tiledPalette;
        float fadeLeft, fadeTotal;
        int lastHurt;
        bool subscribed;
        readonly Dictionary<SpriteRenderer, float> flashes = new Dictionary<SpriteRenderer, float>();
        MaterialPropertyBlock block;

        // ---------- montaje ----------

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            block = new MaterialPropertyBlock();
            var outline = Resources.Load<Shader>("Shaders/SpriteOutlineFlash");
            var add = Resources.Load<Shader>("Shaders/Additive");
            // Nada de Shader.Find: en el compilado solo están los shaders referenciados (los de Resources siempre).
            if (outline == null || add == null)
            {
                Debug.LogWarning($"[Atmosphere] Faltan shaders (contorno: {outline != null}, aditivo: {add != null}): sin estilo del remake");
                styled = false;
                enabled = false;
                return;
            }
            CharacterMaterial = new Material(outline) { name = "Personajes (contorno)" };
            additive = new Material(add) { name = "Aditivo" };
            // Sin luz = el shader de contorno sin contorno.
            unlit = new Material(outline) { name = "Sin luz" };
            unlit.SetFloat("_Outline", 0);
            dot = ShapePainter.ToSprite(ShapePainter.SoftDot(), 64, new Vector2(0.5f, 0.5f));
            shaft = ShapePainter.ToSprite(ShapePainter.Shaft(), 32, new Vector2(0.5f, 1f));
            var w = new Texture2D(1, 1);
            w.SetPixel(0, 0, Color.white);
            w.Apply();
            white = Sprite.Create(w, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);

            props = new PropArt();
            BuildTiles();
            if (mapView != null)
            {
                placeholderTiles = new MapView.StyleTiles
                {
                    Solid = mapView.solid, Ladder = mapView.ladder, Water = mapView.water, Lava = mapView.lava, BossPassable = mapView.bossPassable,
                };
                if (styled) mapView.Style = tiles;   // antes de que GameBootstrap dibuje el primer mapa
            }

            root = new GameObject("Ambiente").transform;
            sky = Layer("Cielo", -100, unlit);
            far = Layer("Siluetas lejanas", -60, unlit);
            fogFar = Layer("Niebla lejana", -55, unlit);
            mid = Layer("Siluetas medias", -40, unlit);
            fogLow = Layer("Niebla baja", -35, unlit);
            near = Layer("Siluetas cercanas", -20, unlit);
            front = Layer("Primer plano", 50, unlit);
            fade = Layer("Fundido", 1000, unlit);
            fade.sprite = white;
            fade.color = Color.black;

            globalLight = NewLight("Luz global", Light2D.LightType.Global);
            playerLight = NewLight("Luz del jugador", Light2D.LightType.Point);
            playerLight.pointLightOuterRadius = 3.2f;
            playerLight.pointLightInnerRadius = 0.2f;
            playerLight.intensity = 0.55f;
            playerLight.falloffIntensity = 0.6f;
            bossLight = NewLight("Luz del jefe", Light2D.LightType.Point);
            bossLight.pointLightOuterRadius = 6.5f;
            bossLight.pointLightInnerRadius = 0.5f;
            bossLight.falloffIntensity = 0.5f;
            bossLight.color = new Color(1f, 0.35f, 0.25f);
            bossLight.intensity = 0;

            motes = BuildMotes();
            gameObject.AddComponent<RemakeHud>().game = game;
            gameObject.AddComponent<HitboxOverlay>().game = game;
            enemyArt = new EnemyArt();
            gameObject.AddComponent<HitEffects>().Init(additive, unlit, dot);
            characters = new CharacterArt { ShieldLevel = () => game != null && game.Inventory != null ? game.Inventory.ShieldLevel : 0 };
            slash = Layer("Estela del espadazo", 12, additive);
            slash.sprite = characters.SlashSprite;
            slash.enabled = false;
            BuildVolume();
        }

        void Start()
        {
            if (cam != null)
            {
                var data = cam.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
            }
            if (player != null)
            {
                Outline(player.body);
                Outline(player.sword);
            }
            Apply(styled);
        }

        void OnDestroy()
        {
            if (CharacterMaterial != null) Destroy(CharacterMaterial);
            CharacterMaterial = null;
            EnemyArt = null;
            Props = null;
            Characters = null;
            Styled = false;
        }

        SpriteRenderer Layer(string name, int order, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            sr.sharedMaterial = mat;
            return sr;
        }

        Light2D NewLight(string name, Light2D.LightType type)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            var l = go.AddComponent<Light2D>();
            l.lightType = type;
            l.targetSortingLayers = SortingLayer.layers.Select(s => s.id).ToArray();
            l.shadowsEnabled = false;
            return l;
        }

        // Personajes con el material de contorno (si el estilo está puesto).
        public void Outline(SpriteRenderer sr)
        {
            if (sr == null) return;
            if (defaultSprite == null && sr.sharedMaterial != CharacterMaterial) defaultSprite = sr.sharedMaterial;
            sr.sharedMaterial = styled ? CharacterMaterial : defaultSprite;
        }

        // ---------- tiles del estilo ----------

        static Sprite TileSprite(System.Func<int, int, Color> px)
        {
            const int n = 16;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                    tex.SetPixel(x, y, px(x, y));
            tex.Apply();
            // Pivote en el centro: el tilemap pone el pivote en el centro de la celda (tileAnchor 0,5).
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n / RoomCoords.TileSize, 0, SpriteMeshType.FullRect);
        }

        static Tile NewTile(string name, Sprite sprite)
        {
            var t = ScriptableObject.CreateInstance<Tile>();
            t.name = name;
            t.sprite = sprite;
            t.flags = TileFlags.LockColor;
            return t;
        }

        void BuildTiles()
        {
            var rng = new System.Random(3);
            float N() => (float)rng.NextDouble();
            // Macizo: piedra con algo de grano y vetas oscuras.
            var solidSprite = TileSprite((x, y) =>
            {
                float v = 0.86f + 0.1f * N() - ((x * 7 + y * 3) % 11 == 0 ? 0.12f : 0);
                return new Color(v, v, v, 1);
            });
            // Borde de arriba: franja clara que se apaga hacia abajo, con alguna brizna.
            var rimSprite = TileSprite((x, y) =>
            {
                float t = (15 - y) / 15f;                   // 0 arriba
                float a = Mathf.Clamp01(1 - t * 3.2f);
                if (y == 15 && (x % 5 == 1)) a = 1;
                return new Color(1, 1, 1, a * 0.9f);
            });
            // La escalera ocupa 2 tiles: el de la izquierda lleva el larguero izquierdo y el de la derecha, el derecho.
            var ladderLeftSprite = TileSprite((x, y) =>
            {
                bool rail = x >= 2 && x <= 4, rung = x > 4 && (y % 8) is 2 or 3;
                return rail || rung ? new Color(1, 1, 1, 1) : Color.clear;
            });
            var ladderRightSprite = TileSprite((x, y) =>
            {
                bool rail = x >= 11 && x <= 13, rung = x < 11 && (y % 8) is 2 or 3;
                return rail || rung ? new Color(1, 1, 1, 1) : Color.clear;
            });
            var waterSprite = TileSprite((x, y) => new Color(1, 1, 1, 0.55f + 0.05f * Mathf.Sin(x * 0.8f + y * 0.3f)));
            var lavaSprite = TileSprite((x, y) => new Color(1, 0.85f + 0.15f * N(), 0.7f + 0.3f * N(), 1));
            solidTile = NewTile("Macizo", solidSprite);
            solidInnerTile = NewTile("Macizo (dentro)", solidSprite);
            solidDeepTile = NewTile("Macizo (fondo)", solidSprite);
            rimTile = NewTile("Borde", rimSprite);
            ladderTile = NewTile("Escalera (izquierda)", ladderLeftSprite);
            ladderRightTile = NewTile("Escalera (derecha)", ladderRightSprite);
            waterTile = NewTile("Agua", waterSprite);
            lavaTile = NewTile("Lava", lavaSprite);
            bossTile = NewTile("Pasable en el combate", solidSprite);
            tiles = new MapView.StyleTiles { Rock = props.Rock, Door = props.Door, KeyDoor = props.KeyDoor, Revolving = props.Revolving, Solid = solidTile, SolidInner = solidInnerTile, SolidDeep = solidDeepTile, Rim = rimTile, Ladder = ladderTile, LadderRight = ladderRightTile, Water = waterTile, Lava = lavaTile, BossPassable = bossTile };
        }

        void ColorTiles(WorldPalette p)
        {
            solidTile.color = p.Ground;
            solidInnerTile.color = Color.Lerp(p.Ground, Color.black, 0.4f);
            solidDeepTile.color = Color.Lerp(p.Ground, Color.black, 0.68f);
            bossTile.color = Color.Lerp(p.Ground, p.Rim, 0.18f);
            rimTile.color = p.Rim;
            ladderTile.color = ladderRightTile.color = Color.Lerp(p.Rim, p.Ground, 0.35f);
            waterTile.color = p.Liquid;
            lavaTile.color = WorldPalette.Lava;
            mapView?.RefreshStyle();
        }

        // ---------- partículas y posproceso ----------

        ParticleSystem BuildMotes()
        {
            var go = new GameObject("Motas");
            go.transform.SetParent(root, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 9f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.maxParticles = 120;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 9f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(RoomCoords.RoomSize.x, RoomCoords.RoomSize.y, 0.1f);
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.25f), new GradientAlphaKey(1, 0.7f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.25f;
            noise.frequency = 0.3f;
            noise.scrollSpeed = 0.15f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            var mat = new Material(additive) { mainTexture = dot.texture };
            r.sharedMaterial = mat;
            r.sortingOrder = -25;                   // detrás de la sala: no ensucian los macizos
            return ps;
        }

        void BuildVolume()
        {
            var go = new GameObject("Posproceso");
            go.transform.SetParent(root, false);
            volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(0.75f);
            bloom.intensity.Override(0.9f);
            bloom.scatter.Override(0.72f);
            vignette = profile.Add<Vignette>(true);
            vignette.intensity.Override(0.38f);
            vignette.smoothness.Override(0.45f);
            grade = profile.Add<ColorAdjustments>(true);
            grade.contrast.Override(12f);
            grade.saturation.Override(-6f);
            volume.sharedProfile = profile;
        }

        // ---------- estado ----------

        void Apply(bool on)
        {
            styled = on;
            Styled = on;
            root.gameObject.SetActive(true);
            foreach (var sr in new[] { sky, far, mid, near, fogFar, fogLow, front }) sr.enabled = on;
            foreach (var s in shafts) s.enabled = on;
            foreach (var (glow, light) in lamps) { glow.enabled = on; light.enabled = on; }
            playerLight.enabled = on;
            volume.enabled = on;
            if (on) motes.Play(); else motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (!on)
            {
                globalLight.color = Color.white;
                globalLight.intensity = 1f;
            }
            if (mapView != null && mapView.Style != (on ? tiles : null))
                mapView.SetStyle(on ? tiles : null, placeholderTiles);
            if (player != null)
            {
                Outline(player.body);
                Outline(player.sword);
                player.Art = on ? characters : null;
                EnemyArt = on ? enemyArt : null;
                Props = on ? props : null;
                Characters = on ? characters : null;
                player.Slash = slash;
            }
            shownRoom = null;                        // vuelve a montar la sala
        }

        // Prueba la paleta de otro mapa en la sala actual (-1 = la del mapa). Las capturas (VisualPreview) quitan el fundido.
        public void PreviewPalette(int index, bool noFade = false)
        {
            paletteOverride = index;
            shownRoom = null;
            skipFade = noFade;
            if (noFade) fadeLeft = 0;
        }

        bool skipFade;

        int MapIndex()
        {
            string m = game != null ? game.mapName : "castle";
            if (m != null && m.StartsWith("world") && int.TryParse(m.Substring(5), out int k)) return k;
            return 0;
        }

        void LateUpdate()
        {
            if (game == null || game.Motor == null) return;
            var kb = Keyboard.current;
            if (kb != null && kb.f7Key.wasPressedThisFrame)
            {
                Apply(!styled);
                game.Say(styled ? "Estilo del remake" : "Estilo provisional");
            }
            if (kb != null && kb.f6Key.wasPressedThisFrame && styled)
            {
                paletteOverride = (paletteOverride + 2) % (WorldPalette.All.Length + 1) - 1;   // -1 = la del mapa
                shownRoom = null;
                game.Say(paletteOverride < 0 ? "Paleta del mapa" : $"Paleta: {WorldPalette.For(paletteOverride).Name}");
            }
            if (!subscribed && game.Enemies != null)
            {
                subscribed = true;
                game.Enemies.Hit += e => { Flash(game.EnemyView(e), 0.12f); HitEffects.EnemyHit(e, game.Motor); };
                game.Enemies.Clinked += e => HitEffects.Clink(e, game.Motor);
                game.Enemies.Killed += e => HitEffects.EnemyKilled(e, new Color(0.82f, 0.82f, 0.88f));
            }

            bool busy = game.AttractRunning || game.EndingRunning;
            root.gameObject.SetActive(!busy);
            if (busy) return;

            var motor = game.Motor;
            bool mapChanged = shownMap != game.mapName;
            if (mapChanged || motor.Room != shownRoom)
            {
                if (mapChanged && shownMap != null) StartFade(MapFadeSeconds);
                shownMap = game.mapName;
                shownRoom = motor.Room;
                if (styled) BuildRoom(motor.Room);
            }

            // Golpe al jugador: destello rojo.
            if (game.HurtTicks > lastHurt) Flash(player != null ? player.body : null, 0.15f, new Color(1f, 0.3f, 0.3f));
            lastHurt = game.HurtTicks;

            TickFade();
            TickFlashes();
            if (!styled) return;
            Parallax(motor);
            BossLighting();
        }

        // ---------- una sala ----------

        void BuildRoom(RoomModel room)
        {
            palette = WorldPalette.For(paletteOverride >= 0 ? paletteOverride : MapIndex());
            // Recolorear los tiles recorre todo el mapa: solo cuando cambia la paleta. Las capas del tema se generan
            // entonces todas a la vez, para no generarlas al entrar en una sala.
            if (palette != tiledPalette)
            {
                tiledPalette = palette;
                ColorTiles(palette);
                for (int v = 0; v < 3; v++)
                    foreach (var d in new[] { ShapePainter.Depth.Far, ShapePainter.Depth.Mid, ShapePainter.Depth.Near })
                        LayerSprite(palette.Theme, d, v);
                LayerSprite(palette.Theme, ShapePainter.Depth.Front, 0);
                LayerSprite(palette.Theme, ShapePainter.Depth.Front, 1);
            }
            globalLight.color = palette.Ambient;
            globalLight.intensity = palette.AmbientIntensity;
            playerLight.color = Color.Lerp(Color.white, palette.Accent, 0.35f);
            vignette.color.Override(palette.SkyTop * 0.4f);

            var center = RoomCoords.RoomOrigin(room) + RoomCoords.RoomSize / 2f;
            root.position = Vector3.zero;
            int variant = room.id % 3;
            bool flip = (room.id / 3) % 2 == 1;

            if (!skyCache.TryGetValue(palette, out var skySprite))
                skyCache[palette] = skySprite = ShapePainter.ToSprite(ShapePainter.Gradient(palette.SkyTop, palette.SkyBottom), 1, new Vector2(0.5f, 0.5f));
            sky.sprite = skySprite;
            sky.color = Color.white;
            baseColors[sky] = Color.white;
            sky.transform.localScale = new Vector3(RoomCoords.RoomSize.x * 1.4f, RoomCoords.RoomSize.y * 1.4f / 128f, 1);
            sky.transform.position = new Vector3(center.x, center.y, 0);

            Place(far, LayerSprite(palette.Theme, ShapePainter.Depth.Far, variant), palette.Far, flip, center);
            Place(mid, LayerSprite(palette.Theme, ShapePainter.Depth.Mid, (variant + 1) % 3), palette.Mid, !flip, center);
            Place(near, LayerSprite(palette.Theme, ShapePainter.Depth.Near, (variant + 2) % 3), WithAlpha(Color.Lerp(palette.Mid, palette.Ground, 0.35f), 0.6f), flip, center);
            var frontSprite = LayerSprite(palette.Theme, ShapePainter.Depth.Front, room.id % 2);
            Place(front, frontSprite, Color.Lerp(palette.Ground, Color.black, 0.75f), flip, center);
            // El primer plano cubre la sala con un 10 % de margen (las siluetas asoman por los bordes).
            front.transform.localScale = Vector3.one * (RoomCoords.RoomSize.x * 1.1f / (ShapePainter.Width / ShapePainter.PixelsPerUnit));

            PlaceFog(fogFar, center + new Vector2(0, -1.5f), new Color(palette.Fog.r, palette.Fog.g, palette.Fog.b, 0.35f), 1, 3.5f);
            PlaceFog(fogLow, center + new Vector2(0, -RoomCoords.RoomSize.y / 2 + 1.2f), new Color(palette.Fog.r, palette.Fog.g, palette.Fog.b, 0.3f), 2, 2.5f);

            var main = motes.main;
            main.startColor = new Color(palette.Motes.r, palette.Motes.g, palette.Motes.b, 0.7f);
            var vel = motes.velocityOverLifetime;
            vel.enabled = true;
            float rise = palette.Theme == Theme.Magma ? 0.6f : palette.Theme is Theme.Coral or Theme.Abyss or Theme.Sunken ? 0.35f : palette.Theme == Theme.Fungal ? -0.08f : 0.06f;
            vel.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
            vel.y = new ParticleSystem.MinMaxCurve(rise * 0.6f, rise);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            motes.transform.position = new Vector3(center.x, center.y, 0);
            motes.Clear();
            motes.Simulate(6f, true, true);
            motes.Play();

            BuildLights(room, center);
        }

        Sprite LayerSprite(Theme theme, ShapePainter.Depth depth, int variant)
        {
            var key = (theme, depth, variant);
            if (!layerCache.TryGetValue(key, out var s))
                layerCache[key] = s = ShapePainter.ToSprite(ShapePainter.Layer(theme, depth, variant + 1), ShapePainter.PixelsPerUnit, new Vector2(0.5f, 0.5f));
            return s;
        }

        void Place(SpriteRenderer sr, Sprite sprite, Color color, bool flip, Vector2 center)
        {
            sr.sprite = sprite;
            sr.color = color;
            baseColors[sr] = color;
            sr.flipX = flip;
            sr.transform.localScale = Vector3.one;
            sr.transform.position = new Vector3(center.x, center.y, 0);
        }

        void PlaceFog(SpriteRenderer sr, Vector2 pos, Color color, int seed, float height)
        {
            if (!fogCache.TryGetValue(seed, out var s))
                fogCache[seed] = s = ShapePainter.ToSprite(ShapePainter.FogBand(seed), 256f / (RoomCoords.RoomSize.x * 1.5f), new Vector2(0.5f, 0.5f));
            sr.sprite = s;
            sr.color = color;
            baseColors[sr] = color;
            sr.transform.position = new Vector3(pos.x, pos.y, 0);
            sr.transform.localScale = new Vector3(1, height / (64f / (256f / (RoomCoords.RoomSize.x * 1.5f))), 1);
        }

        // Rayos de luz desde arriba y farolas en las paredes (sitios elegidos con el número de sala, siempre los mismos).
        void BuildLights(RoomModel room, Vector2 center)
        {
            var rng = new System.Random(room.id * 977 + MapIndex());
            int nShafts = 1 + rng.Next(2);
            while (shafts.Count < nShafts) shafts.Add(Layer("Rayo de luz", -30, additive));
            for (int i = 0; i < shafts.Count; i++)
            {
                var s = shafts[i];
                s.enabled = i < nShafts;
                if (!s.enabled) continue;
                s.sprite = shaft;
                s.color = new Color(palette.Accent.r, palette.Accent.g, palette.Accent.b, 0.16f);
                float x = center.x + ((float)rng.NextDouble() - 0.5f) * RoomCoords.RoomSize.x * 0.8f;
                s.transform.position = new Vector3(x, center.y + RoomCoords.RoomSize.y / 2 + 0.5f, 0);
                s.transform.rotation = Quaternion.Euler(0, 0, ((float)rng.NextDouble() - 0.5f) * 30f);
                s.transform.localScale = new Vector3(1.6f + (float)rng.NextDouble() * 1.6f, 1.4f, 1);
            }

            // Farolas: celdas vacías junto a una pared, en la mitad de arriba de la sala.
            var spots = new List<Vector2>();
            var w = game.World;
            for (int tries = 0; tries < 200 && spots.Count < 2; tries++)
            {
                int col = 1 + rng.Next(30), row = 2 + rng.Next(10);
                int px = col * 8 + 4, py = OriginalPhysics.RoomTopPx + row * 8 + 4;
                if (w.TileAt(room, px, py) != TileClass.Empty) continue;
                bool wall = w.TileAt(room, px - 8, py) == TileClass.Solid || w.TileAt(room, px + 8, py) == TileClass.Solid;
                if (!wall) continue;
                var p = RoomCoords.PixelToWorld(room, px, py);
                if (spots.Any(o => Vector2.Distance(o, p) < 4f)) continue;
                spots.Add(p);
            }
            while (lamps.Count < spots.Count)
            {
                var glow = Layer("Farola", -29, additive);
                glow.sprite = dot;
                var light = NewLight("Luz de farola", Light2D.LightType.Point);
                lamps.Add((glow, light));
            }
            for (int i = 0; i < lamps.Count; i++)
            {
                var (glow, light) = lamps[i];
                bool on = i < spots.Count;
                glow.enabled = light.enabled = on;
                if (!on) continue;
                var p = spots[i];
                glow.transform.position = new Vector3(p.x, p.y, 0);
                glow.transform.localScale = Vector3.one * 1.3f;
                glow.color = new Color(palette.Accent.r, palette.Accent.g, palette.Accent.b, 0.8f);
                light.transform.position = new Vector3(p.x, p.y, 0);
                light.color = palette.Accent;
                light.intensity = 0.9f;
                light.pointLightOuterRadius = 3.5f;
                light.pointLightInnerRadius = 0.3f;
                light.falloffIntensity = 0.55f;
            }
        }

        // ---------- cada fotograma ----------

        void Parallax(PlayerMotor motor)
        {
            var room = motor.Room;
            var center = RoomCoords.RoomOrigin(room) + RoomCoords.RoomSize / 2f;
            var p = RoomCoords.PixelToWorld(room, motor.X, motor.Y - 8);
            Vector2 v = (p - center) * ParallaxFollow;
            float t = Time.time;
            Vector2 sway = new Vector2(Mathf.Sin(t * 0.21f), Mathf.Sin(t * 0.17f) * 0.5f) * 0.06f;
            // Una capa de profundidad k (0 lejos, 1 la sala, >1 delante) se desplaza v·(1 − k) respecto a la sala.
            Set(far, center + v * 0.85f + sway * 0.5f);
            Set(mid, center + v * 0.55f + sway);
            Set(near, center + v * 0.25f + sway * 1.4f);
            Set(front, center + v * -0.6f);
            Set(fogFar, center + new Vector2(Mathf.Sin(t * 0.05f) * 2f, -1.5f) + v * 0.7f);
            Set(fogLow, center + new Vector2(Mathf.Sin(t * 0.08f + 1) * 1.5f, -RoomCoords.RoomSize.y / 2 + 1.2f) + v * 0.3f);
            playerLight.transform.position = new Vector3(p.x, p.y, 0);
            foreach (var s in shafts)
                if (s.enabled)
                {
                    var c = s.color;
                    c.a = 0.13f + 0.04f * Mathf.Sin(t * 0.7f + s.transform.position.x);
                    s.color = c;
                }
            foreach (var (glow, light) in lamps)
                if (light.enabled)
                {
                    float f = 1f + 0.06f * Mathf.Sin(t * 7.3f + glow.transform.position.x * 3) + 0.04f * Mathf.Sin(t * 13.1f);
                    light.intensity = 0.9f * f;
                    glow.transform.localScale = Vector3.one * 1.3f * f;
                }
        }

        static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);

        // Combate contra el jefe: en el original la decoración se deshace en puntos y queda el fondo negro (BossEffects).
        // Aquí la sala se apaga con el mismo avance: las capas del fondo hacia negro, la luz global baja y se vuelve
        // roja, una luz roja sobre el jefe, más viñeta; el parpadeo del borde al invocarlo es un destello de luz.
        void BossLighting()
        {
            if (bossFx == null) bossFx = game.GetComponent<BossEffects>();
            float target = bossFx != null ? bossFx.Darkness : 0;
            darkness = Mathf.MoveTowards(darkness, target, Time.deltaTime * 2.5f);
            float d = darkness;
            foreach (var kv in baseColors)
            {
                if (kv.Key == null) continue;
                var c = kv.Value;
                var dark = Color.Lerp(c, new Color(0.05f, 0.01f, 0.02f, c.a), 0.82f * d);
                dark.a = c.a * (kv.Key == sky ? 1 : 1 - 0.35f * d);
                kv.Key.color = dark;
            }
            if (palette != null)
            {
                globalLight.color = Color.Lerp(palette.Ambient, new Color(1f, 0.55f, 0.5f), d);
                globalLight.intensity = Mathf.Lerp(palette.AmbientIntensity, 0.5f, d) + (bossFx != null && bossFx.BorderFlash ? 0.6f : 0);
                vignette.intensity.Override(0.38f + 0.22f * d);
                vignette.color.Override(Color.Lerp(palette.SkyTop * 0.4f, new Color(0.25f, 0, 0.02f), d));
            }
            playerLight.intensity = 0.55f + 0.4f * d;
            // Los aditivos con alfa casi 0 seguían viéndose (medido en las capturas del combate): se dejan de dibujar.
            foreach (var s in shafts)
            {
                if (s.enabled) { var c = s.color; c.a *= 1 - d; s.color = c; }
                s.forceRenderingOff = d > 0.9f;
            }
            foreach (var (glow, light) in lamps)
            {
                if (light.enabled) { light.intensity *= 1 - 0.8f * d; glow.color = WithAlpha(glow.color, 0.8f * (1 - d)); }
                glow.forceRenderingOff = d > 0.9f;
            }
            var boss = game.Bosses?.Boss;
            if (boss != null && boss.Alive && boss.Room == game.Motor.Room)
            {
                var (x0, y0, x1, y1) = boss.Box();
                var p = RoomCoords.PixelToWorld(boss.Room, (int)((x0 + x1) / 2), (int)((y0 + y1) / 2));
                bossLight.transform.position = new Vector3(p.x, p.y, 0);
                bossLight.intensity = 1.3f * d * (1 + 0.08f * Mathf.Sin(Time.time * 6));
            }
            else
                bossLight.intensity = Mathf.MoveTowards(bossLight.intensity, 0, Time.deltaTime * 2);
        }

        static void Set(SpriteRenderer sr, Vector2 pos) => sr.transform.position = new Vector3(pos.x, pos.y, 0);

        void StartFade(float seconds)
        {
            if (skipFade) return;
            fadeTotal = fadeLeft = seconds;
        }

        void TickFade()
        {
            if (cam != null)
            {
                fade.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0);
                fade.transform.localScale = new Vector3(cam.orthographicSize * cam.aspect * 2.2f, cam.orthographicSize * 2.2f, 1);
            }
            fadeLeft = Mathf.Max(0, fadeLeft - Time.deltaTime);
            float a = fadeTotal > 0 ? Mathf.SmoothStep(0, 1, fadeLeft / fadeTotal) : 0;
            fade.enabled = a > 0.001f;
            fade.color = new Color(0, 0, 0, a);
        }

        // ---------- destellos ----------

        public void Flash(SpriteRenderer sr, float seconds, Color? color = null)
        {
            if (sr == null || !styled) return;
            flashes[sr] = seconds;
            sr.GetPropertyBlock(block);
            block.SetColor("_FlashColor", color ?? Color.white);
            sr.SetPropertyBlock(block);
        }

        void TickFlashes()
        {
            if (flashes.Count == 0) return;
            foreach (var sr in flashes.Keys.ToList())
            {
                float left = flashes[sr] - Time.deltaTime;
                if (sr == null) { flashes.Remove(sr); continue; }
                sr.GetPropertyBlock(block);
                block.SetFloat("_Flash", left > 0 ? 1f : 0f);
                sr.SetPropertyBlock(block);
                if (left <= 0) flashes.Remove(sr); else flashes[sr] = left;
            }
        }
    }
}
