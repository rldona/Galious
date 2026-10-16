using System.Collections;
using System.IO;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Capturas de los bancos (-capturaBancos <carpeta>): de pie junto a unos cuantos y sentado; luego, con geo, morir
    // lejos y despertar en el último banco, y la sombra donde se murió. Solo existe para pruebas.
    public class BenchCapture : MonoBehaviour
    {
        static string dir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-capturaBancos");
            if (i < 0 || i + 1 >= args.Length) return;
            dir = args[i + 1];
            AttractMode.Current = AttractMode.Mode.Game;
            Application.runInBackground = true;
            var go = new GameObject("Capturas de bancos");
            DontDestroyOnLoad(go);
            go.AddComponent<BenchCapture>();
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
            yield return new WaitForSeconds(4f);
            var game = FindAnyObjectByType<GameBootstrap>();
            if (game == null) { Application.Quit(); yield break; }
            CollisionGridOverlay.Show = HitboxOverlay.Show = false;
            foreach (var it in game.Inventory.Data.items)
                if (it.effect != null && it.effect.kind == "worldEntry") game.Inventory.Add(it.type);
            var all = game.DebugBenches;
            Debug.Log($"[Capturas] {all.Count} bancos: " + string.Join(", ", System.Linq.Enumerable.Select(all, b => $"{b.Map}/{b.Room}")));
            // Unos cuantos: el del castillo y los de varios mundos.
            int n = 0;
            foreach (var b in all)
            {
                if (!(b.Map == "castle" || b.Room != 1)) continue;   // el del castillo y los de junto al jefe
                game.DebugGoToBench(b);
                yield return new WaitForSeconds(1.0f);
                yield return Shot($"banco_{n:00}_{b.Map}_{b.Room}_de_pie");
                game.Vitality[(int)game.Motor.Character] = 2;      // para ver las máscaras rellenándose
                game.DebugSit(b);
                yield return new WaitForSeconds(0.35f);
                yield return Shot($"banco_{n:00}_{b.Map}_{b.Room}_sentandose");
                yield return new WaitForSeconds(0.9f);
                yield return Shot($"banco_{n:00}_{b.Map}_{b.Room}_sentado");
                n++;
            }
            // Concentrarse: con el alma llena y poca vida, mantener el botón.
            var pc = FindAnyObjectByType<PlayerController>();
            game.DebugGoTo("castle", 1);
            yield return new WaitForSeconds(0.8f);
            game.Soul.Value = Soul.Max;
            game.Vitality[(int)game.Motor.Character] = 2;
            pc.DebugHoldFocus = true;
            yield return new WaitForSeconds(0.6f);
            yield return Shot("alma_1_concentrando");
            yield return new WaitUntil(() => game.Motor.Pose == "curarse");
            yield return new WaitForSeconds(0.08f);
            yield return Shot("alma_2_curandose");
            yield return new WaitForSeconds(2.4f);
            pc.DebugHoldFocus = false;
            yield return new WaitForSeconds(1.0f);
            yield return Shot("alma_3_despues");
            Debug.Log($"[Capturas] alma {game.Soul.Value}, vitalidad {game.Vitality[(int)game.Motor.Character]}");
            // Morir con geo lejos del último banco: despertar en él y la sombra donde se murió.
            var last = all[all.Count - 1];
            foreach (var b in all) if (b.Map == "world01") { last = b; break; }
            game.DebugGoToBench(last);
            game.DebugSit(last);
            yield return new WaitForSeconds(0.5f);
            game.Coins = 137;
            game.DebugGoTo("world01", 5);
            yield return new WaitForSeconds(1.0f);
            int shadeRoom = game.Motor.Room.id;
            game.DebugDie();
            yield return new WaitForSeconds(0.4f);
            yield return Shot("muerte_1_despertar");
            yield return new WaitForSeconds(1.2f);
            yield return Shot("muerte_2_sentado");
            game.DebugGoTo("world01", shadeRoom);
            yield return new WaitForSeconds(1.5f);
            yield return Shot("muerte_3_sombra");
            Debug.Log($"[Capturas] geo tras morir: {game.Coins}");
            // Cornifer en cada mundo, su pregunta, y el mapa del mundo 1 a lápiz y entintado.
            if (RemakeRules.HollowMaps && game.CorniferView != null)
            {
                Debug.Log("[Capturas] Cornifer: " + string.Join(", ", System.Linq.Enumerable.Select(game.CorniferList.All,
                    c => $"mundo {c.World} sala {c.Room} a {c.Distance} salas")));
                foreach (var c in game.CorniferList.All)
                {
                    game.DebugGoToCornifer(c.World);
                    yield return new WaitForSeconds(0.8f);
                    yield return Shot($"cornifer_{c.World}");
                }
                game.DebugGoToCornifer(1);
                yield return new WaitForSeconds(0.6f);
                game.Coins = 200;
                game.CorniferView.DebugPrompt(1);
                yield return new WaitForSeconds(0.5f);
                yield return Shot("cornifer_pregunta");
                game.CorniferView.Talk(1);
                yield return new WaitForSeconds(0.5f);
                yield return Shot("cornifer_habla");
                game.CorniferView.DebugClose();
                var inv = game.GetComponent<InventoryMenu>();
                foreach (int room in new[] { 1, 2, 3, 4, 5, 6 }) game.VisitedRooms.Add(("world01", room));
                inv.ShowTab(2);
                yield return new WaitForSeconds(0.4f);
                yield return Shot("mapa_boceto");
                inv.Toggle();
                game.BuyMap(1);
                inv.ShowTab(2);
                yield return new WaitForSeconds(0.4f);
                yield return Shot("mapa_entintado");
                inv.Toggle();
            }
            // RemakeRules.HollowSlashes: el espadazo hacia arriba de pie y hacia abajo en el aire.
            if (RemakeRules.HollowSlashes)
            {
                var bench = game.DebugBenches[0];
                game.DebugGoToBench(bench);
                yield return new WaitForSeconds(0.6f);
                for (int i = 0; i < 10 && game.Motor.AttackTicks > 0; i++) game.Motor.Tick(new MotorInput());
                game.Motor.Tick(new MotorInput());
                game.Motor.Tick(new MotorInput { Attack = true, AimUp = true });
                Debug.Log($"[Capturas] espadazo: dir {game.Motor.SlashDir}, ticks {game.Motor.AttackTicks}, {game.Motor.State}, pausa {game.player.Paused}, pose {game.Motor.Pose}, sentado {game.player.Seated}");
                yield return null;
                yield return Shot("espadazo_arriba");
                yield return new WaitForSeconds(0.6f);
                game.Motor.Place(game.World, game.Motor.Room, game.Motor.X, game.Motor.Y - 40);
                for (int i = 0; i < 10 && game.Motor.AttackTicks > 0; i++) game.Motor.Tick(new MotorInput());
                game.Motor.Tick(new MotorInput());
                game.Motor.Tick(new MotorInput { Attack = true, AimDown = true });
                Debug.Log($"[Capturas] espadazo abajo: dir {game.Motor.SlashDir}, ticks {game.Motor.AttackTicks}, {game.Motor.State}, pausa {game.player.Paused}, menú {game.MenuOpen}");
                yield return null;
                yield return Shot("espadazo_abajo");
            }
            Application.Quit();
        }
    }
}
