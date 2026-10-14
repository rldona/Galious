namespace MazeOfHollow.Data
{
    // Clases de colisión del original (docs/world_maps.md, "Colisión de los tiles"):
    // sólido = tile >= 0xBA (>= 0xF0 durante el combate con un jefe), escalera 9A/9B, agua B6/B7, lava B8/B9.
    public enum TileClass : byte
    {
        Empty = 0,
        Solid = 1,
        Ladder = 2,
        Water = 3,
        Lava = 4,
        BossPassable = 5,   // sólido fuera del combate y atravesable durante él (tiles BA-EF)
    }

    public static class TileClasses
    {
        // Letras de la cuadrícula de collision.py.
        public static TileClass FromChar(char c)
        {
            switch (c)
            {
                case '#': return TileClass.Solid;
                case 'H': return TileClass.Ladder;
                case '~': return TileClass.Water;
                case 'L': return TileClass.Lava;
                case 'j': return TileClass.BossPassable;
                default: return TileClass.Empty;
            }
        }
    }
}
