using System;
using System.Collections.Generic;
using MazeOfKnight.Data;
using UnityEngine;
using static MazeOfKnight.Game.Visuals.ShapePainter;

namespace MazeOfKnight.Game.Visuals
{
    // Popolon y Aphrodite del remake (docs/design_bible/12_estilo_visual.md, "Personajes"): arte provisional pintado
    // por código a 4 texels por píxel del original, con contorno de tinta y siluetas propias:
    //   - Popolon: yelmo corintio claro (la "máscara", con rendija en T), cresta roja transversal, escudo redondo de
    //     bronce, capa granate.
    //   - Aphrodite: yelmo con alas, melena dorada, escudo de plata con una rosa, capa violeta.
    // Fotogramas: quieto (2, respira), andar (4), preparar el salto, subir, caer, escalera (2), espadazo (2) y golpe.
    // Solo aspecto: la caja del jugador y la de la espada son las del original (los pies en el pivote; la hoja llega
    // a x + 18, a la altura y − 7 … y − 4 de la zona que golpea).
    public class CharacterArt
    {
        public enum Anim { Idle, Walk, Prep, Rise, Fall, Ladder, Attack, Hurt }

        const int S = 4;                          // texels por píxel del original
        const int Wpx = 48, Hpx = 32;             // lienzo en píxeles del original
        const float FootY = 2f;                   // los pies, 2 px por encima del borde de abajo
        static readonly int W = Wpx * S, H = Hpx * S;

        readonly Dictionary<(Character, int, Anim, int), Sprite> frames = new Dictionary<(Character, int, Anim, int), Sprite>();
        readonly HashSet<(Character, int)> built = new HashSet<(Character, int)>();
        public readonly Sprite SlashSprite;
        // Escudo que lleva (Inventory.ShieldLevel: 0 ninguno, 1 cobre, 2 plata, 3 oro). Sin escudo se ve el brazo.
        public Func<int> ShieldLevel;

        // Animación
        int lastX, lastY;
        float walk, idle;
        RoomModel lastRoom;

        public CharacterArt()
        {
            Build(Character.Popolon, 0);
            Build(Character.Aphrodite, 0);
            SlashSprite = ToSprite(Slash(), 128f / (12f / RoomCoords.PixelsPerUnit), new Vector2(0.5f, 0.5f));   // 12 px de largo
        }

        // Los fotogramas de un personaje con un escudo (se pintan la primera vez que hacen falta).
        void Build(Character c, int shield)
        {
            if (!built.Add((c, shield))) return;
            void Add(Anim a, int i, Pose p) =>
                frames[(c, shield, a, i)] = ToSprite(Paint(c, shield, p), S * RoomCoords.PixelsPerUnit, new Vector2(0.5f, FootY / Hpx));
            {
                Add(Anim.Idle, 0, new Pose());
                Add(Anim.Idle, 1, new Pose { Bob = -0.35f, Cape = 0.3f });
                for (int i = 0; i < 4; i++)
                {
                    float ph = i * Mathf.PI / 2;
                    Add(Anim.Walk, i, new Pose
                    {
                        FootFront = 1.8f * Mathf.Sin(ph), FootBack = -1.8f * Mathf.Sin(ph),
                        LiftFront = Mathf.Max(0, Mathf.Cos(ph)) * 1.2f, LiftBack = Mathf.Max(0, -Mathf.Cos(ph)) * 1.2f,
                        Bob = -0.4f * Mathf.Abs(Mathf.Cos(ph)), Cape = 0.8f + 0.3f * Mathf.Sin(ph * 2), Lean = 0.3f,
                    });
                }
                Add(Anim.Prep, 0, new Pose { Crouch = 1.4f, FootFront = 1, FootBack = -1, Cape = 0.2f });
                Add(Anim.Rise, 0, new Pose { FootFront = 1.2f, LiftFront = 2.2f, FootBack = -1.4f, LiftBack = 1.0f, Cape = -1.2f, Lean = 0.2f });
                Add(Anim.Fall, 0, new Pose { FootFront = 1.6f, LiftFront = 0.4f, FootBack = -1.8f, LiftBack = 0.2f, Cape = 1.8f, CapeLift = 2.2f });
                Add(Anim.Ladder, 0, new Pose { Ladder = true, ArmPhase = 0 });
                Add(Anim.Ladder, 1, new Pose { Ladder = true, ArmPhase = 1 });
                // Estocada con impulso (la espada del original golpea en una franja horizontal, x + 8 … x + 18):
                // lanzarse con el pie adelantado y la hoja extendida, recoger a media distancia y quedar en guardia.
                Add(Anim.Attack, 0, new Pose { Sword = 1, Lean = 1.6f, Crouch = 0.5f, FootFront = 2.8f, FootBack = -2.4f, Cape = 1.9f, CapeLift = 0.6f });
                Add(Anim.Attack, 1, new Pose { Sword = 3, Lean = 0.9f, Crouch = 0.25f, FootFront = 2.0f, FootBack = -1.8f, Cape = 1.3f, CapeLift = 0.3f });
                Add(Anim.Attack, 2, new Pose { Sword = 2, Lean = 0.4f, FootFront = 1.4f, FootBack = -1.2f, Cape = 0.8f });
                Add(Anim.Hurt, 0, new Pose { Lean = -1.2f, Crouch = 0.6f, FootFront = 0.8f, FootBack = -1.6f, Cape = 1.5f, CapeLift = 1.5f, Hurt = true });
            }
        }

