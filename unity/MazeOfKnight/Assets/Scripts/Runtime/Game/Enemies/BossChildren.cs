using System;
using System.Linq;

namespace MazeOfKnight.Game.Enemies
{
    // Hijos de los jefes (docs/boss_ai.md, "Hijos de los jefes"). Por tick.

    // 0x63: chispa de la invocación. Ni toca ni se puede golpear; dura 80 ticks.
    public class SummonSpark : Enemy
    {
        int life = 80;
        public override void Init(EnemyContext ctx) { Harmful = Hittable = false; }
        public override void Tick(EnemyContext ctx) { Visible = (life & 2) == 0; if (--life <= 0) Alive = false; }
    }

    // 0x54 hueso (Bony Dragon) y 0x55 medio hueso: parábola siempre hacia la izquierda, gravedad 0,1875, atraviesan
    // paredes. Un golpe parte el hueso en dos 0x55 (variantes 1 y 2); el medio hueso no se puede golpear 8 ticks.
    // Gira cambiando de sprite cada 2 ticks con la caja de la tabla 6CC4 (como el 0x18): 1E, 0B, 0B, 1F, 25, 0B, 1E.
    // [I] El orden del giro se toma como el del 0x18 hacia delante.
    public class Bone : Enemy
    {
        static readonly (float vx, float vy)[] Variants = { (-2.25f, -4f), (-1.1875f, -3.75f), (-0.125f, -3.5f) };
        static readonly int[] FrameBoxes = { 0x1E, 0x0B, 0x0B, 0x1F, 0x25, 0x0B, 0x1E };
        int shield, frame, anim;

        public override void Init(EnemyContext ctx)
        {
            int v = Type == 0x55 ? 1 + (Param & 1) : Math.Max(0, Math.Min(2, Param));
            Vx = Variants[v].vx; Vy = Variants[v].vy;
            if (Type == 0x55) { shield = 8; Hittable = false; }
            FacingLeft = true;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (shield > 0 && --shield == 0) Hittable = true;
            if (++anim >= 2) { anim = 0; frame = (frame + 1) % FrameBoxes.Length; }
            SetBox(ctx, FrameBoxes[frame]);
            Vy += 0.1875f;
            Move();
        }

        public override void Hurt(EnemyContext ctx)
        {
            if (Type != 0x54) return;
            Alive = false;
            for (int b = 0; b <= 1; b++) { var c = ctx.SpawnChild(0x55, X, Y); if (c != null) { c.Param = b; c.Init(ctx); } }
        }
    }

    // 0x57: llamarada en fila de tiles (Elusive Dragon, King Dragon, Donra). Crece 8 px por tick: B = 0 hacia la
    // derecha (x fija), B = 1 y 2 hacia la izquierda (el extremo derecho fijo). Vive 12 ticks (B = 0, 1) u 8 (B = 2).
    public class FlameRow : Enemy
    {
        int n, life;
        float rightEnd;

        public override void Init(EnemyContext ctx)
        {
            Invulnerable = true;
            life = Param == 2 ? 8 : 12;
            rightEnd = X + 8;
            Height = 8; Width = 8; n = 1;
            SetBox(0, 0, 8, 8 * n);   // caja calculada (`row`, b03:B926): alto 8, ancho 8·n
        }

        public override void Tick(EnemyContext ctx)
        {
            if (--life <= 0) { Alive = false; return; }
            n++;
            Width = 8 * n;
            if (Param != 0) X = rightEnd - Width;
            SetBox(0, 0, 8, 8 * n);
        }
    }

    // 0x58: tiro en abanico (Arlyane): rumbo al jugador + (B − 3)·8 + (−8…7) unidades de ángulo (256 por vuelta), a
    // 3,5 px/tick en línea recta, atravesando paredes. Anima cada 4 ticks con la caja 6F ↔ 0D (IX+11 ^ 62).
    public class FanShot : Enemy
    {
        int anim;

