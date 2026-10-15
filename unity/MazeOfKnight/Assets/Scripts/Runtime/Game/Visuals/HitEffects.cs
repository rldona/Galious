using MazeOfKnight.Data;
using MazeOfKnight.Game.Enemies;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // Respuesta al golpear (docs/design_bible/12_estilo_visual.md, "Golpes"): solo aspecto, sin tocar los tiempos del
    // original (no se para el juego en el golpe).
    //  - Espada o arma contra un enemigo: chispas blancas y amarillas que salen del punto de impacto, destello del
    //    enemigo (Atmosphere) y una sacudida corta de la cámara.
    //  - Escudo ("clinc"): chispas azuladas que rebotan.
    //  - Enemigo muerto: estallido de chispas y una nube de esquirlas de su color.
    //  - Piedra golpeada: esquirlas de piedra; rota, una lluvia de trozos y polvo.
    //  - Golpe al jugador: chispas rojas y una sacudida más fuerte.
    public class HitEffects : MonoBehaviour
    {
        public static HitEffects Instance { get; private set; }

        ParticleSystem sparks, debris;

        public void Init(Material additive, Material unlit, Sprite dot)
        {
            Instance = this;
            sparks = NewSystem("Chispas", additive, dot, ParticleSystemRenderMode.Stretch, 16, 0f);
            debris = NewSystem("Esquirlas", unlit, dot, ParticleSystemRenderMode.Billboard, 14, 2.6f);
        }

        void OnDestroy() { if (Instance == this) Instance = null; }

        ParticleSystem NewSystem(string name, Material mat, Sprite tex, ParticleSystemRenderMode mode, int order, float gravity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 600;
            main.gravityModifier = gravity;
            var emission = ps.emission;
            emission.enabled = false;
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.1f));
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 0.6f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var limit = ps.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.dampen = 0.12f;
            limit.limit = 0.5f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            var m = new Material(mat) { mainTexture = tex.texture };
            r.sharedMaterial = m;
            r.renderMode = mode;
            r.velocityScale = mode == ParticleSystemRenderMode.Stretch ? 0.06f : 0;
            r.lengthScale = mode == ParticleSystemRenderMode.Stretch ? 1.4f : 1;
            r.sortingOrder = order;
            ps.Play();
            return ps;
        }

        // Ráfaga en un punto (unidades de Unity): n partículas, en un abanico de 'spread' grados alrededor de 'dir'.
        void Burst(ParticleSystem ps, Vector2 at, int n, Color a, Color b, float speed, float spread, float dirDeg, float size, float life)
        {
            var p = new ParticleSystem.EmitParams();
            for (int i = 0; i < n; i++)
            {
                float ang = (dirDeg + Random.Range(-spread, spread) / 2f) * Mathf.Deg2Rad;
                float v = speed * Random.Range(0.45f, 1.1f);
                p.position = new Vector3(at.x, at.y, 0);
                p.velocity = new Vector3(Mathf.Cos(ang) * v, Mathf.Sin(ang) * v, 0);
                p.startColor = Color.Lerp(a, b, Random.value);
                p.startSize = size * Random.Range(0.6f, 1.2f);
                p.startLifetime = life * Random.Range(0.6f, 1.1f);
                ps.Emit(p, 1);
            }
        }

        static Vector2 World(RoomModel room, float x, float y) => RoomCoords.PixelToWorld(room, Mathf.RoundToInt(x), Mathf.RoundToInt(y));

        // Punto de impacto: el borde de la caja del enemigo que mira al jugador, a la altura de la espada.
        static Vector2 ImpactPoint(Enemy e, PlayerMotor m)
        {
            var (x0, y0, x1, y1) = e.Box();
            float y = m != null && m.Room == e.Room ? Mathf.Clamp(m.Y - 5.5f, y0, y1) : (y0 + y1) / 2;
            float x = m != null && m.Room == e.Room ? (m.X < (x0 + x1) / 2 ? x0 : x1) : (x0 + x1) / 2;
            return World(e.Room, x, y);
        }

        static float Away(Enemy e, PlayerMotor m)
        {
            if (m == null) return 90;
            var (x0, _, x1, _) = e.Box();
            return m.X < (x0 + x1) / 2 ? 0 : 180;
        }

        // ---------- lo que pasa ----------

        public static void EnemyHit(Enemy e, PlayerMotor m)
        {
            if (Instance == null || e == null || !Atmosphere.Styled) return;
            var at = ImpactPoint(e, m);
            bool boss = e is Boss;
            Instance.Burst(Instance.sparks, at, boss ? 18 : 12, Color.white, new Color(1f, 0.85f, 0.35f), boss ? 7f : 6f, 110, Away(e, m), 0.14f, 0.28f);
            RoomCamera.Shake(boss ? 0.12f : 0.06f);
        }

        public static void Clink(Enemy e, PlayerMotor m)
        {
            if (Instance == null || e == null || !Atmosphere.Styled) return;
            var at = ImpactPoint(e, m);
            Instance.Burst(Instance.sparks, at, 10, new Color(0.7f, 0.85f, 1f), Color.white, 5f, 70, Away(e, m) + 180, 0.12f, 0.22f);
            RoomCamera.Shake(0.03f);
        }

        public static void EnemyKilled(Enemy e, Color tint)
        {
            if (Instance == null || e == null || !Atmosphere.Styled) return;
            var (x0, y0, x1, y1) = e.Box();
            var at = World(e.Room, (x0 + x1) / 2, (y0 + y1) / 2);
            Instance.Burst(Instance.sparks, at, 22, Color.white, new Color(1f, 0.8f, 0.4f), 7f, 360, 0, 0.16f, 0.4f);
            Instance.Burst(Instance.debris, at, 14, tint, Color.Lerp(tint, Color.black, 0.5f), 3.5f, 160, 90, 0.16f, 0.7f);
            RoomCamera.Shake(e is Boss ? 0.3f : 0.08f);
        }

        public static void RockHit(RoomModel room, int x, int y, bool broken)
        {
            if (Instance == null || !Atmosphere.Styled) return;
            var at = World(room, x + 8, y + 8);
            var stone = new Color(0.62f, 0.63f, 0.68f);
            if (broken)
            {
                Instance.Burst(Instance.debris, at, 26, stone, new Color(0.35f, 0.36f, 0.4f), 4.5f, 200, 90, 0.22f, 0.9f);
                Instance.Burst(Instance.debris, at, 10, new Color(0.8f, 0.78f, 0.72f, 0.6f), new Color(0.6f, 0.6f, 0.6f, 0.4f), 1.2f, 360, 90, 0.4f, 0.8f);
                RoomCamera.Shake(0.1f);
            }
            else
            {
                Instance.Burst(Instance.debris, at, 6, stone, Color.white, 3f, 140, 90, 0.12f, 0.45f);
                Instance.Burst(Instance.sparks, at, 5, Color.white, new Color(1f, 0.9f, 0.6f), 4f, 160, 90, 0.1f, 0.2f);
                RoomCamera.Shake(0.03f);
            }
        }

        // Puerta de mundo que se abre (WorldDoors): polvo que cae del hueco a cada paso y un temblor pequeño.
        public static void DoorStep(RoomModel room, int doorX, int doorY, int openRows)
        {
            if (Instance == null || room == null || !Atmosphere.Styled) return;
            var dust = new Color(0.78f, 0.74f, 0.66f, 0.7f);
            for (int k = 0; k < 4; k++)
            {
                var at = World(room, doorX + 9 + k * 4.5f, doorY + 24 - 8 * openRows);
                Instance.Burst(Instance.debris, at, 4, dust, new Color(0.5f, 0.48f, 0.44f, 0.5f), 1.4f, 120, 270, 0.14f, 0.7f);
            }
            RoomCamera.Shake(0.04f);
        }

        public static void PlayerHurt(PlayerMotor m)
        {
            if (Instance == null || m == null || !Atmosphere.Styled) return;
            var at = World(m.Room, m.X, m.Y - 8);
            Instance.Burst(Instance.sparks, at, 14, new Color(1f, 0.35f, 0.3f), Color.white, 5.5f, 360, 0, 0.14f, 0.3f);
            RoomCamera.Shake(0.14f);
        }
    }
}
