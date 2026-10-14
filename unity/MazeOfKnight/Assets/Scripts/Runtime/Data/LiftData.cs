using System;

namespace MazeOfKnight.Data
{
    // Ascensores del castillo (extracted/unity/lifts.json, de tools/extract/lifts.py; docs/entities.md, "Ascensores").
    // El importador lo copia a Resources/Lifts.json. Posiciones en píxeles del original; velocidades en 1/256 px por tick.
    [Serializable]
    public class ShaftInfo
    {
        public int id;
        public int[] rooms = Array.Empty<int>();   // de arriba abajo
        public int startRoom, startIndex, y, x, speed;
        public int topRoom, topY, bottomRoom, bottomY;
    }

    [Serializable]
    public class SlideInfo
    {
        public int room, y, x, speed, minX, maxX;
    }

    [Serializable]
    public class ShaftPreset
    {
        public int id, room, index, y, speed;
        public bool up;
    }

    [Serializable]
    public class LiftBox
    {
        public int width = 32, height = 16;
        public int standDxMin = -2, standDxMax = 34, standDyMin = 0, standDyMax = 7;
        public int sideDyMin = 1, sideDyMax = 31;
        public int pushLeftX = -8, pushLeftFrom = -8, pushLeftTo = -5;
        public int pushRightX = 40, pushRightFrom = 37, pushRightTo = 40;
        public int headDyMin = 24, headDyMax = 31, headPushY = 32;
        public int leaveCooldownTicks = 7;
        public int roomTopY = 0x28, roomBottomY = 0xC0, enterFromBelowY = 0xB9, enterFromAboveY = 0x28;
    }

    [Serializable]
    public class LiftsData
    {
        public ShaftInfo[] shafts = Array.Empty<ShaftInfo>();
        public SlideInfo[] slides = Array.Empty<SlideInfo>();
        public ShaftPreset demo0 = new ShaftPreset();
        public LiftBox box = new LiftBox();
    }
}
