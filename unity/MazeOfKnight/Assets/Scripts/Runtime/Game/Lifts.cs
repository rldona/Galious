using System.Collections.Generic;
using MazeOfKnight.Data;

namespace MazeOfKnight.Game
{
    // Ascensores del castillo (docs/entities.md, "Ascensores"; datos en Resources/Lifts.json). Bloques de 32 × 16 px:
    // la x, y del ascensor es su esquina de arriba a la izquierda y los pies del jugador van sobre su cara de arriba.
    //  - 6 verticales (E4A0) que recorren una columna de salas, 2 px/tick, y siguen moviéndose aunque el jugador esté en
    //    otra sala o en un mundo. Al pasar de y = 0x28 subiendo entran por abajo en la sala de arriba (y = 0xB9); al
    //    llegar a 0xC0 bajando, por arriba en la de abajo (y = 0x28). Dan la vuelta en su sala de arriba y de abajo.
    //  - 15 horizontales de una sala (EC20), que vuelven a su sitio al entrar en ella.
    // Orden de un tick del original, medido con un punto de ruptura en b03:B0C2 durante la demo 0: se mueve el jugador,
    // se mira la colisión (con el ascensor aún donde estaba) y, en el frame siguiente, se mueven los ascensores y lo
    // llevan. PlayerMotor llama a Collide y Move.
    public class LiftSystem
    {
        public class Lift
        {
            public int Id;                 // vertical: 6..1; horizontal: 0
            public bool Vertical;
            public int Y256, X256, Speed;  // 8.8 como en el original
            public int Delta;              // lo que se ha movido en x este tick (horizontal)
            public bool Riding;            // bit 0 de EC12/EC22: el jugador va encima
            public int Cooldown;           // bits 3-6: ticks sin colisión tras bajarse
            public bool Aboard;            // bit 2: ha cambiado de sala con el jugador encima
            public int Index;              // vertical: posición en su lista de salas
            public ShaftInfo Shaft;
            public SlideInfo Slide;
            public int X => (X256 >> 8) & 0xFF;
            public int Y => (Y256 >> 8) & 0xFF;
            public int RoomId => Vertical ? Shaft.rooms[Index] : Slide.room;
            public Lift MemberwiseCloneLift() => (Lift)MemberwiseClone();
        }

        readonly LiftsData data;
        List<Lift> shafts = new List<Lift>();
        Lift slide;
        int roomId = -1;
        int forcedUp;                      // E52C/E52D: saltó desde ese vertical y salió por arriba
        bool skipMove, slideFresh;         // el tick en que se cambia de sala no se mueve nada (se carga la sala nueva)

        // Solo hay ascensores en el castillo (E041 = 1). Los verticales se mueven siempre.
        public bool InCastle = true;
        public LiftBox Box => data.box;
        public IReadOnlyList<Lift> Shafts => shafts;

        public LiftSystem(LiftsData data)
        {
            this.data = data ?? new LiftsData();
            Reset();
        }

        // Copia completa (búsquedas de recorrido, Walkthrough).
        public LiftSystem Clone()
        {
            var c = (LiftSystem)MemberwiseClone();
            c.shafts = new List<Lift>(shafts.Count);
            foreach (var s in shafts) c.shafts.Add(s.MemberwiseCloneLift());
            c.slide = slide?.MemberwiseCloneLift();
            return c;
        }

        // Estado de los ascensores que importa en esa sala (los de otras salas no tocan al jugador hasta que llegan).
        public ulong StateKey(int room)
        {
            ulong k = (ulong)(roomId & 511) | (skipMove ? 1UL : 0) << 9 | (slideFresh ? 1UL : 0) << 10 | (ulong)(forcedUp & 7) << 11;
            var s = ShaftInRoom(room);
            if (s != null)
                k |= 1UL << 14 | (ulong)(s.Index & 7) << 15 | (ulong)(s.Y256 & 0xFFFF) << 18 | (s.Speed < 0 ? 1UL : 0) << 34
                     | (s.Riding ? 1UL : 0) << 35 | (ulong)(s.Cooldown & 7) << 36 | (s.Aboard ? 1UL : 0) << 39;
            var h = SlideInRoom;
            if (h != null && h.Slide.room == room)
                k |= 1UL << 40 | (ulong)((h.X256 >> 8) & 0xFF) << 41 | (ulong)(h.X256 & 0xFF) << 49 | (h.Speed < 0 ? 1UL : 0) << 57
                     | (h.Riding ? 1UL : 0) << 58 | (ulong)(h.Cooldown & 7) << 59;
            return k;
        }

