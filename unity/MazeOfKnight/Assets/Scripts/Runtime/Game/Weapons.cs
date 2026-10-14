using System;
using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game.Enemies;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game
{
    // Un proyectil de arma secundaria (huecos E540… y, las minas, E5A0…). (X, Y) es la esquina de arriba a la izquierda.
    public class Projectile
    {
        public WeaponInfo Weapon;
        public float X, Y;
        public int Dir;                  // 1 derecha, −1 izquierda
        public bool Alive = true;
        public int Age;
        // Fuego rodante: sentido de avance (mx, my) y lado donde está la superficie (sx, sy).
        public int Mx, My, Sx, Sy, Turns;
        public float SpawnX;
        // Fuego: el primer tick solo se asienta. Mina: armada, estallando, ya dañó al jugador.
        public bool Settled, Armed, Exploding, HurtPlayer;
        public int ExplosionTicks;
        public readonly HashSet<Enemy> Touched = new HashSet<Enemy>();

        public int Id => Weapon.id;
    }

    // Armas secundarias (docs/player.md, "Armas secundarias"; datos en extracted/unity/weapons.json). Disparo con una
    // pulsación nueva de M, 1 flecha por disparo, límite por personaje y grupo de huecos; cada arma con su movimiento.
    public class WeaponSystem
    {
        public const int ItemSaber = 0x25, ItemRing = 0x1F;

        readonly WeaponsData data;
        readonly List<Projectile> active = new List<Projectile>();
        public IReadOnlyList<Projectile> Active => active;
        public RoomModel Room { get; private set; }

        public event Action<Projectile> Spawned;
        public event Action<Projectile> Exploded;
        // La mina alcanza al jugador: daño y dirección del retroceso.
        public event Action<int, int> PlayerHit;

        public WeaponSystem(WeaponsData data) => this.data = data ?? new WeaponsData();

        public WeaponsData Data => data;

        public void EnterRoom(RoomModel room)
        {
            foreach (var p in active) p.Alive = false;
            active.Clear();
            Room = room;
        }

        // ¿Se puede disparar ahora? (estado del jugador y límite de huecos)
        public string CanFire(WeaponInfo w, PlayerMotor m, ICollection<int> items, int arrows)
        {
            if (w == null || w.motion == "none") return "sin arma";
            if (arrows < w.cost) return "sin flechas";
            bool ground = m.State == MotorState.Ground || m.State == MotorState.JumpPrep;
            bool air = m.State == MotorState.Jump || m.State == MotorState.Fall;
            if (!(ground && w.ground || air && w.air)) return "aquí no";
            if (w.blockedOnLift && m.OnLift) return "aquí no";
            int max = m.Character == Character.Popolon
                ? (w.maxItem != 0 && items.Contains(w.maxItem) ? w.maxPopolonWithItem : w.maxPopolon)
                : w.maxAphrodite;
            if (active.Count(p => p.Alive && p.Weapon.pool == w.pool) >= max) return "límite";
            return null;
        }

        // Dispara (crea el proyectil y lo mueve una vez). Devuelve el proyectil o null si no se pudo (no gasta).
        public Projectile Fire(WeaponInfo w, PlayerMotor m, RoomWorld world, EnemySystem enemies, Rocks rocks, ICollection<int> items)
        {
            if (Room != m.Room) EnterRoom(m.Room);
            int dir = m.FacingLeft ? -1 : 1;
            var p = new Projectile { Weapon = w, Dir = dir };
            p.X = m.X + (dir > 0 ? w.spawnDxRight : w.spawnDxLeft);
            p.Y = m.Y + w.spawnDy;
            if (w.spawnAlignY8) p.Y = Math.Max(w.spawnMinY, (int)p.Y & ~7);
            p.SpawnX = p.X;
            if (w.motion == "wrapPlatform") { p.Mx = dir; p.My = 0; p.Sx = 0; p.Sy = 1; }
            if (w.motion == "mine")
            {
                var mi = data.mine;
                if (!world.Solid(m.Room, (int)p.X + mi.needsFloorAt[0], m.Y) || !world.Solid(m.Room, (int)p.X + mi.needsFloorAt[1], m.Y))
                    return null;
            }
            active.Add(p);
            Spawned?.Invoke(p);
            if (w.firstTickMoves) Step(p, m, world, enemies, rocks, items);
            return p;
        }

        public void Tick(PlayerMotor m, RoomWorld world, EnemySystem enemies, Rocks rocks, ICollection<int> items)
        {
            if (Room != m.Room) EnterRoom(m.Room);
            foreach (var p in active.ToList())
                if (p.Alive) Step(p, m, world, enemies, rocks, items);
            active.RemoveAll(p => !p.Alive);
        }

        // Caja del proyectil, semiabierta [x0, x1) × [y0, y1).
        public (float x0, float y0, float x1, float y1) Box(Projectile p)
        {
            var b = p.Exploding ? data.mine.explosionHitbox : p.Weapon.hitbox;
            float x = b.alignTo8 ? (int)p.X & ~7 : p.X, y = b.alignTo8 ? (int)p.Y & ~7 : p.Y;
            return (x + b.dx, y + b.dy, x + b.dx + b.w + 1, y + b.dy + b.h + 1);
        }

        bool Solid(RoomWorld world, float x, float y) => y >= P.RoomTopPx && world.Solid(Room, (int)Math.Floor(x), (int)Math.Floor(y));

        void Step(Projectile p, PlayerMotor m, RoomWorld world, EnemySystem enemies, Rocks rocks, ICollection<int> items)
        {
            p.Age++;
            switch (p.Weapon.motion)
            {
                case "straight": StepStraight(p, world); break;
                case "wrapPlatform": StepWrap(p, world); break;
                case "floorFall": StepFire(p, world); break;
                case "mine": StepMine(p, m, enemies, items); return;
            }
            if (!p.Alive)
            {
                // Las piedras también son tiles sólidos: el proyectil que choca con una la daña antes de morir (demo 1,
                // castillo 120: tres flechas rompen la piedra de 15) [O].
                var (bx0, by0, bx1, by1) = Box(p);
                int rd = m.Character == Character.Popolon || items.Contains(ItemRing) ? p.Weapon.rockDamagePopolon : p.Weapon.rockDamageAphrodite;
                rocks?.StrikeRect(Room, world, (int)bx0 - 8, (int)bx1 + 7, (int)by0, (int)by1 - 1, rd);
                return;
            }
            if (p.X < data.killMinX || p.X >= data.killMaxX || p.Y >= data.killMaxY) { p.Alive = false; return; }
            HitThings(p, m, world, enemies, rocks, items);
        }

        // Flechas: 8 (o 4, la cerámica) px/tick; mueren en una pared en su tile o el siguiente.
        void StepStraight(Projectile p, RoomWorld world)
        {
            p.X += p.Weapon.speedX * p.Dir;
            float cx = (int)p.X & ~7, cy = p.Y + 4;
            if (Solid(world, cx, cy) || Solid(world, cx + 8 * p.Dir, cy)) p.Alive = false;
        }

        // Fuego rodante (caja 8 × 8): rodea la plataforma a 4 px/tick (por encima, baja por el lateral, por debajo, sube
        // por el otro); muere si algo le cierra el paso de frente y, tras una vuelta, al cruzar la x en la que nació.
        void StepWrap(Projectile p, RoomWorld world)
        {
            for (int i = 0; i < p.Weapon.speedX && p.Alive; i++)
            {
                if (EdgeSolid(world, p, p.Mx, p.My)) { p.Alive = false; return; }
                float before = p.X;
                p.X += p.Mx; p.Y += p.My;
                if (p.Turns >= 4 && p.Sy == 1 && p.Mx == p.Dir && (before - p.SpawnX) * (p.X - p.SpawnX) <= 0) { p.Alive = false; return; }
                if (!EdgeSolid(world, p, p.Sx, p.Sy))
                {
                    // Dobla la esquina: la nueva superficie es la que acaba de dejar atrás.
                    int nmx = p.Sx, nmy = p.Sy;
                    p.Sx = -p.Mx; p.Sy = -p.My;
                    p.Mx = nmx; p.My = nmy;
                    p.Turns++;
                }
            }
        }

        bool EdgeSolid(RoomWorld world, Projectile p, int dx, int dy)
        {
            if (dx != 0)
            {
                float x = dx > 0 ? p.X + 8 : p.X - 1;
                return Solid(world, x, p.Y) || Solid(world, x, p.Y + 7);
            }
            float y = dy > 0 ? p.Y + 8 : p.Y - 1;
            return Solid(world, p.X, y) || Solid(world, p.X + 7, y);
        }

        // Fuego: el primer tick se asienta 4 px; luego 4 px/tick por el suelo y, sin suelo, cae recto a 4 px/tick.
        void StepFire(Projectile p, RoomWorld world)
        {
            if (!p.Settled) { p.Settled = true; p.Y += 4; return; }
            bool floor = Solid(world, p.X, p.Y + 4) || Solid(world, p.X + 8, p.Y + 4);
            if (!floor)
            {
                p.Y += p.Weapon.fallSpeed;
                if (Solid(world, p.X, p.Y + 4) || Solid(world, p.X + 8, p.Y + 4))
                    p.Y = (((int)p.Y + 4) & ~7) - 4;
                return;
            }
            if (Solid(world, p.Dir > 0 ? p.X + 8 : p.X - 1, p.Y)) { p.Alive = false; return; }
            p.X += p.Weapon.speedX * p.Dir;
        }

        // Mina: se arma cuando el jugador se aparta; estalla al tocarla un enemigo o el jugador y dura 70 ticks.
        void StepMine(Projectile p, PlayerMotor m, EnemySystem enemies, ICollection<int> items)
        {
            var mi = data.mine;
            if (p.Exploding)
            {
                var (x0, y0, x1, y1) = Box(p);
                // Daña cada 32 ticks (E604 múltiplo de 32) a los enemigos que alcanza.
                if (enemies.Context.Tick % p.Weapon.rehitPeriodTicks == 0)
                    foreach (var e in enemies.Overlapping(x0, y0, x1, y1))
                    {
                        if (e is Boss boss)
                        {
                            // Explosión de mina: 2 por golpe a los jefes de la máscara. [C] Por un desfase de la tabla
                            // b03:BC33, la explosión daña a Galious sin la Cross.
                            if (boss.Vulnerable(5) && (boss.CanBeHitNow || boss is Galious && boss.Visible)) enemies.HitBoss(boss, 2);
                        }
                        else if (e.Hittable && !e.Invulnerable)
                            enemies.Damage(e, p.Weapon.damage + (items.Contains(ItemSaber) ? p.Weapon.saberBonus : 0));
                    }
                if (!p.HurtPlayer && TouchesPlayer(mi.playerBox, m, x0, y0, x1, y1))
                {
                    p.HurtPlayer = true;
                    PlayerHit?.Invoke(mi.playerDamage, m.X < p.X + 16 ? -1 : 1);
                }
                if (--p.ExplosionTicks <= 0) p.Alive = false;
                return;
            }
            float dy = m.Y - p.Y, dx = m.X - p.X;
            if (!p.Armed)
            {
                if (!(dy >= 0 && dy < mi.armDy && dx >= mi.armDxMin && dx <= mi.armDxMax)) p.Armed = true;
                return;
            }
            var (bx0, by0, bx1, by1) = Box(p);
            bool enemy = enemies.Overlapping(bx0, by0, bx1, by1).Any(e => e.Harmful || e.Hittable);
            if (enemy || TouchesPlayer(mi.playerBox, m, bx0, by0, bx1, by1))
            {
                p.Exploding = true;
                p.X += mi.explosionShiftX;
                p.Y += mi.explosionShiftY;
                p.ExplosionTicks = mi.explosionTicks;
                Exploded?.Invoke(p);
            }
        }

        static bool TouchesPlayer(WeaponBox b, PlayerMotor m, float x0, float y0, float x1, float y1)
        {
            float px0 = m.X + b.dx, py0 = m.Y + b.dy, px1 = px0 + b.w + 1, py1 = py0 + b.h + 1;
            return px0 < x1 && px1 > x0 && py0 < y1 && py1 > y0;
        }

        // Piedras y enemigos. La cerámica atraviesa enemigos (vuelve a dañar al mismo cada 16 ticks); las piedras la paran.
        void HitThings(Projectile p, PlayerMotor m, RoomWorld world, EnemySystem enemies, Rocks rocks, ICollection<int> items)
        {
            var w = p.Weapon;
            var (x0, y0, x1, y1) = Box(p);
            int rockDamage = m.Character == Character.Popolon || items.Contains(ItemRing) ? w.rockDamagePopolon : w.rockDamageAphrodite;
            if (rocks != null && rocks.StrikeRect(Room, world, (int)x0, (int)x1 - 1, (int)y0, (int)y1 - 1, rockDamage))
            {
                p.Alive = false;
                return;
            }
            int damage = w.damage + (items.Contains(ItemSaber) ? w.saberBonus : 0);
            foreach (var e in enemies.Overlapping(x0, y0, x1, y1))
            {
                if (e is Boss boss)
                {
                    // Jefes: solo las armas de su máscara (b03:BD0A) y si su prueba de choque lo deja; 1 por golpe y
                    // el Saber no suma. La flecha y los fuegos desaparecen aunque no valga el arma (sonido 0x1D).
                    if (!boss.CanBeHitNow) continue;
                    bool first = p.Touched.Add(boss);
                    bool counts = !w.piercing || first || enemies.Context.Tick % w.rehitPeriodTicks == 0;
                    if (counts && boss.Vulnerable(w.id)) enemies.HitBoss(boss, 1);
                    if (!w.piercing) { p.Alive = false; return; }
                    continue;
                }
                if (!e.Hittable) continue;            // sin el bit 5: no se le puede dar
                if (w.piercing)
                {
                    bool first = p.Touched.Add(e);
                    if (!e.Invulnerable && (first || enemies.Context.Tick % w.rehitPeriodTicks == 0))
                        enemies.Damage(e, damage);
                    continue;
                }
                if (!e.Invulnerable) enemies.Damage(e, damage);
                p.Alive = false;                       // la flecha y los fuegos desaparecen al primer enemigo
                return;
            }
        }

        // Lupa (arma 6): M delante de la lápida del mundo (y − 16 − y_lápida y x − 8 − x_lápida en −16 … 16).
        public Tombstone ReadTombstone(PlayerMotor m, int e041)
        {
            var g = data.magnifier;
            foreach (var t in g.tombstones)
            {
                if (t.e041 != e041 || t.room != m.Room.id || t.kind == 0) continue;
                int dy = m.Y + g.playerDy - t.y, dx = m.X + g.playerDx - t.x;
                if (dy >= g.dyMin && dy <= g.dyMax && dx >= g.dxMin && dx <= g.dxMax) return t;
            }
            return null;
        }
    }
}
