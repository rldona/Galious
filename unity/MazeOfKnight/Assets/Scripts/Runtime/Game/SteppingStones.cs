namespace MazeOfKnight.Game
{
    // Piedras que aparecen para cruzar el agua (objeto de clase 6, mundo 5, sala 11; b09:A52E–A62F; docs/entities.md).
    //  - Esperando en el borde de una orilla (pies en y = 0x78, x en 0x18…0x1F a la izquierda o 0xE0…0xE7 a la derecha)
    //    300 pasos, sale la primera piedra, la más cercana a esa orilla.
    //  - Tras 128 pasos encima de una piedra (x de la piedra − 7 … + 23, pies en y = 0x78) sale la siguiente y se va la
    //    anterior. Piedras en x = 0x38, 0x60, 0x90 y 0xB8 (tabla A630), en y = 0x78, de 2 × 2 tiles. Suena 0x04.
    //  - Tras la cuarta, también se va la última; al llegar a la otra orilla y quedarse 128 pasos, se vuelve a armar.
    // [I] Un paso por tick del juego (la guía habla de unos 15 s de espera; 300 ticks son 10 s).
    public class SteppingStones
    {
        static readonly int[] StoneX = { 0x38, 0x60, 0x90, 0xB8 };
        const int Y = 0x78;

        int phase;            // EAB1: 0 esperando; 1–4 piedras; 5 hacia la otra orilla
        int side;             // EAB2: 0 desde la izquierda, 1 desde la derecha
        int index;            // EAB5
        int timer = 300;      // EAB3/EAB4
        bool onStone;         // EAB6

        public int? Current { get; private set; }     // x de la piedra que hay ahora (EAB8)
        public event System.Action<int> Appeared, Vanished;

        public void Tick(int px, int py)
        {
            if (phase == 0)
            {
                if (py != Y) return;
                int s = px - 0x18 >= 0 && px - 0x18 < 8 ? 0 : px - 0xE0 >= 0 && px - 0xE0 < 8 ? 1 : -1;
                if (s < 0) return;
                side = s;
                index = s == 0 ? 0 : 3;
                if (--timer > 0) return;
                Show(StoneX[index]);
                return;
            }
            if (onStone && --timer < 0) timer = 0;
            if (phase == 5)
            {
                if (side == 0 ? px < 0xD9 : px >= 0x18) return;
                onStone = true;
                if (timer > 0) return;
                timer = 300;
                phase = 0;
                onStone = false;
                return;
            }
            if (Current == null || py != Y || (uint)(px - Current.Value + 7) >= 0x1F) return;
            onStone = true;
            if (timer > 0) return;
            Vanished?.Invoke(Current.Value);
            if (phase == 4)
            {
                Current = null;
                timer = 0x80;
                onStone = false;
                phase++;
                return;
            }
            index += side == 0 ? 1 : -1;
            Show(StoneX[index]);
        }

        void Show(int x)
        {
            Current = x;
            Appeared?.Invoke(x);
            timer = 0x80;
            onStone = false;
            phase++;
        }
    }
}
