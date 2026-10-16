using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Efectos de Hollow Knight (Assets/Art/Resources/Hollow/efectos, fuera de git; los importa tools/hollow/efectos.py):
    // animaciones de una vez (chispas, grieta, polvo) y piezas sueltas del atlas de impactos (destello, raya, abanicos,
    // gotas) que aparecen, crecen y se desvanecen, como hace Hollow Knight con ellas. Solo aspecto. HitEffects los usa
    // si están; si no, sigue con sus partículas.
    public class HollowFx : MonoBehaviour
    {
        public const string Folder = "Hollow/efectos";

        class Fx
        {
            public SpriteRenderer sr;
            public FrameClip clip;            // null = una pieza suelta
            public float t, life, w0, w1, a0;
            public Vector2 vel;
        }

        ArtLibrary lib;
        readonly Dictionary<string, float> fps = new Dictionary<string, float>();
        readonly List<Fx> live = new List<Fx>();
        readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>();
        Material additive, alpha;

        public bool Ready => lib != null && lib.Count > 0;

        public static HollowFx Create(Transform parent, Material additive, Material alpha)
        {
            var lib = ArtLibrary.Load(Folder);
            if (lib.Count == 0) return null;
            var fx = new GameObject("Efectos de Hollow Knight").AddComponent<HollowFx>();
            fx.transform.SetParent(parent, false);
            fx.lib = lib;
            fx.additive = additive;
            fx.alpha = alpha;
            var txt = Resources.Load<TextAsset>(Folder + "/fps");
            if (txt != null)
                foreach (var line in txt.text.Split('\n'))
                {
                    var p = line.Trim().Split(' ');
                    if (p.Length == 2 && float.TryParse(p[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float f))
                        fx.fps[p[0]] = f;
                }
            Debug.Log($"[HollowFx] {lib.Count} efectos");
            return fx;
        }

        // Una pieza o animación en 'at' (unidades de Unity). Anchos en píxeles del original, de w0 a w1 a lo largo de su
        // vida; 'light' = aditivo (brillos) o normal (polvo). Para una animación, la vida es la del clip si life <= 0.
        public void Play(string name, Vector2 at, float w0, float w1, float life, float rotDeg = 0, bool flip = false,
            bool light = true, float alpha0 = 1, Vector2 vel = default, int order = 15)
        {
            var clip = lib.Clip(name);
            if (clip == null || clip.Length == 0) return;
            bool anim = clip.Length > 1;
            if (anim)
            {
                clip.Fps = fps.TryGetValue(name, out float f) ? f : FrameAnimator.DefaultFps;
                if (life <= 0) life = clip.Duration;
            }
            var sr = pool.Count > 0 ? pool.Pop() : NewRenderer();
            sr.gameObject.SetActive(true);
            sr.sprite = clip.Frames[0];
            sr.sharedMaterial = light ? additive : alpha;
            sr.sortingOrder = order;
            sr.flipX = false;
            var tr = sr.transform;
            tr.position = new Vector3(at.x, at.y, 0);
            tr.rotation = Quaternion.Euler(0, 0, rotDeg);
            // Girar con la escala: los materiales propios no aplican flipX.
            tr.localScale = new Vector3(flip ? -1 : 1, 1, 1);
            var fx = new Fx { sr = sr, clip = anim ? clip : null, life = Mathf.Max(0.02f, life), w0 = w0, w1 = w1, a0 = alpha0, vel = vel };
            Apply(fx);
            live.Add(fx);
        }

        SpriteRenderer NewRenderer()
        {
            var go = new GameObject("Efecto");
            go.transform.SetParent(transform, false);
            return go.AddComponent<SpriteRenderer>();
        }

        void Apply(Fx fx)
        {
            float k = Mathf.Clamp01(fx.t / fx.life);
            if (fx.clip != null)
                fx.sr.sprite = fx.clip.Frames[Mathf.Min(fx.clip.Length - 1, (int)(fx.t * fx.clip.Fps))];
            // Ancho del sprite en píxeles del original (8 texels por píxel) → escala.
            float native = fx.sr.sprite.rect.width / 8f;
            float w = Mathf.Lerp(fx.w0, fx.w1, 1 - (1 - k) * (1 - k));
            float s = w / Mathf.Max(1, native);
            var sc = fx.sr.transform.localScale;
            fx.sr.transform.localScale = new Vector3(Mathf.Sign(sc.x) * s, s, 1);
            // Las animaciones ya se desvanecen solas; las piezas sueltas, en la segunda mitad de su vida.
            float a = fx.clip != null ? fx.a0 : fx.a0 * (k < 0.4f ? 1 : 1 - (k - 0.4f) / 0.6f);
            fx.sr.color = new Color(1, 1, 1, a);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = live.Count - 1; i >= 0; i--)
            {
                var fx = live[i];
                fx.t += dt;
                if (fx.t >= fx.life)
                {
                    fx.sr.gameObject.SetActive(false);
                    pool.Push(fx.sr);
                    live.RemoveAt(i);
                    continue;
                }
                fx.sr.transform.position += (Vector3)(fx.vel * dt);
                Apply(fx);
            }
        }
    }
}
