using System;
using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Enemigos que crean los generadores periódicos (docs/enemy_ai.md, "generados periódicamente"). px/tick.

    // 0x09: bola que cae (gravedad 0,5, máx. 6) a ±1 px/tick; rebota una vez en cada suelo (vy −4) y después lo
    // atraviesa (solo mira suelos por debajo de donde rebotó). No mira paredes.
    public class BouncingBall : Enemy
    {
        float floorMin;

        public override void Init(EnemyContext ctx) => Vx = ctx.Coin() ? -1 : 1;

        public override void Tick(EnemyContext ctx)
        {
            Vy = Math.Min(6, Vy + 0.5f);
            Move();
            if (Vy <= 0 || Y < floorMin) return;
            float feet = Y + 16;
            bool floor = (Solid(ctx, X, feet) || Solid(ctx, X + 8, feet)) && !Solid(ctx, X, feet - 8) && !Solid(ctx, X + 8, feet - 8);
            if (!floor) return;
            Y = (float)Math.Floor(feet / 8) * 8 - 16;
            Vy = -4;
            Vx = ctx.Coin() ? -1 : 1;
            floorMin = Y + 16;
            FacingLeft = Vx < 0;
        }
    }

    // 0x38: sube acelerando (g −0,5; desde el 2.º tick va a −5,5 fijo [C]); al dar con un techo se queda colgado
    // 60 ticks, se deja caer (vy +4, frenando) y vuelve a subir atravesando ese techo hasta el siguiente. Acaba
    // saliendo por arriba.
    public class CeilingClimber : Enemy
    {
        float limit = 0xA1;
        int hang;

        // Cajas: colgado C4 → 66; tras soltarse C8 → 67 (attr 0x67).
        public override void Init(EnemyContext ctx) { Vx = ctx.Coin() ? -1 : 1; SetBox(ctx, 0x66); }

        public override void Tick(EnemyContext ctx)
        {
            if (hang > 0)
            {
                if (--hang == 0) { Vy = 4; limit = Y - 32; SetBox(ctx, 0x67); }
                return;
            }
            Vy -= 0.5f;
            if (Vy < 0) Vy = -5.5f;
            Move();
            if (Y < limit && (Solid(ctx, X, Y - 1) || Solid(ctx, X + 8, Y - 1)))
            {
                Y = ((int)Y & ~7) + 8;
                Vx = Vy = 0;
                hang = 60;
                SetBox(ctx, 0x66);
            }
        }
    }

    // 0x08: surtidor (2 sprites de ancho). Nace con una salpicadura, sube a 4 px/tick hasta una altura al azar
    // (48..111), flota 60 ticks oscilando ±1 px y estalla en 8 gotas 0x1B.
    public class Geyser : Enemy
    {
        float target;
        int hover = -1;

        public override void Init(EnemyContext ctx)
        {
            Width = 32;
            X = (ctx.Tick & 0x7F) + 0x40;
            target = ctx.Rand(48, 111);
            Vy = -4;
            var s = ctx.SpawnChild(0x6A, X, Y);
            if (s != null) s.Param = 1;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (hover < 0)
            {
                Y += Vy;
                if (Y <= target) { Y = target; hover = 60; }
                return;
            }
            Y += (hover & 7) < 4 ? 0.25f : -0.25f;
            if (--hover > 0) return;
            Alive = false;
            for (int d = 7; d >= 0; d--)
            {
                var c = ctx.SpawnChild(0x1B, X + 8, Y);
                if (c != null) { c.Param = d; ((Droplet)c).Launch(ctx); }
            }
        }
    }

    // 0x1B: gota del surtidor. Sale en una de 8 direcciones (vx, vy, gravedad de la tabla) y muere con una
    // salpicadura al bajar de cierta altura (más abajo en los mundos 9 y 10).
    public class Droplet : Enemy
    {
        static readonly (float vx, float vy, float g)[] Dirs =
            { (0.25f, -4, 0.125f), (0.75f, -3.5f, 0.133f), (1.25f, -3, 0.141f), (2, -2, 0.125f) };
        static readonly int[] Floor = { 0xA8, 0xA0, 0x90, 0x78, 0x78, 0x90, 0xA0, 0xA8 };
        static readonly int[] FloorLate = { 0xB0, 0xA8, 0xA4, 0xA0, 0xA0, 0xA4, 0xA8, 0xB0 };
        float g;

        public override void Init(EnemyContext ctx) => DiesOnTouch = true;

        public void Launch(EnemyContext ctx)
        {
            int d = Param & 7;
            var t = d < 4 ? Dirs[d] : Dirs[7 - d];
            Vx = d < 4 ? t.vx : -t.vx;
            Vy = t.vy; g = t.g;
        }

        public override void Tick(EnemyContext ctx)
        {
            Vy += g;
            Move();
            int floor = (ctx.E041 >= 10 ? FloorLate : Floor)[Param & 7];
            if (Vy > 0 && Y >= floor)
            {
                Alive = false;
                ctx.SpawnChild(0x6A, X, Y);
            }
        }
    }

    // 0x6A: salpicadura quieta 10 ticks (no hace daño).
    public class Splash : Enemy
    {
        int timer = 10;

        public override void Init(EnemyContext ctx) { Harmful = Hittable = false; }

        public override void Tick(EnemyContext ctx) { if (--timer <= 0) Alive = false; }
    }

    // 0x0D: pez que salta desde y = 0x90 (castillo, salas 137–140) junto al jugador o al azar; la fuerza del salto
    // depende de la altura del jugador (cuanto más arriba, más bajo [C]). Gravedad 0,5; y solo con la parte entera de
    // vy. Al volver a 0x90 salpica y desaparece.
    public class LeapFish : Enemy
    {
        static readonly (float vx, float vy)[] Leaps = { (0.664f, -6.5f), (0.762f, -8.5f), (0.887f, -10f), (1.141f, -11.5f) };
        bool placed;

        public void Place(EnemyContext ctx, PeriodicJson p)
        {
            placed = true;
            int min = p.xClamp.Length == 2 ? p.xClamp[0] : 0, max = p.xClamp.Length == 2 ? p.xClamp[1] : 0xE0;
            bool right = ctx.Coin();
            int k;
            if (ctx.Coin())
            {
                X = ctx.Px + (right ? -16 : 16);
                int py = ctx.Py;
                k = py < 0x3D ? 0 : py < 0x5D ? 1 : py < 0x7D ? 2 : 3;
            }
            else
            {
                X = ctx.Rand(min, max);
                k = ctx.Rand(0, 3);
            }
            X = Math.Max(min, Math.Min(max, X));
            if (X < min + 0x30) right = true;
            else if (X >= max - 0x18) right = false;
            Y = 0x90;
            Vx = right ? Leaps[k].vx : -Leaps[k].vx;
            Vy = Leaps[k].vy;
            FacingLeft = !right;
            var s = ctx.SpawnChild(0x6A, X, Y);
            if (s != null) s.Param = 0;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (!placed) Place(ctx, Generator ?? new PeriodicJson());
            X += Vx;
            Y += (float)Math.Floor(Vy);
            Vy += 0.5f;
            if (Vy > 0 && Y >= 0x90)
            {
                Y = 0x90;
                Alive = false;
                ctx.SpawnChild(0x6A, X, Y);
            }
        }
    }

    // 0x21: cae a 2 px/tick hasta un suelo y anda a ±1 px/tick hacia el centro; gira en las paredes pero no en el
    // borde de la pantalla (sale por él).
    public class FallingWalker : Enemy
    {
        public override void Init(EnemyContext ctx) { FacingLeft = X >= 0x80; Walk(1); }

        public override void Tick(EnemyContext ctx)
        {
            if (!Solid(ctx, X + 2, Y + 16) && !Solid(ctx, X + 13, Y + 16))
            {
                Y += 2;
                if (Solid(ctx, X + 2, Y + 16) || Solid(ctx, X + 13, Y + 16)) Y = (int)Y & ~7;
                return;
            }
            if (Solid(ctx, FacingLeft ? X + 1 : X + 14, Y + 8)) Turn();
            X += Vx;
        }
    }

    // 0x35: piedra rodante a ±1,5 px/tick hacia el centro, gravedad 0,25 sin tope; rebota en las paredes (también en
    // el aire) pero no en los bordes; se posa con el centro sobre un sólido y vuelve a caer cuando le falta.
    public class RollingStone : Enemy
    {
        bool grounded;

        public override void Init(EnemyContext ctx) { FacingLeft = X >= 0x80; Walk(1.5f); }

        public override void Tick(EnemyContext ctx)
        {
            if (Solid(ctx, FacingLeft ? X - 1 : X + 16, Y + 8)) Turn();
            X += Vx;
            if (grounded)
            {
                if (!Solid(ctx, X + 8, Y + 16)) grounded = false;
                return;
            }
            Vy += 0.25f;
            Y += Vy;
            if (Vy > 0 && Solid(ctx, X + 8, Y + 16))
            {
                Y = (int)Y & ~7;
                Vy = 0;
                grounded = true;
            }
        }
    }

    // 0x30: criatura ancha (32 px) que sale del agua (y = 0x90) a 4 px/tick hasta una de 4 alturas, hace un arco de
    // 16 ticks, cae a 4 px/tick hasta un suelo y anda a ±2 px/tick; tras 2 giros, al volver a pasar por donde cayó
    // (o al 3.er giro) salta y cae atravesándolo todo hasta el agua.
    public class Wader : Enemy
    {
        static readonly int[] Heights = { 0x24, 0x44, 0x64, 0x84 };
        int phase, arc, turns;
        float target, landX;

        public override void Init(EnemyContext ctx)
        {
            Width = 32;
            target = Heights[ctx.Rand(0, 3)];
            var s = ctx.SpawnChild(0x6A, X + 4, Y);
            if (s != null) s.Param = 2;
        }

        bool FloorUnder(EnemyContext ctx)
        {
            bool any = false;
            for (int c = 0; c <= 32; c += 8)
            {
                if (Solid(ctx, X + Math.Min(c, 31), Y + 8)) return false;
                any |= Solid(ctx, X + Math.Min(c, 31), Y + 16);
            }
            return any;
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:   // sube
                    Y -= 4;
                    if (Y <= target) { Y = target; phase = 1; Vy = -4; arc = 16; }
                    break;
                case 1:   // arco
                case 4:
                    Y += Vy; Vy += 0.5f;
                    if (--arc > 0) break;
                    phase = phase == 1 ? 2 : 5;
                    break;
                case 2:   // cae hasta un suelo
                    Y += 4;
                    if (((int)Y & 7) == 0 && FloorUnder(ctx)) { phase = 3; landX = X; FacingLeft = ctx.Coin(); Walk(2); }
                    else if (Y >= 0x90) { Alive = false; ctx.SpawnChild(0x6A, X + 8, 0x90); }
                    break;
                case 3:   // anda
                {
                    bool ledge = !Solid(ctx, FacingLeft ? X - 1 : X + 32, Y + 16);
                    bool wall = Solid(ctx, FacingLeft ? X - 8 : X + 24, Y + 8);
                    bool edge = FacingLeft ? X < 0x10 : X >= 0xE0;
                    if (ledge || wall || edge)
                    {
                        Turn();
                        if (++turns >= 3) { Jump(); break; }
                    }
                    float before = X;
                    X += Vx;
                    if (turns >= 2 && (before - landX) * (X - landX) <= 0) Jump();
                    break;
                }
                case 5:   // cae atravesándolo todo hasta el agua
                    Y += 4;
                    if (Y >= 0x90) { Alive = false; ctx.SpawnChild(0x6A, X + 8, 0x90); }
                    break;
            }
        }

        public override bool ThroughFloors => phase != 3;

        void Jump() { phase = 4; Vy = -4; Vx = 0; arc = 16; }
    }

    // 0x3E: tejedor del mundo 6: determinista y sin mirar tiles. Sube o baja a 2 px/tick hasta una fila de una tabla,
    // luego ondula 14 ticks a ±1 px/tick; en cada paso invierte el sentido vertical. Gira en x 0x28 / 0xC8.
    // [I] La repetición de la onda en los pasos 2 y 7 se ha omitido; la onda se aplica en el sentido vertical actual.
    public class Weaver : Enemy
    {
        static readonly int[] Order = { 3, 1, 2, 3, 1, 3, 0, 1 };
        static readonly int[] Down = { 0x3C, 0x5C, 0x7C, 0x9C };
        static readonly int[] Up = { 0x2C, 0x4C, 0x6C, 0x8C };
        static readonly int[] Wave = { 3, 3, 2, 1, 1, 0, 0, 0, 0, 0, 0, -1, -1, -2 };
        int step, wave = -1;
        bool down = true;

        public override void Init(EnemyContext ctx) { FacingLeft = X >= 0x80; Walk(1); }

        public override void Tick(EnemyContext ctx)
        {
            if (wave < 0)
            {
                int k = Order[step & 7];
                float target = down ? Down[k] : Up[k];
                Y += down ? 2 : -2;
                if (down ? Y >= target : Y <= target) { Y = target; wave = 0; }
                return;
            }
            if (FacingLeft ? X < 0x28 : X >= 0xC8) Turn();
            X += Vx;
            Y += down ? Wave[wave] : -Wave[wave];
            if (++wave >= Wave.Length) { wave = -1; step++; down = !down; }
        }
    }
}
