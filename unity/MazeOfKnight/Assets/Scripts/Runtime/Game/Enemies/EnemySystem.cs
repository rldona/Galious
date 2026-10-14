using System;
using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Enemigos de la sala actual. Al entrar en una sala se crean los de sus generadores (como b04:6000 al cargar
    // la sala) y al salir se borran; al volver, reaparecen. Cada tick: IA, contacto con el jugador y espadazos.
    public partial class EnemySystem
    {
        static readonly Dictionary<int, Func<Enemy>> Factories = new Dictionary<int, Func<Enemy>>
        {
            // en las salas
            { 0x01, () => new Charger() },
            { 0x02, () => new Crawler() },
            { 0x03, () => new CeilingSlug() },
            { 0x04, () => new Piston() },
            { 0x05, () => new Snake() },
            { 0x07, () => new Hopper() },
            { 0x0A, () => new Knight() },
            { 0x0B, () => new Dart() },
            { 0x0C, () => new FlameMan() },
            { 0x0E, () => new Bat() },
            { 0x0F, () => new GrowingBall() },
            { 0x10, () => new Lurker() },
            { 0x11, () => new Hedgehog() },
            { 0x12, () => new Armadillo() },
            { 0x13, () => new FlameMan() },
            { 0x14, () => new Emerger() },
            { 0x15, () => new Orbiter() },
            { 0x16, () => new Phantom() },
            { 0x17, () => new Sentinel() },
            { 0x18, () => new Roller() },
            { 0x19, () => new FusingBat() },
            { 0x1F, () => new Runner() },
            { 0x20, () => new SlowCharger() },
            { 0x22, () => new FireSpitter() },
            { 0x23, () => new Statue() },
            { 0x24, () => new LevelJumper() },
            { 0x25, () => new Skeleton() },
            { 0x26, () => new Ghost() },
            { 0x27, () => new DiveBird() },
            { 0x28, () => new BounceBug() },
            { 0x29, () => new Bubble() },
            { 0x2A, () => new Teleporter() },
            { 0x2B, () => new Crawler() },
            { 0x2C, () => new InchWorm() },
            { 0x2D, () => new Soldier() },
            { 0x2E, () => new Sprout() },
            { 0x2F, () => new Mimic() },
            { 0x31, () => new Spikes() },
            { 0x32, () => new ArrowTrap() },
            { 0x34, () => new Block() },
            { 0x36, () => new ChainOrb() },
            { 0x3A, () => new WallMouth() },
            { 0x3C, () => new Sorcerer() },
            { 0x40, () => new BigSpirit() },
            { 0x42, () => new WaterMonster() },
            { 0x43, () => new WideWalker() },
            { 0x49, () => new SplitFly() },
            { 0x4C, () => new SporeCloud() },
            { 0x4E, () => new Soul() },
            { 0x50, () => new WallMouth() },
            { 0x51, () => new ArrowTrap() },
            { 0x64, () => new InchWorm() },
            { 0x69, () => new FloatingHead() },
            // muerte y botín
            { 0x5B, () => new DeathExplosion() },
            { 0x5D, () => new LootDrop() },
            { 0x5E, () => new LootDrop() },
            { 0x5F, () => new LootDrop() },
            // generados periódicamente (docs/enemy_ai.md, "generados periódicamente")
            { 0x08, () => new Geyser() },
            { 0x09, () => new BouncingBall() },
            { 0x0D, () => new LeapFish() },
            { 0x1B, () => new Droplet() },
            { 0x21, () => new FallingWalker() },
            { 0x30, () => new Wader() },
            { 0x35, () => new RollingStone() },
            { 0x38, () => new CeilingClimber() },
            { 0x3E, () => new Weaver() },
            { 0x6A, () => new Splash() },
            // hijos (los crea otro enemigo)
            { 0x37, () => new FusingBat() },
            { 0x52, () => new SplitFly() },
            { 0x53, () => new SplitFly() },
        };

        public const int MaxEnemies = 16;   // [I] huecos E800 + 0x20·n; el original limita con E620/E635

        public static bool Implemented(int type) => Factories.ContainsKey(type) || Projectiles.ContainsKey(type);

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

        // Tabla de cajas de colisión (b14:9006); vacía = cajas físicas.
        public IReadOnlyList<BoxRect> Boxes = new BoxRect[0];

        public EnemySystem(IEnumerable<EnemyInfo> infos, int seed = 1, LootTable loot = null, IReadOnlyList<BoxRect> boxes = null)
        {
            if (loot != null) Loot = loot;
            if (boxes != null) Boxes = boxes;
            catalog = infos.ToDictionary(e => e.type);
            Context.Random = new Random(seed);
        }

        public void EnterRoom(RoomWorld world, RoomModel room, PlayerMotor player)
        {
            Context.World = world;
            Context.Player = player;
            Context.Enemies = enemies;
            Context.SpawnChild = SpawnChild;
            Context.ResetRoom();
            ResetLoot();
            StartGenerators(room);
            lastPlayerY = player.Y;
            enemies.Clear();
            Room = room;
            Missing = 0;
            foreach (var s in room.spawns)
            {
                if (Create(s.type, s.x, s.y, 0) == null && !Factories.ContainsKey(s.type))
                    Missing++;
            }
        }

        Enemy Create(int type, float x, float y, int param, PeriodicJson generator = null)
        {
            if (!Factories.TryGetValue(type, out var make) && !Projectiles.TryGetValue(type, out make))
                return null;
            if (enemies.Count(e => e.Alive) >= MaxEnemies)
                return null;
            var e = make();
            e.Type = type;
            e.Room = Room;
            e.X = x;
            e.Y = y;
            e.Param = param;
            e.Generator = generator;
            if (catalog.TryGetValue(type, out var info))
            {
                e.Hp = info.hp;
                e.Damage = info.damage;
                e.Invulnerable = info.invulnerable;
                if (info.box >= 0 && info.box < Boxes.Count)
                {
                    e.HitBox = Boxes[info.box];
                    if (info.box == 0x4E && Boxes.Count > 0x4F) e.LeftBox = Boxes[0x4F];
                }
            }
            e.Init(Context);
            if (e is DeathExplosion boom)
                boom.Finished = b => DropLoot(b.DeadType, b.X, b.Y);
            enemies.Add(e);
            Spawned?.Invoke(e);
            return e;
        }

        // Un enemigo crea otro (b04:6551 / 65DA) en (x, y). Se añade al final y empieza a moverse en el tick siguiente.
        Enemy SpawnChild(int type, float x, float y) => Create(type, x, y, 0);

        int lastPlayerY;

        public void Tick()
        {
            Context.Tick++;
            TickDeaths();
            TickGenerators();
            Context.PlayerDy = Context.Player.Y - lastPlayerY;
            lastPlayerY = Context.Player.Y;
            int n = enemies.Count;
            for (int i = 0; i < n; i++)
            {
                var e = enemies[i];
                if (!e.Alive)
                    continue;
                e.Tick(Context);
                // b06:B452: se borra el enemigo que sale de la pantalla (y ≥ 0xB8 o x ≥ 0xF9; por debajo de 0 el byte
                // da la vuelta y también cuenta).
                if (e.Y >= 0xB8 || e.X >= 0xF9 || e.X < 0 || e.Y < 0)
                    e.Alive = false;
            }
            enemies.RemoveAll(e => !e.Alive);
        }

        public int ImmuneItem(Enemy e) => catalog.TryGetValue(e.Type, out var info) ? info.immuneItem : 0;

        // Enemigo que toca al jugador (caja del jugador: x − 8 … x + 7, y − 16 … y − 1), o null.
        public Enemy Touching(PlayerMotor m, ICollection<int> collected)
        {
            float px0 = m.X + P.ContactLeft, px1 = m.X + P.ContactRight + 1, py0 = m.Y + P.ContactTop, py1 = m.Y + P.ContactBottom + 1;
            foreach (var e in enemies)
            {
                if (!e.Alive || !e.Visible || !e.Harmful || e.Damage <= 0 && !e.DiesOnTouch && !(e is Fairy))
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

        // Quita vida a un enemigo (espada o arma): al llegar a 0 muere, explota y sortea el botín; si no, avisa del golpe.
        public void Damage(Enemy e, int damage)
        {
            e.Hp -= damage;
            Hit?.Invoke(e);
            if (e.Hp <= 0)
            {
                e.Alive = false;
                e.Die(Context);
                Killed?.Invoke(e);
                StartDeath(e);
            }
            else
                e.Hurt(Context);
        }

        // Enemigos vivos y visibles cuya caja se solapa con [x0, x1) × [y0, y1).
        public List<Enemy> Overlapping(float x0, float y0, float x1, float y1)
        {
            var list = new List<Enemy>();
            foreach (var e in enemies)
            {
                if (!e.Alive || !e.Visible || e is LootDrop || e is DeathExplosion)
                    continue;
                var (ex0, ey0, ex1, ey1) = e.Box();
                if (x0 < ex1 && x1 > ex0 && y0 < ey1 && y1 > ey0)
                    list.Add(e);
            }
            return list;
        }

        // Un espadazo recién empezado: 1 de daño (2 con el Saber) a cada enemigo que alcance.
        public void Strike(PlayerMotor m, ICollection<int> collected)
        {
            var (sx0, sx1, sy0, sy1) = m.SwordRect();
            int damage = collected.Contains(ItemSaber) ? 2 : 1;
            foreach (var e in enemies.ToList())
            {
                if (!e.Alive || !e.Visible)
                    continue;
                var (x0, y0, x1, y1) = e.Box();
                if (sx1 < x0 || sx0 >= x1 || sy1 < y0 || sy0 >= y1)
                    continue;
                if (!e.OnHit(Context))
                    continue;
                Damage(e, damage);
            }
        }
    }
}