        // La parte del fotograma donde está el cuerpo (para retratos): sin el sitio de la espada.
        public static readonly Rect PortraitCrop = new Rect(14f / Wpx, 0.02f, 20f / Wpx, 20f / Hpx);

        public Sprite Get(Character c, Anim a, int i, int shield = 0)
        {
            shield = Mathf.Clamp(shield, 0, 3);
            Build(c, shield);
            return frames.TryGetValue((c, shield, a, i), out var s) ? s : frames[(c, shield, Anim.Idle, 0)];
        }

        // ---------- arte dibujado ----------

        // Clips dibujados a mano (ArtLibrary), por nombre: <personaje>_<accion>, con <personaje> popolon o aphrodite y
        // <accion> quieto, correr, girar, salto_preparar, salto_subir, salto_caer, escalera, estocada (fotograma
        // "golpe" en el tick del golpe), golpe (recibido), muerte y celebrar. Si falta uno, se usa el generado.
        public ArtLibrary Drawn;
        readonly FrameAnimator anim = new FrameAnimator();
        static readonly HashSet<string> OneShot = new HashSet<string> { "girar", "salto_preparar", "estocada", "golpe", "muerte", "celebrar" };
        int lastAttackTicks;
        bool lastFacingLeft, facingKnown;
        string drawnAction;

        static string Prefix(Character c) => c == Character.Popolon ? "popolon" : "aphrodite";

        FrameClip DrawnClip(Character c, string action)
        {
            var clip = Drawn?.Clip($"{Prefix(c)}_{action}");
            if (clip != null && OneShot.Contains(action)) clip.Loop = false;
            return clip;
        }

        // Acción que pide el estado del motor (la misma lógica que el arte generado) y si se mueve por el suelo.
        static string Action(PlayerMotor m, int dx)
        {
            if (m.State == MotorState.Knockback || m.Stunned > 0) return "golpe";
            if (m.AttackTicks > 0) return "estocada";
            switch (m.State)
            {
                case MotorState.Ladder: return "escalera";
                case MotorState.JumpPrep: return "salto_preparar";
                case MotorState.Jump: return "salto_subir";
                case MotorState.Fall: return "salto_caer";
            }
            return dx > 0 && dx < 20 || m.State == MotorState.Carried ? "correr" : "quieto";
        }

