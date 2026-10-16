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
            if (pause != null) pause.HideNow();
            // Inventario, amuletos y una lámina, con objetos de prueba.
            var game = FindAnyObjectByType<GameBootstrap>();
            var inv = game != null ? game.GetComponent<InventoryMenu>() : null;
            if (inv != null && RemakeRules.HollowCharms)
            {
                foreach (int t in new[] { 1, 2, 3, 5, 0x0D, 0x13, 0x15, 0x16, 0x20, 0x27, 0x2A, 0x0E, 0x11, 0x19, 0x1B, 0x25, 0x28, 0x2B })
                    game.Inventory.Add(t);
                game.SelectedWeapon = 1;
                game.Arrows = 120; game.Coins = 340; game.Keys = 3;
                yield return new WaitForSeconds(0.3f);
                inv.ShowTab(0);
                yield return new WaitForSeconds(0.4f);
                yield return Shot("inventario");
                inv.ShowTab(1);
                yield return new WaitForSeconds(0.4f);
                yield return Shot("amuletos");
                inv.ShowTab(2);
                yield return new WaitForSeconds(0.4f);
                yield return Shot("mapa");
                if (HuntersJournal.Available)
                {
                    foreach (int t in new[] { 0x0E, 0x0E, 0x25, 0x0A, 0x18, 0x101 }) game.Journal.Killed(t);
                    for (int i = 0; i < 9; i++) game.Journal.Killed(0x0E);
                    inv.ShowJournal(0x0E);
                    yield return new WaitForSeconds(0.4f);
                    yield return Shot("diario");
                    inv.ShowJournal(0x25);
                    yield return new WaitForSeconds(0.4f);
                    yield return Shot("diario_sin_notas");
                }
                inv.Toggle();
                var tut = game.GetComponent<ItemTutorial>();
                if (tut != null)
                {
                    tut.ShowNow(0x15);
                    yield return new WaitForSeconds(1.2f);
                    yield return Shot("lamina_objeto");
                    tut.ShowNow(0x19);
                    yield return new WaitForSeconds(1.2f);
                    yield return Shot("lamina_amuleto");
                    tut.HideNow();
                }
                var hj = game.GetComponent<HuntersJournal>();
                if (hj != null)
                {
                    hj.ShowToast(HuntersJournal.Of(0x27));
                    yield return new WaitForSeconds(0.8f);
                    yield return Shot("diario_aviso");
                }
            }
            Application.Quit();
        }
    }
}
