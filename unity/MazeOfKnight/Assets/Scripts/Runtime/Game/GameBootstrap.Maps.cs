using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game
{
    // Cornifer y los mapas dibujados a mano (RemakeRules.HollowMaps): las salas visitadas, hablar con Cornifer (↓ a su
    // lado, como sentarse en un banco) y comprarle el mapa del mundo.
    public partial class GameBootstrap
    {
        public readonly HashSet<(string, int)> VisitedRooms = new HashSet<(string, int)>();
        public bool CorniferMet;                                   // ya se le ha conocido (la primera vez se presenta)
        public readonly HashSet<int> CorniferGreeted = new HashSet<int>();   // mundos donde ya ha saludado
        public Visuals.CorniferView CorniferView { get; private set; }

        Dictionary<string, RoomWorld> allWorlds;
        Cornifers cornifers;

        // Todos los mapas cargados, con su colisión (bancos, Cornifer y el mapa del menú).
        public Dictionary<string, RoomWorld> AllWorlds =>
            allWorlds ??= maps.ToDictionary(kv => kv.Key, kv => new RoomWorld(kv.Value.rooms.Select(r => r.room)));

        public RoomWorld WorldFor(string map) => AllWorlds.TryGetValue(map, out var w) ? w : null;

        public Cornifers CorniferList => cornifers ??= new Cornifers(AllWorlds, w => Bosses?.InfoFor(w)?.room ?? -1, BenchList);

        public IReadOnlyList<Benches.Bench> BenchesIn(string map) => BenchList.All.Where(b => b.Map == map).ToList();

        // El mapa de ese mundo: comprado a Cornifer o encontrado (el objeto 0x0A de ese mundo).
        public bool HasMapOf(int world) =>
            world > 0 && (Inventory.CurrentWorld == world ? Inventory.Has(Inventory.Map) : Inventory.WorldItemsPacked().Contains(world * 256 + Inventory.Map));

        public Cornifers.Spot? CorniferHere => RemakeRules.HollowMaps && motor != null ? CorniferList.In(Inventory.CurrentWorld, motor.Room.id) : null;

        // Junto a Cornifer, de pie: se ofrece "Escuchar".
        public bool NearCornifer => CorniferView != null && !CorniferView.Talking && player != null && !player.Seated &&
                                    motor.State == MotorState.Ground && !Bosses.InFight &&
                                    CorniferList.At(Inventory.CurrentWorld, motor.Room.id, motor.X, motor.Y) != null;

        void TickCornifer()
        {
            if (!RemakeRules.HollowMaps || motor == null || motor.Room == null) return;
            if (!Busy) VisitedRooms.Add((mapName, motor.Room.id));
            if (NearCornifer && motor.LastInput.DownPressed && CorniferHere is Cornifers.Spot c)
            {
                motor.Place(World, motor.Room, motor.X, c.Y);
                CorniferView.Talk(c.World);
            }
        }

        // Comprar el mapa del mundo en el que se está. Devuelve false si no llega el geo.
        public bool BuyMap(int world)
        {
            int price = Cornifers.Price(world);
            if (Coins < price) return false;
            Coins -= price;
            Inventory.Add(Inventory.Map);
            Sound.Effect(0x2D);
            Say($"Mapa del mundo {world} (−{price} geo)");
            return true;
        }

        // Para las capturas.
        public void DebugGoToCornifer(int world)
        {
            var c = CorniferList.All.FirstOrDefault(s => s.World == world);
            if (c.Room == 0) return;
            player.Seated = false;
            motor.Pose = null;
            string map = Benches.MapName(world);
            if (map != mapName) GoToMap(map);
            motor.Place(World, World.Room(c.Room), c.X + 24, c.Y);
        }
    }
}
