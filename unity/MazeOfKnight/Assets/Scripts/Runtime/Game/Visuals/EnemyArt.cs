using System;
using System.Collections.Generic;
using MazeOfKnight.Game.Enemies;
using UnityEngine;
using static MazeOfKnight.Game.Visuals.ShapePainter;

namespace MazeOfKnight.Game.Visuals
{
    // Enemigos del remake (docs/design_bible/12_estilo_visual.md, "Enemigos"): arte provisional pintado por código, por
    // arquetipos según cómo se mueve cada tipo (la clase de Enemies/), con el lenguaje de los personajes: caparazón
    // oscuro, máscara clara con ojos huecos, contorno de tinta y un color de acento por tipo. 2 fotogramas cada uno.
    // El dibujo ocupa la caja que pinta el juego (Enemy.DrawRect, escalada desde 16 × 16): no cambia ninguna colisión.
    public class EnemyArt
    {
        public enum Kind { Beetle, Worm, Winged, Wisp, Ball, Fish, Flame, Stone, Robe, Spikes, Orb, Bone, Dart, Plant, Loot, Burst, Baby, Fairy }

        [Flags]
        public enum Mod { None = 0, Horns = 1, Shield = 2, Spear = 4, Skeleton = 8, Wide = 16, Spiky = 32, Shell = 64, Rock = 128, Bomb = 256, Water = 512, Mouth = 1024, Piston = 2048, Arrow = 4096, Coins = 8192, Keys = 16384, Head = 32768 }

        const int S = 4, Px = 16;
        readonly Dictionary<(Kind, Mod, int, int), Sprite> cache = new Dictionary<(Kind, Mod, int, int), Sprite>();

