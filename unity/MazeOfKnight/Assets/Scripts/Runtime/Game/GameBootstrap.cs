using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game
{
    // Partida: carga los mapas importados (Resources/Maps), dibuja el actual, coloca al jugador y gestiona el
    // paso entre el castillo y los mundos, las piedras y los objetos que quedan a la vista.
    // Empieza donde el original: castillo, sala 1, x = 192, y = 120, con Popolon.
    public class GameBootstrap : MonoBehaviour
    {
        public string mapName = "castle";
        public int startRoom = 1;
        public int startX = 192;
        public int startY = 120;
        public Character startCharacter = Character.Popolon;
        [Tooltip("Sin inventario todavía: todas las puertas de mundo abiertas y sin requisitos de personaje.")]
        public bool allDoorsOpen = true;

        public MapView mapView;
        public PlayerController player;
        public Sprite itemSprite;

        public RoomWorld World { get; private set; }
        public string Message { get; private set; } = "";
        public readonly List<int> Collected = new List<int>();

        readonly Dictionary<string, MapData> maps = new Dictionary<string, MapData>();
        readonly HashSet<(string, int, int, int)> brokenRocks = new HashSet<(string, int, int, int)>();
        readonly HashSet<(string, int)> takenItems = new HashSet<(string, int)>();
        readonly List<(RoomModel room, GameObject go)> visibleItems = new List<(RoomModel, GameObject)>();
        Rocks rocks;
        PlayerMotor motor;
        float messageUntil;

        void Start()
        {
            foreach (var m in Resources.LoadAll<MapData>("Maps"))
                maps[m.mapName] = m;
            if (!maps.ContainsKey(mapName))
            {
                Debug.LogError($"No encuentro Resources/Maps/{mapName}. Importa las salas: Maze of Knight > Importar salas.");
                enabled = false;
                return;
            }
            LoadMap(mapName);
            motor = new PlayerMotor(World, World.Room(startRoom), startX, startY) { Character = startCharacter };
            motor.DoorEntered += EnterWorld;
            motor.CastleExitReached += ExitToCastle;
            player.Init(motor);
            player.Ticked += AfterTick;
        }

        void LoadMap(string name)
        {
            mapName = name;
            var map = maps[name];
            World = new RoomWorld(map.rooms.Select(r => r.room));
            rocks = new Rocks(name, World, brokenRocks);
            rocks.Broken += OnRockBroken;
            rocks.Hit += r => Say($"Piedra: {Mathf.Max(r.hp, 0)}/{OriginalPhysics.RockHp}");
            mapView.Build(map, World);
            foreach (var r in rocks.All)
                mapView.ShowRock(r.room, r.x, r.y, !r.broken);
            foreach (var (_, go) in visibleItems)
                Destroy(go);
            visibleItems.Clear();
            // Objetos sin piedra encima (o con la piedra ya rota) a la vista desde el principio.
            foreach (var room in World.Rooms)
                if (room.hasItem && (room.item.x != 0 || room.item.y != 0) &&
                    !rocks.All.Any(r => !r.broken && r.room == room && r.x == room.item.x && r.y == room.item.y))
                    ShowItem(room);
        }

        void EnterWorld(int world)
        {
            if (!allDoorsOpen || !OriginalWorlds.HasEntry(world) || !maps.ContainsKey(OriginalWorlds.MapName(world)))
            {
                Say($"La puerta del mundo {world} no se abre (aún no implementado)");
                return;
            }
            LoadMap(OriginalWorlds.MapName(world));
            var (x, y) = OriginalWorlds.Entry[world];
            motor.Place(World, World.Room(1), x, y);
            Say($"WORLD {world}");
        }

        void ExitToCastle(int code)
        {
            int world = maps[mapName].e041 - 1;
            LoadMap("castle");
            var doorRoom = World.Rooms.FirstOrDefault(r => r.doorWorld == world);
            if (doorRoom == null)
            {
                Debug.LogWarning($"Sin puerta del mundo {world} en el castillo (salida 0x{code:X2})");
                doorRoom = World.Room(startRoom);
            }
            motor.Place(World, doorRoom, doorRoom.door.x + OriginalWorlds.DoorExitDx, doorRoom.door.y + OriginalWorlds.DoorExitDy);
            Say("CASTLE");
        }

        void AfterTick()
        {
            if (motor.AttackStartedThisTick)
                rocks.Strike(motor, World);
            // Recoger un objeto a la vista al tocarlo.
            for (int i = visibleItems.Count - 1; i >= 0; i--)
            {
                var (room, go) = visibleItems[i];
                if (room != motor.Room)
                    continue;
                int ix = room.item.x, iy = room.item.y;
                if (motor.X + OriginalPhysics.FootRight < ix || motor.X + OriginalPhysics.FootLeft > ix + 15 ||
                    motor.Y - OriginalPhysics.HeadHeight > iy + 15 || motor.Y <= iy)
                    continue;
                takenItems.Add((mapName, room.id));
                Collected.Add(room.item.type);
                Destroy(go);
                visibleItems.RemoveAt(i);
                Say($"Objeto 0x{room.item.type:X2}");
            }
        }

        void OnRockBroken(Rocks.Rock rock)
        {
            mapView.ShowRock(rock.room, rock.x, rock.y, false);
            mapView.ClearSolid(rock.room, rock.x, rock.y, 2, 2);
            var room = rock.room;
            if (room.hasItem && room.item.x == rock.x && room.item.y == rock.y)
                ShowItem(room);
            Say("Piedra rota");
        }

        void ShowItem(RoomModel room)
        {
            if (takenItems.Contains((mapName, room.id)) || itemSprite == null)
                return;
            var go = new GameObject($"Objeto 0x{room.item.type:X2} (sala {room.id})");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = itemSprite;
            sr.sortingOrder = 5;
            go.transform.position = RoomCoords.PixelToWorld(room, room.item.x + 8, room.item.y + 16);
            visibleItems.Add((room, go));
        }

        void Say(string text)
        {
            Message = text;
            messageUntil = Time.time + 2.5f;
        }

        void Update()
        {
            if (Message != "" && Time.time > messageUntil)
                Message = "";
        }
    }
}
