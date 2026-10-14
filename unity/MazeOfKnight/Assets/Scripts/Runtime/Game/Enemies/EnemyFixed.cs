using System;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Enemigos fijos o casi: trampas, torretas y bichos pegados al techo (docs/enemy_ai.md).

    // 0x03: babosa de techo. Dormida (la mitad) hasta que el jugador pasa por debajo; despierta anda colgada a
    // tirones: quieta 8 ticks, 16 a 1 px/tick, quieta 4 y una pausa de 24. Gira al acabarse el techo o ante una
    // pared. Siempre empieza hacia la derecha (b04: las dos ramas cargan +1) [C].
    // Cajas: dormida y en la pausa C4 → 66 (6 × 14); quieta antes de avanzar CC/D0 → 77; avanzando C8 → 74.
    public class CeilingSlug : Enemy
    {
        int phase, step, timer;

        public override void Init(EnemyContext ctx) { phase = ctx.Coin() ? 0 : -1; Vx = 1; SetBox(ctx, 0x66); }

        void UpdateBox(EnemyContext ctx)
        {
            if (phase == 0 && step < 8) SetBox(ctx, 0x77);
            else if (phase == 0 && step < 28) SetBox(ctx, 0x74);
            else SetBox(ctx, 0x66);
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case -1:
                    if (Math.Abs(Y - (ctx.Py - 24)) < 1 && Math.Abs(ctx.Px - 6 - X) < 32) phase = 0;
                    break;
                case 0:
                    if (!Solid(ctx, FacingLeft ? X - 1 : X + 16, Y - 2) || WallAhead(ctx)) Turn();
                    if (step >= 8 && step < 24) X += Vx;
                    if (++step >= 28) { step = 0; phase = 1; timer = 24; }
                    break;
                case 1:
                    if (--timer <= 0) phase = 0;
                    break;
            }
            UpdateBox(ctx);
        }
    }

    // 0x04: pistón (sin sprite). Baja 8 px cada 15 ticks hasta un sólido o y = 0xB0 y vuelve a subir. El original lo
    // dibuja con tiles (99, 9C, 9D, B4, B5), que no son sólidos [O]: no tapa el paso. La y del enemigo es la cabeza
    // (la fase 0 baja y en 8 px), así que la caja (22, 8 × 9) está en la cabeza y solo la cabeza toca al jugador y a
    // las armas [C] (b03:B954 con origen en IX+5/IX+7); la columna de encima solo se dibuja.
    // [I] La fase latente (2: esperar al jugador 0..24 px por debajo; 3: seguir su y) no se usa: la ficha no dice
    // quién elige la fase inicial.
    public class Piston : Enemy
    {
        int length, timer = 15;
        bool retract;
        float top;

        public override void Init(EnemyContext ctx) { top = Y; Height = 8; }

        public override void Tick(EnemyContext ctx)
        {
            if (--timer > 0) return;
            timer = 15;
            if (!retract)
            {
                if (Y >= 0xB0 || Solid(ctx, X, Y + 8) || Solid(ctx, X + 8, Y + 8)) retract = true;
                else length++;
            }
            else if (length > 0) length--;
            else retract = false;
            Y = top + 8 * length;
        }

        // Se dibuja la columna entera, del techo a la cabeza.
        public override (float x0, float y0, float x1, float y1) DrawRect()
        {
            var (x0, _, x1, y1) = Box();
            return (x0, Math.Min(top, Y), x1, y1);
        }
    }

    // 0x2F: bloque que imita los saltos del jugador: cuando el jugador sube, sube con él (hasta el techo); cuando
    // baja o está en el suelo, baja hasta su altura de reposo.
    public class Mimic : Enemy
    {
        float rest;

        public override void Init(EnemyContext ctx) => rest = Y;

        public override void Tick(EnemyContext ctx)
        {
            var s = ctx.Player.State;
            bool air = s == MotorState.Jump || s == MotorState.Fall;
            if (air && ctx.PlayerDy < 0)
            {
                if (Y + ctx.PlayerDy >= 8 + P.RoomTopPx && !Solid(ctx, X, Y + ctx.PlayerDy) && !Solid(ctx, X + 15, Y + ctx.PlayerDy))
                    Y += ctx.PlayerDy;
            }
            else if (Y < rest)
            {
                Y = Math.Min(rest, Y + Math.Max(1, Math.Abs(ctx.PlayerDy)));
            }
        }
    }

    // 0x32 torreta (dispara a la derecha) y 0x51 trampa de flechas (a la izquierda, sin sprite). Invulnerables y sin
    // daño por contacto. Con el jugador 1..32 px por debajo y en ciertas franjas de x, disparan un 0x33 a ±4 px/tick
    // cada 1..32 ticks. Las franjas miran al lado contrario del disparo [C] (docs/enemy_ai.md, duda abierta).
    public class ArrowTrap : Enemy
    {
        int phase, timer = 30, shot = 30;

        public override void Init(EnemyContext ctx) { Harmful = false; Visible = Type == 0x32; }

        bool InZone(EnemyContext ctx)
        {
            float dy = ctx.Py - Y;
            if (dy < 1 || dy > 32) return false;
            if (Type == 0x32) { float d = X - ctx.Px; return d >= 16 && d < 48 || d >= 64 && d < 96; }
            float e = ctx.Px - X;
            return e >= 40 && e < 72 || e >= 88 && e < 120;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (phase == 0)
            {
                if (--timer > 0) return;
                timer = 256;
                if (InZone(ctx)) phase = 1;
                return;
            }
            if (!InZone(ctx)) { phase = 0; timer = 30; return; }
            if (--shot > 0) return;
            shot = ctx.Rand(1, 32);
            var c = ctx.SpawnChild(0x33, Type == 0x32 ? X + 16 : X, Y + 4);
            if (c != null) { c.Vx = Type == 0x32 ? 4 : -4; c.FacingLeft = c.Vx < 0; }
        }
    }

    // 0x3A boca de pared (a la derecha) y 0x50 (a la izquierda, sin sprite). Invulnerables y sin daño por contacto.
    // Si el jugador está 1..32 px por debajo y a ese lado, se abren 120 ticks y disparan un 0x3B cada 8.
    public class WallMouth : Enemy
    {
        int phase, timer = 60;

        public override void Init(EnemyContext ctx) { Harmful = false; Visible = Type == 0x3A; }

        bool Side(EnemyContext ctx) => Type == 0x3A ? X < ctx.Px : X >= ctx.Px;

        public override void Tick(EnemyContext ctx)
        {
            if (phase == 0)
            {
                if (--timer > 0) return;
                float dy = ctx.Py - Y;
                if (dy > 0 && dy <= 32 && Side(ctx)) { phase = 1; timer = 120; Frame = 1; }
                else timer = 256;
                return;
            }
            if ((ctx.Tick & 7) == 0 && Side(ctx)) ctx.SpawnChild(0x3B, Type == 0x3A ? X + 16 : X, Y + 4);
            if (--timer <= 0) { phase = 0; timer = 120; Frame = 0; }
        }
    }

    // 0x42: monstruo del agua. Oculto mientras el jugador está en el agua; si no, a los 32 ticks sale en una de 4 x
    // fijas y sube de y 0xA0 a 0x94 a 0,5 px/tick, lanza un chorro 0x60 junto al jugador y le dispara un 0x61 cada
    // 32 ticks (si está por encima de 0x94) hasta que el chorro acaba o el jugador se mete en el agua; entonces se
    // hunde y vuelve a empezar.
    public class WaterMonster : Enemy
    {
        static readonly int[] Columns = { 0x30, 0x60, 0x80, 0x90 };
        int phase, timer, shot;

        public override void Init(EnemyContext ctx) => Restart(ctx);

        void Restart(EnemyContext ctx)
        {
            X = Columns[ctx.Rand(0, 3)];
            Y = 0xA0;
            ctx.WaterJetDone = false;
            Visible = Harmful = Hittable = false;
            phase = 0; timer = 32;
        }

        public override void Tick(EnemyContext ctx)
        {
            bool inWater = ctx.Player.InLiquid;
            switch (phase)
            {
                case 0:
                    if (inWater) { timer = 32; break; }
                    if (--timer > 0) break;
                    Visible = Harmful = Hittable = true; phase = 1;
                    break;
                case 1:
                    Y -= 0.5f;
                    if (Y > 0x94) break;
                    Y = 0x94; phase = 2; shot = 1; ctx.WaterJetDone = false;
                    float jx = ctx.Px < 0x30 ? ctx.Px + 8 : ctx.Px >= 0xC8 ? ctx.Px - 16 : ctx.Px + (ctx.Coin() ? 8 : -16);
                    ctx.SpawnChild(0x60, jx, 0xA0);
                    break;
                case 2:
                    if (ctx.WaterJetDone || inWater) { phase = 3; break; }
                    if (ctx.Py < 0x94 && --shot <= 0) { shot = 32; ctx.SpawnChild(0x61, X + 16, Y); }
                    break;
                case 3:
                    Y += 0.5f;
                    if (Y >= 0xA0) Restart(ctx);
                    break;
            }
        }
    }
}