        static Color C(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        static readonly Color Shell = C("#272a3a"), ShellLight = C("#3b4058"), Mask = C("#ece6da"), MaskShade = C("#bfb7a6"), Hole = C("#08080d"),
            Bonec = C("#e8e2cf"), Metal = C("#b9c1d0"), Stonec = C("#5b5d68"), StoneLight = C("#7c7f8c");

        static readonly Color[] Accents =
        {
            C("#e8873a"), C("#d2453c"), C("#6fc36a"), C("#9a6ad6"), C("#4f8fe0"), C("#e6c84a"), C("#3fb8a8"), C("#e06a9a"), C("#a8c8f0"),
        };

        // ---------- qué dibujo lleva cada enemigo ----------

        public static (Kind kind, Mod mod, int accent) Classify(Enemy e)
        {
            string n = e.GetType().Name;
            int acc = Math.Abs(n.GetHashCode() + e.Type * 7) % Accents.Length;
            switch (n)
            {
                case "Charger": case "SlowCharger": return (Kind.Beetle, Mod.Horns, acc);
                case "Runner": case "FallingWalker": case "Wader": case "LevelJumper": case "PlatformJumper": case "Hopper": case "Mimic": return (Kind.Beetle, Mod.None, acc);
                case "Knight": return (Kind.Beetle, Mod.Shield, 8);
                case "Soldier": return (Kind.Beetle, Mod.Spear, 4);
                case "Skeleton": return (Kind.Beetle, Mod.Skeleton | Mod.Shield, 8);
                case "WideWalker": return (Kind.Beetle, Mod.Wide | Mod.Horns, acc);
                case "Crawler": case "CeilingSlug": case "InchWorm": case "Snake": case "CeilingClimber": return (Kind.Worm, Mod.None, acc);
                case "Bat": case "FusingBat": case "DiveBird": case "SplitFly": case "BounceBug": case "Weaver": return (Kind.Winged, Mod.None, acc);
                case "Fairy": return (Kind.Fairy, Mod.None, 8);
                case "Ghost": case "Phantom": case "Soul": case "BigSpirit": case "SpiralGhost": return (Kind.Wisp, Mod.None, acc);
                case "FloatingHead": return (Kind.Wisp, Mod.Head, acc);
                case "Orbiter": case "Bubble": case "SporeCloud": case "Spore": case "Spore2": case "GaliousOrb": case "MonsterBall":
                case "SummonSpark": case "DriftShot": case "Shot": case "EightWayShot": case "FanShot": case "LipShot":
                    return (Kind.Orb, Mod.None, acc);
                case "Hedgehog": case "Roller": case "ChainOrb": return (Kind.Ball, Mod.Spiky, acc);
                case "Armadillo": return (Kind.Ball, Mod.Shell, acc);
                case "GrowingBall": case "BouncingBall": return (Kind.Ball, Mod.None, acc);
                case "RollingStone": case "BabyRock": return (Kind.Ball, Mod.Rock, acc);
                case "RollingBomb": return (Kind.Ball, Mod.Bomb, 1);
                case "LeapFish": case "WaterMonster": return (Kind.Fish, Mod.None, acc);
                case "FlameMan": case "FireSpitter": case "Ember": case "FlameRow": return (Kind.Flame, Mod.None, 0);
                case "Lightning": case "Beam": return (Kind.Flame, Mod.None, 8);
                case "Geyser": case "WaterJet": case "Splash": case "Droplet": return (Kind.Flame, Mod.Water, 4);
                case "Piston": return (Kind.Stone, Mod.Piston, acc);
                case "WallMouth": return (Kind.Stone, Mod.Mouth, 1);
                case "ArrowTrap": return (Kind.Stone, Mod.Arrow, acc);
                case "Statue": case "Sentinel": case "Emerger": case "Lurker": case "Block": return (Kind.Stone, Mod.None, acc);
                case "Sorcerer": case "Teleporter": return (Kind.Robe, Mod.None, acc);
                case "Spikes": return (Kind.Spikes, Mod.None, acc);
                case "Bone": return (Kind.Bone, Mod.None, 8);
                case "Dart": return (Kind.Dart, Mod.None, acc);
                case "Sprout": return (Kind.Plant, Mod.None, 2);
                case "LootDrop": return (Kind.Loot, e.Type == 0x5D ? Mod.Arrow : e.Type == 0x5E ? Mod.Coins : Mod.Keys, 5);
                case "DeathExplosion": return (Kind.Burst, Mod.None, 5);
                case "Baby": return (Kind.Baby, Mod.None, 7);
            }
            // Jefes sin su imagen importada y lo que no esté en la lista: un escarabajo con cuernos.
            return (Kind.Beetle, e is Boss ? Mod.Horns | Mod.Wide : Mod.None, acc);
        }

        public Sprite For(Enemy e, float time)
        {
            var (kind, mod, acc) = Classify(e);
            float speed = kind == Kind.Winged || kind == Kind.Fairy ? 8f : kind == Kind.Flame || kind == Kind.Burst ? 10f : 3f;
            int frame = ((int)(time * speed) + (e.GetHashCode() & 7)) & 1;
            return Get(kind, mod, acc, frame);
        }

        public Sprite Get(Kind kind, Mod mod, int accent, int frame)
        {
            var key = (kind, mod, accent, frame);
            if (!cache.TryGetValue(key, out var s))
                cache[key] = s = Trimmed(Paint(kind, mod, Accents[accent % Accents.Length], frame));
            return s;
        }

        // Sprite recortado a la silueta (lo opaco, con el contorno): el juego lo estira a la caja de colisión, así lo que
        // se ve es lo que golpea, como en el original, donde la caja sale del propio sprite.
        static Sprite Trimmed(Texture2D tex)
        {
            int x0 = tex.width, y0 = tex.height, x1 = -1, y1 = -1;
            var px = tex.GetPixels32();
            for (int y = 0; y < tex.height; y++)
                for (int x = 0; x < tex.width; x++)
                    if (px[y * tex.width + x].a > 40)
                    {
                        if (x < x0) x0 = x;
                        if (x > x1) x1 = x;
                        if (y < y0) y0 = y;
                        if (y > y1) y1 = y;
                    }
            if (x1 < 0) return ToSprite(tex, S * Px, Vector2.zero);
            return Sprite.Create(tex, new Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1), Vector2.zero, S * Px, 0, SpriteMeshType.FullRect);
        }

        // ---------- pintura (en una caja de 16 × 16 px, mirando a la derecha) ----------

        static Texture2D Paint(Kind kind, Mod mod, Color acc, int f)
        {
            var cv = new ColorCanvas(Px * S, Px * S, S, 0, 0);
            switch (kind)
            {
                case Kind.Beetle: Beetle(cv, mod, acc, f); break;
                case Kind.Worm: Worm(cv, acc, f); break;
                case Kind.Winged: Winged(cv, acc, f); break;
                case Kind.Wisp: Wisp(cv, mod, acc, f); break;
                case Kind.Ball: Ball(cv, mod, acc, f); break;
                case Kind.Fish: Fish(cv, acc, f); break;
                case Kind.Flame: Flame(cv, mod, acc, f); break;
                case Kind.Stone: Stone(cv, mod, acc, f); break;
                case Kind.Robe: Robe(cv, acc, f); break;
                case Kind.Spikes: Spikes(cv, f); break;
                case Kind.Orb: Orb(cv, acc, f); break;
                case Kind.Bone: BoneShape(cv, f); break;
                case Kind.Dart: DartShape(cv, acc); break;
                case Kind.Plant: Plant(cv, acc, f); break;
                case Kind.Loot: Loot(cv, mod, f); break;
                case Kind.Burst: Burst(cv, acc, f); break;
                case Kind.Baby: Baby(cv, f); break;
                case Kind.Fairy: FairyShape(cv, f); break;
            }
            if (kind != Kind.Burst && kind != Kind.Orb && kind != Kind.Flame && kind != Kind.Fairy)
                cv.Ink(new Color(0.03f, 0.03f, 0.05f), 2);
            return cv.ToTexture();
        }

