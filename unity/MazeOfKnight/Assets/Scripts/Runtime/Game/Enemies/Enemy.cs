using System;
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

        // Entero en [min, max], ambos incluidos.
        public int Rand(int min, int max) => Random.Next(min, max + 1);
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
        public bool Alive = true;
        public bool FacingLeft;
        public float Grow = 1f;          // parte visible de la caja (pinchos y brotes que crecen), de abajo arriba

        public virtual void Init(EnemyContext ctx) { }
        public abstract void Tick(EnemyContext ctx);

        // Le alcanza un espadazo: true si cuenta (resta vida en el sistema).
        public virtual bool OnHit(EnemyContext ctx) => !Invulnerable && Visible;

        protected bool Solid(EnemyContext ctx, float px, float py) => ctx.World.Solid(Room, (int)px, (int)py);

        // Rumbo de 16 direcciones (0 = derecha, 4 = arriba, 8 = izquierda, 12 = abajo), como b04:7E25.
        protected void Heading(int dir, float speed)
        {
            double a = (dir & 15) * Math.PI / 8;
            Vx = (float)Math.Cos(a) * speed;
            Vy = -(float)Math.Sin(a) * speed;
        }

        // Rebote en los límites de la sala (x 0..240, y 32..176).
        protected void BounceInRoom()
        {
            if (X < 0) { X = 0; Vx = Math.Abs(Vx); }
            if (X > P.RoomWidthPx - Width) { X = P.RoomWidthPx - Width; Vx = -Math.Abs(Vx); }
            if (Y < P.RoomTopPx) { Y = P.RoomTopPx; Vy = Math.Abs(Vy); }
            if (Y > P.RoomBottomPx - Height) { Y = P.RoomBottomPx - Height; Vy = -Math.Abs(Vy); }
        }

        // Caja que hace daño (la parte visible si crece desde el suelo o el techo).
        public (float x0, float y0, float x1, float y1) Box()
        {
            float h = Height * Grow;
            return GrowsDown ? (X, Y, X + Width, Y + h) : (X, Y + Height - h, X + Width, Y + Height);
        }

        public bool GrowsDown;           // cuelga del techo (crece hacia abajo)
    }
}
