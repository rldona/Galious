using UnityEngine;

namespace MazeOfKnight.Game
{
    // La salida del chip de sonido (Sound): escribe sus muestras en el hilo de audio, sin pasar por un clip, para que el
    // sonido salga en cuanto se pide (con el búfer de audio pequeño de ProjectSettings, unos 5 ms).
    public class PsgOutput : MonoBehaviour
    {
        public Sound Sound;

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (Sound != null) Sound.Fill(data, channels);
        }
    }
}
