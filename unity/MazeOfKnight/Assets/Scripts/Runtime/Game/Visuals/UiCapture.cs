using System.Collections;
using System.IO;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Capturas de la interfaz en el juego compilado, para revisarla sin jugar (la interfaz es OnGUI y no sale en
    // Camera.Render). Con el argumento -capturaInterfaz <carpeta>: empieza la partida, guarda el marcador, da unos
    // objetos y abre el menú F1, lo guarda y sale. Solo existe para pruebas; sin el argumento no hace nada.
    public class UiCapture : MonoBehaviour
    {
        static string dir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturaInterfaz");
            if (i < 0 || i + 1 >= args.Length) return;
            dir = args[i + 1];
            AttractMode.Current = AttractMode.Mode.Game;
            Application.runInBackground = true;      // sin la ventana delante, el juego se pararía
            var go = new GameObject("Capturas de la interfaz");
            DontDestroyOnLoad(go);
            go.AddComponent<UiCapture>();
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(dir);
            yield return new WaitForSeconds(1.5f);
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, "interfaz_marcador.png"));
            yield return new WaitForSeconds(4f);          // ya sin el nombre del mapa
            var game = FindAnyObjectByType<GameBootstrap>();
            if (game != null && game.Inventory != null)
            {
                foreach (int t in new[] { 1, 2, 6, 0x07, 0x08, 0x0C, 0x0E, 0x15, 0x1A, 0x21, 0x27, 0x2B })
                    game.Inventory.Add(t);
                game.SelectedWeapon = 1;
                game.Arrows = 120;
                game.Coins = 85;
                game.Keys = 7;
                game.PendingVitality[1] = -3;
                game.Say("Captura de la interfaz");
                yield return new WaitForSeconds(0.4f);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, "interfaz_marcador_objetos.png"));
                yield return new WaitForSeconds(0.5f);
                var menu = game.GetComponent<InventoryMenu>();
                if (menu != null) menu.Toggle();
                yield return new WaitForSeconds(0.6f);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, "interfaz_menu.png"));
                yield return new WaitForSeconds(0.6f);
            }
            Application.Quit();
        }
    }
}
