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
            if (player == null || player.Motor == null || bootstrap.EndingRunning)
                return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = Mathf.Max(12, Screen.height / 45) };
            var m = player.Motor;
            string text =
                "← → andar   ↑ saltar / subir   ↓ bajar / entrar   Espacio espada   M arma   F1 (o I) objetos   K +1 llave   G armas   F2 sala del jefe   F4 sala del bebé   Esc salir\n" +
                $"VIT Popolon {Bar(bootstrap.Vitality[1], bootstrap.MaxVitality[1])}   Aphrodite {Bar(bootstrap.Vitality[0], bootstrap.MaxVitality[0])}" +
                (bootstrap.Inventory != null ? $"   EXP {bootstrap.Inventory.Experience[(int)m.Character]}/48" : "") +
                $"   flechas {bootstrap.Arrows:000}   monedas {bootstrap.Coins:000}   llaves {bootstrap.Keys:000}   arma: {bootstrap.WeaponName(bootstrap.SelectedWeapon)}\n" +
                $"{bootstrap.mapName}  sala {m.Room.id}   x {m.X}  y {m.Y}   {m.State}   {m.Character}" +
                (m.InLiquid ? $"   en {(m.Liquid == MazeOfKnight.Data.TileClass.Lava ? "lava" : "agua")}" : "") +
                (bootstrap.Collected.Count > 0 ? $"   objetos: {bootstrap.Collected.Count}" : "") +
                (bootstrap.Enemies != null && bootstrap.Enemies.Missing > 0 ? $"   enemigos sin implementar aquí: {bootstrap.Enemies.Missing}" : "") +
                (m.Room.doorWorld > 0 ? $"   puerta al mundo {m.Room.doorWorld}" : "") +
                (bootstrap.Bosses?.Boss != null && bootstrap.Bosses.Boss.Alive ? $"   {bootstrap.Bosses.Info.name}: {bootstrap.Bosses.Boss.Hp}/{bootstrap.Bosses.Boss.MaxHp}" : "") +
                (bootstrap.Bosses != null && bootstrap.Bosses.Listening ? "   (sala del jefe: escribe la palabra)" : "") +
                (bootstrap.Message != "" ? $"\n{bootstrap.Message}" : "");
            GUI.Label(new Rect(8, 4, Screen.width - 16, style.fontSize * 6f), text, style);
        }

        static string Bar(int value, int max) =>
            new string('■', Mathf.Max(0, value)) + new string('□', Mathf.Max(0, max - value)) + $" {value}/{max}";

        void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
                Application.Quit();
        }
    }
}
