using System;
using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Enemigos de la sala actual. Al entrar en una sala se crean los de sus generadores (como b04:6000 al cargar
    // la sala) y al salir se borran; al volver, reaparecen. Cada tick: IA, contacto con el jugador y espadazos.
    public class EnemySystem
    {
        static readonly Dictionary<int, Func<Enemy>> Factories = new Dictionary<int, Func<Enemy>>
        {
            { 0x0E, () => new Bat() },
            { 0x25, () => new Skeleton() },
            { 0x31, () => new Spikes() },
            { 0x18, () => new Roller() },
            { 0x34, () => new Block() },
            { 0x2E, () => new Sprout() },
        };

        public static bool Implemented(int type) => Factories.ContainsKey(type);

        public const int ItemSaber = 0x25;   // la espada quita 1 más (docs/items_enemies.md)

        readonly Dictionary<int, EnemyInfo> catalog;
        readonly List<Enemy> enemies = new List<Enemy>();
        public readonly EnemyContext Context = new EnemyContext();
        public RoomModel Room { get; private set; }
        public int Missing { get; private set; }   // generadores de la sala con tipos aún sin implementar

        public IReadOnlyList<Enemy> Enemies => enemies;

        public event Action<Enemy> Spawned;
        public event Action<Enemy> Killed;
        public event Action<Enemy> Hit;

        public EnemySystem(IEnumerable<EnemyInfo> infos, int seed = 1)
        {
            catalog = infos.ToDictionary(e => e.type);
            Context.Random = new Random(seed);
        }

        public void EnterRoom(RoomWorld world, RoomModel room, PlayerMotor player)
        {
            Context.World = world;
            Context.Player = player;
            enemies.Clear();
            Room = room;
            Missing = 0;
            foreach (var s in room.spawns)
            {
                if (!Factories.TryGetValue(s.type, out var make))
                {
                    Missing++;
                    continue;
                }
                var e = make();
                e.Type = s.type;
                e.Room = room;
                e.X = s.x;
                e.Y = s.y;
                if (catalog.TryGetValue(s.type, out var info))
                {
                    e.Hp = info.hp;
                    e.Damage = info.damage;
                    e.Invulnerable = info.invulnerable;
                }
                e.Init(Context);
                enemies.Add(e);
                Spawned?.Invoke(e);
            }
        }

        public void Tick()
        {
            Context.Tick++;
            foreach (var e in enemies)
                if (e.Alive)
                    e.Tick(Context);
        }

        public int ImmuneItem(Enemy e) => catalog.TryGetValue(e.Type, out var info) ? info.immuneItem : 0;

        // Enemigo que toca al jugador (caja del jugador: x − 8 … x + 7, y − 16 … y − 1), o null.
        public Enemy Touching(PlayerMotor m, ICollection<int> collected)
        {
            float px0 = m.X + P.FootLeft, px1 = m.X + P.FootRight + 1, py0 = m.Y - P.EnemyHitHeight, py1 = m.Y;
            foreach (var e in enemies)
            {
                if (!e.Alive || !e.Visible || !e.Harmful)
                    continue;
                int immune = ImmuneItem(e);
                if (immune != 0 && collected.Contains(immune))
                    continue;
                var (x0, y0, x1, y1) = e.Box();
                if (px0 < x1 && px1 > x0 && py0 < y1 && py1 > y0)
                    return e;
            }
            return null;
        }

        // Un espadazo recién empezado: 1 de daño (2 con el Saber) a cada enemigo que alcance.
        public void Strike(PlayerMotor m, ICollection<int> collected)
        {
            var (sx0, sx1, sy0, sy1) = m.SwordRect();
            int damage = collected.Contains(ItemSaber) ? 2 : 1;
            foreach (var e in enemies)
            {
                if (!e.Alive || !e.Visible)
                    continue;
                var (x0, y0, x1, y1) = e.Box();
                if (sx1 < x0 || sx0 >= x1 || sy1 < y0 || sy0 >= y1)
                    continue;
                if (!e.OnHit(Context))
                    continue;
                e.Hp -= damage;
                Hit?.Invoke(e);
                if (e.Hp <= 0)
                {
                    e.Alive = false;
                    Killed?.Invoke(e);
                }
            }
        }
    }
}
