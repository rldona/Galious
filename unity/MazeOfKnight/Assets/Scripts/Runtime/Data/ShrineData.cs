using System;

namespace MazeOfKnight.Data
{
    // Santuarios y tiendas (extracted/unity/shrines.json, de tools/extract/shrines.py; docs/design_bible/08_santuarios.md
    // y docs/entities.md, "Santuarios y tiendas"). El importador lo copia a Resources/Shrines.json.
    [Serializable]
    public class ShrineEntry
    {
        public string how;                    // down (castillo, estado 16) o ladderUp (mundo, estado 14)
        public int godX, godY, xMin, xMax, yMin, yMax, needsItem;
        public bool rockOnTop;
        public int fromRoom, ladderCol, x, exitBelowY;
        public bool hiddenLadder;
    }

    [Serializable]
    public class ShrineExit
    {
        public int toRoom, x, y;
    }

    [Serializable]
    public class ShrineItem
    {
        public int slot, type, price, amount;
        public string name;
        public string rule;                   // flag, quiver, key o shield
        public int needsShield, setsShield;
    }

    [Serializable]
    public class Shrine
    {
        public string map;
        public int room, type, state;
        public string god, kind;              // shop, password, resurrection, hint
        public ShrineEntry entry = new ShrineEntry();
        public ShrineExit exit = new ShrineExit();
        public ShrineItem[] items = Array.Empty<ShrineItem>();
        public int[] messageIds = Array.Empty<int>();
        public string[] messages = Array.Empty<string>();
    }

    [Serializable]
    public class ShrineRules
    {
        public int quiverArrows = 100, keyAmount = 1, counterCap = 999;
        public int resurrectionArrows = 100, resurrectionCoins = 100, resurrectionKeys = 20;
    }

    [Serializable]
    public class ShrineMessage
    {
        public int id;
        public string text;
    }

    [Serializable]
    public class ShrinesData
    {
        public ShrineRules rules = new ShrineRules();
        public ShrineMessage[] messages = Array.Empty<ShrineMessage>();
        public Shrine[] shrines = Array.Empty<Shrine>();

        public string Message(int id) => Array.Find(messages, m => m.id == id)?.text ?? "";
    }
}
