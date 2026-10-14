namespace MazeOfKnight.Game
{
    // Dónde aparece el jugador al entrar en cada mundo por su puerta: sala 1, (x, y). Medido en openMSX
    // (docs/world_maps.md, "Entrar y salir de los mundos"; tools/openmsx/world_doors.py). El mundo 10 tiene
    // una puerta que cambia de sala en cada partida.
    public static class OriginalWorlds
    {
        public static readonly (int x, int y)[] Entry =
        {
            (0, 0),        // sin mundo 0
            (128, 56),     // 1
            (128, 152),    // 2
            (128, 152),    // 3
            (32, 184),     // 4
            (224, 120),    // 5
            (128, 56),     // 6
            (136, 184),    // 7 (medido con Aphrodite: Popolon necesita el Necklace)
            (128, 56),     // 8
            (32, 56),      // 9
            (24, 56),      // 10 [O*] (estado guardado world10_entry)
        };

        // Puerta del mundo 10 (b02:99B3): sala y posición según E06E = R & 3, elegido al empezar la partida; aparece
        // cuando E06C & 3 ≠ 0, es decir, tras coger la Gran Llave del mundo 9 (docs/world_maps.md).
        public static readonly int[] World10DoorRoom = { 9, 30, 39, 1 };
        public static readonly (int x, int y)[] World10DoorPos = { (0x40, 0xA0), (0xA0, 0x20), (0x60, 0xA0), (0xA0, 0x60) };

        public static bool HasEntry(int world) => world >= 1 && world < Entry.Length;

        public static string MapName(int world) => $"world{world:00}";

        // Al salir se vuelve delante de la puerta del mundo: x = X_puerta + 16, y = Y_puerta + 0x18.
        public const int DoorExitDx = 16;
        public const int DoorExitDy = 0x18;
    }
}
