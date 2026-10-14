using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game
{
    public enum Character { Aphrodite = 0, Popolon = 1 }

    public enum MotorState { Ground, JumpPrep, Jump, Fall, Ladder }

    public struct MotorInput
    {
        public bool Left, Right, Up, Down;
    }

    // Movimiento del jugador tick a tick, con los números del original (docs/player.md). Clase sin
    // MonoBehaviour para poder probarla contra las curvas medidas en openMSX (Tests/EditMode).
    // Posición en píxeles del original dentro de la sala: x = centro, y = pies (fila r: y = 8r + 32).
    public class PlayerMotor
    {
        readonly RoomWorld world;

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

        public event System.Action<RoomModel, RoomModel> RoomChanged;

        public PlayerMotor(RoomWorld world, RoomModel room, int x, int y)
        {
            this.world = world;
            Place(room, x, y);
        }

        public void Place(RoomModel room, int x, int y)
        {
            Room = room;
            X = x;
            Y = y;
            State = MotorState.Ground;
        }

        public void Tick(MotorInput input)
        {
            switch (State)
            {
                case MotorState.Ground: TickGround(input); break;
                case MotorState.JumpPrep: TickJumpPrep(input); break;
                case MotorState.Jump: TickJump(input); break;
                case MotorState.Fall: TickFall(input); break;
                case MotorState.Ladder: TickLadder(input); break;
            }
            CrossRoomEdges();
        }

        // ---------- estados ----------

        void TickGround(MotorInput input)
        {
            if (input.Up && TryGrabLadder(Y - 1))
                return;
            // La escalera puede atravesar el suelo: la fila de los pies es sólida y sigue debajo.
            if (input.Down && (TryGrabLadder(Y) || TryGrabLadder(Y + 8, Y)))
                return;
            MoveHorizontal(input, P.WalkSpeed);
            if (input.Up)
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
                if (!HeadBlocked(Y - dy))
                    Y -= dy;
                riseIndex++;
                return;
            }

            // Punto más alto
            MoveHorizontal(input, apexTicks < P.ApexTicksAtRiseSpeed ? P.AirSpeedRising : P.AirSpeedFalling);
            if (++apexTicks >= P.ApexTicks)
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
            int dy = P.FallTable[System.Math.Min(fallIndex, P.FallTable.Length - 1)];
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
            if (input.Up)
            {
                Y -= P.LadderSpeed;
                // Arriba del todo: sin escalera por encima de los pies, queda de pie.
                if (((Y - P.RoomTopPx) & 7) == 0 && !LadderContinues(Y - 1, -1))
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

        // ---------- escaleras ----------

        // Escalera de dos tiles (9A 9B) bajo el centro del jugador.
        bool LadderPairAt(int x, int py) =>
            world.Ladder(Room, x - 1, py) && world.Ladder(Room, x, py);

        // En los mapas del original la escalera atraviesa los suelos: las columnas de la escalera son sólidas en
        // la fila del suelo y la escalera sigue al otro lado (castillo, sala 1: suelo en la fila 11 y escalera
        // en las filas 0-10 y 12-19). Se puede trepar a través de una fila así.
        // Al subir también se atraviesa un suelo sin escalera encima, para salir de pie sobre él.
        bool LadderContinues(int py, int dir)
        {
            if (LadderPairAt(X, py))
                return true;
            if (!SolidPairAt(X, py))
                return false;
            return dir > 0 ? LadderPairAt(X, py + 8) : !SolidPairAt(X, py - 8);
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
            for (int px = X + P.FootLeft; px <= X + P.FootRight; px += 5)
                if (world.Solid(Room, px, Y))
                    return true;
            return world.Solid(Room, X + P.FootRight, Y);
        }

        // De pie si hay sólido bajo los pies o la parte de arriba de una escalera que baja.
        bool Supported()
        {
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
            int dy = Y <= P.RoomTopPx && State != MotorState.Ground ? -1 : Y > P.RoomBottomPx ? 1 : 0;
            if (dx == 0 && dy == 0)
                return;
            var next = world.Neighbour(Room, dx, dy);
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
