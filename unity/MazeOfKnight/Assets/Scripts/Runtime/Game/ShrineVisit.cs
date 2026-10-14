using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;

namespace MazeOfKnight.Game
{
    // Una visita a un santuario (docs/design_bible/08_santuarios.md). Tienda: ← → mueven la mano (salta los huecos
    // vacíos y lo ya comprado en esta visita), espacio compra; sin monedas suficientes sale "COINS ARE NOT ENOUGH.";
    // los escudos van en orden (plata pide cobre, oro pide plata). El carcaj da 100 flechas y la llave, 1 (tope 999).
    // El juego no mira si ya tienes el objeto. Contraseña y Muerte preguntan YES/NO. ↓ sale.
    public class ShrineVisit
    {
        public readonly Shrine Shrine;
        readonly ShrinesData data;
        readonly Inventory inventory;
        public int Arrows, Coins, Keys;
        public string Text { get; private set; }
        public int Cursor { get; private set; } = -1;
        public readonly HashSet<int> Sold = new HashSet<int>();
        public bool AskYesNo { get; private set; }
        public bool Yes = true;
        public bool Refused { get; private set; }   // último intento rechazado (sonido 0x2F)

        public ShrineVisit(Shrine shrine, ShrinesData data, Inventory inventory, int arrows, int coins, int keys)
        {
            Shrine = shrine;
            this.data = data;
            this.inventory = inventory;
            Arrows = arrows; Coins = coins; Keys = keys;
            switch (shrine.kind)
            {
                case "shop":
                    Text = Format(shrine.messages.FirstOrDefault() ?? data.Message(0x05));
                    Cursor = FreeSlots().DefaultIfEmpty(-1).First();
                    break;
                case "password":
                    Text = Format(shrine.messages[0]);
                    AskYesNo = true;
                    break;
                case "resurrection":
                    // [I] Sin vidas de repuesto en el remake (backlog) el otro personaje nunca está muerto: solo el aviso.
                    Text = Format(data.Message(0x00));
                    break;
                default:
                    Text = Format(shrine.messages.FirstOrDefault() ?? "");
                    break;
            }
        }

        public static string Format(string text) =>
            (text ?? "").Replace("{arrows}", "ARROWS ").Replace("{coins}", "COINS ").Replace("{keys}", "KEYS ");

        public ShrineItem ItemAt(int slot) => Shrine.items.FirstOrDefault(i => i.slot == slot && i.type != 0);

        IEnumerable<int> FreeSlots() => Enumerable.Range(0, 3).Where(s => ItemAt(s) != null && !Sold.Contains(s));

        public void Move(int dir)
        {
            if (AskYesNo) { Yes = !Yes; return; }
            var free = FreeSlots().ToList();
            if (free.Count == 0) { Cursor = -1; return; }
            int c = Cursor;
            for (int i = 0; i < 3; i++)
            {
                c = ((c + dir) % 3 + 3) % 3;
                if (free.Contains(c)) { Cursor = c; break; }
            }
            if (Shrine.kind == "shop") Text = Format(Shrine.messages.FirstOrDefault() ?? data.Message(0x05));
        }

        // Espacio. Devuelve true si se ha comprado algo.
        public bool Confirm()
        {
            Refused = false;
            if (AskYesNo)
            {
                AskYesNo = false;
                // [I] La contraseña del original se sustituirá por guardar la partida (backlog).
                Text = Yes ? "(EL REMAKE GUARDARA AQUI LA PARTIDA)\n" + Format(data.Message(0x10)) : Format(data.Message(0x0E));
                return false;
            }
            var item = Cursor >= 0 ? ItemAt(Cursor) : null;
            if (item == null || Sold.Contains(Cursor)) return false;
            if (item.rule == "shield" && inventory.ShieldLevel < item.needsShield) { Refused = true; return false; }
            if (Coins < item.price)
            {
                Refused = true;
                Text = Format(data.Message(0x12));
                return false;
            }
            Coins -= item.price;
            int cap = data.rules.counterCap;
            switch (item.rule)
            {
                case "quiver": Arrows = System.Math.Min(cap, Arrows + data.rules.quiverArrows); break;
                case "key": Keys = System.Math.Min(cap, Keys + data.rules.keyAmount); break;
                default: inventory.Add(item.type); break;
            }
            Sold.Add(Cursor);
            Move(1);
            return true;
        }
    }
}
