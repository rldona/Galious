using System;
using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;

namespace MazeOfKnight.Game
{
    // Objetos del jugador y sus efectos (docs/items_enemies.md: "Objetos recogibles", "Flags: personajes y mundos",
    // "Vitalidad, experiencia y vidas"). Cada objeto tiene una sola flag, compartida por los dos personajes; los de
    // mundo (0x07–0x0A) se guardan por mundo y solo valen dentro de él. 'Active' es la lista que consulta el resto del
    // juego (lo que se tiene ahora).
    public class Inventory
    {
        public const int HolyWater = 0x07, Map = 0x0A, GreatKey = 0x0C, Necklace = 0x0D, Hoi = 0x13, Shoes = 0x11,
            AngelRing = 0x15, Oar = 0x10, Candle = 0x16, Carpet = 0x18, HatHelmet = 0x19, Lamp = 0x1A, Vase = 0x1B, Bible = 0x20, Harp = 0x21,
            ConchShell = 0x23, Dagger = 0x26, Wing = 0x27;

        public readonly ItemsData Data;
        public readonly List<int> Active;
        readonly Dictionary<int, HashSet<int>> worldItems = new Dictionary<int, HashSet<int>>();
        public int CurrentWorld { get; private set; }            // 0 = castillo
        public readonly int[] Experience = new int[2];             // por personaje (Aphrodite = 0, Popolon = 1)
        public int BibleUses;
        public readonly HashSet<int> OpenedDoors = new HashSet<int>();   // mundos cuya puerta se ha abierto (Ala)

        public Inventory(ItemsData data, List<int> active)
        {
            Data = data ?? new ItemsData();
            Active = active;
        }

        public bool Has(int type) => Active.Contains(type);

        bool PerWorld(int type) => type >= HolyWater && type <= Map;

        // Recoger un objeto. Devuelve false si ya se tenía.
        public bool Add(int type)
        {
            if (PerWorld(type) && CurrentWorld > 0)
            {
                if (!worldItems.TryGetValue(CurrentWorld, out var set)) worldItems[CurrentWorld] = set = new HashSet<int>();
                set.Add(type);
            }
            if (Active.Contains(type)) return false;
            Active.Add(type);
            return true;
        }

        // Al entrar en un mundo (o volver al castillo, world = 0) los objetos de mundo pasan a ser los de ese mundo.
        public void EnterWorld(int world)
        {
            Active.RemoveAll(PerWorld);
            CurrentWorld = world;
            if (world > 0)
            {
                OpenedDoors.Add(world);
                if (worldItems.TryGetValue(world, out var set)) Active.AddRange(set);
            }
        }

        // Partida guardada: objetos de cada mundo como mundo * 256 + objeto.
        public List<int> WorldItemsPacked()
        {
            var l = new List<int>();
            foreach (var kv in worldItems) foreach (var t in kv.Value) l.Add(kv.Key * 256 + t);
            return l;
        }

        public void RestoreWorldItems(IEnumerable<int> packed)
        {
            worldItems.Clear();
            foreach (var v in packed)
            {
                if (!worldItems.TryGetValue(v / 256, out var set)) worldItems[v / 256] = set = new HashSet<int>();
                set.Add(v % 256);
            }
        }

        // Angel Ring desde un mundo: se pierden los objetos de ese mundo.
        public void ForgetWorldItems(int world) => worldItems.Remove(world);

        IEnumerable<ItemInfo> Owned => Active.Select(Data.Get).Where(i => i != null);

        // Hat Helmet: el daño de enemigos y proyectiles se queda en la mitad (redondeando hacia abajo); 1 sigue en 1.
        public int ReduceDamage(int damage) => Has(HatHelmet) && damage > 1 ? damage / 2 : damage;

        // Corona, casco, armadura, triángulo y jarra: tocar ese enemigo no hace nada.
        public bool ImmuneTo(int enemyType) =>
            Owned.Any(i => i.effect.kind == "contactImmunity" && i.effect.enemyTypes.Contains(enemyType));

        // El escudo mejor que se tenga para ese proyectil.
        public bool ShieldBlocks(int projectileType)
        {
            var shield = Owned.Where(i => i.effect.kind == "shield").OrderByDescending(i => i.effect.level).FirstOrDefault();
            return shield != null && shield.effect.blocks.Contains(projectileType);
        }

        public int ShieldLevel => Owned.Where(i => i.effect.kind == "shield").Select(i => i.effect.level).DefaultIfEmpty(0).Max();

        // Experiencia por muerte (×2 con el Vase). Al llegar a 48 se restan 48 y se rellena la vitalidad: true.
        public bool AddExperience(Character c, int amount)
        {
            int i = (int)c;
            Experience[i] += Has(Vase) ? amount * 2 : amount;
            if (Experience[i] < Data.vitality.expMax) return false;
            Experience[i] -= Data.vitality.expMax;
            return true;
        }

        // Collar y concha: Popolon no entra al mundo 7 sin el collar; Aphrodite, al 5 sin la concha.
        public string WorldEntryBlocked(int world, Character c)
        {
            foreach (var i in Data.items.Where(i => i.effect.kind == "worldEntry" && i.effect.world == world))
                if (string.Equals(i.effect.character, c.ToString(), StringComparison.OrdinalIgnoreCase) && !Has(i.type))
                    return $"{c} necesita {i.name} para entrar al mundo {world}";
            return null;
        }

        // Sin Hoi, en el mundo 8 izquierda y derecha van al revés.
        public bool ReversedControls => CurrentWorld == 8 && !Has(Hoi);

        // Gran llave: +8 de vitalidad máxima mientras sea menor de 72, y la rellena.
        public bool GreatKeyBoost(int[] vitality, int[] maxVitality, Character c)
        {
            int i = (int)c;
            var v = Data.vitality;
            if (maxVitality[i] >= v.maxVitalityLimit) return false;
            maxVitality[i] += v.maxVitalityStep;
            vitality[i] = maxVitality[i];
            return true;
        }

        // Biblia: congela a los enemigos 150 ticks, 16 veces en toda la partida.
        public bool UseBible()
        {
            var b = Data.Get(Bible);
            int max = b?.effect.maxUses > 0 ? b.effect.maxUses : 16;
            if (!Has(Bible) || BibleUses >= max) return false;
            BibleUses++;
            return true;
        }

        public int BibleTicks => Data.Get(Bible)?.effect.durationTicks > 0 ? Data.Get(Bible).effect.durationTicks : 150;

        // Ala: salas de la puerta de cada mundo en el castillo (1–9).
        public int WingRoom(int world)
        {
            var w = Data.Get(Wing);
            var rooms = w?.effect.doorRooms;
            return rooms != null && world >= 1 && world <= rooms.Length ? rooms[world - 1] : -1;
        }

        public WorldMap MapOf(int world) => Array.Find(Data.worldMaps, m => m.world == world);
    }
}
