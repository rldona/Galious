using System;
using System.Collections.Generic;
using MazeOfKnight.Data;

namespace MazeOfKnight.Game.Enemies
{
    // Generadores periódicos (b04:6186 al entrar, b04:62DB cada tick; docs/entities.md). Cada uno cuenta ticks y, al
    // llegar a 0, intenta crear un enemigo; si la sala está llena el intento se pierde y el temporizador sigue.
    public partial class EnemySystem
    {
        class Generator
        {
            public PeriodicJson Data;
            public int Timer, InBatch, Attempts;
        }

        readonly List<Generator> generators = new List<Generator>();

        // Objetos que tiene el jugador (el Carpet, 0x18, apaga el generador E653).
        public Func<int, bool> HasItem = _ => false;

        void StartGenerators(RoomModel room)
        {
            generators.Clear();
            foreach (var p in room.periodic ?? Array.Empty<PeriodicJson>())
            {
                if (p.disabledByItem != 0 && HasItem(p.disabledByItem))
                    continue;
                generators.Add(new Generator { Data = p, Timer = FirstWait(p) });
            }
        }

        int FirstWait(PeriodicJson p) => p.periodRandom.Length == 2 ? Context.Rand(p.periodRandom[0], p.periodRandom[1]) : p.firstDelay;

        void TickGenerators()
        {
            foreach (var g in generators)
            {
                var p = g.Data;
                if (p.maxTotal > 0 && g.Attempts >= p.maxTotal)
                    continue;
                if (p.pauseInLiquid && Context.Player.InLiquid)
                    continue;
                if (--g.Timer > 0)
                    continue;
                Attempt(g);
                g.Attempts++;
                if (p.periodRandom.Length == 2)
                    g.Timer = Context.Rand(p.periodRandom[0], p.periodRandom[1]);
                else if (p.batch > 0)
                    g.Timer = ++g.InBatch < p.batch ? p.period : (g.InBatch = 0) + p.batchGap;
                else
                    g.Timer = p.period;
            }
        }

        void Attempt(Generator g)
        {
            var p = g.Data;
            float x = p.x;
            if (p.xRule == "sequence" && p.xSequence.Length > 0)
                x = p.xSequence[g.Attempts % p.xSequence.Length];
            else if (p.xRule == "random" && p.randomX.Length >= 2)
            {
                int pair = Context.Rand(0, p.randomX.Length / 2 - 1);
                x = Context.Rand(p.randomX[2 * pair], p.randomX[2 * pair + 1]);
            }
            var e = Create(p.type, Math.Max(0, x), p.y, p.param, p);
            if (e != null && p.xRule == "leap" && e is LeapFish fish)
                fish.Place(Context, p);
        }
    }
}
