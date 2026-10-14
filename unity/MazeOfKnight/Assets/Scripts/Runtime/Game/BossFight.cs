using System;
using System.Collections.Generic;
using System.Linq;
using MazeOfKnight.Data;
using MazeOfKnight.Game.Enemies;

namespace MazeOfKnight.Game
{
    // Combate contra el jefe de un mundo (docs/boss_ai.md, "Invocación" y "Derrota"). En la sala del jefe, sin haberlo
    // vencido, se escucha el teclado: al escribir su palabra (las últimas letras del búfer de 16; mayúsculas y
    // minúsculas dan igual) se cierran las salidas y empieza la invocación (193 frames ≈ 97 ticks); luego salen chispas
    // cada 8 ticks durante 65 y el jefe aparece cuando no queda ningún enemigo. Al vencerlo, 128 frames (64 ticks) de
    // disolución y cae la Gran Llave; las salidas no se abren hasta cogerla. Después, la celebración (480 frames).
    public class BossFight
    {
        public enum Stage { Idle, Invoking, Summoning, Fighting, Dissolving, KeyOnFloor, Fanfare, RightExitOnly }

        readonly BossesData data;
        Stage current = Stage.Idle;
        public Stage Current
        {
            get => current;
            private set { if (value != current) StageTicks = 0; current = value; }
        }
        // Ticks desde que empezó la etapa actual (los efectos de BossEffects van por frames: × 2).
        public int StageTicks { get; private set; }
        // Caja del jefe al vencerlo, para la disolución en puntos.
        public (float x0, float y0, float x1, float y1) LastBossRect { get; private set; }
        public BossInfo Info { get; private set; }
        public RoomModel Room { get; private set; }
        public int World { get; private set; }
        public Boss Boss { get; private set; }
        public (int x, int y) KeyPos { get; private set; }
        public readonly HashSet<int> Defeated = new HashSet<int>();   // mundos con el jefe vencido (E061 + mundo, bit 2)
        string buffer = "";
        char lastKey;
        int timer, sparkIndex;

        public event Action<string> Message;

        public BossFight(BossesData data) => this.data = data ?? new BossesData();

        public BossInfo InfoFor(int world) => data.ForWorld(world);

        public bool Listening => Current == Stage.Idle && Info != null && Room != null && Info.room == Room.id && !Defeated.Contains(World);
        public bool Active => Current != Stage.Idle;
        public bool LocksExits => Current != Stage.Idle;
        // Tras Galious (sin llave): solo se abre la salida derecha, que lleva a la sala 6 (E64B = 6, docs/ending.md).
        public bool RightExitOpen => Current == Stage.RightExitOnly;
        // Estados 15, 19 y 20: el juego no corre (invocación, disolución y celebración).
        public bool FreezesPlayer => Current == Stage.Invoking || Current == Stage.Dissolving || Current == Stage.Fanfare;
        public bool InFight => Current == Stage.Summoning || Current == Stage.Fighting;

        // Al entrar en una sala (world 0 = castillo).
        public void EnterRoom(int world, RoomModel room)
        {
            if (Current == Stage.RightExitOnly && room != Room) Current = Stage.Idle;
            if (Active && room == Room) return;
            World = world;
            Room = room;
            Info = world > 0 ? data.ForWorld(world) : null;
            buffer = "";
            lastKey = '\0';
        }

        // Una tecla de letra o número. El original lee el teclado una vez por tick y solo cuenta una letra si cambia: repetir
        // la misma pide soltarla antes (un tick sin teclas, KeysReleased) o pulsar otra entre medias. Así la repetición
        // automática del sistema tampoco escribe letras de más.
        public void Type(char ch)
        {
            if (!Listening || !char.IsLetterOrDigit(ch)) return;
            ch = char.ToUpperInvariant(ch);
            if (ch == lastKey) return;
            lastKey = ch;
            buffer += char.ToUpperInvariant(ch);
            int max = Math.Max(1, data.invocation.bufferLength);
            if (buffer.Length > max) buffer = buffer.Substring(buffer.Length - max);
            if (!string.IsNullOrEmpty(Info.word) && buffer.EndsWith(Info.word.ToUpperInvariant()))
            {
                Current = Stage.Invoking;
                timer = (data.invocation.totalFrames + 1) / 2;
                Message?.Invoke($"¡{Info.word}!");
            }
        }

        // Un tick sin ninguna tecla pulsada: la letra siguiente cuenta aunque sea la misma.
        public void KeysReleased() => lastKey = '\0';

        public void Tick(EnemySystem enemies, Func<Boss> spawnBoss)
        {
            StageTicks++;
            switch (Current)
            {
                case Stage.Invoking:
                    if (--timer > 0) break;
                    Current = Stage.Summoning;
                    timer = data.summon.countdownTicks;
                    break;
                case Stage.Summoning:
                    timer--;
                    var pos = data.summon.sparkPositions;
                    if (timer >= 0 && (timer & 7) == 0 && pos.Length > 0 && enemies.Enemies.Count(e => e.Alive && e is SummonSpark) < data.summon.maxSparks)
                    {
                        var p = pos[(++sparkIndex & 7) % pos.Length];
                        enemies.Spawn(0x63, p.x, p.y);
                    }
                    if (timer <= 0 && enemies.AliveCount == 0)
                    {
                        Boss = spawnBoss();
                        Current = Stage.Fighting;
                        Message?.Invoke(Info.name);
                    }
                    break;
                case Stage.Dissolving:
                    if (--timer > 0) break;
                    var key = data.defeat.keyPositions.FirstOrDefault(k => k.world == World);
                    if (Info.dropsKey && key != null)
                    {
                        KeyPos = (key.x, key.y);
                        Current = Stage.KeyOnFloor;
                    }
                    else if (Room.afterBossRight >= 0)
                    {
                        Room.right = Room.afterBossRight;
                        Current = Stage.RightExitOnly;
                    }
                    else Finish();
                    break;
                case Stage.Fanfare:
                    if (--timer == data.keyFanfare.messageFrames / 2)
                        Message?.Invoke(data.keyFanfare.message.Replace(" n ", $" {World} "));
                    if (timer <= 0) Finish();
                    break;
            }
        }

        public void OnBossDefeated()
        {
            if (Current != Stage.Fighting) return;
            Defeated.Add(World);
            if (Boss != null) LastBossRect = Boss.DrawRect();
            Boss = null;
            Current = Stage.Dissolving;
            timer = data.defeat.dissolveFrames / 2;
            Message?.Invoke($"{Info.name} derrotado");
        }

        // El jugador coge la Gran Llave del suelo.
        public void KeyTaken()
        {
            if (Current != Stage.KeyOnFloor) return;
            Current = Stage.Fanfare;
            timer = data.keyFanfare.totalFrames / 2;
        }

        // El jugador muere en pleno combate: reaparece en la sala y el jefe se vuelve a invocar sin la palabra, con
        // toda su vida.
        public void PlayerDied()
        {
            if (!InFight) return;
            Boss = null;
            Current = Stage.Summoning;
            timer = data.summon.countdownTicks;
        }

        void Finish()
        {
            Current = Stage.Idle;
            Boss = null;
        }
    }
}
