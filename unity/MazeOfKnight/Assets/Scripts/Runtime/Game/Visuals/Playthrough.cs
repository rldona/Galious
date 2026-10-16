using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game.Enemies;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Recorrido de pruebas (-recorrido <carpeta>): se pasa el juego entero de forma automática buscando fallos, con la
    // partida guardada aparte (SaveGame.Testing). Pasa por todas las salas del castillo y de los diez mundos, compra en
    // todas las tiendas y abre los demás santuarios, compra los mapas a los Cornifer, se sienta en un banco, muere y deja
    // la sombra, invoca y vence a los diez jefes (escribiendo su palabra; el golpe final es directo) y recoge cada Gran
    // Llave, y acaba con el final. Apunta cada error o excepción con el paso en que salió (y una captura), las salas donde
    // no se puede poner al Caballero o queda dentro de la roca, y los generadores con tipos sin implementar. Todo va a
    // <carpeta>/recorrido.txt. Solo existe para pruebas.
    public class Playthrough : MonoBehaviour
    {
        static string dir;
        string step = "arranque";
        readonly List<string> problems = new List<string>();
        readonly HashSet<string> shotSteps = new HashSet<string>();
        int errors, shots;
        GameBootstrap game;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-recorrido");
            if (i < 0 || i + 1 >= args.Length) return;
            dir = args[i + 1];
            AttractMode.Current = AttractMode.Mode.Game;
            Application.runInBackground = true;
            var go = new GameObject("Recorrido de pruebas");
            DontDestroyOnLoad(go);
            go.AddComponent<Playthrough>();
        }

        void OnEnable() => Application.logMessageReceived += OnLog;
        void OnDisable() => Application.logMessageReceived -= OnLog;

        void OnLog(string msg, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            errors++;
            string first = (stack ?? "").Split('\n').FirstOrDefault(l => l.Contains("MazeOfKnight")) ?? "";
            Problem($"{type}: {msg.Split('\n')[0]}  [{first.Trim()}]");
        }

        void Problem(string text)
        {
            string line = $"{step}: {text}";
            if (problems.Count < 400 && !problems.Contains(line)) problems.Add(line);
            Debug.Log("[Recorrido] PROBLEMA " + line);
            if (shots < 40 && shotSteps.Add(step)) { shots++; StartCoroutine(Shot($"problema_{shots:00}")); }
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(dir, name + ".png"));
        }

        static IEnumerator Wait(float s) { yield return new WaitForSecondsRealtime(s); }

        IEnumerator Start()
        {
            Directory.CreateDirectory(dir);
            yield return Wait(4f);
            game = FindAnyObjectByType<GameBootstrap>();
            if (game == null || game.Motor == null) { Problem("no hay partida"); Finish(); yield break; }
            game.DebugInvulnerable = true;
            foreach (var it in game.Inventory.Data.items)
                if (it.effect != null && it.effect.kind == "worldEntry") game.Inventory.Add(it.type);

            // -soloJefes 1,2,5: solo esos jefes (para repetir uno que falla), anotando dónde está el Caballero.
            var a = System.Environment.GetCommandLineArgs();
            int sj = System.Array.IndexOf(a, "-soloJefes");
            if (sj >= 0 && sj + 1 < a.Length)
            {
                foreach (var ws in a[sj + 1].Split(','))
                {
                    int w = int.Parse(ws);
                    game.DebugEnterWorld(w);
                    yield return Wait(1.2f);
                    yield return Boss(w);
                }
                Finish();
                yield break;
            }

            // 1. El castillo: todas las salas, sus tiendas y santuarios, su Cornifer, un banco y morir con geo.
            yield return Sweep("castle");
            yield return Shrines("castle");
            yield return Cornifer(0);
            yield return BenchAndDeath();

            // 2. Los mundos: entrar, todas las salas, santuario, Cornifer y el jefe con su llave.
            for (int w = 1; w <= 10; w++)
            {
                step = $"mundo {w}: entrar";
                game.DebugEnterWorld(w);
                yield return Wait(1.2f);
                if (game.mapName != Benches.MapName(w)) { Problem($"no se ha entrado (mapa {game.mapName})"); continue; }
                yield return Sweep(game.mapName);
                yield return Shrines(game.mapName);
                yield return Cornifer(w);
                yield return Boss(w);
            }

            // 3. El final: la sala del bebé, romper las piedras y tocarlo.
            yield return Ending();

            // 4. Menús.
            // Tras el final se vuelve al título: el inventario solo si sigue la partida.
            Finish();
        }

        IEnumerator Sweep(string map)
        {
            var world = game.World;
            var ids = world.Rooms.Where(r => !r.shrine).Select(r => r.id).OrderBy(i => i).ToList();
            int missing = 0;
            foreach (int id in ids)
            {
                step = $"{map} sala {id}";
                bool placed = true;
                try { game.DebugGoTo(map, id); }
                catch (System.Exception e) { Problem("no se puede ir: " + e.Message); placed = false; }
                if (!placed) continue;
                yield return Wait(0.3f);
                var m = game.Motor;
                if (m.Room == null || m.Room.id != id) { Problem($"el Caballero acaba en la sala {m.Room?.id}"); continue; }
                if (game.World.Solid(m.Room, m.X, m.Y - 6)) Problem($"el Caballero queda dentro de la roca en ({m.X}, {m.Y})");
                if (game.Enemies.Missing > 0) { missing++; Problem($"{game.Enemies.Missing} generadores con tipos sin implementar"); }
            }
            Debug.Log($"[Recorrido] {map}: {ids.Count} salas, {missing} con tipos sin implementar");
        }

        IEnumerator Shrines(string map)
        {
            var screen = game.DebugShrineScreen;
            foreach (var s in game.DebugShrines.Where(x => x.map == map))
            {
                step = $"{map} santuario {s.god} ({s.kind}, sala {s.room})";
                game.Coins = 999;
                game.DebugOpenShrine(s);
                yield return Wait(0.5f);
                var v = screen.Visit;
                if (v == null) { Problem("no se abre"); continue; }
                if (s.kind == "shop")
                {
                    int want = s.items.Count(i => i.type != 0), bought = 0;
                    for (int i = 0; i < 6 && v.Cursor >= 0; i++)
                    {
                        if (v.Confirm()) bought++;
                        else if (v.Refused && v.ItemAt(v.Cursor) is ShrineItem it && it.rule == "shield") v.Move(1);   // escudo sin el anterior
                        yield return null;
                    }
                    if (bought == 0) Problem($"no se ha podido comprar nada ({want} objetos)");
                    Debug.Log($"[Recorrido] {step}: comprados {bought} de {want}");
                }
                else if (v.AskYesNo) { v.Confirm(); yield return null; }
                if (v.Text.Any(char.IsLetter) && ShrineTexts.Spanish(v.Text) == v.Text)
                    Problem($"texto sin traducir: {v.Text.Replace('\n', ' ')}");
                yield return Wait(0.3f);
                if (screen.Open) screen.Close();
                yield return Wait(0.4f);
            }
        }

        IEnumerator Cornifer(int world)
        {
            if (!game.CorniferList.All.Any(c => c.World == world)) yield break;
            step = $"Cornifer del mundo {world}";
            game.DebugGoToCornifer(world);
            yield return Wait(0.6f);
            if (!game.NearCornifer) Problem("el Caballero no queda junto a Cornifer");
            game.Coins = 999;
            game.CorniferView.DebugPrompt(world);
            yield return Wait(0.3f);
            if (!game.BuyMap(world)) Problem("no se puede comprar el mapa");
            game.CorniferView.DebugClose();
            yield return Wait(0.3f);
            if (!game.HasMapOf(world)) Problem("comprado, pero no se tiene el mapa");
        }

        IEnumerator BenchAndDeath()
        {
            var bench = game.DebugBenches.FirstOrDefault(b => b.Map == "castle");
            if (bench.Map == null) { Problem("no hay banco en el castillo"); yield break; }
            step = "banco del castillo";
            game.DebugGoToBench(bench);
            yield return Wait(0.5f);
            game.DebugSit(bench);
            yield return Wait(1f);
            step = "morir con geo";
            game.Coins = 77;
            game.DebugGoTo("castle", 40);
            yield return Wait(0.4f);
            game.DebugDie();
            yield return Wait(1.2f);
            if (game.Coins != 0) Problem($"al morir quedan {game.Coins} geo (se guardan en la sombra)");
            if (game.Motor.Room?.id != bench.Room) Problem($"no se despierta en el banco (sala {game.Motor.Room?.id})");
        }

        IEnumerator Boss(int world)
        {
            var info = game.Bosses.InfoFor(world);
            if (info == null) { step = $"mundo {world}: jefe"; Problem("sin datos del jefe"); yield break; }
            step = $"mundo {world}: jefe ({info.word})";
            game.DebugGoToBoss();
            yield return Wait(0.6f);
            if (!game.Bosses.Listening) { Problem($"en la sala del jefe no escucha la palabra (fase {game.Bosses.Current})"); yield break; }
            foreach (char ch in info.word)
            {
                game.TypeLetter(ch);
                yield return null;
                game.Bosses.KeysReleased();
                yield return null;
            }
            // Que aparezca: hace falta que no quede ningún enemigo en la sala.
            float t = 0;
            while (game.Bosses.Current != BossFight.Stage.Fighting && t < 25)
            {
                foreach (var e in game.Enemies.Enemies.ToList())
                    if (e.Alive && !(e is Boss)) e.Alive = false;
                t += 0.2f;
                yield return Wait(0.2f);
            }
            if (game.Bosses.Current != BossFight.Stage.Fighting) { Problem($"el jefe no aparece (fase {game.Bosses.Current})"); yield break; }
            step = $"mundo {world}: combate";
            yield return Wait(1.5f);
            t = 0;
            while (game.Bosses.Current == BossFight.Stage.Fighting && t < 15)
            {
                if (game.Enemies.ActiveBoss is Boss b) game.Enemies.HitBoss(b, 99);
                t += 0.2f;
                yield return Wait(0.2f);
            }
            if (game.Bosses.Current == BossFight.Stage.Fighting) { Problem("el jefe no muere"); yield break; }
            step = $"mundo {world}: Gran Llave";
            t = 0;
            int bossRoom = game.Motor.Room.id;
            while (game.Bosses.Current != BossFight.Stage.KeyOnFloor && t < 20)
            {
                if (game.Motor.Room.id != bossRoom)
                    Debug.Log($"[Recorrido] mundo {world}: el Caballero pasa a la sala {game.Motor.Room.id} en ({game.Motor.X}, {game.Motor.Y}), fase {game.Bosses.Current}");
                t += 0.2f;
                yield return Wait(0.2f);
            }
            if (world == 10) { if (game.Bosses.Current != BossFight.Stage.RightExitOnly) Problem($"Galious vencido, pero no se abre la salida derecha (fase {game.Bosses.Current})"); yield break; }
            if (game.Bosses.Current != BossFight.Stage.KeyOnFloor)
            {
                Problem($"no cae la llave (fase {game.Bosses.Current}; el Caballero en la sala {game.Motor.Room.id}, ({game.Motor.X}, {game.Motor.Y}))");
                yield break;
            }
            var (kx, ky) = game.Bosses.KeyPos;
            t = 0;
            while (game.Bosses.Current == BossFight.Stage.KeyOnFloor && t < 4)
            {
                game.Motor.Place(game.World, game.Motor.Room, kx + 8, ky + 16);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            if (game.Bosses.Current == BossFight.Stage.KeyOnFloor) Problem($"no se puede recoger la llave en ({kx}, {ky})");
            if (!game.Inventory.Has(Inventory.GreatKey) && world < 10) Problem("sin la Gran Llave en el inventario");
            t = 0;
            while (game.Bosses.Current != BossFight.Stage.Idle && game.Bosses.Current != BossFight.Stage.RightExitOnly && t < 20) { t += 0.2f; yield return Wait(0.2f); }
            if (game.Bosses.Current != BossFight.Stage.Idle && game.Bosses.Current != BossFight.Stage.RightExitOnly)
                Problem($"la celebración no acaba (fase {game.Bosses.Current})");
            if (!game.Bosses.Defeated.Contains(world)) Problem("el jefe no queda vencido");
            Debug.Log($"[Recorrido] mundo {world}: jefe vencido");
        }

        IEnumerator Ending()
        {
            step = "final: sala del bebé";
            game.DebugGoToBaby();
            yield return Wait(1f);
            float t = 0;
            Enemy baby = null;
            while (baby == null && t < 20)
            {
                foreach (var e in game.Enemies.Enemies.ToList())
                    if (e.Alive && !(e is Baby)) game.Enemies.Damage(e, 99);
                baby = game.Enemies.Enemies.FirstOrDefault(e => e is Baby && e.Alive);
                t += 0.3f;
                yield return Wait(0.3f);
            }
            if (baby == null) { Problem("no aparece el bebé tras romper las piedras"); yield break; }
            step = "final: tocar al bebé";
            t = 0;
            while (!game.EndingRunning && t < 4)
            {
                game.Motor.Place(game.World, game.Motor.Room, (int)baby.X + 8, (int)baby.Y + 16);
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            if (!game.EndingRunning) { Problem("tocar al bebé no empieza el final"); yield break; }
            step = "final";
            yield return Wait(8f);
            yield return Shot("final");
            t = 0;
            while (game.EndingRunning && t < 150) { t += 1; yield return Wait(1f); }
            if (game.EndingRunning) Problem("el final no termina en 150 s");
        }

        void Finish()
        {
            var lines = new List<string> { $"Recorrido de pruebas: {problems.Count} problemas, {errors} errores o excepciones", "" };
            lines.AddRange(problems);
            File.WriteAllLines(Path.Combine(dir, "recorrido.txt"), lines);
            Debug.Log($"[Recorrido] fin: {problems.Count} problemas, {errors} errores");
            Application.Quit();
        }
    }
}
