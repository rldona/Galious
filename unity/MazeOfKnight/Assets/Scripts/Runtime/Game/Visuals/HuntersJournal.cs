using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Las entradas del diario del cazador (Assets/Art/Resources/Hollow/diario, fuera de git; las importa
    // tools/hollow/diario.py: retrato, nombre, descripción y notas de la criatura de Hollow Knight que hace de cada
    // enemigo) y el aviso de abajo a la derecha cuando se registra una criatura nueva, como en Hollow Knight. La pestaña
    // Diario del inventario (InventoryMenu.Hollow) las enseña.
    public class HuntersJournal : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public string key, name, desc, notes;
            public bool boss;
            public int[] types = Array.Empty<int>();
        }

        [Serializable]
        class Data { public Entry[] entries = Array.Empty<Entry>(); }

        static Entry[] entries;
        static readonly Dictionary<string, Sprite> portraits = new Dictionary<string, Sprite>();
        static Sprite symbol;

        public static bool Available => Resources.Load<TextAsset>("Hollow/diario/diario") != null;

        public static IReadOnlyList<Entry> Entries
        {
            get
            {
                if (entries != null) return entries;
                var json = Resources.Load<TextAsset>("Hollow/diario/diario");
                entries = json != null ? JsonUtility.FromJson<Data>(json.text).entries : Array.Empty<Entry>();
                return entries;
            }
        }

        public static Entry Of(int type)
        {
            foreach (var e in Entries)
                if (Array.IndexOf(e.types, type) >= 0) return e;
            return null;
        }

        public static Sprite Portrait(Entry e)
        {
            if (e == null) return null;
            if (!portraits.TryGetValue(e.key, out var s)) portraits[e.key] = s = Resources.Load<Sprite>("Hollow/diario/" + e.key);
            return s;
        }

        public static Sprite Symbol => symbol != null ? symbol : symbol = Resources.Load<Sprite>("Hollow/diario/simbolo");

        public static int NotesAt(Entry e) => e.boss ? 1 : Journal.NotesAt;

        public GameBootstrap game;
        Entry toast;
        float toastAt = -10;
        const float ToastSeconds = 3.5f;

        void Start()
        {
            if (game == null) return;
            game.Journal.NewType += t =>
            {
                var e = Of(t);
                if (e == null) return;
                // Solo si es la primera criatura de su entrada (otro tipo de la misma ya la había abierto).
                foreach (int other in e.types) if (other != t && game.Journal.Known(new[] { other })) return;
                toast = e;
                toastAt = Time.unscaledTime;
            };
        }

        // Para las capturas.
        public void ShowToast(Entry e) { toast = e; toastAt = Time.unscaledTime; }

        void OnGUI()
        {
            float t = Time.unscaledTime - toastAt;
            if (toast == null || t > ToastSeconds) return;
            // Con un menú abierto espera a que se cierre.
            if (game != null && game.MenuOpen) { toastAt = Time.unscaledTime - Mathf.Min(t, 0.3f); return; }
            GUI.depth = -7;
            float s = UiKit.Scale, W = Screen.width, H = Screen.height;
            float a = Mathf.Clamp01(t / 0.3f) * Mathf.Clamp01((ToastSeconds - t) / 0.5f);
            var r = new Rect(W - 380 * s, H - 130 * s, 360 * s, 104 * s);
            UiKit.Box(r, new Color(0, 0, 0, 0.6f * a));
            UiKit.Icon(new Rect(r.x + 110 * s, r.yMax - 22 * s, 220 * s, 16 * s), Symbol, 0.5f * a);
            UiKit.Icon(new Rect(r.x + 10 * s, r.y + 8 * s, 88 * s, 88 * s), Portrait(toast), a);
            UiKit.Text(new Rect(r.x + 110 * s, r.y + 14 * s, r.width - 150 * s, 30 * s), "Nueva entrada en el diario", 15, TextAnchor.MiddleLeft, UiKit.Dim, true, a);
            UiKit.Text(new Rect(r.x + 110 * s, r.y + 46 * s, r.width - 120 * s, 44 * s), toast.name, 24, TextAnchor.MiddleLeft, UiKit.Bone, true, a);
        }
    }
}
