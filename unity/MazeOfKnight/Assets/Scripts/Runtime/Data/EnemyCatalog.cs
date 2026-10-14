using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeOfKnight.Data
{
    // Catálogo de enemigos (extracted/unity/enemies.json, de tools/extract/unity_bundle.py): vida del
    // descriptor b04:67B6, daño por contacto de la tabla b04:64E5 y objeto que protege de él.
    [Serializable]
    public class EnemyInfo
    {
        public int type;
        public int hp;
        public bool invulnerable;
        public int damage;
        public int immuneItem;
        // Botín (docs/items_enemies.md, "Muerte y botín"): regla 647B[tipo] y nivel según el daño 64E5[tipo].
        public int lootRule = 4;              // 0..3; 4 = nunca suelta nada
        public int[] lootKills = Array.Empty<int>();   // muertes de ese tipo (en la visita a la sala) que sortean
        public bool lootWhenLastAlive;        // regla 3: sortea si no queda otro vivo de su tipo
        public int lootLevel;                 // 0..3
        public int deathExplosions = 1;       // los grandes explotan 4 veces (4 sorteos)
        public int experience;                // BF27[tipo]
    }

    // Sorteo del botín (b04:6A77, tablas 6A9E y 6A6B): v = 0..15; v < coinBelow → monedas, < arrowBelow → flechas,
    // < keyBelow → llaves, si no, nada. Cantidades por nivel.
    [Serializable]
    public class LootTable
    {
        public int[] coinBelow = { 8, 9, 10, 10 };
        public int[] arrowBelow = { 10, 11, 13, 13 };
        public int[] keyBelow = { 11, 12, 15, 15 };
        public int[] arrowAmount = { 10, 20, 30, 40 };
        public int[] coinAmount = { 1, 4, 7, 10 };
        public int[] keyAmount = { 1, 1, 2, 2 };
        public int arrowObject = 0x5D, coinObject = 0x5E, keyObject = 0x5F;
        public int counterCap = 999;
    }

    [Serializable]
    public class EnemyCatalogJson
    {
        public EnemyInfo[] enemies = Array.Empty<EnemyInfo>();
        public LootTable loot = new LootTable();
    }

    public class EnemyCatalog : ScriptableObject
    {
        public List<EnemyInfo> enemies = new List<EnemyInfo>();
        public LootTable loot = new LootTable();

        public EnemyInfo Find(int type) => enemies.Find(e => e.type == type);
    }
}
