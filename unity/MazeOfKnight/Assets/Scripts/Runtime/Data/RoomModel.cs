using System;

namespace MazeOfKnight.Data
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
        public int height;      // en tiles
        public int mode;        // se retira al subirla (1), al bajarla (2) o en los dos casos (3)
    }

    // Generador periódico de la sala (docs/entities.md, "Generadores periódicos por sala"; tools/extract/periodic.py).
    // Tiempos en ticks. Con periodRandom cada espera, la primera incluida, es aleatoria en [min, max]. Con batch,
    // tandas de 'batch' intentos separados 'period' y 'batchGap' entre tandas. x = -1: la decide el tipo.
    [Serializable]
    public class PeriodicJson
    {
        public string gen;
        public int type;
        public int firstDelay;
        public int period;
        public int[] periodRandom = Array.Empty<int>();
        public int batch;
        public int batchGap;
        public int maxTotal;           // 0 = sin fin
        public int x = -1;
        public int y;
        public int[] xSequence = Array.Empty<int>();
        public int[] randomX = Array.Empty<int>();   // pares [desde, hasta]
        public string xRule;           // sequence, random o leap
        public int[] xClamp = Array.Empty<int>();
        public int param;
        public bool pauseInLiquid;
        public int disabledByItem;     // no se arma si al entrar se tiene este objeto (0x18, Carpet)
    }

    // Eventos de sala (docs/entities.md, "Eventos de sala"; tools/extract/unity_bundle.py, room_events).
    [Serializable]
    public class RoomEventJson
    {
        public string kind;      // ladder, wall o castleItem
        public int room;
    }

    [Serializable]
    public class EventLadderJson
    {
        public int row, col, height;   // 2 tiles de ancho, de la fila hacia abajo; height 0 = no hay
        public int openedBy;           // sala cuyo evento la hace aparecer
    }

    [Serializable]
    public class WallBlockJson
    {
        public int row, col;           // bloque de 4 × 3 tiles
        public int openedBy;           // sala cuyo evento lo abre; -1 = subir 5 veces por el pozo; 0 = no hay
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
        public string[] collisionNoLadders = Array.Empty<string>(); // sin las escaleras extra (las que se retiran)
        public EntityJson[] spawns = Array.Empty<EntityJson>();
        public EntityJson[] objects = Array.Empty<EntityJson>();
        public bool hasItem;
        public EntityJson item;
        public PointJson[] rocks = Array.Empty<PointJson>();
        public LadderJson[] ladders = Array.Empty<LadderJson>();
        public PeriodicJson[] periodic = Array.Empty<PeriodicJson>();
        public int afterBossRight = -1;   // mundo 10, sala 5: tras vencer a Galious la salida derecha lleva aquí (sala 6)
        public bool markedRoom;        // matar a todos sus enemigos marcados dispara un evento
        public RoomEventJson onClear = new RoomEventJson();
        public EventLadderJson eventLadder = new EventLadderJson();
        public WallBlockJson wallBlock = new WallBlockJson();
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
