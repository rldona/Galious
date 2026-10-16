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
        static bool bossesOnly;      // -capturaJefes <carpeta> [mundo]: solo los combates (o el de ese mundo; un combate
        static int bossWorld;        // empezado sigue activo en los demás mundos, así que uno por ejecución)

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturaPersonajes");
            int j = System.Array.IndexOf(args, "-capturaJefes");
            if (j >= 0 && j + 1 < args.Length)
            {
                i = j;
                bossesOnly = true;
                if (j + 2 < args.Length) int.TryParse(args[j + 2], out bossWorld);
            }
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

        // Coloca al jugador, reproduce las teclas y hace la captura cuando se cumple la condición (y 'after' segundos).
        IEnumerator Act(GameBootstrap game, PlayerController player, RoomModel room, int x, int y, DemoInputEntry[] keys,
            System.Func<bool> when, string name, float after = 0)
        {
            player.Demo = null;
            game.Motor.Place(game.World, room, x, y);
            yield return new WaitForSeconds(0.3f);
            var list = new System.Collections.Generic.List<DemoInputEntry>(keys) { new DemoInputEntry { keys = 0xFF } };
            player.Demo = new DemoPlayer(new DemoInfo { inputs = list.ToArray() });
            yield return Until(when);
            if (after > 0) yield return new WaitForSeconds(after);
            yield return Shot(name);
            yield return Until(() => player.Demo == null || player.Demo.Finished);
            player.Demo = null;
        }

        // Jefes: en cada mundo, a la sala del jefe, se escribe su palabra y se fotografía el combate.
        IEnumerator Bosses(GameBootstrap game)
        {
            // Los objetos que piden algunos mundos para entrar (el Necklace del 7…), como si ya se tuvieran.
            foreach (var it in game.Inventory.Data.items)
                if (it.effect != null && it.effect.kind == "worldEntry") game.Inventory.Add(it.type);
            for (int w = bossWorld > 0 ? bossWorld : 1; w <= (bossWorld > 0 ? bossWorld : 10); w++)
            {
                game.DebugEnterWorld(w);
                yield return new WaitForSeconds(1.2f);
                game.DebugGoToBoss();
                yield return new WaitForSeconds(1.0f);
                if (game.Bosses?.Info == null) continue;
                Debug.Log($"[Capturas] mundo {w}: escuchando {game.Bosses.Listening}, palabra {game.Bosses.Info.word}, sala del jefe {game.Bosses.Info.room}, jugador en {game.Motor.Room.id}");
                // Como si se tecleara: una letra repetida necesita soltar la tecla antes (BossFight.KeysReleased).
                foreach (char ch in game.Bosses.Info.word)
                {
                    game.Bosses.KeysReleased();
                    game.Bosses.Type(ch);
                    yield return new WaitForSeconds(0.12f);
                }
                Debug.Log($"[Capturas] mundo {w}: tras la palabra, fase {game.Bosses.Current}");
                yield return new WaitForSeconds(6.5f);
                yield return Shot($"jefe_mundo{w:00}_a");
                yield return new WaitForSeconds(1.5f);
                yield return Shot($"jefe_mundo{w:00}_b");
            }
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
            game.DebugInvulnerable = true;
            if (bossesOnly) { yield return Bosses(game); Application.Quit(); yield break; }
            var room = game.Motor.Room;
            int x0 = game.Motor.X, y0 = game.Motor.Y;
            foreach (var c in new[] { Character.Popolon, Character.Aphrodite })
            {
                string p = c == Character.Popolon ? "popolon" : "aphrodite";
                game.Motor.Character = c;
                game.Motor.Place(game.World, room, x0, y0);
                yield return new WaitForSeconds(0.8f);
                yield return Shot($"{p}_0_quieto");
                // Cada acción, una demo corta desde el mismo sitio.
                yield return Act(game, player, room, x0, y0, new[] { Keys(10, right: true), Keys(20) },
                    () => game.Motor.X > x0 + 6, $"{p}_1_correr");
                yield return Act(game, player, room, x0, y0, new[] { Keys(2, right: true), Keys(1, space: true), Keys(10) },
                    () => game.Motor.AttackTicks >= 3, $"{p}_2_estocada");
                yield return Act(game, player, room, x0, y0, new[] { Keys(2, left: true), Keys(1, space: true), Keys(10) },
                    () => game.Motor.FacingLeft && game.Motor.AttackTicks >= 3, $"{p}_3_estocada_izquierda");
                yield return Act(game, player, room, x0, y0, new[] { Keys(12, up: true), Keys(30) },
                    () => game.Motor.State == MotorState.Jump, $"{p}_4_salto_subir", 0.1f);
                yield return Act(game, player, room, x0, y0, new[] { Keys(12, up: true), Keys(30) },
                    () => game.Motor.State == MotorState.Fall, $"{p}_5_salto_caer", 0.1f);
                // Escalera de la sala 1 del castillo (x = 120): subir unos pasos.
                yield return Act(game, player, room, 120, y0, new[] { Keys(24, up: true), Keys(10) },
                    () => game.Motor.State == MotorState.Ladder && game.Motor.Y < y0 - 12, $"{p}_6_escalera");
                // Cajas de colisión (F9): el dibujo tiene que estar sobre la caja del jugador y la de la espada.
                HitboxOverlay.Show = true;
                yield return Act(game, player, room, x0, y0, new[] { Keys(2, right: true), Keys(1, space: true), Keys(10) },
                    () => game.Motor.AttackTicks >= 3, $"{p}_7_cajas_estocada");
                yield return Act(game, player, room, x0, y0, new[] { Keys(2, left: true), Keys(10) },
                    () => game.Motor.FacingLeft, $"{p}_8_cajas_quieto", 0.3f);
                HitboxOverlay.Show = false;
            }
            // Efectos de Hollow Knight, uno al lado de otro, en dos momentos.
            var fx = HitEffects.Hollow;
            if (fx != null)
            {
                player.Demo = null;
                game.Motor.Place(game.World, room, 16, y0);
                yield return new WaitForSeconds(0.5f);
                string[] names = { "raya", "destello", "abanico_a", "gotas", "chispas", "grieta", "destello_raya", "polvo_pared", "polvo_dash", "polvo_salto" };
                foreach (float when in new[] { 0.04f, 0.1f })
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        var at = RoomCoords.PixelToWorld(room, 36 + i * 22, y0 - 40 - (i % 2) * 30);
                        bool light = !names[i].StartsWith("polvo");
                        fx.Play(names[i], at, 18, 18, names[i].Contains("_") && !names[i].StartsWith("abanico") && names[i] != "destello_raya" ? 0 : 0.4f, 0, false, light);
                    }
                    yield return new WaitForSeconds(when);
                    yield return Shot($"efectos_{(int)(when * 100):00}");
                    yield return new WaitForSeconds(0.6f);
                }
            }
            // Enemigos del castillo con su dibujo de Hollow Knight y las cajas: una sala por tipo.
            HitboxOverlay.Show = true;
            player.Demo = null;
            foreach (int id in new[] { 2, 3, 4, 5, 6, 7, 9, 13, 18, 20, 25, 49 })
            {
                var r = game.World.Room(id);
                if (r == null) continue;
                game.Motor.Place(game.World, r, 128, 120);
                yield return new WaitForSeconds(1.5f);
                yield return Shot($"enemigos_sala_{id:000}");
            }
            // Objetos en salas del castillo (Bible, Earrings, monedas, Pitcher) con su icono de Hollow Knight.
            foreach (int id in new[] { 11, 13, 18, 19 })
            {
                var r = game.World.Room(id);
                if (r == null) continue;
                game.Motor.Place(game.World, r, 128, 120);
                yield return new WaitForSeconds(1.0f);
                yield return Shot($"objetos_sala_{id:000}");
            }
            // Muertes (y su botín): en salas del castillo con Husks, Vengeflies y Baldurs, se mata a todos.
            foreach (int id in new[] { 20, 13, 3 })
            {
                var r = game.World.Room(id);
                if (r == null) continue;
                game.Motor.Place(game.World, r, 128, 120);
                yield return new WaitForSeconds(1.2f);
                foreach (var e in new System.Collections.Generic.List<Enemies.Enemy>(game.Enemies.Enemies))
                    if (e.Alive && e.Room == game.Motor.Room && !e.Invulnerable) game.Enemies.Damage(e, 99);
                yield return new WaitForSeconds(0.15f);
                yield return Shot($"muertes_sala_{id:000}_a");
                yield return new WaitForSeconds(0.35f);
                yield return Shot($"muertes_sala_{id:000}_b");
                yield return new WaitForSeconds(1.2f);
                yield return Shot($"muertes_sala_{id:000}_botin");
            }
            // Enemigos de los diez mundos: las salas que juntan todos sus tipos.
            var worldRooms = new[]
            {
                new[] { 8, 11 }, new[] { 12, 8, 5 }, new[] { 6, 9, 8, 11, 14 }, new[] { 2, 3, 5, 6 }, new[] { 2, 5, 6, 12 },
                new[] { 2, 5, 9, 11 }, new[] { 6, 12, 19, 16 }, new[] { 6, 3, 20 }, new[] { 2, 6, 7, 11, 14, 26 }, new[] { 3, 6 },
            };
            for (int w = 1; w <= 10; w++)
            {
                game.DebugEnterWorld(w);
                yield return new WaitForSeconds(1.5f);
                foreach (int id in worldRooms[w - 1])
                {
                    var r = game.World.Room(id);
                    if (r == null) continue;
                    game.Motor.Place(game.World, r, 128, 120);
                    yield return new WaitForSeconds(1.5f);
                    yield return Shot($"enemigos_mundo{w:00}_sala_{id:000}");
                }
            }
            Application.Quit();
        }
    }
}
