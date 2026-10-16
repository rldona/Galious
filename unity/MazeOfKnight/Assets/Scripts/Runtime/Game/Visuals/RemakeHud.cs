using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game.Visuals
{
    // Marcador del remake (docs/design_bible/12_estilo_visual.md, "Interfaz"), en lugar del texto de pruebas cuando el
    // estilo está puesto (F7). Los mismos datos que el marcador del original:
    //  - arriba a la izquierda, medallón con el personaje que juega, su vitalidad (barra ornamentada que crece con el
    //    máximo, como la del original: 1 unidad por paso, marcas cada 8), la del otro con sus vidas, y la experiencia;
    //  - arriba a la derecha, flechas, monedas y llaves con sus iconos, y el arma elegida;
    //  - abajo, la vida del jefe en combate y los mensajes; al cambiar de mapa, su nombre en grande.
    // F8 muestra también la información de pruebas (DebugHud).
    public class RemakeHud : MonoBehaviour
    {
        public GameBootstrap game;
        public static bool ShowDebug;

        string shownMap;
        InventoryMenu menu;
        ShrineScreen shrine;
        float titleTime = -10;
        static readonly Color PopolonColor = new Color(0.86f, 0.27f, 0.22f), AphroditeColor = new Color(0.66f, 0.45f, 0.9f);

        void Update()
        {
            if (RemakeRules.DebugKeys && Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame) ShowDebug = !ShowDebug;
            if (game != null && game.mapName != shownMap)
            {
                shownMap = game.mapName;
                titleTime = Time.time;
            }
        }

        void OnGUI()
        {
            if (game == null || game.Motor == null || !Atmosphere.Styled || game.AttractRunning || game.EndingRunning) return;
            if (HollowHud.Active) return;   // el marcador de Hollow Knight lo sustituye
            // El menú de objetos y los santuarios tapan la pantalla: ya enseñan lo suyo.
            if (menu == null) menu = game.GetComponent<InventoryMenu>();
            if (shrine == null) shrine = game.GetComponent<ShrineScreen>();
            if (menu != null && menu.Open || shrine != null && shrine.Open) return;
            float s = UiKit.Scale, pad = 18 * s;
            var m = game.Motor;
            int me = (int)m.Character, other = 1 - me;

            // Medallón con el personaje.
            var med = new Rect(pad, pad, 84 * s, 84 * s);
            UiKit.Medallion(med);
            var art = Atmosphere.Characters;
            if (art != null)
            {
                var sp = art.Get(m.Character, CharacterArt.Anim.Idle, 0, game.Inventory?.ShieldLevel ?? 0);
                UiKit.Icon(new Rect(med.x + 10 * s, med.y + 8 * s, med.width - 20 * s, med.height - 16 * s), sp, 1, false, CharacterArt.PortraitCrop);
            }

            // Vitalidad del que juega (y lo que se va a perder o ganar) y del otro.
            float unit = 6.5f * s;
            var colorMe = m.Character == Character.Popolon ? PopolonColor : AphroditeColor;
            var colorOther = m.Character == Character.Popolon ? AphroditeColor : PopolonColor;
            float x0 = med.xMax + 10 * s;
            UiKit.Text(new Rect(x0, pad - 2 * s, 300 * s, 22 * s), m.Character.ToString().ToUpperInvariant(), 17, TextAnchor.UpperLeft, UiKit.Bone, true);
            UiKit.Vessel(new Rect(x0, pad + 22 * s, game.MaxVitality[me] * unit + 4 * s, 16 * s),
                game.Vitality[me], game.PendingVitality[me], game.MaxVitality[me], colorMe);
            var otherName = ((Character)other).ToString();
            UiKit.Vessel(new Rect(x0, pad + 46 * s, game.MaxVitality[other] * unit * 0.5f + 4 * s, 8 * s),
                game.Vitality[other], game.PendingVitality[other], game.MaxVitality[other], colorOther * 0.85f);
            UiKit.Text(new Rect(x0 + game.MaxVitality[other] * unit * 0.5f + 12 * s, pad + 40 * s, 200 * s, 18 * s),
                $"{otherName}  ×{game.Lives[other]}", 13, TextAnchor.UpperLeft, UiKit.Dim);
            if (game.Inventory != null)
            {
                float exp = game.Inventory.Experience[me] / 48f;
                var er = new Rect(x0, pad + 62 * s, 48 * unit * 0.5f, 4 * s);
                UiKit.Box(er, new Color(0, 0, 0, 0.6f));
                UiKit.Box(new Rect(er.x, er.y, er.width * Mathf.Clamp01(exp), er.height), UiKit.Gold);
                UiKit.Text(new Rect(er.xMax + 8 * s, er.y - 7 * s, 120 * s, 18 * s), $"EXP {game.Inventory.Experience[me]}/48", 11, TextAnchor.UpperLeft, UiKit.Dim);
            }

            // Recursos y arma (arriba a la derecha).
            var props = Atmosphere.Props;
            float icon = 26 * s, rx = Screen.width - pad;
            void Counter(int type, int value)
            {
                var tr = new Rect(rx - 52 * s, pad, 52 * s, icon);
                UiKit.Text(tr, value.ToString("000"), 18, TextAnchor.MiddleLeft, UiKit.Bone, true);
                if (props != null) UiKit.Icon(new Rect(tr.x - icon - 2 * s, pad, icon, icon), props.Item(type));
                rx = tr.x - icon - 14 * s;
            }
            Counter(0x2F, game.Keys);
            Counter(0x2E, game.Coins);
            Counter(0x2D, game.Arrows);
            if (game.SelectedWeapon > 0 && props != null)
            {
                var wr = new Rect(Screen.width - pad - 40 * s, pad + icon + 10 * s, 40 * s, 40 * s);
                UiKit.Panel(new Rect(wr.x - 6 * s, wr.y - 6 * s, wr.width + 12 * s, wr.height + 12 * s));
                UiKit.Icon(wr, props.Item(game.SelectedWeapon));
                UiKit.Text(new Rect(wr.x - 200 * s, wr.y, 192 * s, wr.height), game.WeaponName(game.SelectedWeapon), 13, TextAnchor.MiddleRight, UiKit.Dim);
            }

            // Jefe en combate.
            var bosses = game.Bosses;
            if (bosses?.Boss != null && bosses.Boss.Alive)
            {
                float bw = Screen.width * 0.5f;
                var br = new Rect((Screen.width - bw) / 2, Screen.height - pad - 14 * s, bw, 12 * s);
                UiKit.Text(new Rect(br.x, br.y - 28 * s, bw, 24 * s), bosses.Info.name.ToUpperInvariant(), 18, TextAnchor.LowerCenter, UiKit.Bone, true);
                UiKit.Vessel(br, bosses.Boss.Hp, 0, bosses.Boss.MaxHp, new Color(0.75f, 0.12f, 0.15f));
            }
            else if (bosses != null && bosses.Listening)
                UiKit.Text(new Rect(0, Screen.height - pad - 30 * s, Screen.width, 26 * s), "Escribe la palabra de la lápida", 15, TextAnchor.MiddleCenter, UiKit.Dim);

            // Mensaje.
            if (!string.IsNullOrEmpty(game.Message))
                UiKit.Text(new Rect(0, Screen.height - pad - 70 * s, Screen.width, 28 * s), game.Message, 17, TextAnchor.MiddleCenter, UiKit.Bone);

            // Nombre del mapa al entrar: aparece, se queda y se va (3,5 s).
            float t = Time.time - titleTime;
            if (t < 3.5f)
            {
                float a = Mathf.Clamp01(t / 0.6f) * Mathf.Clamp01((3.5f - t) / 0.8f);
                string name = MapTitle(game.mapName);
                UiKit.TitleLine(new Rect(0, Screen.height * 0.2f, Screen.width, 48 * s), name, 34, a);
                var pal = WorldPalette.For(MapIndex(game.mapName));
                UiKit.Text(new Rect(0, Screen.height * 0.2f + 48 * s, Screen.width, 26 * s), Subtitle(pal), 15, TextAnchor.MiddleCenter, UiKit.Dim, false, a);
            }
        }

        static int MapIndex(string map) =>
            map != null && map.StartsWith("world") && int.TryParse(map.Substring(5), out int k) ? k : 0;

        static string MapTitle(string map)
        {
            int k = MapIndex(map);
            return k == 0 ? "CASTILLO DE GRECIA" : $"MUNDO {k}";
        }

        static string Subtitle(WorldPalette p)
        {
            int i = p.Name.IndexOf(':');
            return i >= 0 ? p.Name.Substring(i + 1).Trim() : p.Name;
        }
    }
}
