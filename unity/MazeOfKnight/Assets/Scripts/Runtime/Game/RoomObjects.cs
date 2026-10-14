using System.Collections.Generic;
using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game
{
    // Mecanismos de sala medidos en openMSX (docs/entities.md, "Objetos de sala, medidos"):
    //   - clases 3 y 4: puertas giratorias (de un solo sentido y normales);
    //   - clase 5: muro que se cierra a tu espalda;
    //   - clase 7: pared que se rompe a espadazos;
    //   - escaleras extra del castillo que se retiran un minuto después de usarlas.
    // Las puertas con llave (clases 1 y 2) van en KeyDoors.
    public class RoomObjects
    {
        public const int RevolvingPushTicks = 30;
        public const int RevolvingCarryTicks = 24;   // 24 px a 1 px/tick
        public const int ClosingWallStepTicks = 15;
        public const int LadderRetractTicks = 60 * 30;
        public const int BreakableReach = 14;        // golpe a menos de 14 px del lado de la pared
        public const int ItemBreadAndWater = 0x29;
        public const int ItemPendant = 0x1C;

        public class Revolving { public RoomModel room; public int x, y, cls, type, push; }
        public class ClosingWall { public RoomModel room; public int x, y, noted, height, timer; public bool closed; }
        public class Breakable { public RoomModel room; public int x, y, hits; public bool broken; }
        public class Ladder { public RoomModel room; public LadderJson data; public int timer; }

        readonly List<Revolving> revolving = new List<Revolving>();
        readonly List<ClosingWall> walls = new List<ClosingWall>();
        readonly List<Breakable> breakables = new List<Breakable>();
        readonly List<Ladder> ladders = new List<Ladder>();
        readonly RoomWorld world;
        readonly HashSet<(string, int, int, int)> persistent;
        readonly string mapName;

        // Escalera en uso (para saber en qué sentido se ha recorrido).
        Ladder usingLadder;
        int ladderEntryY;
        MotorState lastState;

        public event System.Action<RoomModel, int, int, int, int> CellsChanged;   // sala, col, fila, ancho, alto
        public event System.Action<string> Message;

        public RoomObjects(string mapName, RoomWorld world, HashSet<(string, int, int, int)> persistent)
        {
            this.mapName = mapName;
            this.world = world;
            this.persistent = persistent;
            foreach (var room in world.Rooms)
            {
                foreach (var o in room.objects)
                {
                    int cls = o.type & 0x0F;
                    bool done = persistent.Contains((mapName, room.id, o.x, o.y));
                    if (cls == 3 || cls == 4)
                        revolving.Add(new Revolving { room = room, x = o.x, y = o.y, cls = cls, type = o.type });
                    else if (cls == 5)
                    {
                        var w = new ClosingWall { room = room, x = o.x, y = o.y, closed = done, height = done ? 3 : 0 };
                        walls.Add(w);
                        if (done)
                            world.SetSolid(room, o.x >> 3, Row(o.y), 1, 3);
                    }
                    else if (cls == 7)
                    {
                        var b = new Breakable { room = room, x = o.x, y = o.y, hits = BreakableHits(o.type), broken = done };
                        breakables.Add(b);
                        if (done)
                            world.Clear(room, o.x >> 3, Row(o.y), 4, 3);
                    }
                }
                foreach (var l in room.ladders)
                    ladders.Add(new Ladder { room = room, data = l });
            }
        }

        public IEnumerable<Revolving> RevolvingDoors => revolving;

        static int Row(int y) => (y - P.RoomTopPx) >> 3;

        // Golpes según el nibble alto del tipo (tabla b09:A65D): 0x07 30, 0x17 20, 0x27 10, 0x37 3.
        public static int BreakableHits(int type)
        {
            switch (type >> 4)
            {
                case 0: return 30;
                case 1: return 20;
                case 2: return 10;
                default: return 3;
            }
        }

        public void Tick(PlayerMotor motor, ICollection<int> collected)
        {
            TickRevolving(motor, collected);
            TickClosingWalls(motor);
            TickLadders(motor);
        }

        // ---------- puertas giratorias ----------

        void TickRevolving(PlayerMotor motor, ICollection<int> collected)
        {
            foreach (var d in revolving)
            {
                int dir = Pushing(d, motor);
                bool allowed = dir != 0;
                if (d.cls == 3)
                    allowed &= dir == ((d.type & 0x10) != 0 ? 1 : -1);   // 0x03 hacia la izquierda, 0x13 hacia la derecha
                else if (motor.Character == Character.Aphrodite)
                    allowed &= collected.Contains(ItemBreadAndWater);
                if (!allowed)
                {
                    d.push = 0;
                    continue;
                }
                if (++d.push < RevolvingPushTicks)
                    continue;
                d.push = 0;
                motor.Carry(dir, 0, RevolvingCarryTicks, 1);
            }
        }

        static int Pushing(Revolving d, PlayerMotor motor)
        {
            if (motor.Room != d.room || motor.State != MotorState.Ground || motor.Y != d.y + 0x18)
                return 0;
            var input = motor.LastInput;
            if (motor.X == d.x - 8 && !motor.FacingLeft && input.Right && !input.Left)
                return 1;
            if (motor.X == d.x + 16 && motor.FacingLeft && input.Left && !input.Right)
                return -1;
            return 0;
        }

        // ---------- muros que se cierran ----------

        void TickClosingWalls(PlayerMotor motor)
        {
            foreach (var w in walls)
            {
                if (w.closed && w.height >= 3)
                    continue;
                if (w.closed)
                {
                    // Crece desde arriba, un tile cada 15 ticks.
                    if (++w.timer < ClosingWallStepTicks)
                        continue;
                    w.timer = 0;
                    w.height++;
                    world.SetSolid(w.room, w.x >> 3, Row(w.y), 1, w.height);
                    CellsChanged?.Invoke(w.room, w.x >> 3, Row(w.y), 1, 3);
                    continue;
                }
                if (motor.Room != w.room || motor.Y <= w.y || motor.Y > w.y + 24)
                {
                    w.noted = 0;
                    continue;
                }
                // Medido: anota el sentido cuando el centro pasa la columna (x = columna + 8 hacia la derecha) y
                // empieza a cerrarse cuando el jugador está entero al otro lado (16 px después).
                if (motor.X >= w.x + 8 && motor.X < w.x + 16) w.noted = 1;
                else if (motor.X <= w.x - 1 && motor.X > w.x - 8) w.noted = -1;
                bool past = w.noted > 0 ? motor.X >= w.x + 16 : w.noted < 0 && motor.X <= w.x - 8;
                if (!past)
                    continue;
                w.closed = true;
                w.timer = ClosingWallStepTicks - 1;
                persistent.Add((mapName, w.room.id, w.x, w.y));
                Message?.Invoke("Un muro se cierra a tu espalda");
            }
        }

        // ---------- paredes que se rompen ----------

        // Al empezar un espadazo: golpea la pared de la sala si está a su altura, mirando hacia ella y cerca.
        public void Strike(PlayerMotor motor, ICollection<int> collected)
        {
            if (motor.Character == Character.Aphrodite && !collected.Contains(ItemPendant))
                return;
            foreach (var b in breakables)
            {
                if (b.broken || b.room != motor.Room || motor.Y <= b.y || motor.Y > b.y + 24)
                    continue;
                int gap = motor.FacingLeft ? (motor.X + P.FootLeft) - (b.x + 31) : b.x - (motor.X + P.FootRight);
                if (gap < 0 || gap >= BreakableReach)
                    continue;
                b.hits--;
                if (b.hits > 0)
                {
                    Message?.Invoke("La pared suena");
                    continue;
                }
                b.broken = true;
                persistent.Add((mapName, b.room.id, b.x, b.y));
                world.Clear(b.room, b.x >> 3, Row(b.y), 4, 3);
                CellsChanged?.Invoke(b.room, b.x >> 3, Row(b.y), 4, 3);
                Message?.Invoke("La pared se rompe");
            }
        }

        // ---------- escaleras que se retiran ----------

        void TickLadders(PlayerMotor motor)
        {
            foreach (var l in ladders)
                if (l.timer > 0 && --l.timer == 0)
                    SetLadder(l, true);

            var state = motor.State;
            if (state == MotorState.Ladder && lastState != MotorState.Ladder)
            {
                usingLadder = ladders.Find(l => l.room == motor.Room && l.timer == 0 && motor.X == l.data.x + 8);
                ladderEntryY = motor.Y;
            }
            else if (state != MotorState.Ladder && lastState == MotorState.Ladder && usingLadder != null)
            {
                // Al terminar de recorrerla: 1 subiendo, 2 bajando; se retira si el modo lo incluye.
                int dir = motor.Y < ladderEntryY ? 1 : 2;
                if (state == MotorState.Ground && System.Math.Abs(motor.Y - ladderEntryY) >= 16 && (usingLadder.data.mode & dir) != 0)
                {
                    usingLadder.timer = LadderRetractTicks;
                    SetLadder(usingLadder, false);
                    Message?.Invoke("La escalera desaparece");
                }
                usingLadder = null;
            }
            lastState = state;
        }

        void SetLadder(Ladder l, bool present)
        {
            var room = l.room;
            var source = present ? room.collision : room.collisionNoLadders;
            if (source.Length == 0)
                return;
            int col = l.data.x >> 3, row = Row(l.data.y);
            world.CopyFrom(room, source, col, row, 2, l.data.height);
            CellsChanged?.Invoke(room, col, row, 2, l.data.height);
        }
    }
}
