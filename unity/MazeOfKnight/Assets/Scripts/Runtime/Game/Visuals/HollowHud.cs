using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game.Visuals
{
    // Marcador con el arte de Hollow Knight (Assets/Art/Resources/Hollow/hud, fuera de git; lo importa tools/hollow/hud.py),
    // con los dos estilos. Los mismos datos que el marcador del original, a la manera de Hollow Knight:
    //  - el orbe de alma, lleno según la experiencia del que juega hacia el siguiente nivel (Hollow Knight gana alma al
    //    golpear; Galious, experiencia);
    //  - la vitalidad en máscaras, una por unidad (elegido por el usuario el 16 de octubre de 2026): se empieza con 8, cada
    //    golpe rompe tantas como daño haga y la vitalidad máxima (hasta 72) añade filas de 9;
    //  - debajo, las del otro personaje en pequeño, con sus vidas;
    //  - arriba a la derecha, geo (monedas), flechas y llaves con sus iconos, y el arma elegida;
    //  - abajo, la vida del jefe en combate, los mensajes y, al cambiar de mapa, su nombre.
    // F8 muestra también la información de pruebas (DebugHud).
    public class HollowHud : MonoBehaviour
    {
        public const int UnitsPerMask = 1, PerRow = 9;
        public static bool Active { get; private set; }

        public GameBootstrap game;
        Sprite frame, maskEmpty, geo;
        Sprite maskFull;
        Sprite crest;
        readonly Sprite[] soul = new Sprite[11];
        readonly List<Sprite> maskBreak = new List<Sprite>();
        readonly Dictionary<(int, int), float> breaking = new Dictionary<(int, int), float>();   // (personaje, máscara) → inicio
        readonly Dictionary<(int, int), float> lastFill = new Dictionary<(int, int), float>();
        InventoryMenu menu;
        ShrineScreen shrine;
        bool ownsDebugKey;
        string shownMap;
        float titleTime = -10;

        public static bool Available => Resources.Load<Sprite>("Hollow/hud/marco") != null;

        void Awake()
        {
            frame = Resources.Load<Sprite>("Hollow/hud/marco");
            maskEmpty = Resources.Load<Sprite>("Hollow/hud/mascara_vacia");
            maskFull = Resources.Load<Sprite>("Hollow/hud/mascara_00");
            geo = Resources.Load<Sprite>("Hollow/hud/geo");
            for (int k = 0; k <= 10; k++) soul[k] = Resources.Load<Sprite>($"Hollow/hud/alma_{k:00}");
            for (int i = 0; ; i++)
            {
                var s = Resources.Load<Sprite>($"Hollow/hud/mascara_rota_{i:00}");
                if (s == null) break;
                maskBreak.Add(s);
            }
            Active = frame != null;
        }

        void OnDestroy() => Active = false;

        void Start() => ownsDebugKey = FindAnyObjectByType<RemakeHud>() == null;   // F8 lo cambia RemakeHud si existe

        void Update()
        {
            if (ownsDebugKey && Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame) RemakeHud.ShowDebug = !RemakeHud.ShowDebug;
            if (game != null && game.mapName != shownMap)
            {
                shownMap = game.mapName;
                titleTime = Time.time;
            }
        }

        // Un sprite en su lienzo, escalado por k, con la esquina de arriba a la izquierda en (x, y). Con 'fill' < 1 solo se
        // dibuja la parte de abajo (lo que está lleno), en su sitio.
        static void Draw(Sprite sp, float x, float y, float k, float fill = 1, float alpha = 1)
        {
            if (sp == null || fill <= 0) return;
            var tr = sp.textureRect;
            var tex = sp.texture;
            float w = tr.width * k, h = tr.height * k;
            fill = Mathf.Clamp01(fill);
            var dest = new Rect(x, y + h * (1 - fill), w, h * fill);
            var uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height * fill / tex.height);
            var old = GUI.color;
            GUI.color = new Color(1, 1, 1, alpha);
            GUI.DrawTextureWithTexCoords(dest, tex, uv);
            GUI.color = old;
        }

        // Filas de máscaras que ocupa un personaje.
        int Rows(int who) => Mathf.Max(1, Mathf.CeilToInt(Mathf.CeilToInt(game.MaxVitality[who] / (float)UnitsPerMask) / (float)PerRow));

        // Las máscaras de un personaje, en filas de PerRow: llena o vacía; rompiéndose al vaciarse.
        void Masks(int who, float x, float y, float k, float step, float rowStep)
        {
            int max = game.MaxVitality[who], v = Mathf.Max(0, game.Vitality[who]);
            int n = Mathf.Max(1, Mathf.CeilToInt(max / (float)UnitsPerMask));
            for (int i = 0; i < n; i++)
            {
                float fill = Mathf.Clamp01((v - i * UnitsPerMask) / (float)UnitsPerMask);
                float mx = x + (i % PerRow) * step;
                float rowY = y + (i / PerRow) * rowStep;
                var key = (who, i);
                if (lastFill.TryGetValue(key, out float before) && before > 0 && fill <= 0) breaking[key] = Time.time;
                lastFill[key] = fill;
                Draw(maskEmpty, mx, rowY, k);
                if (fill > 0) Draw(maskFull, mx, rowY, k, fill);
                else if (breaking.TryGetValue(key, out float t0) && maskBreak.Count > 0)
                {
                    int f = (int)((Time.time - t0) * 20);
                    if (f < maskBreak.Count) Draw(maskBreak[f], mx, rowY, k);
                    else breaking.Remove(key);
                }
            }
        }

        void OnGUI()
        {
            if (!Active || game == null || game.Motor == null || game.AttractRunning || game.EndingRunning) return;
            if (menu == null) menu = game.GetComponent<InventoryMenu>();
            if (shrine == null) shrine = game.GetComponent<ShrineScreen>();
            if (menu != null && menu.Open || shrine != null && shrine.Open) return;
            float s = UiKit.Scale, pad = 12 * s, k = 1.0f * s;
            var m = game.Motor;
            int me = (int)m.Character, other = 1 - me;

            // Orbe de alma: la experiencia del que juega (de 48).
            Draw(frame, pad, pad, k);
            if (game.Inventory != null)
            {
                int level = Mathf.Clamp(Mathf.RoundToInt(game.Inventory.Experience[me] / 48f * 10), 0, 10);
                Draw(soul[level], pad, pad, k);
            }

            // Máscaras del que juega, a la derecha del orbe y bajo el trazo; las del otro, en pequeño, debajo. Se colocan por
            // la máscara que se ve (en su lienzo de 126 × 167 empieza en x 32, y 37 y mide 57 × 70).
            float km = 0.42f * s, ks = 0.22f * s, step = 26 * s, row = 32 * s, stepS = 14 * s, rowS = 17 * s;
            float maskX = pad + 128 * k, maskY = pad + 58 * k;
            Masks(me, maskX - 32 * km, maskY - 37 * km, km, step, row);
            int nMe = Mathf.Min(PerRow, Mathf.CeilToInt(game.MaxVitality[me] / (float)UnitsPerMask));
            // Sin vidas (RemakeRules.HollowDeath) no hay contador.
            if (!RemakeRules.HollowDeath)
                UiKit.Text(new Rect(maskX + nMe * step + 6 * s, maskY + 4 * s, 300 * s, 20 * s),
                    $"{m.Character.ToString().ToUpperInvariant()}  ×{game.Lives[me]}", 14, TextAnchor.UpperLeft, UiKit.Bone, true);
            float otherY = maskY + Rows(me) * row + 6 * s;
            // El otro personaje, en pequeño (con RemakeRules.SoloKnight no hay otro).
            if (!RemakeRules.SoloKnight)
            {
                Masks(other, maskX - 32 * ks, otherY - 37 * ks, ks, stepS, rowS);
                int nOther = Mathf.Min(PerRow, Mathf.CeilToInt(game.MaxVitality[other] / (float)UnitsPerMask));
                UiKit.Text(new Rect(maskX + nOther * stepS + 6 * s, otherY - 3 * s, 220 * s, 18 * s),
                    $"{(Character)other}  ×{game.Lives[other]}", 12, TextAnchor.UpperLeft, UiKit.Dim);
            }

            // Geo (monedas), flechas y llaves, y el arma elegida.
            float icon = 24 * s, rx = Screen.width - pad;
            void Counter(Sprite sp, int value)
            {
                var tr = new Rect(rx - 46 * s, pad, 46 * s, icon);
                UiKit.Text(tr, value.ToString("000"), 18, TextAnchor.MiddleLeft, UiKit.Bone, true);
                UiKit.Icon(new Rect(tr.x - icon - 2 * s, pad, icon, icon), sp);
                rx = tr.x - icon - 14 * s;
            }
            Counter(HollowItems.Icon(0x2F), game.Keys);
            Counter(HollowItems.Icon(0x2D), game.Arrows);
            Counter(geo, game.Coins);
            if (game.SelectedWeapon > 0)
            {
                var wr = new Rect(Screen.width - pad - 34 * s, pad + icon + 10 * s, 34 * s, 34 * s);
                UiKit.Icon(wr, HollowItems.Icon(game.SelectedWeapon));
                UiKit.Text(new Rect(wr.x - 200 * s, wr.y, 192 * s, wr.height), game.WeaponName(game.SelectedWeapon), 13, TextAnchor.MiddleRight, UiKit.Dim);
            }

            // Jefe en combate.
            var bosses = game.Bosses;
            bool bossBar = bosses?.Boss != null && bosses.Boss.Alive;
            if (bossBar)
            {
                float bw = Screen.width * 0.5f;
                var br = new Rect((Screen.width - bw) / 2, Screen.height - pad - 14 * s, bw, 10 * s);
                UiKit.Text(new Rect(br.x, br.y - 28 * s, bw, 24 * s), bosses.Info.name.ToUpperInvariant(), 18, TextAnchor.LowerCenter, UiKit.Bone, true);
                UiKit.Vessel(br, bosses.Boss.Hp, 0, bosses.Boss.MaxHp, new Color(0.9f, 0.9f, 0.92f));
            }
            else if (bosses != null && bosses.Listening)
                UiKit.Text(new Rect(0, Screen.height - pad - 30 * s, Screen.width, 26 * s), "Escribe la palabra de la lápida", 15, TextAnchor.MiddleCenter, UiKit.Dim);

            // Junto a un banco: "Descansar", como en Hollow Knight; al guardar, la cresta abajo a la derecha.
            if (game.NearBench && Camera.main != null)
            {
                var p = Camera.main.WorldToScreenPoint(game.NearBenchLabel);
                UiKit.Text(new Rect(p.x - 150 * s, Screen.height - p.y - 15 * s, 300 * s, 30 * s), "↓  Descansar", 18, TextAnchor.MiddleCenter, UiKit.Bone, true);
            }
            if (Time.time < game.SavedIconUntil)
            {
                crest ??= Resources.Load<Sprite>("Hollow/menu/remate");
                float a = Mathf.Clamp01((game.SavedIconUntil - Time.time) / 0.4f) * (0.6f + 0.4f * Mathf.Sin(Time.time * 6));
                UiKit.Icon(new Rect(Screen.width - 70 * s, Screen.height - 90 * s, 44 * s, 60 * s), crest, a);
            }

            // Mensaje (con la barra del jefe, por encima de su nombre).
            if (!string.IsNullOrEmpty(game.Message))
                UiKit.Text(new Rect(0, Screen.height - pad - (bossBar ? 110 : 70) * s, Screen.width, 28 * s), game.Message, 17, TextAnchor.MiddleCenter, UiKit.Bone);

            // Nombre del mapa al entrar (3,5 s).
            float t = Time.time - titleTime;
            if (t < 3.5f)
            {
                float a = Mathf.Clamp01(t / 0.6f) * Mathf.Clamp01((3.5f - t) / 0.8f);
                int w = game.mapName != null && game.mapName.StartsWith("world") && int.TryParse(game.mapName.Substring(5), out int kk) ? kk : 0;
                UiKit.TitleLine(new Rect(0, Screen.height * 0.2f, Screen.width, 48 * s), w == 0 ? "CASTILLO DE GRECIA" : $"MUNDO {w}", 34, a);
            }
        }
    }
}
