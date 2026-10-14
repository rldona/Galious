using System;

namespace MazeOfKnight.Data
{
    // Armas secundarias (extracted/unity/weapons.json, de tools/extract/weapons.py; docs/player.md, "Armas
    // secundarias"). El importador lo copia a Resources/Weapons.json. Píxeles y ticks del original.
    [Serializable]
    public class WeaponBox
    {
        public int dy, dx, h, w;     // bordes incluidos: cubre (h + 1) × (w + 1) px
        public bool alignTo8;        // sobre (y, x) redondeados a 8 (las flechas)
    }

    [Serializable]
    public class WeaponInfo
    {
        public int id;               // 1 flecha, 2 cerámica, 3 fuego rodante, 4 fuego, 5 mina, 6 lupa
        public string name;
        public int cost;             // flechas por disparo
        public int damage, saberBonus;
        public int rockDamagePopolon, rockDamageAphrodite;
        public bool piercing;
        public int rehitPeriodTicks; // la cerámica vuelve a dañar cada 16; la mina, cada 32
        public string pool;          // projectile (huecos E540…) o mine (E5A0…)
        public int maxPopolon, maxAphrodite, maxItem, maxPopolonWithItem;
        public bool air, ladder, ground;
        public int spawnDxRight, spawnDxLeft, spawnDy;
        public bool spawnAlignY8;
        public int spawnMinY;
        public bool firstTickMoves;
        public int speedX, fallSpeed;
        public string motion;        // straight, wrapPlatform, floorFall, mine, none
        public WeaponBox hitbox = new WeaponBox();
    }

    [Serializable]
    public class MineInfo
    {
        public int armDy = 21, armDxMin = -8, armDxMax = 25;
        public int[] needsFloorAt = { 2, 15 };
        public int explosionTicks = 70, explosionShiftY = -12, explosionShiftX = -8;
        public WeaponBox explosionHitbox = new WeaponBox { dy = -16, h = 32, w = 32 };
        public int playerDamage = 2;
        public WeaponBox playerBox = new WeaponBox { dy = -16, dx = -8, h = 16, w = 16 };
    }

    [Serializable]
    public class Tombstone
    {
        public int world, e041, room, kind, x, y;
    }

    [Serializable]
    public class MagnifierInfo
    {
        public int dyMin = -16, dyMax = 16, playerDy = -16, dxMin = -16, dxMax = 16, playerDx = -8;
        public Tombstone[] tombstones = Array.Empty<Tombstone>();
    }

    [Serializable]
    public class WeaponsData
    {
        public int killMinX = 8, killMaxX = 248, killMaxY = 184;
        public WeaponInfo[] weapons = Array.Empty<WeaponInfo>();
        public MineInfo mine = new MineInfo();
        public MagnifierInfo magnifier = new MagnifierInfo();

        public WeaponInfo Get(int id) => Array.Find(weapons, w => w.id == id);
    }
}
