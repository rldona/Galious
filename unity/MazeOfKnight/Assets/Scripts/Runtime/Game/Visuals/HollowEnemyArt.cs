using System.Collections.Generic;
using System.Globalization;
using MazeOfKnight.Data;
using MazeOfKnight.Game.Enemies;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Enemigos y proyectiles con el dibujo de una criatura de Hollow Knight (Assets/Art/Resources/Hollow/enemigos y
    // enemigos_centro, fuera de git; los importa tools/hollow/enemigos.py con su tabla de equivalencias). Solo aspecto:
    //   - el dibujo se apoya en su caja de contacto original (abajo en el centro los que andan, en el centro los que
    //     vuelan) y se escala igual en los dos ejes, sin deformarlo: su cuerpo pasa de la caja como mucho K veces, por
    //     arriba y por los lados (la escala es la menor de las dos). Ninguna colisión cambia;
    //   - acciones: "atacar" cuando lanza algo (Enemy.Parent de lo que crea), "girar" al darse la vuelta, "mover" si se
    //     ha movido hace poco y si no "quieto"; al morir deja su animación de muerte donde estaba ("morir");
    //   - los proyectiles alargados (spin) apuntan hacia donde vuelan.
    public class HollowEnemyArt
    {
        public class Info
        {
            public string Key;
            public bool Feet;                // pivote en los pies (los que andan) o en el centro
            public float K;                  // el cuerpo mide como mucho K veces la caja de contacto, en alto y en ancho
            public float RefHeight, RefWidth;  // alto y ancho del cuerpo en reposo, en texels (8 por píxel del original)
            public float? Spin;              // proyectil alargado: ángulo al que apunta el dibujo (gira con su vuelo)
            public bool OnWeapon;            // a lo ancho, en la zona que recibe golpes y no en toda la de contacto (el cuello del Elusive Dragon)
        }

        class State
        {
            public FrameAnimator Anim = new FrameAnimator();
            public float LastX, LastY, Moved = -10, Angle;
            public float SizeW, SizeH;       // jefes: la caja de contacto de su primera aparición (la suya cambia con cada pose)
            public bool LastLeft, Known;
            public string OneShot;           // "atacar" o "girar" mientras se ven
        }

        class Corpse
        {
            public SpriteRenderer Sr;
            public FrameAnimator Anim = new FrameAnimator();
            public float Left;               // segundos que le quedan tras acabar la animación
        }

        readonly ArtLibrary lib;
        readonly Dictionary<int, Info> byType = new Dictionary<int, Info>();
        readonly Dictionary<string, float> fps = new Dictionary<string, float>();
        readonly Dictionary<Enemy, State> states = new Dictionary<Enemy, State>();
        readonly List<Corpse> corpses = new List<Corpse>();
        Transform corpseRoot;

        public int Count => byType.Count;
        // Colisión de la sala (RoomWorld.Solid): para apoyar en el suelo a los que andan.
        public System.Func<RoomModel, int, int, bool> Solid;

        HollowEnemyArt(ArtLibrary lib) => this.lib = lib;

        public static HollowEnemyArt Load()
        {
            var txt = Resources.Load<TextAsset>("Hollow/enemigos/info");
            if (txt == null) return null;
            var sprites = new List<(string, Sprite)>();
            foreach (var folder in new[] { "Hollow/enemigos", "Hollow/enemigos_centro" })
                foreach (var s in Resources.LoadAll<Sprite>(folder)) sprites.Add((s.name, s));
            var art = new HollowEnemyArt(new ArtLibrary(sprites));
            foreach (var line in txt.text.Split('\n'))
            {
                var p = line.Trim().Split(' ');
                if (p.Length == 3 && p[0] == "clip" && float.TryParse(p[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                    art.fps[p[1]] = f;
                else if (p.Length == 9 && p[0] == "enemigo")
                {
                    var info = new Info
                    {
                        Key = p[1], Feet = p[3] == "pies",
                        K = float.Parse(p[4], CultureInfo.InvariantCulture),
                        RefHeight = float.Parse(p[5], CultureInfo.InvariantCulture),
                        RefWidth = float.Parse(p[6], CultureInfo.InvariantCulture),
                        Spin = p[7] == "-" ? (float?)null : float.Parse(p[7], CultureInfo.InvariantCulture),
                        OnWeapon = p[8] == "arma",
                    };
                    foreach (var t in p[2].Split(','))
                        art.byType[int.Parse(t, NumberStyles.HexNumber)] = info;
                }
            }
            if (art.byType.Count == 0) return null;
            Debug.Log($"[HollowEnemyArt] {art.byType.Count} tipos de enemigo y proyectil con dibujo de Hollow Knight");
            return art;
        }

        public Info For(Enemy e) => e != null && byType.TryGetValue(e.Type, out var i) ? i : null;

        // Escala del sprite (8 texels por píxel) para una caja de contacto de w × h píxeles: la misma en los dos ejes, la
        // menor de las que hacen que el cuerpo mida K veces la caja de alto o K veces de ancho.
        public static float Scale(Info info, float w, float h)
        {
            float byHeight = Mathf.Max(1f, h) * 8f * info.K / Mathf.Max(1f, info.RefHeight);
            float byWidth = Mathf.Max(1f, w) * 8f * info.K / Mathf.Max(1f, info.RefWidth);
            return Mathf.Min(byHeight, byWidth);
        }

        // Zona que recibe golpes (la unión de sus cajas de arma); si no tiene, la de contacto.
        public static (float x0, float y0, float x1, float y1) WeaponBox(Enemy e)
        {
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            foreach (var (a, b, c, d) in e.WeaponRects())
            {
                x0 = Mathf.Min(x0, a); y0 = Mathf.Min(y0, b); x1 = Mathf.Max(x1, c); y1 = Mathf.Max(y1, d);
            }
            return x0 <= x1 ? (x0, y0, x1, y1) : ContactBox(e);
        }

        // Caja de contacto original del enemigo (la unión de sus cajas de contacto).
        public static (float x0, float y0, float x1, float y1) ContactBox(Enemy e)
        {
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            foreach (var (a, b, c, d) in e.ContactRects())
            {
                x0 = Mathf.Min(x0, a); y0 = Mathf.Min(y0, b); x1 = Mathf.Max(x1, c); y1 = Mathf.Max(y1, d);
            }
            return x0 <= x1 ? (x0, y0, x1, y1) : e.Box();
        }

        // Dónde y cómo se pinta: posición del pivote, escala (con el giro a la izquierda en el signo de x) y rotación.
        public (Vector3 pos, Vector3 scale, Quaternion rot) Place(Enemy e, Info info)
        {
            var (cx0, cy0, cx1, cy1) = ContactBox(e);
            if (info.OnWeapon)
            {
                // A lo ancho, la zona que recibe golpes (el lado de la cabeza); a lo alto, la de contacto (la de golpes sube
                // por encima de la cabeza). Del ancho, el de la zona de golpes.
                var (wx0, _, wx1, _) = WeaponBox(e);
                cx0 = wx0;
                cx1 = wx1;
            }
            float w = cx1 - cx0, h = cy1 - cy0;
            // Los jefes cambian de caja con cada pose del original: el tamaño, con la primera (si no, palpitaría).
            if (e is Boss && states.TryGetValue(e, out var bs))
            {
                if (bs.SizeW <= 0) { bs.SizeW = w; bs.SizeH = h; }
                w = bs.SizeW;
                h = bs.SizeH;
            }
            float s = Scale(info, w, h);
            float feet = cy1;
            // Los que andan, con los pies en el suelo: si bajo el centro de la caja hay suelo dentro de ella (la caja
            // del original puede bajar de él, como la de los jefes), ahí. Si no, el borde de abajo de la caja.
            if (info.Feet && Solid != null)
            {
                int x = Mathf.RoundToInt((cx0 + cx1) / 2f);
                for (int y = Mathf.RoundToInt((cy0 + cy1) / 2f); y < cy1; y++)
                    if (Solid(e.Room, x, y)) { feet = y; break; }
            }
            var at = RoomCoords.PixelToWorld(e.Room, 0, 0) +
                     new Vector2((cx0 + cx1) / 2f, -(info.Feet ? feet : (cy0 + cy1) / 2f)) / RoomCoords.PixelsPerUnit;
            if (info.Spin is float spin && states.TryGetValue(e, out var st))
                return (new Vector3(at.x, at.y, 0), new Vector3(s, s, 1), Quaternion.Euler(0, 0, st.Angle - spin));
            return (new Vector3(at.x, at.y, 0), new Vector3(e.FacingLeft ? -s : s, s, 1), Quaternion.identity);
        }

        FrameClip Clip(Info info, string action, bool loop)
        {
            var clip = lib.Clip($"{info.Key}_{action}");
            if (clip == null || clip.Length == 0) return null;
            if (fps.TryGetValue(clip.Name, out float f)) clip.Fps = f;
            clip.Loop = loop;
            return clip;
        }

        // Algo que acaba de aparecer: si lo ha lanzado un enemigo, ese enemigo anima "atacar".
        public void Spawned(Enemy child)
        {
            var parent = child?.Parent;
            if (parent == null || !states.TryGetValue(parent, out var s)) return;
            var info = For(parent);
            var clip = info != null ? Clip(info, "atacar", false) : null;
            if (clip == null) return;
            s.OneShot = "atacar";
            s.Anim.Play(clip, true);
        }

        // El fotograma que toca. Null si este tipo no tiene dibujo.
        public Sprite Frame(Enemy e, float dt)
        {
            var info = For(e);
            if (info == null) return null;
            if (!states.TryGetValue(e, out var s)) states[e] = s = new State { LastX = e.X, LastY = e.Y };
            float dx = e.X - s.LastX, dy = e.Y - s.LastY;
            if (Mathf.Abs(dx) > 0.01f || Mathf.Abs(dy) > 0.01f)
            {
                s.Moved = 0;
                s.Angle = Mathf.Atan2(-dy, dx) * Mathf.Rad2Deg;   // y del original hacia abajo
            }
            else s.Moved += dt;
            s.LastX = e.X;
            s.LastY = e.Y;
            // Darse la vuelta: "girar" una vez (si no está atacando).
            bool turned = s.Known && e.FacingLeft != s.LastLeft;
            s.Known = true;
            s.LastLeft = e.FacingLeft;
            if (turned && s.OneShot != "atacar" && info.Spin == null && Clip(info, "girar", false) is FrameClip turn)
            {
                s.OneShot = "girar";
                s.Anim.Play(turn, true);
            }
            if (s.OneShot != null && s.Anim.Finished) s.OneShot = null;
            if (s.OneShot == null)
            {
                string action = s.Moved < 0.1f ? "mover" : "quieto";
                var clip = Clip(info, action, true) ?? Clip(info, "quieto", true);
                if (clip == null) return null;
                s.Anim.Play(clip);
            }
            s.Anim.Tick(dt);
            return s.Anim.Current;
        }

        // Muerto: deja su animación de muerte donde estaba, la termina, espera un poco y se desvanece.
        public void Killed(Enemy e, Material material, int sortingOrder)
        {
            var info = For(e);
            var clip = info != null ? Clip(info, "morir", false) : null;
            states.Remove(e);
            if (clip == null || e.Room == null) return;
            if (corpseRoot == null) corpseRoot = new GameObject("Muertes de Hollow Knight").transform;
            var go = new GameObject($"Muerte {info.Key}");
            go.transform.SetParent(corpseRoot, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;
            if (material != null) sr.sharedMaterial = material;
            var (pos, scale, rot) = Place(e, info);
            go.transform.SetPositionAndRotation(pos, rot);
            go.transform.localScale = scale;
            var c = new Corpse { Sr = sr, Left = 0.6f };
            c.Anim.Play(clip, true);
            sr.sprite = c.Anim.Current;
            corpses.Add(c);
        }

        // Cada fotograma: avanza las muertes y quita las que han terminado (o todas si 'clear', al cambiar de sala).
        public void TickCorpses(float dt, bool clear = false)
        {
            for (int i = corpses.Count - 1; i >= 0; i--)
            {
                var c = corpses[i];
                if (!clear)
                {
                    c.Anim.Tick(dt);
                    c.Sr.sprite = c.Anim.Current;
                    if (c.Anim.Finished) c.Left -= dt;
                    c.Sr.color = new Color(1, 1, 1, Mathf.Clamp01(c.Left / 0.3f));
                }
                if (clear || c.Left <= 0)
                {
                    Object.Destroy(c.Sr.gameObject);
                    corpses.RemoveAt(i);
                }
            }
        }

        // Olvida a los que ya no están.
        public void Forget(Enemy e) => states.Remove(e);
    }
}
