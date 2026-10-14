using System.Collections.Generic;
using MazeOfKnight.Data;
using P = MazeOfKnight.Game.OriginalPhysics;

namespace MazeOfKnight.Game
{
    // Piedras del castillo (docs/entities.md): bloque de 2 x 2 tiles en (x, y), 15 de vida. Cada golpe de
    // espada quita 5 (Popolon) o 1 (Aphrodite). Rota, se vacía su hueco y queda a la vista el objeto que
    // hubiera debajo. Lo roto no vuelve (en el original, un bit por piedra); aquí dura la sesión.
    public class Rocks
    {
        public class Rock
        {
            public RoomModel room;
            public int x, y, hp = P.RockHp;
            public bool broken;
        }

        readonly List<Rock> rocks = new List<Rock>();
        readonly HashSet<(string, int, int, int)> brokenForever;
        readonly string mapName;

        public event System.Action<Rock> Broken;
        public event System.Action<Rock> Hit;

        public Rocks(string mapName, RoomWorld world, HashSet<(string, int, int, int)> broken)
        {
            this.mapName = mapName;
            brokenForever = broken;
            foreach (var room in world.Rooms)
                foreach (var r in room.rocks)
                {
                    var rock = new Rock { room = room, x = r.x, y = r.y };
                    if (broken.Contains((mapName, room.id, r.x, r.y)))
                    {
                        rock.broken = true;
                        world.Clear(room, r.x >> 3, (r.y - P.RoomTopPx) >> 3, 2, 2);
                    }
                    rocks.Add(rock);
                }
        }

        public IEnumerable<Rock> All => rocks;

        // Un golpe de espada recién empezado: daña las piedras de la sala que toca.
        public void Strike(PlayerMotor motor, RoomWorld world)
        {
            var (x0, x1, y0, y1) = motor.SwordRect();
            int damage = motor.Character == Character.Popolon ? P.RockDamagePopolon : P.RockDamageAphrodite;
            foreach (var rock in rocks)
            {
                if (rock.broken || rock.room != motor.Room)
                    continue;
                if (x1 < rock.x || x0 > rock.x + 15 || y1 < rock.y || y0 > rock.y + 15)
                    continue;
                rock.hp -= damage;
                Hit?.Invoke(rock);
                if (rock.hp > 0)
                    continue;
                rock.broken = true;
                brokenForever.Add((mapName, rock.room.id, rock.x, rock.y));
                world.Clear(rock.room, rock.x >> 3, (rock.y - P.RoomTopPx) >> 3, 2, 2);
                Broken?.Invoke(rock);
            }
        }

        // Un proyectil toca una piedra (caja de la piedra [y, y + 16] × [x, x + 20], docs/player.md): la daña y true.
        public bool StrikeRect(RoomModel room, RoomWorld world, int x0, int x1, int y0, int y1, int damage)
        {
            foreach (var rock in rocks)
            {
                if (rock.broken || rock.room != room)
                    continue;
                if (x1 < rock.x || x0 > rock.x + 20 || y1 < rock.y || y0 > rock.y + 16)
                    continue;
                if (damage <= 0)
                    return true;
                rock.hp -= damage;
                Hit?.Invoke(rock);
                if (rock.hp <= 0)
                {
                    rock.broken = true;
                    brokenForever.Add((mapName, rock.room.id, rock.x, rock.y));
                    world.Clear(rock.room, rock.x >> 3, (rock.y - P.RoomTopPx) >> 3, 2, 2);
                    Broken?.Invoke(rock);
                }
                return true;
            }
            return false;
        }
    }
}
