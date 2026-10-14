namespace MazeOfKnight.Game
{
    // Cambios del remake respecto al original (Design Bible, cap. 11). Cada uno se puede desactivar para
    // volver al comportamiento medido en openMSX.
    public static class RemakeRules
    {
        // Original: a media escalera, ← y → no hacen nada (medido en el castillo, sala 1). Remake: si hay sitio a
        // ese lado, el jugador se suelta y cae con control en el aire.
        public static bool LeaveLadderSideways = false;   // desactivado: se juega como el original

        // Original: la piedra del bebé (mundo 10, sala 6) sale de (0..3) + (0..7), así que 1 de cada 32 veces vale 10 y
        // ninguna de las 10 piedras lo tiene: solo se sale perdiendo una vida (docs/ending.md). Remake: una de las 10 al
        // azar, siempre.
        public static bool BabyAlwaysPresent = false;     // desactivado: se juega como el original

        // Original: al cambiar de sala el juego se para 11 ticks (~0,37 s) mientras carga la sala nueva (medido con la
        // demo 1). Remake: se sigue sin pausa. Las demos la llevan siempre.
        public static bool NoRoomChangePause = true;      // activado: el usuario prefiere no tener el parón
    }
}
