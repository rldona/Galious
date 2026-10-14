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
            weapon = game.SelectedWeapon != 0 ? game.SelectedWeapon : OwnedWeapons().FirstOrDefault();
            game.SetPaused(true);
        }

        void CloseMenu()
        {
            Open = false;
            game.ApplyMenu(character, weapon);
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
                if (row == 0)
                {
                    var other = character == Character.Popolon ? Character.Aphrodite : Character.Popolon;
                    string why = game.Inventory.WorldEntryBlocked(game.Inventory.CurrentWorld, other);
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
                Open = false;
                game.SetPaused(false);
                game.UseAngelRing();
                return;
            }
            for (int w = 1; w <= 9; w++)
                if (kb[(Key)((int)Key.Digit1 + w - 1)].wasPressedThisFrame && game.Inventory.Has(Inventory.Wing))
                {
                    if (game.UseWing(w)) { Open = false; game.SetPaused(false); }
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

            Box(R(0, 32, 256, 160), new Color(0, 0, 0, 0.92f));
            // Retratos (fila de personaje).
            Box(R(96, 40, 16, 24), new Color(0.35f, 0.55f, 1f));
            Box(R(144, 40, 16, 24), new Color(1f, 0.45f, 0.7f));
            GUI.Label(R(80, 64, 48, 8), "Popolon", small);
            GUI.Label(R(128, 64, 48, 8), "Aphrodite", small);
            if (row != 0 || blink)
                Frame(R(character == Character.Popolon ? 94 : 142, 38, 20, 28), Color.yellow);

            GUI.Label(R(112, 70, 40, 10), "ITEM", label);
            foreach (var item in game.Inventory.Data.items)
            {
                var m = item.menu;
                if (m.area != "items" && m.area != "world") continue;
                bool own = game.Inventory.Has(item.type) || item.type == 0x28 && game.Inventory.ShieldLevel > 0;
                if (!own) { Frame(R(m.x, m.y, 15, 15), new Color(1, 1, 1, 0.12f)); continue; }
                string name = item.type == 0x28 ? new[] { "", "Copper", "Silver", "Gold" }[game.Inventory.ShieldLevel] + " Shield" : item.name;
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