        static float Ellipse(float x, float y, float cx, float cy, float rx, float ry)
        {
            float u = (x - cx) / rx, v = (y - cy) / ry;
            return (Mathf.Sqrt(u * u + v * v) - 1) * Mathf.Min(rx, ry);
        }

        // Máscara clara de perfil con un ojo hueco (y otro si se ve de frente).
        static void MaskFace(ColorCanvas cv, float cx, float cy, float r, bool twoEyes, Color tint)
        {
            var m = Color.Lerp(Mask, tint, 0.12f);
            cv.Fill((x, y) => Circle(x, y, cx, cy, r), m, cx - r, cy - r, cx + r, cy + r);
            cv.Fill((x, y) => Mathf.Max(Circle(x, y, cx - r * 0.35f, cy - r * 0.3f, r * 0.85f), Circle(x, y, cx, cy, r)), new Color(MaskShade.r, MaskShade.g, MaskShade.b, 0.7f),
                cx - r, cy - r, cx + r, cy + r);
            float ey = cy + r * 0.1f;
            cv.Fill((x, y) => Ellipse(x, y, cx + r * 0.38f, ey, r * 0.24f, r * 0.34f), Hole, cx, cy - r, cx + r, cy + r);
            if (twoEyes) cv.Fill((x, y) => Ellipse(x, y, cx - r * 0.3f, ey, r * 0.22f, r * 0.32f), Hole, cx - r, cy - r, cx, cy + r);
        }

