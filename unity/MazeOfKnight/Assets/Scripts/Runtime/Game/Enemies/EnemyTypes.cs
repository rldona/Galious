using System;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Tipos de enemigo implementados a partir de sus fichas (docs/enemy_ai.md). Los tiempos son ticks de IA.

    // 0x0E: murciélago. Cuelga del techo 80 ticks, cae 10 a 1 px/tick, revolotea 96..127 y vuelve a buscar techo
    // volando en uno de 16 rumbos a 2 px/tick (cambia cada 16..31 y rebota en los límites). Atraviesa los muros.
    public class Bat : Enemy
    {
        int phase, timer, turn;

        bool CeilingAbove(EnemyContext ctx) => Solid(ctx, X, Y - 1) && Solid(ctx, X + 8, Y - 1);

        public override void Init(EnemyContext ctx)
        {
            if (CeilingAbove(ctx) && !Solid(ctx, X, Y) && !Solid(ctx, X + 8, Y))
                Hang();
            else
                StartFlying(ctx, 1);
        }

        void Hang() { phase = 2; timer = 80; Vx = Vy = 0; }

        void StartFlying(EnemyContext ctx, int next)
        {
            phase = next;
            Heading(ctx.Rand(0, 15), 2);
            turn = ctx.Rand(16, 31);
            if (next == 4)
                timer = ctx.Rand(96, 127);
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 1:
                case 4:
                    X += Vx;
                    Y += Vy;
                    BounceInRoom();
                    if (--turn <= 0)
                    {
                        Heading(ctx.Rand(0, 15), 2);
                        turn = ctx.Rand(16, 31);
                    }
                    if (phase == 4 && --timer <= 0)
                        phase = 1;
                    else if (phase == 1 && Vy < 0 && CeilingAbove(ctx))
                        Hang();
                    break;
                case 2:
                    if (--timer <= 0) { phase = 3; timer = 10; Vx = 0; Vy = 1; }
                    break;
                case 3:
                    Y += Vy;
                    if (--timer <= 0)
                        StartFlying(ctx, 4);
                    break;
            }
            FacingLeft = Vx < 0;
        }
    }

    // 0x25: esqueleto (dos sprites, 32 x 16). Anda a 1,125 px/tick hacia el jugador, salta (vy -4,31 o -1,81,
    // vx ±2, gravedad 0,3125, vy máx. 6), descansa 32..159 ticks. Al recibir golpes se enrosca (3 pasos), queda
    // enroscado 60 ticks y se desenrosca un paso cada 8 recuperando la vida (8).
    // [I] Simplificado: el original sondea plataformas para decidir el salto (b04:9A67, 9CA3); aquí, al acabar su
    // temporizador, salta al azar alto o bajo, y ante un hueco o una pared salta si hay suelo a 48 px, o da la vuelta.
    public class Skeleton : Enemy
    {
        const float Walk = 1.125f, Gravity = 0.3125f;
        enum Phase { Walk, Air, Pause, Curling, Curled, Uncurling }
        Phase phase;
        int walkTimer, jumpTimer, pauseTimer, curl = 3, curlTimer;
        float yAcc;
        int maxHp;

        public override void Init(EnemyContext ctx)
        {
            Width = 32;
            maxHp = Hp;
            walkTimer = ctx.Rand(32, 159);
            jumpTimer = ctx.Rand(0, 127);
            FacingLeft = ctx.Player.X < X + 16;
            Vx = FacingLeft ? -Walk : Walk;
        }

        // Pared a lo alto de toda la caja (arriba, centro y abajo) en la columna x.
        bool BodyBlocked(EnemyContext ctx, float x) => Solid(ctx, x, Y) || Solid(ctx, x, Y + 8) || Solid(ctx, x, Y + 15);

        bool FloorUnder(EnemyContext ctx) => Solid(ctx, X + 4, Y + 16) || Solid(ctx, X + Width - 4, Y + 16);

        void Jump(float vy) { phase = Phase.Air; Vy = vy; Vx = FacingLeft ? -2 : 2; Y = (float)Math.Round(Y); yAcc = 0; }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case Phase.Walk:
                {
                    if (!FloorUnder(ctx)) { phase = Phase.Air; Vy = 0; yAcc = 0; Y = (float)Math.Round(Y); break; }
                    float lead = FacingLeft ? X - 1 : X + Width;
                    bool wall = BodyBlocked(ctx, lead);
                    bool gap = !Solid(ctx, lead, Y + 16);
                    bool edge = FacingLeft ? X <= 0x10 : X >= 0xD8;
                    if (wall || gap || edge)
                    {
                        float land = FacingLeft ? X - 32 : X + 48;
                        if (!edge && !wall && Solid(ctx, land, Y + 32)) Jump(-1.8125f);
                        else { FacingLeft = !FacingLeft; Vx = -Vx; }
                        break;
                    }
                    X += Vx;
                    if (--jumpTimer <= 0) { jumpTimer = ctx.Rand(0, 127); Jump(ctx.Rand(0, 2) == 1 ? -4.3125f : -1.8125f); }
                    else if (--walkTimer <= 0) { phase = Phase.Pause; pauseTimer = ctx.Rand(32, 159); }
                    break;
                }
                case Phase.Air:
                {
                    Vy = Math.Min(6f, Vy + Gravity);
                    float nx = X + Vx;
                    if (nx >= 0 && nx + Width <= P.RoomWidthPx && !BodyBlocked(ctx, Vx < 0 ? nx : nx + Width - 1))
                        X = nx;
                    // Vertical píxel a píxel: choca con el techo y se posa en el primer suelo.
                    yAcc += Vy;
                    int steps = (int)yAcc;
                    yAcc -= steps;
                    for (int i = 0; i < Math.Abs(steps); i++)
                    {
                        if (steps < 0)
                        {
                            if (Y - 1 < P.RoomTopPx || Solid(ctx, X + 4, Y - 1) || Solid(ctx, X + Width - 4, Y - 1))
                            {
                                Vy = 0; yAcc = 0;
                                break;
                            }
                            Y -= 1;
                        }
                        else
                        {
                            if (((int)Y + 16 - P.RoomTopPx) % 8 == 0 && FloorUnder(ctx))
                            {
                                phase = Phase.Walk;
                                Vx = FacingLeft ? -Walk : Walk;
                                yAcc = 0;
                                break;
                            }
                            Y += 1;
                        }
                    }
                    break;
                }
                case Phase.Pause:
                    if (--pauseTimer <= 0) { phase = Phase.Walk; walkTimer = ctx.Rand(32, 159); }
                    break;
                case Phase.Curling:
                    if (--curlTimer <= 0) { phase = Phase.Uncurling; curlTimer = 8; }
                    break;
                case Phase.Curled:
                    if (--curlTimer <= 0) { phase = Phase.Uncurling; curlTimer = 8; }
                    break;
                case Phase.Uncurling:
                    if (--curlTimer > 0) break;
                    curlTimer = 8;
                    if (++curl >= 3) { Hp = maxHp; Harmful = true; phase = Phase.Walk; walkTimer = ctx.Rand(32, 159); }
                    break;
            }
            Grow = 0.5f + curl / 6f;
        }

        public override bool OnHit(EnemyContext ctx)
        {
            if (phase == Phase.Air)
                return true;
            // Se enrosca un paso por golpe; enroscado no hace daño [I: borra el bit 6 de IX+2].
            Harmful = false;
            curl = Math.Max(0, curl - 1);
            phase = curl == 0 ? Phase.Curled : Phase.Curling;
            curlTimer = 60;
            return true;
        }
    }

    // 0x31: pinchos que emergen del suelo (o cuelgan del techo). Invulnerables. Tras una espera de 1..256 ticks,
    // 8 etapas de 32 ticks (alternando cada 3 ticks con la siguiente) por la tabla 00 C4 C8 CC D0 CC C8 C4 00;
    // ocultos 32 ticks y repite. La etapa C4 es visible pero no daña.
    public class Spikes : Enemy
    {
        static readonly int[] Levels = { 0, 1, 2, 3, 4, 3, 2, 1, 0 };
        int phase, timer, stage;

        public override void Init(EnemyContext ctx)
        {
            Invulnerable = true;
            GrowsDown = !Solid(ctx, X, Y + 16);
            timer = ctx.Rand(1, 256);
            Apply(0);
        }

        void Apply(int level)
        {
            Visible = level > 0;
            Harmful = level > 1;
            Grow = level / 4f;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (phase == 0)
            {
                if (--timer > 0) return;
                phase = 1; stage = 0; timer = 32;
            }
            if (phase == 2)
            {
                if (--timer <= 0) { phase = 1; stage = 0; timer = 32; }
                return;
            }
            int level = (timer / 3) % 2 == 0 ? Levels[stage] : Levels[stage + 1];
            Apply(level);
            if (--timer > 0) return;
            timer = 32;
            if (++stage >= 8) { phase = 2; Apply(0); }
        }
    }

    // 0x18: rodador. 0,5 px/tick en un sentido al azar; anima un ciclo de 7 fotogramas cada 4 ticks. Ante un
    // hueco, una pared o el borde gira si está en el fotograma 0 o 6; si no, se para hasta llegar a uno de ellos.
    public class Roller : Enemy
    {
        int frame, animTimer = 4;
        bool stopped;

        public override void Init(EnemyContext ctx)
        {
            FacingLeft = ctx.Rand(0, 1) == 1;
            Vx = FacingLeft ? -0.5f : 0.5f;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (--animTimer <= 0)
            {
                animTimer = 4;
                frame = (frame + (FacingLeft ? 6 : 1)) % 7;
            }
            bool canTurn = frame == 0 || frame == 6;
            if (stopped)
            {
                if (canTurn) { stopped = false; Turn(); }
                return;
            }
            float lead = FacingLeft ? X - 1 : X + Width;
            bool blocked = Solid(ctx, lead, Y + 8) || !Solid(ctx, lead, Y + 16) || lead < 0 || lead >= P.RoomWidthPx;
            if (blocked)
            {
                if (canTurn) Turn();
                else stopped = true;
                return;
            }
            X += Vx;
        }

        void Turn() { FacingLeft = !FacingLeft; Vx = -Vx; }
    }

    // 0x34: bloque que sube y baja, más rápido cuanto más cerca está el jugador. Velocidad = término x (por
    // |x jugador − x|: ≥64 1,0; 40..63 1,5; 24..39 2,0; <24 2,5) + término y (por |y jugador − y|: ≥128 1,0;
    // 96..127 1,5; 64..95 2,0; 32..63 2,5; <32 3,0). Invierte al tocar un sólido (sonda en y + 8 bajando, como el
    // original, que se mete medio tile). Por encima de y = 0x20 baja con solo el término x.
    public class Block : Enemy
    {
        bool up;

        public override void Tick(EnemyContext ctx)
        {
            float dx = Math.Abs(ctx.Player.X - X), dy = Math.Abs(ctx.Player.Y - Y);
            float tx = dx >= 64 ? 1f : dx >= 40 ? 1.5f : dx >= 24 ? 2f : 2.5f;
            float ty = dy >= 128 ? 1f : dy >= 96 ? 1.5f : dy >= 64 ? 2f : dy >= 32 ? 2.5f : 3f;
            float speed = tx + ty;
            if (Y < 0x20) { up = false; speed = tx; }
            if (up ? Solid(ctx, X, Y) : Solid(ctx, X, Y + 8))
                up = !up;
            Y += up ? -speed : speed;
        }
    }

    // 0x2E: brote que crece y se esconde. Oculto 1..256 ticks; 9 pasos (C4 C8 CC D0 D4 D0 CC C8 C4); los pasos
    // 0, 4 y 8 duran 16 ticks y el resto 4. En los pasos 0 y 8 se ve pero no daña. Luego vuelve a esconderse.
    public class Sprout : Enemy
    {
        static readonly int[] Levels = { 1, 2, 3, 4, 5, 4, 3, 2, 1 };
        int step = -1, timer;

        public override void Init(EnemyContext ctx) => Hide(ctx);

        void Hide(EnemyContext ctx)
        {
            step = -1;
            timer = ctx.Rand(1, 256);
            Visible = Harmful = false;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (--timer > 0) return;
            if (++step >= 9) { Hide(ctx); return; }
            timer = step == 0 || step == 4 || step == 8 ? 16 : 4;
            Visible = true;
            Harmful = step != 0 && step != 8;
            Grow = Levels[step] / 5f;
        }
    }
}
