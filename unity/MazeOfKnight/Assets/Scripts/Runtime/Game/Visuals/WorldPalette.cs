using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Estilo visual del remake (docs/design_bible/12_estilo_visual.md): cada mapa tiene un tema (qué siluetas salen en
    // el fondo) y una paleta casi monocroma con un acento de luz. Es arte propio: no sale de la ROM.
    public enum Theme { Temple, Cavern, Coral, Lair, Crystal, Sunken, Fungal, Abyss, Magma, Ossuary, Throne }

    public class WorldPalette
    {
        public string Name;
        public Theme Theme;
        public Color SkyTop, SkyBottom;     // degradado del fondo del todo
        public Color Far, Mid;              // siluetas lejanas y medias
        public Color Ground, Rim;           // macizo de la sala y su borde de arriba
        public Color Fog;                   // niebla entre capas
        public Color Ambient;               // luz global
        public float AmbientIntensity;
        public Color Accent;                // farolas, rayos de luz
        public Color Motes;                 // partículas
        public Color Liquid;                // agua (la lava tiene su color)

        static Color C(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        static WorldPalette P(string name, Theme theme, string skyTop, string skyBottom, string far, string mid, string ground,
            string rim, string fog, string ambient, float ambientIntensity, string accent, string motes, string liquid) =>
            new WorldPalette
            {
                Name = name, Theme = theme, SkyTop = C(skyTop), SkyBottom = C(skyBottom), Far = C(far), Mid = C(mid),
                Ground = C(ground), Rim = C(rim), Fog = C(fog), Ambient = C(ambient), AmbientIntensity = ambientIntensity,
                Accent = C(accent), Motes = C(motes), Liquid = C(liquid),
            };

        // Índice 0 = castillo, 1–10 = mundos.
        public static readonly WorldPalette[] All =
        {
            P("Castillo: templo de mármol", Theme.Temple, "#141a29", "#3b4862", "#5a6884", "#2c3650", "#2a3142", "#d6e0f2", "#8b9cba", "#a9b8d6", 0.75f, "#ffd59a", "#e2ebff", "#3d6fa8"),
            P("Mundo 1: cueva verde agua", Theme.Cavern, "#081619", "#1d4646", "#2f6a68", "#173839", "#1b2a2c", "#8ee6d6", "#4fa39a", "#8fd6cc", 0.7f, "#a6fff0", "#c4fff4", "#2f8f8a"),
            P("Mundo 2: arrecife", Theme.Coral, "#08142a", "#1c4a6b", "#2d6f90", "#153953", "#182634", "#99dcff", "#4a8fb5", "#8cc6ea", 0.7f, "#b3f2ff", "#d6f3ff", "#2a78b8"),
            P("Mundo 3: guarida ámbar", Theme.Lair, "#1f1108", "#583213", "#8a5521", "#452810", "#2b1b10", "#ffd27f", "#c48a3e", "#e8b77a", 0.7f, "#ffb347", "#ffe4a6", "#5a7f8f"),
            P("Mundo 4: cristal violeta", Theme.Crystal, "#130a22", "#3d2465", "#6b4aa0", "#32205a", "#221833", "#ecd2ff", "#9a7ad0", "#c7aef0", 0.7f, "#f2dcff", "#ffe8ff", "#6a5ad0"),
            P("Mundo 5: ruinas hundidas", Theme.Sunken, "#0a1220", "#22385c", "#3b5a88", "#1d2d4a", "#1a2233", "#b0caff", "#5d7fb5", "#9db5e0", 0.7f, "#d0e0ff", "#e0e9ff", "#2d5fa8"),
            P("Mundo 6: bosque de hongos", Theme.Fungal, "#09170c", "#23452a", "#3f7546", "#1e3b24", "#18251a", "#bdf2a6", "#6aa86a", "#a6d8a0", 0.7f, "#f2ff9e", "#eaffb6", "#3a8f6a"),
            P("Mundo 7: abismo luminoso", Theme.Abyss, "#03071a", "#0f1d45", "#1d3470", "#0d1a3a", "#0e1424", "#7fb5ff", "#2a4f9a", "#6f8fd0", 0.6f, "#66ffe0", "#8cffee", "#1d5fb0"),
            P("Mundo 8: magma", Theme.Magma, "#1c0505", "#5a170c", "#8f2a12", "#3d0e08", "#241010", "#ffa266", "#c5401c", "#f09a7a", 0.65f, "#ffb060", "#ffd38a", "#3f6fa0"),
            P("Mundo 9: osario", Theme.Ossuary, "#171513", "#4a4540", "#7a7268", "#3a3530", "#25221f", "#f2e9d8", "#9e9588", "#d6cfc2", 0.75f, "#fff2d2", "#ffffff", "#4f7f9f"),
            P("Mundo 10: trono carmesí", Theme.Throne, "#120308", "#46101f", "#7a1f38", "#380c1a", "#220b12", "#ff94b4", "#a3304f", "#e08aa4", 0.6f, "#ff6b8f", "#ffb8cb", "#5a3f9f"),
        };

        public static readonly Color Lava = new Color(1f, 0.45f, 0.12f);

        public static WorldPalette For(int index) => All[Mathf.Clamp(index, 0, All.Length - 1)];
    }
}
