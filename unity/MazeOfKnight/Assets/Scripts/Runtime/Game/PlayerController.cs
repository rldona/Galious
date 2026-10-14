using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game
{
    // Une el motor con Unity: lee la entrada, ejecuta los ticks del original a 30 por segundo y coloca el
    // sprite. Controles del original: ← → andar, ↑ saltar o subir escalera, ↓ bajar escalera o entrar por
    // una puerta, espacio espada, M arma (docs/player.md). El personaje se cambia en el menú de objetos (F1).
    public class PlayerController : MonoBehaviour
    {
        public SpriteRenderer body;
        public SpriteRenderer sword;
        public Color popolonColor = new Color(0.35f, 0.55f, 1f);
        public Color aphroditeColor = new Color(1f, 0.45f, 0.7f);

        public PlayerMotor Motor { get; private set; }
        public bool Paused;              // menú de objetos abierto: no corren los ticks
        public bool ReverseControls;     // mundo 8 sin Hoi: izquierda y derecha al revés
        public event System.Action Ticked;

        InputAction move, jump, attack, fire;
        bool fireLatch;
        // Arma secundaria (M): una pulsación nueva por disparo; el arranque la consume en su tick.
        public bool ConsumeFire() { bool f = fireLatch; fireLatch = false; return f; }

        float accumulator;
        bool attackLatch;   // un toque más corto que un tick no se pierde
        // ↑ y ↓ igual: una pulsación entre dos ticks llega al siguiente tick como "pulsada" (al menos ese tick) y como
        // pulsación nueva aunque ya se haya soltado o se soltara y volviera a pulsar entre medias.
        bool upLatch, downLatch, upWasDown, downWasDown;

        public void Init(PlayerMotor motor)
        {
            Motor = motor;
            UpdateVisual();
        }

        void OnEnable()
        {
            move = new InputAction("Mover", InputActionType.Value);
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            move.AddBinding("<Gamepad>/leftStick");
            move.AddBinding("<Gamepad>/dpad");
            jump = new InputAction("Saltar", InputActionType.Button);
            jump.AddBinding("<Gamepad>/buttonSouth");
            attack = new InputAction("Espada", InputActionType.Button);
            attack.AddBinding("<Keyboard>/space");
            attack.AddBinding("<Gamepad>/buttonWest");
            fire = new InputAction("Arma", InputActionType.Button);
            fire.AddBinding("<Keyboard>/m");
            fire.AddBinding("<Gamepad>/buttonNorth");
            fire.Enable();
            move.Enable();
            jump.Enable();
            attack.Enable();
        }

        void OnDisable()
        {
            move.Disable();
            fire.Disable();
            jump.Disable();
            attack.Disable();
        }

        void Update()
        {
            if (Motor == null)
                return;
            if (Paused)
            {
                accumulator = 0;
                UpdateVisual();
                return;
            }

            if (attack.WasPressedThisFrame())
                attackLatch = true;
            if (fire.WasPressedThisFrame())
                fireLatch = true;
            var v = move.ReadValue<Vector2>();
            bool up = v.y > 0.5f || jump.IsPressed(), down = v.y < -0.5f;
            if (up && !upWasDown) upLatch = true;
            if (down && !downWasDown) downLatch = true;
            upWasDown = up;
            downWasDown = down;
            var input = new MotorInput
            {
                Left = ReverseControls ? v.x > 0.5f : v.x < -0.5f,
                Right = ReverseControls ? v.x < -0.5f : v.x > 0.5f,
                Up = up || upLatch,
                Down = down || downLatch,
                UpPressed = upLatch,
                DownPressed = downLatch,
                Attack = attack.IsPressed() || attackLatch,
            };
            accumulator += Time.deltaTime;
            float tick = 1f / OriginalPhysics.TicksPerSecond;
            int guard = 0;
            while (accumulator >= tick && guard++ < 4)
            {
                accumulator -= tick;
                Motor.Tick(input);
                attackLatch = upLatch = downLatch = false;
                input.Attack = attack.IsPressed();
                input.Up = up;
                input.Down = down;
                input.UpPressed = input.DownPressed = false;
                Ticked?.Invoke();
            }
            if (guard >= 4)
                accumulator = 0;
            UpdateVisual();
        }

        void UpdateVisual()
        {
            // El sprite de 16 x 16 px tiene el pivote abajo en el centro: coincide con los pies del original.
            transform.position = RoomCoords.PixelToWorld(Motor.Room, Motor.X, Motor.Y);
            if (body != null)
            {
                var color = Motor.Character == Character.Popolon ? popolonColor : aphroditeColor;
                // El original cambia de sprite en la escalera; aquí, provisionalmente, se oscurece.
                body.color = Motor.State == MotorState.Ladder ? Color.Lerp(color, Color.black, 0.45f) : color;
                body.flipX = Motor.FacingLeft;
                // Preparando el salto el original cambia de sprite (mueve los pies): aquí, provisionalmente, se agacha.
                body.transform.localScale = new Vector3(1, Motor.State == MotorState.JumpPrep ? 0.85f : 1, 1);
            }
            if (sword != null)
            {
                // La espada: una barra en la zona que golpea, mientras dura el golpe.
                sword.enabled = Motor.AttackTicks > 0;
                var (x0, x1, y0, y1) = Motor.SwordRect();
                int midY = (y0 + y1) / 2;
                sword.transform.position = RoomCoords.PixelToWorld(Motor.Room, (x0 + x1 + 1) / 2, midY + 1);
                sword.flipX = Motor.FacingLeft;
            }
        }
    }
}
