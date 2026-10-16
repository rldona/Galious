using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Opciones como en Hollow Knight, para el menú principal y el de pausa: Audio, Vídeo (con su página de Brillo), Mando
    // y Teclado. ↑ ↓ eligen, ← → cambian, Aceptar entra o cambia y Atrás vuelve. En Mando y Teclado, Aceptar sobre un
    // botón espera a que se pulse el nuevo (Controls). Los valores son los de GameSettings.
    public class OptionsMenu
    {
        public enum Page { Root, Audio, Video, Brightness, Pad, Keys }
        public Page Current = Page.Root;
        int sel;
        string listening;                  // id del botón que se está cambiando
        static Sprite padArt, sample;

        struct Row
        {
            public string Label, Value;
            public bool Slider;
            public int Level;              // 0–10 para los deslizadores
        }

        public void Open(Page p = Page.Root) { Current = p; sel = 0; listening = null; }

        string Title => Current switch
        {
            Page.Audio => "Audio", Page.Video => "Vídeo", Page.Brightness => "Brillo",
            Page.Pad => "Mando", Page.Keys => "Teclado", _ => "Opciones",
        };

        static string YesNo(bool b) => b ? "Sí" : "No";

        List<Row> Rows()
        {
            var r = new List<Row>();
            void Add(string label, string value = null) => r.Add(new Row { Label = label, Value = value });
            void Slide(string label, int level) => r.Add(new Row { Label = label, Value = level.ToString(), Slider = true, Level = level });
            switch (Current)
            {
                case Page.Root:
                    Add("Audio"); Add("Vídeo"); Add("Mando"); Add("Teclado"); Add("Volver");
                    break;
                case Page.Audio:
                    Slide("Volumen general", GameSettings.Master);
                    Slide("Sonido", GameSettings.Effects);
                    Slide("Música", GameSettings.Music);
                    Add("Restablecer"); Add("Volver");
                    break;
                case Page.Video:
                    var res = GameSettings.Resolution;
                    Add("Resolución", $"{res.x} × {res.y}");
                    Add("Pantalla completa", YesNo(GameSettings.FullScreen));
                    Add("Sincronía vertical", YesNo(GameSettings.VSync));
                    Add("Límite de fotogramas", GameSettings.FrameCap == 0 ? "Sin límite" : GameSettings.FrameCap.ToString());
                    Add("Brillo", $"{GameSettings.Brightness} %");
                    Add("Restablecer"); Add("Volver");
                    break;
                case Page.Brightness:
                    Slide("Brillo", GameSettings.Brightness / 10);
                    Add("Volver");
                    break;
                case Page.Pad:
                case Page.Keys:
                    bool pad = Current == Page.Pad;
                    foreach (var d in Controls.All)
                        if ((pad ? d.Pad : d.Key) != null)
                            Add(d.Name, listening == d.Id ? "· · ·" : Controls.Label(Controls.PathOf(d.Id, pad)));
                    Add("Restablecer"); Add("Volver");
                    break;
            }
            return r;
        }

        // Devuelve true al salir de las opciones.
        public bool Tick()
        {
            if (listening != null)
            {
                bool pad = Current == Page.Pad;
                var path = Controls.Listen(pad);
                if (path == null) return false;
                if (path != "") Controls.Rebind(listening, pad, path);
                listening = null;
                return false;
            }
            var rows = Rows();
            int n = rows.Count;
            if (HollowMenus.Up) sel = (sel + n - 1) % n;
            if (HollowMenus.Down) sel = (sel + 1) % n;
            int dx = HollowMenus.Left ? -1 : HollowMenus.Right ? 1 : 0;
            bool accept = HollowMenus.Accept;
            if (HollowMenus.Back || accept && rows[sel].Label == "Volver") return GoBack();
            string label = rows[sel].Label;
            switch (Current)
            {
                case Page.Root:
                    if (accept) Open(label switch { "Audio" => Page.Audio, "Vídeo" => Page.Video, "Mando" => Page.Pad, _ => Page.Keys });
                    break;
                case Page.Audio:
                    if (label == "Restablecer" && accept) GameSettings.ResetAudio();
                    if (dx == 0) break;
                    if (label == "Volumen general") GameSettings.Master += dx;
                    if (label == "Sonido") { GameSettings.Effects += dx; Sound.Effect(0x13); }
                    if (label == "Música") GameSettings.Music += dx;
                    break;
                case Page.Video:
                    if (label == "Brillo" && accept) { Open(Page.Brightness); break; }
                    if (label == "Restablecer" && accept) GameSettings.ResetVideo();
                    if (dx == 0 && !accept) break;
                    int step = dx != 0 ? dx : 1;
                    if (label == "Pantalla completa") GameSettings.FullScreen = !GameSettings.FullScreen;
                    if (label == "Sincronía vertical") GameSettings.VSync = !GameSettings.VSync;
                    if (label == "Límite de fotogramas")
                    {
                        int i = System.Array.IndexOf(GameSettings.FrameCaps, GameSettings.FrameCap);
                        GameSettings.FrameCap = GameSettings.FrameCaps[((i < 0 ? 0 : i) + step + GameSettings.FrameCaps.Length) % GameSettings.FrameCaps.Length];
                    }
                    if (label == "Resolución")
                    {
                        var all = GameSettings.Resolutions;
                        if (all.Count == 0) break;
                        int i = all.IndexOf(GameSettings.Resolution);
                        GameSettings.Resolution = all[((i < 0 ? all.Count - 1 : i) + step + all.Count) % all.Count];
                    }
                    break;
                case Page.Brightness:
                    if (label == "Brillo" && dx != 0) GameSettings.Brightness += dx * 10;
                    break;
                case Page.Pad:
                case Page.Keys:
                    bool pad = Current == Page.Pad;
                    if (!accept) break;
                    if (label == "Restablecer") { Controls.Reset(pad); break; }
                    foreach (var d in Controls.All)
                        if (d.Name == label) listening = d.Id;
                    break;
            }
            return false;
        }

        bool GoBack()
        {
            if (Current == Page.Root) return true;
            var back = Current == Page.Brightness ? Page.Video : Page.Root;
            var from = Current;
            Open(back);
            // Vuelve con la opción de la que se venía elegida.
            var rows = Rows();
            string name = from switch { Page.Audio => "Audio", Page.Video => "Vídeo", Page.Pad => "Mando", Page.Keys => "Teclado", Page.Brightness => "Brillo", _ => "" };
            sel = Mathf.Max(0, rows.FindIndex(r => r.Label == name));
            return false;
        }

        // Dibuja la página centrada en cx desde y.
        public void Draw(float cx, float y, float alpha = 1)
        {
            float s = UiKit.Scale;
            UiKit.Text(new Rect(cx - 400 * s, y - 10 * s, 800 * s, 40 * s), Title, 30, TextAnchor.MiddleCenter, UiKit.Bone, true, alpha);
            HollowMenus.Divider(cx, y + 30 * s, 320 * s, alpha);
            y += 70 * s;
            if (Current == Page.Brightness) { DrawBrightness(cx, y, alpha); y += 300 * s; }
            if (Current == Page.Pad) { DrawPad(cx, y, alpha); y += 250 * s; }
            var rows = Rows();
            sel = Mathf.Clamp(sel, 0, rows.Count - 1);
            float step = (Current == Page.Pad || Current == Page.Keys ? 36 : 44) * s;
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                bool on = i == sel;
                var c = on ? UiKit.Bone : UiKit.Dim;
                var r = new Rect(cx - 400 * s, y + i * step, 800 * s, step);
                if (row.Value == null)
                {
                    UiKit.Text(r, row.Label, 23, TextAnchor.MiddleCenter, c, true, alpha);
                    if (on) HollowMenus.Arrows(cx, r.center.y, row.Label, 23, alpha);
                    continue;
                }
                UiKit.Text(new Rect(cx - 400 * s, r.y, 380 * s, step), row.Label, 22, TextAnchor.MiddleRight, c, true, alpha);
                if (row.Slider)
                {
                    // Diez marcas, como los deslizadores de Hollow Knight.
                    for (int k = 0; k < 10; k++)
                        UiKit.Box(new Rect(cx + 24 * s + k * 18 * s, r.center.y - 6 * s, 12 * s, 12 * s),
                            new Color(c.r, c.g, c.b, (k < row.Level ? 0.9f : 0.18f) * alpha));
                    UiKit.Text(new Rect(cx + 214 * s, r.y, 80 * s, step), row.Value, 20, TextAnchor.MiddleLeft, c, true, alpha);
                }
                else UiKit.Text(new Rect(cx + 20 * s, r.y, 380 * s, step), row.Value, 22, TextAnchor.MiddleLeft, row.Value == "· · ·" ? UiKit.Gold : c, true, alpha);
                if (on) UiKit.Text(new Rect(cx - 420 * s, r.y, 20 * s, step), "›", 26, TextAnchor.MiddleCenter, UiKit.Gold, true, alpha);
            }
            string help = listening != null ? (Current == Page.Pad ? "Pulsa un botón del mando (Start cancela)" : "Pulsa una tecla (Esc cancela)")
                : Current == Page.Pad ? "Moverse: stick izquierdo o cruceta.   Pausa: Start."
                : Current == Page.Keys ? "Moverse y saltar: flechas (↑ salta).   Pausa: Esc.   Inventario también con Tab o F1."
                : null;
            if (help != null)
                UiKit.Text(new Rect(cx - 500 * s, y + rows.Count * step + 14 * s, 1000 * s, 30 * s), help, 16, TextAnchor.MiddleCenter, listening != null ? UiKit.Gold : UiKit.Dim, false, alpha);
        }

        // Una escena de muestra con el brillo elegido (el posproceso no toca la interfaz: se imita aquí).
        void DrawBrightness(float cx, float y, float alpha)
        {
            float s = UiKit.Scale;
            sample ??= Resources.Load<Sprite>("Hollow/escenario/cruces/lejano__00") ?? Resources.Load<Sprite>("Hollow/escenario/palacio/lejano__05");
            var r = new Rect(cx - 260 * s, y, 520 * s, 280 * s);
            if (sample != null) GUI.DrawTexture(r, sample.texture, ScaleMode.ScaleAndCrop);
            else UiKit.Box(r, new Color(0.3f, 0.32f, 0.4f, alpha));
            float e = Brightness.Exposure;
            if (e > 0) UiKit.Box(r, new Color(1, 1, 1, Mathf.Clamp01(e / 3f) * alpha));
            if (e < 0) UiKit.Box(r, new Color(0, 0, 0, Mathf.Clamp01(-e / 2f) * alpha));
            UiKit.Frame(r, new Color(1, 1, 1, 0.4f * alpha), Mathf.Max(1, s));
            UiKit.Text(new Rect(r.x, r.yMax - 40 * s, r.width, 30 * s), "Ajusta el brillo hasta que la roca del fondo se vea apenas.", 15, TextAnchor.MiddleCenter, UiKit.Bone, false, alpha);
        }

        // El dibujo del mando de Hollow Knight con los botones de ahora.
        void DrawPad(float cx, float y, float alpha)
        {
            float s = UiKit.Scale;
            padArt ??= Resources.Load<Sprite>("Hollow/menu/mando");
            float h = 230 * s, w = padArt != null ? h * padArt.rect.width / padArt.rect.height : 0;
            if (padArt != null) UiKit.Icon(new Rect(cx - w / 2, y, w, h), padArt, 0.9f * alpha);
            else UiKit.Text(new Rect(cx - 300 * s, y, 600 * s, h), Gamepad() ? "" : "No hay ningún mando conectado", 18, TextAnchor.MiddleCenter, UiKit.Dim, false, alpha);
            if (!Gamepad())
                UiKit.Text(new Rect(cx - 300 * s, y + h - 24 * s, 600 * s, 24 * s), "No hay ningún mando conectado", 16, TextAnchor.MiddleCenter, UiKit.Gold, false, alpha);
        }

        static bool Gamepad() => UnityEngine.InputSystem.Gamepad.current != null;
    }
}
