using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game.Enemies;
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
        public Sprite enemySprite;

        public RoomWorld World { get; private set; }
        public string Message { get; private set; } = "";
        public readonly List<int> Collected = new List<int>();

        // Recursos del marcador (E046 flechas, E048 monedas, E04A llaves), hasta 999.
        public int Arrows, Coins, Keys;
        // Vitalidad por personaje (Aphrodite = 0, Popolon = 1), en unidades del original.
        public readonly int[] Vitality = { OriginalPhysics.StartVitality, OriginalPhysics.StartVitality };
        public readonly int[] MaxVitality = { OriginalPhysics.StartVitality, OriginalPhysics.StartVitality };
        int liquidTicks;

        readonly Dictionary<string, MapData> maps = new Dictionary<string, MapData>();
        readonly HashSet<(string, int, int, int)> brokenRocks = new HashSet<(string, int, int, int)>();
        readonly HashSet<(string, int)> takenItems = new HashSet<(string, int)>();
        readonly HashSet<(string, int, int, int)> openedDoors = new HashSet<(string, int, int, int)>();
        KeyDoors keyDoors;
        RoomObjects roomObjects;
        readonly HashSet<(string, int, int, int)> roomObjectState = new HashSet<(string, int, int, int)>();
        readonly List<(RoomModel room, GameObject go)> visibleItems = new List<(RoomModel, GameObject)>();
        Rocks rocks;
        PlayerMotor motor;
        public EnemySystem Enemies { get; private set; }
        readonly Dictionary<Enemy, SpriteRenderer> enemyViews = new Dictionary<Enemy, SpriteRenderer>();
        public int HurtTicks { get; private set; }   // invulnerable tras un golpe (parpadea)
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
            var catalog = Resources.Load<EnemyCatalog>("Enemies");
            Enemies = new EnemySystem(catalog != null ? catalog.enemies : new List<EnemyInfo>(), System.Environment.TickCount, catalog != null ? catalog.loot : null);
            Enemies.Spawned += ShowEnemy;
            Enemies.HasItem = t => Collected.Contains(t);
            Enemies.Killed += e => { if (enemyViews.TryGetValue(e, out var v)) v.enabled = false; Say("Enemigo derrotado"); };
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
            if (Enemies != null) Enemies.Context.E041 = map.e041;
            World = new RoomWorld(map.rooms.Select(r => r.room));
            rocks = new Rocks(name, World, brokenRocks);
            rocks.Broken += OnRockBroken;
            rocks.Hit += r => Say($"Piedra: {Mathf.Max(r.hp, 0)}/{OriginalPhysics.RockHp}");
            keyDoors = new KeyDoors(name, World, openedDoors);
            keyDoors.Changed += d => mapView.ShowKeyDoor(d.room, d.x, d.y, d.height);
            keyDoors.Opened += d => Say(d.cls == 1 ? "Puerta abierta (se cerrará detrás)" : "Puerta abierta");
            roomObjects = new RoomObjects(name, World, roomObjectState);
            roomObjects.CellsChanged += (room, col, row, w, h) => mapView.RedrawCells(World, room, col, row, w, h);
            roomObjects.Message += Say;
            mapView.Build(map, World);
            foreach (var d in roomObjects.RevolvingDoors)
                mapView.ShowRevolvingDoor(d.room, d.x, d.y);
            foreach (var r in rocks.All)
                mapView.ShowRock(r.room, r.x, r.y, !r.broken);
            foreach (var d in keyDoors.All)
                mapView.ShowKeyDoor(d.room, d.x, d.y, d.height);
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
            if (Enemies.Room != motor.Room)
                EnterEnemyRoom();
            Enemies.Tick();
            var drop = Enemies.PickUp(motor);
            if (drop != null)
                Say(TakeLoot(drop));
            if (motor.AttackStartedThisTick)
                Enemies.Strike(motor, Collected);
            if (HurtTicks > 0)
                HurtTicks--;
            else if (motor.State != MotorState.Knockback && motor.State != MotorState.Carried)
            {
                var enemy = Enemies.Touching(motor, Collected);
                if (enemy != null)
                    HurtBy(enemy);
            }
            if (motor.AttackStartedThisTick)
            {
                rocks.Strike(motor, World);
                roomObjects.Strike(motor, Collected);
            }
            roomObjects.Tick(motor, Collected);
            int keysBefore = Keys;
            Keys = keyDoors.Tick(motor, Keys);
            if (Keys != keysBefore)
                Say($"Llave usada ({Keys})");
            LiquidDamage();
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
                Say(PickUp(room.item.type));
            }
        }

        // Golpe de un enemigo (medido, docs/player.md): daño de la tabla b04:64E5, retroceso alejándose de él e
        // invulnerabilidad de 42 ticks.
        void HurtBy(Enemy enemy)
        {
            int c = (int)motor.Character;
            if (enemy is Fairy)
            {
                // El hada rellena la vitalidad (b04:612C).
                Vitality[c] = MaxVitality[c];
                enemy.Alive = false;
                Say("Vitalidad llena");
                return;
            }
            if (enemy is Shot shot && shot.Stuns)
            {
                motor.Stunned = 60;   // E500 = 6, E50F = 60
                enemy.Alive = false;
                Say("Paralizado");
                return;
            }
            Vitality[c] -= enemy.Damage;
            if (enemy.DiesOnTouch)
                enemy.Alive = false;
            int dir = enemy.X + enemy.Width / 2f >= motor.X ? -1 : 1;
            motor.Knockback(dir);
            HurtTicks = OriginalPhysics.HurtInvulnerableTicks;
            Say($"-{enemy.Damage} (0x{enemy.Type:X2})");
            if (Vitality[c] <= 0)
                LoseLife();
        }

        void EnterEnemyRoom()
        {
            foreach (var v in enemyViews.Values)
                if (v != null) Destroy(v.gameObject);
            enemyViews.Clear();
            Enemies.EnterRoom(World, motor.Room, motor);
        }

        // Colores provisionales: los de las primeras fichas y, para el resto, un tono fijo por tipo. Los proyectiles
        // (hijos que mueren al tocar) salen más claros.
        static Color EnemyColor(Enemy e)
        {
            switch (e.Type)
            {
                case 0x0E: return new Color(0.75f, 0.45f, 0.95f);   // murciélago
                case 0x25: return new Color(0.92f, 0.92f, 0.85f);   // esqueleto
                case 0x31: return new Color(0.80f, 0.80f, 0.95f);   // pinchos
                case 0x18: return new Color(0.95f, 0.55f, 0.25f);   // rodador
                case 0x34: return new Color(0.60f, 0.30f, 0.30f);   // bloque
                case 0x2E: return new Color(0.40f, 0.90f, 0.40f);   // brote
                case 0x5B: return new Color(1f, 0.85f, 0.3f, 0.6f);  // explosión
                case 0x5D: return new Color(0.85f, 0.85f, 0.85f);   // flechas
                case 0x5E: return new Color(1f, 0.8f, 0.1f);        // monedas
                case 0x5F: return new Color(0.3f, 0.9f, 1f);        // llaves
            }
            var c = Color.HSVToRGB((e.Type * 0.137f) % 1f, e.DiesOnTouch ? 0.35f : 0.75f, 0.95f);
            return c;
        }

        void ShowEnemy(Enemy e)
        {
            var go = new GameObject($"Enemigo 0x{e.Type:X2}");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = enemySprite;
            sr.color = EnemyColor(e);
            sr.sortingOrder = 8;
            enemyViews[e] = sr;
        }

        void LateUpdate()
        {
            if (motor == null)
                return;
            foreach (var e in enemyViews.Keys.Where(e => !e.Alive).ToList())
            {
                if (enemyViews[e] != null) Destroy(enemyViews[e].gameObject);
                enemyViews.Remove(e);
            }
            foreach (var (e, sr) in enemyViews.Select(kv => (kv.Key, kv.Value)))
            {
                if (sr == null) continue;
                sr.enabled = e.Alive && e.Visible;
                var (x0, y0, x1, y1) = e.Box();
                var bottomLeft = RoomCoords.PixelToWorld(e.Room, (int)x0, (int)y1);
                sr.transform.position = new Vector3(bottomLeft.x, bottomLeft.y, 0);
                sr.transform.localScale = new Vector3((x1 - x0) / 16f, Mathf.Max(0.05f, (y1 - y0) / 16f), 1);
                sr.flipX = e.FacingLeft;
            }
            // Parpadeo del jugador mientras es invulnerable.
            if (player.body != null)
                player.body.enabled = HurtTicks <= 0 || (HurtTicks / 2) % 2 == 0;
        }

        // Botín de un enemigo (b03:B2CB): suma al contador, con tope 999.
        string TakeLoot(LootDrop d)
        {
            var loot = Enemies.Loot;
            if (d.Type == loot.arrowObject) { Arrows = Mathf.Min(loot.counterCap, Arrows + d.Amount); return $"+{d.Amount} flechas ({Arrows})"; }
            if (d.Type == loot.coinObject) { Coins = Mathf.Min(loot.counterCap, Coins + d.Amount); return $"+{d.Amount} monedas ({Coins})"; }
            Keys = Mathf.Min(loot.counterCap, Keys + d.Amount);
            return $"+{d.Amount} llaves ({Keys})";
        }

        // Recursos (docs/items_enemies.md): 0x2D +50 flechas, 0x2E +10 monedas, 0x2F +2 llaves.
        string PickUp(int type)
        {
            switch (type)
            {
                case 0x2D: Arrows = Mathf.Min(999, Arrows + 50); return $"+50 flechas ({Arrows})";
                case 0x2E: Coins = Mathf.Min(999, Coins + 10); return $"+10 monedas ({Coins})";
                case 0x2F: Keys = Mathf.Min(999, Keys + 2); return $"+2 llaves ({Keys})";
                default: return $"Objeto 0x{type:X2}";
            }
        }

        // Agua y lava (docs/player.md): 1 de vitalidad cada 4 ticks; Aphrodite en el agua, cada 255.
        void LiquidDamage()
        {
            if (!motor.InLiquid)
            {
                liquidTicks = 0;
                return;
            }
            bool aphroditeInWater = motor.Character == Character.Aphrodite && motor.Liquid == TileClass.Water;
            int every = aphroditeInWater ? OriginalPhysics.AphroditeWaterDamageTicks : OriginalPhysics.LiquidDamageTicks;
            if (++liquidTicks < every)
                return;
            liquidTicks = 0;
            int c = (int)motor.Character;
            Vitality[c]--;
            if (Vitality[c] <= 0)
                LoseLife();
        }

        // Sin vidas ni fin de partida todavía: vuelve a la entrada del mapa con la vitalidad llena.
        void LoseLife()
        {
            int c = (int)motor.Character;
            Vitality[c] = MaxVitality[c];
            liquidTicks = 0;
            int world = maps[mapName].e041 - 1;
            if (world >= 1 && OriginalWorlds.HasEntry(world))
            {
                var (x, y) = OriginalWorlds.Entry[world];
                motor.Place(World, World.Room(1), x, y);
            }
            else
                motor.Place(World, World.Room(startRoom), startX, startY);
            Say("Vida perdida");
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
            // Depuración: K da una llave (en el original salen de piedras, enemigos y tiendas).
            if (UnityEngine.InputSystem.Keyboard.current?.kKey.wasPressedThisFrame == true)
            {
                Keys = Mathf.Min(999, Keys + 1);
                Say($"[depuración] +1 llave ({Keys})");
            }
        }
    }
}
