using System.Linq;
using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game
{
    // Menú de objetos (F1; docs/items_enemies.md, "Inventario (menú F1)"). Pausa el juego. Filas: personaje y armas
    // (↑ ↓); ← → cambia de personaje o de arma; F1 cierra y aplica. Return usa el Angel Ring; 1–9, el Ala (en el
    // castillo). Se dibuja sobre la pantalla del original, de 256 × 192 px. [I] Los iconos son provisionales: cuadros
    // con el nombre abreviado.
    public class InventoryMenu : MonoBehaviour
    {
        public GameBootstrap game;
        public bool Open { get; private set; }
        int row;                 // 0 personaje, 1 armas
        Character character;
        int weapon;
        GUIStyle small, label;
        Texture2D white;

        static readonly int[] WeaponX = { 112, 136, 160, 176, 192, 216 };

        public void Toggle()
        {
            if (!Open) OpenMenu();
            else CloseMenu();
        }

        void OpenMenu()
        {
            if (!game.CanOpenMenu) return;
            Open = true;
            row = 0;
            character = game.Motor.Character;
            game.Lives[(int)character]++;      // el menú cuenta la vida en juego (E058 + E501)
            weapon = game.SelectedWeapon != 0 ? game.SelectedWeapon : OwnedWeapons().FirstOrDefault();
            game.SetPaused(true);
        }

        void CloseMenu()
        {
            Open = false;
            game.Lives[(int)character]--;      // al cerrar, el elegido gasta una
            game.ApplyMenu(character, weapon);
            game.SetPaused(false);
        }

        // Cerrar sin elegir (Angel Ring, Ala): sigue el mismo personaje.
        void CloseKeeping()
        {
            Open = false;
            game.Lives[(int)game.Motor.Character]--;
            game.SetPaused(false);
        }

        int[] OwnedWeapons() => Enumerable.Range(1, 6).Where(game.Inventory.Has).ToArray();

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || game == null || game.Inventory == null) return;
            if (kb.f1Key.wasPressedThisFrame || kb.iKey.wasPressedThisFrame && !game.TypingWord || Gamepad.current?.startButton.wasPressedThisFrame == true)
            {
                Toggle();
                return;
            }
            if (!Open) return;
            if (kb.upArrowKey.wasPressedThisFrame) row = 0;
            if (kb.downArrowKey.wasPressedThisFrame && OwnedWeapons().Length > 0) row = 1;
            int dx = kb.leftArrowKey.wasPressedThisFrame ? -1 : kb.rightArrowKey.wasPressedThisFrame ? 1 : 0;
            if (dx != 0)
            {
                if (row == 0 && RemakeRules.SoloKnight) { }   // solo el Caballero: nada que elegir
                else if (row == 0)
                {
                    var other = character == Character.Popolon ? Character.Aphrodite : Character.Popolon;
                    string why = game.Lives[(int)other] <= 0 ? $"{other} no tiene vidas"
                        : game.Inventory.WorldEntryBlocked(game.Inventory.CurrentWorld, other);
                    if (why == null) character = other;
                    else game.Say(why);
                }
                else
                {
                    var owned = OwnedWeapons();
                    int i = System.Array.IndexOf(owned, weapon);
                    weapon = owned[((i < 0 ? 0 : i + dx) % owned.Length + owned.Length) % owned.Length];
                }
            }
            if (kb.enterKey.wasPressedThisFrame && game.Inventory.Has(Inventory.AngelRing))
            {
                CloseKeeping();
                game.UseAngelRing();
                return;
            }
            for (int w = 1; w <= 9; w++)
                if (kb[(Key)((int)Key.Digit1 + w - 1)].wasPressedThisFrame && game.Inventory.Has(Inventory.Wing))
                {
                    if (game.UseWing(w)) CloseKeeping();
                    return;
                }
        }

        void OnGUI()
        {
            if (!Open) return;
            white ??= Texture2D.whiteTexture;
            float scale = Mathf.Min(Screen.width / 256f, Screen.height / 192f);
            float ox = (Screen.width - 256 * scale) / 2, oy = (Screen.height - 192 * scale) / 2;
            Rect R(float x, float y, float w, float h) => new Rect(ox + x * scale, oy + y * scale, w * scale, h * scale);
            small ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true, clipping = TextClipping.Clip };
            label ??= new GUIStyle(GUI.skin.label);
            small.fontSize = Mathf.Max(8, (int)(4.2f * scale));
            label.fontSize = Mathf.Max(10, (int)(6.5f * scale));
            bool blink = (Time.frameCount / 8) % 2 == 0;
            if (Visuals.Atmosphere.Styled)
            {
                {
                    // Centrado en la ventana: el recuadro del menú (y = 28…196) en el medio.
                    var (CR, cscale) = Visuals.UiKit.Centered(-4, 28, 264, 168);
                    DrawStyled(CR, cscale, blink);
                }
                return;
            }

            Box(R(0, 32, 256, 160), new Color(0, 0, 0, 0.92f));
            // Retratos (fila de personaje).
            // El retrato solo sale si ese personaje tiene vidas.
            if (game.Lives[1] > 0 || RemakeRules.SoloKnight) Box(R(RemakeRules.SoloKnight ? 120 : 96, 40, 16, 24), new Color(0.35f, 0.55f, 1f));
            GUI.Label(R(RemakeRules.SoloKnight ? 104 : 80, 64, 48, 8), $"Popolon x{game.Lives[1]}", small);
            if (!RemakeRules.SoloKnight)
            {
                if (game.Lives[0] > 0) Box(R(144, 40, 16, 24), new Color(1f, 0.45f, 0.7f));
                GUI.Label(R(128, 64, 48, 8), $"Aphrodite x{game.Lives[0]}", small);
            }
            if (!RemakeRules.SoloKnight && (row != 0 || blink))
                Frame(R(character == Character.Popolon ? 94 : 142, 38, 20, 28), Color.yellow);

            GUI.Label(R(112, 70, 40, 10), "ITEM", label);
            foreach (var item in game.Inventory.Data.items)
            {
                var m = item.menu;
                if (m.area != "items" && m.area != "world") continue;
                bool own = game.Inventory.Has(item.type) || item.type == 0x28 && game.Inventory.ShieldLevel > 0;
                if (!own) { Frame(R(m.x, m.y, 15, 15), new Color(1, 1, 1, 0.12f)); continue; }
                string name = item.type == 0x28 ? new[] { "", "Copper", "Silver", "Gold" }[game.Inventory.ShieldLevel] + " Shield" : item.name;
                int iconType = item.type == 0x28 ? new[] { 0x28, 0x28, 0x2B, 0x2C }[game.Inventory.ShieldLevel] : item.type;
                if (Visuals.HollowItems.Icon(iconType) is Sprite hk)
                {
                    // Icono de Hollow Knight (Visuals/HollowItems).
                    Frame(R(m.x, m.y, 15, 15), new Color(1, 1, 1, 0.25f));
                    Visuals.UiKit.Icon(R(m.x, m.y, 15, 15), hk);
                    continue;
                }
                Box(R(m.x, m.y, 15, 15), m.area == "world" ? new Color(0.25f, 0.35f, 0.15f) : new Color(0.2f, 0.2f, 0.35f));
                GUI.Label(R(m.x, m.y, 15, 15), Short(name), small);
            }
            GUI.Label(R(40, 128, 120, 10), "MAP", label);
            DrawMap(R);

            // Armas.
            Frame(R(104, 160, 136, 24), Color.white);
            var weapons = game.Inventory.Data.items.Where(i => i.menu.area == "weapons");
            foreach (var w in weapons)
            {
                if (!game.Inventory.Has(w.type)) continue;
                int x = WeaponX[w.type - 1];
                if (Visuals.HollowItems.Icon(w.type) is Sprite hk) { Visuals.UiKit.Icon(R(x - 2, 164, 16, 16), hk); continue; }
                GUI.Label(R(x - 4, 166, 24, 10), Short(w.name), small);
            }
            if (weapon > 0 && (row != 1 || blink))
                Frame(R(WeaponX[weapon - 1] - 4, 164, 24, 14), Color.yellow);

            string help = "↑↓ fila   ←→ cambiar   F1 cerrar" +
                          (game.Inventory.Has(Inventory.AngelRing) ? "   Return: Angel Ring" : "") +
                          (game.Inventory.Has(Inventory.Wing) && game.Inventory.CurrentWorld == 0 ? "   1-9: Ala" : "");
            GUI.Label(R(4, 33, 250, 8), help, small);
            var names = game.Inventory.Active.Select(t => game.Inventory.Data.Get(t)?.name).Where(n => n != null && !n.StartsWith("Quiver"));
            GUI.Label(R(4, 184, 250, 8), string.Join(" · ", names), small);
        }

        // Con el estilo del remake (Visuals/UiKit): el mismo menú con marco de filigrana, iconos y los personajes
        // dibujados. Las posiciones son las del original.
        void DrawStyled(System.Func<float, float, float, float, Rect> R, float scale, bool blink)
        {
            var props = Visuals.Atmosphere.Props;
            var chars = Visuals.Atmosphere.Characters;
            void T(Rect r, string text, float px, TextAnchor a, Color c, bool title = false) =>
                Visuals.UiKit.Text(r, text, px * scale / Visuals.UiKit.Scale, a, c, title);
            Visuals.UiKit.Shade(new Rect(0, 0, Screen.width, Screen.height), 0.55f);
            Visuals.UiKit.Panel(R(-4, 28, 264, 168));
            Visuals.UiKit.TitleLine(R(0, 33, 256, 8), "OBJETOS", 6.5f * scale / Visuals.UiKit.Scale);

            // Retratos: solo si ese personaje tiene vidas.
            int shield = game.Inventory.ShieldLevel;
            var portraits = RemakeRules.SoloKnight ? new[] { (Character.Popolon, 120f) } : new[] { (Character.Popolon, 96f), (Character.Aphrodite, 144f) };
            foreach (var (c, x) in portraits)
            {
                bool alive = game.Lives[(int)c] > 0 || RemakeRules.SoloKnight;
                if (chars != null) Visuals.UiKit.Icon(R(x - 4, 38, 24, 26), chars.Get(c, Visuals.CharacterArt.Anim.Idle, 0, shield), alive ? 1 : 0.25f, false, Visuals.CharacterArt.PortraitCrop);
                T(R(x - 16, 64, 48, 8), $"{c} ×{game.Lives[(int)c]}", 4.4f, TextAnchor.MiddleCenter, alive ? Visuals.UiKit.Bone : Visuals.UiKit.Dim);
            }
            if (!RemakeRules.SoloKnight && (row != 0 || blink))
                Visuals.UiKit.Frame(R(character == Character.Popolon ? 90 : 138, 38, 28, 34), Visuals.UiKit.Gold, Mathf.Max(1, scale * 0.6f));

            T(R(96, 70, 64, 10), "ITEM", 6, TextAnchor.MiddleCenter, Visuals.UiKit.Bone, true);
            foreach (var item in game.Inventory.Data.items)
            {
                var m = item.menu;
                if (m.area != "items" && m.area != "world") continue;
                bool own = game.Inventory.Has(item.type) || item.type == 0x28 && shield > 0;
                var cell = R(m.x, m.y, 15, 15);
                Visuals.UiKit.Frame(cell, new Color(1, 1, 1, own ? 0.25f : 0.08f), Mathf.Max(1, scale * 0.3f));
                if (!own) continue;
                int type = item.type == 0x28 ? new[] { 0x28, 0x28, 0x2B, 0x2C }[shield] : item.type;
                var icon = Visuals.HollowItems.Icon(type) ?? props?.Item(type);
                if (icon != null) Visuals.UiKit.Icon(cell, icon);
            }
            T(R(8, 128, 120, 10), "MAPA", 6, TextAnchor.MiddleLeft, Visuals.UiKit.Bone, true);
            DrawMap(R);

            // Armas.
            Visuals.UiKit.Frame(R(104, 160, 136, 24), new Color(1, 1, 1, 0.5f), Mathf.Max(1, scale * 0.4f));
            foreach (var w in game.Inventory.Data.items.Where(i => i.menu.area == "weapons"))
            {
                var wicon = Visuals.HollowItems.Icon(w.type) ?? props?.Item(w.type);
                if (!game.Inventory.Has(w.type) || wicon == null) continue;
                Visuals.UiKit.Icon(R(WeaponX[w.type - 1] - 2, 164, 16, 16), wicon);
            }
            if (weapon > 0 && (row != 1 || blink))
                Visuals.UiKit.Frame(R(WeaponX[weapon - 1] - 4, 163, 20, 18), Visuals.UiKit.Gold, Mathf.Max(1, scale * 0.6f));

            string help = "↑↓ fila   ←→ cambiar   F1 cerrar" +
                          (game.Inventory.Has(Inventory.AngelRing) ? "   Return: Angel Ring" : "") +
                          (game.Inventory.Has(Inventory.Wing) && game.Inventory.CurrentWorld == 0 ? "   1-9: Ala" : "");
            T(R(0, 186, 256, 8), help, 4.2f, TextAnchor.MiddleCenter, Visuals.UiKit.Dim);
        }

        // Mapa del mundo (con el Map de ese mundo): una celda de 8 × 4 px por sala; la sala actual en rojo y, con la
        // Lamp, la del jefe en azul. En el castillo no hay mapa.
        void DrawMap(System.Func<float, float, float, float, Rect> R)
        {
            var inv = game.Inventory;
            if (inv.CurrentWorld == 0 || !inv.Has(Inventory.Map)) return;
            var map = inv.MapOf(inv.CurrentWorld);
            if (map == null) return;
            foreach (var c in map.roomCells)
            {
                var color = c.room == game.Motor.Room.id ? Color.red : new Color(0.6f, 0.6f, 0.6f);
                if (c.room != game.Motor.Room.id && inv.Has(Inventory.Lamp) && c.room == map.bossRoom) color = new Color(0.3f, 0.5f, 1f);
                Box(R(8 * c.col, 144 + 4 * c.row, 7, 3), color);
            }
        }

        static string Short(string name)
        {
            var words = name.Split(' ', '&').Where(w => w.Length > 0).ToArray();
            return words.Length > 1 ? string.Concat(words.Select(w => w.Substring(0, System.Math.Min(3, w.Length)))) : name.Substring(0, System.Math.Min(6, name.Length));
        }

        void Box(Rect r, Color c)
        {
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, white);
            GUI.color = old;
        }

        void Frame(Rect r, Color c)
        {
            Box(new Rect(r.x, r.y, r.width, 1), c);
            Box(new Rect(r.x, r.yMax - 1, r.width, 1), c);
            Box(new Rect(r.x, r.y, 1, r.height), c);
            Box(new Rect(r.xMax - 1, r.y, 1, r.height), c);
        }
    }
}
