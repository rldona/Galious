using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // RemakeRules.HollowMusic: la música de fondo de Hollow Knight en lugar de la del chip (Assets/Art/Resources/Hollow/
    // musica, fuera de git; tools/hollow/musica.py): la del castillo y cada mundo, la de los santuarios y la de cada jefe.
    // Cambia con un fundido. Mientras el chip toca una fanfarria (objeto, invocación, jefe vencido…) se baja, para oírla.
    public class HollowMusic : MonoBehaviour
    {
        const float Fade = 1.2f;
        AudioSource a, b;              // la que suena y la que entra
        string current;

        public static bool Available => Resources.Load<AudioClip>("Hollow/musica/castillo") != null;

        void Awake()
        {
            a = gameObject.AddComponent<AudioSource>();
            b = gameObject.AddComponent<AudioSource>();
            foreach (var s in new[] { a, b }) { s.loop = true; s.volume = 0; s.playOnAwake = false; }
        }

        // La pista que toca (null: ninguna).
        public void Play(string track)
        {
            if (track == current) return;
            current = track;
            var clip = track != null ? Resources.Load<AudioClip>("Hollow/musica/" + track) : null;
            // La que sonaba pasa a b y se apaga; la nueva entra en a.
            (a, b) = (b, a);
            a.clip = clip;
            a.volume = 0;
            if (clip != null) a.Play(); else a.Stop();
        }

        public bool Ducked;            // una fanfarria del chip suena encima
        public GameBootstrap game;

        void Update()
        {
            if (game != null)
            {
                // La de ahora, según lo que pediría el original; el chip calla en las de fondo y suena en las fanfarrias.
                string track = game.HollowTrack();
                Play(track);
                var snd = Sound.Instance;
                int song = snd != null ? snd.CurrentSong : -1;
                bool background = GameBootstrap.IsBackgroundSong(song);
                if (snd != null) snd.MusicMuted = track != null && (background || song < 0);
                Ducked = track != null && song >= 0 && !background;
            }
            float target = GameSettings.HollowMusicVolume * (Ducked ? 0.25f : 1f);
            float step = Time.unscaledDeltaTime / Fade;
            if (a.clip != null) a.volume = Mathf.MoveTowards(a.volume, target, step * Mathf.Max(target, 0.05f));
            b.volume = Mathf.MoveTowards(b.volume, 0, step);
            if (b.volume <= 0 && b.isPlaying) b.Stop();
        }
    }
}
