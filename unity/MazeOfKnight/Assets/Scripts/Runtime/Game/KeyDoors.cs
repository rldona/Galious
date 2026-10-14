using System.Collections.Generic;
using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game
{
    // Puertas con llave: objetos de sala de clase 1 (castillo) y 2 (mundos), una columna de 1 x 3 tiles en
    // (x, y). Medido en openMSX (docs/entities.md, "Puertas con llave"): con al menos una llave, a la altura
    // exacta (y = y_puerta + 0x18), pegado a la puerta (x = x_puerta - 8 o + 16) y empujando 30 ticks, gasta
    // una llave, la puerta encoge en 2 pasos de 6 ticks y arrastra al jugador 24 px hasta el otro lado.
    // La del castillo se vuelve a cerrar (cada paso cuesta una llave); la de los mundos desaparece.
    public class KeyDoors
    {
        public class Door
        {
            public RoomModel room;
            public int x, y, cls;
            public int pushTicks;
            public int openTicks = -1;   // >= 0 mientras se abre y arrastra
            public int height = 3;       // tiles cerrados
            public bool gone;            // clase 2 ya abierta
        }

        readonly List<Door> doors = new List<Door>();
        readonly RoomWorld world;
        readonly HashSet<(string, int, int, int)> opened;
        readonly string mapName;

        public event System.Action<Door> Changed;
        public event System.Action<Door> Opened;

        public KeyDoors(string mapName, RoomWorld world, HashSet<(string, int, int, int)> opened)
        {
            this.mapName = mapName;
            this.world = world;
            this.opened = opened;
            foreach (var room in world.Rooms)
                foreach (var o in room.objects)
                {
                    int cls = o.type & 0x0F;
                    if (cls != 1 && cls != 2)
                        continue;
                    var door = new Door { room = room, x = o.x, y = o.y, cls = cls };
                    if (opened.Contains((mapName, room.id, o.x, o.y)))
                    {
                        door.gone = true;
                        door.height = 0;
                        Apply(door);
                    }
                    doors.Add(door);
                }
        }

        public IEnumerable<Door> All => doors;

        // Un tick después del motor. keys: llaves disponibles; devuelve las que quedan.
        public int Tick(PlayerMotor motor, int keys)
        {
            foreach (var door in doors)
            {
                if (door.gone)
                    continue;
                if (door.openTicks >= 0)
                {
                    TickOpening(door, motor);
                    continue;
                }
                int dir = Pushing(door, motor);
                if (dir == 0 || keys <= 0)
                {
                    door.pushTicks = 0;
                    continue;
                }
                if (++door.pushTicks < P.KeyDoorPushTicks)
                    continue;
                keys--;
                door.pushTicks = 0;
                door.openTicks = 0;
                motor.Carry(dir, P.KeyDoorOpenTicks, P.KeyDoorCarryTicks, P.KeyDoorCarrySpeed);
                Opened?.Invoke(door);
            }
            return keys;
        }

        static int Pushing(Door door, PlayerMotor motor)
        {
            if (motor.Room != door.room || motor.State != MotorState.Ground || motor.Y != door.y + 0x18)
                return 0;
            var input = motor.LastInput;
            if (motor.X == door.x - 8 && !motor.FacingLeft && input.Right && !input.Left)
                return 1;
            if (motor.X == door.x + 16 && motor.FacingLeft && input.Left && !input.Right)
                return -1;
            return 0;
        }

        void TickOpening(Door door, PlayerMotor motor)
        {
            door.openTicks++;
            // Encoge desde abajo un tile cada 6 ticks, hasta dejar solo el de arriba.
            int height = 3 - System.Math.Min(2, door.openTicks / 6);
            if (height != door.height)
            {
                door.height = height;
                Apply(door);
            }
            if (door.openTicks < P.KeyDoorOpenTicks + P.KeyDoorCarryTicks)
                return;
            door.openTicks = -1;
            if (door.cls == 2)
            {
                door.gone = true;
                door.height = 0;
                opened.Add((mapName, door.room.id, door.x, door.y));
            }
            else
                door.height = 3;             // la del castillo se vuelve a cerrar
            Apply(door);
        }

        // Colisión de la puerta: los tiles de arriba hasta su altura son sólidos.
        void Apply(Door door)
        {
            int col = door.x >> 3, row = (door.y - P.RoomTopPx) >> 3;
            world.Clear(door.room, col, row, 1, 3);
            world.SetSolid(door.room, col, row, 1, door.height);
            Changed?.Invoke(door);
        }
    }
}
