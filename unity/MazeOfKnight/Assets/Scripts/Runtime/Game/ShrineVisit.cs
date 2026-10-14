using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;

namespace MazeOfKnight.Game
{
    // Una visita a un santuario (docs/design_bible/08_santuarios.md). Tienda: ← → mueven la mano (salta los huecos
    // vacíos y lo ya comprado en esta visita), espacio compra; sin monedas suficientes sale "COINS ARE NOT ENOUGH.";
    // los escudos van en orden (plata pide cobre, oro pide plata). El carcaj da 100 flechas y la llave, 1 (tope 999).
    // El juego no mira si ya tienes el objeto. Demeter (guardar, en vez de la contraseña) y la Muerte preguntan YES/NO.
    // ↓ sale.
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
        // Muerte: vidas de reserva y crédito de resurrección (por personaje), y quién juega.
        readonly int[] lives, credits;
        readonly int other;
        public bool Revived { get; private set; }
        public int RevivedCharacter => other;
        public bool SaveRequested { get; private set; }
        // Tras la respuesta de la Muerte se sale sola a los 180 frames; mientras, ↓ no hace nada.
        public int AutoExitFrames { get; private set; }

        public ShrineVisit(Shrine shrine, ShrinesData data, Inventory inventory, int arrows, int coins, int keys,
            int[] lives = null, int[] credits = null, Character active = Character.Popolon)
        {
            Shrine = shrine;
            this.data = data;
            this.inventory = inventory;
            Arrows = arrows; Coins = coins; Keys = keys;
            this.lives = lives ?? new[] { 1, 0 };
            this.credits = credits ?? new[] { 1, 1 };
            other = 1 - (int)active;
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
                    // Si el otro no tiene vidas, le queda crédito y llevas 100 flechas, 100 monedas y 20 llaves, lo ofrece.
                    var r = data.rules;
                    if (this.lives[other] == 0 && this.credits[other] > 0 && Arrows >= r.resurrectionArrows &&
                        Coins >= r.resurrectionCoins && Keys >= r.resurrectionKeys)
                    {
                        Text = Format(data.Message(0x02));
                        AskYesNo = true;
                    }
                    else
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
                if (Shrine.kind == "resurrection")
                {
                    var r = data.rules;
                    if (Yes)
                    {
                        Arrows -= r.resurrectionArrows; Coins -= r.resurrectionCoins; Keys -= r.resurrectionKeys;
                        lives[other] = 1;
                        credits[other]--;
                        Revived = true;
                        Text = Format(data.Message(0x03));
                    }
                    else
                        Text = Format(data.Message(0x04));
                    AutoExitFrames = r.deathAutoExitFrames > 0 ? r.deathAutoExitFrames : 180;
                    return false;
                }
                // Remake: en vez de la contraseña, se guarda la partida (SaveGame.cs).
                SaveRequested = Yes;
                Text = Yes ? "PARTIDA GUARDADA.\n" + Format(data.Message(0x10)) : Format(data.Message(0x0E));
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