        static void Beetle(ColorCanvas cv, Mod mod, Color acc, int f)
        {
            bool wide = (mod & Mod.Wide) != 0, bone = (mod & Mod.Skeleton) != 0;
            float rx = wide ? 6.4f : 5.2f, cx = wide ? 7.2f : 6.8f, cy = 6.4f;
            var shell = bone ? Bonec * 0.85f + new Color(0, 0, 0, 0.15f) : Shell;
            // Patas (alternan).
            for (int i = 0; i < 3; i++)
            {
                float lx = cx - rx * 0.6f + i * rx * 0.6f, step = (i + f) % 2 == 0 ? 0.9f : -0.9f;
                cv.Fill((x, y) => Segment(x, y, lx, cy - 1.5f, lx + step, 0.7f, 0.55f), bone ? Bonec : Shell, lx - 2, 0, lx + 2, cy);
            }
            // Caparazón con una franja de color.
            cv.Fill((x, y) => Mathf.Max(Ellipse(x, y, cx, cy, rx, 4.6f), 2.4f - y), shell, cx - rx, 2, cx + rx, cy + 5);
            if (bone)
                for (int i = -1; i <= 1; i++)
                {
                    float rxx = cx + i * 1.8f;
                    cv.Fill((x, y) => Box(x, y, rxx, cy + 0.6f, 0.35f, 2.6f), Hole, rxx - 1, cy - 3, rxx + 1, cy + 4);
                }
            else
            {
                cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Ellipse(x, y, cx, cy - 0.4f, rx - 1.2f, 3.6f)) - 0.5f, cy - 0.3f - y), acc, cx - rx, cy, cx + rx, cy + 5);
                cv.Fill((x, y) => Ellipse(x, y, cx - 1.6f, cy + 2.4f, 1.6f, 0.7f), new Color(1, 1, 1, 0.25f), cx - 4, cy + 1, cx, cy + 4);
            }
            // Máscara delante.
            float mx = cx + rx - 0.8f, my = cy + 0.2f;
            MaskFace(cv, mx, my, 2.7f, false, acc);
            if ((mod & Mod.Horns) != 0)
            {
                cv.Fill((x, y) => Segment(x, y, mx - 0.6f, my + 2.0f, mx - 1.8f, my + 5.4f, 0.5f), Mask, mx - 3, my + 1, mx + 1, my + 7);
                cv.Fill((x, y) => Segment(x, y, mx + 0.8f, my + 2.0f, mx + 2.4f, my + 4.9f, 0.5f), Mask, mx, my + 1, mx + 4, my + 7);
            }
            if ((mod & Mod.Shield) != 0)
            {
                cv.Fill((x, y) => Circle(x, y, 14.3f, 5.0f, 2.3f), Metal * 0.75f + new Color(0, 0, 0, 0.25f), 11.5f, 2.5f, 16, 7.5f);
                cv.Fill((x, y) => Circle(x, y, 14.4f, 5.1f, 1.7f), Metal, 12, 3, 16, 7);
            }
            if ((mod & Mod.Spear) != 0)
            {
                float t = f == 0 ? 0 : 0.8f;
                cv.Fill((x, y) => Segment(x, y, 2.5f + t, 10.5f, 13.8f + t, 6.0f, 0.3f), C("#8a6a3a"), 2, 5, 15, 11);
                cv.Fill((x, y) => Mathf.Max(Mathf.Max(Mathf.Abs(y - 6.0f) - (15.8f + t - x) * 0.5f, x - (15.8f + t)), 13.4f + t - x), Metal, 13, 4.5f, 16, 7.5f);
            }
        }

        static void Worm(ColorCanvas cv, Color acc, int f)
        {
            for (int i = 0; i < 5; i++)
            {
                float cx = 2.6f + i * 2.3f, cy = 3.0f + Mathf.Sin(i * 1.3f + f * Mathf.PI) * 0.6f, r = 2.0f + i * 0.12f;
                cv.Fill((x, y) => Circle(x, y, cx, cy, r), i % 2 == 0 ? Shell : ShellLight, cx - r, cy - r, cx + r, cy + r);
                cv.Fill((x, y) => Mathf.Max(Circle(x, y, cx, cy, r), cy + r * 0.4f - y), acc, cx - r, cy, cx + r, cy + r);
            }
            MaskFace(cv, 13.2f, 3.6f, 2.6f, false, acc);
        }

        static void Winged(ColorCanvas cv, Color acc, int f)
        {
            float up = f == 0 ? 1 : -1;
            // Ala de atrás, cuerpo, ala de delante.
            Wing(cv, 7.2f, 8.4f, up, Shell * 0.8f + new Color(0, 0, 0, 0.2f), -1.2f);
            cv.Fill((x, y) => Circle(x, y, 7.6f, 7.6f, 3.4f), Shell, 4, 4, 11, 11);
            cv.Fill((x, y) => Mathf.Max(Circle(x, y, 7.6f, 7.6f, 3.4f), y - 6.6f), acc, 4, 4, 11, 7);
            MaskFace(cv, 10.6f, 8.2f, 2.2f, false, acc);
            Wing(cv, 6.6f, 8.8f, up, ShellLight, 0);
        }

        static void Wing(ColorCanvas cv, float bx, float by, float up, Color color, float dx)
        {
            float tipY = by + 5.2f * up, midY = by + 3.0f * up;
            cv.Fill((x, y) => Mathf.Min(Mathf.Min(Segment(x, y, bx, by, bx - 4.6f + dx, tipY, 0.9f), Segment(x, y, bx, by, bx - 6.2f + dx, midY, 0.8f)),
                    Segment(x, y, bx, by, bx - 2.4f + dx, tipY + up * 0.6f, 0.7f)), color, 0, Mathf.Min(by, tipY) - 2, bx + 1, Mathf.Max(by, tipY) + 2);
        }

        static void Wisp(ColorCanvas cv, Mod mod, Color acc, int f)
        {
            var body = Color.Lerp(Color.white, acc, 0.25f);
            body.a = 0.92f;
            float sway = f == 0 ? 0.8f : -0.8f;
            if ((mod & Mod.Head) == 0)
                for (int k = 0; k < 3; k++)
                {
                    float tx = 6.4f + k * 1.6f, tail = 2.0f + k * 0.6f;
                    cv.Fill((x, y) => Segment(x, y, tx, 7.5f, tx + sway * (k - 1) * 0.8f + sway, tail, 0.9f - k * 0.1f), body, 2, 0, 14, 9);
                }
            cv.Fill((x, y) => Circle(x, y, 8, 9.2f, 4.6f), body, 3, 4, 13, 14);
            cv.Fill((x, y) => Ellipse(x, y, 6.6f, 9.6f, 0.8f, 1.3f), Hole, 5, 8, 8, 11);
            cv.Fill((x, y) => Ellipse(x, y, 9.6f, 9.6f, 0.8f, 1.3f), Hole, 8, 8, 11, 11);
            if ((mod & Mod.Head) != 0)
                cv.Fill((x, y) => Ellipse(x, y, 8.1f, 6.6f, 1.4f, f == 0 ? 0.5f : 0.9f), Hole, 6, 5, 10, 8);
        }

        static void Ball(ColorCanvas cv, Mod mod, Color acc, int f)
        {
            float cx = 8, cy = 8, r = 5.6f;
            if ((mod & Mod.Spiky) != 0)
            {
                for (int k = 0; k < 8; k++)
                {
                    float a = k * Mathf.PI / 4 + f * Mathf.PI / 8, ex = cx + Mathf.Cos(a) * 7.4f, ey = cy + Mathf.Sin(a) * 7.4f;
                    cv.Fill((x, y) => Segment(x, y, cx, cy, ex, ey, 0.7f), Mask, 0, 0, 16, 16);
                }
                r = 5f;
            }
            var bodyC = (mod & Mod.Rock) != 0 ? Stonec : (mod & Mod.Bomb) != 0 ? C("#1a1a22") : Shell;
            cv.Fill((x, y) => Circle(x, y, cx, cy, r), bodyC, cx - r, cy - r, cx + r, cy + r);
            if ((mod & Mod.Rock) != 0)
            {
                cv.Fill((x, y) => Mathf.Max(Circle(x, y, cx - 1, cy + 1.2f, r - 1.2f), Circle(x, y, cx, cy, r)), StoneLight, cx - r, cy - r, cx + r, cy + r);
                cv.Fill((x, y) => Segment(x, y, cx - 2, cy + 3, cx + 1, cy - 1, 0.25f), Hole, cx - 3, cy - 2, cx + 2, cy + 4);
            }
            else if ((mod & Mod.Shell) != 0)
                for (int k = -1; k <= 1; k++)
                {
                    float bx = cx + k * 2.4f + (f == 0 ? 0 : 0.6f);
                    cv.Fill((x, y) => Mathf.Max(Mathf.Abs(x - bx) - 0.4f, Circle(x, y, cx, cy, r - 0.4f)), acc, cx - r, cy - r, cx + r, cy + r);
                }
            else if ((mod & Mod.Bomb) != 0)
            {
                cv.Fill((x, y) => Segment(x, y, cx + 2.8f, cy + 4.2f, cx + 4.4f, cy + 6.4f, 0.35f), C("#8a6a3a"), cx + 2, cy + 3, cx + 5, cy + 7);
                cv.Fill((x, y) => Circle(x, y, cx + 4.6f, cy + 6.8f, f == 0 ? 1.0f : 0.6f), C("#ffd36a"), cx + 3, cy + 5, cx + 6, cy + 8);
            }
            else
                cv.Fill((x, y) => Mathf.Max(Mathf.Abs(Circle(x, y, cx, cy, r - 1.4f)) - 0.6f, cy - y), acc, cx - r, cy, cx + r, cy + r);
            if ((mod & (Mod.Rock | Mod.Bomb)) == 0)
            {
                cv.Fill((x, y) => Ellipse(x, y, cx + 1.4f, cy + 0.6f, 0.7f, 1.1f), Hole, cx, cy - 1, cx + 3, cy + 2);
                cv.Fill((x, y) => Ellipse(x, y, cx - 1.0f, cy + 0.6f, 0.7f, 1.1f), Hole, cx - 2, cy - 1, cx, cy + 2);
            }
            cv.Fill((x, y) => Circle(x, y, cx - 2.2f, cy + 2.6f, 0.9f), new Color(1, 1, 1, 0.35f), cx - 4, cy + 1, cx, cy + 4);
        }

        static void Fish(ColorCanvas cv, Color acc, int f)
        {
            float tail = f == 0 ? 1.4f : -1.4f;
            cv.Fill((x, y) => Mathf.Min(Segment(x, y, 3.4f, 7.5f, 0.8f, 7.5f + tail + 2.2f, 0.8f), Segment(x, y, 3.4f, 7.5f, 0.8f, 7.5f + tail - 2.2f, 0.8f)),
                ShellLight, 0, 3, 5, 12);
            cv.Fill((x, y) => Ellipse(x, y, 8.2f, 7.5f, 5.6f, 3.6f), Shell, 2.4f, 3.8f, 14, 11.2f);
            cv.Fill((x, y) => Mathf.Max(Ellipse(x, y, 8.2f, 7.5f, 5.6f, 3.6f), y - 6.4f), acc, 2.4f, 3.8f, 14, 7);
            cv.Fill((x, y) => Segment(x, y, 7.4f, 10.6f, 5.2f, 12.6f, 0.7f), ShellLight, 4, 10, 9, 13.5f);
            MaskFace(cv, 12.0f, 7.9f, 2.4f, false, acc);
        }

        static void Flame(ColorCanvas cv, Mod mod, Color acc, int f)
        {
            bool water = (mod & Mod.Water) != 0;
            Color outer = water ? C("#4fa6e8") : acc, mid = water ? C("#a8dcff") : C("#ffd36a"), core = Color.white;
            float sway = f == 0 ? 1.0f : -1.0f;
            void Tongue(float r, float top, Color col)
            {
                cv.Fill((x, y) => Mathf.Min(Circle(x, y, 8, 5, r), Spike(x, y, 8, 5, r * 0.95f, top) - 0.2f), col, 0, 0, 16, 16);
                cv.Fill((x, y) => Spike(x, y, 8 + sway, 5, r * 0.7f, top - 0.6f), col, 0, 0, 16, 16);
            }
            Tongue(4.6f, 15.5f, outer);
            Tongue(3.1f, 12.5f, mid);
            Tongue(1.7f, 9.5f, core);
            if (!water)
            {
                cv.Fill((x, y) => Ellipse(x, y, 7.0f, 6.0f, 0.5f, 0.8f), Hole, 6, 5, 8, 7);
                cv.Fill((x, y) => Ellipse(x, y, 9.0f, 6.0f, 0.5f, 0.8f), Hole, 8, 5, 10, 7);
            }
        }

        static void Stone(ColorCanvas cv, Mod mod, Color acc, int f)
        {
            if ((mod & Mod.Piston) != 0)
            {
                cv.Fill((x, y) => Box(x, y, 8, 5.5f, 1.6f, 5.5f), Metal * 0.7f + new Color(0, 0, 0, 0.3f), 5, 0, 11, 11);
                cv.Fill((x, y) => Box(x, y, 8, 13.4f, 6.4f, 2.4f) - 0.4f, Stonec, 1, 10, 15, 16);
                cv.Fill((x, y) => Box(x, y, 8, 14.4f, 5.6f, 1.0f), StoneLight, 2, 13, 14, 16);
                return;
            }
            cv.Fill((x, y) => Box(x, y, 8, 8, 6.6f, 6.6f) - 0.8f, Stonec, 0.6f, 0.6f, 15.4f, 15.4f);
            cv.Fill((x, y) => Mathf.Max(Box(x, y, 8, 8, 6.6f, 6.6f) - 0.8f, 10.5f - y), StoneLight, 0.6f, 10, 15.4f, 15.4f);
            cv.Fill((x, y) => Segment(x, y, 3, 4, 5.5f, 6.5f, 0.2f), Hole, 2, 3, 6, 7);
            // Cara tallada con los ojos encendidos.
            var glow = Color.Lerp(acc, Color.white, f == 0 ? 0.35f : 0.6f);
            cv.Fill((x, y) => Ellipse(x, y, 5.8f, 9.0f, 1.1f, 0.8f), glow, 4, 8, 8, 10);
            cv.Fill((x, y) => Ellipse(x, y, 10.2f, 9.0f, 1.1f, 0.8f), glow, 8, 8, 12, 10);
            if ((mod & Mod.Mouth) != 0)
                cv.Fill((x, y) => Ellipse(x, y, 8, 5.0f, 3.6f, f == 0 ? 1.2f : 2.2f), Hole, 4, 2, 12, 8);
            else if ((mod & Mod.Arrow) != 0)
                cv.Fill((x, y) => Circle(x, y, 8, 5.0f, 1.3f), Hole, 6, 3, 10, 7);
            else
                cv.Fill((x, y) => Box(x, y, 8, 5.2f, 2.4f, 0.35f), Hole, 5, 4, 11, 6);
        }

        static void Robe(ColorCanvas cv, Color acc, int f)
        {
            cv.Fill((x, y) => Mathf.Max(Spike(x, y, 8, 0.4f, 6.0f, 15f), y - 12f), Shell, 1, 0, 15, 13);
            cv.Fill((x, y) => Mathf.Max(Mathf.Max(Spike(x, y, 8, 0.4f, 6.0f, 15f), y - 2.0f), -0.1f), acc, 1, 0, 15, 2.5f);
            cv.Fill((x, y) => Circle(x, y, 8.4f, 11.4f, 3.4f), ShellLight, 4.5f, 8, 12.5f, 15);
            MaskFace(cv, 9.0f, 11.0f, 2.2f, true, acc);
            float sy = f == 0 ? 0 : 0.6f;
            cv.Fill((x, y) => Segment(x, y, 13.4f, 1, 13.4f, 13 + sy, 0.35f), C("#8a6a3a"), 12.5f, 0, 14.5f, 14.5f);
            cv.Fill((x, y) => Circle(x, y, 13.4f, 13.6f + sy, 1.1f), Color.Lerp(acc, Color.white, 0.5f), 12, 12, 15, 15.5f);
        }

        static void Spikes(ColorCanvas cv, int f)
        {
            for (int k = 0; k < 4; k++)
            {
                float cx = 2 + k * 4;
                cv.Fill((x, y) => Spike(x, y, cx, 0.4f, 1.8f, 15f), Metal, cx - 2, 0, cx + 2, 15);
                cv.Fill((x, y) => Spike(x, y, cx - 0.5f, 0.4f, 0.6f, 13.5f), Color.white, cx - 1.5f, 0, cx, 14);
            }
        }

        static void Orb(ColorCanvas cv, Color acc, int f)
        {
            float r = f == 0 ? 6.0f : 5.2f;
            cv.Fill((x, y) => Circle(x, y, 8, 8, r), new Color(acc.r, acc.g, acc.b, 0.85f), 8 - r, 8 - r, 8 + r, 8 + r);
            cv.Fill((x, y) => Circle(x, y, 8, 8, r * 0.6f), Color.Lerp(acc, Color.white, 0.6f), 2, 2, 14, 14);
            cv.Fill((x, y) => Circle(x, y, 8, 8, r * 0.28f), Color.white, 4, 4, 12, 12);
        }

        static void BoneShape(ColorCanvas cv, int f)
        {
            float a = f == 0 ? 0.5f : -0.5f;
            float x0 = 8 - 5 * Mathf.Cos(a), y0 = 8 - 5 * Mathf.Sin(a), x1 = 8 + 5 * Mathf.Cos(a), y1 = 8 + 5 * Mathf.Sin(a);
            cv.Fill((x, y) => Mathf.Min(Segment(x, y, x0, y0, x1, y1, 1.1f),
                    Mathf.Min(Mathf.Min(Circle(x, y, x0 - 0.8f, y0 + 0.8f, 1.5f), Circle(x, y, x0 + 0.8f, y0 - 0.8f, 1.5f)),
                              Mathf.Min(Circle(x, y, x1 - 0.8f, y1 + 0.8f, 1.5f), Circle(x, y, x1 + 0.8f, y1 - 0.8f, 1.5f)))), Bonec, 0, 0, 16, 16);
        }

        static void DartShape(ColorCanvas cv, Color acc)
        {
            cv.Fill((x, y) => Segment(x, y, 1.5f, 8, 12, 8, 0.45f), C("#8a6a3a"), 1, 7, 13, 9);
            cv.Fill((x, y) => Mathf.Max(Mathf.Max(Mathf.Abs(y - 8) - (15.5f - x) * 0.55f, x - 15.5f), 11.5f - x), Metal, 11, 6, 16, 10);
            cv.Fill((x, y) => Mathf.Min(Segment(x, y, 1.5f, 8, 3.5f, 10, 0.4f), Segment(x, y, 1.5f, 8, 3.5f, 6, 0.4f)), acc, 0, 5, 5, 11);
        }

        static void Plant(ColorCanvas cv, Color acc, int f)
        {
            float sway = f == 0 ? 0.6f : -0.6f;
            cv.Fill((x, y) => Segment(x, y, 8, 0.5f, 8 + sway, 8, 0.7f), C("#3c6b3a"), 6, 0, 10, 9);
            cv.Fill((x, y) => Ellipse(x, y, 5.2f, 4.0f, 2.6f, 1.0f), acc, 2, 2.5f, 8, 5.5f);
            cv.Fill((x, y) => Ellipse(x, y, 10.8f, 5.2f, 2.6f, 1.0f), acc, 8, 3.5f, 14, 7);
            cv.Fill((x, y) => Circle(x, y, 8 + sway, 10.5f, 3.6f), Shell, 4, 6.5f, 12.5f, 14.5f);
            cv.Fill((x, y) => Ellipse(x, y, 9.6f + sway, 10.2f, 1.8f, f == 0 ? 0.6f : 1.2f), Hole, 7.5f, 8.5f, 12, 12);
            cv.Fill((x, y) => Circle(x, y, 7.4f + sway, 12.0f, 0.6f), Mask, 6.5f, 11, 8.5f, 13);
        }

        static void Loot(ColorCanvas cv, Mod mod, int f)
        {
            if ((mod & Mod.Coins) != 0)
            {
                var gold = C("#e8c04a");
                cv.Fill((x, y) => Ellipse(x, y, 8, 7, f == 0 ? 4.6f : 2.6f, 4.6f), gold * 0.7f + new Color(0, 0, 0, 0.3f), 2, 2, 14, 12);
                cv.Fill((x, y) => Ellipse(x, y, 8, 7.2f, f == 0 ? 3.6f : 1.8f, 3.6f), gold, 3, 3, 13, 11);
                cv.Fill((x, y) => Circle(x, y, 6.8f, 8.8f, 0.8f), Color.white, 5, 7, 9, 10);
            }
            else if ((mod & Mod.Arrow) != 0)
            {
                for (int k = -1; k <= 1; k++)
                {
                    float dy = k * 1.6f;
                    cv.Fill((x, y) => Segment(x, y, 2, 6 + dy, 12, 9 + dy, 0.35f), C("#c8a060"), 1, 4, 13, 12);
                    cv.Fill((x, y) => Circle(x, y, 12.8f, 9.3f + dy, 0.9f), Metal, 11, 7, 15, 12);
                }
            }
            else
            {
                var key = C("#7fe0ff");
                cv.Fill((x, y) => Mathf.Abs(Circle(x, y, 5, 9, 2.4f)) - 0.8f, key, 1.5f, 5.5f, 8.5f, 12.5f);
                cv.Fill((x, y) => Segment(x, y, 7, 8, 13, 5, 0.6f), key, 6, 4, 14, 9);
                cv.Fill((x, y) => Segment(x, y, 11.6f, 5.7f, 12.6f, 3.6f, 0.5f), key, 10.5f, 2.5f, 13.5f, 6.5f);
            }
        }

        static void Burst(ColorCanvas cv, Color acc, int f)
        {
            float r = f == 0 ? 4.5f : 7f;
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI / 4 + f * 0.4f, ex = 8 + Mathf.Cos(a) * r, ey = 8 + Mathf.Sin(a) * r;
                cv.Fill((x, y) => Segment(x, y, 8, 8, ex, ey, 0.9f), new Color(acc.r, acc.g, acc.b, 0.85f), 0, 0, 16, 16);
            }
            cv.Fill((x, y) => Circle(x, y, 8, 8, r * 0.45f), Color.white, 0, 0, 16, 16);
        }

        static void Baby(ColorCanvas cv, int f)
        {
            cv.Fill((x, y) => Ellipse(x, y, 7.4f, 6.4f, 5.6f, 4.4f), C("#f2e6da"), 1.5f, 2, 13.5f, 11);
            cv.Fill((x, y) => Mathf.Abs(Ellipse(x, y, 7.4f, 6.4f, 4.4f, 3.2f)) - 0.3f, C("#e8a0b4"), 2.5f, 3, 12.5f, 10);
            cv.Fill((x, y) => Circle(x, y, 11.6f, 8.2f, 2.6f), C("#ffd9c4"), 9, 5.5f, 14.5f, 11);
            cv.Fill((x, y) => Circle(x, y, 12.4f, 8.6f, f == 0 ? 0.35f : 0.15f), Hole, 11.5f, 7.5f, 13.5f, 9.5f);
        }

        static void FairyShape(ColorCanvas cv, int f)
        {
            float up = f == 0 ? 1 : 0.4f;
            var wing = new Color(0.85f, 0.95f, 1f, 0.7f);
            cv.Fill((x, y) => Ellipse(x, y, 5.0f, 10.0f + up, 3.0f, 2.0f), wing, 1, 7, 9, 14);
            cv.Fill((x, y) => Ellipse(x, y, 11.0f, 10.0f + up, 3.0f, 2.0f), wing, 7, 7, 15, 14);
            cv.Fill((x, y) => Circle(x, y, 8, 8, 3.0f), new Color(1, 1, 0.9f, 0.9f), 4, 4, 12, 12);
            cv.Fill((x, y) => Circle(x, y, 8, 8, 1.6f), Color.white, 6, 6, 10, 10);
        }

        // ---------- armas del jugador ----------

        // 1 flecha, 2 cerámica, 3 fuego rodante, 4 fuego, 5 mina.
        public Sprite Weapon(int id, int frame)
        {
            var key = (Kind.Dart, (Mod)(1 << 20 | id), 0, frame);
            if (cache.TryGetValue(key, out var s)) return s;
            var cv = new ColorCanvas(Px * S, Px * S, S, 0, 0);
            switch (id)
            {
                case 1: DartShape(cv, C("#e8e8f0")); break;
                case 2:
                    cv.Fill((x, y) => Ellipse(x, y, 8, 8, frame == 0 ? 6 : 3, 6), C("#e6d6b0"), 1, 1, 15, 15);
                    cv.Fill((x, y) => Mathf.Abs(Ellipse(x, y, 8, 8, frame == 0 ? 4 : 2, 4)) - 0.4f, C("#a0784a"), 2, 2, 14, 14);
                    break;
                case 3: case 4: Flame(cv, Mod.None, C("#ff8a3a"), frame); break;
                default: Ball(cv, Mod.Bomb, Accents[1], frame); break;
            }
            if (id == 1 || id == 2 || id == 5) cv.Ink(new Color(0.03f, 0.03f, 0.05f), 2);
            cache[key] = s = Trimmed(cv.ToTexture());
            return s;
        }
    }
}
