using MazeOfKnight.Data;

namespace MazeOfKnight.Game
{
    // Reproduce una demo grabada (b15:BAFD; docs/attract.md): cada entrada mantiene sus teclas durante 'frames' ticks
    // (contando el tick en que se carga) y las teclas FF acaban la demo. Las teclas llegan al motor como si vinieran
    // del teclado. [I] El azar del original no se siembra y el remake no es idéntico, así que la demo puede torcerse.
    public class DemoPlayer
    {
        readonly DemoInfo demo;
        int entry = -1, left;
        public bool Finished { get; private set; }
        public bool FirePressed { get; private set; }
        bool fireHeld;

        public DemoPlayer(DemoInfo demo) => this.demo = demo;

        // Las teclas del tick siguiente.
        public MotorInput Next()
        {
            FirePressed = false;
            if (Finished) return default;
            if (left <= 0)
            {
                entry++;
                if (entry >= demo.inputs.Length || demo.inputs[entry].keys == 0xFF) { Finished = true; return default; }
                left = demo.inputs[entry].frames;
            }
            left--;
            var e = demo.inputs[entry];
            FirePressed = e.m && !fireHeld;
            fireHeld = e.m;
            return new MotorInput { Left = e.left, Right = e.right, Up = e.up, Down = e.down, Attack = e.space };
        }
    }
}
