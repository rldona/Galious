using System;

namespace MazeOfKnight.Data
{
    // Jefes (extracted/unity/bosses.json, de tools/extract/bosses.py; docs/boss_ai.md: "Invocación", "Derrota",
    // "Cajas de los jefes" e "Hijos de los jefes"). El importador lo copia a Resources/Bosses.json.
    [Serializable]
    public class BossBox
    {
        public bool valid;
        public int dy, dx, h, w;      // desde (E705, E707); bordes incluidos: cubre (h + 1) × (w + 1) px
    }

    [Serializable]
    public class BossSprite
    {
        public int id, w, h;          // tamaño del dibujo en px
        public string part;           // body, head…
        public BossBox weaponBox = new BossBox(), weaponBox2 = new BossBox(), weaponBox3 = new BossBox();
        public BossBox contactBox = new BossBox(), contactBox2 = new BossBox();
        public int knockbackRefDx = 0x20;
    }

    [Serializable]
    public class BossHitTest
    {
        public string kind;           // box, threeBoxes, minSprite, cross, head, elusive, none…
        public int minSprite;
    }

    [Serializable]
    public class BossPoint
    {
        public int x, y;
    }

    [Serializable]
    public class BossInfo
    {
        public int id, world;
        public string name, word;
        public int room, hp, hpHolyWater, weaponMask;
        public BossHitTest hitTest = new BossHitTest();
        public int contactDamage, contactDamageCloak;
        public BossPoint init = new BossPoint();
        public BossSprite[] sprites = Array.Empty<BossSprite>();
        public bool dropsKey = true;

        public BossSprite Sprite(int id) => Array.Find(sprites, s => s.id == id) ?? (sprites.Length > 0 ? sprites[0] : null);
    }

    [Serializable]
    public class KeyPosition
    {
        public int world, x, y;
    }

    [Serializable]
    public class BossSummon
    {
        public int countdownTicks = 65, sparkEveryTicks = 8, sparkLifeTicks = 80, maxSparks = 4;
        public BossPoint[] sparkPositions = Array.Empty<BossPoint>();
    }

    [Serializable]
    public class BossInvocation
    {
        public int bufferLength = 16, totalFrames = 193;
    }

    [Serializable]
    public class BossDefeat
    {
        public int dissolveFrames = 128;
        public KeyPosition[] keyPositions = Array.Empty<KeyPosition>();
    }

    [Serializable]
    public class BossFanfare
    {
        public int totalFrames = 480, messageFrames = 120;
        public string message = " I HAVE TAKEN WORLD n BACK !";
    }

    [Serializable]
    public class BossesData
    {
        public BossInfo[] bosses = Array.Empty<BossInfo>();
        public BossInvocation invocation = new BossInvocation();
        public BossSummon summon = new BossSummon();
        public BossDefeat defeat = new BossDefeat();
        public BossFanfare keyFanfare = new BossFanfare();

        public BossInfo ForWorld(int world) => Array.Find(bosses, b => b.world == world);
    }
}
