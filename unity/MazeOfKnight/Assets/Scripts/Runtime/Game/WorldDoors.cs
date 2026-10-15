using System;
using System.Collections.Generic;

namespace MazeOfKnight.Game
{
    // Puertas de mundo del castillo (docs/state_machine.md, "Puertas y llaves"; medido con
    // tools/openmsx/door_open_measure.py en la sala 104):
    //  - cada puerta empieza cerrada (E062 + n = 0). La Gran Llave del castillo desbloquea la del mundo 1 y la de cada
    //    mundo k, la del k + 1 (bit 1);
    //  - desbloqueada, se abre sola en cuanto el jugador se pone delante, en el mismo sitio en que se entra con ↓
    //    (y = Y_puerta + 0x18, x entre X_puerta + 14 y + 18), sin tener que pulsar nada ni quedarse quieto;
    //  - la apertura son 3 pasos de 6 ticks: en el primero no cambia nada, en el segundo se abre la fila de abajo del
    //    hueco (2 × 2 tiles) y en el tercero la de arriba; al acabar el tercero la puerta queda abierta (bit 0, para
    //    siempre) y la Gran Llave se gasta;
    //  - solo se entra con la puerta abierta.
    // Si el jugador sale de la sala a mitad, la apertura se pierde (la sala vuelve a cargar su puerta cerrada).
    public class WorldDoors
    {
        public const int StepTicks = 6, Steps = 3;

        public readonly HashSet<int> Unlocked;      // bit 1: desbloqueadas y sin abrir
        public readonly HashSet<int> Opened;        // bit 0: abiertas

        public int OpeningWorld { get; private set; }     // 0 = ninguna
        int ticks;

        // Filas del hueco ya abiertas (0, 1 o 2) al cambiar: para dibujarla.
        public event Action<int, int> Stage;
        public event Action<int> DoorOpened;

        public WorldDoors(HashSet<int> unlocked, HashSet<int> opened)
        {
            Unlocked = unlocked;
            Opened = opened;
        }

        public bool IsOpen(int world) => Opened.Contains(world);

        // Para dibujarla: filas del hueco abiertas.
        public int OpenRows(int world)
        {
            if (Opened.Contains(world)) return 2;
            if (world == OpeningWorld) return Math.Min(2, ticks / StepTicks);
            return 0;
        }

        // Gran Llave recogida en el mapa 'world' (0 = castillo): desbloquea la puerta del mundo siguiente.
        public void KeyTaken(int world)
        {
            int next = world + 1;
            if (next <= 10 && !Opened.Contains(next)) Unlocked.Add(next);
        }

        // Cada tick del juego, en el castillo.
        public void Tick(PlayerMotor m, bool inCastle)
        {
            if (OpeningWorld != 0 && (!inCastle || m.Room.doorWorld != OpeningWorld))
                Cancel();
            if (!inCastle) return;
            int w = m.Room.doorWorld;
            if (OpeningWorld == 0)
            {
                if (w > 0 && Unlocked.Contains(w) && !Opened.Contains(w) && m.AtDoor())
                {
                    OpeningWorld = w;
                    ticks = 0;
                    Stage?.Invoke(w, 0);
                }
                return;
            }
            int before = OpenRows(OpeningWorld);
            ticks++;
            if (ticks >= Steps * StepTicks)
            {
                int done = OpeningWorld;
                OpeningWorld = 0;
                Unlocked.Remove(done);
                Opened.Add(done);
                DoorOpened?.Invoke(done);
                return;
            }
            if (OpenRows(OpeningWorld) != before) Stage?.Invoke(OpeningWorld, OpenRows(OpeningWorld));
        }

        void Cancel()
        {
            int w = OpeningWorld;
            OpeningWorld = 0;
            Stage?.Invoke(w, 0);
        }
    }
}
