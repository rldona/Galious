using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace MazeOfKnight.EditorTools
{
    // Al abrir el proyecto con la escena vacía por defecto, enseña la vista general del castillo
    // (si ya se han importado las salas). Una vez por sesión del editor.
    [InitializeOnLoad]
    static class OpenOverviewOnStartup
    {
        const string Key = "MazeOfKnight.OpenOverviewOnStartup";

        static OpenOverviewOnStartup()
        {
            if (UnityEngine.Application.isBatchMode || SessionState.GetBool(Key, false))
                return;
            SessionState.SetBool(Key, true);
            EditorApplication.delayCall += () =>
            {
                var scene = SceneManager.GetActiveScene();
                bool untitled = string.IsNullOrEmpty(scene.path) && !scene.isDirty;
                if (untitled && File.Exists("Assets/Generated/Scenes/castle_overview.unity"))
                    GaliousImporter.OpenMap("castle");
            };
        }
    }
}
