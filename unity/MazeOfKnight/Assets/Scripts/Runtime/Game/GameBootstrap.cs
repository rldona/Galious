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
        public WeaponSystem Weapons { get; private set; }
        public int SelectedWeapon;   // E510: 1 flecha … 6 lupa (0 = ninguna)
        public Inventory Inventory { get; private set; }
        public PlayerMotor Motor => motor;
        EnemyCatalog enemyCatalog;
        InventoryMenu menu;
        bool harpUsed;               // el arpa rompe las piedras con el primer espadazo de cada visita a la sala
        string typed = "";           // letras tecleadas (la daga: UMBRELLA)
        ShrinesData shrines = new ShrinesData();
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
            if (UnityEngine.InputSystem.Keyboard.current != null)
                UnityEngine.InputSystem.Keyboard.current.onTextInput += OnTextInput;
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
            ApplyCarpet();
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
                Say(TakeLoot(drop));
            if (motor.AttackStartedThisTick)
                Enemies.Strike(motor, Collected);
            if (player.ConsumeFire())
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
                    ApplyCarpet();
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
            if (Inventory.ImmuneTo(enemy.Type))
                return;
            int dir = enemy.X + enemy.Width / 2f >= motor.X ? -1 : 1;
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
            Vitality[c] -= damage;
            motor.Knockback(dir);
            HurtTicks = OriginalPhysics.HurtInvulnerableTicks;
            Say(message);
            if (Vitality[c] <= 0)
                LoseLife();
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
            if (Weapons.Fire(w, motor, World, Enemies, rocks, Collected) != null)
                Arrows -= w.cost;
        }

        // ---------- eventos de sala ----------

        // Muere el último enemigo marcado de la sala: aparecen sus escaleras y se abren sus muros (en esta sala o en
        // otra del mismo mundo) y se queda así. En el castillo pone E61C = 1 (objetos con condición, en el backlog).
        void OnRoomCleared(RoomModel room)
        {
            firedEvents.Add((mapName, room.id));
            if (ApplyEvent(room.id) > 0) Say("Se oye algo abrirse…");
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
                shrineScreen.Show(new ShrineVisit(s, shrines, Inventory, Arrows, Coins, Keys));
                SetPaused(true);
                return true;
            }
            return false;
        }

        public void LeaveShrine(ShrineVisit v)
        {
            Arrows = v.Arrows; Coins = v.Coins; Keys = v.Keys;
            var s = v.Shrine;
            var room = World.Room(s.exit.toRoom);
            if (s.state == 16)
                motor.Place(World, room, s.exit.x, s.exit.y);
            else
                motor.PlaceOnLadder(room, s.exit.x, s.exit.y);
            SetPaused(false);
        }

        // ---------- inventario ----------

        public bool CanOpenMenu => motor != null && !shrineScreen.Open && motor.State != MotorState.Knockback && motor.State != MotorState.Carried && motor.Stunned == 0;

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
            int exp = enemyCatalog?.Find(e.Type)?.experience ?? 0;
            if (exp <= 0) return;
            int c = (int)motor.Character;
            if (Inventory.AddExperience(motor.Character, exp) && Vitality[c] < MaxVitality[c])
            {
                Vitality[c] = MaxVitality[c];
                Say("Experiencia: vitalidad llena");
            }
        }

        // Carpet: en el mundo 8, las salas con lava se rellenan de piedra de la fila 12 a la 19.
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
            if (UnityEngine.InputSystem.Keyboard.current != null)
                UnityEngine.InputSystem.Keyboard.current.onTextInput -= OnTextInput;
        }

        void ShowProjectile(Projectile p)
        {
            var go = new GameObject($"Proyectil {p.Weapon.name}");
            var sr = go.AddComponent<SpriteRenderer>();
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
            Inventory.Experience[c] = 0;
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

        public void Say(string text)
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
            // Biblia: CTRL congela a los enemigos 150 ticks (16 veces en la partida).
            var kbd = UnityEngine.InputSystem.Keyboard.current;
            if (kbd != null && (kbd.leftCtrlKey.wasPressedThisFrame || kbd.rightCtrlKey.wasPressedThisFrame) && !player.Paused
                && Enemies.FreezeTicks == 0 && Inventory.UseBible())
            {
                Enemies.FreezeTicks = Inventory.BibleTicks;
                Say($"Biblia: enemigos congelados ({16 - Inventory.BibleUses} usos)");
            }
            // Depuración: G da las seis armas y 50 flechas para probarlas.
            if (UnityEngine.InputSystem.Keyboard.current?.gKey.wasPressedThisFrame == true)
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
