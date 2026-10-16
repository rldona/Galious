using System.Collections.Generic;
using MazeOfKnight.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MazeOfKnight.Game
{
    // Decorados que en el original son baldosas del fondo y el escenario de Hollow Knight no pinta (solo dibuja la
    // colisión): las lápidas de cada mundo (la de la palabra del jefe, una tablilla que brilla), las entradas a los
    // santuarios del castillo (cuando no las tapa una piedra; las tiendas, una cueva de la zona) y el marco de las
    // puertas de los mundos. Sprites de
    // Assets/Art/Resources/Hollow/decorados (tools/hollow/decorados.py), con los pies abajo en el centro.
    public partial class GameBootstrap
    {
        readonly List<GameObject> decorViews = new List<GameObject>();
        readonly Dictionary<(int room, int x, int y), GameObject> shrineDoorViews = new Dictionary<(int, int, int), GameObject>();

        static Sprite Decor(string name) => Resources.Load<Sprite>("Hollow/decorados/" + name);

        GameObject Place(RoomModel room, Sprite sprite, int x, int y, int order, string name, bool flip = false)
        {
            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = order;
            sr.flipX = flip;
            var lit = Resources.Load<Material>("Materials/SpriteLit");
            if (lit != null) sr.sharedMaterial = lit;
            // A 8 texels por píxel del original, como el resto del arte de Hollow Knight.
            float k = 16f / RoomCoords.PixelsPerUnit;
            go.transform.localScale = new Vector3(k, k, 1);
            var p = RoomCoords.PixelToWorld(room, x, y);
            go.transform.position = new Vector3(p.x, p.y, 0);
            decorViews.Add(go);
            return go;
        }

        // Suelo que se deja libre de adornos del escenario (Visuals.HollowScenery.Reserved): (x, y) en píxeles del original,
        // y la del suelo. Las puertas de los mundos y las entradas, con margen a los lados; bancos, Cornifer y lápidas.
        bool DecorReserved(RoomModel room, float x, float y)
        {
            bool Near(float x0, float x1, float floor) => x >= x0 && x <= x1 && Mathf.Abs(y - floor) <= 8;
            if (room.doorWorld > 0 && room.door != null && Near(room.door.x - 12, room.door.x + 44, room.door.y + 24)) return true;
            foreach (var s in shrines.shrines)
                if (s.map == mapName && s.room == room.id && s.entry.how == "down" && Near(s.entry.godX - 16, s.entry.godX + 32, s.entry.godY + 16))
                    return true;
            foreach (var b in BenchList.In(mapName, room.id))
                if (Near(b.X - 28, b.X + 28, b.Y)) return true;
            if (RemakeRules.HollowMaps && CorniferList.In(Inventory.CurrentWorld, room.id) is Cornifers.Spot c && Near(c.X - 24, c.X + 24, c.Y))
                return true;
            int e041 = maps.TryGetValue(mapName, out var md) ? md.e041 : -1;
            foreach (var t in Weapons.Data.magnifier.tombstones)
                if (t.e041 == e041 && t.room == room.id && Near(t.x - 8, t.x + 24, t.y + 16)) return true;
            return false;
        }

        void ShowDecor()
        {
            foreach (var go in decorViews) if (go != null) Destroy(go);
            decorViews.Clear();
            shrineDoorViews.Clear();
            if (World == null || Decor("lapida") == null) return;
            // Lápidas del mundo: de 2 × 2 casillas desde (x, y).
            int e041 = maps.TryGetValue(mapName, out var md) ? md.e041 : -1;
            foreach (var t in Weapons.Data.magnifier.tombstones)
            {
                if (t.e041 != e041 || !(World.Room(t.room) is RoomModel room)) continue;
                var go = Place(room, Decor(t.kind > 0 ? "lapida_palabra" : "lapida"), t.x + 8, t.y + 16, 3, $"Lápida (sala {t.room})");
                if (t.kind > 0) Glow(go);
            }
            // Entradas a los santuarios del castillo (↓ delante): de 2 × 2 casillas desde (godX, godY).
            foreach (var s in shrines.shrines)
            {
                if (s.map != mapName || s.entry.how != "down" || !(World.Room(s.room) is RoomModel room)) continue;
                // Las tiendas, una boca de cueva, la de la zona de Hollow Knight del mapa si la hay (lo pidió el usuario).
                var sprite = s.kind == "shop"
                    ? Decor("puerta_tienda_" + Visuals.HollowScenery.LookFor(mapName).Zone) ?? Decor("puerta_tienda")
                    : Decor("puerta_santuario");
                var door = Place(room, sprite, s.entry.godX + 8, s.entry.godY + 16, 2,
                    $"Entrada al santuario (sala {s.room})");
                shrineDoorViews[(s.room, s.entry.godX, s.entry.godY)] = door;
            }
            UpdateShrineDoors();
            // Marco de las puertas de los mundos: la puerta ocupa 4 × 3 casillas desde (door.x, door.y), con la de jefe de
            // Hollow Knight en el hueco del centro (MapView.ShowDoor); alrededor, una fachada de Hollow Knight según el
            // mundo (tools/hollow/decorados.py; sin ella, columnas a los lados y un arco encima).
            foreach (var room in World.Rooms)
            {
                if (room.doorWorld <= 0 || room.door == null) continue;
                int x = room.door.x, y = room.door.y;
                if ((Decor($"puerta_mundo_{room.doorWorld}") ?? Decor("puerta_mundo")) is Sprite frame)
                    // La fachada del mundo al que lleva (o la de los Creyentes), apoyada en el suelo de la puerta, con el
                    // hueco en el del original y detrás del sello.
                {
                    var go = Place(room, frame, x + 16, y + 24, -9, $"Puerta (mundo {room.doorWorld})");   // por debajo del sello (−7)
                    // Que quepa entre el suelo y el techo (si no, el techo la corta): el aire sobre el centro de la puerta.
                    int free = 0;
                    while (free < 160 && !World.Solid(room, x + 16, y + 23 - free) && !World.Solid(room, x + 4, y + 23 - free) &&
                           !World.Solid(room, x + 28, y + 23 - free)) free++;
                    float tall = frame.bounds.size.y * go.transform.localScale.y * RoomCoords.PixelsPerUnit;   // px del original
                    if (tall > free + 4 && tall > 0) go.transform.localScale *= Mathf.Max(0.5f, (free + 4) / tall);
                    // Estandartes de entrada a los lados de la fachada, de pie en el suelo (si no hay pared ahí), delante de ella.
                    float half = frame.bounds.size.x * go.transform.localScale.x * RoomCoords.PixelsPerUnit / 2;
                    if (Decor("estandarte") is Sprite banner)
                        foreach (int side in new[] { -1, 1 })
                        {
                            int bx = x + 16 + side * Mathf.RoundToInt(half + 4);
                            if (bx < 4 || bx > 252 || World.Solid(room, bx, y + 12) || !World.Solid(room, bx, y + 24)) continue;
                            var b = Place(room, banner, bx, y + 24, -8, $"Estandarte (mundo {room.doorWorld})", side > 0);
                            float bt = banner.bounds.size.y * b.transform.localScale.y * RoomCoords.PixelsPerUnit;
                            if (bt > free + 4 && bt > 0) b.transform.localScale *= Mathf.Max(0.5f, (free + 4) / bt);
                        }
                }
                else
                {
                    Place(room, Decor("columna"), x + 3, y + 24, 2, $"Columna (mundo {room.doorWorld})");
                    Place(room, Decor("columna"), x + 29, y + 24, 2, $"Columna (mundo {room.doorWorld})", true);
                    Place(room, Decor("arco"), x + 16, y + 6, 2, $"Arco (mundo {room.doorWorld})");
                }
                DoorLight(room, x + 16, y + 12);
            }
        }

        // Las entradas tapadas por una piedra sin romper no se ven (en el original la piedra las esconde).
        void UpdateShrineDoors()
        {
            if (rocks == null) return;
            foreach (var kv in shrineDoorViews)
            {
                bool hidden = false;
                foreach (var r in rocks.All)
                    if (!r.broken && r.room.id == kv.Key.room && Mathf.Abs(r.x - kv.Key.x) < 16 && Mathf.Abs(r.y - kv.Key.y) < 16) hidden = true;
                if (kv.Value != null) kv.Value.SetActive(!hidden);
            }
        }

        // La puerta de un mundo, iluminada para que se distinga (lo pidió el usuario): un halo que late detrás de la puerta
        // y una luz 2D que alumbra el marco y la roca de alrededor.
        void DoorLight(RoomModel room, int x, int y)
        {
            var go = new GameObject("Luz de la puerta");
            var p = RoomCoords.PixelToWorld(room, x, y);
            go.transform.position = new Vector3(p.x, p.y, 0);
            decorViews.Add(go);
            var halo = go.AddComponent<SpriteRenderer>();
            halo.sprite = Resources.Load<Sprite>("Hollow/inventario/luz");
            halo.sortingOrder = 41;                    // como las luces de Hollow Knight, atraviesa la oscuridad (HollowAmbience)
            halo.color = new Color(0.75f, 0.85f, 1f, 0.5f);
            go.transform.localScale = new Vector3(3.2f, 3.2f, 1);
            go.AddComponent<Visuals.Pulse>();
            var lightGo = new GameObject("Luz 2D");
            lightGo.transform.SetParent(go.transform, false);
            lightGo.transform.localScale = Vector3.one / 3.2f;
            var l = lightGo.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.targetSortingLayers = SortingLayer.layers.Select(sl => sl.id).ToArray();
            l.color = new Color(0.75f, 0.85f, 1f);
            l.intensity = 1.4f;
            l.pointLightOuterRadius = 56f / RoomCoords.PixelsPerUnit;
            l.pointLightInnerRadius = 12f / RoomCoords.PixelsPerUnit;
        }

        // La tablilla de la palabra: un brillo suave detrás, que late.
        static void Glow(GameObject go)
        {
            var halo = new GameObject("Brillo");
            halo.transform.SetParent(go.transform, false);
            var sr = halo.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Hollow/inventario/luz");
            sr.sortingOrder = 1;
            sr.color = new Color(0.8f, 0.85f, 1f, 0.45f);
            halo.transform.localPosition = new Vector3(0, 0.9f, 0);
            halo.transform.localScale = new Vector3(1.6f, 1.6f, 1);
        }
    }
}