        // El fotograma dibujado que toca, o null si no hay clip para esta acción. La estocada arranca en su fotograma
        // "golpe" justo en el tick del golpe del original; las acciones de una vez (girar, estocada…) se terminan de ver
        // si el personaje sigue en el suelo. La escalera avanza con la y, como el original, no con el tiempo.
        Sprite DrawnFrame(PlayerMotor m, float dt, int dx)
        {
            if (Drawn == null || Drawn.Count == 0) return null;
            var c = m.Character;
            string action = Action(m, dx);
            bool grounded = action is "correr" or "quieto";
            bool newAttack = m.AttackTicks > 0 && lastAttackTicks == 0;
            lastAttackTicks = m.AttackTicks;
            bool turned = facingKnown && m.FacingLeft != lastFacingLeft && grounded;
            facingKnown = true;
            lastFacingLeft = m.FacingLeft;

            if (newAttack && DrawnClip(c, "estocada") is var hit && hit != null)
            {
                anim.PlayAt(hit, Math.Max(0, hit.TagFrame("golpe")));
                drawnAction = "estocada";
            }
            else if (turned && DrawnClip(c, "girar") is var turn && turn != null)
            {
                anim.Play(turn, true);
                drawnAction = "girar";
            }
            else if (grounded && drawnAction is "estocada" or "girar" && anim.Clip != null && !anim.Finished)
            {
                // Se termina de ver la acción de una vez.
            }
            else
            {
                var clip = DrawnClip(c, action);
                if (clip == null) { drawnAction = null; anim.Stop(); return null; }
                if (action == "escalera")
                {
                    drawnAction = action;
                    anim.PlayAt(clip, ((m.Y >> 2) % Math.Max(1, clip.Length) + clip.Length) % clip.Length);
                    return anim.Current;
                }
                anim.Play(clip, drawnAction != action && OneShot.Contains(action));
                drawnAction = action;
            }
            anim.Tick(dt);
            return anim.Current;
        }

        // Capa del escudo dibujada aparte (<personaje>_escudo_<cobre|plata|oro>_<accion>), en el mismo fotograma que el
        // cuerpo; null si no hay escudo o no está dibujada.
        public Sprite DrawnShield(PlayerMotor m)
        {
            int shield = ShieldLevel?.Invoke() ?? 0;
            if (shield <= 0 || Drawn == null || anim.Clip == null || drawnAction == null) return null;
            string metal = shield == 1 ? "cobre" : shield == 2 ? "plata" : "oro";
            var clip = Drawn.Clip($"{Prefix(m.Character)}_escudo_{metal}_{drawnAction}");
            if (clip == null || clip.Length == 0) return null;
            return clip.Frames[Math.Min(anim.Frame, clip.Length - 1)];
        }

        // El fotograma que toca según el estado del motor (andar avanza con la distancia recorrida; la escalera, con la y).
        public Sprite Frame(PlayerMotor m, float dt)
        {
            if (m.Room != lastRoom) { lastRoom = m.Room; lastX = m.X; lastY = m.Y; }
            int dx = Math.Abs(m.X - lastX), dy = m.Y - lastY;
            lastX = m.X;
            lastY = m.Y;
            var drawnSprite = DrawnFrame(m, dt, dx);
            if (drawnSprite != null) return drawnSprite;
            var c = m.Character;
            int shield = ShieldLevel?.Invoke() ?? 0;
            if (m.State == MotorState.Knockback || m.Stunned > 0) return Get(c, Anim.Hurt, 0, shield);
            if (m.AttackTicks > 0) return Get(c, Anim.Attack, m.AttackTicks >= 3 ? 0 : m.AttackTicks == 2 ? 1 : 2, shield);
            switch (m.State)
            {
                case MotorState.Ladder:
                    return Get(c, Anim.Ladder, (m.Y >> 2) & 1, shield);
                case MotorState.JumpPrep:
                    return Get(c, Anim.Prep, 0, shield);
                case MotorState.Jump:
                    return Get(c, Anim.Rise, 0, shield);
                case MotorState.Fall:
                    return Get(c, Anim.Fall, 0, shield);
            }
            if (dx > 0 && dx < 20 || m.State == MotorState.Carried)
            {
                walk += Math.Max(dx, 1);
                idle = 0;
                return Get(c, Anim.Walk, (int)(walk / 5) & 3, shield);
            }
            idle += dt;
            return Get(c, Anim.Idle, (int)(idle / 0.7f) & 1, shield);
        }

