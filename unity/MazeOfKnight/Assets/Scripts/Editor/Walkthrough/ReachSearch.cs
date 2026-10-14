using System;
using System.Collections.Generic;
using MazeOfKnight.Data;
using MazeOfKnight.Game;

namespace MazeOfKnight.EditorTools.Walkthrough
{
    // Búsqueda exhaustiva de los estados del jugador en un mapa: desde unos estados de partida, todas las teclas posibles
    // tick a tick con el propio PlayerMotor (y los ascensores del castillo), hasta agotar los estados nuevos. Sin
    // enemigos ni espadazos: lo que se puede alcanzar moviéndose. Lo usa Walkthrough para comprobar que la partida se
    // puede acabar.
    //  - Agua (Popolon) y lava: como mucho DipBudget ticks seguidos dentro; más es perder la vida [I].
    //  - De pie o en una escalera se puede cambiar de personaje (menú) y esperar a que llegue un ascensor vertical.
    //  - Las puertas (con llave y giratorias) se cruzan con los macros que da quien llama (Carry).
    public class ReachSearch
    {
        public struct Key : IEquatable<Key>
        {
            public ulong A, B, C;
            public bool Equals(Key o) => A == o.A && B == o.B && C == o.C;
            public override bool Equals(object o) => o is Key k && Equals(k);
            public override int GetHashCode()
            {
                ulong h = A * 0x9E3779B97F4A7C15UL ^ B * 0xC2B2AE3D27D4EB4FUL ^ C * 0x165667B19E3779F9UL;
                return (int)(h ^ h >> 32);
            }
        }

        // ← → o nada, por ↑ ↓ o nada, y ↑ ↓ como pulsación nueva (soltar y volver a pulsar entre dos ticks).
        static readonly MotorInput[] Inputs = BuildInputs();

        static MotorInput[] BuildInputs()
        {
            var l = new List<MotorInput>();
            foreach (int h in new[] { 0, -1, 1 })
                foreach (int v in new[] { 0, 1, 2, -1, -2 })
                    l.Add(new MotorInput
                    {
                        Left = h < 0, Right = h > 0,
                        Up = v > 0, UpPressed = v == 2,
                        Down = v < 0, DownPressed = v == -2,
                    });
            return l.ToArray();
        }

        public int DipBudget = 28;                       // 7 de vitalidad a 1 cada 4 ticks
        public int MaxStates = 40_000_000;
        public Func<Character, bool> CharacterAllowed = _ => true;
        public Action<PlayerMotor> Visit;                 // cada estado nuevo
        public Func<PlayerMotor, IEnumerable<PlayerMotor>> Macros;   // sucesores extra desde un estado estable

        public int States { get; private set; }
        public bool Truncated { get; private set; }
        public readonly HashSet<int> CastleExits = new HashSet<int>();
        public readonly HashSet<int> DoorsEntered = new HashSet<int>();
        public readonly HashSet<(int from, int to, int dir)> Transitions = new HashSet<(int, int, int)>();

        int firedExit, firedDoor;
        readonly Action<int> onExit, onDoor;

        public ReachSearch()
        {
            onExit = c => firedExit = c;
            onDoor = w => firedDoor = w;
        }

        public static bool Harmful(PlayerMotor m) =>
            m.Liquid == TileClass.Lava || m.Liquid == TileClass.Water && m.Character == Character.Popolon;

        static bool Stable(PlayerMotor m) => m.State == MotorState.Ground || m.State == MotorState.Ladder;

        Key KeyOf(PlayerMotor m, int dip)
        {
            var (a, b) = m.StateKey();
            return new Key { A = a, B = b | (ulong)dip << 52, C = m.Lifts?.StateKey(m.Room.id) ?? 0 };
        }

        public static PlayerMotor Copy(PlayerMotor m)
        {
            var c = m.Clone();
            c.Lifts = m.Lifts?.Clone();
            return c;
        }

        public void Run(IEnumerable<PlayerMotor> starts)
        {
            var seen = new HashSet<Key>();
            var queue = new Queue<(PlayerMotor, int)>();
            void Push(PlayerMotor m, int dip)
            {
                if (seen.Add(KeyOf(m, dip))) queue.Enqueue((m, dip));
            }
            foreach (var s in starts)
                if (CharacterAllowed(s.Character)) Push(s, 0);

            while (queue.Count > 0)
            {
                if (seen.Count > MaxStates) { Truncated = true; break; }
                var (m, dip) = queue.Dequeue();
                Visit?.Invoke(m);
                foreach (var input in Inputs)
                {
                    var c = Copy(m);
                    firedExit = firedDoor = 0;
                    c.CastleExitReached += onExit;
                    c.DoorEntered += onDoor;
                    int room = c.Room.id, y = c.Y;
                    c.Tick(input);
                    if (firedExit != 0) { CastleExits.Add(firedExit); continue; }
                    if (firedDoor != 0) { DoorsEntered.Add(firedDoor); continue; }
                    if (c.Room.id != room)
                        Transitions.Add((room, c.Room.id, c.Y > y + 80 ? 1 : c.Y < y - 80 ? -1 : 0));   // 1 hacia arriba, -1 hacia abajo
                    int d = Harmful(c) ? dip + 1 : 0;
                    if (d > DipBudget) continue;
                    Push(c, d);
                }
                if (!Stable(m)) continue;
                // Menú: el otro personaje, en el mismo sitio.
                var other = m.Character == Character.Popolon ? Character.Aphrodite : Character.Popolon;
                if (CharacterAllowed(other))
                {
                    var c = Copy(m);
                    c.Character = other;
                    Push(c, Harmful(c) ? Math.Max(dip, 1) : 0);
                }
                // Esperar al ascensor vertical de la columna (por arriba bajando o por abajo subiendo).
                if (m.Lifts != null)
                    foreach (bool down in new[] { true, false })
                    {
                        var c = Copy(m);
                        if (c.Lifts.ArriveShaft(c.Room.id, down)) Push(c, dip);
                    }
                if (Macros != null)
                    foreach (var c in Macros(m))
                        Push(c, dip);
            }
            States = seen.Count;
        }
    }
}
