namespace MazeOfKnight.Game
{
    // Números del movimiento del original, medidos frame a frame en openMSX (docs/player.md,
    // "Física del movimiento"). Todo en píxeles por tick; la lógica del original avanza a 30 ticks/s.
    public static class OriginalPhysics
    {
        public const float TicksPerSecond = 30f;

        public const int WalkSpeed = 2;
        public const int AirSpeedRising = 2;
        public const int AirSpeedFalling = 3;      // desde el 2.º tick en el punto más alto hasta aterrizar
        public const int LadderSpeed = 1;

        public const int JumpPrepTicks = 2;        // el tick en que se pulsa ↑ cuenta; sube en el 3.º
        public static readonly int[] RiseTable = { 6, 6, 6, 6, 6, 5, 5, 4, 4, 3, 2, 1 };
        public const int RiseStartPopolon = 0;     // 54 px
        public const int RiseStartAphrodite = 2;   // 42 px
        public const int ApexTicks = 4;
        public const int ApexTicksAtRiseSpeed = 1; // el primer tick arriba aún va a 2 px/tick
        public static readonly int[] FallTable = { 1, 2, 3, 4, 4, 5, 5, 6 };  // y luego 6
        public const int LedgeFallStart = 2;       // caer por un borde empieza en el "3"

        // Caja del jugador: los pies en y (borde superior de la fila en la que está de pie),
        // de x - 8 a x + 7 en horizontal y hasta 13 px por encima de los pies.
        public const int FootLeft = -8;
        public const int FootRight = 7;
        public const int HeadHeight = 13;         // sala 97: choca en y = 110 y aún sube 1 px

        // Espada (docs/player.md, "Espada"): 4 ticks, alcance hasta x + 18 delante del centro. La franja vertical
        // no se ha medido: se supone a la altura del cuerpo [I].
        public const int SwordTicks = 4;
        public const int SwordFrom = 8;
        public const int SwordReach = 18;
        public const int SwordTop = 12;
        public const int SwordBottom = 3;
        public const int RockHp = 15;
        public const int RockDamagePopolon = 5;
        public const int RockDamageAphrodite = 1;

        // Agua y lava (docs/player.md, "Agua y lava"): dentro del líquido el movimiento vertical va a 1/4 (las
        // tablas de salto y caída avanzan una entrada cada 4 ticks), el horizontal a la mitad, y no se baja de
        // y = 184. Daño: 1 cada 4 ticks (Popolon en el agua; los dos en la lava), Aphrodite en el agua 1 cada 255.
        public const int LiquidTimeScale = 4;
        public const int LiquidFloorY = 184;
        public const int LiquidDamageTicks = 4;
        public const int AphroditeWaterDamageTicks = 255;
        public const int StartVitality = 8;          // unidades (la barra mide 4 px por unidad)

        // Puertas con llave (clases 1 y 2): empujar 30 ticks, 2 pasos de 6 ticks abriendo y 12 ticks de arrastre
        // a 2 px/tick hasta el otro lado.
        public const int KeyDoorPushTicks = 30;
        public const int KeyDoorOpenTicks = 12;
        public const int KeyDoorCarryTicks = 12;
        public const int KeyDoorCarrySpeed = 2;

        public const int RoomWidthPx = 256;
        public const int RoomTopPx = 32;           // la sala empieza bajo el marcador
        public const int RoomBottomPx = RoomTopPx + 160;
    }
}
