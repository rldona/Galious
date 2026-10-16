using System.Collections.Generic;

namespace MazeOfKnight.Game
{
    // Los textos de los santuarios en castellano (Galious: The Hollow Kingdom; lo pidió el usuario: salían en inglés, como
    // en la ROM). Se traducen los mensajes del original (Shrines.json, ya con ShrineVisit.Format), con el sentido de cada
    // uno; con RemakeRules.SoloKnight no hay compañera a la que revivir ni Aphrodite a quien hablar, y esos se adaptan.
    public static class ShrineTexts
    {
        static readonly Dictionary<string, string> es = new Dictionary<string, string>
        {
            ["IF YOUR LOVER IS\nKILLED,COME BACK\nHERE WITH ARROWS x100,\nCOINS x100 AND KEYS x20."] =
                "Si tu compañera cae, vuelve aquí con cien flechas, cien geo y veinte llaves.",
            ["SEE A FAIRY TO\nRESTORE YOUR\nVITALITY."] = "Busca un hada para recuperar tu vitalidad.",
            ["GIVE ME ARROWS x100,\nCOINS x100 AND KEYS x20 IN\nRETURN FOR REVIVING\nYOUR LOVER."] =
                "Dame cien flechas, cien geo y veinte llaves y devolveré la vida a tu compañera.",
            ["YOUR LOVER HAS\nREVIVED.GO AND GET\nBACK THE LITTLE BABY\nTOGETHER.GO AHEAD!"] =
                "Tu compañera ha vuelto a la vida. Id juntos a rescatar al pequeño. ¡Adelante!",
            ["THEN,YOU MUST FIGHT\nALL BY YOURSELF.\nGO,RIGHT NOW."] = "Entonces lucharás solo. Ve, ahora mismo.",
            ["YOU CAN BUY SOME OF\nMY TREASURE."] = "Puedes comprar algunos de mis tesoros.",
            ["TAKE A MINE WITH YOU\nWHEN YOU GO TO\nWORLD 4."] = "Lleva una mina contigo cuando vayas al mundo 4.",
            ["HIT THE WALL WITH\nYOURE SWORD IF YOU\nWANT TO GO TO THE\nOPPOSITE SHORE."] =
                "Golpea la pared con tu aguijón si quieres llegar a la otra orilla.",
            ["APHRODITE.DO NOT BE\nAFRAID OF WATER."] = "Aphrodite, no tengas miedo del agua.",
            ["THE PURE WATER IS\nINSIDE THE WALL."] = "El agua pura está dentro de la pared.",
            ["HIT THE WALL WHEN\nYOU ARE HALFWAY OF\nTHE STAIRS."] = "Golpea la pared cuando estés a mitad de la escalera.",
            ["A GATE WHICH LEARDS\nTO GALIOUS WILL\nAPPEAR SOMEWHERE IN\nTHE CASTLE."] =
                "En algún lugar del castillo aparecerá una puerta que lleva hasta Galious.",
            ["YOU WANT THE SECRET\nCODE TO WAKEN\nMY MEMORY KEEP THE\nRECORDS ABOUT YOU?"] =
                "¿Quieres que guarde en mi memoria el recuerdo de tu viaje?",
            ["RESCUE THE BABY AS\nQUICKLY AS POSSIBLE."] = "Rescata al pequeño cuanto antes.",
            ["DID YOU GET THEM?\n   YES"] = "¿Los has conseguido?",
            ["SEE YOU AGAIN."] = "Hasta pronto.",
            ["YOU SHALL HAVE SOME\nBREAD AND WATER.\nEVERYTIME YOU DEFCAT\nA DEVIL."] =
                "Cada vez que derrotes a un demonio tendrás pan y agua.",
            ["COINS ARE NOT\nENOUGH."] = "No tienes geo suficiente.",
            ["FIND THE SPELL\nWRITTEN ON A STONE\nMONUMENT UNDER THE\nWATER."] =
                "Busca el conjuro escrito en una lápida bajo el agua.",
        };

        // Con el Caballero solo (RemakeRules.SoloKnight): sin compañera ni Aphrodite.
        static readonly Dictionary<string, string> solo = new Dictionary<string, string>
        {
            ["IF YOUR LOVER IS\nKILLED,COME BACK\nHERE WITH ARROWS x100,\nCOINS x100 AND KEYS x20."] =
                "Aquí no hay nadie a quien devolver la vida. Tú, pequeño caballero, despertarás siempre en tu último banco.",
            ["APHRODITE.DO NOT BE\nAFRAID OF WATER."] = "No tengas miedo del agua.",
        };

        public const string Saved = "PARTIDA GUARDADA.\n";

        // El texto en castellano (si no está, el original).
        public static string Spanish(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            if (text.StartsWith(Saved)) return "Partida guardada.\n" + Spanish(text.Substring(Saved.Length));
            if (RemakeRules.SoloKnight && solo.TryGetValue(text, out var s)) return s;
            return es.TryGetValue(text, out var t) ? t : text;
        }

        public static string God(string god) => god switch
        {
            "Demeter" => "Deméter",
            "Athena" => "Atenea",
            "Hephaestus" => "Hefesto",
            "Asclepius" => "Asclepio",
            "Death" => "La Muerte",
            _ => god,
        };

        // Lo que se vende, por su tipo: el carcaj (100 flechas) y la llave con su nombre de tienda.
        public static string Item(int type) => type switch
        {
            0x2D => "Carcaj",
            0x2F => "Llave",
            _ => ItemTexts.Get(type).Name,
        };

        public static string ItemText(int type) => type switch
        {
            0x2D => "Cien flechas para el carcaj. Las gastan todas las armas.",
            0x2F => "Una llave para una de las puertas pequeñas con cerradura.",
            _ => ItemTexts.Get(type).Text,
        };
    }
}
