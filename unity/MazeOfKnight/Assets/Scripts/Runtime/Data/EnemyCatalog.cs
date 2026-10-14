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
    }

    [Serializable]
    public class EnemyCatalogJson
    {
        public EnemyInfo[] enemies = Array.Empty<EnemyInfo>();
    }

    public class EnemyCatalog : ScriptableObject
    {
        public List<EnemyInfo> enemies = new List<EnemyInfo>();

        public EnemyInfo Find(int type) => enemies.Find(e => e.type == type);
    }
}
