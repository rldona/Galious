using System;

namespace MazeOfKnight.Data
{
    // Ciclo de presentación (extracted/unity/attract.json, de tools/extract/attract.py; docs/attract.md): logo, título,
    // demo o historia, y el inicio de partida. Tiempos en frames de 60 Hz salvo 'frames' de las entradas de la demo,
    // que son ticks del juego. El importador lo copia a Resources/Attract.json.
    [Serializable]
    public class AttractScreenInfo
    {
        public string id;
        public int durationFrames;
    }

    [Serializable]
    public class DemoInputEntry
    {
        public int frames;           // ticks que dura (contando el tick en que se carga)
        public int keys;             // FF = fin de la demo
        public bool left, right, up, down, space, m;
    }

    [Serializable]
    public class DemoInfo
    {
        public int index;
        public string map;
        public int room, x, y;
        public DemoInputEntry[] inputs = Array.Empty<DemoInputEntry>();
        public int durationTicks;
    }

    [Serializable]
    public class AttractStory
    {
        public int stepFramesMeasured = 3, waitFramesMeasured = 513;
    }

    [Serializable]
    public class AttractData
    {
        public AttractScreenInfo[] screens = Array.Empty<AttractScreenInfo>();
        public EpilogueLine[] storyLines = Array.Empty<EpilogueLine>();
        public AttractStory story = new AttractStory();
        public DemoInfo[] demos = Array.Empty<DemoInfo>();

        public int Frames(string id, int fallback) => Array.Find(screens, s => s.id == id)?.durationFrames ?? fallback;
    }
}
