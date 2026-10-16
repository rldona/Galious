using System;
using System.Collections.Generic;

namespace MazeOfKnight.Game
{
    // Diario del cazador de Galious: The Hollow Kingdom (el Hunter's Journal de Hollow Knight; RemakeRules.HollowJournal):
    // cuántas criaturas de cada tipo se han derrotado. Las invulnerables (pinchos, torretas…) cuentan al verlas, porque no
    // se pueden derrotar. Lo dibuja InventoryMenu (pestaña Diario) con las entradas de Visuals/HuntersJournal.
    public class Journal
    {
        public const int NotesAt = 10;          // derrotas para leer las notas del cazador (los jefes, 1)
        readonly Dictionary<int, int> kills = new Dictionary<int, int>();
        readonly HashSet<int> seen = new HashSet<int>();

        // Primera vez que se registra un tipo (para el aviso de entrada nueva).
        public event Action<int> NewType;

        public void Killed(int type)
        {
            kills.TryGetValue(type, out int n);
            kills[type] = n + 1;
            if (seen.Add(type)) NewType?.Invoke(type);
        }

        public void Seen(int type)
        {
            if (seen.Add(type)) NewType?.Invoke(type);
        }

        public bool Known(IEnumerable<int> types)
        {
            foreach (int t in types) if (seen.Contains(t)) return true;
            return false;
        }

        public int Kills(IEnumerable<int> types)
        {
            int n = 0;
            foreach (int t in types) if (kills.TryGetValue(t, out int k)) n += k;
            return n;
        }

        // Partida guardada: tipo * 65536 + derrotas (los vistos sin derrotar, con 0).
        public List<int> Pack()
        {
            var l = new List<int>();
            foreach (int t in seen) l.Add(t * 65536 + Math.Min(65535, kills.TryGetValue(t, out int k) ? k : 0));
            return l;
        }

        public void Restore(IEnumerable<int> packed)
        {
            kills.Clear();
            seen.Clear();
            foreach (int v in packed)
            {
                int t = v / 65536, k = v % 65536;
                seen.Add(t);
                if (k > 0) kills[t] = k;
            }
        }
    }
}
