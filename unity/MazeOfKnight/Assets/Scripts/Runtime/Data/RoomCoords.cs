using UnityEngine;

namespace MazeOfKnight.Data
{
    // Conversión entre el original y Unity (Design Bible, cap. 11): 1 unidad = 16 px, tile de 8 px = 0,5 u.
    // Una sala mide 32 x 20 tiles (256 x 160 px) = 16 x 10 unidades. Origen de una sala: su esquina inferior
    // izquierda; las salas se colocan en la rejilla del mapa sin huecos.
    public static class RoomCoords
    {
        public const int Cols = 32;
        public const int Rows = 20;
        public const float PixelsPerUnit = 16f;
        public const float TileSize = 8f / PixelsPerUnit;
        public const int RoomTopPixel = 32;    // la sala empieza bajo el marcador

        public static readonly Vector2 RoomSize = new Vector2(Cols * TileSize, Rows * TileSize);

        // Celda de Tilemap (en tiles) de la columna/fila de una sala; la fila 0 es la de arriba.
        public static Vector3Int Cell(RoomModel room, int col, int row) =>
            new Vector3Int(room.gridX * Cols + col, room.gridY * Rows + (Rows - 1 - row), 0);

        public static Vector2 RoomOrigin(RoomModel room) =>
            new Vector2(room.gridX * RoomSize.x, room.gridY * RoomSize.y);

        // Píxel del original (x, y de pantalla) a posición de mundo dentro de la sala.
        public static Vector2 PixelToWorld(RoomModel room, int x, int y) =>
            RoomOrigin(room) + new Vector2(x / PixelsPerUnit, (Rows * 8 - (y - RoomTopPixel)) / PixelsPerUnit);
    }
}
