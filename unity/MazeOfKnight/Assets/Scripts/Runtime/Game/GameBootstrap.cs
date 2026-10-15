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
        // Daño, cura y relleno van a un pendiente que se aplica 1 unidad cada 4 ticks (b01:6171).
        public readonly int[] PendingVitality = { 0, 0 };
        int vitalityTicks;
        // Vidas de reserva (E058 Aphrodite, E059 Popolon): al empezar 1 y 0. El personaje en juego no cuenta la suya.
        public readonly int[] Lives = { 1, 0 };
        // Crédito de resurrección de la Muerte (E05A/E05B): 1 para cada uno.
        public readonly int[] ResurrectionCredit = { 1, 1 };
        public bool GameOver { get; private set; }
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
        public LiftSystem Lifts { get; private set; }
        readonly List<SpriteRenderer> liftViews = new List<SpriteRenderer>();
        Sprite liftSprite;

        // Ascensor: dos bloques de 16 × 16 con marco, como los del original (dibujados aquí, sin sus gráficos).
        Sprite LiftSprite()
        {
            if (liftSprite != null) return liftSprite;
            var tex = new Texture2D(32, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            Color edge = new Color(0.95f, 0.95f, 0.95f), face = new Color(0.75f, 0.2f, 0.25f), dark = new Color(0.35f, 0.08f, 0.1f);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 32; x++)
                {
                    int bx = x & 15;
                    bool border = bx == 0 || bx == 15 || y == 0 || y == 15;
                    bool inner = bx == 2 || bx == 13 || y == 2 || y == 13;
                    tex.SetPixel(x, y, border ? edge : inner ? dark : face);
                }
            tex.Apply();
            return liftSprite = Sprite.Create(tex, new Rect(0, 0, 32, 16), Vector2.zero, RoomCoords.PixelsPerUnit);
        }
        public EnemySystem Enemies { get; private set; }
        public WeaponSystem Weapons { get; private set; }
        public int SelectedWeapon;   // E510: 1 flecha … 6 lupa (0 = ninguna)
        public Inventory Inventory { get; private set; }
        public PlayerMotor Motor => motor;
        EnemyCatalog enemyCatalog;
        InventoryMenu menu;
        bool harpUsed;               // el arpa rompe las piedras con el primer espadazo de cada visita a la sala
        string typed = "";           // letras tecleadas (la daga: UMBRELLA)
        ShrinesData shrines = new ShrinesData();
        public BossFight Bosses { get; private set; }
        // En la sala del jefe las letras sirven para escribir la palabra: I, G y K no hacen nada más.
        public bool TypingWord => Bosses != null && Bosses.Listening;
        GameObject keyView;
        EndingScreen ending;
        public bool EndingRunning => ending != null && ending.Running;
        EndingData endingData;
        ShrineScreen shrineScreen;
        bool downWasHeld;
        // Eventos de sala ya disparados (mapa, sala): escaleras que aparecen y muros que se abren se quedan así.
        readonly HashSet<(string, int)> firedEvents = new HashSet<(string, int)>();
        readonly Dictionary<Projectile, SpriteRenderer> projectileViews = new Dictionary<Projectile, SpriteRenderer>();
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
            Enemies = new EnemySystem(catalog != null ? catalog.enemies : new List<EnemyInfo>(), System.Environment.TickCount, catalog != null ? catalog.loot : null, catalog != null ? catalog.boxRects : null);
            Enemies.Spawned += ShowEnemy;
            Enemies.HasItem = t => Collected.Contains(t);
            Enemies.Killed += e => { if (enemyViews.TryGetValue(e, out var v)) v.enabled = false; Say("Enemigo derrotado"); };
            Enemies.Clinked += _ => Say("Clinc");
            var weaponsJson = Resources.Load<TextAsset>("Weapons");
            Weapons = new WeaponSystem(weaponsJson != null ? JsonUtility.FromJson<WeaponsData>(weaponsJson.text) : null);
            Weapons.Spawned += ShowProjectile;
            Weapons.PlayerHit += (damage, dir) => Hurt(Inventory.ReduceDamage(damage), dir, $"-{damage} (mina)");
            var itemsJson = Resources.Load<TextAsset>("Items");
            Inventory = new Inventory(itemsJson != null ? JsonUtility.FromJson<ItemsData>(itemsJson.text) : null, Collected);
            enemyCatalog = catalog;
            Enemies.Killed += GainExperience;
            menu = gameObject.AddComponent<InventoryMenu>();
            menu.game = this;
            var shrinesJson = Resources.Load<TextAsset>("Shrines");
            if (shrinesJson != null) shrines = JsonUtility.FromJson<ShrinesData>(shrinesJson.text);
            shrineScreen = gameObject.AddComponent<ShrineScreen>();
            shrineScreen.game = this;
            Enemies.RoomCleared += OnRoomCleared;
            var bossesJson = Resources.Load<TextAsset>("Bosses");
            Bosses = new BossFight(bossesJson != null ? JsonUtility.FromJson<BossesData>(bossesJson.text) : null);
            Bosses.Message += Say;
            Enemies.BossDefeated += _ => Bosses.OnBossDefeated();
            var endingJson = Resources.Load<TextAsset>("Ending");
            endingData = endingJson != null ? JsonUtility.FromJson<EndingData>(endingJson.text) : new EndingData();
            ending = gameObject.AddComponent<EndingScreen>();
            Sound.Ensure();
            var bossFx = gameObject.AddComponent<BossEffects>();
            bossFx.game = this;
            bossFx.player = player;
            if (UnityEngine.InputSystem.Keyboard.current != null)
                UnityEngine.InputSystem.Keyboard.current.onTextInput += OnTextInput;
            LoadMap(mapName);
            motor = new PlayerMotor(World, World.Room(startRoom), startX, startY) { Character = startCharacter };
            var liftsJson = Resources.Load<TextAsset>("Lifts");
            Lifts = new LiftSystem(liftsJson != null ? JsonUtility.FromJson<LiftsData>(liftsJson.text) : null) { InCastle = mapName == "castle" };
            motor.Lifts = Lifts;
            motor.DoorEntered += EnterWorld;
            motor.RoomChanged += OnRoomChanged;
            motor.CastleExitReached += ExitToCastle;
            player.Init(motor);
            player.Ticked += AfterTick;
            StartMode();
        }

        // ---------- presentación, demo o partida (docs/attract.md) ----------

        AttractScreen attract;
        public bool AttractRunning => attract != null && attract.Running;
        // Presentación, demo o final: ni menú ni teclas de depuración.
        public bool Busy => AttractRunning || player != null && player.Demo != null || EndingRunning;
        public bool skipAttract;     // para probar en el editor: empieza directamente la partida

        void StartMode()
        {
            var json = Resources.Load<TextAsset>("Attract");
            var data = json != null ? JsonUtility.FromJson<AttractData>(json.text) : new AttractData();
            if (skipAttract && AttractMode.Current == AttractMode.Mode.Attract)
                AttractMode.Current = AttractMode.Mode.Game;
            if (AttractMode.Current == AttractMode.Mode.Game && AttractMode.LoadSave)
            {
                AttractMode.LoadSave = false;
                if (!LoadSavedGame()) Say("No hay partida guardada");
            }
            if (AttractMode.Current == AttractMode.Mode.Game)
            {
                // PLAY START ya ha pedido la 0x87 (castillo con su entrada); si no (skipAttract), se pide aquí.
                if (Sound.Instance != null && Sound.Instance.CurrentSong != 0x87) Sound.PlayMusic(0x87);
                musicWanted = 0x88;
            }
            else Sound.StopMusic();
            switch (AttractMode.Current)
            {
                case AttractMode.Mode.Attract:
                    SetPaused(true);
                    attract = gameObject.AddComponent<AttractScreen>();
                    attract.Begin(data);
                    break;
                case AttractMode.Mode.Demo:
                {
                    // Como una partida nueva, con el Arrow elegido y 100 flechas (b01:7DA1), y las teclas de la grabación.
                    var demo = data.demos[AttractMode.Demo % data.demos.Length];
                    if (demo.map != mapName) LoadMap(demo.map);
                    motor.Place(World, World.Room(demo.room), demo.x, demo.y);
                    Lifts.PresetForStart(demo.room);   // la demo 0 empieza sobre el ascensor de la sala 146
                    Sound.PlayMusic(0x88);
                    musicWanted = 0x88;
                    Inventory.Add(1);
                    SelectedWeapon = 1;
                    Arrows = 100;
                    player.Demo = new DemoPlayer(demo);
                    player.DemoInterrupted += () => { player.Demo = null; AttractMode.TripDone(true, true); };
                    player.Ticked += () =>
                    {
                        if (player.Demo != null && player.Demo.Finished) { player.Demo = null; AttractMode.TripDone(true, false); }
                    };
                    break;
                }
            }
        }

        void LoadMap(string name)
        {
            mapName = name;
            var map = maps[name];
            if (Enemies != null) Enemies.Context.E041 = map.e041;
            if (Lifts != null) Lifts.InCastle = name == "castle";
            World = new RoomWorld(map.rooms.Select(r => r.room));
            rocks = new Rocks(name, World, brokenRocks);
            rocks.Broken += OnRockBroken;
            rocks.Hit += r => { Sound.Effect(0x1E); Say($"Piedra: {Mathf.Max(r.hp, 0)}/{OriginalPhysics.RockHp}"); };
            keyDoors = new KeyDoors(name, World, openedDoors);
            keyDoors.Changed += d => mapView.ShowKeyDoor(d.room, d.x, d.y, d.height);
            keyDoors.Opened += d => { Sound.Effect(d.cls == 1 ? 0x22 : 0x21); Say(d.cls == 1 ? "Puerta abierta (se cerrará detrás)" : "Puerta abierta"); };
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
            ApplyWorldItems();
            foreach (var (m, id) in firedEvents)
                if (m == name) ApplyEvent(id);
        }

        void EnterWorld(int world)
        {
            if (!allDoorsOpen || !OriginalWorlds.HasEntry(world) || !maps.ContainsKey(OriginalWorlds.MapName(world)))
            {
                Say($"La puerta del mundo {world} no se abre (aún no implementado)");
                return;
            }
            string blocked = Inventory.WorldEntryBlocked(world, motor.Character);
            if (blocked != null)
            {
                Say(blocked);
                return;
            }
            Inventory.EnterWorld(world);
            LoadMap(OriginalWorlds.MapName(world));
            worldRetaken = false;
            Jingle(0x8E);                     // al entrar por la puerta (estado 8), luego la música del mundo
            var (x, y) = OriginalWorlds.Entry[world];
            motor.Place(World, World.Room(1), x, y);
            Say($"WORLD {world}");
        }

        void ExitToCastle(int code)
        {
            int world = maps[mapName].e041 - 1;
            Inventory.EnterWorld(0);
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
            {
                EnterEnemyRoom();
                harpUsed = false;
            }
            motor.Shoes = Inventory.Has(Inventory.Shoes);
            if (Collected.Count != itemsApplied) ApplyWorldItems();
            ShowWorld10Door();
            TickBossFight();
            if (TouchBaby())
                return;
            bool downPressed = motor.LastInput.Down && !downWasHeld;
            downWasHeld = motor.LastInput.Down;
            if (TryEnterShrine(downPressed))
                return;
            player.ReverseControls = Inventory.ReversedControls;
            if (motor.AttackStartedThisTick && !harpUsed && Inventory.Has(Inventory.Harp))
            {
                harpUsed = true;
                if (rocks.BreakAll(motor.Room, World) > 0) Say("El arpa rompe las piedras");
            }
            Enemies.Tick();
            var drop = Enemies.PickUp(motor);
            if (drop != null)
            {
                Sound.Effect(0x2D);
                Say(TakeLoot(drop));
            }
            if (motor.AttackStartedThisTick)
            {
                Sound.Effect(0x13);
                Enemies.Strike(motor, Collected);
            }
            if (player.ConsumeFire() && !AttractRunning)
                FireWeapon();
            Weapons.Tick(motor, World, Enemies, rocks, Collected);
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
            TickVitality();
            UpdateMusic();
            TickSteppingStones();
            if (mapName == "castle")
            {
                var spot = castleItems.Tick(motor, firedEvents.Contains((mapName, motor.Room.id)), Bosses.Defeated.Contains, Collected.Contains);
                if (spot != null) { SpawnItem(motor.Room, spot.Value.Type, spot.Value.X, spot.Value.Y); Sound.Effect(0x2E); Say("¡Ha aparecido algo!"); }
            }
            for (int i = looseItems.Count - 1; i >= 0; i--)
            {
                var (room, type, ix, iy, go) = looseItems[i];
                if (room != motor.Room) { go.SetActive(false); continue; }
                go.SetActive(true);
                if (motor.X + OriginalPhysics.FootRight < ix || motor.X + OriginalPhysics.FootLeft > ix + 15 ||
                    motor.Y - OriginalPhysics.HeadHeight > iy + 15 || motor.Y <= iy)
                    continue;
                Inventory.Add(type);
                Destroy(go);
                looseItems.RemoveAt(i);
                Jingle(0x85);
                Say(PickUp(type));
            }
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
                Inventory.Add(room.item.type);
                if (SelectedWeapon == 0 && room.item.type >= 1 && room.item.type <= 6)
                    SelectedWeapon = room.item.type;
                if (room.item.type == Inventory.GreatKey && Inventory.GreatKeyBoost(Vitality, MaxVitality, motor.Character))
                    Say($"Vitalidad máxima {MaxVitality[(int)motor.Character]}");
                if (room.item.type == Inventory.Carpet)
                    ApplyWorldItems();
                Destroy(go);
                visibleItems.RemoveAt(i);
                if (room.item.type >= 0x2D && room.item.type <= 0x2F) Sound.Effect(0x2D); else Jingle(0x85);
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
                Sound.Effect(0x29);
                // El hada rellena la vitalidad (b04:612C).
                Refill(c);
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
            if (Inventory.ImmuneTo(enemy.Type))
                return;
            int dir = enemy is Boss boss ? boss.KnockbackDir(motor.X) : enemy.X + enemy.Width / 2f >= motor.X ? -1 : 1;
            // Escudo: para el proyectil si el jugador lo mira y no está dando un espadazo.
            if (enemy.DiesOnTouch && Inventory.ShieldBlocks(enemy.Type) && motor.AttackTicks == 0 && (dir < 0) != motor.FacingLeft)
            {
                enemy.Alive = false;
                Say("Escudo");
                return;
            }
            if (enemy.DiesOnTouch)
                enemy.Alive = false;
            int damage = Inventory.ReduceDamage(enemy.Damage);
            Hurt(damage, dir, $"-{damage} (0x{enemy.Type:X2})");
        }

        // Daño al jugador: vitalidad, retroceso y 42 ticks de invulnerabilidad.
        void Hurt(int damage, int dir, string message)
        {
            if (HurtTicks > 0)
                return;
            int c = (int)motor.Character;
            PendingVitality[c] -= damage;
            if (Vitality[c] + PendingVitality[c] < 6) Sound.Effect(0x2A);   // aviso de vitalidad baja (b03:B3DC)
            motor.Knockback(dir);
            HurtTicks = OriginalPhysics.HurtInvulnerableTicks;
            Say(message);
        }

        // ---------- armas secundarias ----------

        public string WeaponName(int id) => Weapons?.Data.Get(id)?.name ?? (id == 0 ? "ninguna" : $"arma {id}");

        void FireWeapon()
        {
            if (SelectedWeapon == 0) return;
            var w = Weapons.Data.Get(SelectedWeapon);
            if (w == null) return;
            if (w.motion == "none")
            {
                // Lupa: lee la lápida del mundo y muestra la palabra del jefe.
                var t = Weapons.ReadTombstone(motor, maps[mapName].e041);
                Say(t != null ? $"La lápida dice: {maps[mapName].bossWord}" : "Aquí no hay nada que leer");
                return;
            }
            if (Weapons.CanFire(w, motor, Collected, Arrows) != null) return;
            if (Weapons.Fire(w, motor, World, Enemies, rocks, Collected) != null && !(Bosses.InFight && Inventory.Has(0x09)))
                Arrows -= w.cost;   // con la Magic Rod, en el combate contra el jefe no gasta
        }

        // ---------- eventos de sala ----------

        // Muere el último enemigo marcado de la sala: aparecen sus escaleras y se abren sus muros (en esta sala o en
        // otra del mismo mundo) y se queda así. En el castillo pone E61C = 1 (objetos con condición, CastleItems).
        void OnRoomCleared(RoomModel room)
        {
            firedEvents.Add((mapName, room.id));
            if (ApplyEvent(room.id) > 0) { Sound.Effect(0x2E); Say("Se oye algo abrirse…"); }
        }

        // Mundo 3, sala 4: el pozo enlaza hacia arriba consigo mismo. Subir 5 veces seguidas abre el muro de arriba a la
        // derecha (E0A0; salir de la sala por otro lado reinicia la cuenta, b00:479C).
        int shaftClimbs;
        void OnRoomChanged(RoomModel from, RoomModel to)
        {
            RingBell(to);
            if (Inventory.CurrentWorld != 3 || from == null || from.id != 4) return;
            if (to.id != 4 || motor.Y < OriginalPhysics.RoomBottomPx - 32) { shaftClimbs = 0; return; }
            if (++shaftClimbs < 5 || firedEvents.Contains((mapName, -1))) return;
            firedEvents.Add((mapName, -1));
            if (ApplyEvent(-1) > 0) { Sound.Effect(0x2E); Say("Se oye algo abrirse…"); }
        }

        int ApplyEvent(int openerRoom)
        {
            int n = 0;
            foreach (var r in World.Rooms)
            {
                var l = r.eventLadder;
                if (l != null && l.height > 0 && l.openedBy == openerRoom)
                {
                    World.SetLadder(r, l.col, l.row, 2, l.height);
                    mapView.RedrawCells(World, r, l.col, l.row, 2, l.height);
                    n++;
                }
                var w = r.wallBlock;
                if (w != null && w.openedBy == openerRoom)
                {
                    World.Clear(r, w.col, w.row, 4, 3);
                    mapView.RedrawCells(World, r, w.col, w.row, 4, 3);
                    n++;
                }
            }
            return n;
        }

        // ---------- jefes ----------

        void TickBossFight()
        {
            if (UnityEngine.InputSystem.Keyboard.current != null && !UnityEngine.InputSystem.Keyboard.current.anyKey.isPressed)
                Bosses.KeysReleased();
            Bosses.Tick(Enemies, () => Enemies.SpawnBoss(Bosses.Info, Inventory.Has(0x07), Inventory.Has(0x08), Inventory.Has(0x0B)));
            World.LockedRoom = Bosses.LocksExits ? Bosses.Room : null;
            World.LockAllowsRight = Bosses.RightExitOpen;
            World.BossFight = Bosses.InFight;
            if (Bosses.FreezesPlayer) motor.Stunned = 2;
            if (Bosses.Current == BossFight.Stage.KeyOnFloor)
            {
                var (kx, ky) = Bosses.KeyPos;
                if (keyView == null) keyView = ShowPickup(motor.Room, kx, ky, new Color(1f, 0.85f, 0.2f), "Gran Llave");
                if (motor.X + OriginalPhysics.FootRight >= kx && motor.X + OriginalPhysics.FootLeft <= kx + 15 &&
                    motor.Y - OriginalPhysics.HeadHeight <= ky + 15 && motor.Y > ky)
                {
                    Destroy(keyView);
                    keyView = null;
                    Inventory.Add(Inventory.GreatKey);
                    if (Inventory.GreatKeyBoost(Vitality, MaxVitality, motor.Character))
                        Say($"Gran Llave: vitalidad máxima {MaxVitality[(int)motor.Character]}");
                    Bosses.KeyTaken();
                }
            }
        }

        GameObject ShowPickup(RoomModel room, int x, int y, Color color, string name)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            StyleSprite(sr);
            sr.sprite = ItemSprite(Inventory.GreatKey);
            sr.color = Visuals.Atmosphere.Props != null ? Color.white : color;
            sr.sortingOrder = 7;
            var p = RoomCoords.PixelToWorld(room, x + 8, y + 16);
            go.transform.position = new Vector3(p.x, p.y, 0);
            return go;
        }

        // ---------- final ----------

        // Tocar al bebé (sin estar parpadeando) empieza el final (b03:B2AD → estado 17).
        bool TouchBaby()
        {
            if (HurtTicks > 0 || ending.Running) return false;
            foreach (var e in Enemies.Enemies)
            {
                if (!(e is Baby) || !e.Alive) continue;
                var (x0, y0, x1, y1) = e.Box();
                if (motor.X + OriginalPhysics.ContactRight + 1 > x0 && motor.X + OriginalPhysics.ContactLeft < x1 &&
                    motor.Y + OriginalPhysics.ContactBottom + 1 > y0 && motor.Y + OriginalPhysics.ContactTop < y1)
                {
                    SetPaused(true);
                    foreach (var other in Enemies.Enemies)
                        if (!(other is Baby)) other.Visible = false;
                    ending.Begin(endingData, new Vector2(motor.X, motor.Y), new Vector2(e.X, e.Y));
                    return true;
                }
            }
            return false;
        }

        // ---------- santuarios ----------

        // Castillo (estado 16): ↓ de pie delante del dios (x = dios + 6…10, y = dios + 16…17), con la piedra que lo
        // tapa ya rota; a la Muerte solo con la Salt. Mundos (estado 14): subir por la escalera de la fila 0 de la sala
        // de debajo del santuario hasta y < 40.
        bool TryEnterShrine(bool downPressed)
        {
            foreach (var s in shrines.shrines)
            {
                if (s.map != mapName) continue;
                var e = s.entry;
                bool enter;
                if (s.state == 16)
                    enter = downPressed && motor.State == MotorState.Ground && motor.Room.id == s.room
                            && motor.X >= e.xMin && motor.X <= e.xMax && motor.Y >= e.yMin && motor.Y <= e.yMax
                            && (e.needsItem == 0 || Inventory.Has(e.needsItem))
                            && !rocks.All.Any(r => !r.broken && r.room == motor.Room && r.x == e.godX && r.y == e.godY);
                else
                    enter = motor.Room.id == e.fromRoom && motor.Y < e.exitBelowY && System.Math.Abs(motor.X - e.x) <= 8
                            && World.Ladder(motor.Room, e.x, OriginalPhysics.RoomTopPx + 2);
                if (!enter) continue;
                if (s.state == 16 && s.entry.needsItem != 0 && !Inventory.Has(s.entry.needsItem)) return false;
                var visit = new ShrineVisit(s, shrines, Inventory, Arrows, Coins, Keys, Lives, ResurrectionCredit, motor.Character);
                musicWanted = 0x8D;
                SetPaused(true);
                if (s.state == 16)
                {
                    // Castillo (estado 16): centrado en godX + 8, suena 0x82 y se hunde 16 px en 64 frames; luego el dios.
                    Sound.PlayMusic(0x82, 0x8D);
                    StartCoroutine(Sink(true, () => shrineScreen.Show(visit)));
                }
                else
                {
                    Sound.PlayMusic(0x8D);
                    shrineScreen.Show(visit);
                }
                return true;
            }
            return false;
        }

        public void LeaveShrine(ShrineVisit v)
        {
            Arrows = v.Arrows; Coins = v.Coins; Keys = v.Keys;
            if (v.Revived)
            {
                // El otro personaje recupera 1 vida, la vitalidad al máximo y la experiencia a 0 (suena 0x30).
                int o = v.RevivedCharacter;
                Vitality[o] = MaxVitality[o];
                PendingVitality[o] = 0;
                Inventory.Experience[o] = 0;
                Say($"{(Character)o} ha revivido");
            }
            var s = v.Shrine;
            var room = World.Room(s.exit.toRoom);
            if (s.state == 16)
            {
                // Al salir se redibuja la sala y el jugador sube los 16 px en el mismo sitio.
                motor.Place(World, room, s.exit.x, s.exit.y);
                StartCoroutine(Sink(false, () => SetPaused(false)));
            }
            else
            {
                motor.PlaceOnLadder(room, s.exit.x, s.exit.y);
                SetPaused(false);
            }
        }

        // Hundirse (o subir) 16 px en 64 frames (design bible, cap. 8: sinkPixels, sinkFrames).
        System.Collections.IEnumerator Sink(bool down, System.Action then)
        {
            int px = shrines.rules.sinkPixels > 0 ? shrines.rules.sinkPixels : 16, frames = shrines.rules.sinkFrames > 0 ? shrines.rules.sinkFrames : 64;
            for (float f = 0; f < frames; f += Time.deltaTime * 60f)
            {
                float t = f / frames;
                player.HopPx = -px * (down ? t : 1 - t);
                yield return null;
            }
            player.HopPx = 0;
            then();
        }

        // ---------- partida guardada (Demeter; SaveGame.cs) ----------

        public void SaveGameNow(ShrineVisit v)
        {
            var d = new SaveData
            {
                items = new List<int>(Collected),
                worldItems = Inventory.WorldItemsPacked(),
                openedWorlds = new List<int>(Inventory.OpenedDoors),
                bossesDefeated = new List<int>(Bosses.Defeated),
                bibleUses = Inventory.BibleUses,
                experience = (int[])Inventory.Experience.Clone(),
                vitality = (int[])Vitality.Clone(),
                maxVitality = (int[])MaxVitality.Clone(),
                lives = (int[])Lives.Clone(),
                credits = (int[])ResurrectionCredit.Clone(),
                arrows = v.Arrows, coins = v.Coins, keys = v.Keys,
                weapon = SelectedWeapon, character = (int)motor.Character, world10Door = World10Door,
                takenItems = SaveData.Pack(takenItems), brokenRocks = SaveData.Pack(brokenRocks),
                openedDoors = SaveData.Pack(openedDoors), roomObjects = SaveData.Pack(roomObjectState),
                firedEvents = SaveData.Pack(firedEvents),
            };
            for (int c = 0; c < 2; c++) d.vitality[c] = Mathf.Clamp(Vitality[c] + PendingVitality[c], 0, MaxVitality[c]);
            try { d.Write(); Say("Partida guardada"); }
            catch (System.Exception e) { Say($"No se pudo guardar: {e.Message}"); }
        }

        // Se carga desde el título (L) y se aparece a la salida del santuario de Demeter.
        bool LoadSavedGame()
        {
            var d = SaveData.Read();
            if (d == null) return false;
            Collected.Clear();
            Collected.AddRange(d.items);
            Inventory.RestoreWorldItems(d.worldItems);
            Inventory.OpenedDoors.Clear();
            foreach (var w in d.openedWorlds) Inventory.OpenedDoors.Add(w);
            Bosses.Defeated.Clear();
            foreach (var w in d.bossesDefeated) Bosses.Defeated.Add(w);
            Inventory.BibleUses = d.bibleUses;
            for (int c = 0; c < 2; c++)
            {
                Inventory.Experience[c] = d.experience[c];
                Vitality[c] = d.vitality[c];
                MaxVitality[c] = d.maxVitality[c];
                Lives[c] = d.lives[c];
                ResurrectionCredit[c] = d.credits[c];
            }
            Arrows = d.arrows; Coins = d.coins; Keys = d.keys;
            SelectedWeapon = d.weapon;
            World10Door = d.world10Door;
            SaveData.Unpack(d.takenItems, takenItems);
            SaveData.Unpack(d.brokenRocks, brokenRocks);
            SaveData.Unpack(d.openedDoors, openedDoors);
            SaveData.Unpack(d.roomObjects, roomObjectState);
            SaveData.Unpack(d.firedEvents, firedEvents);
            Inventory.EnterWorld(0);
            LoadMap("castle");
            var demeter = shrines.shrines.FirstOrDefault(s => s.map == "castle" && s.kind == "password");
            var room = World.Room(demeter?.exit.toRoom ?? startRoom);
            motor.Character = (Character)d.character;
            motor.Place(World, room, demeter?.exit.x ?? startX, demeter?.exit.y ?? startY);
            Say("Partida cargada");
            return true;
        }

        // ---------- inventario ----------

        public bool CanOpenMenu => motor != null && !Busy && !shrineScreen.Open && motor.State != MotorState.Knockback && motor.State != MotorState.Carried && motor.Stunned == 0;

        public void SetPaused(bool paused) => player.Paused = paused;

        // Al cerrar el menú: el personaje elegido pasa a ser el activo y el arma elegida, la de M.
        public void ApplyMenu(Character character, int weapon)
        {
            motor.Character = character;
            if (weapon > 0) SelectedWeapon = weapon;
        }

        // Angel Ring: a la sala 72 del castillo (el santuario de Demeter). Desde un mundo se pierden sus objetos.
        public void UseAngelRing()
        {
            int world = Inventory.CurrentWorld;
            if (world > 0)
            {
                Inventory.ForgetWorldItems(world);
                Inventory.EnterWorld(0);
                LoadMap("castle");
            }
            var room = World.Room(72);
            var (x, y) = StandSpot(room);
            motor.Place(World, room, x, y);
            Say("Angel Ring: santuario de Demeter");
        }

        // Ala: en el castillo, a la puerta de un mundo ya abierto.
        public bool UseWing(int world)
        {
            if (Inventory.CurrentWorld != 0) { Say("El ala solo funciona en el castillo"); return false; }
            if (!Inventory.OpenedDoors.Contains(world)) { Say($"La puerta del mundo {world} aún no se ha abierto"); return false; }
            var room = World.Room(Inventory.WingRoom(world));
            if (room == null || room.doorWorld == 0) return false;
            motor.Place(World, room, room.door.x + OriginalWorlds.DoorExitDx, room.door.y + OriginalWorlds.DoorExitDy);
            Say($"Ala: puerta del mundo {world}");
            return true;
        }

        // Primer sitio donde se puede estar de pie, empezando por el centro de la sala.
        (int, int) StandSpot(RoomModel room)
        {
            foreach (int x in new[] { 128, 96, 160, 64, 192, 40, 216 })
                for (int y = OriginalPhysics.RoomTopPx + 16; y < OriginalPhysics.RoomBottomPx; y += 8)
                    if (World.Solid(room, x, y) && !World.Solid(room, x, y - 1) && !World.Solid(room, x, y - 14))
                        return (x, y);
            return (startX, startY);
        }

        // Experiencia por muerte (BF27[tipo], ×2 con el Vase); a 48, se rellena la vitalidad.
        void GainExperience(Enemy e)
        {
            int exp = e.ExperienceOverride ?? enemyCatalog?.Find(e.Type)?.experience ?? 0;
            if (exp <= 0) return;
            int c = (int)motor.Character;
            if (Inventory.AddExperience(motor.Character, exp) && Vitality[c] < MaxVitality[c])
            {
                Refill(c);
                Say("Experiencia: vitalidad llena");
            }
        }

        // Carpet: en el mundo 8, las salas con lava se rellenan de piedra de la fila 12 a la 19.
        // Objetos que cambian salas de su mundo: Carpet (mundo 8), Oar y Candle (mundo 5). Se aplican al cargar el mapa y
        // cada vez que cambia lo que se lleva (tiendas, objetos recogidos).
        void ApplyWorldItems()
        {
            itemsApplied = Collected.Count;
            if (Inventory.CurrentWorld == 5)
            {
                // Oar (b03:B037): en las salas 12 y 13 la superficie del agua (B6, la fila de agua con aire encima) es suelo.
                if (Inventory.Has(Inventory.Oar))
                    foreach (var id in new[] { 12, 13 })
                    {
                        var room = World.Room(id);
                        if (room == null) continue;
                        for (int row = 0; row < room.collision.Length; row++)
                            for (int col = 0; col < 32; col++)
                                if (room.collision[row][col] == '~' && (row == 0 || room.collision[row - 1][col] != '~'))
                                {
                                    World.SetSolid(room, col, row, 1, 1);
                                    mapView.RedrawCells(World, room, col, row, 1, 1);
                                }
                    }
                // Candle (b00:55F7): sin ella la sala 6 es oscura: los muros se dibujan como el fondo y siguen ahí.
                var dark = World.Room(6);
                if (dark != null) mapView.SetDark(World, dark, !Inventory.Has(Inventory.Candle));
            }
            ApplyCarpet();
        }

        int itemsApplied;

        // Piedras del agua del mundo 5, sala 11 (SteppingStones.cs). Vuelven a empezar al entrar en la sala.
        SteppingStones stones;
        RoomModel stonesRoom;

        void TickSteppingStones()
        {
            var room = motor.Room;
            bool here = room.objects != null && System.Array.Exists(room.objects, o => (o.type & 0x0F) == 6);
            if (stonesRoom != null && stonesRoom != room)
            {
                // Al salir se quita la piedra que hubiera (al volver empieza de nuevo).
                if (stones?.Current is int c && World.Room(stonesRoom.id) == stonesRoom) SetStone(stonesRoom, c, false);
                stones = null;
                stonesRoom = null;
            }
            if (!here) return;
            if (stonesRoom != room)
            {
                stonesRoom = room;
                stones = new SteppingStones();
                stones.Appeared += x => { SetStone(room, x, true); Sound.Effect(0x04); };
                stones.Vanished += x => SetStone(room, x, false);
            }
            stones.Tick(motor.X, motor.Y);
        }

        void SetStone(RoomModel room, int x, bool on)
        {
            int col = x / 8, row = (0x78 - OriginalPhysics.RoomTopPx) / 8;
            if (on) World.SetSolid(room, col, row, 2, 2);
            else World.CopyFrom(room, room.collision, col, row, 2, 2);
            mapView.RedrawCells(World, room, col, row, 2, 2);
        }

        // E06E (0..3), elegido al empezar la partida: dónde sale la puerta del mundo 10.
        public int World10Door = new System.Random().Next(4);
        RoomModel world10Room;

        void ShowWorld10Door()
        {
            if (mapName != "castle" || world10Room != null || !Bosses.Defeated.Contains(9) || Bosses.Active) return;
            var room = World.Room(OriginalWorlds.World10DoorRoom[World10Door]);
            if (room == null) return;
            var (x, y) = OriginalWorlds.World10DoorPos[World10Door];
            world10Room = room;
            room.doorWorld = 10;
            room.door = new PointJson { x = x, y = y };
            mapView.ShowDoor(room, x, y);
        }

        void ApplyCarpet()
        {
            if (Inventory.CurrentWorld != 8 || !Inventory.Has(Inventory.Carpet)) return;
            foreach (var room in World.Rooms.Where(r => r.collision.Any(row => row.Contains('L'))))
            {
                World.SetSolid(room, 0, 12, 32, 8);
                mapView.RedrawCells(World, room, 0, 12, 32, 8);
            }
        }

        // Daga: tecleando UMBRELLA mueren los murciélagos de la sala (con su experiencia y botín).
        void OnTextInput(char ch)
        {
            if (player != null && !player.Paused) Bosses?.Type(ch);
            if (!char.IsLetter(ch)) return;
            typed = (typed + char.ToUpperInvariant(ch));
            if (typed.Length > 8) typed = typed.Substring(typed.Length - 8);
            if (typed != "UMBRELLA" || !Inventory.Has(Inventory.Dagger)) return;
            typed = "";
            int n = 0;
            foreach (var e in Enemies.Enemies.Where(e => e.Alive && e.Type == 0x0E).ToList())
            {
                Enemies.Damage(e, e.Hp);
                n++;
            }
            Say($"UMBRELLA: {n} murciélagos");
        }

        void OnDestroy()
        {
            // La puerta del mundo 10 se pone en los datos de la sala: se quita para no tocar el asset importado.
            if (world10Room != null) { world10Room.doorWorld = 0; world10Room.door = new PointJson(); }
            if (UnityEngine.InputSystem.Keyboard.current != null)
                UnityEngine.InputSystem.Keyboard.current.onTextInput -= OnTextInput;
        }

        void ShowProjectile(Projectile p)
        {
            var go = new GameObject($"Proyectil {p.Weapon.name}");
            var sr = go.AddComponent<SpriteRenderer>();
            StyleSprite(sr);
            sr.sprite = enemySprite;
            sr.color = p.Id switch { 1 => Color.white, 2 => new Color(0.9f, 0.8f, 0.6f), 3 => new Color(1f, 0.5f, 0.1f), 4 => new Color(1f, 0.2f, 0.1f), _ => Color.gray };
            sr.sortingOrder = 9;
            projectileViews[p] = sr;
        }

        void EnterEnemyRoom()
        {
            foreach (var v in enemyViews.Values)
                if (v != null) Destroy(v.gameObject);
            enemyViews.Clear();
            Enemies.EnterRoom(World, motor.Room, motor, motor.Room.markedRoom && !firedEvents.Contains((mapName, motor.Room.id)));
            Bosses.EnterRoom(Inventory.CurrentWorld, motor.Room);
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

        readonly Dictionary<Enemy, (int dx, int dy)> bossArt = new Dictionary<Enemy, (int, int)>();
        BossGfxList bossGfx;

        [System.Serializable] class BossGfx { public int world, dx, dy, w, h; public string file; }
        [System.Serializable] class BossGfxList { public BossGfx[] bosses = new BossGfx[0]; }

        (Sprite sprite, int dx, int dy) BossArt(int world)
        {
            if (bossGfx == null)
            {
                var j = Resources.Load<TextAsset>("Gfx/bosses_gfx");
                bossGfx = j != null ? JsonUtility.FromJson<BossGfxList>(j.text) : new BossGfxList();
            }
            var b = System.Array.Find(bossGfx.bosses, x => x.world == world);
            var tex = b != null ? Resources.Load<Texture2D>("Gfx/" + b.file) : null;
            if (tex == null) return (null, 0, 0);
            return (Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 1), RoomCoords.PixelsPerUnit), b.dx, b.dy);
        }

        // Con el estilo del remake, contorno de tinta y destello (Visuals/Atmosphere).
        static void StyleSprite(SpriteRenderer sr)
        {
            if (Visuals.Atmosphere.CharacterMaterial != null) sr.sharedMaterial = Visuals.Atmosphere.CharacterMaterial;
        }

        public SpriteRenderer EnemyView(Enemy e) => e != null && enemyViews.TryGetValue(e, out var sr) ? sr : null;

        void ShowEnemy(Enemy e)
        {
            var go = new GameObject($"Enemigo 0x{e.Type:X2}");
            var sr = go.AddComponent<SpriteRenderer>();
            StyleSprite(sr);
            sr.sprite = enemySprite;
            sr.color = EnemyColor(e);
            sr.sortingOrder = 8;
            enemyViews[e] = sr;
            // Jefe: su dibujo del original, si se ha importado (tools/extract/boss_gfx.py; un solo fotograma [I]).
            if (e is Boss && BossArt(Bosses.World) is var art && art.sprite != null)
            {
                sr.sprite = art.sprite;
                sr.color = Color.white;
                bossArt[e] = (art.dx, art.dy);
            }
        }

        void LateUpdate()
        {
            if (motor == null)
                return;
            foreach (var p in projectileViews.Keys.Where(p => !p.Alive).ToList())
            {
                if (projectileViews[p] != null) Destroy(projectileViews[p].gameObject);
                projectileViews.Remove(p);
            }
            foreach (var (p, sr) in projectileViews.Select(kv => (kv.Key, kv.Value)))
            {
                var (x0, y0, x1, y1) = Weapons.Box(p);
                var bl = RoomCoords.PixelToWorld(Weapons.Room ?? motor.Room, (int)x0, (int)y1);
                sr.transform.position = new Vector3(bl.x, bl.y, 0);
                sr.transform.localScale = new Vector3((x1 - x0) / 16f, (y1 - y0) / 16f, 1);
                var weaponArt = Visuals.Atmosphere.EnemyArt;
                if (weaponArt != null)
                {
                    sr.sprite = weaponArt.Weapon(p.Id, (p.Age / 3) & 1);
                    sr.flipX = p.Dir < 0;
                    var size = sr.sprite.bounds.size;
                    sr.transform.localScale = new Vector3((x1 - x0) / 16f / size.x, (y1 - y0) / 16f / size.y, 1);
                    if (!p.Exploding && p.Id != 5) sr.color = Color.white;
                    if (sr.flipX) sr.transform.position = new Vector3(bl.x + (x1 - x0) / 16f, bl.y, 0);   // como los enemigos
                }
                else
                    sr.sprite = enemySprite;
                if (p.Exploding) sr.color = new Color(1f, 0.9f, 0.3f, 0.7f);
                else if (p.Id == 5) sr.color = p.Armed && (p.Age / 4) % 2 == 0 ? Color.white : Color.gray;
            }
            foreach (var e in enemyViews.Keys.Where(e => !e.Alive).ToList())
            {
                if (enemyViews[e] != null) Destroy(enemyViews[e].gameObject);
                enemyViews.Remove(e);
            }
            foreach (var (e, sr) in enemyViews.Select(kv => (kv.Key, kv.Value)))
            {
                if (sr == null) continue;
                sr.enabled = e.Alive && e.Visible;
                if (bossArt.TryGetValue(e, out var off))
                {
                    // Como en el original, en la rejilla de 8 px (pivote arriba a la izquierda).
                    var tl = RoomCoords.PixelToWorld(e.Room, ((int)e.X & ~7) + off.dx, ((int)e.Y & ~7) + off.dy);
                    sr.transform.position = new Vector3(tl.x, tl.y, 0);
                    sr.transform.localScale = Vector3.one;
                    continue;
                }
                var (x0, y0, x1, y1) = e.DrawRect();
                var bottomLeft = RoomCoords.PixelToWorld(e.Room, (int)x0, (int)y1);
                sr.transform.position = new Vector3(bottomLeft.x, bottomLeft.y, 0);
                sr.transform.localScale = new Vector3((x1 - x0) / 16f, Mathf.Max(0.05f, (y1 - y0) / 16f), 1);
                sr.flipX = e.FacingLeft;
                // Estilo del remake: el dibujo de su arquetipo (Visuals/EnemyArt); si no, el cuadrado de color.
                var art = Visuals.Atmosphere.EnemyArt;
                if (art != null)
                {
                    // El dibujo, recortado a su silueta, llena exactamente la caja que se pinta (la de colisión).
                    sr.sprite = art.For(e, Time.time);
                    sr.color = Color.white;
                    var size = sr.sprite.bounds.size;
                    sr.transform.localScale = new Vector3((x1 - x0) / 16f / size.x, Mathf.Max(0.05f, (y1 - y0) / 16f) / size.y, 1);
                }
                else { sr.sprite = enemySprite; sr.color = EnemyColor(e); }
                // flipX voltea alrededor del pivote (abajo a la izquierda): mirando a la izquierda el dibujo caería una
                // caja entera a la izquierda de donde está el enemigo. Se coloca desde la esquina de la derecha.
                if (sr.flipX) sr.transform.position = new Vector3(bottomLeft.x + (x1 - x0) / 16f, bottomLeft.y, 0);
            }
            // Ascensores de la sala: dos bloques de 16 × 16 (x … x + 31, y … y + 15).
            int n = 0;
            if (Lifts != null)
                foreach (var l in Lifts.InRoom(motor.Room.id))
                {
                    if (n == liftViews.Count)
                    {
                        var go = new GameObject("Ascensor");
                        var lsr = go.AddComponent<SpriteRenderer>();
                        lsr.sprite = LiftSprite();
                        lsr.color = Color.white;
                        lsr.sortingOrder = 6;
                        liftViews.Add(lsr);
                    }
                    var v = liftViews[n++];
                    v.enabled = true;
                    v.sprite = Visuals.Atmosphere.Props?.Lift ?? LiftSprite();
                    var bl = RoomCoords.PixelToWorld(motor.Room, l.X, l.Y + Lifts.Box.height);
                    v.transform.position = new Vector3(bl.x, bl.y, 0);
                    v.transform.localScale = Vector3.one;
                }
            for (; n < liftViews.Count; n++)
                liftViews[n].enabled = false;
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
                wasInLiquid = false;
                return;
            }
            if (!wasInLiquid) { wasInLiquid = true; Sound.Effect(0x24); }   // entrar en el agua (b03:B063)
            bool aphroditeInWater = motor.Character == Character.Aphrodite && motor.Liquid == TileClass.Water;
            int every = aphroditeInWater ? OriginalPhysics.AphroditeWaterDamageTicks : OriginalPhysics.LiquidDamageTicks;
            if (++liquidTicks < every)
                return;
            liquidTicks = 0;
            PendingVitality[(int)motor.Character]--;
            Sound.Effect(0x25);
        }
        bool wasInLiquid;

        // ---------- música (docs/music.md, "Canciones") ----------

        int musicWanted = -1;            // la de fondo que toca ahora (sin contar las fanfarrias)
        bool worldRetaken;               // tras coger la Gran Llave sigue la 0x8B hasta salir del mundo

        // Fanfarria corta y luego la música de fondo que toque.
        void Jingle(int id)
        {
            musicWanted = -1;
            Sound.PlayMusic(id, BackgroundMusic());
            musicWanted = BackgroundMusic();
        }

        int BackgroundMusic()
        {
            if (shrineScreen != null && shrineScreen.Open) return 0x8D;
            if (Bosses != null && Bosses.Room == motor.Room)
            {
                switch (Bosses.Current)
                {
                    case BossFight.Stage.Invoking: return 0x81;
                    case BossFight.Stage.Summoning:
                    case BossFight.Stage.Fighting: return 0x90;
                    case BossFight.Stage.Dissolving: return 0x84;
                    case BossFight.Stage.KeyOnFloor: return 0x91;
                    case BossFight.Stage.Fanfare:
                    case BossFight.Stage.RightExitOnly: return 0x8B;
                }
                if (Bosses.Listening) return 0x8F;
            }
            if (mapName == "castle") return 0x88;
            return worldRetaken ? 0x8B : 0x8A;
        }

        // Cada tick: si la música de fondo cambia (sala del jefe, combate, mundo, castillo…), se pide.
        void UpdateMusic()
        {
            if (Bosses.Current == BossFight.Stage.Fanfare) worldRetaken = true;
            int want = BackgroundMusic();
            if (want == musicWanted) return;
            // Las fanfarrias sin bucle (invocación, jefe vencido) dejan detrás la siguiente.
            if (want == 0x81) Sound.PlayMusic(0x81, 0x90);
            else if (want == 0x84) Sound.PlayMusic(0x84, 0x91);
            else if (!(want == 0x88 && Sound.Instance != null && Sound.Instance.CurrentSong == 0x87)) Sound.PlayMusic(want);
            musicWanted = want;
        }

        // La Campana (0x14, b00:5D24): en el castillo, al entrar en una sala de la lista del mundo de número más alto con
        // la puerta desbloqueada y sin abrir, suena 0x28. [I] Desbloqueada = el mundo 1, o el jefe del anterior vencido;
        // el mundo 10 (según E06E) no se mira.
        static readonly int[][] BellRooms =
        {
            new int[0], new[] { 104, 90, 91, 92, 105, 117, 118 }, new[] { 98, 86, 87, 97, 99, 110, 111, 112 },
            new[] { 63, 55, 56, 64, 76, 77, 78 }, new[] { 57, 67, 68 }, new[] { 62, 54, 55, 74, 75, 76 },
            new[] { 114, 101, 113, 126, 127 }, new[] { 6, 2, 7, 14 }, new[] { 23, 13, 22, 33 }, new[] { 108, 2, 3, 4 },
        };

        void RingBell(RoomModel to)
        {
            if (mapName != "castle" || !Inventory.Has(0x14)) return;
            for (int w = 9; w >= 1; w--)
            {
                bool unlocked = w == 1 || Bosses.Defeated.Contains(w - 1);
                if (!unlocked || Inventory.OpenedDoors.Contains(w)) continue;
                if (System.Array.IndexOf(BellRooms[w], to.id) >= 0) { Sound.Effect(0x28); Say("La campana suena"); }
                return;
            }
        }

        public void Refill(int c) => PendingVitality[c] = MaxVitality[c] - Vitality[c];

        // b01:6171: cada 4 ticks, 1 unidad del pendiente de cada personaje. La vida se pierde cuando la barra llega a 0.
        void TickVitality()
        {
            if (++vitalityTicks % 4 != 0) return;
            for (int c = 0; c < 2; c++)
            {
                if (PendingVitality[c] > 0) { PendingVitality[c]--; if (Vitality[c] < MaxVitality[c]) Vitality[c]++; else PendingVitality[c] = 0; }
                else if (PendingVitality[c] < 0) { PendingVitality[c]++; Vitality[c]--; }
            }
            if (Vitality[(int)motor.Character] <= 0)
                LoseLife();
        }

        // Vida perdida (estado 6; state_machine.md): se gasta una vida de reserva del personaje en juego y, si no le
        // queda ninguna, una del otro, que pasa a jugar (b03:B337). Sin vidas en ninguno, game over (estado 7). El que
        // muere se queda con la experiencia y la vitalidad a 0. [I] Se reaparece en la entrada del mapa (o en la sala
        // del jefe durante el combate).
        void LoseLife()
        {
            int c = (int)motor.Character, o = 1 - c;
            Vitality[c] = 0;
            Inventory.Experience[c] = 0;
            PendingVitality[0] = PendingVitality[1] = 0;
            liquidTicks = 0;
            if (Lives[c] > 0) Lives[c]--;
            else if (Lives[o] > 0) { Lives[o]--; motor.Character = (Character)o; c = o; }
            else { StartGameOver(); return; }
            Sound.Effect(0x2B);
            Jingle(0x95);
            if (Vitality[c] <= 0) Vitality[c] = MaxVitality[c];
            if (Bosses.InFight)
            {
                // En pleno combate: se reaparece en la sala del jefe y se vuelve a invocar con toda su vida.
                var (bx, by) = StandSpot(motor.Room);
                motor.Place(World, motor.Room, bx, by);
                EnterEnemyRoom();
                Bosses.PlayerDied();
                Say("Vida perdida");
                return;
            }
            int world = maps[mapName].e041 - 1;
            if (world >= 1 && OriginalWorlds.HasEntry(world))
            {
                var (x, y) = OriginalWorlds.Entry[world];
                motor.Place(World, World.Room(1), x, y);
            }
            else
                motor.Place(World, World.Room(startRoom), startX, startY);
            Say(motor.Character == (Character)c ? "Vida perdida" : $"Vida perdida: juega {motor.Character}");
        }

        // Estado 7: GAME OVER unos 7 s y vuelta al logo.
        float gameOverUntil;
        void StartGameOver()
        {
            GameOver = true;
            SetPaused(true);
            gameOverUntil = Time.time + 7f;
            Sound.Effect(0x2B);
            Sound.PlayMusic(0x95);
            Say("GAME OVER");
        }

        void OnRockBroken(Rocks.Rock rock)
        {
            mapView.ShowRock(rock.room, rock.x, rock.y, false);
            mapView.ClearSolid(rock.room, rock.x, rock.y, 2, 2);
            var room = rock.room;
            if (room.hasItem && room.item.x == rock.x && room.item.y == rock.y)
                ShowItem(room);
            Sound.Effect(0x20);
            Say("Piedra rota");
        }

        // Objeto suelto en una sala (los que aparecen con una condición): se queda hasta recogerlo.
        readonly List<(RoomModel room, int type, int x, int y, GameObject go)> looseItems = new List<(RoomModel, int, int, int, GameObject)>();
        readonly CastleItems castleItems = new CastleItems();

        void SpawnItem(RoomModel room, int type, int x, int y)
        {
            if (looseItems.Any(l => l.room == room && l.type == type)) return;
            var go = new GameObject($"Objeto 0x{type:X2} (sala {room.id})");
            var sr = go.AddComponent<SpriteRenderer>();
            StyleSprite(sr);
            sr.sprite = ItemSprite(type);
            sr.sortingOrder = 7;
            var p = RoomCoords.PixelToWorld(room, x + 8, y + 16);
            go.transform.position = new Vector3(p.x, p.y, 0);
            looseItems.Add((room, type, x, y, go));
        }

        void ShowItem(RoomModel room)
        {
            if (takenItems.Contains((mapName, room.id)) || itemSprite == null)
                return;
            var go = new GameObject($"Objeto 0x{room.item.type:X2} (sala {room.id})");
            var sr = go.AddComponent<SpriteRenderer>();
            StyleSprite(sr);
            sr.sprite = ItemSprite(room.item.type);
            sr.sortingOrder = 5;
            go.transform.position = RoomCoords.PixelToWorld(room, room.item.x + 8, room.item.y + 16);
            visibleItems.Add((room, go));
        }

        // Con el estilo del remake, el dibujo de cada objeto (Visuals/PropArt); si no, el rombo provisional.
        Sprite ItemSprite(int type) => Visuals.Atmosphere.Props?.Item(type) ?? itemSprite;

        public void Say(string text)
        {
            Message = text;
            messageUntil = Time.time + 2.5f;
        }

        void Update()
        {
            if (Message != "" && Time.time > messageUntil)
                Message = "";
            if (GameOver)
            {
                if (Time.time > gameOverUntil) { GameOver = false; AttractMode.Go(AttractMode.Mode.Attract); }
                return;
            }
            if (Busy)
                return;
            // Depuración: K da una llave (en el original salen de piedras, enemigos y tiendas).
            if (!TypingWord && UnityEngine.InputSystem.Keyboard.current?.kKey.wasPressedThisFrame == true)
            {
                Keys = Mathf.Min(999, Keys + 1);
                Say($"[depuración] +1 llave ({Keys})");
            }
            // Biblia: CTRL congela a los enemigos 150 ticks (16 veces en la partida).
            var kbd = UnityEngine.InputSystem.Keyboard.current;
            if (kbd != null && (kbd.leftCtrlKey.wasPressedThisFrame || kbd.rightCtrlKey.wasPressedThisFrame) && !player.Paused
                && Enemies.FreezeTicks == 0 && Inventory.UseBible())
            {
                Enemies.FreezeTicks = Inventory.BibleTicks;
                Say($"Biblia: enemigos congelados ({16 - Inventory.BibleUses} usos)");
            }
            // Depuración: F4 lleva a la sala del bebé (mundo 10, sala 6).
            if (UnityEngine.InputSystem.Keyboard.current?.f4Key.wasPressedThisFrame == true && !Bosses.Active && !ending.Running && maps.ContainsKey("world10"))
            {
                Inventory.EnterWorld(10);
                LoadMap("world10");
                var room = World.Room(6);
                if (room != null)
                {
                    motor.Place(World, room, 24, 184);
                    Say("[depuración] sala del bebé: rompe las piedras");
                }
            }
            // Depuración: F2 lleva a la sala del jefe del mundo en el que estás.
            if (UnityEngine.InputSystem.Keyboard.current?.f2Key.wasPressedThisFrame == true && Inventory.CurrentWorld > 0 && !Bosses.Active)
            {
                var info = Bosses.InfoFor(Inventory.CurrentWorld);
                var room = info != null ? World.Room(info.room) : null;
                if (room != null)
                {
                    var (bx, by) = StandSpot(room);
                    motor.Place(World, room, bx, by);
                    Say($"[depuración] sala del jefe: escribe {info.word}");
                }
            }
            // Depuración: G da las seis armas y 50 flechas para probarlas.
            if (!TypingWord && UnityEngine.InputSystem.Keyboard.current?.gKey.wasPressedThisFrame == true)
            {
                for (int id = 1; id <= 6; id++)
                    Inventory.Add(id);
                Arrows = Mathf.Min(999, Arrows + 50);
                if (SelectedWeapon == 0) SelectedWeapon = 1;
                Say($"[depuración] las 6 armas y +50 flechas ({Arrows})");
            }
        }
    }
}
