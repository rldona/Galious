using System.Collections.Generic;
using System.Globalization;
using MazeOfKnight.Game.Enemies;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Enemigos con el dibujo de una criatura de Hollow Knight (Assets/Art/Resources/Hollow/enemigos y enemigos_centro,
    // fuera de git; los importa tools/hollow/enemigos.py con su tabla de equivalencias). Solo aspecto: el dibujo se
    // apoya en la caja que pinta el juego (abajo en el centro los que andan, en el centro los que vuelan), sin deformarlo,
    // a 'k' veces la altura de la caja, y puede sobresalir de ella; ninguna colisión cambia.
    public class HollowEnemyArt
    {
        public class Info
        {
            public string Key;
            public bool Feet;            // pivote en los pies (los que andan) o en el centro
            public float K, RefHeight;   // altura del dibujo = K · altura de la caja; RefHeight, la de su reposo en texels
        }

        class State
        {
            public FrameAnimator Anim = new FrameAnimator();
            public float LastX, LastY, Moved = -10;
        }

        readonly ArtLibrary lib;
        readonly Dictionary<int, Info> byType = new Dictionary<int, Info>();
        readonly Dictionary<string, float> fps = new Dictionary<string, float>();
        readonly Dictionary<Enemy, State> states = new Dictionary<Enemy, State>();

        public int Count => byType.Count;

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
                else if (p.Length == 6 && p[0] == "enemigo")
                {
                    var info = new Info
                    {
                        Key = p[1], Feet = p[3] == "pies",
                        K = float.Parse(p[4], CultureInfo.InvariantCulture), RefHeight = float.Parse(p[5], CultureInfo.InvariantCulture),
                    };
                    foreach (var t in p[2].Split(','))
                        art.byType[int.Parse(t, NumberStyles.HexNumber)] = info;
                }
            }
            if (art.byType.Count == 0) return null;
            Debug.Log($"[HollowEnemyArt] {art.byType.Count} tipos de enemigo con dibujo de Hollow Knight");
            return art;
        }

        public Info For(Enemy e) => e != null && byType.TryGetValue(e.Type, out var i) ? i : null;

        // El fotograma que toca: "mover" si se ha movido hace poco (el motor va a 30 ticks/s y la pantalla más deprisa),
        // si no "quieto". Null si este tipo no tiene dibujo.
        public Sprite Frame(Enemy e, float dt)
        {
            var info = For(e);
            if (info == null) return null;
            if (!states.TryGetValue(e, out var s)) states[e] = s = new State { LastX = e.X, LastY = e.Y };
            if (Mathf.Abs(e.X - s.LastX) > 0.01f || Mathf.Abs(e.Y - s.LastY) > 0.01f) s.Moved = 0;
            else s.Moved += dt;
            s.LastX = e.X;
            s.LastY = e.Y;
            string action = s.Moved < 0.1f ? "mover" : "quieto";
            var clip = lib.Clip($"{info.Key}_{action}") ?? lib.Clip($"{info.Key}_quieto");
            if (clip == null || clip.Length == 0) return null;
            if (fps.TryGetValue(clip.Name, out float f)) clip.Fps = f;
            s.Anim.Play(clip);
            s.Anim.Tick(dt);
            return s.Anim.Current;
        }

        // Olvida a los que ya no están.
        public void Forget(Enemy e) => states.Remove(e);
    }
}
