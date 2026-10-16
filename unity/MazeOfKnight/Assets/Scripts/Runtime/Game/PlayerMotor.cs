using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game
{
    public enum Character { Aphrodite = 0, Popolon = 1 }

    public enum MotorState { Ground, JumpPrep, Jump, Fall, Ladder, Carried, Knockback }

    public struct MotorInput
    {
        public bool Left, Right, Up, Down, Attack;
        // Pulsaciones nuevas ocurridas desde el tick anterior, aunque entre medias se soltara la tecla (la entrada se lee
        // en cada fotograma de Unity y el motor va a 30 ticks/s: sin esto se pierde el que suelta y vuelve a pulsar
        // entre dos ticks).
        public bool UpPressed, DownPressed;
    }

    // Movimiento del jugador tick a tick, con los números del original (docs/player.md). Clase sin
    // MonoBehaviour para poder probarla contra las curvas medidas en openMSX (Tests/EditMode).
    // Posición en píxeles del original dentro de la sala: x = centro, y = pies (fila r: y = 8r + 32).
    public class PlayerMotor
    {
        RoomWorld world;

        public RoomModel Room { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public Character Character = Character.Popolon;
        public MotorState State { get; private set; } = MotorState.Ground;
        public bool FacingLeft { get; private set; }

        // Salto y caída
        int prepTicks;
        int riseIndex;          // entrada de RiseTable; RiseTable.Length = ya en el punto más alto
        int apexTicks;
        bool cutDone;           // ya se cortó la subida (soltar ↑ o techo)
        int fallIndex;
        bool airControl;        // false al caer por un borde

        // Líquidos: 1/4 de velocidad vertical y 1/2 horizontal, con fracciones en 1/256 de píxel.
        public TileClass Liquid { get; private set; }
        public bool InLiquid => Liquid == TileClass.Water || Liquid == TileClass.Lava;
        // RemakeRules.HollowSwim: en el agua, flota en la superficie.
        bool Swim => RemakeRules.HollowSwim && Liquid == TileClass.Water;
        // Nadando: flotando en la superficie (para el dibujo).
        public bool Swimming => Swim && State == MotorState.Ground && Y == FloatY();
        // Lo que va 4 veces más lento en el líquido (con HollowSwim, el agua ya no frena saltos ni caídas).
        bool SlowLiquid => InLiquid && !Swim;

        // Altura de los pies flotando: la superficie del agua de la columna del jugador más SwimDepth.
        int FloatY()
        {
            int py = world.TileAt(Room, X, Y) == TileClass.Water ? Y : Y - 1;
            while (py - 8 >= P.RoomTopPx && world.TileAt(Room, X, py - 8) == TileClass.Water) py -= 8;
            int surface = P.RoomTopPx + ((py - P.RoomTopPx) >> 3 << 3);
            return surface + RemakeRules.SwimDepth;
        }
        int vAcc, hAcc, liquidSub;

        // Retroceso tras un golpe (E500 = 4).
        int knockDir, knockStep;

        // Arrastrado por una puerta: espera y luego avanza sin colisión.
        int carryWait, carryTicks, carryDir, carrySpeed;

        public MotorInput LastInput { get; private set; }

        // Espada
        public int AttackTicks { get; private set; }
        public bool AttackStartedThisTick { get; private set; }
        bool attackHeld;
        bool upHeld, downHeld;   // saltar, engancharse a una escalera y entrar por una puerta piden pulsar de nuevo

        public event System.Action<RoomModel, RoomModel> RoomChanged;

        // Ascensores del castillo (Lifts.cs): después del jugador se mira la colisión y luego se mueven y lo llevan. Encima
        // de uno cuenta como suelo (b03:A8CE).
        public LiftSystem Lifts;
        public bool OnLift => Lifts != null && Lifts.Riding(Room.id);
        // E52B/E52D: el salto empezó sobre ese ascensor vertical; con él se puede salir por arriba de la sala saltando.
        public int JumpedFromShaft;
        public event System.Action<int> DoorEntered;        // mundo
        public event System.Action<int> CastleExitReached;  // código de salida 0xF5-0xFE

        public PlayerMotor(RoomWorld world, RoomModel room, int x, int y)
        {
            this.world = world;
            Place(room, x, y);
        }

        // Cambia de mapa (entrar o salir de un mundo).
        public void Place(RoomWorld newWorld, RoomModel room, int x, int y)
        {
            world = newWorld;
            Place(room, x, y);
        }

        public void Place(RoomModel room, int x, int y)
        {
            AttackTicks = 0;
            vAcc = hAcc = liquidSub = 0;
            carryTicks = carryWait = 0;
            Room = room;
            X = x;
            Y = y;
            State = MotorState.Ground;
            landY = -1;
            jumpDir = 0;
            hoverTicks = 0;
            JumpedFromShaft = 0;
            hitWall = false;
        }

        // El vertical cambia de sala con el jugador encima (b03:A7F8): entra con él, a su altura.
        public void RideToRoom(int roomId, int y)
        {
            var next = world.Room(roomId);
            if (next == null) return;
            var previous = Room;
            Room = next;
            Y = y;
            RoomChanged?.Invoke(previous, next);
        }

        // b03:A916: encima de un ascensor, los pies a su altura y, en el horizontal, lo que se ha movido.
        public void CarryOnLift(int dx, int y)
        {
            X += dx;
            Y = y;
        }

        // b03:B0C2: el ascensor lo aparta por un lado o lo empuja por debajo.
        public void PushByLift(int x, int y)
        {
            X = x;
            Y = y;
        }

        // E519 bit 3: suelo de tiles bajo los pies.
        public bool OnTileFloor => ((Y - P.RoomTopPx) & 7) == 0 && SolidUnderFeet();

        // La puerta arrastra al jugador: wait ticks quieto y luego ticks a speed px/tick en dir.
        // Colocarlo agarrado a una escalera (al salir del santuario de un mundo, encima de la escalera de la sala de abajo).
        public void PlaceOnLadder(RoomModel room, int x, int y)
        {
            Place(room, x, y);
            State = MotorState.Ladder;
        }

        public void Carry(int dir, int wait, int ticks, int speed)
        {
            State = MotorState.Carried;
            carryDir = dir;
            carryWait = wait;
            carryTicks = ticks;
            carrySpeed = speed;
            AttackTicks = 0;
        }

        // Golpe de un enemigo: sale despedido alejándose de él (dir = -1 izquierda, 1 derecha).
        public void Knockback(int dir)
        {
            State = MotorState.Knockback;
            knockDir = dir;
            knockStep = 0;
            fallIndex = P.LedgeFallStart;
            AttackTicks = 0;
        }

        // Paralizado (E500 = 6, E50F ticks [I]): lo deja así el disparo 0x3F del soldado; no responde a los mandos.
        public int Stunned;

        // Zapatos (0x11): andar 2,5 px/tick (2 y 3 alternos) y escaleras 1,25 (un paso más cada 4 ticks) [C][O].
        public bool Shoes;
        bool shoeWalk;
        int shoeLadder;

        // Copia del estado para las búsquedas de recorrido (Walkthrough): sin suscriptores ni ascensores (los pone quien
        // copia).
        public PlayerMotor Clone()
        {
            var c = (PlayerMotor)MemberwiseClone();
            c.RoomChanged = null;
            c.DoorEntered = null;
            c.CastleExitReached = null;
            c.Lifts = null;
            return c;
        }

        // Todo lo que decide los ticks siguientes, empaquetado (sin LastInput, la espada ni el retroceso: la búsqueda no
        // ataca ni recibe golpes). Los contadores que solo cuentan en módulo se reducen.
        public (ulong, ulong) StateKey()
        {
            ulong a = (ulong)(Room.id & 511)
                      | (ulong)((X + 64) & 1023) << 9
                      | (ulong)((Y + 64) & 511) << 19
                      | (ulong)Character << 28
                      | (ulong)State << 29
                      | (FacingLeft ? 1UL : 0) << 32
                      | (ulong)(prepTicks & 3) << 33
                      | (ulong)(riseIndex & 15) << 35
                      | (ulong)(apexTicks & 7) << 39
                      | (cutDone ? 1UL : 0) << 42
                      | (ulong)System.Math.Min(fallIndex, P.FallTable.Length) << 43
                      | (airControl ? 1UL : 0) << 47
                      | (ulong)Liquid << 48
                      | (ulong)(liquidSub & 3) << 51
                      | (ulong)(jumpDir + 1) << 53
                      | (hitWall ? 1UL : 0) << 55
                      | (upHeld ? 1UL : 0) << 56
                      | (downHeld ? 1UL : 0) << 57
                      | (ulong)(hoverTicks & 7) << 58
                      | (Shoes ? 1UL : 0) << 61
                      | (shoeWalk ? 1UL : 0) << 62;
            ulong b = (ulong)(vAcc & 255)
                      | (ulong)(hAcc & 255) << 8
                      | (ulong)(carryWait & 31) << 16
                      | (ulong)(carryTicks & 31) << 21
                      | (ulong)(carryDir + 1) << 26
                      | (ulong)(carrySpeed & 3) << 28
                      | (ulong)((landY + 1) & 511) << 30
                      | (ulong)(shoeLadder & 3) << 39
                      | (ulong)(JumpedFromShaft & 7) << 41
                      | (ulong)(Stunned & 127) << 44
                      | (ulong)(coyoteLeft & 3) << 51
                      | (ulong)(jumpBuffer & 3) << 53;
            return (a, b);
        }

        public void Tick(MotorInput input)
        {
            TickPlayer(input);
            Lifts?.Collide(this);
            Lifts?.Move(this);
            if (State == MotorState.Ground || State == MotorState.Ladder)
                JumpedFromShaft = 0;     // b03:A3D8: al aterrizar
        }

        void TickPlayer(MotorInput input)
        {
            if (Stunned > 0)
            {
                Stunned--;
                input = default;
            }
            bool upPressed = input.Up && (!upHeld || input.UpPressed), downPressed = input.Down && (!downHeld || input.DownPressed);
            upHeld = input.Up;
            downHeld = input.Down;
            LastInput = input;
            // RemakeRules.JumpBuffer: una pulsación de ↑ en el aire se recuerda unos ticks.
            bool airborne = State == MotorState.Jump || State == MotorState.Fall;
            if (!RemakeRules.JumpBuffer || !airborne && State != MotorState.Ground) jumpBuffer = 0;
            else if (airborne && upPressed) jumpBuffer = RemakeRules.JumpBufferTicks;
            else if (airborne && jumpBuffer > 0) jumpBuffer--;
            var liquidBefore = Liquid;
            Liquid = LiquidAt();
            if (!InLiquid || liquidBefore != Liquid)
                vAcc = hAcc = liquidSub = 0;
            // Nadando por debajo de la superficie: sube a flotar, 1 px por tick.
            if (Swim && State == MotorState.Ground && Y > FloatY() && !HeadBlocked(Y - 1))
                Y--;

            // Espada: una pulsación nueva fuera de un golpe y fuera de la escalera; dura 4 ticks.
            bool pressed = input.Attack && !attackHeld;
            attackHeld = input.Attack;
            AttackStartedThisTick = false;
            if (AttackTicks > 0)
                AttackTicks--;
            else if (pressed && State != MotorState.Ladder)
            {
                AttackTicks = P.SwordTicks;
                AttackStartedThisTick = true;
            }

            if (landY >= 0)
            {
                // Tick de aterrizaje: si llegó justo al suelo, aún se mueve como en el aire; si se hundió, solo se corrige
                // (demo 1: 117 → 123 → 120 sin moverse; 190 → 193 → 196 en la sala 143) [O].
                if (airControl && Y == landY) AirMove(input, false);
                Y = landY;
                landY = -1;
                State = MotorState.Ground;
                CrossRoomEdges();
                return;
            }
            switch (State)
            {
                case MotorState.Ground: TickGround(input, upPressed, downPressed); break;
                case MotorState.JumpPrep: TickJumpPrep(input); break;
                case MotorState.Jump: TickJump(input); break;
                case MotorState.Fall: TickFall(input, upPressed); break;
                case MotorState.Ladder:
                    TickLadder(input);
                    if (Shoes && State == MotorState.Ladder && (input.Up || input.Down) && ++shoeLadder % 4 == 0)
                        TickLadder(input);
                    break;
                case MotorState.Carried: TickCarried(); break;
                case MotorState.Knockback: TickKnockback(); break;
            }
            CrossRoomEdges();
        }

        // ---------- estados ----------

        // Medido en openMSX: con ↑ o ↓ mantenidos no se repite nada al llegar al suelo (ni otro salto ni la
        // escalera siguiente); hace falta una pulsación nueva.
        void TickGround(MotorInput input, bool upPressed, bool downPressed)
        {
            // RemakeRules.JumpBuffer: la pulsación recordada del aire vale en el primer tick en el suelo, si ↑ sigue pulsado.
            bool buffered = jumpBuffer > 0 && input.Up;
            jumpBuffer = 0;
            if (AttackTicks > 0)
            {
                // En el suelo se para mientras golpea.
                if (!Supported())
                    StartLedgeFall();
                return;
            }
            if (downPressed && AtDoor())
            {
                DoorEntered?.Invoke(Room.doorWorld);
                return;
            }
            if (upPressed && TryGrabLadder(Y - 1))
                return;
            // La escalera puede atravesar el suelo: la fila de los pies es sólida y sigue debajo. Al engancharse
            // el jugador cruza esa fila de golpe (y + 8), como en el original.
            if (downPressed && TryGrabLadder(Y))
                return;
            if (downPressed && TryGrabLadder(Y + 8, Y))
            {
                Y += 8;
                return;
            }
            // El tick en que se procesa ↑ ya no anda (trazas de las demos: el original lee las teclas con un tick de
            // retraso, y lo que parecía "aún anda 2 px" era ese retraso; el retraso lo pone PlayerController).
            if (upPressed || buffered)
            {
                StartJump();
                return;
            }
            MoveHorizontal(input, P.WalkSpeed + (Shoes && (shoeWalk = !shoeWalk) ? 1 : 0));
            if (!Supported())
                StartLedgeFall();
        }

        void StartJump()
        {
            JumpedFromShaft = Lifts?.RidingShaft(Room.id) ?? 0;   // b03:A308
            State = MotorState.JumpPrep;
            prepTicks = 1;               // este tick ya cuenta como preparación
            jumpBuffer = coyoteLeft = 0;
        }

        void TickJumpPrep(MotorInput input)
        {
            if (!input.Up && Character == Character.Popolon)
            {
                State = MotorState.Ground;   // Popolon no salta si suelta ↑ antes de despegar
                return;
            }
            if (!LiquidStep())
                return;
            if (++prepTicks <= P.JumpPrepTicks)
                return;                      // tick quieto antes de despegar
            State = MotorState.Jump;
            jumpDir = InputDir(input);
            hitWall = false;
            riseIndex = Character == Character.Popolon ? P.RiseStartPopolon : P.RiseStartAphrodite;
            apexTicks = 0;
            cutDone = false;
            TickJump(input);
        }

        void TickJump(MotorInput input)
        {
            if (riseIndex < P.RiseTable.Length)
            {
                AirMove(input, true);
                int dy = P.RiseTable[riseIndex];
                bool released = !input.Up && Character == Character.Popolon;
                if (!cutDone && (released || HeadBlocked(Y - dy)))
                {
                    // Soltar ↑ o chocar con el techo: un tick quieto y salta a la última entrada de la subida.
                    cutDone = true;
                    riseIndex = P.RiseTable.Length - 1;
                    return;
                }
                bool step = LiquidStep();
                int move = ScaleVertical(dy);
                if (!HeadBlocked(Y - move))
                    Y -= move;
                if (step)
                    riseIndex++;
                return;
            }

            // Punto más alto
            if (OnLift)
            {
                State = MotorState.Ground;   // b03:A3AC: sobre un ascensor aterriza
                return;
            }
            AirMove(input, apexTicks < P.ApexTicksAtRiseSpeed);
            if (LiquidStep() && ++apexTicks >= P.ApexTicks)
                StartFall(0, true);
        }

        // Movimiento horizontal en un salto (trazas de las demos en openMSX): el salto conserva la dirección con la que
        // despegó (o la última pulsada) aunque se suelte: 2 px/tick. Subiendo y en el primer tick arriba, 2; después, 3
        // solo con ↑ y la dirección pulsadas, y 2 con la dirección sola o sin nada.
        int jumpDir;
        bool hitWall;
        int landY = -1;            // aterrizaje pendiente (se ha hundido en el suelo este tick)

        // Con ← y → a la vez, gana ← (trazas de la demo 1: el jugador va a la izquierda).
        static int InputDir(MotorInput input) => input.Left ? -1 : input.Right ? 1 : 0;

        void AirMove(MotorInput input, bool rising)
        {
            int dir = InputDir(input);
            if (dir != 0) jumpDir = dir;
            if (jumpDir == 0) return;
            // Tras chocar con una pared ya no pasa de 2 px/tick hasta aterrizar (demo 0, castillo 101: 91 → 88, parado
            // contra la pared, y luego 86 84 82 con ← y ↑ pulsados) [O].
            int speed = !rising && dir != 0 && input.Up && !hitWall ? P.AirSpeedFalling : P.AirSpeedRising;
            var held = input;
            held.Left = jumpDir < 0;
            held.Right = jumpDir > 0;
            // Al chocar con una pared se pierde la inercia del salto (demo 1, castillo 121 → 132) [O].
            if (!MoveHorizontal(held, speed))
            {
                hitWall = true;
                if (dir == 0) jumpDir = 0;
            }
        }

        // Al salir andando de una plataforma (o quedarse sin suelo): 4 ticks quieto en el aire y luego la tabla de caída
        // desde el principio (1, 2, 3, 4…), sin control (trazas de las demos en openMSX).
        int hoverTicks;
        void StartLedgeFall()
        {
            StartFall(0, false);
            hoverTicks = P.LedgeHoverTicks;
            coyoteLeft = RemakeRules.CoyoteJump ? RemakeRules.CoyoteTicks : 0;
        }

        // RemakeRules.CoyoteJump: ticks que quedan para saltar tras salir de un borde; RemakeRules.JumpBuffer: ticks
        // que se recuerda una pulsación de ↑ en el aire.
        int coyoteLeft, jumpBuffer;

        void StartFall(int index, bool control)
        {
            State = MotorState.Fall;
            fallIndex = index;
            airControl = control;
            coyoteLeft = 0;
        }

        void TickFall(MotorInput input, bool upPressed)
        {
            if (OnLift)
            {
                // b03:A4BE: sobre un ascensor aterriza, también en los ticks quieto antes de caer.
                State = MotorState.Ground;
                hoverTicks = 0;
                return;
            }
            if (coyoteLeft > 0)
            {
                // Aún quieto en el borde (sin haber bajado): salta como desde el suelo.
                coyoteLeft--;
                if (upPressed && hoverTicks > 0)
                {
                    hoverTicks = 0;
                    StartJump();
                    return;
                }
            }
            if (hoverTicks > 0)
            {
                hoverTicks--;
                return;
            }
            if (airControl)
                AirMove(input, false);
            int dy = ScaleVertical(P.FallTable[System.Math.Min(fallIndex, P.FallTable.Length - 1)]);
            if (LiquidStep())
                fallIndex++;
            // Baja píxel a píxel hasta tocar suelo. Si le sobra caída, el original se hunde lo que le sobra ese tick y al
            // siguiente se corrige encima del suelo sin hacer nada más (demo 1: 117 → 123 → 120) [O].
            for (int i = 0; i < dy; i++)
            {
                if (Supported())
                {
                    if (i > 0) { landY = Y; Y += dy - i; return; }
                    State = MotorState.Ground;
                    return;
                }
                Y++;
            }
            // Llega justo al suelo: ese tick aún es aire y el siguiente también se mueve como en el aire (demo 1,
            // castillo 143: aterriza en 193 y avanza a 196 antes de andar) [O].
            if (Supported())
                landY = Y;
        }

        void TickLadder(MotorInput input)
        {
            // A la altura de un suelo (sólido bajo los pies) se puede salir de la escalera andando, también cuando
            // la escalera atraviesa ese suelo y sigue por encima y por debajo.
            if ((input.Left || input.Right) && !input.Up && !input.Down &&
                ((Y - P.RoomTopPx) & 7) == 0 && SolidUnderFeet())
            {
                State = MotorState.Ground;
                TickGround(input, false, false);
                return;
            }
            // Remake: soltarse de la escalera hacia un lado con sitio libre (RemakeRules).
            int side = input.Left ? -1 : input.Right ? 1 : 0;
            if (RemakeRules.LeaveLadderSideways && side != 0 && !input.Up && !input.Down && !BodyBlockedAt(X + side, side))
            {
                StartFall(0, true);
                hitWall = false;
                MoveHorizontal(input, P.AirSpeedFalling);
                return;
            }
            if (input.Up)
            {
                Y -= P.LadderSpeed;
                // Al entrar en una fila de suelo, el original salta arriba de ella y queda de pie (y = 127 -> 120).
                if (SolidPairAt(X, Y))
                {
                    Y = ((Y - P.RoomTopPx) & ~7) + P.RoomTopPx;
                    State = MotorState.Ground;
                }
                // Arriba del todo: sin escalera ni suelo por encima de los pies, queda de pie.
                else if (((Y - P.RoomTopPx) & 7) == 0 && !LadderContinues(Y - 1, -1))
                    State = MotorState.Ground;
            }
            else if (input.Down)
            {
                Y += P.LadderSpeed;
                if (((Y - P.RoomTopPx) & 7) == 0 && !LadderContinues(Y, 1))
                {
                    if (SolidUnderFeet())
                        State = MotorState.Ground;
                    else
                        StartFall(0, false);
                }
            }
        }

        // Puerta de mundo del castillo: y = Y_puerta + 0x18, x entre X_puerta + 14 y + 18 (state_machine.md, estado 8).
        public bool AtDoor() =>
            Room.doorWorld > 0 && Y == Room.door.y + 0x18 && X >= Room.door.x + 14 && X <= Room.door.x + 18;

        // Zona de la espada en píxeles de la sala (incluidos los extremos).
        public (int x0, int x1, int y0, int y1) SwordRect() =>
            FacingLeft
                ? (X - P.SwordReach, X - P.SwordFrom, Y - P.SwordTop, Y - P.SwordBottom)
                : (X + P.SwordFrom, X + P.SwordReach, Y - P.SwordTop, Y - P.SwordBottom);

        void TickKnockback()
        {
            for (int i = 0; i < P.KnockbackSpeed; i++)
            {
                if (BodyBlockedAt(X + knockDir, knockDir))
                    break;
                X += knockDir;
            }
            if (knockStep < P.KnockbackDy.Length)
            {
                int dy = P.KnockbackDy[knockStep++];
                if (dy < 0 && !HeadBlocked(Y + dy))
                    Y += dy;
                else if (dy > 0 && !Supported())
                    Y += dy;
                if (knockStep == P.KnockbackDy.Length && Supported())
                    State = MotorState.Ground;
                return;
            }
            // Sin suelo al acabar el arco: sigue cayendo (con el empuje horizontal) hasta aterrizar.
            int fall = P.FallTable[System.Math.Min(fallIndex++, P.FallTable.Length - 1)];
            for (int i = 0; i < fall && !Supported(); i++)
                Y++;
            if (Supported())
                State = MotorState.Ground;
        }

        void TickCarried()
        {
            if (carryWait > 0)
            {
                carryWait--;
                return;
            }
            X += carryDir * carrySpeed;
            if (--carryTicks <= 0)
                State = Supported() ? MotorState.Ground : MotorState.Fall;
        }

        // ---------- líquidos ----------

        TileClass LiquidAt()
        {
            foreach (int py in new[] { Y - 1, Y })
            {
                var t = world.TileAt(Room, X, py);
                if (t == TileClass.Water || t == TileClass.Lava)
                    return t;
            }
            return TileClass.Empty;
        }

        // En el líquido, las tablas avanzan una entrada cada 4 ticks.
        bool LiquidStep() => !SlowLiquid || (++liquidSub % P.LiquidTimeScale) == 0;

        int ScaleVertical(int dy)
        {
            if (!SlowLiquid)
                return dy;
            vAcc += dy * 256 / P.LiquidTimeScale;
            int move = vAcc / 256;
            vAcc -= move * 256;
            return move;
        }

        int ScaleHorizontal(int speed)
        {
            if (!InLiquid)
                return speed;
            hAcc += speed * 128;
            int move = hAcc / 256;
            hAcc -= move * 256;
            return move;
        }

        // ---------- escaleras ----------

        // Escalera de dos tiles (9A 9B) bajo el centro del jugador.
        bool LadderPairAt(int x, int py) =>
            world.Ladder(Room, x - 1, py) && world.Ladder(Room, x, py);

        // En los mapas del original la escalera atraviesa los suelos: las columnas de la escalera son sólidas en
        // la fila del suelo y la escalera sigue al otro lado (castillo, sala 1: suelo en la fila 11 y escalera
        // en las filas 0-10 y 12-19). Se puede trepar a través de una fila así.
        // Bajando, la escalera sigue solo si hay escalera bajo los pies: al llegar a un suelo se queda de pie
        // aunque la escalera siga al otro lado. Subiendo se entra en una fila de suelo (y se sale encima de ella).
        bool LadderContinues(int py, int dir)
        {
            if (LadderPairAt(X, py))
                return true;
            if (dir > 0)
                return false;
            // Por el borde de arriba se sigue trepando: hacia la escalera de la sala de arriba, una salida al
            // castillo o el suelo de la sala de arriba (que se cruza de golpe, como dentro de una sala).
            if (py < P.RoomTopPx)
                return true;
            return SolidPairAt(X, py);
        }

        bool SolidPairAt(int x, int py) => world.Solid(Room, x - 1, py) && world.Solid(Room, x, py);

        // floorPy >= 0: la escalera está bajo un suelo que hay que atravesar (sólido en sus dos columnas).
        bool TryGrabLadder(int py, int floorPy = -1)
        {
            // El original alinea al jugador con la escalera: x = x de la escalera + 8 (docs/player.md).
            int col = X >> 3;
            int left;
            if (world.Ladder(Room, (col - 1) * 8, py) && world.Ladder(Room, col * 8, py)) left = col - 1;
            else if (world.Ladder(Room, col * 8, py) && world.Ladder(Room, (col + 1) * 8, py)) left = col;
            else return false;
            if (floorPy >= 0 && !(world.Solid(Room, left * 8, floorPy) && world.Solid(Room, left * 8 + 8, floorPy)))
                return false;
            int ladderX = left * 8 + 8;
            if (System.Math.Abs(ladderX - X) > 4)
                return false;
            X = ladderX;
            State = MotorState.Ladder;
            return true;
        }

        // ---------- colisión ----------

        bool SolidUnderFeet()
        {
            for (int px = X + P.SupportLeft; px <= X + P.SupportRight; px += 4)
                if (world.Solid(Room, px, Y))
                    return true;
            return false;
        }

        // De pie si hay sólido bajo los pies o la parte de arriba de una escalera que baja.
        bool Supported()
        {
            if (OnLift)
                return true;
            if (InLiquid && Y >= P.LiquidFloorY)
                return true;                 // en el líquido no se baja de y = 184
            if (Swim && Y >= FloatY())
                return true;                 // RemakeRules.HollowSwim: flota
            if (((Y - P.RoomTopPx) & 7) != 0)
                return false;
            return SolidUnderFeet() || (LadderPairAt(X, Y) && !LadderPairAt(X, Y - 1));
        }

        bool HeadBlocked(int newY)
        {
            int py = newY - P.HeadHeight;
            // Por encima de la sala está el marcador: el salto no pasa de y = 24 (medido en el mundo 1, sala 1:
            // desde y = 56 sube hasta 37 y no sale por arriba). Solo las escaleras llevan a la sala de arriba.
            if (py < P.RoomTopPx)
                return py < P.RoomTopPx - 8;
            // [I] El techo se mira con el mismo ancho que el suelo (x − 4 … x + 4): al caer pegado a una pared el cuerpo
            // queda unos píxeles metido en ella, y con el ancho entero la pared contaba como techo y no se podía saltar
            // hasta apartarse.
            return world.Solid(Room, X + P.SupportLeft, py) || world.Solid(Room, X, py) || world.Solid(Room, X + P.SupportRight, py);
        }

        // Pared en el borde hacia el que se mueve (dir), con el cuerpo en x. Solo ese borde: si el jugador ha caído
        // pegado a una pared y la solapa por el otro lado, puede alejarse de ella.
        bool BodyBlockedAt(int x, int dir)
        {
            int edge = dir < 0 ? x + P.FootLeft : x + P.FootRight;
            foreach (int py in new[] { Y - P.WallTop, Y - 8, Y - 1 })
            {
                // Por encima de la sala está el marcador: no es pared (la cabeza llega a y = 24 al saltar pegado al
                // borde de arriba y se sigue avanzando; castillo, sala 73).
                if (py < P.RoomTopPx)
                    continue;
                if (world.Solid(Room, edge, py))
                    return true;
            }
            return false;
        }

        // Devuelve false si una pared lo ha parado (o lo ha sacado de ella).
        bool MoveHorizontal(MotorInput input, int speed)
        {
            int dir = input.Left ? -1 : input.Right ? 1 : 0;
            if (dir == 0)
                return true;
            FacingLeft = dir < 0;
            // Metido en una pared por ese lado (al caer pegado a ella): el original lo saca hasta el borde del tile en
            // vez de moverlo (demo 1, castillo 120: x 196 → 200 pulsando ← junto a una piedra) [O].
            if (BodyBlockedAt(X, dir))
            {
                X = dir < 0 ? ((X + P.FootLeft) & ~7) + 8 - P.FootLeft : ((X + P.FootRight) & ~7) - 1 - P.FootRight;
                return false;
            }
            // Todo el paso o nada: si el destino choca, no se mueve (demo 0, castillo 100: en x = 81 junto a una piedra
            // que empieza en x = 71, pulsando ← se queda en 81 en vez de avanzar 1 px) [O].
            speed = ScaleHorizontal(speed);
            if (speed == 0)
                return true;
            if (BodyBlockedAt(X + dir * speed, dir))
                return false;
            X += dir * speed;
            return true;
        }

        // ---------- cambio de sala ----------

        // Cambio de sala (medido en openMSX, trazas de las demos, docs/attract.md): a la derecha al llegar a x = 248 y
        // se aparece en x = 16; a la izquierda al bajar de x = 16 y se aparece en x = 247; abajo al pasar de y = 192 y
        // se aparece en y = 40; arriba (por una escalera o una plataforma, no saltando) al bajar de y = 40 y se aparece
        // en y = 185.
        void CrossRoomEdges()
        {
            int dx = X < P.ExitLeftX ? -1 : X >= P.ExitRightX ? 1 : 0;
            bool climbing = State == MotorState.Ladder;
            // b03:A7F8: saltando desde un ascensor vertical también se sale por arriba, y ese ascensor sube detrás (si
            // sube). En un ascensor que cambia de sala, el jugador va con él (LiftSystem).
            bool liftJump = JumpedFromShaft != 0 && (State == MotorState.Jump || State == MotorState.JumpPrep || State == MotorState.Fall);
            int dy = (climbing || liftJump) && Y < P.ExitUpY ? -1 : Y > P.RoomBottomPx ? 1 : 0;
            if (dx == 0 && dy == 0)
                return;
            var next = world.Neighbour(Room, dx, dy);
            int exit = RoomWorld.CastleExit(Room, dx, dy);
            if (next == null && exit != 0)
            {
                CastleExitReached?.Invoke(exit);
                return;
            }
            if (next == null || next.shrine)
            {
                // Sin vecino o hacia la sala de un santuario de mundo (se entra por su pantalla): se queda donde está.
                if (dx < 0) X = System.Math.Max(X, 0);
                if (dx > 0) X = System.Math.Min(X, P.RoomWidthPx - 1);
                return;
            }
            if (dy < 0 && liftJump)
                Lifts?.PlayerJumpedOut(JumpedFromShaft);
            var previous = Room;
            Room = next;
            if (dx > 0) X = P.EnterFromLeftX;
            if (dx < 0) X = P.EnterFromRightX;
            if (dy > 0) Y = P.EnterFromTopY;
            if (dy < 0) Y = P.EnterFromBottomY;
            RoomChanged?.Invoke(previous, next);
        }
    }
}
