using System.Collections;
using System.IO;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Capturas de los menús (-capturaMenu <carpeta>): el principal, sus opciones y sus extras, y el de pausa en la
    // partida. Solo existe para pruebas.
    public class MenuCapture : MonoBehaviour
    {
        static string dir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturaMenu");
            if (i < 0 || i + 1 >= args.Length) return;
            dir = args[i + 1];
            AttractMode.Current = AttractMode.Mode.Attract;
            Application.runInBackground = true;
            var go = new GameObject("Capturas de menús");
            DontDestroyOnLoad(go);
            go.AddComponent<MenuCapture>();
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, name + ".png"));
            yield return null;
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(dir);
            yield return new WaitForSeconds(3f);
            var menu = FindAnyObjectByType<HollowMainMenu>();
            if (menu == null) { Debug.LogError("[Capturas] sin menú principal"); Application.Quit(); yield break; }
            yield return Shot("menu_principal");
            menu.Show(1);
            yield return new WaitForSeconds(0.5f);
            yield return Shot("menu_opciones");
            menu.Show(2);
            yield return new WaitForSeconds(0.5f);
            yield return Shot("menu_extras");
            AttractMode.Go(AttractMode.Mode.Game);
            yield return new WaitForSeconds(4f);
            var pause = FindAnyObjectByType<HollowPauseMenu>();
            if (pause != null) { pause.ShowNow(); yield return new WaitForSeconds(0.4f); yield return Shot("menu_pausa"); }
            Application.Quit();
        }
    }
}
