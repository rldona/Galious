using System;

namespace MazeOfHollow.Data
{
    // Formato del paquete que escribe tools/extract/unity_bundle.py (extracted/unity/<mapa>.json).
    // Las mismas clases se guardan dentro de los ScriptableObjects, así que no hay conversión intermedia.
    // Coordenadas x/y en píxeles del original: origen arriba a la izquierda de la pantalla del MSX, y la sala
    // empieza en y = 32 (bajo el marcador). Ver RoomCoords para pasarlas a unidades de Unity.

    [Serializable]
    public class PointJson
    {
        public int x;
        public int y;
    }

    [Serializable]
    public class EntityJson
    {
        public int x;
        public int y;
        public int type;
    }

    [Serializable]
    public class LadderJson
    {
        public int x;
        public int y;
        public int height;
    }

    [Serializable]
    public class RoomModel
    {
        public int id;
        public bool placed;     // false: sala sin enlaces, colocada en una columna aparte de la rejilla
        public bool shrine;     // última sala de un mundo: sala de dios (estado 14), sin disposición propia
        public int gridX;
        public int gridY;       // crece hacia arriba
        public int up = -1;     // -1 sin salida; 0xF5-0xFE salida al castillo
        public int down = -1;
        public int left = -1;
        public int right = -1;
        public string[] collision = Array.Empty<string>();      // 20 filas de 32 caracteres, fila 0 arriba
        public string[] collisionBoss = Array.Empty<string>();  // solo en la sala del jefe, durante el combate
        public EntityJson[] spawns = Array.Empty<EntityJson>();
        public EntityJson[] objects = Array.Empty<EntityJson>();
        public bool hasItem;
        public EntityJson item;
        public PointJson[] rocks = Array.Empty<PointJson>();
        public LadderJson[] ladders = Array.Empty<LadderJson>();
        public int doorWorld;   // castillo: puerta al mundo n (0 = ninguna)
        public PointJson door;
        public int godType;     // castillo: sala de dios (E690), 0 = ninguna
        public string godName;
        public PointJson god;
    }

    [Serializable]
    public class MapBundle
    {
        public string map;
        public int e041;
        public int bossRoom;
        public string bossWord;
        public RoomModel[] rooms = Array.Empty<RoomModel>();
    }
}
