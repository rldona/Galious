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
            menu.Show(4);
            yield return new WaitForSeconds(0.2f);
            yield return Shot("aviso_autoguardado");
            menu.Show(0);
            yield return new WaitForSeconds(0.5f);
            yield return Shot("menu_principal");
            yield return new WaitForSeconds(2.5f);
            float peak = 0;
            for (int i = 0; i < 60; i++) { peak = Mathf.Max(peak, Sound.OutputLevel()); yield return null; }
            Debug.Log($"[Capturas] sonido en el menú: {peak:0.0000} ({menu.MusicState}; listeners {FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length})");
            menu.Show(1);
            yield return new WaitForSeconds(0.5f);
            yield return Shot("menu_opciones");
            foreach (var page in new[] { OptionsMenu.Page.Audio, OptionsMenu.Page.Video, OptionsMenu.Page.Brightness, OptionsMenu.Page.Pad, OptionsMenu.Page.Keys })
            {
                menu.Show(1, page);
                yield return new WaitForSeconds(0.4f);
                yield return Shot("opciones_" + page.ToString().ToLowerInvariant());
            }
            menu.Show(3);
            yield return new WaitForSeconds(0.4f);
            yield return Shot("menu_salir");
            menu.Show(2);
            yield return new WaitForSeconds(0.5f);
            yield return Shot("menu_extras");
            AttractMode.Go(AttractMode.Mode.Game);
            yield return new WaitForSeconds(4f);
            // Latencia: sin música, cuánto tarda el efecto de la espada (0x13) en salir por los altavoces.
            Sound.Instance.MusicMuted = true;
            yield return new WaitForSeconds(0.5f);
            float t0 = Time.realtimeSinceStartup, heard = -1;
            Sound.Effect(0x13);
            while (Time.realtimeSinceStartup - t0 < 1f)
            {
                if (Sound.OutputLevel() > 0.005f) { heard = Time.realtimeSinceStartup - t0; break; }
                yield return null;
            }
            Debug.Log($"[Capturas] la espada suena a los {heard * 1000:0} ms");
            Sound.Instance.MusicMuted = false;
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
                var pad = game.GetComponent<WordPad>();
                if (pad != null)
                {
                    pad.ShowNow();
                    yield return new WaitForSeconds(0.4f);
                    yield return Shot("escribir_mando");
                    pad.HideNow();
                }
                var hj = game.GetComponent<HuntersJournal>();
                if (hj != null)
                {
                    hj.ShowToast(HuntersJournal.Of(0x27));
                    yield return new WaitForSeconds(0.8f);
                    yield return Shot("diario_aviso");
                }
            }
            // El final: la escena del castillo y el epílogo.
            var end = game != null ? game.GetComponent<EndingScreen>() : null;
            var endJson = Resources.Load<TextAsset>("Ending");
            if (end != null && endJson != null)
            {
                end.Begin(JsonUtility.FromJson<MazeOfKnight.Data.EndingData>(endJson.text), new Vector2(120, 120), new Vector2(128, 104));
                end.DebugJump(3, 700);
                yield return new WaitForSeconds(0.3f);
                yield return Shot("final_castillo");
                foreach (int f in new[] { 900, 1300, 1700 })
                {
                    end.DebugJump(4, f);
                    yield return new WaitForSeconds(0.2f);
                    yield return Shot($"final_epilogo_{f}");
                }
            }
            Application.Quit();
        }
    }
}