        // ---------- pintura ----------

        struct Pose
        {
            public float FootFront, FootBack, LiftFront, LiftBack, Bob, Crouch, Lean, Cape, CapeLift;
            public int Sword;          // 0 no, 1 golpe extendido, 2 recogiendo
            public bool Ladder, Hurt;
            public int ArmPhase;
        }

        class Palette
        {
            public Color Body, BodyLight, Cape, Helm, HelmShade, Slit, Crest, CrestLight, Trim, Emblem, Hair, Wing;
            public bool Wings;
        }

        static Color C(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        static readonly Palette PopolonColors = new Palette
        {
            Body = C("#262b40"), BodyLight = C("#3d4566"), Cape = C("#8c2a35"), Helm = C("#efe9da"), HelmShade = C("#c3baa4"),
            Slit = C("#0b0c12"), Crest = C("#d23c32"), CrestLight = C("#ff7a60"), Trim = C("#7f5520"),
            Emblem = C("#f6d98a"),
        };

        static readonly Palette AphroditeColors = new Palette
        {
            Body = C("#2d2540"), BodyLight = C("#4a3d68"), Cape = C("#5f449a"), Helm = C("#f5efe8"), HelmShade = C("#cbc2c8"),
            Slit = C("#0b0c12"), Trim = C("#6b5a7f"), Emblem = C("#e8679a"), Hair = C("#f0c86e"),
            Wing = C("#e6ecf7"), Wings = true,
        };

        // Escudos de las tiendas (cara, borde): cobre, plata y oro.
        static readonly (Color face, Color rim)[] Shields =
        {
            (Color.clear, Color.clear), (C("#c27445"), C("#6e3a1c")), (C("#c9cfdc"), C("#7a8296")), (C("#e2b84a"), C("#8f6a14")),
        };

        static Texture2D Paint(Character who, int shield, Pose p)
        {
            var k = who == Character.Popolon ? PopolonColors : AphroditeColors;
            var cv = new ColorCanvas(W, H, S, Wpx / 2f, FootY);
            var (shieldFace, shieldRim) = Shields[shield];
            bool hasShield = shield > 0;
            float up = p.Bob - p.Crouch;                       // el cuerpo baja al agacharse
            float lx = p.Lean;
            float hipY = 4.6f - p.Crouch * 0.6f;
            float torsoY = 7.0f + up, headY = 11.4f + up;

            // Capa (detrás de todo): del hombro hacia atrás, ondea según la pose.
            float capeX = -4.2f - p.Cape, capeY = 2.6f + p.CapeLift;
            cv.Fill((x, y) => Mathf.Min(Segment(x, y, lx - 0.8f, torsoY + 1.8f, lx + capeX, capeY, 1.5f),
                                        Segment(x, y, lx - 0.4f, torsoY + 1.2f, lx + capeX + 1.2f, capeY - 0.6f, 1.2f)),
                k.Cape, lx + capeX - 3, capeY - 3, lx + 2, torsoY + 4);

            if (k.Hair.a > 0)
            {
                float sw = p.Cape * 0.6f;
                cv.Fill((x, y) => Mathf.Min(Segment(x, y, lx - 1.8f, headY - 0.6f, lx - 4.4f - sw, headY - 5.2f + p.CapeLift * 0.4f, 1.15f),
                                            Segment(x, y, lx - 1.2f, headY - 1.4f, lx - 3.2f - sw * 0.7f, headY - 6.4f, 0.9f)),
                    k.Hair, lx - 7, headY - 8, lx, headY + 1);
            }

            // Piernas (la de atrás más oscura).
            cv.Fill((x, y) => Segment(x, y, lx - 0.6f, hipY, p.FootBack - 0.2f, 0.8f + p.LiftBack, 0.95f), k.Body * 0.8f + new Color(0, 0, 0, 0.2f), -6, -1, 4, hipY + 1);
            cv.Fill((x, y) => Box(x, y, p.FootBack + 0.3f, 0.5f + p.LiftBack, 1.2f, 0.55f), k.Body * 0.7f + new Color(0, 0, 0, 0.3f), -6, -1, 4, 3);

            // Brazo de la espada (atrás) o la espada envainada.
            if (p.Sword == 0 && !p.Ladder)
                cv.Fill((x, y) => Segment(x, y, lx - 2.4f, torsoY - 3.2f, lx + 0.4f, torsoY - 0.4f, 0.35f), k.Trim, lx - 4, torsoY - 5, lx + 2, torsoY + 1);

            // Torso con faldón y cinturón.
            cv.Fill((x, y) => Box(x, y, lx, torsoY, 2.1f, 2.0f) - 0.6f, k.Body, lx - 3, torsoY - 3, lx + 3, torsoY + 3);
            cv.Fill((x, y) => Box(x, y, lx + 0.2f, hipY + 0.3f, 2.6f, 0.75f) - 0.2f, k.BodyLight, lx - 3, hipY - 1, lx + 3, hipY + 2);
            cv.Fill((x, y) => Box(x, y, lx, hipY + 1.2f, 2.5f, 0.28f), k.Trim, lx - 3, hipY + 0.5f, lx + 3, hipY + 2);

            // Pierna de delante.
            cv.Fill((x, y) => Segment(x, y, lx + 0.6f, hipY, p.FootFront + 0.2f, 0.8f + p.LiftFront, 0.95f), k.Body, -4, -1, 6, hipY + 1);
            cv.Fill((x, y) => Box(x, y, p.FootFront + 0.7f, 0.5f + p.LiftFront, 1.25f, 0.55f), k.Body * 0.85f + new Color(0, 0, 0, 0.15f), -4, -1, 6, 3);

            // Escalera: escudo a la espalda y los dos brazos arriba, alternando.
            if (p.Ladder)
            {
                if (hasShield)
                    cv.Fill((x, y) => Circle(x, y, lx - 2.2f, torsoY, 2.4f), shieldFace, lx - 5, torsoY - 3, lx, torsoY + 3);
            }

            // Yelmo: la "máscara" clara, con sombra en la nuca, carrillera y rendija en T.
            float hx = lx + 0.6f + (p.Hurt ? -0.6f : 0);
            cv.Fill((x, y) => Mathf.Min(Circle(x, y, hx, headY, 3.1f), Box(x, y, hx + 1.4f, headY - 1.6f, 1.4f, 1.3f) - 0.3f), k.Helm,
                hx - 4, headY - 4, hx + 4, headY + 4);
            cv.Fill((x, y) => Mathf.Max(Circle(x, y, hx - 1.4f, headY - 1.2f, 2.6f), Circle(x, y, hx, headY, 3.1f)), new Color(k.HelmShade.r, k.HelmShade.g, k.HelmShade.b, 0.75f),
                hx - 4, headY - 4, hx + 2, headY + 2);
            if (!p.Ladder)
            {
                cv.Fill((x, y) => Mathf.Min(Box(x, y, hx + 1.9f, headY + 0.1f, 1.5f, 0.42f), Box(x, y, hx + 1.35f, headY - 1.0f, 0.36f, 1.0f)), k.Slit,
                    hx, headY - 2.5f, hx + 4, headY + 1);
            }

            // Cresta (Popolon) o alas y melena (Aphrodite).
            if (!k.Wings)
            {
                float cy = headY + 0.4f;
                cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, hx - 0.3f, cy, 3.9f)) - 0.85f, cy + 1.2f - y), k.Crest, hx - 5, cy, hx + 4, cy + 5);
                cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, hx - 0.3f, cy, 4.35f)) - 0.3f, cy + 2.4f - y), k.CrestLight, hx - 4, cy + 2, hx + 3, cy + 5);
                cv.Fill((x, y) => Segment(x, y, hx - 3.4f, cy + 2.2f, hx - 4.6f - p.Cape * 0.5f, cy - 1.8f + p.CapeLift * 0.3f, 0.75f), k.Crest, hx - 7, cy - 3, hx - 2, cy + 3);
            }
            else
            {
                for (int f = 0; f < 3; f++)
                {
                    float ex = hx - 3.6f - f * 0.5f + p.Cape * -0.2f, ey = headY + 3.4f - f * 1.2f;
                    cv.Fill((x, y) => Segment(x, y, hx - 1.0f, headY + 1.2f, ex, ey, 0.55f - f * 0.08f), k.Wing, hx - 6, headY - 1, hx, headY + 5);
                }
            }

            // Escalera: los brazos, delante del yelmo, agarrando por encima de la cabeza (alternan).
            if (p.Ladder)
            {
                float a = p.ArmPhase == 0 ? 1.4f : -0.2f, b = p.ArmPhase == 0 ? -0.2f : 1.4f;
                cv.Fill((x, y) => Mathf.Min(Segment(x, y, lx + 1.6f, torsoY + 1.2f, lx + 3.0f, headY + 2.4f + a, 0.6f),
                                            Segment(x, y, lx - 1.6f, torsoY + 1.2f, lx - 2.6f, headY + 2.4f + b, 0.6f)), k.BodyLight,
                    lx - 4, torsoY, lx + 4, headY + 5);
                cv.Fill((x, y) => Mathf.Min(Circle(x, y, lx + 3.0f, headY + 2.4f + a, 0.75f), Circle(x, y, lx - 2.6f, headY + 2.4f + b, 0.75f)), k.Body,
                    lx - 4, headY, lx + 4, headY + 5);
            }

            // Brazo del escudo y escudo redondo (delante), o la espada extendida.
            if (!p.Ladder)
            {
                if (p.Sword == 1)
                {
                    // Brazo extendido, guarda y hoja hasta x + 18 a la altura de la zona que golpea (y − 5.5).
                    cv.Fill((x, y) => Segment(x, y, lx + 1.2f, torsoY + 1.0f, lx + 5.6f, 5.6f, 0.75f), k.BodyLight, lx, 4, lx + 7, torsoY + 2);
                    cv.Fill((x, y) => Box(x, y, 6.6f, 5.6f, 0.35f, 1.1f), k.Trim, 6, 4, 7.5f, 7);
                    cv.Fill((x, y) => Mathf.Max(Segment(x, y, 7.0f, 5.6f, 17.6f, 5.5f, 0.55f), 0), new Color(0.88f, 0.93f, 1f), 6.5f, 4.5f, 18.5f, 6.6f);
                    cv.Fill((x, y) => Segment(x, y, 7.4f, 5.85f, 16.8f, 5.75f, 0.15f), Color.white, 7, 5.4f, 17.5f, 6.3f);
                }
                else if (p.Sword == 3 || p.Sword == 2)
                {
                    // Recogiendo (a media distancia) o en guardia: la hoja sigue horizontal, apuntando al frente.
                    float hand = p.Sword == 3 ? 5.0f : 3.6f, tip = p.Sword == 3 ? 14.0f : 10.6f, hy = p.Sword == 3 ? 5.7f : 6.0f;
                    cv.Fill((x, y) => Segment(x, y, lx + 1.1f, torsoY + 0.9f, lx + hand, hy, 0.75f), k.BodyLight, lx, hy - 1, lx + hand + 1, torsoY + 2);
                    cv.Fill((x, y) => Box(x, y, lx + hand + 0.9f, hy, 0.35f, 1.0f), k.Trim, lx + hand, hy - 1.5f, lx + hand + 2, hy + 1.5f);
                    cv.Fill((x, y) => Segment(x, y, lx + hand + 1.3f, hy, lx + tip, hy - 0.1f, 0.5f), new Color(0.86f, 0.91f, 0.99f), lx + hand, hy - 1, lx + tip + 1, hy + 1);
                }
                if (!hasShield)
                {
                    // Sin escudo: el brazo de delante, con el puño cerrado.
                    float hx2 = lx + (p.Sword > 0 ? 0.6f : 2.2f), hy2 = torsoY - 1.4f + (p.Hurt ? 1.6f : 0);
                    cv.Fill((x, y) => Segment(x, y, lx + 1.0f, torsoY + 1.1f, hx2, hy2, 0.7f), k.BodyLight, lx - 1, hy2 - 1, lx + 4, torsoY + 2);
                    cv.Fill((x, y) => Circle(x, y, hx2 + 0.2f, hy2 - 0.2f, 0.8f), k.Body, hx2 - 1, hy2 - 1.5f, hx2 + 1.5f, hy2 + 1);
                }
            }
            if (!p.Ladder && hasShield)
            {
                float sx = lx + (p.Sword > 0 ? 1.0f : 2.6f), sy = torsoY - 0.4f + (p.Hurt ? 1.0f : 0);
                cv.Fill((x, y) => Circle(x, y, sx, sy, 2.75f), shieldRim, sx - 3, sy - 3, sx + 3, sy + 3);
                cv.Fill((x, y) => Circle(x, y, sx + 0.15f, sy + 0.1f, 2.15f), shieldFace, sx - 3, sy - 3, sx + 3, sy + 3);
                if (k.Wings)
                    for (int petal = 0; petal < 5; petal++)
                    {
                        float ang = petal * Mathf.PI * 2 / 5, ex = sx + 0.15f + Mathf.Cos(ang) * 0.6f, ey = sy + 0.1f + Mathf.Sin(ang) * 0.6f;
                        cv.Fill((x, y) => Circle(x, y, ex, ey, 0.55f), k.Emblem, sx - 2, sy - 2, sx + 2, sy + 2);
                    }
                else
                    cv.Fill((x, y) => Mathf.Abs(Circle(x, y, sx + 0.15f, sy + 0.1f, 1.0f)) - 0.3f, k.Emblem, sx - 2, sy - 2, sx + 2, sy + 2);
                // Brillo del escudo.
                cv.Fill((x, y) => Circle(x, y, sx + 0.9f, sy + 1.0f, 0.55f), new Color(1, 1, 1, 0.55f), sx, sy, sx + 2, sy + 2);
            }

            cv.Ink(new Color(0.03f, 0.03f, 0.05f));
            return cv.ToTexture();
        }

        // Estela de la estocada: destello horizontal a lo largo de la hoja, más brillante en la punta, con dos líneas de
        // velocidad detrás (128 × 32; la izquierda es el lado del jugador).
        static Texture2D Slash()
        {
            const int w = 128, h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float u = (x + 0.5f) / w, v = (y + 0.5f) / h * 2 - 1;
                    float core = Mathf.SmoothStep(0, 1, u / 0.18f) * (1 - Mathf.SmoothStep(0.9f, 1f, u)) * Mathf.Exp(-(v / 0.2f) * (v / 0.2f)) * (0.35f + 0.65f * u);
                    float lines = 0;
                    foreach (float lv in new[] { -0.6f, 0.55f })
                        lines = Mathf.Max(lines, Mathf.Clamp01(1 - Mathf.Abs(v - lv) / 0.09f) * Mathf.Clamp01(u / 0.1f) * Mathf.Clamp01((0.68f - u) / 0.2f) * 0.6f);
                    float tip = Mathf.Clamp01(1 - Vector2.Distance(new Vector2(u * 4, v), new Vector2(0.93f * 4, 0)) / 0.45f);
                    float a = Mathf.Clamp01(Mathf.Max(core, Mathf.Max(lines, tip * tip)));
                    tex.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            tex.Apply();
            return tex;
        }
    }
}
