using System;

namespace MazeOfKnight.Game.Enemies
{
    // RemakeRules.HollowDeath: la sombra del jugador, donde murió por última vez, con el geo que llevaba (la Hollow Shade
    // de Hollow Knight). Flota quieta meciéndose y, si el jugador se acerca, va a por él despacio; tocarla hace daño como
    // un enemigo. Al derrotarla (Hp golpes de espada) se recupera el geo (GameBootstrap, por EnemySystem.Killed).
    public class Shade : Enemy
    {
        public const int TypeId = 0x1F1;           // fuera de los tipos del original
        public const int Hits = 4, ContactDamage = 1;
        public const float Speed = 0.45f, Sight = 96;
        public int Geo;
        float baseY, t;

        public override void Init(EnemyContext ctx)
        {
            Hp = Hits;
            Damage = ContactDamage;
            Width = 16;
            Height = 16;
            baseY = Y;
        }

        public override void Tick(EnemyContext ctx)
        {
            t += 1;
            float dx = ctx.Px - (X + 8), dy = ctx.Py - 8 - (Y + 8);
            float d = (float)Math.Sqrt(dx * dx + dy * dy);
            if (d < Sight && d > 1)
            {
                X += dx / d * Speed;
                Y += dy / d * Speed;
                baseY = Y;
                FacingLeft = dx < 0;
            }
            else
                Y = baseY + (float)Math.Sin(t * 0.08f) * 2;
        }
    }
}
