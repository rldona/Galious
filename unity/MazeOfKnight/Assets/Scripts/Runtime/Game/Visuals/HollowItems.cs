using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Iconos de Hollow Knight para los objetos del original (Assets/Art/Resources/Hollow/objetos_sala/objetoXX.png, fuera
    // de git; los importa tools/hollow/objetos.py con su tabla de equivalencias): ajustados a 16 × 16 px y con el pivote
    // abajo en el centro, como los objetos de las salas. Null si no se han importado.
    public static class HollowItems
    {
        static Dictionary<int, Sprite> icons;

        public static bool Ready => Load().Count > 0;

        static Dictionary<int, Sprite> Load()
        {
            if (icons != null) return icons;
            icons = new Dictionary<int, Sprite>();
            foreach (var s in Resources.LoadAll<Sprite>("Hollow/objetos_sala"))
                if (s.name.StartsWith("objeto") && int.TryParse(s.name.Substring(6), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int t))
                    icons[t] = s;
            if (icons.Count > 0) Debug.Log($"[HollowItems] {icons.Count} objetos con icono de Hollow Knight");
            return icons;
        }

        public static Sprite Icon(int type) => Load().TryGetValue(type, out var s) ? s : null;

        // Botín que sueltan los enemigos (0x5D flechas, 0x5E monedas, 0x5F llaves): el icono de su recurso.
        public static Sprite Loot(int lootType) => lootType switch
        {
            0x5D => Icon(0x2D),
            0x5E => Icon(0x2E),
            0x5F => Icon(0x2F),
            _ => null,
        };
    }
}
