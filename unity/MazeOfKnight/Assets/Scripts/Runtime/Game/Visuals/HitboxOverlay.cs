using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeOfKnight.Game.Visuals
{
    // F9: las cajas de colisión encima del juego, para comprobar que lo que se ve es lo que golpea
    // (docs/items_enemies.md, "Cajas de colisión"):
    //   - verde: el jugador contra los enemigos (x − 6 … x + 6, y − 14 … y − 2);
    //   - amarillo: la espada mientras golpea;
    //   - rojo: cada enemigo que hace daño (sus cajas de contacto); gris, los que no.
    public class HitboxOverlay : MonoBehaviour
    {
        public GameBootstrap game;
        // Encendidas por defecto mientras se prueba la fusión con Hollow Knight (16 de octubre de 2026); F9 las quita.
        public static bool Show = true;

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame) Show = !Show;
        }

        void OnGUI()
        {
            if (!Show || game == null || game.Motor == null || game.AttractRunning || game.EndingRunning) return;
            var cam = Camera.main;
            if (cam == null) return;
            var m = game.Motor;
            // Rectángulo en píxeles del original (semiabierto) → pantalla.
            void Draw(Data.RoomModel room, float x0, float y0, float x1, float y1, Color c)
            {
                var a = cam.WorldToScreenPoint(Data.RoomCoords.PixelToWorld(room, (int)Mathf.Floor(x0), (int)Mathf.Floor(y0)));
                var b = cam.WorldToScreenPoint(Data.RoomCoords.PixelToWorld(room, (int)Mathf.Ceil(x1), (int)Mathf.Ceil(y1)));
                var r = new Rect(Mathf.Min(a.x, b.x), Screen.height - Mathf.Max(a.y, b.y), Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y));
                UiKit.Frame(r, c, 2);
            }
            var here = m.Room;
            Draw(here, m.X + OriginalPhysics.ContactLeft, m.Y + OriginalPhysics.ContactTop,
                m.X + OriginalPhysics.ContactRight + 1, m.Y + OriginalPhysics.ContactBottom + 1, Color.green);
            if (m.AttackTicks > 0)
            {
                var (sx0, sx1, sy0, sy1) = m.SwordRect();
                Draw(here, sx0, sy0, sx1 + 1, sy1 + 1, Color.yellow);
            }
            if (game.Enemies == null) return;
            foreach (var e in game.Enemies.Enemies)
            {
                if (!e.Alive || !e.Visible || e.Room != m.Room) continue;
                bool harmful = e.Harmful && (e.Damage > 0 || e.DiesOnTouch);
                foreach (var (x0, y0, x1, y1) in e.ContactRects())
                    Draw(here, x0, y0, x1, y1, harmful ? Color.red : new Color(0.7f, 0.7f, 0.7f));
            }
        }

    }
}
