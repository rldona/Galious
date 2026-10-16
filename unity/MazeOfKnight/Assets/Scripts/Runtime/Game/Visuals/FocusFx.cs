using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Los efectos de luz de concentrarse de Hollow Knight (Light Effects, importados con el Caballero en su mismo lienzo:
    // popolon_efecto_concentrar y popolon_efecto_cura): mientras se concentra, la luz que sube a su alrededor (aparece y
    // luego en bucle); al curarse, el estallido. Encima del cuerpo, en aditivo. Solo aspecto (RemakeRules.HollowSoul).
    public class FocusFx : MonoBehaviour
    {
        public GameBootstrap game;
        PlayerController player;
        SpriteRenderer sr;
        readonly FrameAnimator anim = new FrameAnimator();
        FrameClip focus, burst;
        string playing;

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            var lib = player?.Art?.Drawn;
            focus = lib?.Clip("popolon_efecto_concentrar");
            burst = lib?.Clip("popolon_efecto_cura");
            if (focus != null) { focus.Fps = 20; focus.Loop = true; focus.LoopStart = Mathf.Min(7, focus.Length - 1); }
            if (burst != null) { burst.Fps = 20; burst.Loop = false; }
            sr = new GameObject("Luz de concentrarse").AddComponent<SpriteRenderer>();
            var add = Resources.Load<Shader>("Shaders/Additive");
            if (add != null) sr.sharedMaterial = new Material(add) { name = "Concentrarse (aditivo)" };
            sr.enabled = false;
        }

        void LateUpdate()
        {
            if (game == null || player == null || player.body == null || focus == null) return;
            string pose = game.Motor?.Pose;
            string want = pose == "curarse" && burst != null ? "cura" : pose == "concentrar" ? "concentrar" : null;
            if (want != playing)
            {
                if (want == "cura") anim.Play(burst, true);
                else if (want == "concentrar") anim.Play(focus, true);
                playing = want;
            }
            if (playing == null || playing == "cura" && anim.Finished) { sr.enabled = false; if (playing == "cura" && anim.Finished) playing = null; return; }
            anim.Tick(Time.deltaTime);
            sr.enabled = true;
            sr.sprite = anim.Current;
            sr.sortingOrder = player.body.sortingOrder + 2;
            var t = player.body.transform;
            sr.transform.position = t.position;
            // Girar con la escala: el material aditivo no aplica flipX.
            var sc = t.lossyScale;
            sr.transform.localScale = new Vector3(player.body.flipX ? -sc.x : sc.x, sc.y, 1);
        }
    }
}
