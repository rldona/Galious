using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game
{
    // Vibración del mando, como en Hollow Knight (se quita en Opciones > Mando: GameSettings.Vibration): un pulso corto al
    // golpear con la espada (enemigo, piedra, pared o un escudo), uno más fuerte al matar, al recibir daño y al curarse, y
    // un zumbido suave que crece mientras se concentra. Low es el motor grande (grave) y High el pequeño (agudo), de 0 a 1.
    // En macOS el Input System no hace vibrar los mandos: va por el framework de Apple (Plugins/macOS/GameHaptics.bundle,
    // de tools/native/GameHaptics.m), que vale para los de Xbox, PlayStation y Switch.
    public class Rumble : MonoBehaviour
    {
        static Rumble instance;
        float low, high, until;            // el pulso de ahora
        float holdLow, holdHigh, holdUntil;   // lo que se mantiene mientras se siga pidiendo (concentrarse)
        bool running;
        float nativeSent = -1, nativeAt;

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        [DllImport("GameHaptics")] static extern int GH_Available();
        [DllImport("GameHaptics")] static extern void GH_Rumble(float intensity, float sharpness, float seconds);
        [DllImport("GameHaptics")] static extern void GH_Stop();
        static bool? native;
        static bool Native
        {
            get
            {
                if (native == null)
                {
                    try { native = true; GH_Available(); }
                    catch (System.Exception e) { native = false; Debug.Log($"[Rumble] sin vibración nativa: {e.Message}"); }
                }
                return native.Value;
            }
        }
        static bool NativeAvailable() { try { return Native && GH_Available() != 0; } catch { return false; } }
#else
        static bool Native => false;
        static bool NativeAvailable() => false;
        static void GH_Rumble(float i, float s, float t) { }
        static void GH_Stop() { }
#endif

        static bool CanRumble => GameSettings.Vibration && (Gamepad.current != null || NativeAvailable());

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
            if (!CanRumble) return;
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
            if (!CanRumble) return;
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
            if (Native && NativeAvailable())
            {
                // Al framework de Apple: cuando cambia, o cada 0,1 s mientras dura (cada envío vibra 0,15 s).
                float intensity = Mathf.Max(l, h), now = Time.unscaledTime;
                if (intensity > 0 && (Mathf.Abs(intensity - nativeSent) > 0.05f || now - nativeAt > 0.1f))
                {
                    GH_Rumble(intensity, h / Mathf.Max(0.001f, l + h), 0.15f);
                    nativeSent = intensity;
                    nativeAt = now;
                }
                else if (intensity <= 0 && nativeSent > 0) { GH_Stop(); nativeSent = 0; }
                return;
            }
            if (gp == null) return;
            if (l > 0 || h > 0) { gp.SetMotorSpeeds(l, h); running = true; }
            else if (running) { gp.SetMotorSpeeds(0, 0); running = false; }
        }

        void OnApplicationFocus(bool focus) { if (!focus) StopAll(); }
        void OnDisable() => StopAll();

        void StopAll()
        {
            Gamepad.current?.SetMotorSpeeds(0, 0);
            if (Native) { GH_Stop(); nativeSent = 0; }
        }

        // Para comprobarlo (registro de las capturas): si hay un mando que pueda vibrar y por dónde.
        public static string Describe() =>
            $"mando {(Gamepad.current != null ? Gamepad.current.displayName : "ninguno (Input System)")}, nativo {(Native ? (NativeAvailable() ? "sí" : "sin mando") : "no")}";
    }
}
