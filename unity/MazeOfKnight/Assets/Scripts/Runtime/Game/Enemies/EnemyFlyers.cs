using System;
using System.Linq;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Enemigos que vuelan, flotan o recorren superficies, a partir de sus fichas (docs/enemy_ai.md).

    // Deambulador común (b04:8145; 0x19, 0x22, 0x27, 0x4C, 0x4E, 0x4F): rumbo al azar de 16 durante 3..31 ticks;
    // si entra en la franja derecha (x 0xC8..0xEF) o inferior (y 0x8F..0xDE) toma durante 24 ticks uno de 3 rumbos
    // hacia el centro según el cuadrante y sigue 255 más. No hay franja izquierda ni superior: el que sale por ahí
    // da la vuelta al byte y se borra (b06:B452) [C].
    public abstract class Wanderer : Enemy
    {
        static readonly int[][] ToCentre = { new[] { 13, 14, 15 }, new[] { 9, 10, 11 }, new[] { 3, 2, 1 }, new[] { 5, 6, 7 } };
        int wanderState, wanderTimer;
        protected bool BigBox;

        protected void Wander(EnemyContext ctx, float speed = 1)
        {
            switch (wanderState)
            {
                case 0:
                    Heading(ctx.Rand(0, 15), speed);
                    wanderTimer = ctx.Rand(3, 31);
                    wanderState = 1;
                    break;
                case 1:
                    bool outside = BigBox ? X >= 0xB8 && X < 0xF0 || Y >= 0x7F && Y < 0xDF
                                          : X >= 0xC8 && X < 0xF0 || Y >= 0x8F && Y < 0xDF;
                    if (outside)
                    {
                        int q = (Y >= 0x70 ? 2 : 0) + (X >= 0x80 ? 1 : 0);
                        Heading(ToCentre[q][ctx.Rand(0, 2)], speed);
                        wanderTimer = 24;
                        wanderState = 3;
                    }
                    else if (--wanderTimer <= 0)
                        wanderState = 0;
                    break;
                case 3:
                    if (--wanderTimer <= 0) { wanderState = 1; wanderTimer = 256; }
                    break;
            }
            FacingLeft = Vx < 0;
        }
    }

    // 0x02 chispa y 0x2B trepamuros: recorren el contorno de los bloques a 1 px/tick (suelo, pared, techo). Ante una
    // pared delante suben por ella; al acabarse la superficie doblan la esquina; tras cada giro no miran la
    // superficie durante 16 ticks. En el borde de la pantalla dan media vuelta. La chispa además da media vuelta cada
    // 80..127 ticks.
    public class Crawler : Enemy
    {
        int mx = 1, my, sx, sy = 1, settle, flip;

        public override void Init(EnemyContext ctx)
        {
            // [I] La dirección inicial la pone la creación (no se ve); se elige la superficie que tenga al lado.
            if (Solid(ctx, X + 4, Y + 16)) { sx = 0; sy = 1; }
            else if (Solid(ctx, X + 4, Y - 1)) { sx = 0; sy = -1; }
            else if (Solid(ctx, X + 16, Y + 4)) { sx = 1; sy = 0; }
            else if (Solid(ctx, X - 1, Y + 4)) { sx = -1; sy = 0; }
            mx = sy != 0 ? 1 : 0; my = sx != 0 ? -1 : 0;
            settle = 17;
            flip = ctx.Rand(80, 127);
        }

        bool EdgeSolid(EnemyContext ctx, int dx, int dy)
        {
            if (dx != 0)
            {
                float x = dx > 0 ? X + 16 : X - 1;
                return Solid(ctx, x, Y) || Solid(ctx, x, Y + 15);
            }
            float y = dy > 0 ? Y + 16 : Y - 1;
            return Solid(ctx, X, y) || Solid(ctx, X + 15, y);
        }

        public override void Tick(EnemyContext ctx)
        {
            bool atScreen = mx > 0 && X >= 0xF0 || mx < 0 && X < 8 || my > 0 && Y >= 0xA8 || my < 0 && Y < 0x20;
            if (atScreen) { mx = -mx; my = -my; settle = 16; }
            else if (EdgeSolid(ctx, mx, my))
            {
                // Sube por la pared: la nueva superficie es la que tenía delante.
                int nmx = -sx, nmy = -sy;
                sx = mx; sy = my; mx = nmx; my = nmy;
                settle = 16;
            }
            else if (settle > 0) settle--;
            else if (!EdgeSolid(ctx, sx, sy))
            {
                // Dobla la esquina.
                int nmx = sx, nmy = sy;
                sx = -mx; sy = -my; mx = nmx; my = nmy;
                settle = 16;
            }
            if (Type == 0x02 && settle == 0 && --flip <= 0) { mx = -mx; my = -my; flip = ctx.Rand(80, 127); }
            X += mx; Y += my;
            FacingLeft = mx < 0;
        }
    }

    // 0x15: guardián (40 de vida). Gira en un círculo de 31 px alrededor de un centro; la velocidad angular sube y
    // baja entre −8 y 8 (1 cada 60 ticks). Tras 128..255 ticks el centro se desplaza 48..111 ticks a 0,5 px/tick
    // (al azar; pasados ~1920 ticks, hacia el jugador). Dispara un 0x45 cada 32..95 ticks.
    public class Orbiter : Enemy
    {
        float cx = 0x80, cy = 0x60;
        int angle, speed = 1, speedDir = 1, speedTimer = 60, phase, timer, anger = 60, shot;
        float dcx, dcy;

        public override void Init(EnemyContext ctx)
        {
            timer = ctx.Rand(128, 255);
            shot = ctx.Rand(32, 95);
            Place();
        }

        void Place()
        {
            double a = angle * Math.PI * 2 / 256;
            X = cx + 31 * (float)Math.Cos(a);
            Y = cy + 31 * (float)Math.Sin(a);
        }

        public override void Tick(EnemyContext ctx)
        {
            angle = (angle + speed) & 255;
            if (--speedTimer <= 0)
            {
                speedTimer = 60;
                speed += speedDir;
                if (speed >= 8 || speed <= -8) speedDir = -speedDir;
            }
            if ((ctx.Tick & 0x1F) == 0 && anger > 0) anger--;
            if (phase == 0)
            {
                if (--timer <= 0)
                {
                    phase = 1; timer = ctx.Rand(48, 111);
                    if (anger > 0)
                    {
                        int dir = ctx.Rand(0, 15);
                        if (cy < 0x40) dir = 12 + ctx.Rand(-2, 2);
                        else if (cy >= 0x80) dir = 4 + ctx.Rand(-2, 2);
                        double a = (dir & 15) * Math.PI / 8;
                        dcx = (float)Math.Cos(a) * 0.5f; dcy = -(float)Math.Sin(a) * 0.5f;
                    }
                    else
                    {
                        float dx = ctx.Px - 8 - cx, dy = ctx.Py - 16 - cy, d = Math.Max(1, (float)Math.Sqrt(dx * dx + dy * dy));
                        dcx = dx / d * 0.5f; dcy = dy / d * 0.5f;
                    }
                }
            }
            else
            {
                if (speed != 0)
                {
                    cx = Math.Max(0x20, Math.Min(0xC0, cx + dcx));
                    cy = Math.Max(0x20, Math.Min(0x80, cy + dcy));
                }
                if (--timer <= 0) { phase = 0; timer = ctx.Rand(128, 255); }
            }
            Place();
            if (--shot <= 0) { shot = ctx.Rand(32, 95); ctx.SpawnChild(0x45, X + 8, Y + 8); }
        }
    }

    // 0x16: aparición (24 de vida). Parpadea 30 ticks, embiste en línea recta hacia el jugador a 2 px/tick (atraviesa
    // paredes) hasta salir de x 0x10..0xE0 / y 0x20..0xA0, parpadea 30, desaparece 30 dejando dos 0x46 y reaparece en
    // otro sitio.
    public class Phantom : Enemy
    {
        int phase, timer = 30;

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    Visible = (timer & 1) == 0;
                    if (--timer > 0) break;
                    Visible = true;
                    AimAtPlayer(ctx, 0x40);
                    FacingLeft = Vx < 0;
                    phase = 1;
                    break;
                case 1:
                    Move();
                    if (X < 0x10 || X > 0xE0 || Y < 0x20 || Y > 0xA0) { Vx = Vy = 0; phase = 2; timer = 30; }
                    break;
                case 2:
                    Visible = (timer & 1) == 0;
                    if (--timer > 0) break;
                    Visible = Harmful = false; phase = 3; timer = 30;
                    break;
                case 3:
                    if (--timer % 10 == 0 && timer > 0) ctx.SpawnChild(0x46, X, Y);
                    if (timer > 0) break;
                    X = ctx.Rand(48, 175); Y = (ctx.Tick & 0x7F) + 0x20;
                    Harmful = true; phase = 0; timer = 30;
                    break;
            }
        }
    }

    // 0x19: murciélagos que se fusionan (y 0x37, el grande). Deambulan a 1 px/tick; cada dos forman pareja y a los
    // 128 ticks se lanzan uno contra otro a 4 px/tick; al tocarse (a menos de 18 px) se funden en uno del nivel
    // siguiente (vida 1 → 2 → 4; el cuarto nivel es el 0x37). Tras la primera fusión de la sala disparan un 0x48 cada
    // 64 ticks. El daño depende del tamaño: 1, 2, 4, 8 (b03:B3AE).
    public class FusingBat : Wanderer
    {
        public int Level, Pair;
        bool hasPair = true, merge;
        int phase, fuse = 128, shot = 80;
        FusingBat partner;

        public override void Init(EnemyContext ctx)
        {
            if (Type == 0x37)
            {
                // El grande: no se empareja, vida 8, daño por contacto 0 (la tabla por tamaño solo vale para 0x19) [C].
                Level = 3; hasPair = false; shot = 48;
                Width = Height = 32; BigBox = true;
                return;
            }
            Level = ctx.FusionLevel;
            if (Level == 0) Pair = ctx.PairCounter++ >> 1;
            else { Pair = Param; phase = 6; }
            Hp = 1 << Level;
            Damage = 1 << Level;   // b03:B3AE: 1, 2, 4 según el tamaño
            if (Level >= 2) { Width = Height = 32; BigBox = true; }   // [I] tamaño de los sprites por nivel
        }

        public override void Tick(EnemyContext ctx)
        {
            if (ctx.FusionLevel != 0 && --shot <= 0) { shot = 64; ctx.SpawnChild(0x48, X + 16, Y + 16); }
            switch (phase)
            {
                case 0:
                    Wander(ctx);
                    Move();
                    if (hasPair && Type == 0x19 && --fuse <= 0) phase = 4;
                    break;
                case 4:
                    partner = ctx.Enemies.OfType<FusingBat>().FirstOrDefault(b => b != this && b.Alive && b.Type == 0x19 && b.Pair == Pair && b.Level == Level);
                    if (partner == null) { hasPair = false; phase = 0; break; }
                    AimAt(partner.X, partner.Y, 0x80);
                    phase = 5; ctx.Fusing = true;
                    break;
                case 5:
                    if (merge)
                    {
                        Alive = false;
                        ctx.FusionLevel = Level + 1;
                        var c = ctx.SpawnChild(Level + 1 >= 3 ? 0x37 : 0x19, X, Y);
                        if (c != null) c.Param = Pair;
                        break;
                    }
                    if (partner == null || !partner.Alive) { hasPair = false; phase = 0; break; }
                    Move();
                    if (Math.Abs(partner.X - X) < 18 && Math.Abs(partner.Y - Y) < 18)
                    {
                        Alive = false;
                        partner.merge = true;
                        partner.phase = 5;
                    }
                    break;
                case 6:
                    if (ctx.Enemies.OfType<FusingBat>().Any(b => b.Alive && b.phase == 5)) break;
                    Pair >>= 1; phase = 0; fuse = 128; ctx.Fusing = false;
                    break;
            }
        }
    }

    // 0x22: escupefuego (24 de vida). Deambula a 1 px/tick; cada 64 ticks abre o cierra la boca y, abierta, suelta
    // un 0x4A cada 4 ticks.
    public class FireSpitter : Wanderer
    {
        int mouth = 64, burst = 4;
        bool open;

        public override void Tick(EnemyContext ctx)
        {
            if (--mouth <= 0) { mouth = 64; open = !open; }
            if (--burst <= 0)
            {
                burst = 4;
                if (open) ctx.SpawnChild(0x4A, X + 8, Y);
            }
            else Wander(ctx);
            Move();
        }
    }

    // 0x26: fantasma que gira alrededor de donde aparece (6/256 de vuelta por tick) con un radio que crece de 0 a
    // 40 px a 1/16 px/tick, se queda 48 ticks, baja a 5 y repite. Dispara un 0x4B cada 16..63 ticks.
    public class Ghost : Enemy
    {
        float cx, cy, radius;
        int angle, phase, hold, shot;

        public override void Init(EnemyContext ctx) { cx = X; cy = Y; shot = ctx.Rand(16, 63); }

        public override void Tick(EnemyContext ctx)
        {
            angle = (angle + 6) & 255;
            if (hold > 0) { if (--hold == 0) phase ^= 1; }
            else if (phase == 0) { radius += 1 / 16f; if (radius >= 40) { radius = 40; hold = 48; } }
            else { radius -= 1 / 16f; if (radius <= 5) { radius = 5; hold = 48; } }
            double a = angle * Math.PI * 2 / 256;
            Y = cy + radius * (float)Math.Sin(a);
            X = cx + radius * (float)Math.Cos(a);
            if (--shot <= 0) { shot = ctx.Rand(16, 63); ctx.SpawnChild(0x4B, X + 8, Y + 8); }
        }
    }

    // 0x27: ave que se lanza en picado. Deambula a 1 px/tick; en los ratos de ataque (alterna cada 1..256 ticks), si
    // el jugador está cerca y por debajo, se queda 32 ticks rebotando arriba y abajo y luego se lanza hacia donde
    // estaba a 4 px/tick.
    public class DiveBird : Wanderer
    {
        int phase, toggle, timer;
        bool calm;
        float homeY, tx, ty;

        public override void Init(EnemyContext ctx) => toggle = ctx.Rand(0, 15);

        public override void Tick(EnemyContext ctx)
        {
            if (--toggle <= 0) { toggle = ctx.Rand(0, 15); if (toggle == 0) toggle = 256; calm = !calm; }
            switch (phase)
            {
                case 0:
                    if (!calm && Y >= 0x30 && ctx.Py > Y - 0x30 && ctx.Py <= Y + 0x40 && ctx.Px > X - 0x38 && ctx.Px <= X + 0x38)
                    {
                        phase = 4; timer = 32; homeY = Y; Vx = 0; Vy = 3;
                        break;
                    }
                    Wander(ctx);
                    Move();
                    break;
                case 4:
                    Vy += Y > homeY ? -0.5f : 0.5f;
                    Y += Vy;
                    if (--timer > 0) break;
                    tx = ctx.Px - 8; ty = ctx.Py - 16;
                    AimAt(tx, ty, 0x80);
                    FacingLeft = Vx < 0;
                    phase = 5;
                    break;
                case 5:
                    Move();
                    if (Math.Abs(X - tx) <= 0x14 && Math.Abs(Y - ty) <= 0x14 || Y < 0x20 || Y >= 0xA8 || X < 8 || X >= 0xF0)
                    {
                        phase = 0; calm = true;
                    }
                    break;
            }
        }
    }

    // 0x28: insecto que rebota: rumbo al azar de 16 a 3 px/tick, rebota en los bordes de la pantalla (no en los
    // tiles) y cambia de rumbo cada 7..70 ticks (16 tras un rebote).
    public class BounceBug : Enemy
    {
        int timer;

        public override void Init(EnemyContext ctx) { Heading(ctx.Rand(0, 15), 3); timer = ctx.Rand(7, 70); }

        public override void Tick(EnemyContext ctx)
        {
            Move();
            if (BounceIn(8, 0xE8, 0x28, 0xB0)) timer = 16;
            if (--timer <= 0) { Heading(ctx.Rand(0, 15), 3); timer = ctx.Rand(7, 70); }
            FacingLeft = Vx < 0;
        }
    }

    // 0x29: burbuja invulnerable. Aparece en un sitio al azar (x 64..191, y 64..127) y va en línea recta a
    // 1 px/tick rebotando en los bordes.
    public class Bubble : Enemy
    {
        int start = 1, dir;

        public override void Init(EnemyContext ctx)
        {
            X = ctx.Rand(0, 0x7F) + 0x40;
            Y = ctx.Rand(0, 0x3F) + 0x40;
            dir = ctx.Rand(0, 15);
        }

        public override void Tick(EnemyContext ctx)
        {
            if (start > 0 && --start == 0) Heading(dir, 1);
            Move();
            BounceIn(8, 0xE0, 0x20, 0xB0);
        }
    }

    // 0x2A: aparición que salta cada 16 ticks a otro sitio sobre una plataforma (si el sorteo falla, se queda
    // oculta hasta el siguiente intento).
    public class Teleporter : Enemy
    {
        int timer = 1;

        public override void Tick(EnemyContext ctx)
        {
            if (--timer > 0) return;
            timer = 16;
            bool ok = TryPlaceOnPlatform(ctx);
            Visible = Harmful = Hittable = ok;
        }
    }

    // 0x36: orbes en cadena (invulnerables). Cada una oscila sobre su casa (muelle de 0,5 px/tick²); la 9.ª vuela a
    // 6 px/tick a la casa de otra al azar, que al llegar sale a su vez hacia otra: un relevo sin fin.
    public class ChainOrb : Enemy
    {
        public int Index;
        public float HomeX, HomeY;
        public int State;
        float tx, ty;

        public override void Init(EnemyContext ctx)
        {
            HomeX = X; HomeY = Y;
            Vy = 2;
            Index = ctx.OrbCounter++;
            if (Index == 8) { Vy = 0; State = 1; }
            ctx.OrbLast = 8;
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (State)
            {
                case 0:
                    Vy += Y <= HomeY ? 0.5f : -0.5f;
                    Y += Vy;
                    break;
                case 1:
                {
                    int target;
                    do target = ctx.Rand(0, 7); while (target == ctx.OrbLast);
                    ctx.OrbLast = target;
                    var dest = ctx.Enemies.OfType<ChainOrb>().FirstOrDefault(o => o.Alive && o.Index == target);
                    if (dest == null) { State = 0; break; }
                    tx = dest.HomeX; ty = dest.HomeY;
                    AimAt(tx, ty, 0xC0);
                    State = 2;
                    break;
                }
                case 2:
                    Move();
                    if (Y > ty - 24 && Y <= ty + 24 && X > tx - 18 && X <= tx + 18)
                    {
                        X = HomeX = tx; Y = HomeY = ty; Vx = 0; Vy = 2; State = 0;
                        var evicted = ctx.Enemies.OfType<ChainOrb>().FirstOrDefault(o => o.Alive && o.Index == ctx.OrbLast && o != this);
                        if (evicted != null) evicted.State = 1;
                    }
                    break;
            }
        }
    }

    // 0x3C: hechicero. Visible 64 ticks (dispara un 0x3D cada 32), parpadea 8, desaparece y reaparece sobre una
    // plataforma al azar.
    public class Sorcerer : Enemy
    {
        int phase, timer = 64, shot = 32;

        public override void Tick(EnemyContext ctx)
        {
            switch (phase)
            {
                case 0:
                    if (--shot <= 0) { shot = 32; ctx.SpawnChild(0x3D, X + 8, Y + 8); }
                    if (--timer <= 0) { phase = 1; timer = 8; }
                    break;
                case 1:
                    Visible = Harmful = Hittable = (timer & 1) == 0;
                    if (--timer <= 0) { Visible = Harmful = Hittable = false; phase = 2; }
                    break;
                case 2:
                    if (!TryPlaceOnPlatform(ctx)) break;
                    Visible = Harmful = Hittable = true; phase = 0; timer = 64;
                    break;
            }
        }
    }

    // 0x40: espíritu grande (35 de vida, 32 px [I]). Va hacia el jugador a 0,5 px/tick (vuelve a apuntar cada 60
    // ticks), atraviesa paredes y rebota en x 0x10..0xE0 / y 0x20..0xA0. Cada 120 ticks se para 30 y suelta un 0x41.
    public class BigSpirit : Enemy
    {
        int phase, timer = 120, aim = 60;

        public override void Init(EnemyContext ctx) { Width = Height = 32; AimAtPlayer(ctx, 0x10); }

        public override void Tick(EnemyContext ctx)
        {
            if (phase == 1)
            {
                if (--timer > 0) return;
                phase = 0; timer = 120;
                ctx.SpawnChild(0x41, X, Y);
                AimAtPlayer(ctx, 0x10);
                return;
            }
            Move();
            if (X < 0x10 || X > 0xE0 || Y < 0x20 || Y > 0xA0) { Vx = -Vx; Vy = -Vy; }
            else if (--aim <= 0) { aim = 60; AimAtPlayer(ctx, 0x10); }
            FacingLeft = Vx < 0;
            if (--timer <= 0) { phase = 1; timer = 30; }
        }
    }

    // 0x49 mosca (vida 5), 0x52 (3) y 0x53 (1): rumbo al azar de 16 a 2 px/tick cada 16..31 ticks, rebotando en
    // x 8..0xE0 / y 0x20..0xB0. Un golpe que no la mata (IX+1C, b03:B54D) divide la 0x49 en dos 0x52 y cada 0x52 en
    // dos 0x53, que ya solo mueren. Las nuevas no se pueden golpear durante 8 ticks (la 0x49, 1).
    public class SplitFly : Enemy
    {
        int timer = 1, shield;

        public override void Init(EnemyContext ctx) { shield = Type == 0x49 ? 1 : 8; Hittable = false; }

        public override void Tick(EnemyContext ctx)
        {
            if (shield > 0 && --shield == 0) Hittable = true;
            if (--timer <= 0) { timer = 16 + ctx.Rand(0, 15); Heading(ctx.Rand(0, 15), 2); }
            Move();
            BounceIn(8, 0xE0, 0x20, 0xB0);
            FacingLeft = Vx < 0;
        }

        public override void Hurt(EnemyContext ctx)
        {
            if (Type == 0x53) return;
            Alive = false;
            int child = Type == 0x49 ? 0x52 : 0x53;
            for (int p = 1; p >= 0; p--)
            {
                var c = ctx.SpawnChild(child, X, Y);
                if (c != null) c.Param = p;
            }
        }
    }

    // 0x4C: nube que deambula a 1 px/tick y suelta un 0x4D cada 16 ticks.
    public class SporeCloud : Wanderer
    {
        int spore = 16;

        public override void Tick(EnemyContext ctx)
        {
            if (--spore <= 0) { spore = 16; ctx.SpawnChild(0x4D, X + 16, Y + 16); return; }
            Wander(ctx);
            Move();
        }
    }

    // 0x4E: almas (no dañan ni se pueden golpear). Deambulan a 1 px/tick; si el jugador se queda 112 ticks en un
    // punto concreto de la sala, vuelan al centro (0x80, 0x70) a 4 px/tick y desaparecen; con la última aparece un
    // 0x4F. [I] El efecto de 4086/5D12 (la primera vez) no se ha seguido.
    public class Soul : Wanderer
    {
        int wait = 112;
        bool gathering;

        public override void Init(EnemyContext ctx) { Harmful = Hittable = false; }

        bool PlayerOnSpot(EnemyContext ctx)
        {
            int id = Room.id;
            if (id == 0x6E) return ctx.Px >= 0xC0 && ctx.Px <= 0xCF && ctx.Py == 0x58;
            if (id == 0x19) return ctx.Px >= 0x60 && ctx.Px <= 0x6F && ctx.Py == 0x58;
            return ctx.Px >= 0x60 && ctx.Px <= 0x6F && ctx.Py == 0x78;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (gathering)
            {
                Move();
                if (Y >= 0x66 && Y < 0x7A && X >= 0x76 && X < 0x8A)
                {
                    Alive = false;
                    if (ctx.AliveCount == 0) ctx.SpawnChild(0x4F, 0x80, 0x70);
                }
                return;
            }
            if (PlayerOnSpot(ctx))
            {
                if (--wait <= 0) { ctx.SoulsCalled = true; gathering = true; AimAt(0x80, 0x70, 0x80); return; }
            }
            else wait = 112;
            Wander(ctx);
            Move();
        }
    }

    // 0x69: cabezas flotantes en formación (invulnerables). Derivan a 0,125 px/tick y oscilan ±2 px sobre su altura.
    // La primera dirige los turnos: cada 16 ticks (40 al principio) una cabeza (de las 4 primeras) destella 24 ticks
    // y lanza un 0x06, al azar o la más cercana al jugador apuntando hacia él.
    public class FloatingHead : Enemy
    {
        int index, timer = 40, turns, mode, flash, dir;
        float homeY;

        public override void Init(EnemyContext ctx)
        {
            index = ctx.HeadCounter++;
            if (index == 0) { ctx.HeadPhase = 0; turns = ctx.Rand(1, 7); }
            homeY = Y;
            Vy = ctx.Coin() ? 0.5f : -0.5f;
            Vx = ctx.Coin() ? 0.125f : -0.125f;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (X < 0x10 || X >= 0xD8) Vx = -Vx;
            Vy += Y <= homeY ? 1 / 16f : -1 / 16f;
            Move();
            if (index == 0 && ctx.HeadPhase == 0 && --timer <= 0) Choose(ctx);
            if (ctx.HeadPhase == 1 && ctx.HeadShooter == index)
            {
                if (--flash > 0) return;
                ctx.HeadPhase = 0;
                var leader = ctx.Enemies.OfType<FloatingHead>().FirstOrDefault(h => h.index == 0);
                if (leader != null) leader.timer = 16;
                var c = ctx.SpawnChild(0x06, X + 8, Y + 10);
                if (c != null) c.Param = dir;
            }
        }

        void Choose(EnemyContext ctx)
        {
            int count = ctx.Enemies.OfType<FloatingHead>().Count(h => h.Alive);
            ctx.HeadPhase = 1;
            if (--turns <= 0) { mode ^= 1; turns = mode == 0 ? ctx.Rand(1, 7) : ctx.Rand(1, 3); }
            FloatingHead shooter;
            if (mode == 0)
            {
                int i = ctx.Rand(0, 3);
                if (i >= count) i = 0;
                shooter = ctx.Enemies.OfType<FloatingHead>().FirstOrDefault(h => h.index == i);
                if (shooter != null) shooter.dir = ctx.Rand(0, 9);
            }
            else
            {
                shooter = ctx.Enemies.OfType<FloatingHead>().Where(h => h.index < 4 && h.Alive)
                    .OrderBy(h => Math.Abs(ctx.Px - 8 - h.X)).FirstOrDefault();
                if (shooter != null)
                {
                    float dx = ctx.Px - 8 - shooter.X;
                    // [I] Tabla 7626 no reconstruida: 0..4 a la izquierda, 5..9 a la derecha, según lo lejos que esté.
                    int k = Math.Min(4, (int)(Math.Abs(dx) / 24));
                    shooter.dir = dx < 0 ? k : 5 + k;
                }
            }
            if (shooter == null) { ctx.HeadPhase = 0; timer = 16; return; }
            ctx.HeadShooter = shooter.index;
            shooter.flash = 24;
        }

        public bool Flashing => flash > 0;
    }
}
