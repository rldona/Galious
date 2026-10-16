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
                if (menu != null) menu.Toggle();
                // Cajas de colisión (F9) en salas con enemigos.
                HitboxOverlay.Show = true;
                foreach (int id in new[] { 45, 20, 90 })
                {
                    var room = game.World.Room(id);
                    if (room == null) continue;
                    game.Motor.Place(game.World, room, 128, 120);
                    yield return new WaitForSeconds(1.2f);
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir, $"interfaz_cajas_{id}.png"));
                    yield return new WaitForSeconds(0.4f);
                }
                HitboxOverlay.Show = false;
                // Chispas: un golpe y una muerte forzados contra el primer enemigo de la sala, y una piedra que se rompe.
                foreach (var e in game.Enemies.Enemies)
                    if (e.Alive && e.Room == game.Motor.Room)
                    {
                        HitEffects.EnemyHit(e, game.Motor);
                        HitEffects.EnemyKilled(e, new Color(0.82f, 0.82f, 0.88f));
                        break;
                    }
                foreach (var r in game.Motor.Room.rocks) { HitEffects.RockHit(game.Motor.Room, r.x, r.y, true); break; }
                yield return new WaitForSeconds(0.08f);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, "golpes_chispas.png"));
                yield return new WaitForSeconds(0.4f);
                // Puerta del mundo 1 (castillo, sala 104): cerrada, abriéndose y abierta.
                var doorRoom = game.World.Room(104);
                if (doorRoom != null && doorRoom.doorWorld == 1)
                {
                    game.Motor.Place(game.World, doorRoom, doorRoom.door.x - 24, doorRoom.door.y + 0x18);
                    yield return new WaitForSeconds(0.8f);
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir, "puerta_0_cerrada.png"));
                    yield return new WaitForSeconds(0.3f);
                    game.Doors.KeyTaken(0);
                    game.Motor.Place(game.World, doorRoom, doorRoom.door.x + 16, doorRoom.door.y + 0x18);
                    yield return new WaitForSeconds(0.32f);
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir, "puerta_1_abriendo.png"));
                    yield return new WaitForSeconds(0.8f);
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir, "puerta_2_abierta.png"));
                    yield return new WaitForSeconds(0.3f);
                }
                // Santuarios: uno de cada dios (la tienda de Ares, la primera).
                foreach (var god in new[] { "Ares", "Hephaestus", "Demeter", "Asclepius", "Death", "Athena" })
                {
                    game.GetComponent<ItemTutorial>()?.HideNow();   // sin láminas de tutorial pendientes encima
                    if (!game.DebugOpenShrine(god)) continue;
                    yield return new WaitForSeconds(0.8f);
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir, god == "Ares" ? "interfaz_santuario.png" : $"interfaz_santuario_{god.ToLowerInvariant()}.png"));
                    yield return new WaitForSeconds(0.5f);
                    game.GetComponent<ShrineScreen>()?.Close();
                    yield return new WaitForSeconds(0.5f);
                }
                // Jefe del mundo 1: la sala se apaga al invocarlo y queda en penumbra roja durante el combate.
                game.DebugEnterWorld(1);
                yield return new WaitForSeconds(1.5f);
                game.DebugGoToBoss();
                yield return new WaitForSeconds(1.0f);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, "jefe_0_sala.png"));
                foreach (char ch in game.Bosses.Info.word) { game.Bosses.Type(ch); yield return new WaitForSeconds(0.15f); }
                yield return new WaitForSeconds(2.5f);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, "jefe_1_invocando.png"));
                yield return new WaitForSeconds(5.5f);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir, "jefe_2_combate.png"));
                yield return new WaitForSeconds(0.5f);
            }
            Application.Quit();
        }
    }
}
