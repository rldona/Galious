using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Hace latir despacio el brillo de un sprite (la luz de las puertas de los mundos).
    public class Pulse : MonoBehaviour
    {
        SpriteRenderer sr;
        float baseAlpha, phase;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            baseAlpha = sr != null ? sr.color.a : 1;
            phase = Random.value * 6.28f;
        }

        void Update()
        {
            if (sr == null) return;
            var c = sr.color;
            c.a = baseAlpha * (0.75f + 0.25f * Mathf.Sin(Time.time * 1.6f + phase));
            sr.color = c;
        }
    }
}
