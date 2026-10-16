using MazeOfKnight.Data;
using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Game.Visuals;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game
{
    // El menú de objetos de Galious: The Hollow Kingdom, como el de Hollow Knight (RemakeRules.HollowCharms; arte en
    // Assets/Art/Resources/Hollow/inventario, fuera de git, de tools/hollow/inventario.py). Tres pestañas que se cambian
    // como con LT/RT (Q/E, o los gatillos y los botones de hombro del mando):
    //   - Inventario: las armas a la izquierda (Aceptar elige la de M), los contadores debajo, los objetos en medio y,
    //     a la derecha, el nombre, qué hace y cómo se usa el elegido. Aceptar usa el Anillo del ángel y el Ala.
    //   - Amuletos: los equipados y las muescas arriba, todos los amuletos debajo y el elegido a la derecha con su coste.
    //     Solo se equipan o se quitan sentado en un banco.
    //   - Mapa: el del mundo, si se tiene.
    // Se abre y se cierra con I, Tab o F1 (Select en el mando; I y Select se cambian en Opciones); Esc o B también cierran.
    public partial class InventoryMenu
    {
        enum Tab { Inventory, Charms, Map, Journal }
        static readonly string[] TabNames = { "Inventario", "Amuletos", "Mapa", "Diario" };
        static int Tabs => RemakeRules.HollowJournal && HuntersJournal.Available ? 4 : 3;
        static readonly int[] CharmOrder = { 0x0E, 0x0F, 0x17, 0x22, 0x24, 0x11, 0x19, 0x1A, 0x1B, 0x1D, 0x1E, 0x21, 0x25, 0x28 };

        Tab tab;
        int sel;                       // índice en las celdas de la pestaña
        int wingWorld;                 // > 0: eligiendo el mundo del Ala
        int closedFrame = -10;
        int journalSel, journalTop;
        const int JournalRows = 11;
        float openedAt;
        string notice;
        float noticeUntil;
        readonly List<(Rect r, int type)> cells = new List<(Rect, int)>();

        static Sprite slot, notchOn, notchOff, cost, tabArrow, glow;
        static bool artLoaded;

        static bool UseHollow => RemakeRules.HollowCharms && HollowMenus.Available;
        public bool JustClosed => Time.frameCount - closedFrame <= 1;

        static void LoadArt()
        {
            if (artLoaded) return;
            artLoaded = true;
            Sprite L(string n) => Resources.Load<Sprite>("Hollow/inventario/" + n);
            slot = L("hueco"); notchOn = L("muesca_llena"); notchOff = L("muesca_vacia"); cost = L("coste");
            tabArrow = L("flecha_pestana"); glow = L("luz");
        }

        // Para las capturas.
        public void ShowTab(int which)
        {
            if (!Open) HollowOpen();
            tab = (Tab)which;
            sel = 0;
        }

        void HollowOpen()
        {
            if (!game.CanOpenMenu) return;
            if (game.GetComponent<HollowPauseMenu>()?.Open == true || game.GetComponent<ItemTutorial>()?.Open == true) return;
            Open = true;
            sel = 0;
            wingWorld = 0;
            openedAt = Time.unscaledTime;
            game.SetPaused(true);
        }

        void HollowClose()
        {
            Open = false;
            closedFrame = Time.frameCount;
            game.SetPaused(false);
        }

        void Say(string text) { notice = text; noticeUntil = Time.unscaledTime + 2.5f; }

        static bool Pressed(Key key, System.Func<Gamepad, UnityEngine.InputSystem.Controls.ButtonControl> pad)
        {
            var kb = Keyboard.current;
            if (kb != null && kb[key].wasPressedThisFrame) return true;
            var gp = Gamepad.current;
            return gp != null && pad != null && pad(gp).wasPressedThisFrame;
        }

        void HollowUpdate()
        {
            bool toggle = !game.TypingWord && (Controls.Pressed("inventario") || Pressed(Key.Tab, null)) || Pressed(Key.F1, null);
            if (!Open)
            {
                if (toggle) HollowOpen();
                return;
            }
            if (toggle || HollowMenus.Back && wingWorld == 0) { HollowClose(); return; }
            int dt = Pressed(Key.Q, g => g.leftShoulder) || Pressed(Key.Q, g => g.leftTrigger) ? -1
                   : Pressed(Key.E, g => g.rightShoulder) || Pressed(Key.E, g => g.rightTrigger) ? 1 : 0;
            if (dt != 0 && wingWorld == 0)
            {
                tab = (Tab)(((int)tab + dt + Tabs) % Tabs);
                sel = 0;
                return;
            }
            if (wingWorld > 0) { TickWing(); return; }
            int dx = HollowMenus.Left ? -1 : HollowMenus.Right ? 1 : 0, dy = HollowMenus.Up ? -1 : HollowMenus.Down ? 1 : 0;
            if (tab == Tab.Journal)
            {
                int n = HuntersJournal.Entries.Count;
                if (n > 0 && dy != 0) journalSel = (journalSel + dy + n) % n;
                if (n > 0 && dx != 0) journalSel = Mathf.Clamp(journalSel + dx * JournalRows, 0, n - 1);
                return;
            }
            if ((dx != 0 || dy != 0) && cells.Count > 0) sel = Step(sel, dx, dy);
            // Ala desde el teclado, como en el original.
            var kb = Keyboard.current;
            if (kb != null && game.Inventory.Has(Inventory.Wing))
                for (int w = 1; w <= 9; w++)
                    if (kb[(Key)((int)Key.Digit1 + w - 1)].wasPressedThisFrame)
                    {
                        if (game.UseWing(w)) HollowClose();
                        return;
                    }
            if (HollowMenus.Accept && Time.unscaledTime - openedAt > 0.15f && sel < cells.Count) Use(cells[sel].type);
        }

        // La celda más cercana en esa dirección.
        int Step(int from, int dx, int dy)
        {
            if (from >= cells.Count) return 0;
            var c = cells[from].r.center;
            int best = from;
            float bestD = float.MaxValue;
            for (int i = 0; i < cells.Count; i++)
            {
                if (i == from) continue;
                var d = cells[i].r.center - c;
                float along = d.x * dx + d.y * dy;
                if (along <= 1) continue;
                float side = Mathf.Abs(dx != 0 ? d.y : d.x);
                float score = along + side * 2.5f;
                if (score < bestD) { bestD = score; best = i; }
            }
            return best;
        }

        void Use(int type)
        {
            var inv = game.Inventory;
            if (tab == Tab.Inventory)
            {
                if (type >= 1 && type <= 6) { game.ApplyMenu(game.Motor.Character, type); Say($"{ItemTexts.Get(type).Name}: en M"); }
                else if (type == Inventory.AngelRing)
                {
                    if (game.Bosses != null && game.Bosses.InFight) { Say("No vale durante la pelea con un jefe"); return; }
                    HollowClose();
                    game.UseAngelRing();
                }
                else if (type == Inventory.Wing)
                {
                    if (inv.CurrentWorld != 0) Say("El ala solo funciona en el castillo");
                    else if (inv.OpenedDoors.Count == 0) Say("Aún no se ha abierto ninguna puerta");
                    else wingWorld = inv.OpenedDoors.Where(w => w <= 9).DefaultIfEmpty(1).Min();
                }
                return;
            }
            if (tab != Tab.Charms || !inv.OwnsCharm(type)) return;
            if (!game.Seated) { Say("Siéntate en un banco para cambiar los amuletos"); return; }
            if (inv.Equipped(type)) { inv.Unequip(type); Sound.Effect(0x2D); }
            else if (inv.Equip(type)) Sound.Effect(0x2E);
            else Say("No quedan muescas libres");
        }

        void TickWing()
        {
            var opened = game.Inventory.OpenedDoors.Where(w => w >= 1 && w <= 9).OrderBy(w => w).ToList();
            if (opened.Count == 0 || HollowMenus.Back) { wingWorld = 0; return; }
            int i = Mathf.Max(0, opened.IndexOf(wingWorld));
            if (HollowMenus.Left || HollowMenus.Up) wingWorld = opened[(i + opened.Count - 1) % opened.Count];
            if (HollowMenus.Right || HollowMenus.Down) wingWorld = opened[(i + 1) % opened.Count];
            if (HollowMenus.Accept)
            {
                int w = wingWorld;
                wingWorld = 0;
                if (game.UseWing(w)) HollowClose();
            }
        }

        // ---------- dibujo ----------

        float S => UiKit.Scale;

        void HollowGUI()
        {
            LoadArt();
            GUI.depth = -8;
            float s = S, W = Screen.width, H = Screen.height;
            UiKit.Box(new Rect(0, 0, W, H), new Color(0.01f, 0.01f, 0.03f, 0.9f));
            var area = new Rect(Mathf.Max(20 * s, (W - 1240 * s) / 2), H * 0.07f, Mathf.Min(W - 40 * s, 1240 * s), H * 0.86f);
            DrawHeader(area);
            var body = new Rect(area.x + 40 * s, area.y + 90 * s, area.width - 80 * s, area.height - 150 * s);
            cells.Clear();
            switch (tab)
            {
                case Tab.Inventory: DrawInventory(body); break;
                case Tab.Charms: DrawCharms(body); break;
                case Tab.Map: DrawMapTab(body); break;
                case Tab.Journal: DrawJournal(body); break;
            }
            if (sel >= cells.Count) sel = Mathf.Max(0, cells.Count - 1);
            if (cells.Count > 0) DrawCursor(cells[sel].r);
            // Pie: aviso o ayuda.
            string foot = notice != null && Time.unscaledTime < noticeUntil ? notice
                : wingWorld > 0 ? "← → mundo   Aceptar: ir   Atrás: cancelar"
                : "Q / E  pestaña     Flechas  elegir     Aceptar  usar     I  cerrar";
            UiKit.Text(new Rect(area.x, area.yMax - 40 * s, area.width, 30 * s), foot, 15, TextAnchor.MiddleCenter,
                notice != null && Time.unscaledTime < noticeUntil ? UiKit.Gold : UiKit.Dim);
        }

        void DrawHeader(Rect area)
        {
            float s = S;
            int t = (int)tab;
            UiKit.Text(new Rect(area.x, area.y, area.width, 50 * s), TabNames[t], 34, TextAnchor.MiddleCenter, UiKit.Bone, true);
            HollowMenus.Divider(area.center.x, area.y + 54 * s, 260 * s);
            // Las flechas de los lados con la pestaña de al lado.
            float ah = 44 * s, aw = tabArrow != null ? ah * tabArrow.rect.width / tabArrow.rect.height : ah * 0.7f;
            float ay = area.y + area.height / 2 - ah / 2;
            UiKit.Icon(new Rect(area.x - aw - 4 * s, ay, aw, ah), tabArrow, 0.85f, true);
            UiKit.Icon(new Rect(area.xMax + 4 * s, ay, aw, ah), tabArrow, 0.85f);
            UiKit.Text(new Rect(area.x, area.y + 8 * s, 300 * s, 30 * s), "◂ " + TabNames[(t + Tabs - 1) % Tabs], 17, TextAnchor.MiddleLeft, UiKit.Dim, true);
            UiKit.Text(new Rect(area.xMax - 300 * s, area.y + 8 * s, 300 * s, 30 * s), TabNames[(t + 1) % Tabs] + " ▸", 17, TextAnchor.MiddleRight, UiKit.Dim, true);
        }

        // El cursor de Hollow Knight: cuatro esquinas que laten alrededor de la celda.
        void DrawCursor(Rect r)
        {
            float s = S, pad = (8 + 3 * Mathf.Sin(Time.unscaledTime * 4)) * s, len = 16 * s, w = Mathf.Max(2, 2.5f * s);
            var c = new Color(UiKit.Bone.r, UiKit.Bone.g, UiKit.Bone.b, 0.9f);
            var cr = new Rect(r.x - pad, r.y - pad, r.width + pad * 2, r.height + pad * 2);
            foreach (var (x, y, sx, sy) in new[] { (cr.x, cr.y, 1, 1), (cr.xMax, cr.y, -1, 1), (cr.x, cr.yMax, 1, -1), (cr.xMax, cr.yMax, -1, -1) })
            {
                UiKit.Box(new Rect(sx > 0 ? x : x - len, sy > 0 ? y : y - w, len, w), c);
                UiKit.Box(new Rect(sx > 0 ? x : x - w, sy > 0 ? y : y - len, w, len), c);
            }
        }

        static Sprite IconOf(int type) => HollowItems.Icon(type) ?? Atmosphere.Props?.Item(type);

        // El escudo como amuleto: el del mejor nivel que se tiene.
        int ShieldType => new[] { 0x28, 0x28, 0x2B, 0x2C }[Mathf.Clamp(game.Inventory.OwnedShieldLevel, 0, 3)];
        int Shown(int type) => type == 0x28 ? ShieldType : type;

        // El panel de la derecha: nombre, (coste), icono grande, descripción y uso.
        void DrawDetails(Rect r, int type, bool charm)
        {
            float s = S;
            if (type == 0) return;
            int shown = Shown(type);
            var e = ItemTexts.Get(shown);
            float y = r.y;
            UiKit.Text(new Rect(r.x, y, r.width, 44 * s), e.Name, 28, TextAnchor.MiddleCenter, UiKit.Bone, true);
            y += 46 * s;
            HollowMenus.Divider(r.center.x, y, 220 * s);
            y += 46 * s;
            if (charm)
            {
                int c = Inventory.CostOf(type);
                UiKit.Text(new Rect(r.x, y, r.width / 2 - 4 * s, 28 * s), "Coste", 17, TextAnchor.MiddleRight, UiKit.Dim, true);
                for (int i = 0; i < c; i++) UiKit.Icon(new Rect(r.center.x + 4 * s + i * 26 * s, y + 2 * s, 24 * s, 24 * s), cost);
                y += 36 * s;
                if (glow != null) UiKit.Icon(new Rect(r.center.x - 90 * s, y - 10 * s, 180 * s, 160 * s), glow, 0.35f);
                UiKit.Icon(new Rect(r.center.x - 60 * s, y, 120 * s, 120 * s), IconOf(shown));
                y += 132 * s;
            }
            UiKit.Text(new Rect(r.x, y, r.width, 220 * s), ItemTexts.Describe(shown), 17, TextAnchor.UpperCenter, UiKit.Bone);
            if (e.Use != null)
                UiKit.Text(new Rect(r.x, r.yMax - 70 * s, r.width, 60 * s), e.Use, 16, TextAnchor.LowerCenter, UiKit.Gold);
        }

        // Una celda: el hueco del amuleto, más pequeño que el objeto, y el objeto encima, centrado.
        void Cell(Rect r, int type, float alpha = 1, bool emptySlot = false)
        {
            if (emptySlot && slot != null) UiKit.Icon(Inset(r, 0.72f), slot, 0.7f);
            if (type != 0) UiKit.Icon(Inset(r, 0.96f), IconOf(Shown(type)), alpha);
        }

        static Rect Inset(Rect r, float k) => new Rect(r.center.x - r.width * k / 2, r.center.y - r.height * k / 2, r.width * k, r.height * k);

        void DrawInventory(Rect body)
        {
            float s = S;
            var inv = game.Inventory;
            float leftW = body.width * 0.26f, midW = body.width * 0.38f;
            var left = new Rect(body.x, body.y, leftW, body.height);
            var mid = new Rect(left.xMax + 20 * s, body.y, midW, body.height);
            var right = new Rect(mid.xMax + 40 * s, body.y, body.xMax - mid.xMax - 40 * s, body.height);

            // Armas: 2 × 3, las que no se tienen como hueco vacío.
            UiKit.Text(new Rect(left.x, left.y, left.width, 30 * s), "Armas", 20, TextAnchor.MiddleLeft, UiKit.Dim, true);
            float cs = Mathf.Min(78 * s, left.width / 2.4f);
            for (int w = 1; w <= 6; w++)
            {
                var r = new Rect(left.x + ((w - 1) % 2) * cs * 1.2f, left.y + 40 * s + ((w - 1) / 2) * cs * 1.15f, cs, cs);
                if (!inv.Has(w)) { if (slot != null) UiKit.Icon(r, slot, 0.25f); continue; }
                Cell(r, w);
                if (game.SelectedWeapon == w)
                    UiKit.Text(new Rect(r.x, r.yMax - 6 * s, r.width, 18 * s), "M", 14, TextAnchor.MiddleCenter, UiKit.Gold, true);
                cells.Add((r, w));
            }
            // Contadores.
            float cy = left.y + 40 * s + 3 * cs * 1.15f + 20 * s;
            var counts = new (int type, int n)[] { (0x2D, game.Arrows), (0x2E, game.Coins), (0x2F, game.Keys) };
            foreach (var (t, n) in counts)
            {
                UiKit.Icon(new Rect(left.x, cy, 36 * s, 36 * s), IconOf(t));
                UiKit.Text(new Rect(left.x + 46 * s, cy, left.width - 46 * s, 36 * s), $"{n}", 22, TextAnchor.MiddleLeft, UiKit.Bone, true);
                cy += 44 * s;
            }
            if (inv.Has(Inventory.Bible))
                UiKit.Text(new Rect(left.x, cy, left.width, 30 * s), $"Biblia: {16 - inv.BibleUses} usos", 16, TextAnchor.MiddleLeft, UiKit.Dim);

            // Objetos: los que se tienen, los de mundo primero.
            UiKit.Text(new Rect(mid.x, mid.y, mid.width, 30 * s), "Objetos", 20, TextAnchor.MiddleLeft, UiKit.Dim, true);
            var items = Enumerable.Range(0x07, 0x2A - 0x07 + 1)
                .Where(t => !Inventory.IsCharm(t) && inv.Has(t) && t != 0x2B && t != 0x2C).ToList();
            if (!RemakeRules.HollowCharms) items.RemoveAll(t => t == 0x28);
            int cols = 5;
            float ic = Mathf.Min(74 * s, mid.width / (cols + 0.6f));
            for (int i = 0; i < items.Count; i++)
            {
                var r = new Rect(mid.x + (i % cols) * ic * 1.12f, mid.y + 40 * s + (i / cols) * ic * 1.12f, ic, ic);
                Cell(r, items[i]);
                cells.Add((r, items[i]));
            }
            if (items.Count == 0)
                UiKit.Text(new Rect(mid.x, mid.y + 40 * s, mid.width, 30 * s), "Nada todavía", 17, TextAnchor.MiddleLeft, UiKit.Dim);

            int selType = sel < cells.Count ? cells[sel].type : 0;
            if (wingWorld > 0)
            {
                UiKit.Text(new Rect(right.x, right.y, right.width, 44 * s), "Ala", 28, TextAnchor.MiddleCenter, UiKit.Bone, true);
                UiKit.Text(new Rect(right.x, right.y + 80 * s, right.width, 60 * s), $"◂  Mundo {wingWorld}  ▸", 30, TextAnchor.MiddleCenter, UiKit.Gold, true);
                UiKit.Text(new Rect(right.x, right.y + 150 * s, right.width, 60 * s), "A la puerta de ese mundo, en el castillo.", 17, TextAnchor.UpperCenter, UiKit.Dim);
            }
            else DrawDetails(right, selType, false);
        }

        void DrawCharms(Rect body)
        {
            float s = S;
            var inv = game.Inventory;
            var left = new Rect(body.x, body.y, body.width * 0.6f, body.height);
            var right = new Rect(left.xMax + 50 * s, body.y, body.xMax - left.xMax - 50 * s, body.height);

            // Equipados y muescas.
            UiKit.Text(new Rect(left.x, left.y, left.width, 30 * s), "Equipados", 20, TextAnchor.MiddleLeft, UiKit.Dim, true);
            float ec = 64 * s, x = left.x;
            var equipped = CharmOrder.Where(inv.Equipped).ToList();
            foreach (int c in equipped) { Cell(new Rect(x, left.y + 36 * s, ec, ec), c); x += ec * 1.1f; }
            if (equipped.Count == 0)
                UiKit.Text(new Rect(left.x, left.y + 36 * s, left.width, ec), "Ninguno", 17, TextAnchor.MiddleLeft, UiKit.Dim);
            float ny = left.y + 36 * s + ec + 14 * s;
            UiKit.Text(new Rect(left.x, ny, 140 * s, 30 * s), "Muescas", 20, TextAnchor.MiddleLeft, UiKit.Dim, true);
            int used = inv.NotchesUsed;
            for (int i = 0; i < inv.Notches; i++)
            {
                var r = new Rect(left.x + 130 * s + i * 32 * s, ny + 1 * s, 28 * s, 28 * s);
                if (i < used) UiKit.Icon(new Rect(r.x - 4 * s, r.y - 4 * s, r.width + 8 * s, r.height + 8 * s), notchOn);
                else UiKit.Icon(r, notchOff);
            }
            float dy = ny + 46 * s;
            for (int i = 0; i < 40; i++)   // línea fina que se apaga hacia los extremos
                UiKit.Box(new Rect(left.x + left.width * 0.8f * i / 40f, dy, left.width * 0.8f / 40f + 1, Mathf.Max(1, s * 1.5f)),
                    new Color(1, 1, 1, 0.55f * Mathf.Sin(Mathf.PI * (i + 0.5f) / 40f)));

            // Todos los amuletos: los equipados, apagados; los que no se tienen, hueco vacío.
            int cols = 7;
            float gc = Mathf.Min(84 * s, left.width / (cols + 0.8f));
            for (int i = 0; i < CharmOrder.Length; i++)
            {
                int c = CharmOrder[i];
                var r = new Rect(left.x + (i % cols) * gc * 1.12f, dy + 26 * s + (i / cols) * gc * 1.12f, gc, gc);
                bool own = inv.OwnsCharm(c);
                Cell(r, own ? c : 0, inv.Equipped(c) ? 0.35f : 1, true);
                if (own) cells.Add((r, c));
            }
            if (cells.Count == 0)
                UiKit.Text(new Rect(left.x, dy + 26 * s + 2 * gc * 1.12f + 10 * s, left.width, 30 * s), "Aún no tienes amuletos", 17, TextAnchor.MiddleLeft, UiKit.Dim);
            UiKit.Text(new Rect(left.x, left.yMax - 36 * s, left.width, 30 * s),
                game.Seated ? "Aceptar: equipar o quitar" : "Siéntate en un banco para cambiar los amuletos", 16, TextAnchor.MiddleLeft,
                game.Seated ? UiKit.Gold : UiKit.Dim);
            if (sel < cells.Count) DrawDetails(right, cells[sel].type, true);
        }

        // Diario del cazador: la lista de criaturas a la izquierda (las que no se conocen, sin nombre) y la elegida a la
        // derecha con su retrato, sus derrotas, su descripción y, a partir de las necesarias, las notas del cazador.
        void DrawJournal(Rect body)
        {
            float s = S;
            var all = HuntersJournal.Entries;
            if (all.Count == 0) return;
            var jr = game.Journal;
            int known = 0;
            foreach (var e in all) if (jr.Known(e.types)) known++;
            var left = new Rect(body.x, body.y, body.width * 0.3f, body.height);
            var right = new Rect(left.xMax + 50 * s, body.y, body.xMax - left.xMax - 50 * s, body.height);
            UiKit.Text(new Rect(left.x, left.y - 6 * s, left.width, 30 * s), $"Encontradas  {known} / {all.Count}", 18, TextAnchor.MiddleCenter, UiKit.Dim, true);
            UiKit.Icon(new Rect(left.x + left.width * 0.15f, left.y + 24 * s, left.width * 0.7f, 18 * s), HuntersJournal.Symbol, 0.7f);
            journalSel = Mathf.Clamp(journalSel, 0, all.Count - 1);
            if (journalSel < journalTop) journalTop = journalSel;
            if (journalSel >= journalTop + JournalRows) journalTop = journalSel - JournalRows + 1;
            float rowH = (left.height - 100 * s) / JournalRows, y = left.y + 52 * s;
            for (int i = journalTop; i < Mathf.Min(all.Count, journalTop + JournalRows); i++, y += rowH)
            {
                var e = all[i];
                bool k = jr.Known(e.types), on = i == journalSel;
                var r = new Rect(left.x, y, left.width, rowH);
                if (on) UiKit.Box(new Rect(r.x - 8 * s, r.y + 2 * s, r.width + 16 * s, r.height - 4 * s), new Color(1, 1, 1, 0.07f));
                if (k) UiKit.Icon(new Rect(r.x, r.y + 4 * s, rowH - 8 * s, rowH - 8 * s), HuntersJournal.Portrait(e), on ? 1 : 0.7f);
                UiKit.Text(new Rect(r.x + rowH + 4 * s, r.y, r.width - rowH, rowH), k ? e.name : "· · ·", 18, TextAnchor.MiddleLeft,
                    on ? UiKit.Bone : UiKit.Dim, true);
                if (on) DrawCursor(new Rect(r.x + 2 * s, r.y + 6 * s, rowH - 12 * s, rowH - 12 * s));
            }
            if (journalTop > 0) UiKit.Text(new Rect(left.x, left.y + 38 * s, left.width, 20 * s), "▲", 12, TextAnchor.MiddleCenter, UiKit.Dim);
            if (journalTop + JournalRows < all.Count) UiKit.Text(new Rect(left.x, y, left.width, 20 * s), "▼", 12, TextAnchor.MiddleCenter, UiKit.Dim);

            var cur = all[journalSel];
            if (!jr.Known(cur.types))
            {
                UiKit.Text(new Rect(right.x, right.center.y - 40 * s, right.width, 80 * s), "Aún no has encontrado esta criatura.", 20, TextAnchor.MiddleCenter, UiKit.Dim);
                return;
            }
            float py = right.y;
            var portrait = HuntersJournal.Portrait(cur);
            float ph = 230 * s;
            UiKit.Icon(new Rect(right.center.x - ph * 0.6f, py, ph * 1.2f, ph), portrait);
            py += ph + 10 * s;
            UiKit.Text(new Rect(right.x, py, right.width, 40 * s), cur.name, 28, TextAnchor.MiddleCenter, UiKit.Bone, true);
            py += 42 * s;
            int kills = jr.Kills(cur.types);
            bool canKill = false;
            foreach (int t in cur.types) if (!(game.Enemies?.IsInvulnerable(t) ?? false)) canKill = true;
            UiKit.Text(new Rect(right.x, py, right.width, 26 * s), canKill ? $"Derrotados: {kills}" : "No se puede derrotar", 17, TextAnchor.MiddleCenter, UiKit.Dim, true);
            py += 30 * s;
            HollowMenus.Divider(right.center.x, py, 220 * s, 0.8f);
            py += 34 * s;
            UiKit.Text(new Rect(right.x, py, right.width, 70 * s), cur.desc, 17, TextAnchor.UpperCenter, UiKit.Bone);
            py += 66 * s;
            int need = HuntersJournal.NotesAt(cur) - kills;
            string notes = !canKill || need <= 0 ? cur.notes : $"Derrota {need} más para leer las notas del cazador.";
            UiKit.Text(new Rect(right.x, py, right.width, right.yMax - py), notes, 16, TextAnchor.UpperCenter,
                !canKill || need <= 0 ? new Color(0.78f, 0.76f, 0.7f) : UiKit.Dim, false);
        }

        // Para las capturas: la entrada de ese tipo.
        public void ShowJournal(int type)
        {
            ShowTab((int)Tab.Journal);
            var all = HuntersJournal.Entries;
            for (int i = 0; i < all.Count; i++) if (System.Array.IndexOf(all[i].types, type) >= 0) journalSel = i;
        }

        static Sprite benchPin, here;

        void DrawMapTab(Rect body)
        {
            float s = S;
            var inv = game.Inventory;
            int world = inv.CurrentWorld;
            string map = game.mapName;
            string zone = HandMap.ZoneName(HollowScenery.LookFor(map).Zone);
            UiKit.Text(new Rect(body.x, body.y - 6 * s, body.width, 36 * s), world == 0 ? "El castillo" : $"Mundo {world}", 26, TextAnchor.MiddleCenter, UiKit.Bone, true);
            if (zone != "") UiKit.Text(new Rect(body.x, body.y + 26 * s, body.width, 26 * s), zone, 17, TextAnchor.MiddleCenter, UiKit.Dim, true);
            var rw = game.WorldFor(map);
            if (!RemakeRules.HollowMaps || rw == null) return;
            bool inked = game.HasMapOf(world);
            var tex = HandMap.Get(map, rw, game.VisitedRooms, inked);
            var area = new Rect(body.x, body.y + 64 * s, body.width, body.height - 110 * s);
            // Lo más grande que quepa, sin que una sala pase de un tercio del ancho (con pocas salas visitadas).
            float k = Mathf.Min(Mathf.Min(area.width / tex.width, area.height / tex.height), area.width / 3f / HandMap.RoomW);
            var r = new Rect(area.center.x - tex.width * k / 2, area.center.y - tex.height * k / 2, tex.width * k, tex.height * k);
            GUI.DrawTexture(r, tex);
            Rect At(RoomModel room, int x, int y, float size)
            {
                var p = HandMap.Point(room, x, y) * k;
                return new Rect(r.x + p.x - size / 2, r.y + p.y - size, size, size);
            }
            // Bancos (en las salas que se ven) y la sala del jefe con la Lámpara.
            benchPin ??= Resources.Load<Sprite>("Hollow/inventario/pin_banco");
            here ??= Resources.Load<Sprite>("Hollow/hud/mascara_00");
            foreach (var b in game.BenchesIn(map))
                if ((inked || game.VisitedRooms.Contains((map, b.Room))) && rw.Room(b.Room) is RoomModel br)
                    UiKit.Icon(At(br, b.X, b.Y, 26 * s), benchPin, 0.9f);
            var wm = world > 0 ? inv.MapOf(world) : null;
            if (inked && wm != null && inv.Has(Inventory.Lamp) && rw.Room(wm.bossRoom) is RoomModel boss)
            {
                var bp = At(boss, 128, 120, 18 * s);
                UiKit.Box(bp, new Color(0.9f, 0.3f, 0.25f, 0.85f));
            }
            // Dónde estás: la máscara del Caballero, que late.
            var m = game.Motor;
            if (m?.Room != null)
            {
                float size = (34 + 4 * Mathf.Sin(Time.unscaledTime * 4)) * s;
                if (here != null) UiKit.Icon(At(m.Room, m.X, m.Y, size), here);
                else UiKit.Box(At(m.Room, m.X, m.Y, 10 * s), UiKit.Gold);
            }
            string foot = world == 0 ? "Nadie ha cartografiado el castillo: es tu boceto."
                : inked ? (inv.Has(Inventory.Lamp) ? "El mapa de Cornifer. La marca roja es el jefe." : "El mapa de Cornifer.")
                : "Un boceto de lo que has recorrido. Cornifer vende el mapa de este mundo.";
            UiKit.Text(new Rect(body.x, body.yMax - 34 * s, body.width, 30 * s), foot, 16, TextAnchor.MiddleCenter, UiKit.Dim);
        }
    }
}
