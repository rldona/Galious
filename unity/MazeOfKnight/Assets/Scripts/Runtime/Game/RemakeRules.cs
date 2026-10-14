namespace MazeOfKnight.Game
{
    // Cambios del remake respecto al original (Design Bible, cap. 11). Cada uno se puede desactivar para
    // volver al comportamiento medido en openMSX.
    public static class RemakeRules
    {
        // Original: a media escalera, ← y → no hacen nada (medido en el castillo, sala 1). Remake: si hay sitio a
        // ese lado, el jugador se suelta y cae con control en el aire.
        public static bool LeaveLadderSideways = true;
    }
}