        public override void Init(EnemyContext ctx)
        {
            DiesOnTouch = true;
            double a = Math.Atan2(ctx.Py - 16 - Y, ctx.Px - 8 - X);
            a += ((Param - 3) * 8 + ctx.Rand(-8, 7)) * Math.PI * 2 / 256;
            Vx = (float)Math.Cos(a) * 3.5f;
            Vy = (float)Math.Sin(a) * 3.5f;
            SetBox(ctx, 0x6F);
        }

        public override void Tick(EnemyContext ctx)
        {
            if (++anim >= 4) { anim = 0; if (BoxIndex >= 0) SetBox(ctx, BoxIndex ^ 0x62); }
            Move();
        }
    }

    // 0x59: espora (Sea Anemone): vy y |vx| de tablas por i = B >> 1, vx negativa con B impar; gravedad 0,5.
    public class Spore2 : Enemy
    {
        static readonly float[] Vys = { -4.5f, -8f, -7.5f, -7f, -6.5f, -6f, -5.5f, -5f };
        static readonly float[] Vxs = { 3, 0, 2, 3, 1, 4, 3, 2 };

        public override void Init(EnemyContext ctx)
        {
            DiesOnTouch = true;
            int i = (Param & 15) >> 1;
            Vy = Vys[i];
            Vx = (Param & 1) == 1 ? -Vxs[i] : Vxs[i];
        }

        public override void Tick(EnemyContext ctx) { Vy += 0.5f; Move(); }
    }

    // 0x65: tiro de Big Lip, en línea recta en 8 direcciones según dónde esté el jugador respecto al jefe (rejilla 3×3
    // de 0x40 px; la casilla central dispara hacia abajo).
    public class LipShot : Enemy
    {
        static readonly (float vx, float vy)[] Cells =
            { (-1.41f, -1.41f), (-2, 0), (-1.41f, 1.41f), (0, -2), (0, 2), (0, 2), (1.41f, -1.41f), (2, 0), (1.41f, 1.41f) };

        public override void Init(EnemyContext ctx)
        {
            DiesOnTouch = true;
            float bx = X - 0x18, by = Y - 0x18;
            int col = ctx.Px <= bx ? 0 : ctx.Px <= bx + 0x40 ? 1 : 2;
            int row = ctx.Py <= by ? 0 : ctx.Py <= by + 0x40 ? 1 : 2;
            (Vx, Vy) = Cells[3 * col + row];
        }

        public override void Tick(EnemyContext ctx) => Move();
    }

    // 0x68: bola de Monster. Sale en diagonal (rotando arriba-izquierda, arriba-derecha, abajo-derecha, abajo-izquierda)
    // a 4 px/tick por eje tras 8 ticks quieta, rebota en los tiles sólidos y en y < 0x20, y desaparece a los 192 ticks.
    public class MonsterBall : Enemy
    {
        static int rotation;
        static readonly (int vy, int vx)[] Diagonals = { (-4, -4), (-4, 4), (4, 4), (4, -4) };
        int age;

        public override void Init(EnemyContext ctx)
        {
            DiesOnTouch = true;
            var d = Diagonals[rotation++ & 3];
            Vx = d.vx; Vy = d.vy;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (++age >= 192) { Alive = false; return; }
            if (age <= 8) return;
            if (age == 9) { X = (int)X & ~7; Y = (int)Y & ~7; }
            if (Solid(ctx, Vx > 0 ? X + 16 : X - 1, Y + 8)) Vx = -Vx;
            if (Vy < 0 && (Y + Vy < 0x20 || Solid(ctx, X + 8, Y - 1)) || Vy > 0 && Solid(ctx, X + 8, Y + 16)) Vy = -Vy;
            Move();
        }
    }

