using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MazeOfKnight.Game
{
    // Opciones de Galious: The Hollow Kingdom, como las de Hollow Knight (se guardan en PlayerPrefs):
    //   - Audio: volumen general, de los efectos y de la música, de 0 a 10. La música y los efectos salen del mismo chip
    //     (Sound/Psg): el efecto suena por el canal C, que mientras tanto va con el volumen de los efectos. Con los dos
    //     al máximo se oye como el original.
    //   - Vídeo: resolución, pantalla completa, sincronía vertical y límite de fotogramas.
    //   - Brillo: de 0 a 100 % (50, el normal), como exposición del posproceso (Brightness): aclara u oscurece el juego,
    //     no la interfaz.
    public static class GameSettings
    {
        public static readonly int[] FrameCaps = { 0, 30, 60, 120, 144 };   // 0: sin límite

        static int Get(string k, int d) => PlayerPrefs.GetInt(k, d);
        static void Set(string k, int v) { PlayerPrefs.SetInt(k, v); Apply(); }

        public static int Master
        {
            // Antes había un solo volumen de 0 a 1 ("volumen", 0,35 por defecto): el general sale de él.
            get => Mathf.Clamp(Get("vol_general", PlayerPrefs.HasKey("volumen") ? Mathf.RoundToInt(PlayerPrefs.GetFloat("volumen") / 0.05f) : 7), 0, 10);
            set => Set("vol_general", Mathf.Clamp(value, 0, 10));
        }
        public static int Effects { get => Get("vol_sonido", 10); set => Set("vol_sonido", Mathf.Clamp(value, 0, 10)); }
        public static int Music { get => Get("vol_musica", 10); set => Set("vol_musica", Mathf.Clamp(value, 0, 10)); }

        // Volumen de lo que no es el chip (el tarareo de Cornifer): general por efectos.
        public static float EffectsVolume => Master * 0.05f * Effects / 10f;
        // La música de Hollow Knight (grabada y normalizada por tools/hollow/musica.py): general por música, con el general a
        // 7 casi entera (se oía muy baja frente a los efectos del chip).
        public static float HollowMusicVolume => Mathf.Clamp01(Master / 7f) * Music / 10f;

        public static bool VSync { get => Get("vsync", 1) == 1; set => Set("vsync", value ? 1 : 0); }
        public static int FrameCap { get => Get("fps", 0); set => Set("fps", value); }
        public static int Brightness { get => Get("brillo", 50); set => Set("brillo", Mathf.Clamp(value, 0, 100)); }
        public static bool Vibration { get => Get("vibracion", 1) == 1; set => Set("vibracion", value ? 1 : 0); }

        public static bool FullScreen
        {
            get => Screen.fullScreen;
            set { Screen.fullScreenMode = value ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed; Screen.fullScreen = value; }
        }

        // Las resoluciones de la pantalla, sin repetir (de menor a mayor).
        public static List<Vector2Int> Resolutions =>
            Screen.resolutions.Select(r => new Vector2Int(r.width, r.height)).Distinct().OrderBy(r => r.x * 10000 + r.y).ToList();

        public static Vector2Int Resolution
        {
            get => new Vector2Int(Screen.width, Screen.height);
            set => Screen.SetResolution(value.x, value.y, Screen.fullScreenMode);
        }

        public static void ResetAudio()
        {
            foreach (var k in new[] { "vol_general", "vol_sonido", "vol_musica", "volumen" }) PlayerPrefs.DeleteKey(k);
            Apply();
        }

        public static void ResetVideo()
        {
            foreach (var k in new[] { "vsync", "fps", "brillo" }) PlayerPrefs.DeleteKey(k);
            Apply();
        }

        public static void Apply()
        {
            var snd = Sound.Instance;
            if (snd != null)
            {
                snd.Volume = Master * 0.05f;            // 7 → 0,35, el de siempre
                snd.EffectsGain = Effects / 10f;
                snd.MusicGain = Music / 10f;
            }
            QualitySettings.vSyncCount = VSync ? 1 : 0;
            Application.targetFrameRate = FrameCap > 0 ? FrameCap : -1;
            Visuals.Brightness.Ensure();
        }
    }
}

namespace MazeOfKnight.Game.Visuals
{
    // El brillo de Opciones: un volumen de posproceso global con la exposición (GameSettings.Brightness: 50 % no cambia
    // nada; 0 %, −1,5 pasos; 100 %, +1,5).
    public class Brightness : MonoBehaviour
    {
        static Brightness instance;
        ColorAdjustments adjust;

        public static void Ensure()
        {
            if (instance != null) return;
            var go = new GameObject("Brillo");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<Brightness>();
        }

        public static float Exposure => (GameSettings.Brightness - 50) / 50f * 1.5f;

        void Awake()
        {
            var v = gameObject.AddComponent<Volume>();
            v.isGlobal = true;
            v.priority = 100;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            adjust = profile.Add<ColorAdjustments>(true);
            v.sharedProfile = profile;
        }

        void LateUpdate()
        {
            float e = Exposure;
            adjust.postExposure.Override(e);
            // Con un brillo distinto del normal, la cámara tiene que hacer posproceso (en la partida ya lo hace).
            if (Mathf.Abs(e) > 0.001f && Camera.main != null && Camera.main.TryGetComponent<UniversalAdditionalCameraData>(out var d))
                d.renderPostProcessing = true;
        }
    }
}
