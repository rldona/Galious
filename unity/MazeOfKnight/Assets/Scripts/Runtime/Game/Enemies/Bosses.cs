using System;
using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game.Enemies
{
    // Los jefes (docs/boss_ai.md). (X, Y) = (E707, E705). Una actualización por tick, como los enemigos. 'Sprite' es el
    // fotograma (E70A): elige la caja de colisión y, en algunos, si se le puede golpear.
    public abstract class Boss : Enemy
    {
        public int BossId;               // E700: 1 Bony Dragon … 11 Jump Slime (el 5 no se usa)
        public BossInfo Info;
        public int MaxHp;
        public int WeaponMask;           // b03:BD0A: bit 0 espada, 1 mina, 2 fuego rodante, 3 fuego, 4 cerámica, 5 flecha
        public int Sprite;
        public int Phase;
        protected int Timer;

        // Prueba de choque propia del jefe (tabla b03:BB7A): además de su caja, ¿se le puede dar ahora?
        public virtual bool CanBeHitNow => Hittable && Visible;

        public override bool KeepOffScreen => true;

        // Golpe válido (para el King Dragon: retrocede).
        public virtual void OnBossHit(EnemyContext ctx) { }

        // Máscara de un arma: 0 = espada; 1 flecha, 2 cerámica, 3 fuego rodante, 4 fuego, 5 mina.
        public static int Bit(int weapon) => weapon switch { 0 => 0x01, 1 => 0x20, 2 => 0x10, 3 => 0x04, 4 => 0x08, _ => 0x02 };

        public bool Vulnerable(int weapon) => (WeaponMask & Bit(weapon)) != 0;

        // Grado de daño (E1C5): 0 por encima de 1/4 de la vida máxima, 1 hasta 1/8, 2 por debajo.
        public int DamageLevel => Hp > MaxHp / 4 ? 0 : Hp > MaxHp / 8 ? 1 : 2;

        protected int R(EnemyContext ctx) => ctx.Random.Next(256);

        // ---------- cajas (docs/boss_ai.md, "Cajas de los jefes") ----------

        protected static (float, float, float, float) Rect(float x, float y, BossBox b) => (x + b.dx, y + b.dy, x + b.dx + b.w + 1, y + b.dy + b.h + 1);

        protected BossSprite CurrentSprite => Info?.Sprite(Sprite);

        public override IEnumerable<(float x0, float y0, float x1, float y1)> ContactRects()
        {
            var s = CurrentSprite;
            if (s == null) { yield return Box(); yield break; }
            if (s.contactBox.valid) yield return Rect(X, Y, s.contactBox);
            if (s.contactBox2.valid) yield return Rect(X, Y, s.contactBox2);
        }

        public override IEnumerable<(float x0, float y0, float x1, float y1)> WeaponRects()
        {
            var s = CurrentSprite;
            if (s == null) yield break;
            foreach (var b in new[] { s.weaponBox, s.weaponBox2, s.weaponBox3 })
                if (b.valid) yield return Rect(X, Y, b);
        }

        public override (float x0, float y0, float x1, float y1) DrawRect()
        {
            var s = CurrentSprite;
            return s == null ? Box() : (X, Y, X + s.w, Y + s.h);
        }

        // Retroceso al tocar su cuerpo (b03:BA6E): a la derecha si x_jefe + ref < x_jugador − 6.
        public int KnockbackDir(float playerX) => X + (CurrentSprite?.knockbackRefDx ?? 0x20) < playerX - 6 ? 1 : -1;

        // Integra la velocidad como b06:B0ED (si el bit 4 está activo).
        protected bool Moving = true;
        protected void Integrate() { if (Moving) { X += Vx; Y += Vy; } }
    }

    // Jefe 1 — Bony Dragon (mundo 1). Pasea 14 px a 0,5 px/tick; cada 80..207 ticks salta (vy −7, g 0,5) hasta una
    // altura al azar, lanza 3 huesos 0x54 en la cima o cada 16..47 ticks, y descansa 60 al caer.
    public class BonyDragon : Boss
    {
        int jumpTimer, boneTimer, lands;
        float targetY;
        const float Floor = 0x68;

        public override void Init(EnemyContext ctx)
        {
            Y = Floor; X = 0x98; Sprite = 0x0C;
            jumpTimer = 0x50 + (R(ctx) & 0x7F);
            boneTimer = (R(ctx) & 0x1F) + 0x10;
            Vx = ctx.Px < X ? -0.5f : 0.5f;
        }

        void Bones(EnemyContext ctx)
        {
            for (int b = 2; b >= 0; b--) { var c = ctx.SpawnChild(0x54, X, Y); if (c != null) c.Param = b; }
        }

        public override void Tick(EnemyContext ctx)
        {
            switch (Phase)
            {
                case 0:
                    Sprite = (ctx.Tick & 0x10) == 0 ? (Vx > 0 ? 0x0E : 0x0D) : 0x0C;
                    if (--boneTimer <= 0) { Bones(ctx); boneTimer = (R(ctx) & 0x1F) + 0x10; }
                    if (Vx < 0 && X < 0x92 || Vx > 0 && X >= 0xA0) Vx = -Vx;
                    X += Vx;
                    if (--jumpTimer <= 0) { Phase = 1; Sprite = 0x0D; Vx = 0; Vy = -7; targetY = ((R(ctx) & 3) + 3) * 16 + 5; }
                    break;
                case 1:
                    if ((ctx.Tick & 3) == 0) Sprite ^= 2;
                    Vy += 0.5f;
                    if (Vy < 0 && Y < targetY) { Vy = -Vy; Bones(ctx); }
                    Y += Vy;
                    if (Vy > 0 && Y >= Floor)
                    {
                        Y = Floor; Vy = 0; Sprite = 0x0C; Phase = 2; Timer = 60;
                        jumpTimer = 0x50 + (R(ctx) & 0x7F);
                    }
                    break;
                case 2:
                    if (--Timer > 0) break;
                    Phase = 0;
                    lands++;
                    bool towards = (lands & 3) != 0;
                    Vx = (towards ? ctx.Px < X : ctx.Coin()) ? -0.5f : 0.5f;
                    break;
            }
        }
    }

    // Jefe 2 — Elusive Dragon (mundo 3). Asoma por la izquierda (crece el cuello 8 px cada 4 ticks) o por la derecha (a
    // 2 px/tick) a una de 5 alturas, abre la boca 30 ticks, escupe una llamarada 0x57 durante 96 y se esconde
    // (invisible e invulnerable) para reaparecer en otro sitio. [I] En el lado derecho la longitud se cuenta aquí.
    public class ElusiveDragon : Boss
    {
        static readonly int[] Heights = { 0x20, 0x40, 0x60, 0x80, 0xA0, 0x20, 0xA0, 0x40 };
        static readonly int[] Jitter = { 0, 2, 1, 0, 1, 0, 2, 1 };
        int appearances, length, slot;
        bool right;

        public int Length => length;
        public bool RightSide => right;

        public override void Init(EnemyContext ctx) => Choose(ctx);

        void Choose(EnemyContext ctx)
        {
            int band = ctx.Py < 0x40 ? 0 : ctx.Py < 0x60 ? 1 : ctx.Py < 0x80 ? 2 : ctx.Py < 0xA0 ? 3 : 4;
            switch ((appearances >> 2) & 3)
            {
                case 0: slot = band * 2 + (ctx.Player.FacingLeft ? 1 : 0); break;
                case 1: slot = Math.Max(1, Math.Min(4, band - 1 + Jitter[R(ctx) & 7])) * 2 + (ctx.Px >= 0x80 ? 1 : 0); break;
                case 2: slot = (ctx.Py < 0x70 ? 0 : 4) * 2 + (ctx.Px < 0x80 ? 1 : 0); break;
                default: slot = R(ctx) & 15; break;
            }
            right = (slot & 1) == 1;
            Y = Heights[(slot >> 1) & 7];
            X = right ? 0xF8 : 0;
            length = right ? 1 : 1;
            Sprite = right ? 6 : 8;
            Visible = Hittable = true;
            Harmful = true;
            Phase = 0; Timer = 0;
            Width = 16;
        }

        // Cabeza (x de la punta): izquierda, x + 8·longitud; derecha, x.
        public float HeadX => right ? X : X + 8 * length;

        public override void Tick(EnemyContext ctx)
        {
            Timer++;
            switch (Phase)
            {
                case 0:
                    if ((Timer & 7) == 0) Sprite ^= 1;
                    if (right) { X -= 2; if ((Timer & 3) == 0) length++; }
                    else if ((Timer & 3) == 0) length++;
                    Width = 8 * length + 8;
                    if (right) Width = (int)(0x100 - X);
                    float dist = right ? HeadX - ctx.Px : ctx.Px - HeadX;
                    bool ahead = ctx.Py - Y >= 0 && ctx.Py - Y < 25 && dist - 8 >= 0 && dist - 8 < 104;
                    if (ahead && length >= 12)
                    {
                        int r = R(ctx) & 3;
                        if (r == 1 || r == 3) Open();
                        else if (r == 2) Phase = 3;
                    }
                    else if (length >= 16) Open();
                    break;
                case 1:
                    if (Timer >= 30)
                    {
                        Phase = 2; Timer = 0;
                        var c = ctx.SpawnChild(0x57, right ? ((int)X & ~7) - 16 : HeadX + 8, Y + 8);
                        if (c != null) c.Param = right ? 1 : 0;
                    }
                    break;
                case 2:
                    if (Timer >= 96) { Phase = 3; Timer = 0; Sprite = right ? 6 : 8; }
                    break;
                case 3:
                    if (right) { X += 2; length = Math.Max(1, length - ((Timer & 3) == 0 ? 1 : 0)); Width = (int)Math.Max(8, 0x100 - X); }
                    else if ((Timer & 3) == 0) length--;
                    if (!right) Width = 8 * length + 8;
                    if (right ? X >= 0xF8 : length < 2)
                    {
                        Visible = Hittable = Harmful = false;
                        Phase = 4; Timer = 0;
                    }
                    break;
                case 4:
                    if (Timer >= 30) { appearances++; Choose(ctx); }
                    break;
            }
        }

        void Open() { Phase = 1; Timer = 0; Sprite = right ? 10 : 11; }

        // Cajas calculadas (b03:BB90/BC15/BDD7): n tiles de ancho desde xa = x & F8; contacto en todo el cuello (40 px
        // de alto), armas solo en los 64 px del lado de la cabeza (y hasta 24 px por encima de la caja del arma).
        int Tiles => right ? Math.Max(1, (int)Math.Ceiling((0x100 - X) / 8f)) : length + 1;
        public override IEnumerable<(float x0, float y0, float x1, float y1)> ContactRects()
        {
            float xa = (int)X & ~7;
            yield return (xa, Y, xa + 8 * Tiles + 8, Y + 40);
        }
        public override IEnumerable<(float x0, float y0, float x1, float y1)> WeaponRects()
        {
            float xa = (int)X & ~7;
            int n = Tiles;
            if (right) yield return (xa - 8, Y - 24, xa - 8 + Math.Min(8 * n, 64), Y + 24);
            else yield return (xa + Math.Max(8 * n - 64, 0), Y - 24, xa + 8 * n, Y + 24);
        }
        public override (float x0, float y0, float x1, float y1) DrawRect()
        {
            float xa = (int)X & ~7;
            return (xa, Y, xa + 8 * Tiles, Y + 24);
        }

        public override bool CanBeHitNow => Visible && Hittable;
    }

    // Jefe 3 — Arlyane (mundo 6). Camina a 1 px/tick en x 0x29..0x8C (sigue al jugador si está a su altura o
    // debajo) y cada 31..158 ticks se para y lanza 0..3 ráfagas de 6 tiros 0x58 separadas 16 ticks.
    public class Arlyane : Boss
    {
        int walk, wander;

        public override void Init(EnemyContext ctx)
        {
            walk = (((R(ctx) & 0x7F) + 0x50) & 0x7F) + 0x1F;
            Y = 0x90; X = 0x60; Sprite = 0x3A;
            wander = R(ctx) & 0x1F;
            Vx = ctx.Coin() ? -1 : 1;
        }

        public override void Tick(EnemyContext ctx)
        {
            if (Phase == 0)
            {
                if (X < 0x29) { X = 0x29; Vx = 1; }
                if (X > 0x8C) { X = 0x8C; Vx = -1; }
                if ((ctx.Tick & 7) == 0) { int k = (ctx.Tick >> 3) & 3; Sprite = 0x3A + (k == 3 ? 1 : k); }
                if (ctx.Py >= 0x90)
                {
                    if ((ctx.Tick & 0x1F) == 0) Vx = ctx.Px - 8 >= ((int)X & ~7) + 0x28 ? 1 : -1;
                }
                else if (--wander <= 0) { wander = R(ctx) & 0x1F; if (wander == 0) wander = 256; Vx = ctx.Coin() ? 1 : -1; }
                X += Vx;
                if (--walk <= 0) { Phase = 1; Sprite = 0x3C; walk = ((((R(ctx) & 3) + 1) * 16) & 0x30) + 0x0F; }
                return;
            }
            --walk;
            if (walk > 0 && (walk & 0x0F) == 0)
                for (int b = 5; b >= 0; b--) { var c = ctx.SpawnChild(0x58, X + 0x20, Y + 0x10); if (c != null) c.Param = b; }
            if (walk <= 0) { Phase = 0; walk = (((R(ctx) & 0x7F) + 0x50) & 0x7F) + 0x1F; }
        }
    }

    // Jefe 4 — Sea Anemone (mundo 2). Fija: cerrada 80..207 ticks (aviso los últimos 20), abierta 80..207 lanzando una
    // espora 0x59 cada 8 ticks, y 20 cerrándose. Solo se le puede dar abierta (sprite ≥ 0x12).
    public class SeaAnemone : Boss
    {
        public override void Init(EnemyContext ctx) { Y = 0x48; X = 0x68; Sprite = 0x10; Timer = 0x50 + (R(ctx) & 0x7F); }

        public override void Tick(EnemyContext ctx)
        {
            switch (Phase)
            {
                case 0:
                    if (--Timer < 20) Sprite = 0x11;
                    if (Timer <= 0) { Timer = 0x50 + (R(ctx) & 0x7F); Sprite = 0x12; Phase = 1; }
                    break;
                case 1:
                    --Timer;
                    if ((Timer & 3) == 0)
                    {
                        Sprite = Sprite == 0x12 ? 0x13 : 0x12;
                        if (Sprite == 0x13) { var c = ctx.SpawnChild(0x59, X + 0x10, Y - 8); if (c != null) c.Param = R(ctx) & 15; }
                    }
                    if (Timer <= 0) { Sprite = 0x11; Timer = 20; Phase = 2; }
                    break;
                case 2:
                    if (--Timer <= 0) { Phase = 0; Timer = 0x50 + (R(ctx) & 0x7F); Sprite = 0x10; }
                    break;
            }
        }

        public override bool CanBeHitNow => Sprite >= 0x12;
    }

    // Jefe 6 — King Dragon (mundo 9). 48 ticks paseando (±0,5 px/tick, x 0x48..0x78) y 48 parado escupiendo una
    // llamarada 0x57 cada 16; la cabeza cambia entre 3 poses; retrocede 32 ticks al recibir un golpe. Crea 4 nubes 0x69.
    public class KingDragon : Boss
    {
        int poseTimer = 0x10, cycle = 0x30, spit = 2;
        bool stopped, hit;
        public int Pose;

        public override void Init(EnemyContext ctx)
        {
            Y = 0x70; X = 0x58; Sprite = 0x34; Vx = 0.5f;
            foreach (int x in new[] { 0x40, 0x60, 0x80, 0xA0 }) ctx.SpawnChild(0x69, x, 0x28);
        }

        public override void OnBossHit(EnemyContext ctx) => hit = true;

        public override void Tick(EnemyContext ctx)
        {
            if (--poseTimer <= 0)
            {
                poseTimer = ((R(ctx) >> 3) & 0x1F) + 3;
                Sprite = Sprite == 0x35 ? (ctx.Coin() ? 0x34 : 0x36) : 0x35;
                Pose = Sprite - 0x34;
            }
            if (--cycle <= 0) { cycle = 0x30; stopped = !stopped; }
            Moving = !stopped;
            if (stopped && --spit <= 0)
            {
                spit = 16; poseTimer = 24;
                (int dx, int dy)[] mouth = { (-8, 0x18), (-0x10, 0x18), (-0x18, 0x38) };
                var c = ctx.SpawnChild(0x57, X + mouth[Pose].dx, Y + mouth[Pose].dy);
                if (c != null) c.Param = 2;
            }
            if (Phase == 0)
            {
                if (hit) { hit = false; Vx = 0; Timer = 32; Phase = 1; }
                else if (((int)X & 7) == 0 && (X < 0x50 || X >= 0x78)) Vx = X < 0x50 ? 0.5f : -0.5f;
            }
            else
            {
                Vx = X < 0x78 ? 0.5f : 0;
                if (--Timer <= 0) { Phase = 0; Vx = -0.5f; }
            }
            Integrate();
        }
    }

    // Jefe 7 — Big Lip (mundo 7). Viajes de ida y vuelta desde el centro, en horizontal (1 px/tick, rebota en x 0x18 y
    // x + 0x30 = 0xE0) o en vertical (y 0x40..0x70); mientras se mueve dispara un 0x65 cada 16 ticks y a ratos saca la
    // lengua 96 ticks, parado (también cada vez que golpea al jugador). Solo se le puede dar con la boca abierta o la
    // lengua fuera (sprite ≥ 0x19).
    public class BigLip : Boss
    {
        int shot = 0x10, tongue = 0xB0, anim, ignoreEdges;
        bool hAllowed = true, vAllowed = true, tongueOut;

        public override void Init(EnemyContext ctx) { X = 0x68; Y = 0x40; Sprite = 0x18; }

        void StartTongue() { tongueOut = true; anim = 96; Sprite = 0x1A; Moving = false; }

        public override void Tick(EnemyContext ctx)
        {
            if (tongueOut)
            {
                --anim;
                Sprite = anim > 64 ? 0x1A : anim > 32 ? 0x1B : 0x1A;
                if (anim <= 0) { tongueOut = false; Sprite = 0x18; Moving = true; }
                return;
            }
            if (ctx.Player.State == MotorState.Knockback) { StartTongue(); return; }
            switch (Phase)
            {
                case 0:
                    if (hAllowed && ctx.Coin()) { Vx = ctx.Coin() ? 1 : -1; Vy = 0; ignoreEdges = 8; vAllowed = false; Phase = 1; }
                    else Phase = 2;
                    return;
                case 1:
                    if (--ignoreEdges < 0 && (Vx < 0 && X < 0x18 || Vx > 0 && X + 0x30 >= 0xE0)) Vx = -Vx;
                    X += Vx;
                    if (ignoreEdges < 0 && Math.Abs(X - 0x68) < 0.5f) { X = 0x68; Vx = 0; vAllowed = true; Phase = 2; return; }
                    break;
                case 2:
                    if (vAllowed && ctx.Coin()) { Vy = ctx.Coin() ? 1 : -1; Vx = 0; hAllowed = false; Phase = 3; }
                    else Phase = 0;
                    return;
                case 3:
                    if (Y < 0x40 || Y > 0x70) Vy = -Vy;
                    Y += Vy;
                    if (Y == 0x40 || Y == 0x70) { Vy = 0; hAllowed = true; Phase = 0; return; }
                    break;
            }
            // Mientras se mueve: disparo cada 16 ticks y, a ratos, la lengua.
            if (--shot <= 0)
            {
                shot = 0x10;
                Sprite = 0x19;
                ctx.SpawnChild(0x65, X + 0x18, Y + 0x18);
            }
            else if (ctx.Coin() && --tongue <= 0) { tongue = 0xB0; StartTongue(); }
        }

        public override bool CanBeHitNow => Sprite >= 0x19;
    }

    // Jefe 8 — Monster (mundo 8). Anda a 0,5 px/tick, cae a 4 px/tick por los bordes de su plataforma (x 0x38 / 0x98),
    // sube a 2 px/tick hasta el piso donde estaba el jugador y anda hacia él; cada 128 ticks sin moverse en vertical se
    // para 48 y escupe 5 bolas 0x68.
    public class Monster : Boss
    {
        int attack = 0x80, savedPhase, savedSprite, wait = 0x30, ballSide;
        float targetX, targetFloor;

        public override void Init(EnemyContext ctx) { X = 0x68; Y = 0x20; Sprite = 0x28; }

        public override void Tick(EnemyContext ctx)
        {
            if (Phase == 7)
            {
                --Timer;
                if (Timer == 0x20) Sprite = 0x27;
                if (Timer >= 0x11 && Timer <= 0x15) { ctx.SpawnChild(0x68, ((int)X & ~7) + (ballSide ^= 1) * 0x0C + 10, Y + 4); }
                if (Timer < 0x10) Sprite = 0x24;
                if (Timer <= 0) { Phase = savedPhase; Sprite = savedSprite; Moving = true; attack = 0x80; }
                return;
            }
            if (attack > 0) attack--;
            else if (Vy == 0) { savedPhase = Phase; savedSprite = Sprite; Moving = false; Timer = 48; Sprite = 0x24; Phase = 7; return; }
            switch (Phase)
            {
                case 0:
                    if (--wait > 0) { if (wait % 15 == 0) Sprite ^= 1; return; }
                    Sprite = 0x25; Phase = 1;
                    targetX = ctx.Px; targetFloor = (ctx.Py & 0xE0) - 8;
                    Vx = X >= 0x98 ? -0.5f : X < 0x38 ? 0.5f : (targetX < 0x80 ? -0.5f : 0.5f);
                    break;
                case 1:
                    if ((ctx.Tick & 3) == 0) Sprite ^= 1;
                    X += Vx;
                    if (X <= 0x38 || X >= 0x98)
                    {
                        X = X <= 0x38 ? 0x38 : 0x98;
                        if (Y >= 0xA0) { JumpUp(); break; }
                        Vx = 0; Sprite = 0x2A; Vy = 4; Phase = 2;
                    }
                    break;
                case 2:
                    Y += Vy;
                    if (Y >= 0x98)
                    {
                        Y = 0x98; Vy = 0;
                        if ((targetX < 0x80) == (X < 0x80)) JumpUp();
                        else { Vx = X < 0x80 ? 0.5f : -0.5f; Phase = 3; }
                    }
                    break;
                case 3:
                    X += Vx;
                    if (X <= 0x38 || X >= 0x98) JumpUp();
                    break;
                case 4:
                    if (--Timer <= 0) { Sprite = 0x2A; Vy = -2; Phase = 5; }
                    break;
                case 5:
                    Y += Vy;
                    if (Y <= targetFloor || Y <= 0x20)
                    {
                        Y = Math.Max(Y, Math.Min(targetFloor, Y)); Vy = 0; Sprite = 0x25;
                        Timer = 80; Vx = targetX < X ? -0.5f : 0.5f; Phase = 6;
                    }
                    break;
                case 6:
                    X = Math.Max(0x38, Math.Min(0x98, X + Vx));
                    if (--Timer <= 0) { Sprite = 0x28; Vx = 0; wait = 48; Phase = 0; }
                    break;
            }
        }

        void JumpUp()
        {
            Vx = 0; Sprite = 0x24;
            if (Math.Abs(Y - 8 - targetFloor) < 1) { Timer = 80; Phase = 6; return; }
            Timer = 8; Phase = 4;
        }
    }

    // Jefe 9 — Galious (mundo 10). Rodeado de 8 bolas 0x67 que giran a 24..50 px (se cierran si te acercas) y que lanza
    // de 4 en 4 como bumeranes; cada 250 ticks se teletransporta a una de 8 posiciones (63 oculto + 72 parpadeando,
    // invulnerable). Sin la Cross no se le puede dañar (b03:BBA8).
    public class Galious : Boss
    {
        static readonly (int x, int y)[] Spots = { (0x60, 0x20), (0x80, 0x20), (0x60, 0x40), (0x70, 0x40), (0xA0, 0x40), (0x30, 0x60), (0x40, 0x60), (0xB0, 0x60) };
        public float CenterX = 0x74, CenterY = 0x58;
        public int OrbMode;              // E71B: 0 órbita, 1 ocultas, 2 vuelven al centro, 4 lanzar
        public float OrbRadiusSpeed = 0x70;
        int attack = 0x30, teleport = 0xFA, blink;
        public bool HasCross;

        public override void Init(EnemyContext ctx)
        {
            X = 0x60; Y = 0x40; Sprite = 0x37;
            int[] angles = { 0x00, 0x40, 0x80, 0xC0, 0x20, 0x60, 0xA0, 0xE0 };
            foreach (int a in angles)
            {
                var c = ctx.SpawnChild(0x67, X, Y);
                if (c != null) c.Param = a;
            }
        }

        public override void Tick(EnemyContext ctx)
        {
            var orbs = ctx.Enemies.OfType<GaliousOrb>().Where(o => o.Alive).ToList();
            switch (Phase)
            {
                case 0:
                    OrbMode = 0;
                    if (orbs.Count == 0) { Sprite = 0x37; if (--teleport <= 0) StartTeleport(); break; }
                    bool near = Math.Abs(ctx.Px - CenterX) < 0x1E && Math.Abs(ctx.Py - 16 - CenterY) < 0x1E;
                    OrbRadiusSpeed = near ? 0x90 : 0x70;
                    if (orbs.Any(o => o.Launched)) break;
                    if (!near && --attack <= 0)
                    {
                        attack = 0x30;
                        OrbMode = 4;
                        Sprite = 0x38;
                        int dir = R(ctx) & 15;
                        foreach (var o in orbs.Take(4)) { o.Launch(dir); dir = (dir - 4) & 15; }
                        break;
                    }
                    if (--teleport <= 0) StartTeleport();
                    break;
                case 1:
                    if (orbs.Any(o => o.Launched)) break;
                    Timer = 63; OrbMode = 1; Visible = Hittable = Harmful = false; Phase = 2;
                    break;
                case 2:
                    if (--Timer > 0) break;
                    var s = Spots[R(ctx) & 7];
                    X = s.x; Y = s.y; CenterX = X + 0x14; CenterY = Y + 0x18;
                    OrbMode = 2; Phase = 3; blink = 24 * 3;
                    break;
                case 3:
                    bool on = (blink / 3) % 2 == 0;
                    Visible = Hittable = on;
                    if (--blink <= 0) { Visible = Hittable = Harmful = true; attack = 0x30; teleport = 0xFA; Phase = 0; }
                    break;
            }
        }

        void StartTeleport() { Phase = 1; }

        public override bool CanBeHitNow => Hittable && Visible && HasCross;
    }

    // Jefe 10 — Donra (mundo 5). Vuela en x 0x40..0x88 (1 px/tick) y en y 0x40..0x78 (baja a 2, sube a 1); la cabeza
    // sigue la altura del jugador a lo largo del cuerpo; cada 8..39 ticks se para y escupe una llamarada 0x57 desde la
    // cabeza. Solo se le puede dar en la cabeza.
    public class Donra : Boss
    {
        public int HeadStep = 1;         // E71C: 1..4
        int headTimer = 3, fire = 4;

        public override void Init(EnemyContext ctx) { X = 0x70; Y = 0x60; Sprite = 0x1C; Vy = 4; Vx = -4; }

        public float HeadY => Y + 8 * HeadStep;

        // Solo se le da en la cabeza (b03:BC02: usa la caja del sprite de la cabeza, E719 = 0x1D + paso).
        BossSprite Head => Info?.Sprite(0x1D + HeadStep);
        public override IEnumerable<(float x0, float y0, float x1, float y1)> WeaponRects()
        {
            var h = Head;
            if (h != null && h.weaponBox.valid) yield return Rect(X, Y, h.weaponBox);
        }
        public override IEnumerable<(float x0, float y0, float x1, float y1)> ContactRects()
        {
            foreach (var r in base.ContactRects()) yield return r;
            var h = Head;
            if (h != null && h.weaponBox.valid) yield return Rect(X, Y, h.weaponBox);
        }

        public override void Tick(EnemyContext ctx)
        {
            if ((ctx.Tick % (Vy < 0 ? 4 : 10)) == 0) Sprite ^= 1;
            switch (Phase)
            {
                case 0:
                    if (Y + Vy >= 0x78) Vy = -1;
                    else if (Y + Vy < 0x40) Vy = 2;
                    if (X + Vx >= 0x88) Vx = -1;
                    else if (X + Vx < 0x40) Vx = 1;
                    if (--headTimer <= 0)
                    {
                        headTimer = 8;
                        if (HeadY < ctx.Py - 16 && HeadStep < 4) HeadStep++;
                        else if (HeadY > ctx.Py - 16 && HeadStep > 1) HeadStep--;
                    }
                    Integrate();
                    if (--fire <= 0) { Phase = 1; Timer = 16; }
                    break;
                case 1:
                    if (--Timer > 0) break;
                    int[] dy = { 0x08, 0x10, 0x18, 0x20 };
                    var c = ctx.SpawnChild(0x57, X - 0x10, Y + dy[HeadStep - 1]);
                    if (c != null) c.Param = 2;
                    Timer = 32; Phase = 2;
                    break;
                case 2:
                    if (--Timer > 0) break;
                    fire = 8 + (R(ctx) & 0x1F); Phase = 0;
                    break;
            }
        }
    }

    // Jefe 11 — Jump Slime (mundo 4). Se arrastra a 0,5 px/tick; si el jugador está cerca se agacha 8 ticks y salta
    // (vx ±1,5, vy −4,5 o, 1 de cada 4, −6; g 0,25); al caer en y = 0x70 se queda 32 ticks. 64 px de ancho.
    public class JumpSlime : Boss
    {
        const float Floor = 0x70;
        int landings, air;
        bool right;

        public override void Init(EnemyContext ctx) { X = 0x80; Y = Floor; Sprite = 0x2B; right = ctx.Coin(); Vx = right ? 0.5f : -0.5f; }

        public override void Tick(EnemyContext ctx)
        {
            switch (Phase)
            {
                case 0:
                    if ((ctx.Tick & 7) == 0) Sprite = Sprite == 0x2B ? 0x2C : 0x2B;
                    if (X < 0x18 || X + 0x40 >= 0xE0) { Vx = -Vx; right = Vx > 0; }
                    X += Vx;
                    bool close = right ? ctx.Px <= X + 0x70 : ctx.Px > X - 0x20;
                    if (close && (R(ctx) & 3) != 0) { Sprite = right ? 0x2D : 0x2E; Timer = 8; Phase = 1; }
                    break;
                case 1:
                    if (--Timer > 0) break;
                    Sprite = right ? 0x2F : 0x30; Timer = 8; Phase = 2;
                    break;
                case 2:
                    X += right ? 0.5f : -0.5f;
                    if (--Timer > 0) break;
                    Vx = right ? 1.5f : -1.5f;
                    Vy = (R(ctx) & 3) == 1 ? -6 : -4.5f;
                    air = 0; Phase = 3;
                    break;
                case 3:
                    Vy += 0.25f;
                    if (X + Vx < 0x18 || X + Vx + 0x40 >= 0xE0) Vx = 0;
                    X += Vx; Y += Vy;
                    if (++air <= 32) Sprite = 0x2B;
                    if (air > 32 && Y > Floor)
                    {
                        Y = Floor; Vx = Vy = 0; Timer = 32; Phase = 4;
                    }
                    break;
                case 4:
                    if (--Timer > 0) break;
                    landings++;
                    right = (landings & 3) == 0 ? ctx.Coin() : ctx.Px > X;
                    Vx = right ? 0.5f : -0.5f; Phase = 0;
                    break;
            }
        }
    }
}
