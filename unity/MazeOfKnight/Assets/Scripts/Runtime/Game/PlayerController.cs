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
        // RemakeRules.HollowBenches: sentado en un banco (sin moverse) y si se ha pedido levantarse.
        public bool Seated, WantsToStand;
        public SpriteRenderer sword;
        // Estilo del remake (Visuals/Atmosphere): fotogramas dibujados y la estela del espadazo. Null = provisional.
        public Visuals.CharacterArt Art;
        public SpriteRenderer Slash;
        SpriteRenderer shieldOverlay;        // capa del escudo dibujado (CharacterArt.DrawnShield)
        Sprite placeholder, slashDefault;
        public Color popolonColor = new Color(0.35f, 0.55f, 1f);
        public Color aphroditeColor = new Color(1f, 0.45f, 0.7f);

        public PlayerMotor Motor { get; private set; }
        public bool Paused;              // menú de objetos abierto: no corren los ticks
        public bool ReverseControls;     // mundo 8 sin Hoi: izquierda y derecha al revés
        public DemoPlayer Demo;          // demo en curso: las teclas salen de la grabación
        public event System.Action DemoInterrupted;   // una tecla de juego durante la demo
        public event System.Action Ticked;
        // Celebración tras coger la Gran Llave (BossEffects): saltitos en el sitio, girándose en cada aterrizaje.
        public float HopPx;
        // Cambio de sala: el original deja de jugar mientras dibuja la sala nueva (estado 10).
        int roomPause;
        // RemakeRules.HitStop: ticks que el juego se queda parado tras acertar un golpe.
        int hitStop;
        public void HitStop()
        {
            if (RemakeRules.HitStop && Demo == null) hitStop = Mathf.Max(hitStop, RemakeRules.HitStopTicks);
        }
        public bool HopFlip;

        InputAction move, jump, attack, fire, focus;
        // RemakeRules.HollowSoul: el botón de concentrarse, mantenido.
        public bool FocusHeld => DebugHoldFocus || focus != null && focus.IsPressed() && Demo == null;
        public bool DebugHoldFocus;          // para las capturas
        // Concentrándose: no se mueve (como sentado).
        public bool Focusing;
        bool fireLatch;
        // Arma secundaria (M): una pulsación nueva por disparo; el arranque la consume en su tick.
        public bool ConsumeFire() { bool f = fireLatch; fireLatch = false; return f; }

        float accumulator;
        // El original lee las teclas con un tick de retraso (trazas de las demos en openMSX): el motor recibe en cada
        // tick lo que se pulsó en el anterior.
        MotorInput delayed;
        bool attackLatch;   // un toque más corto que un tick no se pierde
        // ↑ y ↓ igual: una pulsación entre dos ticks llega al siguiente tick como "pulsada" (al menos ese tick) y como
        // pulsación nueva aunque ya se haya soltado o se soltara y volviera a pulsar entre medias.
        bool upLatch, downLatch, upWasDown, downWasDown;

        public void Init(PlayerMotor motor)
        {
            Motor = motor;
            // La demo siempre la lleva: sin ella se desincroniza de la grabación.
            Motor.RoomChanged += (_, __) =>
                roomPause = RemakeRules.NoRoomChangePause && Demo == null ? 0 : OriginalPhysics.RoomChangePauseTicks;
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
            focus = new InputAction("Concentrarse", InputActionType.Button);
            focus.AddBinding("<Keyboard>/f");
            focus.AddBinding("<Gamepad>/buttonEast");
            focus.Enable();
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
            if (Demo != null)
            {
                if (move.WasPressedThisFrame() || attack.WasPressedThisFrame() || fire.WasPressedThisFrame() || jump.WasPressedThisFrame())
                {
                    DemoInterrupted?.Invoke();
                    return;
                }
                accumulator += Time.deltaTime;
                float dt = 1f / OriginalPhysics.TicksPerSecond;
                int g = 0;
                while (accumulator >= dt && g++ < 4)
                {
                    accumulator -= dt;
                    if (roomPause > 0) { roomPause--; continue; }   // la demo tampoco avanza mientras se carga la sala
                    var di = Demo.Next();
                    if (Demo.FirePressed) fireLatch = true;
                    Motor.Tick(delayed);
                    delayed = di;
                    Ticked?.Invoke();
                    if (Demo == null) break;
                }
                if (g >= 4) accumulator = 0;
                UpdateVisual();
                return;
            }
            var v = move.ReadValue<Vector2>();
            bool up = v.y > 0.5f || jump.IsPressed(), down = v.y < -0.5f;
            if (up && !upWasDown) upLatch = true;
            if (down && !downWasDown) downLatch = true;
            upWasDown = up;
            downWasDown = down;
            // Sentado en un banco: el jugador no se mueve; cualquier dirección, salto o espada lo levanta.
            if (Focusing && !Seated)
            {
                upLatch = downLatch = attackLatch = false;
                v = Vector2.zero;
                up = down = false;
            }
            if (Seated)
            {
                if (v.x < -0.5f || v.x > 0.5f || upLatch || attack.WasPressedThisFrame()) WantsToStand = true;
                upLatch = downLatch = attackLatch = false;
                v = Vector2.zero;
                up = down = false;
            }
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
                if (roomPause > 0) { roomPause--; continue; }
                if (hitStop > 0) { hitStop--; continue; }   // las pulsaciones se guardan para cuando siga
                Motor.Tick(delayed);
                delayed = input;
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

        // La estela usa el material aditivo, cuyo shader no aplica flipX (Unity gira los sprites en el shader): se gira
        // con la escala, alrededor de su pivote (los pies, en el tajo dibujado).
        void FaceSlash(bool left)
        {
            Slash.flipX = false;
            Slash.transform.localScale = new Vector3(left ? -1 : 1, 1, 1);
        }

        void UpdateVisual()
        {
            // El sprite de 16 x 16 px tiene el pivote abajo en el centro: coincide con los pies del original.
            transform.position = RoomCoords.PixelToWorld(Motor.Room, Motor.X, Motor.Y) + new Vector2(0, HopPx / RoomCoords.PixelsPerUnit);
            if (body != null && placeholder == null) placeholder = body.sprite;
            // Sin fotograma (arte solo dibujado y sin clip): el cuadro provisional.
            var frame = Art != null && body != null ? Art.Frame(Motor, hitStop > 0 ? 0 : Time.deltaTime) : null;
            if (frame != null)
            {
                // La caja del jugador va de x − 6 a x + 6 (13 px, centro en x + 0,5): el dibujo, centrado en las piernas,
                // se corre medio píxel para quedar en medio de ella (si no, queda medio píxel a la izquierda).
                transform.position += new Vector3(0.5f / RoomCoords.PixelsPerUnit, 0, 0);
                body.sprite = frame;   // (con el juego parado por un golpe, la animación también se para)
                body.color = Color.white;
                body.flipX = Motor.FacingLeft ^ HopFlip;
                body.transform.localScale = Vector3.one;
                if (sword != null) sword.enabled = false;
                // Escudo dibujado en una capa aparte (el arte no repite el cuerpo por cada metal), en el mismo fotograma.
                var shieldSprite = Art.DrawnShield(Motor);
                if (shieldSprite != null)
                {
                    if (shieldOverlay == null)
                    {
                        shieldOverlay = new GameObject("Escudo").AddComponent<SpriteRenderer>();
                        shieldOverlay.transform.SetParent(body.transform, false);
                        shieldOverlay.sortingOrder = body.sortingOrder + 1;
                    }
                    shieldOverlay.enabled = true;
                    shieldOverlay.sprite = shieldSprite;
                    shieldOverlay.flipX = body.flipX;
                    shieldOverlay.sharedMaterial = body.sharedMaterial;
                }
                else if (shieldOverlay != null) shieldOverlay.enabled = false;
                var drawnSlash = Slash != null ? Art.DrawnSlash(Motor) : null;
                if (Slash != null && slashDefault == null && drawnSlash == null) slashDefault = Slash.sprite;
                if (drawnSlash != null)
                {
                    // El tajo dibujado va en el lienzo del cuerpo: mismo sitio y mismo pivote.
                    Slash.enabled = true;
                    Slash.sprite = drawnSlash;
                    Slash.transform.position = body.transform.position;
                    FaceSlash(Motor.FacingLeft);
                    Slash.color = Color.white;
                }
                else if (Slash != null)
                {
                    if (slashDefault != null) Slash.sprite = slashDefault;
                    // Estela de la estocada sobre la zona que golpea (x + 8 … x + 18, y − 7 … y − 4).
                    Slash.enabled = Motor.AttackTicks > 0;
                    if (Slash.enabled)
                    {
                        var (sx0, sx1, sy0, sy1) = Motor.SwordRect();
                        var at = RoomCoords.PixelToWorld(Motor.Room, (sx0 + sx1 + 1) / 2, (sy0 + sy1) / 2 + 1);
                        Slash.transform.position = new Vector3(at.x, at.y, 0);
                        FaceSlash(Motor.FacingLeft);
                        // Fuerte en el tick del golpe y se apaga al recoger la hoja.
                        float a = Motor.AttackTicks >= 4 ? 1f : Motor.AttackTicks == 3 ? 0.7f : Motor.AttackTicks == 2 ? 0.3f : 0f;
                        Slash.color = new Color(1, 1, 1, a);
                    }
                }
                return;
            }
            if (Slash != null) Slash.enabled = false;
            if (shieldOverlay != null) shieldOverlay.enabled = false;
            if (body != null)
            {
                if (placeholder != null) body.sprite = placeholder;
                var color = Motor.Character == Character.Popolon ? popolonColor : aphroditeColor;
                // El original cambia de sprite en la escalera; aquí, provisionalmente, se oscurece.
                body.color = Motor.State == MotorState.Ladder ? Color.Lerp(color, Color.black, 0.45f) : color;
                body.flipX = Motor.FacingLeft ^ HopFlip;
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
