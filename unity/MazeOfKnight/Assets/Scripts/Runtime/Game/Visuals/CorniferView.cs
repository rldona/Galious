using MazeOfKnight.Data;
using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Cornifer en su sala (RemakeRules.HollowMaps; arte, tarareo y frases en Assets/Art/Resources/Hollow/cornifer, fuera
    // de git, de tools/hollow/cornifer.py): sentado dibujando su mapa, tarareando más fuerte cuanto más cerca se está,
    // "↓ Escuchar" a su lado y, al hablarle, su conversación como en Hollow Knight. La primera vez se presenta; luego
    // saluda con lo que dice en la zona de Hollow Knight de ese mundo, pregunta si se quiere el mapa (Sí / No) y, al
    // venderlo, lo que dice después. Sin sus frases importadas usa unas propias.
    public class CorniferView : MonoBehaviour
    {
        public GameBootstrap game;
        public static bool Available => Resources.Load<Sprite>("Hollow/cornifer/cornifer_escribir_00") != null;

        // La zona de Hollow Knight de cada mundo, para sus frases (la de los Cruces no, que habla de Bocasucia).
        static readonly string[] Zone = { "CROSSROADS", "GREENPATH", "WATERWAYS", "FOG_CANYON", "MINES", "CITY", "FUNGAL_WASTES", "ABYSS", "DEEPNEST", "OUTSKIRTS" };
        const float Fps = 10, HearPx = 200;

        Sprite[] writing, talking;
        Dictionary<string, string> lines;
        GameObject view;
        SpriteRenderer sr;
        AudioSource hum;
        Cornifers.Spot? here;

        enum Stage { None, Pages, Prompt, After }
        Stage stage;
        List<string> pages = new List<string>();
        int page, world, choice;
        float shownAt;
        public bool Talking => stage != Stage.None;

        void Awake()
        {
            writing = Frames("escribir");
            talking = Frames("hablar");
            lines = new Dictionary<string, string>();
            var json = Resources.Load<TextAsset>("Hollow/cornifer/textos");
            if (json != null)
                foreach (var l in JsonUtility.FromJson<Lines>(json.text).lines) lines[l.key] = l.text;
            hum = gameObject.AddComponent<AudioSource>();
            hum.clip = Resources.Load<AudioClip>("Hollow/cornifer/tarareo");
            hum.loop = true;
            hum.volume = 0;
        }

        void Start()
        {
            if (game != null && game.World != null) UseMap();
        }

        [System.Serializable] class Entry { public string key, text; }
        [System.Serializable] class Lines { public Entry[] lines = new Entry[0]; }

        static Sprite[] Frames(string clip)
        {
            var l = new List<Sprite>();
            for (int i = 0; ; i++)
            {
                var s = Resources.Load<Sprite>($"Hollow/cornifer/cornifer_{clip}_{i:00}");
                if (s == null) break;
                l.Add(s);
            }
            return l.ToArray();
        }

        string Line(string key, string fallback) => lines.TryGetValue(key, out var t) ? t : fallback;

        // Al cargar un mapa: Cornifer en la sala de ese mundo, si hay.
        public void UseMap()
        {
            if (view != null) Destroy(view);
            view = null;
            here = null;
            int w = game.Inventory.CurrentWorld;
            foreach (var c in game.CorniferList.All)
                if (c.World == w) here = c;
            if (!(here is Cornifers.Spot spot) || !(game.World?.Room(spot.Room) is RoomModel room)) return;
            view = new GameObject($"Cornifer (sala {spot.Room})");
            sr = view.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;
            var lit = Resources.Load<Material>("Materials/SpriteLit");
            if (lit != null) sr.sharedMaterial = lit;
            // A 8 texels por píxel del original, como el resto del arte de Hollow Knight.
            float k = 16f / RoomCoords.PixelsPerUnit;
            view.transform.localScale = new Vector3(k, k, 1);
            var p = RoomCoords.PixelToWorld(room, spot.X, spot.Y);
            view.transform.position = new Vector3(p.x, p.y, 0);
        }

        public void Talk(int w)
        {
            world = w;
            page = 0;
            shownAt = Time.unscaledTime;
            pages.Clear();
            if (game.HasMapOf(w)) Say(Line(Zone[w] + "_BOUGHT", "¿Qué te parece mi mapa? Todo mapa es un trabajo en progreso."), Stage.After);
            else
            {
                string text = !game.CorniferMet ? Line("CORNIFER_MEET", "¿Hmm? Ah, hola. Soy cartógrafo, y ahora mismo estoy dibujando esta zona. ¿Te gustaría comprar lo que llevo hasta ahora?")
                    : game.CorniferGreeted.Contains(w) ? Line("CORNIFER_AGAIN", "¿Hmm? ¿Has cambiado de idea sobre comprar un mapa?")
                    : Line(Zone[w] + "_GREET", "¡Hola otra vez! ¿Te interesa comprar una copia de mi mapa de esta zona?");
                game.CorniferMet = true;
                game.CorniferGreeted.Add(w);
                Say(text, Stage.Pages);
            }
            game.SetPaused(true);
        }

        void Say(string text, Stage next)
        {
            pages = new List<string>(text.Split(new[] { "<page>" }, System.StringSplitOptions.RemoveEmptyEntries));
            page = 0;
            stage = next;
            shownAt = Time.unscaledTime;
        }

        void Close()
        {
            stage = Stage.None;
            game.SetPaused(false);
        }

        void Update()
        {
            if (game == null) return;
            // Tarareo: más fuerte cerca, nada fuera de su sala.
            float target = 0;
            if (view != null && here is Cornifers.Spot c && game.Motor != null && game.Motor.Room?.id == c.Room && !game.Busy)
                target = Mathf.Clamp01(1 - Mathf.Abs(game.Motor.X - c.X) / HearPx) * GameSettings.EffectsVolume * (Talking ? 0.3f : 1f);
            hum.volume = Mathf.MoveTowards(hum.volume, target, Time.unscaledDeltaTime);
            if (hum.volume > 0 && !hum.isPlaying && hum.clip != null) hum.Play();
            if (hum.volume == 0 && hum.isPlaying) hum.Stop();
            // Animación.
            if (view != null && sr != null)
            {
                var clip = Talking || game.NearCornifer ? talking : writing;
                if (clip.Length > 0) sr.sprite = clip[(int)(Time.time * Fps) % clip.Length];
            }
            if (!Talking || Time.unscaledTime - shownAt < 0.25f) return;
            if (stage == Stage.Prompt)
            {
                if (HollowMenus.Left || HollowMenus.Up) choice = 0;
                if (HollowMenus.Right || HollowMenus.Down) choice = 1;
                if (HollowMenus.Back) choice = 1;
                if (!HollowMenus.Accept && !HollowMenus.Back) return;
                if (choice == 0 && game.BuyMap(world)) Say(Line(Zone[world] + "_BOUGHT", "Que te sea útil. Todo mapa es un trabajo en progreso."), Stage.After);
                else if (choice == 0) Say(Line("CORNIFER_NOT_ENOUGH", "¿No tienes geo suficiente? Ah, lo entiendo."), Stage.After);
                else Say(Line("CORNIFER_REFUSE", "¿Entonces no te interesa? Perfectamente comprensible. Quizás nos volvamos a encontrar."), Stage.After);
                return;
            }
            if (!HollowMenus.Accept && !HollowMenus.Back) return;
            if (page + 1 < pages.Count) { page++; shownAt = Time.unscaledTime; return; }
            if (stage == Stage.Pages) { stage = Stage.Prompt; choice = 0; shownAt = Time.unscaledTime; return; }
            Close();
        }

        // Sin la pasada de maquetación de IMGUI (no se usa GUILayout): era la mayor parte de la basura de cada fotograma.
        void OnEnable() => useGUILayout = false;

        void OnGUI()
        {
            if (game == null) return;
            float s = UiKit.Scale, W = Screen.width;
            if (!Talking)
            {
                if (game.NearCornifer && !game.MenuOpen && Camera.main != null && here is Cornifers.Spot c)
                {
                    var p = Camera.main.WorldToScreenPoint(RoomCoords.PixelToWorld(game.Motor.Room, c.X, c.Y - 40));
                    UiKit.Text(new Rect(p.x - 150 * s, Mathf.Max(70 * s, Screen.height - p.y - 15 * s), 300 * s, 30 * s), "↓  Escuchar", 18, TextAnchor.MiddleCenter, UiKit.Bone, true);
                }
                return;
            }
            GUI.depth = -9;
            // La caja de diálogo de Hollow Knight: arriba, negra, con filigrana arriba y abajo.
            float bw = Mathf.Min(W - 60 * s, 900 * s), bh = 190 * s;
            var r = new Rect((W - bw) / 2, 90 * s, bw, bh);
            UiKit.Box(new Rect(r.x - 30 * s, r.y - 20 * s, r.width + 60 * s, r.height + 40 * s), new Color(0, 0, 0, 0.82f));
            HollowMenus.Divider(r.center.x, r.y - 44 * s, 200 * s);
            HollowMenus.Divider(r.center.x, r.yMax + 22 * s, 200 * s);
            if (stage == Stage.Prompt)
            {
                UiKit.Text(new Rect(r.x, r.y + 20 * s, r.width, 40 * s), Line("CORNIFER_PROMPT", "¿Comprar un mapa de la zona?"), 24, TextAnchor.MiddleCenter, UiKit.Bone);
                UiKit.Text(new Rect(r.x, r.y + 64 * s, r.width, 30 * s), $"{Cornifers.Price(world)} geo   (tienes {game.Coins})", 18, TextAnchor.MiddleCenter, UiKit.Gold, true);
                HollowMenus.List(new[] { "Sí", "No" }, choice, r.center.x, r.y + 104 * s, 38, 22);
                return;
            }
            if (page == 0 && stage == Stage.Pages || page == 0 && stage == Stage.After)
                UiKit.Text(new Rect(r.x, r.y - 6 * s, r.width, 30 * s), "Cornifer", 18, TextAnchor.MiddleLeft, UiKit.Dim, true);
            UiKit.Text(new Rect(r.x, r.y + 26 * s, r.width, bh - 40 * s), pages.Count > 0 ? pages[page] : "", 21, TextAnchor.UpperLeft, UiKit.Bone);
            if (Time.unscaledTime - shownAt > 0.4f)
                UiKit.Text(new Rect(r.x, r.yMax - 26 * s, r.width, 24 * s), "▾", 16, TextAnchor.MiddleRight, UiKit.Dim, false, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 5));
        }

        // Para las capturas.
        public void DebugPrompt(int w) { Talk(w); stage = Stage.Prompt; choice = 0; }
        public void DebugClose() => Close();
    }
}
