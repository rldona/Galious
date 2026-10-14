using UnityEngine;

namespace MazeOfKnight.Game
{
    // Ayuda en pantalla para las pruebas: controles, mapa, sala, posición en píxeles del original y estado.
    public class DebugHud : MonoBehaviour
    {
        public GameBootstrap bootstrap;
        public PlayerController player;
        GUIStyle style;

        void OnGUI()
        {
            if (player == null || player.Motor == null)
                return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(12, Screen.height / 45) };
            var m = player.Motor;
            string text =
                "← → andar   ↑ saltar / subir   ↓ bajar escalera   Tab cambiar de personaje   Esc salir\n" +
                $"{bootstrap.mapName}  sala {m.Room.id}   x {m.X}  y {m.Y}   {m.State}   {m.Character}";
            GUI.Label(new Rect(8, 4, Screen.width - 16, style.fontSize * 3f), text, style);
        }

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
                Application.Quit();
        }
    }
}
