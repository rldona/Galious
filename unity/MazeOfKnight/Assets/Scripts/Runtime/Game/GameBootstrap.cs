using System.Linq;
using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game
{
    // Arranque de la escena de juego: carga el mapa importado (Resources/Maps, generado por el importador),
    // lo dibuja y coloca al jugador. Por defecto, donde empieza el original: castillo, sala 1, x = 192,
    // y = 120, con Popolon.
    public class GameBootstrap : MonoBehaviour
    {
        public string mapName = "castle";
        public int startRoom = 1;
        public int startX = 192;
        public int startY = 120;
        public Character startCharacter = Character.Popolon;

        public MapView mapView;
        public PlayerController player;

        public RoomWorld World { get; private set; }

        void Start()
        {
            var map = Resources.Load<MapData>($"Maps/{mapName}");
            if (map == null)
            {
                Debug.LogError($"No encuentro Resources/Maps/{mapName}. Importa las salas: Maze of Knight > Importar salas.");
                enabled = false;
                return;
            }
            mapView.Build(map);
            World = new RoomWorld(map.rooms.Select(r => r.room));
            var room = World.Room(startRoom);
            var motor = new PlayerMotor(World, room, startX, startY) { Character = startCharacter };
            player.Init(motor);
        }
    }
}
