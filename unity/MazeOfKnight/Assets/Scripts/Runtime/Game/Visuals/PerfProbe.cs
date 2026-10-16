using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Medida de rendimiento (-rendimiento <carpeta>): sin sincronía vertical ni límite de fotogramas, para ver el margen,
    // pasa por tres salas de cada mapa (con su ambiente), por el inventario y por un santuario, y mide en cada sitio
    // durante unos segundos los fotogramas por segundo (media y el 1 % más lento), el tiempo de procesador y de gráfica
    // (FrameTimingManager) y las recolecciones de basura. Va a <carpeta>/rendimiento.txt. Solo existe para pruebas.
    public class PerfProbe : MonoBehaviour
    {
        static string dir;
        const float Seconds = 4f;
        readonly List<string> lines = new List<string>();
        GameBootstrap game;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            int i = System.Array.IndexOf(args, "-rendimiento");
            if (i < 0 || i + 1 >= args.Length) return;
            dir = args[i + 1];
            AttractMode.Current = AttractMode.Mode.Game;
            Application.runInBackground = true;
            var go = new GameObject("Medida de rendimiento");
            DontDestroyOnLoad(go);
            go.AddComponent<PerfProbe>();
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(dir);
            yield return new WaitForSecondsRealtime(4f);
            game = FindAnyObjectByType<GameBootstrap>();
            if (game == null) { Application.Quit(); yield break; }
            game.DebugInvulnerable = true;
            foreach (var it in game.Inventory.Data.items)
                if (it.effect != null && it.effect.kind == "worldEntry") game.Inventory.Add(it.type);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            lines.Add($"Pantalla {Screen.width}×{Screen.height}, {SystemInfo.graphicsDeviceName}, {SystemInfo.processorType}");
            lines.Add("sitio | FPS medio | 1 % más lento (FPS) | ms medio | ms peor | CPU ms | GPU ms | GC");
            for (int w = 0; w <= 10; w++)
            {
                string map = Benches.MapName(w);
                if (w > 0) { game.DebugEnterWorld(w); yield return new WaitForSecondsRealtime(1.5f); }
                var ids = game.World.Rooms.Where(r => !r.shrine).Select(r => r.id).OrderBy(i => i).ToList();
                foreach (int id in new[] { ids[0], ids[ids.Count / 2], ids[ids.Count - 1] }.Distinct())
                {
                    game.DebugGoTo(map, id);
                    yield return new WaitForSecondsRealtime(0.8f);
                    yield return Measure($"{map} sala {id}");
                }
            }
            var menu = game.GetComponent<InventoryMenu>();
            if (menu != null) { menu.Toggle(); yield return new WaitForSecondsRealtime(0.5f); yield return Measure("inventario"); menu.Toggle(); }
            game.DebugGoTo("castle", 1);
            yield return new WaitForSecondsRealtime(0.5f);
            if (game.DebugOpenShrine("Ares")) { yield return new WaitForSecondsRealtime(0.5f); yield return Measure("santuario"); game.DebugShrineScreen.Close(); }
            // Basura: bytes pedidos por fotograma en una sala con enemigos (lo que crece la memoria entre recolecciones), con
            // todo y quitando cada componente que se actualiza o dibuja interfaz en cada fotograma, para ver de dónde sale.
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-basura") >= 0)
            {
                Application.targetFrameRate = 120;
                game.DebugGoTo("castle", 74);
                yield return new WaitForSecondsRealtime(1f);
                var rate = new float[1];
                yield return AllocRate(rate);
                lines.Add($"basura con todo: {rate[0]:0} bytes por fotograma");
                var all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(m => m.enabled && m != this).ToList();
                var gui = all.Where(m => Declares(m.GetType(), "OnGUI")).ToList();
                var frame = all.Where(m => Declares(m.GetType(), "Update") || Declares(m.GetType(), "LateUpdate")).ToList();
                foreach (var (label, group) in new[] { ("sin las que dibujan interfaz (OnGUI)", gui), ("sin las que se actualizan cada fotograma", frame), ("sin ninguna", all) })
                {
                    foreach (var m in group) m.enabled = false;
                    yield return new WaitForSecondsRealtime(0.3f);
                    yield return AllocRate(rate);
                    foreach (var m in group) m.enabled = true;
                    lines.Add($"   {label}: {rate[0]:0}  ({string.Join(", ", group.Select(m => m.GetType().Name).Distinct())})");
                    yield return new WaitForSecondsRealtime(0.3f);
                }
                // Sin interfaz, quitando cada uno de los que se actualizan.
                foreach (var o in gui) o.enabled = false;
                foreach (var m in frame.Where(f => !gui.Contains(f)).GroupBy(f => f.GetType()).Select(g => g.First()))
                {
                    var same = frame.Where(f => f.GetType() == m.GetType()).ToList();
                    foreach (var o in same) o.enabled = false;
                    yield return new WaitForSecondsRealtime(0.2f);
                    yield return AllocRate(rate);
                    foreach (var o in same) o.enabled = true;
                    lines.Add($"   sin interfaz y sin {m.GetType().Name}: {rate[0]:0}");
                }
                foreach (var m in gui)
                {
                    foreach (var o in gui) o.enabled = o == m;
                    yield return new WaitForSecondsRealtime(0.2f);
                    yield return AllocRate(rate);
                    lines.Add($"   solo {m.GetType().Name} dibujando: {rate[0]:0}");
                }
                foreach (var m in gui) m.enabled = true;
            }

            // Fluidez: el Caballero anda a la derecha 3 s en el suelo ancho de la sala 1 del castillo, a 60 y a 120 FPS, y se
            // mide cuánto avanza su dibujo en cada fotograma (a saltos: muchos fotogramas quietos y otros con el salto).
            foreach (int fps in new[] { 60, 120 })
            {
                Application.targetFrameRate = fps;
                game.DebugGoTo("castle", 1);
                game.Motor.Place(game.World, game.Motor.Room, 40, game.Motor.Y);
                yield return new WaitForSecondsRealtime(0.5f);
                game.player.Demo = new DemoPlayer(new DemoInfo { inputs = new[] { new DemoInputEntry { frames = 120, keys = 1, right = true }, new DemoInputEntry { keys = 0xFF } } });
                yield return new WaitForSecondsRealtime(0.3f);
                var steps = new List<float>();
                float last = game.player.transform.position.x, t = 0;
                while (t < 2.5f)
                {
                    yield return null;
                    t += Time.unscaledDeltaTime;
                    float x = game.player.transform.position.x;
                    steps.Add((x - last) * RoomCoords.PixelsPerUnit / Mathf.Max(1e-4f, Time.unscaledDeltaTime));   // px/s
                    last = x;
                }
                game.player.Demo = null;
                float mean = steps.Average();
                int still = steps.Count(v => Mathf.Abs(v) < 0.05f * Mathf.Abs(mean));
                float dev = Mathf.Sqrt(steps.Average(v => (v - mean) * (v - mean)));
                lines.Add($"fluidez a {fps} FPS: {steps.Count} fotogramas, velocidad media {mean:0} px/s, desviación {dev:0} px/s ({dev / Mathf.Max(1, Mathf.Abs(mean)) * 100:0} %), quietos {still * 100 / Mathf.Max(1, steps.Count)} %");
                Debug.Log("[Rendimiento] " + lines[lines.Count - 1]);
            }
            File.WriteAllLines(Path.Combine(dir, "rendimiento.txt"), lines);
            Application.Quit();
        }

        static bool Declares(System.Type t, string method)
        {
            const System.Reflection.BindingFlags f = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
                                                     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly;
            for (; t != null && t != typeof(MonoBehaviour); t = t.BaseType)
                if (t.GetMethod(method, f) != null) return true;
            return false;
        }

        // Bytes por fotograma durante 2 s: lo que sube la memoria gestionada de un fotograma al siguiente (las bajadas son
        // recolecciones y no cuentan).
        static IEnumerator AllocRate(float[] result)
        {
            long last = System.GC.GetTotalMemory(false), sum = 0;
            int frames = 0;
            float t = 0;
            while (t < 2f)
            {
                yield return null;
                t += Time.unscaledDeltaTime;
                long now = System.GC.GetTotalMemory(false);
                if (now > last) sum += now - last;
                last = now;
                frames++;
            }
            result[0] = frames > 0 ? sum / (float)frames : 0;
        }

        IEnumerator Measure(string where)
        {
            var dts = new List<float>();
            double cpu = 0, gpu = 0;
            int timed = 0, gc0 = System.GC.CollectionCount(0);
            var timings = new FrameTiming[1];
            float t = 0;
            while (t < Seconds)
            {
                yield return null;
                float dt = Time.unscaledDeltaTime;
                t += dt;
                dts.Add(dt);
                FrameTimingManager.CaptureFrameTimings();
                if (FrameTimingManager.GetLatestTimings(1, timings) > 0 && timings[0].cpuFrameTime > 0)
                {
                    cpu += timings[0].cpuFrameTime;
                    gpu += timings[0].gpuFrameTime;
                    timed++;
                }
            }
            dts.Sort();
            float avg = dts.Average(), worst = dts[dts.Count - 1];
            float p99 = dts[Mathf.Min(dts.Count - 1, (int)(dts.Count * 0.99f))];
            string line = $"{where} | {1 / avg:0} | {1 / p99:0} | {avg * 1000:0.0} | {worst * 1000:0.0} | " +
                          $"{(timed > 0 ? (cpu / timed).ToString("0.0") : "-")} | {(timed > 0 ? (gpu / timed).ToString("0.0") : "-")} | {System.GC.CollectionCount(0) - gc0}";
            lines.Add(line);
            Debug.Log("[Rendimiento] " + line);
        }
    }
}
