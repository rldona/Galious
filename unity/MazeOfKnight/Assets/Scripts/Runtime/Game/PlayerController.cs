using MazeOfKnight.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game
{
    // Une el motor con Unity: lee la entrada, ejecuta los ticks del original a 30 por segundo y coloca el
    // sprite. Controles del original: ← → andar, ↑ saltar o subir escalera, ↓ bajar escalera o entrar por
    // una puerta, espacio espada (docs/player.md). Tab cambia de personaje (en el original, desde el menú).
    public class PlayerController : MonoBehaviour
    {
        public SpriteRenderer body;
        public SpriteRenderer sword;
        public Color popolonColor = new Color(0.35f, 0.55f, 1f);
        public Color aphroditeColor = new Color(1f, 0.45f, 0.7f);

        public PlayerMotor Motor { get; private set; }
        public event System.Action Ticked;

        InputAction move, jump, attack, switchCharacter;
        float accumulator;
        bool attackLatch;   // un toque más corto que un tick no se pierde

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
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            move.AddBinding("<Gamepad>/leftStick");
            move.AddBinding("<Gamepad>/dpad");
            jump = new InputAction("Saltar", InputActionType.Button);
            jump.AddBinding("<Gamepad>/buttonSouth");
            attack = new InputAction("Espada", InputActionType.Button);
            attack.AddBinding("<Keyboard>/space");
            attack.AddBinding("<Gamepad>/buttonWest");
            switchCharacter = new InputAction("Cambiar personaje", InputActionType.Button);
            switchCharacter.AddBinding("<Keyboard>/tab");
            switchCharacter.AddBinding("<Gamepad>/select");
            move.Enable();
            jump.Enable();
            attack.Enable();
            switchCharacter.Enable();
        }

        void OnDisable()
        {
            move.Disable();
            jump.Disable();
            attack.Disable();
            switchCharacter.Disable();
        }

        void Update()
        {
            if (Motor == null)
                return;
            if (switchCharacter.WasPressedThisFrame())
                Motor.Character = Motor.Character == Character.Popolon ? Character.Aphrodite : Character.Popolon;

            if (attack.WasPressedThisFrame())
                attackLatch = true;
            var v = move.ReadValue<Vector2>();
            var input = new MotorInput
            {
                Left = v.x < -0.5f,
                Right = v.x > 0.5f,
                Up = v.y > 0.5f || jump.IsPressed(),
                Down = v.y < -0.5f,
                Attack = attack.IsPressed() || attackLatch,
            };
            accumulator += Time.deltaTime;
            float tick = 1f / OriginalPhysics.TicksPerSecond;
            int guard = 0;
            while (accumulator >= tick && guard++ < 4)
            {
                accumulator -= tick;
                Motor.Tick(input);
                attackLatch = false;
                input.Attack = attack.IsPressed();
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
                body.color = Motor.Character == Character.Popolon ? popolonColor : aphroditeColor;
                body.flipX = Motor.FacingLeft;
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