    // 0x67: bola de Galious (16 de vida, se puede destruir). Gira alrededor del centro de Galious (+8 de ángulo por
    // tick, radio 24..50 que se abre o se cierra ±0,4375 por tick). Galious las lanza de 4 en 4: cada una sale cuando
    // su ángulo coincide con 0xC0 − 0x10·dir, su centro avanza 9 ticks a 8 px/tick, vuelve 7 y se reengancha.
    public class GaliousOrb : Enemy
    {
        int angle;
        float radius = 50, ox, oy;
        int launchDir = -1, launchStep;
        public bool Launched => launchDir >= 0;
        public override bool KeepOffScreen => true;

        public override void Init(EnemyContext ctx) { angle = Param & 255; }

        public void Launch(int dir) { launchDir = dir & 15; launchStep = -1; }

        public override void Tick(EnemyContext ctx)
        {
            var g = ctx.Enemies.OfType<Galious>().FirstOrDefault(b => b.Alive);
            if (g == null) { Alive = false; return; }
            angle = (angle + 8) & 255;
            if (g.OrbMode == 1) { Visible = Harmful = Hittable = false; return; }
            radius = g.OrbRadiusSpeed > 0x80 ? Math.Max(24, radius - 0.4375f) : Math.Min(50, radius + 0.4375f);
            if (Launched)
            {
                if (launchStep < 0) { if (angle == ((0xC0 - 0x10 * launchDir) & 255)) launchStep = 0; }
                else
                {
                    double a = launchDir * Math.PI / 8;
                    float sx = (float)Math.Cos(a) * 8, sy = -(float)Math.Sin(a) * 8;
                    if (launchStep < 9) { ox += sx; oy += sy; }
                    else { ox -= sx; oy -= sy; }
                    if (++launchStep >= 16) { launchDir = -1; ox = oy = 0; }
                }
            }
            double t = angle * Math.PI * 2 / 256;
            X = g.CenterX + ox + radius * (float)Math.Sin(t + Math.PI / 2) - 8;
            Y = g.CenterY + oy + radius * (float)Math.Sin(t) - 8;
            bool inside = Y >= 8 && Y <= 0xB7;
            Visible = Harmful = Hittable = inside;
        }
    }
}

namespace MazeOfKnight.Game.Enemies
{
    // Final del juego (docs/ending.md): mundo 10, sala 6.

    // 0x5C: piedra de la sala del bebé. Quieta y sin daño, 5 de vida. Al romperla deja al bebé (si es la elegida) o
    // un fantasma 0x39, sin explosión, experiencia ni botín.
    public class BabyRock : Enemy
    {
        public bool HasBaby;

        public override void Init(EnemyContext ctx) { Harmful = false; }

        public override void Tick(EnemyContext ctx) { }

        public override void Die(EnemyContext ctx) => ctx.SpawnChild(HasBaby ? 0x6B : 0x39, X, Y);
    }

    // 0x39: fantasma que sale de una piedra vacía. Espiral que se abre alrededor de (y, x ± 10): +8/256 de vuelta y
    // +0,5 px de radio por tick; desaparece al salir de la zona visible. 40 de vida, 14 de daño.
    public class SpiralGhost : Enemy
    {
        float cx, cy, radius;
        int angle;

        public override void Init(EnemyContext ctx) { cy = Y; cx = X + (ctx.Coin() ? 10 : -10); }

        public override void Tick(EnemyContext ctx)
        {
            angle = (angle + 8) & 255;
            radius += 0.5f;
            double t = angle * System.Math.PI * 2 / 256;
            X = cx + radius * (float)System.Math.Sin(t + System.Math.PI / 2);
            Y = cy + radius * (float)System.Math.Sin(t);
            if (X < 0 || X > 0xF0 || Y < OriginalPhysics.RoomTopPx || Y > OriginalPhysics.RoomBottomPx - 16) Alive = false;
        }
    }

    // 0x6B: el bebé (Pampas). Quieto, no se puede golpear ni hace daño; al tocarlo empieza el final.
    public class Baby : Enemy
    {
        public override void Init(EnemyContext ctx) { Harmful = Hittable = false; }
        public override void Tick(EnemyContext ctx) { }
    }
}
