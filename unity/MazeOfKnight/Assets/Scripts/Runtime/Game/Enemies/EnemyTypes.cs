using System;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Tipos de enemigo implementados a partir de sus fichas (docs/enemy_ai.md). Los tiempos son ticks de IA.

    // 0x0E: murciélago. Cuelga del techo 80 ticks, cae 10 a 1 px/tick, revolotea 96..127 y vuelve a buscar techo
    // volando en uno de 16 rumbos a 2 px/tick (cambia cada 16..31 y rebota en los límites). Atraviesa los muros.
    // Cajas: colgado 67 (sprite A0); al soltarse 69 (AC) y revoloteando 69 ↔ 0F (IX+11 ^ 66 con la animación).
    // [I] La animación del vuelo se toma cada 6 ticks, como la caída (la ficha da 4..6).
    public class Bat : Enemy
    {
        int phase, timer, turn, anim = 6;

        bool CeilingAbove(EnemyContext ctx) => Solid(ctx, X, Y - 1) && Solid(ctx, X + 8, Y - 1);

        public override void Init(EnemyContext ctx)
        {
            if (CeilingAbove(ctx) && !Solid(ctx, X, Y) && !Solid(ctx, X + 8, Y))
                Hang(ctx);
            else
                StartFlying(ctx, 1);
        }

        void Flap(EnemyContext ctx)
        {
            if (--anim > 0) return;
            anim = 6;
            SetBox(ctx, BoxIndex == 0x69 ? 0x0F : 0x69);
        }

        void Hang(EnemyContext ctx) { phase = 2; timer = 80; Vx = Vy = 0; SetBox(ctx, 0x67); }

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
                    Flap(ctx);
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
                        Hang(ctx);
                    break;
                case 2:
                    if (--timer <= 0) { phase = 3; timer = 10; Vx = 0; Vy = 1; anim = 6; SetBox(ctx, 0x69); }
                    break;
                case 3:
                    Flap(ctx);
                    Y += Vy;
                    if (--timer <= 0)
                        StartFlying(ctx, 4);
                    break;
            }
            FacingLeft = Vx < 0;
        }
    }

    // 0x25: esqueleto: sprite base de 16 px y un segundo sprite hacia donde mira (medido en openMSX: castillo sala 9,
    // mirando a la derecha, sprites en x y x + 16; sala 83, a la izquierda, en x y x − 16). Choca con las paredes
    // solo con el sprite base; el segundo puede quedar sobre un muro, como en el original. Anda a 1,125 px/tick hacia
    // el jugador, salta (vy −4,31 subiendo o −1,81, vx ±2, gravedad 0,3125, vy máx. 6), descansa 32..159 ticks. Al
    // recibir golpes se enrosca (3 pasos), queda enroscado 60 ticks y se desenrosca un paso cada 8 recuperando la vida
    // (8). Lleva escudo por delante (IX+2 bit 1, b04:98C0): solo se le hace daño por la espalda.
    // Saltos como el 0x1F (docs/enemy_ai.md): ante un hueco, con +1E = 0, salta a su mismo nivel si hay suelo a 48 px
    // (32 a la izquierda); al acabar +08 elige un tipo de salto 0..2 y en cada tick sondea subir o bajar un piso
    // (ProbeLevelJump); cada giro baja +18 (6), con 4 y 2 cambia de tipo y al pasar de 0 deja de buscar.
    // Cajas: andando 4E / 4F (24 de ancho hacia donde mira); enroscado, tabla 9A61 por el paso: 2 → 4D, 1 → 0B, 0 → 19.
    public class Skeleton : Enemy
    {
        const float WalkSpeed = 1.125f, Gravity = 0.3125f, HighJump = -4.3125f, LowJump = -1.8125f;
        enum Phase { Walk, Air, Pause, Curling, Curled, Uncurling }
        Phase phase;
        int walkTimer, jumpTimer, pauseTimer, curl = 3, curlTimer, jumpKind, turns;
        bool searching;
        float yAcc;
        int maxHp;

        public override void Init(EnemyContext ctx)
        {
            maxHp = Hp;
            ShieldSide = true;
            walkTimer = ctx.Rand(32, 159);
            jumpTimer = ctx.Rand(0, 127);
            FacingLeft = ctx.Player.X < X + 16;
            Vx = FacingLeft ? -WalkSpeed : WalkSpeed;
            UpdateBox(ctx);
        }

        static readonly int[] CurlBoxes = { 0x19, 0x0B, 0x4D };

        void UpdateBox(EnemyContext ctx)
        {
            if (curl >= 3) { if (SetBox(ctx, 0x4E, 0x4F)) return; }
            else if (SetBox(ctx, CurlBoxes[curl])) return;
            Grow = 0.5f + curl / 6f;
        }

        // Pared a lo alto de toda la caja (arriba, centro y abajo) en la columna x.
        bool BodyBlocked(EnemyContext ctx, float x) => Solid(ctx, x, Y) || Solid(ctx, x, Y + 8) || Solid(ctx, x, Y + 15);

        bool FloorUnder(EnemyContext ctx) => Solid(ctx, X + 4, Y + 16) || Solid(ctx, X + Width - 4, Y + 16);

        // Hueco a su mismo nivel (dy +16): suelo a 48 px por la derecha o 32 por la izquierda y sitio para el cuerpo.
        bool CanJumpGap(EnemyContext ctx)
        {
            float tx = FacingLeft ? X - 32 : X + 48;
            return tx >= 0x10 && tx < 0xF0 && Solid(ctx, tx, Y + 16) && BodyFree(ctx, tx, Y);
        }

        void Jump(float vy)
        {
            phase = Phase.Air; Vy = vy; Vx = FacingLeft ? -2 : 2; Y = (float)Math.Round(Y); yAcc = 0;
            searching = false;
        }

        void TurnAround(EnemyContext ctx)
        {
            FacingLeft = !FacingLeft; Vx = -Vx;
            if (!searching) return;
            turns--;
            if (turns == 4 || turns == 2) jumpKind = (jumpKind + 1) % 3;
            else if (turns < 0) { searching = false; jumpTimer = ctx.Rand(0, 127); }
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case Phase.Walk:
                {
                    if (!FloorUnder(ctx)) { phase = Phase.Air; Vy = 0; yAcc = 0; Y = (float)Math.Round(Y); break; }
                    if (searching && ProbeLevelJump(ctx, jumpKind)) { Jump(jumpKind == 1 ? HighJump : LowJump); break; }
                    float lead = FacingLeft ? X - 1 : X + Width;
                    bool wall = BodyBlocked(ctx, lead);
                    bool gap = !Solid(ctx, lead, Y + 16);
                    bool edge = FacingLeft ? X <= 0x10 : X >= 0xD8;
                    if (wall || gap || edge)
                    {
                        if (jumpKind == 0 && !edge && !wall && CanJumpGap(ctx)) Jump(LowJump);
                        else TurnAround(ctx);
                        break;
                    }
                    X += Vx;
                    if (!searching && --jumpTimer <= 0) { searching = true; jumpKind = ctx.Rand(0, 2); turns = 6; }
                    if (--walkTimer <= 0) { phase = Phase.Pause; pauseTimer = ctx.Rand(32, 159); }
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
                            if (Y - 1 < P.RoomTopPx || Solid(ctx, X, Y - 1) || Solid(ctx, X + 8, Y - 1) || Solid(ctx, X + Width - 1, Y - 1))
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
                                Vx = FacingLeft ? -WalkSpeed : WalkSpeed;
                                jumpTimer = ctx.Rand(0, 127);
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
            UpdateBox(ctx);
        }

        public override string ToString() => $"fase {phase} vx {Vx:0.00} vy {Vy:0.00}";

        public override bool OnHit(EnemyContext ctx)
        {
            if (phase == Phase.Air)
                return true;
            // Se enrosca un paso por golpe; enroscado no hace daño [I: borra el bit 6 de IX+2].
            Harmful = false;
            curl = Math.Max(0, curl - 1);
            phase = curl == 0 ? Phase.Curled : Phase.Curling;
            curlTimer = 60;
            UpdateBox(ctx);
            return true;
        }
    }

    // 0x31: pinchos que emergen del suelo (o cuelgan del techo). Invulnerables. Tras una espera de 1..256 ticks,
    // 8 etapas de 32 ticks (alternando cada 3 ticks con la siguiente) por la tabla 00 C4 C8 CC D0 CC C8 C4 00;
    // ocultos 32 ticks y repite. La etapa C4 es visible pero no daña. Cajas de la tabla 8C17 por el sprite: desde el
    // suelo C4 72, C8 73, CC 74, D0 75; colgando del techo D4 76, D8 77, DC 74, E0 75.
    public class Spikes : Enemy
    {
        static readonly int[] Levels = { 0, 1, 2, 3, 4, 3, 2, 1, 0 };
        static readonly int[] FloorBoxes = { 0x72, 0x72, 0x73, 0x74, 0x75 }, CeilingBoxes = { 0x76, 0x76, 0x77, 0x74, 0x75 };
        int phase, timer, stage;

        public override void Init(EnemyContext ctx)
        {
            Invulnerable = true;
            GrowsDown = !Solid(ctx, X, Y + 16);
            timer = ctx.Rand(1, 256);
            Apply(ctx, 0);
        }

        void Apply(EnemyContext ctx, int level)
        {
            Visible = level > 0;
            Harmful = level > 1;
            SetBoxOrGrow(ctx, (GrowsDown ? CeilingBoxes : FloorBoxes)[level], level / 4f);
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
            Apply(ctx, level);
            if (--timer > 0) return;
            timer = 32;
            if (++stage >= 8) { phase = 2; Apply(ctx, 0); }
        }
    }

    // 0x18: rodador. 0,5 px/tick en un sentido al azar; anima un ciclo de 7 fotogramas cada 4 ticks. Ante un
    // hueco, una pared o el borde gira si está en el fotograma 0 o 6; si no, se para hasta llegar a uno de ellos.
    // Caja por fotograma (tabla 6CC4): A0 1E, A8 0B, AC 0B, A4 1F, B4 25, B0 0B, A0 1E.
    public class Roller : Enemy
    {
        static readonly int[] FrameBoxes = { 0x1E, 0x0B, 0x0B, 0x1F, 0x25, 0x0B, 0x1E };

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
            SetBox(ctx, FrameBoxes[frame]);
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
    // Cajas de la tabla A0D1: 26, 27, 28, 29, 29, 29, 28, 27, 26.
    public class Sprout : Enemy
    {
        static readonly int[] Levels = { 1, 2, 3, 4, 5, 4, 3, 2, 1 };
        static readonly int[] StepBoxes = { 0x26, 0x27, 0x28, 0x29, 0x29, 0x29, 0x28, 0x27, 0x26 };
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
            SetBoxOrGrow(ctx, StepBoxes[step], Levels[step] / 5f);
        }
    }
}
