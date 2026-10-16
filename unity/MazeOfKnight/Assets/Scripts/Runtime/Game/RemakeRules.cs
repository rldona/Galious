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

        // Sensación de Hollow Knight (guía "Hollow Knight por dentro", Knight y Combate). F11 activa o quita las tres a la vez.
        //
        // Original: al acertar un golpe el juego sigue sin pararse. Remake: todo el juego se para HitStopTicks ticks
        // (Hollow Knight: ~0,11 s) para que el golpe pese. No cambia lo que pasa, solo cuándo se ve.
        public static bool HitStop = false;
        public const int HitStopTicks = 3;                  // 0,1 s
        // Original: al salir andando de un borde, 4 ticks quieto en el aire sin poder hacer nada. Remake: en los primeros
        // CoyoteTicks de esos 4, una pulsación de ↑ salta como desde el suelo (Hollow Knight: 0,04 s de "coyote time").
        public static bool CoyoteJump = false;
        public const int CoyoteTicks = 2;
        // Original: ↑ pulsado en el aire no hace nada al aterrizar (hace falta otra pulsación en el suelo). Remake: una
        // pulsación en los últimos JumpBufferTicks ticks en el aire salta al tocar el suelo si ↑ sigue pulsado
        // (Hollow Knight: búfer de salto de 0,04 s).
        public static bool JumpBuffer = false;
        public const int JumpBufferTicks = 2;

        public static bool HollowFeel
        {
            get => HitStop && CoyoteJump && JumpBuffer;
            set => HitStop = CoyoteJump = JumpBuffer = value;
        }
    }
}
