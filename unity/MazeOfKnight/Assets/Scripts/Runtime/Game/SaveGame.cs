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
        public List<string> takenItems = new List<string>(), brokenRocks = new List<string>(), openedDoors = new List<string>(),
            roomObjects = new List<string>(), firedEvents = new List<string>();

        public static string Path => System.IO.Path.Combine(Application.persistentDataPath, "partida.json");
        public static bool Exists => File.Exists(Path);

        public void Write() => File.WriteAllText(Path, JsonUtility.ToJson(this, true));

        public static SaveData Read()
        {
            try { return Exists ? JsonUtility.FromJson<SaveData>(File.ReadAllText(Path)) : null; }
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
