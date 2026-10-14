using System;

namespace MazeOfKnight.Data
{
    // Objetos e inventario (extracted/unity/items.json, de tools/extract/items_export.py; docs/items_enemies.md,
    // "Objetos recogibles", "Inventario (menú F1)" y "Vitalidad, experiencia y vidas"). El importador lo copia a
    // Resources/Items.json.
    [Serializable]
    public class ItemEffect
    {
        public string kind;
        public int[] enemyTypes = Array.Empty<int>();   // contactImmunity
        public int[] blocks = Array.Empty<int>();       // shield: proyectiles que para
        public int level;                               // shield
        public int world;                               // worldEntry, walkOnWater, bridge, noReversedControls…
        public int room;
        public int[] rooms = Array.Empty<int>();
        public string character;                        // popolon / aphrodite
        public int value, multiplier;
        public int durationTicks, maxUses;              // Biblia
        public int maxVitalityStep, maxVitalityLimit;   // Gran llave
        public float walkSpeedWith, ladderSpeedWith;    // Zapatos
        public string word;                             // Daga: UMBRELLA
        public int enemyType;
        public int[] doorRooms = Array.Empty<int>();    // Ala
        public int waitTicks;
    }

    [Serializable]
    public class ItemMenuSlot
    {
        public string area;      // items, world, weapons o none
        public int slot, x, y, w, h;
    }

    [Serializable]
    public class ItemInfo
    {
        public int type;
        public string name;
        public string category;
        public bool perWorld;    // 0x07–0x0A: uno por mundo, solo valen dentro de él
        public ItemEffect effect = new ItemEffect();
        public ItemMenuSlot menu = new ItemMenuSlot();
    }

    [Serializable]
    public class VitalityRules
    {
        public int expMax = 48;
        public int vitStart = 8, vitMaxStart = 8;
        public int maxVitalityStep = 8, maxVitalityLimit = 72;
        public int lowVitalityBelow = 6;
    }

    [Serializable]
    public class MapCell
    {
        public int room, col, row;
    }

    [Serializable]
    public class WorldMap
    {
        public int world, x, y, widthTiles, heightTiles;
        public int bossRoom, bossCol, bossRow;
        public MapCell[] roomCells = Array.Empty<MapCell>();
    }

    [Serializable]
    public class ItemsData
    {
        public ItemInfo[] items = Array.Empty<ItemInfo>();
        public VitalityRules vitality = new VitalityRules();
        public WorldMap[] worldMaps = Array.Empty<WorldMap>();

        public ItemInfo Get(int type) => Array.Find(items, i => i.type == type);
    }
}
