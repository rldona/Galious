using System.IO;
using UnityEditor;
using UnityEngine;

namespace MazeOfKnight.EditorTools.Walkthrough
{
    // Menú y modo batch de la prueba de partida completa. Escribe el informe en logs/unity/walkthrough.md:
    //   Unity -batchmode -projectPath unity/MazeOfKnight -executeMethod MazeOfKnight.EditorTools.Walkthrough.WalkthroughRunner.RunBatch -quit
    public static class WalkthroughRunner
    {
        static string ReportPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../../../logs/unity/walkthrough.md"));

        [MenuItem("Maze of Knight/Prueba de partida completa", false, 60)]
        public static void RunMenu()
        {
            var ok = RunAndWrite();
            EditorUtility.DisplayDialog("Prueba de partida completa", ok ? "Se puede acabar el juego." : "No se llega al final.\n" + ReportPath, "OK");
        }

        public static void RunBatch()
        {
            bool ok = RunAndWrite();
            EditorApplication.Exit(ok ? 0 : 1);
        }

        static bool RunAndWrite()
        {
            var w = new Walkthrough();
            if (!w.HasData)
            {
                Debug.LogError("[Walkthrough] Sin salas importadas (Maze of Knight > Importar salas)");
                return false;
            }
            w.Run();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, w.Report());
            Debug.Log($"[Walkthrough] {(w.Ending ? "final alcanzado" : "SIN FINAL")} → {ReportPath}");
            return w.Ending;
        }
    }
}
