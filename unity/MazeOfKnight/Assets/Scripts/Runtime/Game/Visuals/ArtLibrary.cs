using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Arte dibujado a mano, por nombre de fichero (docs/art/encargo/README.md, "Nombres de fichero"). Los PNG van en
    // Assets/Art/Resources/Dibujado/<carpeta>/ y se agrupan en clips:
    //   <tipo>_<accion>_<NN>[_<etiqueta>].png  →  clip "<tipo>_<accion>", fotograma NN (desde 00 o 01), etiqueta opcional
    //   <tipo>_<accion>.png                     →  clip de un solo fotograma
    // Ejemplos: popolon_correr_03.png, popolon_estocada_01_golpe.png, enemigo25_andar_00.png, mundo05_fondo_medio.png.
    // El número de fotograma lleva siempre dos cifras y los números de tipo o de mundo van pegados a la palabra
    // (enemigo25, jefe01, mundo05), para que no se confundan con él. El importador (Editor/ArtImporter) fija la escala
    // (8 texels por píxel del original) y el pivote. Si un clip no existe, el juego sigue con el arte provisional
    // generado por código.
    public class ArtLibrary
    {
        public const string Folder = "Dibujado";
        static readonly Regex Frame = new Regex(@"^(?<clip>.+?)_(?<n>\d{2,3})(?:_(?<tag>[a-z]+))?$", RegexOptions.Compiled);

        readonly Dictionary<string, FrameClip> clips = new Dictionary<string, FrameClip>();

        public int Count => clips.Count;
        public IEnumerable<string> Names => clips.Keys;

        public static ArtLibrary Load(string folder = Folder) =>
            new ArtLibrary(Resources.LoadAll<Sprite>(folder).Select(s => (s.name, s)));

        public ArtLibrary(IEnumerable<(string name, Sprite sprite)> files)
        {
            var frames = new Dictionary<string, List<(int n, string tag, Sprite s)>>();
            foreach (var (name, sprite) in files)
            {
                if (!Parse(name, out var clip, out int n, out var tag)) continue;
                if (!frames.TryGetValue(clip, out var list)) frames[clip] = list = new List<(int, string, Sprite)>();
                list.Add((n, tag, sprite));
            }
            foreach (var (clip, list) in frames.Select(kv => (kv.Key, kv.Value)))
            {
                var ordered = list.OrderBy(f => f.n).ToList();
                clips[clip] = new FrameClip
                {
                    Name = clip,
                    Frames = ordered.Select(f => f.s).ToArray(),
                    Tags = ordered.Any(f => f.tag != null) ? ordered.Select(f => f.tag).ToArray() : null,
                };
            }
        }

        // Nombre de fichero (sin extensión) → clip, número de fotograma y etiqueta. Sin número es un clip de 1 fotograma.
        public static bool Parse(string file, out string clip, out int n, out string tag)
        {
            clip = file; n = 0; tag = null;
            if (string.IsNullOrEmpty(file)) return false;
            var m = Frame.Match(file);
            if (!m.Success) return true;
            clip = m.Groups["clip"].Value;
            n = int.Parse(m.Groups["n"].Value);
            tag = m.Groups["tag"].Success ? m.Groups["tag"].Value : null;
            return true;
        }

        public FrameClip Clip(string name) => clips.TryGetValue(name, out var c) ? c : null;
        public bool Has(string name) => clips.ContainsKey(name);
    }
}
