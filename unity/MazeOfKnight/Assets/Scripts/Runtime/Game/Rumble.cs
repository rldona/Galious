using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game
{
    // Vibración del mando, como en Hollow Knight (se quita en Opciones > Mando: GameSettings.Vibration): un pulso corto al
    // golpear con la espada (enemigo, piedra, pared o un escudo), uno más fuerte al matar, al recibir daño y al curarse, y
    // un zumbido suave que crece mientras se concentra. Low es el motor grande (grave) y High el pequeño (agudo), de 0 a 1.
    public class Rumble : MonoBehaviour
    {
        static Rumble instance;
        float low, high, until;            // el pulso de ahora
        float holdLow, holdHigh, holdUntil;   // lo que se mantiene mientras se siga pidiendo (concentrarse)
        bool running;

        static Rumble Instance
        {
            get
            {
                if (instance != null) return instance;
                var go = new GameObject("Vibración");
                DontDestroyOnLoad(go);
                return instance = go.AddComponent<Rumble>();
            }
        }

        public static void Pulse(float low, float high, float seconds)
        {
            if (!GameSettings.Vibration || Gamepad.current == null) return;
            var r = Instance;
            // Un pulso más flojo no corta uno más fuerte que aún dura.
            if (Time.unscaledTime < r.until && low + high < r.low + r.high) return;
            r.low = low;
            r.high = high;
            r.until = Time.unscaledTime + seconds;
        }

        // Mientras se siga llamando (cada tick del juego), vibra así de flojo (se suma al pulso).
        public static void Hold(float low, float high)
        {
            if (!GameSettings.Vibration || Gamepad.current == null) return;
            var r = Instance;
            r.holdLow = low;
            r.holdHigh = high;
            r.holdUntil = Time.unscaledTime + 0.08f;
        }

        void LateUpdate()
        {
            var gp = Gamepad.current;
            bool pulse = Time.unscaledTime < until;
            bool hold = Time.unscaledTime < holdUntil;
            float l = Mathf.Clamp01((pulse ? low : 0) + (hold ? holdLow : 0)), h = Mathf.Clamp01((pulse ? high : 0) + (hold ? holdHigh : 0));
            if (gp == null) return;
            if (l > 0 || h > 0) { gp.SetMotorSpeeds(l, h); running = true; }
            else if (running) { gp.SetMotorSpeeds(0, 0); running = false; }
        }

        void OnApplicationFocus(bool focus) { if (!focus) Gamepad.current?.SetMotorSpeeds(0, 0); }
        void OnDisable() => Gamepad.current?.SetMotorSpeeds(0, 0);
    }
}
