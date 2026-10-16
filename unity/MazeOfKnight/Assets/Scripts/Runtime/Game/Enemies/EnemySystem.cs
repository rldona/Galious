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

        public const int MaxEnemies = 16;

        // Enemigos marcados (b04:60AE, b04:6584, b03:B6AE): en una sala con evento, los de estos tipos cuentan; al
        // morir el último se dispara el evento de la sala.
        public static readonly HashSet<int> MarkedTypes = new HashSet<int>
            { 0x2F, 0x2D, 0x24, 0x0F, 0x2E, 0x22, 0x0C, 0x0E, 0x43, 0x10, 0x2A, 0x4C, 0x16, 0x1F, 0x13, 0x40, 0x15, 0x0A, 0x34, 0x14, 0x12, 0x07, 0x17 };
        public bool EventPending;           // la sala tiene evento y aún no se ha disparado
        int markedAlive;
        public event Action<RoomModel> RoomCleared;   // [I] huecos E800 + 0x20·n; el original limita con E620/E635

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
        // Espadazo contra un enemigo con IX+2 bit 2: «clinc» (b03:AE88) y sin daño.
        public event Action<Enemy> Clinked;

        // Tabla de cajas de colisión (b14:9006); vacía = cajas físicas.
        public IReadOnlyList<BoxRect> Boxes = new BoxRect[0];

        public EnemySystem(IEnumerable<EnemyInfo> infos, int seed = 1, LootTable loot = null, IReadOnlyList<BoxRect> boxes = null)
        {
            if (loot != null) Loot = loot;
            if (boxes != null) Boxes = boxes;
            Context.Boxes = Boxes;
            catalog = infos.ToDictionary(e => e.type);
            Context.Random = new Random(seed);
        }

        // eventPending: la sala tiene un evento de enemigos marcados aún sin disparar.
        public void EnterRoom(RoomWorld world, RoomModel room, PlayerMotor player, bool eventPending = false)
        {
            EventPending = eventPending;
            Context.World = world;
            Context.Player = player;
            Context.Enemies = enemies;
            Context.SpawnChild = SpawnChild;
            Context.Boxes = Boxes;
            Context.ResetRoom();
            ResetLoot();
            FreezeTicks = 0;
            StartGenerators(room);
            lastPlayerY = player.Y;
            enemies.Clear();
            Room = room;
            Missing = 0;
            markedAlive = 0;
            foreach (var s in room.spawns)
            {
                if (Create(s.type, s.x, s.y, 0) == null && !Factories.ContainsKey(s.type) && !Projectiles.ContainsKey(s.type))
                    Missing++;
            }
            // Sala del bebé: al cargarla se elige su piedra, (0..3) + (0..7) en el orden de la lista; con 10, ninguna
            // (b06:AABE, docs/ending.md).
            var rocks = enemies.OfType<BabyRock>().ToList();
            if (rocks.Count > 0)
            {
                int pick = RemakeRules.BabyAlwaysPresent ? Context.Rand(0, rocks.Count - 1) : Context.Rand(0, 3) + Context.Rand(0, 7);
                if (pick < rocks.Count) rocks[pick].HasBaby = true;
            }
        }

        Enemy ticking;     // el que está actuando: lo que cree es suyo (Enemy.Parent)

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
            e.Parent = ticking;
            if (catalog.TryGetValue(type, out var info))
            {
                e.Hp = info.hp;
                e.Damage = info.damage;
                e.Invulnerable = info.invulnerable;
                if (info.box >= 0 && info.box < Boxes.Count)
                {
                    e.HitBox = Boxes[info.box];
                    e.BoxIndex = info.box;
                    if (info.box == 0x4E && Boxes.Count > 0x4F) e.LeftBox = Boxes[0x4F];
                }
            }
            if (EventPending && MarkedTypes.Contains(type))
            {
                e.Marked = true;
                markedAlive++;
            }
            e.Init(Context);
            if (e is DeathExplosion boom)
                boom.Finished = b => DropLoot(b.DeadType, b.X, b.Y);
            enemies.Add(e);
            Spawned?.Invoke(e);
            return e;
        }

        // Crea el jefe (b01:7F56): vida de 7F97 (a la mitad con el Holy Water) y su posición inicial.
        public Boss SpawnBoss(BossInfo info, bool holyWater, bool cloak, bool cross)
        {
            Boss b = info.id switch
            {
                1 => new BonyDragon(), 2 => new ElusiveDragon(), 3 => new Arlyane(), 4 => new SeaAnemone(), 6 => new KingDragon(),
                7 => new BigLip(), 8 => new Monster(), 9 => new Galious { HasCross = cross }, 10 => new Donra(), 11 => new JumpSlime(),
                _ => null,
            };
            if (b == null) return null;
            b.BossId = info.id; b.Info = info; b.Type = 0x100 + info.id; b.Room = Room;
            b.MaxHp = info.hp;
            b.Hp = holyWater ? info.hpHolyWater : info.hp;
            b.WeaponMask = info.weaponMask;
            b.Damage = cloak ? info.contactDamageCloak : info.contactDamage;
            if (RemakeRules.HollowBosses)
                b.Damage = Math.Max(1, RemakeRules.BossContactMasks(info.contactDamage) - (cloak ? 1 : 0));   // la Cloak, una menos
            b.X = info.init.x; b.Y = info.init.y;
            b.Init(Context);
            enemies.Add(b);
            Spawned?.Invoke(b);
            return b;
        }

        public Enemy Spawn(int type, float x, float y, int param = 0) => Create(type, x, y, param);

        public int AliveCount => enemies.Count(e => e.Alive);

        public Boss ActiveBoss => enemies.OfType<Boss>().FirstOrDefault(b => b.Alive);

        // Un enemigo crea otro (b04:6551 / 65DA) en (x, y). Se añade al final y empieza a moverse en el tick siguiente.
        Enemy SpawnChild(int type, float x, float y) => Create(type, x, y, 0);

        int lastPlayerY;

        // Biblia: enemigos, generadores (y el jefe) quietos; tocarlos sigue haciendo daño. Acaba al cambiar de sala.
        public int FreezeTicks;

        public void Tick()
        {
            Context.Tick++;
            TickDeaths();
            if (FreezeTicks > 0)
            {
                FreezeTicks--;
                return;
            }
            TickGenerators();
            Context.PlayerDy = Context.Player.Y - lastPlayerY;
            lastPlayerY = Context.Player.Y;
            int n = enemies.Count;
            for (int i = 0; i < n; i++)
            {
                var e = enemies[i];
                if (!e.Alive)
                    continue;
                ticking = e;
                e.Tick(Context);
                ticking = null;
                // b06:B452: se borra el enemigo que sale de la pantalla (y ≥ 0xB8 o x ≥ 0xF9; por debajo de 0 el byte
                // da la vuelta y también cuenta).
                if (!e.KeepOffScreen && (e.Y >= 0xB8 || e.X >= 0xF9 || e.X < 0 || e.Y < 0))
                    e.Alive = false;
            }
            enemies.RemoveAll(e => !e.Alive);
        }

        public bool IsInvulnerable(int type) => catalog.TryGetValue(type, out var info) && info.invulnerable;

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
                // Contra el cuerpo de un jefe el jugador cuenta como 16 × 16 desde (y − 14, x − 6) (b03:B9B2).
                float qx0 = px0, qx1 = px1, qy0 = py0, qy1 = py1;
                if (e is Boss) { qx0 = m.X - 6; qx1 = m.X + 10; qy0 = m.Y - 14; qy1 = m.Y + 2; }
                foreach (var (x0, y0, x1, y1) in e.ContactRects())
                    if (qx0 < x1 && qx1 > x0 && qy0 < y1 && qy1 > y0)
                        return e;
            }
            return null;
        }

        // Golpe válido a un jefe: resta vida (sonido 0x27) y, si llega a 0, se acaba el combate.
        public event Action<Boss> BossDefeated;
        public void HitBoss(Boss b, int damage)
        {
            b.Hp -= damage;
            Hit?.Invoke(b);
            b.OnBossHit(Context);
            if (b.Hp > 0) return;
            b.Alive = false;
            BossDefeated?.Invoke(b);
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
                if (e.Marked && EventPending && --markedAlive <= 0)
                {
                    EventPending = false;
                    RoomCleared?.Invoke(Room);
                }
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
                if (e.WeaponRects().Any(r => x0 < r.x1 && x1 > r.x0 && y0 < r.y1 && y1 > r.y0))
                    list.Add(e);
            }
            return list;
        }

        // Un espadazo recién empezado: 1 de daño (2 con el Saber) a cada enemigo que alcance. Escudos (b03:B6C1): con
        // IX+2 bit 2 suena el «clinc» sin daño; con el bit 1, el golpe solo vale si el enemigo mira hacia el mismo lado
        // que el jugador (bit 0 = E503) y si no se pierde sin daño.
        public void Strike(PlayerMotor m, ICollection<int> collected)
        {
            var (sx0, sx1, sy0, sy1) = m.SwordRect();
            int damage = collected.Contains(ItemSaber) ? 2 : 1;
            foreach (var e in enemies.ToList())
            {
                if (!e.Alive || !e.Visible)
                    continue;
                if (!e.WeaponRects().Any(r => !(sx1 < r.x0 || sx0 >= r.x1 || sy1 < r.y0 || sy0 >= r.y1)))
                    continue;
                if (e is Boss boss)
                {
                    // Jefes: la espada vale con todos (bit 0), si su prueba de choque lo deja; el Saber no suma.
                    if (boss.CanBeHitNow && boss.Vulnerable(0)) HitBoss(boss, 1);
                    continue;
                }
                if (e.Hittable && e.Deflects(m.FacingLeft))
                {
                    if (e.ShieldAll) Clinked?.Invoke(e);
                    continue;
                }
                if (!e.OnHit(Context))
                    continue;
                Damage(e, damage);
            }
        }
    }
}
