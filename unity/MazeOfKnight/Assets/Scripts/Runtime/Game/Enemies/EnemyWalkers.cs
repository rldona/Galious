using System;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Enemigos que andan por las plataformas, a partir de sus fichas (docs/enemy_ai.md). Tiempos en ticks de IA.

    // 0x01: embestidor. Patrulla a 1,5 px/tick; si el jugador está en su piso (o uno más abajo) se para 30 ticks,
    // embiste hasta la x donde estaba el jugador y, si estaba en su piso y hay techo sólido encima, ataca y se para
    // 15 ticks. [I] El golpe directo (b03:B389) se deja en el daño por contacto.
    public class Charger : Enemy
    {
        const float Speed = 1.5f;
        int phase, timer, level;
        float targetX;

        public override void Init(EnemyContext ctx) { FacingLeft = ctx.Coin(); Walk(Speed); }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    level = Math.Abs(ctx.Py - 16 - Y) < 1 ? 1 : Math.Abs(ctx.Py - 32 - Y) < 1 ? 2 : 0;
                    if (level != 0) { phase = 1; timer = 30; break; }
                    Patrol(ctx, Speed);
                    break;
                case 1:
                    if (--timer > 0) break;
                    targetX = ctx.Px - 8;
                    FacePlayer(ctx);
                    phase = 2;
                    break;
                case 2:
                {
                    bool turned = Patrol(ctx, Speed);
                    bool passed = FacingLeft ? X <= targetX : X >= targetX;
                    if (!passed && !turned) break;
                    if (level == 1 && Solid(ctx, FacingLeft ? X + 4 : X + 12, Y - 16)) { phase = 3; timer = 15; }
                    else phase = 0;
                    break;
                }
                case 3:
                    if (--timer <= 0) phase = 0;
                    break;
            }
        }
    }

    // 0x05: serpiente. Patrulla a 1 px/tick; cada 120 ticks se para 10 y escupe un 0x1A a ±4 px/tick. Si el
    // jugador está en su piso, se queda mirándolo 120 ticks y escupe 3 veces (en 96, 64 y 32); luego anda 60 ticks
    // sin mirarlo.
    public class Snake : Enemy
    {
        int phase, timer = 120;

        public override void Init(EnemyContext ctx) { FacingLeft = ctx.Coin(); Walk(1); }

        void Spit(EnemyContext ctx)
        {
            var c = ctx.SpawnChild(0x1A, X, Y + 4);
            if (c != null) { c.Vx = FacingLeft ? -4 : 4; c.FacingLeft = FacingLeft; }
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    if (SameLevel(ctx)) { phase = 2; timer = 120; break; }
                    Patrol(ctx, 1);
                    if (--timer <= 0) { phase = 1; timer = 10; }
                    break;
                case 1:
                    if (--timer <= 0) { Spit(ctx); phase = 0; timer = 120; }
                    break;
                case 2:
                    FacePlayer(ctx);
                    --timer;
                    if (timer > 0 && (timer & 0x1F) == 0) Spit(ctx);
                    if (timer <= 0) { phase = 3; timer = 60; }
                    break;
                case 3:
                    Patrol(ctx, 1);
                    if (--timer <= 0) { phase = 0; timer = 120; }
                    break;
            }
        }
    }

    // 0x07: oruga saltarina. Anda a 0,5 px/tick; al borde de una plataforma salta a otra a 48 px (o 45 hacia la
    // izquierda) a su misma altura si hay suelo y sitio: vy −2,125, vx ±1,375, gravedad 0,133. Si no, gira.
    public class Hopper : Enemy
    {
        bool air;
        float home;

        public override void Init(EnemyContext ctx) { FacingLeft = ctx.Coin(); Walk(0.5f); home = Y; }

        public override void Tick(EnemyContext ctx)
        {
            if (air)
            {
                Vy += 0.1328f;
                if (Vy < 0 && (Y + Vy < P.RoomTopPx || Solid(ctx, X + 4, Y + Vy) || Solid(ctx, X + 12, Y + Vy))) Vy = 0;
                float lead = Vx < 0 ? X + Vx : X + 15 + Vx;
                if (Solid(ctx, lead, Y + 1) || Solid(ctx, lead, Y + 8) || Solid(ctx, lead, Y + 15)) Vx = 0;
                X += Vx; Y += Vy;
                if (Y > home) { Y = home; Vy = 0; air = false; Walk(0.5f); }
                return;
            }
            if (WallAhead(ctx)) { Turn(); return; }
            if (!FloorAhead(ctx))
            {
                bool can = FacingLeft ? X >= 0x38 : X < 0xB8;
                float tx = FacingLeft ? X - 45 : X + 48;
                if (can && Solid(ctx, tx, Y + 16) && Solid(ctx, tx + 8, Y + 16) && BodyFree(ctx, tx, Y))
                {
                    air = true; Vy = -2.125f; Vx = FacingLeft ? -1.375f : 1.375f;
                }
                else Turn();
                return;
            }
            X += Vx;
        }
    }

    // Base de los que saltan entre plataformas (0x0A caballero, 0x1F corredor, 0x2D soldado): saltan huecos a su
    // altura y, al cabo de un rato, buscan subir o bajar un piso (32 px) comprobando antes dónde caen.
    // [I] Sondeos simplificados (9A67, 9CA3, 9C68 sin seguir del todo): un sitio vale si hay suelo bajo los pies y el
    // cuerpo cabe.
    public abstract class PlatformJumper : Enemy
    {
        protected int phase, timer, turns, jumpKind;
        protected float walkSpeed, gravity;

        protected bool CanLand(EnemyContext ctx, float dx, float dy)
        {
            float tx = X + (FacingLeft ? -dx : dx), ty = Y + dy;
            if (tx < 0x10 || tx >= 0xF0 || ty < 0x28 || ty >= 0xB9) return false;
            return (Solid(ctx, tx + 4, ty + 16) || Solid(ctx, tx + 12, ty + 16)) && BodyFree(ctx, tx, ty);
        }

        protected abstract (float vy, float vx) JumpSpeed(int kind);

        protected void Jump(int kind)
        {
            var (vy, vx) = JumpSpeed(kind);
            Vy = vy; Vx = FacingLeft ? -vx : vx;
            Y = (float)Math.Round(Y);
            phase = 2;
        }

        // Ante un obstáculo: salta el hueco si hay dónde caer a gapDx; si no, gira.
        protected void Obstacle(EnemyContext ctx, float gapDx, float gapDxLeft)
        {
            bool headFree = !Solid(ctx, FacingLeft ? X - 1 : X + Width, Y);
            if (FloorAheadMissing(ctx) && headFree && CanLand(ctx, FacingLeft ? gapDxLeft : gapDx, 0)) { Jump(0); return; }
            FacingLeft = !FacingLeft;
            Walk(walkSpeed);
            turns--;
        }

        bool FloorAheadMissing(EnemyContext ctx) => !FloorAhead(ctx) && !WallAhead(ctx);

        // Probar subir (1) o bajar (2) un piso.
        protected bool TryLevelJump(EnemyContext ctx, int kind, float upDx, float upDxLeft, float downDx, float downDxLeft)
        {
            if (kind == 1 && CanLand(ctx, FacingLeft ? upDxLeft : upDx, -32)) { Jump(1); return true; }
            if (kind == 2 && CanLand(ctx, FacingLeft ? downDxLeft : downDx, 32)) { Jump(2); return true; }
            return false;
        }

        protected bool Air(EnemyContext ctx)
        {
            if (!Fall(ctx, gravity)) return false;
            phase = 0;
            Walk(walkSpeed);
            return true;
        }

        // Paso de andar; true si hay obstáculo.
        protected bool Step(EnemyContext ctx)
        {
            if (!Solid(ctx, X + 4, Y + 16) && !Solid(ctx, X + 12, Y + 16)) { Vx = 0; Vy = 0; phase = 2; return false; }
            if (Blocked(ctx)) return true;
            Walk(walkSpeed);
            X += Vx;
            return false;
        }
    }

    // 0x0A: caballero del castillo. Anda a 0,4375 px/tick; salta huecos (vy −2, vx ±2, gravedad 0,3125, caída
    // máx. 6); cada 0..127 ticks busca el piso del jugador: salta arriba (vy −5) o abajo, o usa una escalera a
    // 1 px/tick. Empieza alejándose del jugador.
    public class Knight : PlatformJumper
    {
        bool ladderUp;

        public override bool ThroughFloors => phase == 3;

        public override void Init(EnemyContext ctx)
        {
            walkSpeed = 0.4375f; gravity = 0.3125f;
            timer = ctx.Rand(0, 127); turns = 5;
            FacingLeft = !PlayerLeft(ctx);
            Walk(walkSpeed);
        }

        protected override (float, float) JumpSpeed(int kind) => (kind == 1 ? -5f : -2f, 2f);

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                case 1:
                    if (phase == 1 && ctx.Coin())
                    {
                        if (TryLevelJump(ctx, jumpKind, 32, 16, 40, 24) || TryLadder(ctx)) break;
                    }
                    if (Step(ctx)) Obstacle(ctx, 48, 32);
                    if (phase == 0 && (--timer <= 0 || turns <= 0))
                    {
                        jumpKind = timer <= 0 ? (ctx.Py < Y + 12 ? 1 : ctx.Py > Y + 20 ? 2 : 0) : ctx.Rand(0, 2);
                        phase = 1; turns = 6;
                    }
                    else if (phase == 1 && turns < 0)
                    {
                        phase = 0; timer = ctx.Rand(0, 127); turns = 5;
                    }
                    break;
                case 2:
                    if (Air(ctx)) { timer = ctx.Rand(0, 127); turns = 5; }
                    break;
                case 3:
                    Climb(ctx);
                    break;
            }
        }

        bool TryLadder(EnemyContext ctx)
        {
            if (jumpKind == 0) return false;
            ladderUp = jumpKind == 1;
            float probe = ladderUp ? Y + 15 : Y + 24;
            foreach (float cx in new[] { X + 4, X + 12 })
            {
                if (!Ladder(ctx, cx, probe)) continue;
                // Centrado en la escalera (2 tiles de ancho).
                int col = (int)Math.Floor(cx / 8);
                if (Ladder(ctx, col * 8 - 8, probe)) col--;
                X = col * 8;
                if (!ladderUp) Y += 9;
                Vx = 0; Vy = ladderUp ? -1 : 1;
                phase = 3;
                return true;
            }
            return false;
        }

        void Climb(EnemyContext ctx)
        {
            Y += Vy;
            if (Y < 0x20 || Y > 0xAF) { Vy = -Vy; return; }
            if (((int)Y & 7) != 0) return;
            float cx = X + 8;
            // Subiendo atraviesa los suelos que cruza la escalera y sale en cuanto la cabeza queda libre; bajando, se
            // para en un suelo que no siga con escalera debajo.
            bool onTop = Vy < 0 && !Ladder(ctx, cx, Y + 15) && !Solid(ctx, cx, Y + 15) && !Solid(ctx, cx, Y);
            bool atBottom = Vy > 0 && Solid(ctx, cx, Y + 16) && !Ladder(ctx, cx, Y + 16) && !Ladder(ctx, cx, Y + 24);
            if (onTop || atBottom)
            {
                Vy = 0; phase = 0; timer = ctx.Rand(0, 127); turns = 5;
                FacePlayer(ctx);
                Walk(walkSpeed);
            }
        }
    }

    // 0x0B: dardo. Va a ±4 px/tick sin mirar el suelo (solo paredes y bordes) durante 80..127 ticks y se para
    // 0..47 (0 = 256).
    public class Dart : Enemy
    {
        int run, rest;
        bool resting;

        public override void Init(EnemyContext ctx) { FacingLeft = ctx.Coin(); Walk(4); Timers(ctx); }

        void Timers(EnemyContext ctx) { rest = ctx.Rand(0, 47); run = rest + 80; if (rest == 0) rest = 256; }

        public override void Tick(EnemyContext ctx)
        {
            if (resting)
            {
                if (--rest <= 0) { resting = false; Timers(ctx); }
                return;
            }
            if (WallAhead(ctx)) Turn();
            else X += Vx;
            if (--run <= 0) resting = true;
        }
    }

    // 0x0C hombre-llama y 0x13 espectro (misma rutina): aparece en 4 pasos de 10 ticks, anda a 1 px/tick 256 ticks
    // disparando (0x56; el 0x13, 0x44) cada 48..127 ticks si mira al jugador, se desvanece, vuela al azar a 2 px/tick
    // (16 rumbos, cambia cada 16..31) y, pasados 48 ticks, se posa en el primer suelo libre que encuentra.
    public class FlameMan : Enemy
    {
        int phase, timer, shot, turn, step;
        bool flying;

        public override void Init(EnemyContext ctx) { timer = 10; shot = ctx.Rand(0, 0x4F) + 0x30; Harmful = true; }

        public override void Tick(EnemyContext ctx)
        {
            if (flying) { Fly(ctx); return; }
            switch (phase)
            {
                case 0:
                case 2:
                    Grow = phase == 0 ? (step + 1) / 4f : 1 - step / 4f;
                    if (--timer > 0) break;
                    timer = 10;
                    if (++step < 4) break;
                    step = 0;
                    if (phase == 0) { phase = 1; FacingLeft = ctx.Coin(); Walk(1); timer = 256; Grow = 1; }
                    else { flying = true; timer = 48; Heading(ctx.Rand(0, 15), 2); turn = ctx.Rand(16, 31); Grow = 1; }
                    break;
                case 1:
                    Patrol(ctx, 1);
                    if (--shot <= 0)
                    {
                        if (PlayerLeft(ctx) == FacingLeft)
                        {
                            shot = ctx.Rand(0, 0x4F) + 0x30;
                            ctx.SpawnChild(Type == 0x13 ? 0x44 : 0x56, X + 8, Y + 8);
                        }
                        else shot = 1;
                    }
                    if (--timer <= 0) { phase = 2; timer = 10; Vx = 0; }
                    break;
            }
        }

        void Fly(EnemyContext ctx)
        {
            Move();
            BounceIn(8, 0xE8, 0x28, 0xB0);
            if (--turn <= 0) { Heading(ctx.Rand(0, 15), 2); turn = ctx.Rand(16, 31); }
            FacingLeft = Vx < 0;
            if (--timer > 0) return;
            timer = 1;
            float y = (float)Math.Floor((Y + 16) / 8) * 8 - 16;
            if (Math.Abs(y - Y) < 2 && Solid(ctx, X, y + 16) && Solid(ctx, X + 8, y + 16) && BodyFree(ctx, X, y))
            {
                Y = y; Vx = Vy = 0; flying = false; phase = 0; timer = 10; step = 0;
            }
        }
    }

    // 0x0F: bola que crece. Anda a 1 px/tick; cada golpe que recibe sin morir la hace crecer una etapa (hasta 3).
    // [I] Quién activa el crecimiento (IX+1C) no está localizado; se supone el golpe.
    public class GrowingBall : Enemy
    {
        public int Stage;

        public override void Init(EnemyContext ctx) { FacingLeft = ctx.Coin(); Walk(1); }

        public override void Tick(EnemyContext ctx) => Patrol(ctx, 1);

        public override void Hurt(EnemyContext ctx) => Stage = Math.Min(3, Stage + 1);
    }

    // 0x10: acechador. Quieto hasta que el jugador está en su piso; embiste a 2 px/tick hasta un borde o una pared
    // y vuelve a 1 px/tick a su sitio.
    public class Lurker : Enemy
    {
        int phase;
        float home;

        public override void Init(EnemyContext ctx) => home = X;

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    if (!SameLevel(ctx)) break;
                    FacePlayer(ctx); Walk(2); phase = 1;
                    break;
                case 1:
                    if (Blocked(ctx)) { FacingLeft = !FacingLeft; Walk(1); phase = 2; break; }
                    X += Vx;
                    break;
                case 2:
                    Vx = Math.Sign(home - X);
                    X += Vx;
                    if (Math.Abs(X - home) <= 0.5f) { X = home; Vx = 0; phase = 0; }
                    break;
            }
        }
    }

    // 0x11: erizo. Anda a 2 px/tick 120 ticks y descansa 90 + 30. Si el jugador está a menos de 65 px en x se para
    // 60 ticks y, a los 28, le dispara un 0x1D a ±4 px/tick (después 40 ticks sin disparar).
    public class Hedgehog : Enemy
    {
        int phase, timer = 120, cooldown;

        public override void Init(EnemyContext ctx) { FacingLeft = ctx.Coin(); Walk(2); }

        public override void Tick(EnemyContext ctx)
        {
            if (cooldown > 0) cooldown--;
            switch (phase)
            {
                case 0:
                    Patrol(ctx, 2);
                    if (cooldown == 0 && Math.Abs(ctx.Px - 8 - X) < 65) { phase = 3; timer = 60; break; }
                    if (--timer <= 0) { phase = 1; timer = 90; }
                    break;
                case 1:
                    if (--timer <= 0) { phase = 2; timer = 30; }
                    break;
                case 2:
                    if (--timer <= 0) { phase = 0; timer = 120; }
                    break;
                case 3:
                    if (--timer == 32)
                    {
                        cooldown = 40;
                        var c = ctx.SpawnChild(0x1D, X, Y + 8);
                        if (c != null) { c.Vx = PlayerLeft(ctx) ? -4 : 4; c.FacingLeft = c.Vx < 0; }
                    }
                    if (timer <= 0) { phase = 0; timer = 120; }
                    break;
            }
        }
    }

    // 0x12: armadillo. Oculto 96..159 ticks, emerge en 3 pasos de 30, rueda a 1 px/tick (paradas de 30 cada
    // 128..191 con un 0x1E a ±2 px/tick) durante 160..640 ticks y se vuelve a esconder. Para el sentido, el original
    // compara la y del jugador con la x del enemigo (b04:7102) [C].
    public class Armadillo : Enemy
    {
        int phase, timer, step, life, roll;

        public override void Init(EnemyContext ctx) => Hide(ctx);

        void Hide(EnemyContext ctx) { phase = 0; timer = ctx.Rand(96, 159); Visible = Harmful = Hittable = false; Vx = 0; }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    if (--timer > 0) break;
                    phase = 1; timer = 30; step = 0; Visible = true; Grow = 0.33f;
                    break;
                case 1:
                    if (--timer > 0) break;
                    timer = 30;
                    Grow = (++step + 1) / 3f;
                    if (step < 3) break;
                    FacingLeft = ctx.Py < X;
                    Walk(1); life = ctx.Rand(5, 20); roll = ctx.Rand(128, 191);
                    Harmful = Hittable = true; Grow = 1; phase = 2;
                    break;
                case 2:
                    Patrol(ctx, 1);
                    if ((ctx.Tick & 0x1F) == 0 && --life <= 0) { phase = 4; step = 3; timer = 30; Harmful = Hittable = false; break; }
                    if (--roll <= 0) { phase = 3; timer = 30; }
                    break;
                case 3:
                    if (--timer > 0) break;
                    phase = 2; roll = ctx.Rand(128, 191);
                    var c = ctx.SpawnChild(0x1E, X, Y);
                    if (c != null) { c.Vx = FacingLeft ? -2 : 2; c.FacingLeft = FacingLeft; }
                    break;
                case 4:
                    if (--timer > 0) break;
                    timer = 30;
                    Grow = --step / 3f;
                    if (step <= 0) Hide(ctx);
                    break;
            }
        }
    }

    // 0x14: criatura que emerge del suelo. Oculta 128..191 ticks, emerge en 3 pasos de 6, espera 10, pasea a
    // 1 px/tick 128..191 ticks, espera 10 y se hunde en 3 pasos de 6.
    public class Emerger : Enemy
    {
        int phase, timer, step;

        public override void Init(EnemyContext ctx) => Hide(ctx);

        void Hide(EnemyContext ctx) { phase = 0; timer = ctx.Rand(128, 191); Visible = Harmful = Hittable = false; }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    if (--timer > 0) break;
                    phase = 1; timer = 6; step = 0; Visible = true; Grow = 1 / 3f;
                    break;
                case 1:
                    if (--timer > 0) break;
                    timer = 6;
                    Grow = (++step + 1) / 3f;
                    if (step < 3) break;
                    Grow = 1; Harmful = Hittable = true; FacingLeft = ctx.Coin(); phase = 2; timer = 10;
                    break;
                case 2:
                    if (--timer <= 0) { phase = 3; timer = ctx.Rand(128, 191); }
                    break;
                case 3:
                    Patrol(ctx, 1);
                    if (--timer <= 0) { phase = 4; timer = 10; }
                    break;
                case 4:
                    if (--timer <= 0) { phase = 5; timer = 6; step = 3; Harmful = Hittable = false; }
                    break;
                case 5:
                    if (--timer > 0) break;
                    timer = 6;
                    Grow = --step / 3f;
                    if (step <= 0) Hide(ctx);
                    break;
            }
        }
    }

    // 0x17: centinela. Patrulla a 2 px/tick; si el jugador está en su piso se para mirándolo, a los 30 ticks
    // dispara un 0x47 y espera 120 antes de seguir.
    public class Sentinel : Enemy
    {
        int phase, timer;
        bool walkLeft;

        public override void Init(EnemyContext ctx) { FacingLeft = ctx.Coin(); Walk(2); }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    if (SameLevel(ctx)) { walkLeft = FacingLeft; FacePlayer(ctx); phase = 1; timer = 30; break; }
                    Patrol(ctx, 2);
                    break;
                case 1:
                    if (--timer > 0) break;
                    var c = ctx.SpawnChild(0x47, X, Y);
                    if (c != null) c.Param = FacingLeft ? 1 : 0;
                    phase = 2; timer = 120;
                    break;
                case 2:
                    if (--timer <= 0) { phase = 0; FacingLeft = walkLeft; Walk(2); }
                    break;
            }
        }
    }

    // 0x1F: corredor. Corre a 3,625 px/tick, salta huecos (a 44 px, o 30 hacia la izquierda) y cada 0..127 ticks
    // intenta subir (vy −9) o bajar (vy −4) de plataforma. Gravedad 1, caída máx. 6.
    public class Runner : PlatformJumper
    {
        public override void Init(EnemyContext ctx)
        {
            walkSpeed = 3.625f; gravity = 1;
            timer = ctx.Rand(0, 127); turns = 6;
            FacingLeft = ctx.Coin(); Walk(walkSpeed);
        }

        protected override (float, float) JumpSpeed(int kind) => (kind == 1 ? -9f : -4f, kind == 1 ? 3.375f : 3.1875f);

        public override void Tick(EnemyContext ctx)
        {
            if (phase == 2)
            {
                if (Air(ctx)) timer = ctx.Rand(0, 127);
                return;
            }
            if (phase == 1 && TryLevelJump(ctx, jumpKind, 64, 48, 40, 24)) return;
            if (Step(ctx)) { Obstacle(ctx, 44, 30); if (phase == 1 && (turns == 4 || turns == 2)) jumpKind = (jumpKind + 1) % 3; }
            if (phase == 0 && --timer <= 0) { phase = 1; jumpKind = ctx.Rand(0, 2); turns = 6; }
            else if (phase == 1 && turns < 0) { phase = 0; timer = ctx.Rand(0, 127); }
        }
    }

    // 0x20: embestidor invulnerable. Patrulla a 0,25 px/tick; si el jugador está delante, en su piso y a menos de
    // 80 px, acelera a 1 px/tick hasta el siguiente obstáculo.
    public class SlowCharger : Enemy
    {
        float speed = 0.25f;

        public override void Init(EnemyContext ctx) { FacingLeft = ctx.Coin(); Walk(speed); }

        public override void Tick(EnemyContext ctx)
        {
            if (Blocked(ctx)) { speed = 0.25f; FacingLeft = !FacingLeft; Walk(speed); return; }
            float dx = ctx.Px - 8 - X;
            if (Math.Abs(dx) < 80 && (dx < 0) == FacingLeft && SameLevel(ctx)) speed = 1;
            Walk(speed);
            X += Vx;
        }
    }

    // 0x23: estatua (64 de vida, 24 px de alto). Dormida e invisible hasta que el jugador pasa por los dos lados a
    // su altura; entonces avanza hacia él a 0,25 px/tick. Ante un obstáculo se para hasta que el jugador esté al
    // otro lado, en su piso.
    public class Statue : Enemy
    {
        int phase, seen;
        float level;

        public override void Init(EnemyContext ctx)
        {
            Height = 24;
            level = Y + 8;
            Visible = Harmful = Hittable = false;
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    if (ctx.Py >= level && ctx.Py <= level + 16)
                    {
                        seen |= 1;
                        if (ctx.Px >= X - 16 && ctx.Px <= X - 11) seen |= 2;
                        if (ctx.Px >= X + 20 && ctx.Px <= X + 25) seen |= 4;
                    }
                    if (seen != 7) break;
                    Visible = Harmful = Hittable = true;
                    FacePlayer(ctx); Walk(0.25f); phase = 1;
                    break;
                case 1:
                    if (Blocked(ctx)) { Vx = 0; phase = 2; break; }
                    X += Vx;
                    break;
                case 2:
                    bool otherSide = FacingLeft ? ctx.Px > X + 8 : ctx.Px < X + 8;
                    if (otherSide && Math.Abs(ctx.Py - Height - Y) <= 4) { FacingLeft = !FacingLeft; Walk(0.25f); phase = 1; }
                    break;
            }
        }
    }

    // 0x24: saltador entre pisos. Anda a 2 px/tick; cada 32 ticks puede saltar 1 o 2 pisos arriba o abajo (al azar,
    // o hacia el piso del jugador en los bloques de persecución), solo si el jugador está en el suelo y hay dónde
    // caer a 56 px (40 hacia la izquierda). No se posa hasta llegar al piso de destino.
    public class LevelJumper : Enemy
    {
        static readonly float[,] Jumps =
        {
            // vy, gravedad, vx, pisos (−arriba)
            { 0, 0, 0, 0 }, { -6, 0.5f, 2, -1 }, { -3, 0.5f, 2, 1 }, { -6.3125f, 0.3125f, 1.5f, -2 }, { -2.75f, 0.25f, 1, 2 },
        };

        int phase, timer, level;
        bool air;
        float gravity, minY;

        public override bool ThroughFloors => air;

        public override void Init(EnemyContext ctx)
        {
            timer = ctx.Rand(0, 255);
            level = (((int)Y & 0xE0) >> 5) - 1;
            FacingLeft = ctx.Coin(); Walk(2);
        }

        bool CanJump(EnemyContext ctx, int kind)
        {
            if (kind == 0 || ctx.Player.State != MotorState.Ground) return false;
            int to = level + (int)Jumps[kind, 3];
            if (to < 0 || to > 4) return false;
            float tx = X + (FacingLeft ? -40 : 56), floor = Y + 16 + 32 * Jumps[kind, 3];
            if (tx < 0x10 || tx >= 0xF0 || floor - 16 < 0x28 || floor - 16 >= 0xB9) return false;
            if (!Solid(ctx, tx, floor) && !Solid(ctx, tx + 8, floor)) return false;
            // [I] (9D1C/9D2D sin seguir del todo) el camino ha de estar libre: subiendo, el techo encima; bajando, los
            // suelos intermedios en la x de llegada.
            if (kind == 1 || kind == 3)
                return !Solid(ctx, X + 8, Y - 1) && !Solid(ctx, X + 8, Y - 17) && BodyFree(ctx, tx, floor - 16);
            for (float f = Y + 16; f < floor; f += 32)
                if (Solid(ctx, tx, f) || Solid(ctx, tx + 8, f)) return false;
            return BodyFree(ctx, tx, floor - 16);
        }

        void Jump(int kind)
        {
            Vy = Jumps[kind, 0]; gravity = Jumps[kind, 1]; Vx = FacingLeft ? -Jumps[kind, 2] : Jumps[kind, 2];
            level += (int)Jumps[kind, 3];
            minY = 0x28 + 0x20 * level;
            air = true;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (air)
            {
                bool landed = Fall(ctx, gravity);
                if (landed && Y >= minY - 0.5f) { air = false; Walk(2); level = (((int)Y & 0xE0) >> 5) - 1; }
                else if (landed) Y += 1;   // aún no es su piso: lo atraviesa
                return;
            }
            Patrol(ctx, 2);
            timer--;
            if (phase == 0)
            {
                if ((ctx.Tick & 0x1F) == 0)
                {
                    int kind = ctx.Rand(0, 4);
                    if (CanJump(ctx, kind)) { Jump(kind); return; }
                }
                if (timer <= 0) { phase = 1; timer = ctx.Rand(0, 255); }
            }
            else
            {
                if ((timer & 0x20) == 0 && (ctx.Tick & 7) == 0)
                {
                    FacePlayer(ctx); Walk(2);
                    float dy = ctx.Py - 16 - Y;
                    int kind = Math.Abs(dy) <= 4 ? 0 : dy < 0 ? (dy >= -32 ? 1 : 3) : (dy <= 32 ? 2 : 4);
                    if (CanJump(ctx, kind)) { Jump(kind); return; }
                }
                if (timer <= 0) { phase = 0; timer = ctx.Rand(0, 255); }
            }
        }
    }

    // 0x2C cabeza y 0x64 cola de oruga: por turnos, una da un paso (26 de cada 32 ticks a 1 px/tick) mientras la
    // otra espera. Si una muere, la otra anda sin parar a 2 px/tick (pasos de 13 de cada 17 ticks).
    public class InchWorm : Enemy
    {
        int group, delay, count;
        bool solo, started;
        InchWorm partner;
        int moveUntil = 26, cycle = 32;

        bool IsHead => Type == 0x2C;

        public override void Init(EnemyContext ctx)
        {
            Walk(1);
            if (IsHead)
            {
                group = ctx.GroupCounter++ & 15;
                delay = ctx.Rand(16, 31);
                return;
            }
            // La cola copia el grupo y el retardo de la cabeza del hueco anterior.
            for (int i = ctx.Enemies.Count - 1; i >= 0; i--)
                if (ctx.Enemies[i] is InchWorm h && h.IsHead && h.partner == null)
                {
                    partner = h; h.partner = this; group = h.group; delay = h.delay;
                    FacingLeft = h.FacingLeft;
                    break;
                }
            if (partner == null) GoSolo(ctx);
        }

        void GoSolo(EnemyContext ctx)
        {
            solo = true; moveUntil = 13; cycle = 17;
            Walk(2);
            ctx.GroupState[group] = 2;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (!started) { if (--delay > 0) return; started = true; }
            if (!solo && (partner == null || !partner.Alive))
                GoSolo(ctx);
            int state = ctx.GroupState[group];
            bool myTurn = state >= 2 || (IsHead ? state == 0 : state == 1);
            if (!myTurn) return;
            count++;
            if (count < moveUntil)
            {
                if (Blocked(ctx)) { FacingLeft = !FacingLeft; Walk(solo ? 2 : 1); }
                else X += Vx;
            }
            if (count >= cycle)
            {
                count = 0;
                if (state < 2) ctx.GroupState[group] ^= 1;
            }
        }
    }

    // 0x2D: soldado. Patrulla a 1,5 px/tick, salta huecos (36 px) y cambia de piso (vy −4,5 arriba, −2,5 abajo,
    // gravedad 0,25). Si el jugador está en su fila se para, lo mira y a los 30 ticks dispara un 0x3F a ±4 px/tick;
    // luego espera 60; si el disparo ha paralizado al jugador, carga a 4 px/tick hasta un obstáculo.
    public class Soldier : PlatformJumper
    {
        int aim;

        public override void Init(EnemyContext ctx)
        {
            walkSpeed = 1.5f; gravity = 0.25f;
            timer = ctx.Rand(0, 127); turns = 6;
            FacingLeft = ctx.Coin(); Walk(walkSpeed);
        }

        protected override (float, float) JumpSpeed(int kind) => (kind == 1 ? -4.5f : -2.5f, 1.5f);

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 2:
                    if (Air(ctx)) { phase = 0; timer = ctx.Rand(0, 127); }
                    return;
                case 5:
                    FacePlayer(ctx);
                    if (--aim > 0) return;
                    var c = ctx.SpawnChild(0x3F, X, Y);
                    if (c != null) { c.Vx = FacingLeft ? -4 : 4; c.FacingLeft = FacingLeft; }
                    phase = 3; aim = 60;
                    return;
                case 3:
                    // Si el disparo ha paralizado al jugador (E500 = 6), carga contra él a 4 px/tick.
                    if (ctx.Player.Stunned > 0) { phase = 4; FacePlayer(ctx); return; }
                    if (--aim <= 0) { phase = 0; timer = ctx.Rand(0, 127); Walk(walkSpeed); }
                    return;
                case 4:
                    if (Blocked(ctx)) { FacingLeft = !FacingLeft; phase = 0; timer = ctx.Rand(0, 127); Walk(walkSpeed); return; }
                    Walk(4);
                    X += Vx;
                    return;
            }
            if (phase == 1 && TryLevelJump(ctx, jumpKind, 32, 16, 40, 24)) return;
            if (Step(ctx)) { Obstacle(ctx, 36, 36); if (phase == 1 && turns % 2 == 0) jumpKind = jumpKind == 1 ? 2 : jumpKind == 2 ? 0 : 1; }
            else if (phase == 0 && Math.Abs(ctx.Py - 16 - Y) < 1) { phase = 5; aim = 30; Vx = 0; return; }
            if (phase == 0 && --timer <= 0) { phase = 1; jumpKind = ctx.Rand(0, 3) % 3; turns = 6; }
            else if (phase == 1 && turns < 0) { phase = 0; timer = ctx.Rand(0, 127); }
        }
    }

    // 0x43: criatura ancha (32 px). Anda a 2 px/tick (siempre empieza hacia la izquierda, b04:8623); si el jugador
    // está a su derecha a 32 px o menos (sin mirar la y) se para y suelta un rayo 0x62; sigue cuando ya no queda
    // ningún otro enemigo en la sala (E620 = 1) [C].
    public class WideWalker : Enemy
    {
        bool waiting;

        public override void Init(EnemyContext ctx) { Width = 32; FacingLeft = true; Walk(2); }

        public override void Tick(EnemyContext ctx)
        {
            if (waiting)
            {
                if (ctx.AliveCount == 1) { waiting = false; FacingLeft = true; Walk(2); }
                return;
            }
            if (ctx.Px > X && ctx.Px <= X + 32)
            {
                waiting = true; Vx = 0;
                ctx.SpawnChild(0x62, X + 14, Y);
                return;
            }
            bool edge = FacingLeft ? X < 0x10 : X >= 0xE0;
            if (edge || Blocked(ctx)) { FacingLeft = !FacingLeft; Walk(2); return; }
            X += Vx;
        }
    }
}
