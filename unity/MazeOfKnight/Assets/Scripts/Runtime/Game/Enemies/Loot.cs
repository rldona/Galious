using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;

namespace MazeOfKnight.Game.Enemies
{
    // Muerte y botín (docs/items_enemies.md, "Muerte y botín"). El enemigo muerto a espadazos deja una explosión
    // (0x5B) de 24 ticks; al acabar se decide el botín según su tipo: la regla dice en qué muertes de ese tipo hay
    // sorteo (por visita a la sala, con 2 tipos como mucho) y el nivel, qué sale. El objeto (0x5D flechas, 0x5E
    // monedas, 0x5F llaves) se queda quieto en la rejilla de 8 px hasta que lo recoges o sales de la sala.
    public partial class EnemySystem
    {
        public const int ExplosionTicks = 24;          // 8 fotogramas de 3 ticks
        public const int BigExplosionInterval = 16;    // [I] b04:69D2 crea una cada 16 llamadas
        public const int ExplosionType = 0x5B;

        public LootTable Loot = new LootTable();

        readonly List<int> trackedTypes = new List<int>();          // E6C2/E6C3
        readonly Dictionary<int, int> kills = new Dictionary<int, int>();   // E6C4/E6C5
        readonly List<(int tick, int type, float x, float y)> pendingExplosions = new List<(int, int, float, float)>();

        public event System.Action<LootDrop> Dropped;

        void ResetLoot()
        {
            trackedTypes.Clear();
            kills.Clear();
            pendingExplosions.Clear();
        }

        // El enemigo muere: una explosión (o 4, una cada 16 ticks, en los grandes) que al acabar sortea el botín.
        void StartDeath(Enemy e)
        {
            int n = catalog.TryGetValue(e.Type, out var info) ? info.deathExplosions : 1;
            if (n <= 0) return;
            if (n == 1)
            {
                Explode(e.Type, e.X, e.Y);
                return;
            }
            (float dy, float dx)[] offsets = { (8, 8), (0, 16), (16, 0), (16, 16) };
            for (int i = 0; i < n && i < offsets.Length; i++)
                pendingExplosions.Add((Context.Tick + i * BigExplosionInterval, e.Type, e.X + offsets[i].dx, e.Y + offsets[i].dy));
        }

        void Explode(int type, float x, float y)
        {
            var boom = Create(ExplosionType, x, y, 0) as DeathExplosion;
            if (boom != null) boom.DeadType = type;
        }

        void TickDeaths()
        {
            for (int i = pendingExplosions.Count - 1; i >= 0; i--)
            {
                var p = pendingExplosions[i];
                if (Context.Tick < p.tick) continue;
                pendingExplosions.RemoveAt(i);
                Explode(p.type, p.x, p.y);
            }
        }

        // ¿Toca sorteo? (b04:6A3B). Cuenta la muerte del tipo si es uno de los 2 que se siguen en la sala.
        public bool LootRoll(int type)
        {
            if (!catalog.TryGetValue(type, out var info) || info.lootRule >= 4) return false;
            if (info.lootWhenLastAlive)
                return !enemies.Any(e => e.Alive && e.Type == type);
            if (!trackedTypes.Contains(type))
            {
                if (trackedTypes.Count >= 2) return false;
                trackedTypes.Add(type);
                kills[type] = 0;
            }
            kills[type]++;
            return info.lootKills.Contains(kills[type]);
        }

        // Sorteo (b04:6A77): v = 0..15 contra los umbrales del nivel.
        public void DropLoot(int type, float x, float y)
        {
            if (!LootRoll(type)) return;
            int level = catalog.TryGetValue(type, out var info) ? System.Math.Max(0, System.Math.Min(3, info.lootLevel)) : 0;
            int v = Context.Random.Next(16);
            int obj, amount;
            if (v < Loot.coinBelow[level]) { obj = Loot.coinObject; amount = Loot.coinAmount[level]; }
            else if (v < Loot.arrowBelow[level]) { obj = Loot.arrowObject; amount = Loot.arrowAmount[level]; }
            else if (v < Loot.keyBelow[level]) { obj = Loot.keyObject; amount = Loot.keyAmount[level]; }
            else return;
            var drop = Create(obj, (int)x & ~7, (int)y & ~7, 0) as LootDrop;
            if (drop == null) return;   // sala llena: se pierde
            drop.Amount = amount;
            Dropped?.Invoke(drop);
        }

        // Objeto de botín que toca el jugador (caja: y del jugador en [y + 2, y + 30], x en [x − 6, x + 22]), o null.
        public LootDrop PickUp(PlayerMotor m)
        {
            foreach (var e in enemies)
            {
                if (!(e is LootDrop d) || !d.Alive) continue;
                if (m.Y >= d.Y + 2 && m.Y <= d.Y + 30 && m.X >= d.X - 6 && m.X <= d.X + 22)
                {
                    d.Alive = false;
                    return d;
                }
            }
            return null;
        }
    }

    // 0x5B: explosión al morir. No hace daño ni se puede golpear; al acabar decide el botín.
    public class DeathExplosion : Enemy
    {
        public int DeadType;
        int timer = EnemySystem.ExplosionTicks;
        public System.Action<DeathExplosion> Finished;

        public override void Init(EnemyContext ctx) { Harmful = Hittable = false; }

        public override void Tick(EnemyContext ctx)
        {
            if (--timer > 0) return;
            Alive = false;
            Finished?.Invoke(this);
        }
    }

    // 0x5D flechas, 0x5E monedas, 0x5F llaves: bloque de 2×2 tiles quieto (no cae ni caduca).
    public class LootDrop : Enemy
    {
        public int Amount;

        public override void Init(EnemyContext ctx) { Harmful = Hittable = false; }

        public override void Tick(EnemyContext ctx) { }
    }
}