        // Pone el vertical de esa columna entrando en la sala (bajando por arriba o subiendo por abajo), como si el
        // jugador lo hubiera esperado. false si no pasa por ahí en ese sentido.
        public bool ArriveShaft(int room, bool down)
        {
            foreach (var s in shafts)
            {
                int i = System.Array.IndexOf(s.Shaft.rooms, room);
                if (i < 0) continue;
                if (down ? room == s.Shaft.topRoom : room == s.Shaft.bottomRoom) return false;
                s.Index = i;
                s.Y256 = (down ? data.box.enterFromAboveY : data.box.enterFromBelowY) << 8;
                s.Speed = down ? System.Math.Abs(s.Speed) : -System.Math.Abs(s.Speed);
                s.Riding = s.Aboard = false;
                s.Cooldown = 0;
                return true;
            }
            return false;
        }

        // Partida nueva (b01:63C2): cada vertical en su sala de arriba, bajando.
        public void Reset()
        {
            shafts.Clear();
            foreach (var s in data.shafts)
                shafts.Add(new Lift { Id = s.id, Vertical = true, Shaft = s, Index = s.startIndex, Y256 = s.y << 8, X256 = s.x << 8, Speed = s.speed });
            slide = null;
            roomId = -1;
            forcedUp = 0;
        }

        // La demo 0 empieza en la sala 146 sobre el ascensor 6, que sube (b00:5FA8).
        public void PresetForStart(int room)
        {
            var p = data.demo0;
            if (p == null || room != p.room) return;
            var s = shafts.Find(l => l.Id == p.id);
            if (s == null) return;
            s.Index = p.index;
            s.Y256 = p.y << 8;
            s.Speed = p.speed;
        }

        public Lift ShaftInRoom(int room) => InCastle ? shafts.Find(l => l.RoomId == room) : null;
        public Lift SlideInRoom => InCastle ? slide : null;

        // Los ascensores de la sala actual (vertical primero, como EC10 y EC20).
        public IEnumerable<Lift> InRoom(int room)
        {
            var s = ShaftInRoom(room);
            if (s != null) yield return s;
            if (SlideInRoom != null && slide.Slide.room == room) yield return slide;
        }

        // A8CE: el jugador va sobre un ascensor de la sala.
        public bool Riding(int room)
        {
            foreach (var l in InRoom(room))
                if (l.Riding) return true;
            return false;
        }

        // Id del vertical sobre el que va (para el salto que sale por arriba), 0 si ninguno.
        public int RidingShaft(int room)
        {
            var s = ShaftInRoom(room);
            return s != null && s.Riding ? s.Id : 0;
        }

        // A7F8: saltó desde ese vertical y la y bajó de 0x28: sale por arriba y el ascensor, si sube, lo sigue.
        public void PlayerJumpedOut(int shaftId) => forcedUp = shaftId;

        // Al entrar en una sala (b01:63EA): el horizontal vuelve a su sitio. Si no se ha llegado con un vertical, el
        // jugador ya no va encima de ninguno.
        void EnterRoom(int room, bool keepRiding)
        {
            roomId = room;
            slide = null;
            foreach (var s in shafts)
                if (!keepRiding || s.RoomId != room) { s.Riding = false; s.Aboard = false; }
            if (!InCastle) return;
            foreach (var d in data.slides)
                if (d.room == room)
                    slide = new Lift { Slide = d, Y256 = d.y << 8, X256 = d.x << 8, Speed = d.speed };
        }

