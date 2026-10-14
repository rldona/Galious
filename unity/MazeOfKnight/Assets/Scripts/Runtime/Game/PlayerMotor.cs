using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game
{
    public enum Character { Aphrodite = 0, Popolon = 1 }

    public enum MotorState { Ground, JumpPrep, Jump, Fall, Ladder, Carried }

    public struct MotorInput
    {
        public bool Left, Right, Up, Down, Attack;
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
        int vAcc, hAcc, liquidSub;

        // Arrastrado por una puerta: espera y luego avanza sin colisión.
        int carryWait, carryTicks, carryDir, carrySpeed;

        public MotorInput LastInput { get; private set; }

        // Espada
        public int AttackTicks { get; private set; }
        public bool AttackStartedThisTick { get; private set; }
        bool attackHeld;
        bool upHeld, downHeld;   // saltar, engancharse a una escalera y entrar por una puerta piden pulsar de nuevo

        public event System.Action<RoomModel, RoomModel> RoomChanged;
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
        }

        // La puerta arrastra al jugador: wait ticks quieto y luego ticks a speed px/tick en dir.
        public void Carry(int dir, int wait, int ticks, int speed)
        {
            State = MotorState.Carried;
            carryDir = dir;
            carryWait = wait;
            carryTicks = ticks;
            carrySpeed = speed;
            AttackTicks = 0;
        }

        public void Tick(MotorInput input)
        {
            bool upPressed = input.Up && !upHeld, downPressed = input.Down && !downHeld;
            upHeld = input.Up;
            downHeld = input.Down;
            LastInput = input;
            var liquidBefore = Liquid;
            Liquid = LiquidAt();
            if (!InLiquid || liquidBefore != Liquid)
                vAcc = hAcc = liquidSub = 0;

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

            switch (State)
            {
                case MotorState.Ground: TickGround(input, upPressed, downPressed); break;
                case MotorState.JumpPrep: TickJumpPrep(input); break;
                case MotorState.Jump: TickJump(input); break;
                case MotorState.Fall: TickFall(input); break;
                case MotorState.Ladder: TickLadder(input); break;
                case MotorState.Carried: TickCarried(); break;
            }
            CrossRoomEdges();
        }

        // ---------- estados ----------

        // Medido en openMSX: con ↑ o ↓ mantenidos no se repite nada al llegar al suelo (ni otro salto ni la
        // escalera siguiente); hace falta una pulsación nueva.
        void TickGround(MotorInput input, bool upPressed, bool downPressed)
        {
            if (AttackTicks > 0)
            {
                // En el suelo se para mientras golpea.
                if (!Supported())
                    StartFall(P.LedgeFallStart, false);
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
            MoveHorizontal(input, P.WalkSpeed);
            if (upPressed)
            {
                State = MotorState.JumpPrep;
                prepTicks = 1;               // este tick ya cuenta como preparación
                return;
            }
            if (!Supported())
                StartFall(P.LedgeFallStart, false);
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
            riseIndex = Character == Character.Popolon ? P.RiseStartPopolon : P.RiseStartAphrodite;
            apexTicks = 0;
            cutDone = false;
            TickJump(input);
        }

        void TickJump(MotorInput input)
        {
            if (riseIndex < P.RiseTable.Length)
            {
                MoveHorizontal(input, P.AirSpeedRising);
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
            MoveHorizontal(input, apexTicks < P.ApexTicksAtRiseSpeed ? P.AirSpeedRising : P.AirSpeedFalling);
            if (LiquidStep() && ++apexTicks >= P.ApexTicks)
                StartFall(0, true);
        }

        void StartFall(int index, bool control)
        {
            State = MotorState.Fall;
            fallIndex = index;
            airControl = control;
        }

        void TickFall(MotorInput input)
        {
            if (airControl)
                MoveHorizontal(input, P.AirSpeedFalling);
            int dy = ScaleVertical(P.FallTable[System.Math.Min(fallIndex, P.FallTable.Length - 1)]);
            if (LiquidStep())
                fallIndex++;
            // Baja píxel a píxel hasta tocar suelo (el original se hunde y corrige: aquí se para en el borde).
            for (int i = 0; i < dy; i++)
            {
                if (Supported())
                {
                    State = MotorState.Ground;
                    return;
                }
                Y++;
            }
            if (Supported())
                State = MotorState.Ground;
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
            int side = input.Left == input.Right ? 0 : input.Left ? -1 : 1;
            if (RemakeRules.LeaveLadderSideways && side != 0 && !input.Up && !input.Down && !BodyBlockedAt(X + side))
            {
                StartFall(0, true);
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
        bool AtDoor() =>
            Room.doorWorld > 0 && Y == Room.door.y + 0x18 && X >= Room.door.x + 14 && X <= Room.door.x + 18;

        // Zona de la espada en píxeles de la sala (incluidos los extremos).
        public (int x0, int x1, int y0, int y1) SwordRect() =>
            FacingLeft
                ? (X - 1 - P.SwordReach, X - 1 - P.SwordFrom, Y - P.SwordTop, Y - P.SwordBottom)
                : (X + P.SwordFrom, X + P.SwordReach, Y - P.SwordTop, Y - P.SwordBottom);

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
        bool LiquidStep() => !InLiquid || (++liquidSub % P.LiquidTimeScale) == 0;

        int ScaleVertical(int dy)
        {
            if (!InLiquid)
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
            if (InLiquid && Y >= P.LiquidFloorY)
                return true;                 // en el líquido no se baja de y = 184
            if (((Y - P.RoomTopPx) & 7) != 0)
                return false;
            return SolidUnderFeet() || (LadderPairAt(X, Y) && !LadderPairAt(X, Y - 1));
        }

        bool HeadBlocked(int newY)
        {
            int py = newY - P.HeadHeight;
            return world.Solid(Room, X + P.FootLeft, py) || world.Solid(Room, X, py) || world.Solid(Room, X + P.FootRight, py);
        }

        bool BodyBlockedAt(int x)
        {
            int edgeLeft = x + P.FootLeft, edgeRight = x + P.FootRight;
            foreach (int py in new[] { Y - P.HeadHeight, Y - 8, Y - 1 })
                if (world.Solid(Room, edgeLeft, py) || world.Solid(Room, edgeRight, py))
                    return true;
            return false;
        }

        void MoveHorizontal(MotorInput input, int speed)
        {
            int dir = input.Left == input.Right ? 0 : input.Left ? -1 : 1;
            if (dir == 0)
                return;
            FacingLeft = dir < 0;
            speed = ScaleHorizontal(speed);
            for (int i = 0; i < speed; i++)
            {
                if (BodyBlockedAt(X + dir))
                    break;
                X += dir;
            }
        }

        // ---------- cambio de sala ----------

        void CrossRoomEdges()
        {
            int dx = X < 0 ? -1 : X >= P.RoomWidthPx ? 1 : 0;
            // Arriba: en cuanto los pies pasan del borde (y < 32); de pie justo en el borde (y = 32) no se cambia.
            int dy = Y < P.RoomTopPx ? -1 : Y > P.RoomBottomPx ? 1 : 0;
            if (dx == 0 && dy == 0)
                return;
            var next = world.Neighbour(Room, dx, dy);
            int exit = RoomWorld.CastleExit(Room, dx, dy);
            if (next == null && exit != 0)
            {
                CastleExitReached?.Invoke(exit);
                return;
            }
            if (next == null)
            {
                // Sin vecino (o salida al castillo, aún sin implementar): se queda en el borde.
                if (dx < 0) X = 0;
                if (dx > 0) X = P.RoomWidthPx - 1;
                return;
            }
            var previous = Room;
            Room = next;
            X -= dx * P.RoomWidthPx;
            Y -= dy * 160;
            RoomChanged?.Invoke(previous, next);
        }
    }
}
