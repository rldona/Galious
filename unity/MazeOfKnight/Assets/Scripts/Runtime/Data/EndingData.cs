using System;

namespace MazeOfKnight.Data
{
    // Final del juego (extracted/unity/ending.json, de tools/extract/ending.py; docs/ending.md). El importador lo copia a
    // Resources/Ending.json. Tiempos en frames de 60 Hz (el estado 17 corre una vez por frame).
    [Serializable]
    public class EndingScreenInfo
    {
        public string id;
        public int durationFrames;
        public int music;
        public string[] texts = Array.Empty<string>();
    }

    [Serializable]
    public class CastlePhase
    {
        public int phase, frames;
    }

    [Serializable]
    public class CastleScene
    {
        public CastlePhase[] phases = Array.Empty<CastlePhase>();
        public string[] balloons = { "NICE!", "GREAT!" };
    }

    [Serializable]
    public class EpilogueLine
    {
        public string kind;           // text, blank, wait, musicStop, end
        public int col, rows, steps;
        public string text;
    }

    [Serializable]
    public class Epilogue
    {
        public int stepFramesMeasured = 3, waitStepFramesMeasured = 2;
        public EpilogueLine[] lines = Array.Empty<EpilogueLine>();
    }

    [Serializable]
    public class EndingData
    {
        public EndingScreenInfo[] screens = Array.Empty<EndingScreenInfo>();
        public CastleScene castleScene = new CastleScene();
        public Epilogue epilogue = new Epilogue();

        public int Frames(string id, int fallback) => Array.Find(screens, s => s.id == id)?.durationFrames ?? fallback;
    }
}
