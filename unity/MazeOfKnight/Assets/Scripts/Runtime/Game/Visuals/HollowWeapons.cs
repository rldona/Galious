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

        // Solo aspecto: cuántos píxeles más arriba de su caja se pintan. Las flechas (1 y 2) salen en el original a 4 px del
        // suelo (y − 8 alineada a 8, caja de y + 2 a y + 5) y, con el caballero de Hollow Knight (unos 22 px de alto), se
        // verían rodando por el suelo: se pintan a media altura del personaje (los pies − 11). La caja no se mueve, así que
        // siguen dando a los bichos bajos como en el original. El fuego y la mina sí van por el suelo.
        public const int ArrowLift = 7;
        public static int Lift(int weapon) => weapon == 1 || weapon == 2 ? ArrowLift : 0;

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
