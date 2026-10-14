namespace MazeOfKnight.Game
{
    // Objetos que aparecen con una condición en el castillo (b02:9F82; docs/entities.md, "Objetos que aparecen con una
    // condición"). Como mucho uno por visita a la sala (E539). Suena 0x2E.
    //   48 Hoi y 75 Candle: matar a todos los marcados de la sala (E61C).
    //   24 Saber: igual, y el jefe del mundo 6 vencido (bit 2 de E068).
    //   40 Vase: jefe del mundo 4 vencido (bit 2 de E066) y 9 pasos (E536): 3 espadazos mirando a la derecha, 3 mirando
    //      a la izquierda y 3 saltos; uno fuera de orden pone la cuenta a 0. No mira dónde está el jugador.
    //   43 Dagger: sobre el ascensor, pulsar →, ↓, ←, ↑ (tabla A056); otra tecla reinicia.
    public class CastleItems
    {
        public struct Spot
        {
            public int Room, Type, X, Y;
            public Spot(int room, int type, int x, int y) { Room = room; Type = type; X = x; Y = y; }
        }

        public static readonly Spot[] Spots =
        {
            new Spot(48, 0x13, 0x50, 0x28), new Spot(75, 0x16, 0x20, 0xA8), new Spot(24, 0x25, 0xD0, 0x68),
            new Spot(40, 0x1B, 0x80, 0x88), new Spot(43, 0x26, 0x60, 0x28),
        };

        int room = -1;
        bool done;               // E539: ya ha salido (o no puede salir) en esta visita
        public int VaseSteps { get; private set; }
        public int DaggerStep { get; private set; }
        int lastKeys;
        MotorState lastState;

        const int Right = 1, Down = 2, Left = 4, Up = 8, Other = 16;
        static readonly int[] DaggerKeys = { Right, Down, Left, Up };

        // Un tick en el castillo. cleared: la sala tiene ya su evento (E61C); boss(w): jefe del mundo w vencido;
        // has(t): ya se tiene el objeto. Devuelve el objeto que aparece, o null.
        public Spot? Tick(PlayerMotor m, bool cleared, System.Func<int, bool> boss, System.Func<int, bool> has)
        {
            if (m.Room.id != room)
            {
                room = m.Room.id;
                done = false;
                VaseSteps = DaggerStep = 0;
                lastKeys = Keys(m.LastInput);
                lastState = m.State;
            }
            var inp = m.LastInput;
            int keys = Keys(inp), pressed = keys & ~lastKeys;
            lastKeys = keys;
            bool jumped = m.State == MotorState.JumpPrep && lastState != MotorState.JumpPrep;
            lastState = m.State;
            if (done) return null;
            Spot? spot = null;
            foreach (var s in Spots) if (s.Room == room) spot = s;
            if (spot == null) return null;
            var p = spot.Value;
            if (has(p.Type)) { done = true; return null; }
            bool ok = false;
            switch (room)
            {
                case 48:
                case 75: ok = cleared; break;
                case 24: ok = cleared && boss(6); break;
                case 40:
                    if (!boss(4)) break;
                    if (m.AttackStartedThisTick)
                        VaseSteps = VaseSteps < 3 && !m.FacingLeft || VaseSteps >= 3 && VaseSteps < 6 && m.FacingLeft ? VaseSteps + 1 : 0;
                    if (jumped)
                        VaseSteps = VaseSteps >= 6 && VaseSteps < 9 ? VaseSteps + 1 : 0;
                    ok = VaseSteps >= 9;
                    break;
                case 43:
                    if (!m.OnLift || pressed == 0) break;
                    DaggerStep = pressed == DaggerKeys[DaggerStep] ? DaggerStep + 1 : pressed == DaggerKeys[0] ? 1 : 0;
                    ok = DaggerStep >= DaggerKeys.Length;
                    break;
            }
            if (!ok) return null;
            done = true;
            return p;
        }

        static int Keys(MotorInput i) =>
            (i.Right ? Right : 0) | (i.Down ? Down : 0) | (i.Left ? Left : 0) | (i.Up ? Up : 0) | (i.Attack ? Other : 0);
    }
}
