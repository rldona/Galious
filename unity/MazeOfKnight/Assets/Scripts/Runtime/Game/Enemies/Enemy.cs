using System;
using System.Collections.Generic;
using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Lo que un enemigo puede consultar en su tick (30 por segundo, como la IA del original).
    public class EnemyContext
    {
        public RoomWorld World;
        public PlayerMotor Player;
        public Random Random;
        public int Tick;                 // contador global de ticks (E604 en el original)
        public int E041 = 1;             // 1 castillo, n + 1 mundo n
        public int PlayerDy;             // lo que se ha movido el jugador en y este tick (0x2F copia su vy)
        public IReadOnlyList<Enemy> Enemies;
        public Func<int, float, float, Enemy> SpawnChild;   // tipo, x, y → el hijo (o null si no hay hueco)
        public IReadOnlyList<BoxRect> Boxes = Array.Empty<BoxRect>();   // tabla de cajas b14:9006 (vacía = cajas físicas)

        // Variables de sala compartidas entre enemigos (en el original, globales que se reinician al entrar).
        public int FusionLevel;          // E62F: nivel de los murciélagos 0x19 tras fusionarse
        public int PairCounter;          // E62C: creados de 0x19, cada dos forman pareja
        public bool Fusing;              // E62D: hay una pareja de 0x19 lanzándose una contra otra
        public int GroupCounter;         // E637: grupos cabeza 0x2C + cola 0x64
        public readonly int[] GroupState = new int[16];   // E638..
        public int OrbCounter, OrbLast = 8;               // E633 / E634: orbes 0x36
        public int HeadCounter, HeadPhase, HeadShooter;   // E62A / E62B / E629: cabezas 0x69
        public bool SoulsCalled;         // E63D: las almas 0x4E ya se reunieron una vez
        public bool WaterJetDone;        // E628: el chorro 0x60 terminó

        // Entero en [min, max], ambos incluidos.
        public int Rand(int min, int max) => Random.Next(min, max + 1);
        public bool Coin() => Random.Next(2) == 1;

        // Jugador en el sistema del original: E505 = y de los pies, E507 = x del centro.
        public int Px => Player.X;
        public int Py => Player.Y;

        public int AliveCount
        {
            get { int n = 0; foreach (var e in Enemies) if (e.Alive) n++; return n; }
        }

        public void ResetRoom()
        {
            Tick = 0;
            FusionLevel = PairCounter = GroupCounter = OrbCounter = HeadCounter = HeadPhase = HeadShooter = 0;
            OrbLast = 8;
            Fusing = SoulsCalled = WaterJetDone = false;
            Array.Clear(GroupState, 0, GroupState.Length);
        }
    }

    // Un enemigo, con la posición en píxeles del original: (X, Y) es la esquina superior izquierda de su caja
    // (la del hueco E800 + 0x20·n: +7 x, +5 y). Velocidades en px/tick (docs/enemy_ai.md, "px/f" = por tick de IA).
    public abstract class Enemy
    {
        public int Type;
        public RoomModel Room;
        public float X, Y, Vx, Vy;
        public int Width = 16, Height = 16;
        public int Hp, Damage;
        public bool Invulnerable;        // vida FE/FF en el descriptor
        public bool Visible = true;      // IX+2 bit 7
        public bool Harmful = true;      // IX+2 bit 6 [I]: hace daño al tocarlo
        public bool Hittable = true;     // IX+2 bit 5 [I]: la espada lo alcanza
        public bool Alive = true;
        public bool FacingLeft;
        public float Grow = 1f;          // parte visible de la caja (pinchos y brotes que crecen), de abajo arriba
        public int Param;                // B del creador (E624): dirección, pareja…
        public bool Marked;              // IX+2 bit 3: cuenta para el evento de la sala
        public PeriodicJson Generator;   // generador periódico que lo creó (null si no)
        public bool DiesOnTouch;         // los proyectiles desaparecen al dar al jugador [I]

        // Escudos (b03:B6C1 espada, b03:B54D armas; docs/items_enemies.md, "Espada", y docs/player.md, "Escudos") [C]:
        // IX+2 bit 2: la espada hace «clinc» sin daño y los proyectiles rebotan. IX+2 bit 1: lo mismo solo si el lado
        // del enemigo (bit 0 = FacingLeft) no coincide con la orientación del ataque (E503 o la del proyectil); es
        // decir, solo se le da por la espalda.
        public bool ShieldAll;           // IX+2 bit 2 (0x06, 0x36, 0x69; 0x12 mientras está oculto o emergiendo)
        public bool ShieldSide;          // IX+2 bit 1 (0x0A, 0x25)

        // ¿Para el escudo un ataque que mira a la izquierda (attackerLeft) o a la derecha?
        public bool Deflects(bool attackerLeft) => ShieldAll || ShieldSide && FacingLeft != attackerLeft;

        // Experiencia al morir si no es la del catálogo (b03:BF27): el 0x19 da 1, 2, 4, 4 según su tamaño (IX+1B).
        public virtual int? ExperienceOverride => null;

        // Atraviesa suelos ahora mismo (escaleras del caballero, saltos entre pisos): las pruebas no lo cuentan como
        // metido en un muro.
        public virtual bool ThroughFloors => false;

        // No se borra al salir de la pantalla (b06:B452 no se aplica a los jefes; las bolas de Galious se ocultan).
        public virtual bool KeepOffScreen => false;

        public virtual void Init(EnemyContext ctx) { }
        public abstract void Tick(EnemyContext ctx);

        // Le alcanza un espadazo: true si cuenta (resta vida en el sistema).
        public virtual bool OnHit(EnemyContext ctx) => !Invulnerable && Visible && Hittable;

        // Golpeado sin morir (IX+1C en el original [I]).
        public virtual void Hurt(EnemyContext ctx) { }

        // Al morir por la espada (p. ej. la mosca 0x49 se divide).
        public virtual void Die(EnemyContext ctx) { }

        // Por encima de la sala (el marcador) no hay sólidos para los enemigos: los generados en y = 0 caen por ahí.
        protected bool Solid(EnemyContext ctx, float px, float py) =>
            py >= P.RoomTopPx && ctx.World.Solid(Room, (int)Math.Floor(px), (int)Math.Floor(py));
        protected bool Ladder(EnemyContext ctx, float px, float py) => ctx.World.Ladder(Room, (int)Math.Floor(px), (int)Math.Floor(py));

        protected void Move() { X += Vx; Y += Vy; }

        // Rumbo de 16 direcciones (0 = derecha, 4 = arriba, 8 = izquierda, 12 = abajo), como b04:7E25.
        protected void Heading(int dir, float speed)
        {
            double a = (dir & 15) * Math.PI / 8;
            Vx = (float)Math.Cos(a) * speed;
            Vy = -(float)Math.Sin(a) * speed;
            if (Math.Abs(Vx) < 1e-4f) Vx = 0;
            if (Math.Abs(Vy) < 1e-4f) Vy = 0;
        }

        // Velocidad hacia un punto con módulo s/32 px/tick (B18F con E045 = 0 [I]; el original cuantiza el ángulo).
        protected void AimAt(float tx, float ty, int s)
        {
            float dx = tx - X, dy = ty - Y, d = (float)Math.Sqrt(dx * dx + dy * dy);
            float speed = s / 32f;
            if (d < 0.01f) { Vx = 0; Vy = speed; return; }
            Vx = dx / d * speed;
            Vy = dy / d * speed;
        }

        // Hacia el jugador: B18F apunta a (jugador_y − 16, jugador_x − 8), la esquina de un 16×16 centrado en él.
        protected void AimAtPlayer(EnemyContext ctx, int s) => AimAt(ctx.Px - 8, ctx.Py - 16, s);

        // Rebote en los límites de la sala (x 0..240, y 32..176).
        protected void BounceInRoom()
        {
            if (X < 0) { X = 0; Vx = Math.Abs(Vx); }
            if (X > P.RoomWidthPx - Width) { X = P.RoomWidthPx - Width; Vx = -Math.Abs(Vx); }
            if (Y < P.RoomTopPx) { Y = P.RoomTopPx; Vy = Math.Abs(Vy); }
            if (Y > P.RoomBottomPx - Height) { Y = P.RoomBottomPx - Height; Vy = -Math.Abs(Vy); }
        }

        // Rebote en una caja (invierte la componente que sale), como 7DDD/930E.
        protected bool BounceIn(float x0, float x1, float y0, float y1)
        {
            bool hit = false;
            if (Vy > 0 && Y >= y1 || Vy < 0 && Y < y0) { Vy = -Vy; hit = true; }
            if (Vx > 0 && X >= x1 || Vx < 0 && X < x0) { Vx = -Vx; hit = true; }
            return hit;
        }

        // ---------- andar por plataformas (6BEE, 6C04, 6C25) ----------

        protected float Dir => FacingLeft ? -1 : 1;

        protected void Walk(float speed) => Vx = FacingLeft ? -speed : speed;

        // Columna que pisará en el paso siguiente (x − 1 o x + 16 a 1 px/tick; más lejos si va más rápido).
        float Lead => FacingLeft ? X - Math.Max(1, Math.Abs(Vx)) : X + Width - 1 + Math.Max(1, Math.Abs(Vx));

        // Hay suelo delante: tile en (x + 16 o x − 1, y + 16).
        protected bool FloorAhead(EnemyContext ctx) => Solid(ctx, Lead, Y + Height);

        // Pared delante (tile en y + 8) o borde de pantalla (x < 8 a la izquierda, x ≥ 0xF0 a la derecha).
        protected bool WallAhead(EnemyContext ctx) =>
            Solid(ctx, Lead, Y + Height - 8) || (FacingLeft ? X - Math.Abs(Vx) < 8 : X + Math.Abs(Vx) >= 0xF0 - (Width - 16));

        protected bool Blocked(EnemyContext ctx) => !FloorAhead(ctx) || WallAhead(ctx);

        protected void Turn() { FacingLeft = !FacingLeft; Vx = -Vx; }

        // Un paso de patrulla: gira ante un hueco, una pared o el borde; si no, avanza. true si giró.
        protected bool Patrol(EnemyContext ctx, float speed)
        {
            if (Blocked(ctx)) { FacingLeft = !FacingLeft; Walk(speed); return true; }
            Walk(speed);
            X += Vx;
            return false;
        }

        // El jugador está en su piso: |jugador_y − 16 − y| ≤ 4.
        protected bool SameLevel(EnemyContext ctx) => Math.Abs(ctx.Py - 16 - Y) <= 4;

        protected bool PlayerLeft(EnemyContext ctx) => ctx.Px < X + 8;

        protected void FacePlayer(EnemyContext ctx) => FacingLeft = PlayerLeft(ctx);

        protected bool BodyFree(EnemyContext ctx, float x, float y) =>
            !Solid(ctx, x, y) && !Solid(ctx, x + 8, y) && !Solid(ctx, x, y + 8) && !Solid(ctx, x + 8, y + 8);

        // Sitio libre sobre una plataforma (96AB): fila al azar de {0x28 … 0xA8} (las centrales salen más) y
        // columna de {0x30 … 0xB0}. Un intento; false si no vale.
        protected bool TryPlaceOnPlatform(EnemyContext ctx)
        {
            int[] rows = { 0x28, 0x48, 0x68, 0x88, 0xA8 };
            int y = rows[ctx.Rand(0, 3) + ctx.Rand(0, 1)];
            int x = 0x30 + 0x20 * ctx.Rand(0, 4);
            if (!Solid(ctx, x, y + 16) || !Solid(ctx, x + 8, y + 16) || !BodyFree(ctx, x, y))
                return false;
            X = x; Y = y;
            return true;
        }

        // Caída con gravedad hasta posarse en un suelo (y alineada a 8 px). true al aterrizar. Mientras sube no
        // mira nada (como 0x0A, 0x1F, 0x24…); bajando, píxel a píxel.
        float fallAcc;
        protected bool Fall(EnemyContext ctx, float gravity, float maxFall = 6)
        {
            Vy = Math.Min(maxFall, Vy + gravity);
            // [I] Choca con las paredes de lado (el original no lo hace en todos los tipos; así no se mete en ellas).
            float lead = Vx < 0 ? X + Vx : X + Vx + Width - 1;
            if (Vx != 0 && (Solid(ctx, lead, Y + 1) || Solid(ctx, lead, Y + Height / 2f) || Solid(ctx, lead, Y + Height - 1)))
                Vx = 0;
            X += Vx;
            if (X < 0) X = 0;
            if (X > P.RoomWidthPx - Width) X = P.RoomWidthPx - Width;
            if (Vy <= 0)
            {
                // Subiendo, píxel a píxel hasta el techo.
                fallAcc = 0;
                for (float left = -Vy; left > 0; left -= 1)
                {
                    float d = Math.Min(1, left);
                    if (Y - d < P.RoomTopPx || Solid(ctx, X + 1, Y - d) || Solid(ctx, X + Width - 2, Y - d)) { Vy = 0; break; }
                    Y -= d;
                }
                return false;
            }
            fallAcc += Vy;
            int n = (int)fallAcc;
            fallAcc -= n;
            Y = (float)Math.Round(Y);
            for (int i = 0; i < n; i++)
            {
                // [I] Se posa en cualquier sólido bajo la caja (el original mira x + 4 y x + 12), para no caer
                // pegado a una pared y quedar metido en ella.
                if (((int)Y + Height) % 8 == 0 && (Solid(ctx, X + 1, Y + Height) || Solid(ctx, X + Width / 2f, Y + Height) || Solid(ctx, X + Width - 2, Y + Height)))
                {
                    Vy = 0; fallAcc = 0;
                    return true;
                }
                Y += 1;
            }
            return false;
        }

        // Animación (B0D6): cuenta un temporizador y cambia de fotograma al llegar a 0.
        public int Frame;

        // Caja de colisión de la tabla (null = la caja física Width × Height). LeftBox: la del sprite mirando a la
        // izquierda cuando es otra (0x25 y 0x0A: 4E a la derecha, 4F a la izquierda).
        public BoxRect HitBox, LeftBox;

        // Caja que toca al jugador y a la espada, semiabierta [x0, x1) × [y0, y1). Los que crecen (pinchos, brotes…)
        // muestran solo una parte, desde el suelo o desde el techo.
        public (float x0, float y0, float x1, float y1) Box()
        {
            var b = FacingLeft && LeftBox != null ? LeftBox : HitBox;
            float x0 = b != null ? X + b.dx : X, y0 = b != null ? Y + b.dy : Y;
            float w = b != null ? b.w + 1 : Width, hFull = b != null ? b.h + 1 : Height;
            float h = hFull * Grow;
            return GrowsDown ? (x0, y0, x0 + w, y0 + h) : (x0, y0 + hFull - h, x0 + w, y0 + hFull);
        }

        public bool GrowsDown;           // cuelga del techo (crece hacia abajo)

        // Índice de la caja actual en la tabla (IX+11), −1 si es la física o una calculada.
        public int BoxIndex = -1;

        // Cambia la caja por la de la tabla b14:9006 (IX+11; docs/items_enemies.md, "Cajas por tipo"). left: la caja
        // mirando a la izquierda cuando es otra (4E/4F). Sin tabla (pruebas) se queda la caja física y da false.
        protected bool SetBox(EnemyContext ctx, int index, int left = -1)
        {
            var t = ctx.Boxes;
            if (t == null || index < 0 || index >= t.Count) return false;
            HitBox = t[index];
            LeftBox = left >= 0 && left < t.Count ? t[left] : null;
            BoxIndex = index;
            Grow = 1;
            return true;
        }

        // Caja de la tabla o, sin tabla, la física recortada a la fracción grow (lo que se hacía antes de tener cajas).
        protected void SetBoxOrGrow(EnemyContext ctx, int index, float grow)
        {
            if (!SetBox(ctx, index)) Grow = grow;
        }

        // Caja que no sale de la tabla (b03:B23E/B4F4: 0x57, 0x60, 0x62), con la convención de la tabla (bordes
        // incluidos: h + 1 × w + 1 px).
        protected void SetBox(int dy, int dx, int h, int w)
        {
            HitBox = new BoxRect { dy = dy, dx = dx, h = h, w = w };
            LeftBox = null;
            BoxIndex = -1;
        }

        // ---------- sondeo de salto a otro piso (0x0A, 0x1F, 0x25, 0x2D) ----------

        // b04:9A67: primero un hueco libre en el piso de destino (tabla 9ADF: subir, (y − 16, x + 32 | x − 16); bajar,
        // (y + 16, x + 40 | x − 24)) y después la plataforma de destino (9C68/9CA3: subir, suelo en (y − 16, x + 64 |
        // x − 48); bajar, suelo en y + 48), con x' en [0x10, 0xF0) e y' en [0x28, 0xB9) [C] (docs/enemy_ai.md, 0x0A,
        // 0x1F y 0x25). [I] Sin datos en las fichas: la x de la plataforma al bajar (se usa la del hueco), y las
        // comprobaciones extra de 9D1C/9D2D, que aquí se reducen a que el cuerpo quepa donde cae.
        protected bool ProbeLevelJump(EnemyContext ctx, int kind)
        {
            if (kind != 1 && kind != 2) return false;
            bool up = kind == 1;
            float gx = X + (FacingLeft ? (up ? -16 : -24) : (up ? 32 : 40));
            float gy = up ? Y - 16 : Y + 16;
            if (gx < 0 || gx >= P.RoomWidthPx || Solid(ctx, gx, gy)) return false;
            float px = up ? X + (FacingLeft ? -48 : 64) : gx;
            float floor = up ? Y - 16 : Y + 48;
            float ty = floor - 16;
            if (px < 0x10 || px >= 0xF0 || ty < 0x28 || ty >= 0xB9) return false;
            return Solid(ctx, px, floor) && BodyFree(ctx, px, ty);
        }

        // Cajas contra el jugador y contra la espada y las armas (los jefes tienen varias y distintas). Semiabiertas.
        public virtual IEnumerable<(float x0, float y0, float x1, float y1)> ContactRects() { yield return Box(); }
        public virtual IEnumerable<(float x0, float y0, float x1, float y1)> WeaponRects() { yield return Box(); }
        // Rectángulo que se dibuja (provisional, sin gráficos).
        public virtual (float x0, float y0, float x1, float y1) DrawRect() => Box();
    }
}
