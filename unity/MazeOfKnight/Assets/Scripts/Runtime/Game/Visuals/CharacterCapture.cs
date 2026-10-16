using System.Collections;
using System.IO;
using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Capturas de los protagonistas moviéndose en el juego compilado, para revisar sus animaciones sin jugar. Con el
    // argumento -capturaPersonajes <carpeta>: con Popolon y luego con Aphrodite, teclas guionizadas como una demo
    // (quieto, correr, estocada, saltar) y una captura en cada momento. Solo existe para pruebas.
    public class CharacterCapture : MonoBehaviour
    {
        static string dir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturaPersonajes");
            if (i < 0 || i + 1 >= args.Length) return;
            dir = args[i + 1];
            AttractMode.Current = AttractMode.Mode.Game;
            Application.runInBackground = true;
            var go = new GameObject("Capturas de personajes");
            DontDestroyOnLoad(go);
            go.AddComponent<CharacterCapture>();
        }

        static DemoInputEntry Keys(int ticks, bool left = false, bool right = false, bool up = false, bool space = false) =>
            new DemoInputEntry { frames = ticks, keys = 1, left = left, right = right, up = up, space = space };

        // Espera a que se cumpla, como mucho 3 s (si el guion se tuerce, la captura sale igual).
        static IEnumerator Until(System.Func<bool> cond)
        {
            float t = Time.time;
            while (!cond() && Time.time - t < 3f) yield return null;
        }

        IEnumerator Shot(string name)
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, name + ".png"));
            yield return null;
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(dir);
            yield return new WaitForSeconds(5f);       // sin el nombre del mapa
            var game = FindAnyObjectByType<GameBootstrap>();
            var player = FindAnyObjectByType<PlayerController>();
            if (game == null || player == null) { Application.Quit(); yield break; }
            var room = game.Motor.Room;
            int x0 = game.Motor.X, y0 = game.Motor.Y;
            foreach (var c in new[] { Character.Popolon, Character.Aphrodite })
            {
                string p = c == Character.Popolon ? "popolon" : "aphrodite";
                game.Motor.Character = c;
                game.Motor.Place(game.World, room, x0, y0);
                yield return new WaitForSeconds(0.8f);
                yield return Shot($"{p}_0_quieto");
                // Correr a la derecha, estocada, saltar, correr a la izquierda.
                player.Demo = new DemoPlayer(new DemoInfo
                {
                    inputs = new[]
                    {
                        Keys(12, right: true), Keys(1, space: true), Keys(8), Keys(1, up: true), Keys(30),
                        Keys(12, left: true), Keys(1, space: true), Keys(20), new DemoInputEntry { keys = 0xFF },
                    },
                });
                float t0 = Time.time;
                yield return Until(() => Time.time - t0 > 8 / 30f);
                yield return Shot($"{p}_1_correr");
                yield return Until(() => game.Motor.AttackTicks > 0);
                yield return Shot($"{p}_2_estocada");
                yield return Until(() => game.Motor.State == MotorState.Jump);
                yield return new WaitForSeconds(0.1f);
                yield return Shot($"{p}_3_salto_subir");
                yield return Until(() => game.Motor.State == MotorState.Fall);
                yield return new WaitForSeconds(0.1f);
                yield return Shot($"{p}_4_salto_caer");
                yield return Until(() => game.Motor.FacingLeft && game.Motor.AttackTicks > 0);
                yield return Shot($"{p}_5_estocada_izquierda");
                yield return Until(() => player.Demo == null || player.Demo.Finished);
                player.Demo = null;
            }
            Application.Quit();
        }
    }
}
