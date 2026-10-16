using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MazeOfKnight.Game
{
    // Partida guardada (sustituye a la contraseña de Demeter; design bible, cap. 8 y 11). Se guarda en el santuario de
    // Demeter (castillo, sala 72) y se carga con L en el título, la tecla con la que el original pide la contraseña.
    // Guarda todo lo que la contraseña no podía (piedras rotas, puertas, eventos…): es una mejora del remake.
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public List<int> items = new List<int>();
        public List<int> worldItems = new List<int>();      // mundo * 256 + objeto
        public List<int> openedWorlds = new List<int>();
        public List<int> unlockedWorlds = new List<int>();   // puertas desbloqueadas sin abrir
        public List<int> bossesDefeated = new List<int>();
        public int bibleUses;
        public int[] experience = new int[2], vitality = new int[2], maxVitality = new int[2];
        public int[] lives = new int[2], credits = new int[2];
        public int arrows, coins, keys, weapon, character;
        public int world10Door;              // E06E: dónde sale la puerta del mundo 10
        // RemakeRules.HollowBenches / HollowDeath: el último banco y la sombra (mapa vacío = ninguno).
        public int soul;                     // RemakeRules.HollowSoul
        public List<int> charms = new List<int>();      // RemakeRules.HollowCharms: los que se tienen (equipados: en items)
        public List<int> tutorials = new List<int>();   // láminas ya vistas
        public List<int> journal = new List<int>();     // RemakeRules.HollowJournal: tipo * 65536 + derrotas
        public List<string> visited = new List<string>();   // RemakeRules.HollowMaps: salas visitadas (mapa:sala)
        public bool corniferMet;
        public bool castleMap;          // el mapa del castillo, comprado a su Cornifer
        public List<int> corniferGreeted = new List<int>();
        public string benchMap = "";
        public int benchRoom, benchX, benchY;
        public string shadeMap = "";
        public int shadeRoom, shadeX, shadeY, shadeGeo;
        public List<string> takenItems = new List<string>(), brokenRocks = new List<string>(), openedDoors = new List<string>(),
            roomObjects = new List<string>(), firedEvents = new List<string>();

        // Las capturas y el recorrido de pruebas (-captura…, -recorrido) guardan aparte: antes escribían encima de la
        // partida del jugador (se sientan en bancos, que guardan).
        public static readonly bool Testing = System.Array.Exists(System.Environment.GetCommandLineArgs(),
            a => a.StartsWith("-captura") || a == "-recorrido");
        public static string Path => System.IO.Path.Combine(Application.persistentDataPath, Testing ? "partida_pruebas.json" : "partida.json");
        public static bool Exists => File.Exists(Path) || File.Exists(Path + ".new");

        public void Write() => WriteAtomic(Path, JsonUtility.ToJson(this, true));

        // Escribe primero <fichero>.new y luego lo pone en su sitio de una vez (como el user1.dat.new de Hollow Knight):
        // si el juego se cierra a mitad, queda la partida anterior entera, nunca una a medias.
        public static void WriteAtomic(string path, string text)
        {
            string tmp = path + ".new";
            File.WriteAllText(tmp, text);
            if (File.Exists(path)) File.Replace(tmp, path, null);
            else File.Move(tmp, path);
        }

        // Lee la partida; si el cierre llegó justo antes de ponerla en su sitio, se queda la .new (ya estaba completa).
        public static string ReadAtomic(string path)
        {
            if (!File.Exists(path) && File.Exists(path + ".new")) File.Move(path + ".new", path);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public static SaveData Read()
        {
            try { var text = ReadAtomic(Path); return text != null ? JsonUtility.FromJson<SaveData>(text) : null; }
            catch (Exception e) { Debug.LogWarning($"No se pudo leer la partida: {e.Message}"); return null; }
        }

        public static List<string> Pack(IEnumerable<(string, int)> set)
        {
            var l = new List<string>();
            foreach (var (m, a) in set) l.Add($"{m}|{a}");
            return l;
        }

        public static List<string> Pack(IEnumerable<(string, int, int, int)> set)
        {
            var l = new List<string>();
            foreach (var (m, a, b, c) in set) l.Add($"{m}|{a}|{b}|{c}");
            return l;
        }

        public static void Unpack(List<string> src, HashSet<(string, int)> dst)
        {
            dst.Clear();
            foreach (var s in src) { var p = s.Split('|'); dst.Add((p[0], int.Parse(p[1]))); }
        }

        public static void Unpack(List<string> src, HashSet<(string, int, int, int)> dst)
        {
            dst.Clear();
            foreach (var s in src) { var p = s.Split('|'); dst.Add((p[0], int.Parse(p[1]), int.Parse(p[2]), int.Parse(p[3]))); }
        }
    }
}