        public void Move(PlayerMotor m)
        {
            if (skipMove)
            {
                skipMove = false;
                return;
            }
            if (m.Room.id != roomId) EnterRoom(m.Room.id, false);
            var box = data.box;
            // b01:643C: los verticales.
            foreach (var s in shafts)
            {
                bool inRoom = InCastle && s.RoomId == roomId;
                s.Y256 = (s.Y256 + s.Speed) & 0xFFFF;
                int y = s.Y;
                bool up = s.Speed < 0;
                if (up ? s.RoomId == s.Shaft.topRoom && y + 1 < s.Shaft.topY : s.RoomId == s.Shaft.bottomRoom && y >= s.Shaft.bottomY)
                {
                    s.Speed = -s.Speed;
                    up = !up;
                }
                if (up)
                {
                    if (!(forcedUp == s.Id) && y >= box.roomTopY) continue;
                    s.Index--;
                    y = box.enterFromBelowY;
                }
                else
                {
                    if (y < box.roomBottomY) continue;
                    s.Index++;
                    y = box.enterFromAboveY;
                }
                s.Y256 = (y << 8) | (s.Y256 & 0xFF);
                if (inRoom)
                {
                    if (m.JumpedFromShaft == s.Id) m.JumpedFromShaft = 0;
                    if (s.Riding) s.Aboard = true;
                }
            }
            forcedUp = 0;
            // b03:A7F8: el vertical ha cambiado de sala con el jugador encima: va con él.
            foreach (var s in shafts)
            {
                if (!s.Aboard) continue;
                s.Aboard = false;
                if (!InCastle) continue;
                m.RideToRoom(s.RoomId, s.Y);
                roomId = s.RoomId;
                slide = null;
                foreach (var d in data.slides)
                    if (d.room == roomId)
                        slide = new Lift { Slide = d, Y256 = d.y << 8, X256 = d.x << 8, Speed = d.speed };
                slideFresh = true;
            }
            // b01:6531: el horizontal. Da la vuelta con la x de antes de moverse (medido: 174 176 178 176…). En la sala a la
            // que se acaba de llegar con un vertical arranca en el tick siguiente (demo 0, sala 126).
            if (slideFresh)
                slideFresh = false;
            else if (InCastle && slide != null)
            {
                int oldX = slide.X;
                slide.X256 += slide.Speed;
                slide.Delta = slide.Speed;
                if (slide.Speed < 0 ? oldX <= slide.Slide.minX : oldX >= slide.Slide.maxX)
                    slide.Speed = -slide.Speed;
            }
            // b03:A916: lleva al jugador.
            foreach (var l in InRoom(roomId))
            {
                if (!l.Riding) continue;
                m.CarryOnLift(l.Vertical ? 0 : l.Delta >> 8, l.Y);
            }
        }

        // b03:B0C2, después de mover al jugador.
        public void Collide(PlayerMotor m)
        {
            if (m.Room.id != roomId)
            {
                // Ha cambiado de sala andando, saltando o por una escalera: el original deja el resto del tick para cargar
                // la sala nueva (los ascensores no se mueven mientras tanto).
                bool first = roomId < 0;
                EnterRoom(m.Room.id, false);
                if (!first)
                {
                    skipMove = true;
                    return;
                }
            }
            var b = data.box;
            foreach (var l in InRoom(roomId))
            {
                bool was = l.Riding;
                l.Riding = false;
                if (l.Cooldown > 0)
                {
                    l.Cooldown--;
                    continue;
                }
                int px = m.X, py = m.Y;
                int dy = py - l.Y;
                if (dy >= b.sideDyMin && dy <= b.sideDyMax)
                {
                    int d = px - l.X;
                    if (d >= b.pushLeftFrom && d <= b.pushLeftTo) px = l.X + b.pushLeftX;
                    else if (d >= b.pushRightFrom && d <= b.pushRightTo) px = l.X + b.pushRightX;
                }
                bool done = false;
                int dx = px - l.X;
                if (dx >= b.standDxMin && dx <= b.standDxMax)
                {
                    bool tileFloor = m.OnTileFloor;
                    if (dy >= b.standDyMin && dy <= b.standDyMax)
                    {
                        if (was || !tileFloor) l.Riding = true;
                        done = true;
                    }
                    else if (dy >= b.headDyMin && dy <= b.headDyMax)
                    {
                        if (!tileFloor) py = l.Y + b.headPushY;
                        done = true;
                    }
                }
                if (px != m.X || py != m.Y) m.PushByLift(px, py);
                if (!done && was && !l.Riding)
                    l.Cooldown = b.leaveCooldownTicks;
            }
        }
    }
}
