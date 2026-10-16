using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Los proyectiles de las armas del jugador con los de Hollow Knight (Assets/Art/Resources/Hollow/armas, fuera de git;
    // los importa tools/hollow/armas.py): armaN_NN.png, mirando a la derecha y centrados. Null si no están.
    public static class HollowWeapons
    {
        const float Fps = 12;
        static readonly Dictionary<int, Sprite[]> clips = new Dictionary<int, Sprite[]>();

        public static Sprite Frame(int weapon, int ageTicks)
        {
            if (!clips.TryGetValue(weapon, out var frames))
            {
                var l = new List<Sprite>();
                for (int i = 0; ; i++)
                {
                    var s = Resources.Load<Sprite>($"Hollow/armas/arma{weapon}_{i:00}");
                    if (s == null) break;
                    l.Add(s);
                }
                clips[weapon] = frames = l.ToArray();
            }
            if (frames.Length == 0) return null;
            return frames[(int)(ageTicks / (float)OriginalPhysics.TicksPerSecond * Fps) % frames.Length];
        }
    }
}
