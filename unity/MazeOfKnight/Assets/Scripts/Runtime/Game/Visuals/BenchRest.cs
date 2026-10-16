using System.Collections.Generic;
using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game.Visuals
{
    // El efecto de sentarse en un banco (RemakeRules.HollowBenches): solo motas de luz (el usuario quitó el destello, el
    // polvo y el halo): un anillo que se abre al sentarse y, mientras está sentado, motas que suben despacio.
    // Solo aspecto.
    public class BenchRest : MonoBehaviour
    {
        public GameBootstrap game;
        PlayerController player;
        Material additive;
        readonly List<(SpriteRenderer sr, Vector2 pos, float t, float life, float vx)> motes = new List<(SpriteRenderer, Vector2, float, float, float)>();
        readonly Stack<SpriteRenderer> pool = new Stack<SpriteRenderer>();
        bool wasSeated;
        float moteTimer;
        static Sprite dot;

        void Start()
        {
            player = FindAnyObjectByType<PlayerController>();
            var add = Resources.Load<Shader>("Shaders/Additive");
            if (add != null) additive = new Material(add) { name = "Banco (aditivo)" };
            dot ??= Radial(16, 0.35f);

        }

        static Sprite Radial(int n, float core)
        {
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                    float a = d <= core ? 1 : Mathf.Pow(Mathf.Clamp01(1 - (d - core) / (1 - core)), 2);
                    t.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            t.Apply(false, true);
            return Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
        }

        SpriteRenderer New(string name, int order)
        {
            var sr = pool.Count > 0 ? pool.Pop() : new GameObject(name).AddComponent<SpriteRenderer>();
            sr.gameObject.SetActive(true);
            sr.sortingOrder = order;
            if (additive != null) sr.sharedMaterial = additive;
            return sr;
        }

        void LateUpdate()
        {
            if (game == null || game.Motor == null || player == null) return;
            var m = game.Motor;
            bool seated = player.Seated;
            var at = RoomCoords.PixelToWorld(m.Room, m.X, m.Y - 10);
            if (seated && !wasSeated) Burst(m);
            wasSeated = seated;
            // Motas que suben mientras descansa.
            if (seated && (moteTimer -= Time.deltaTime) <= 0)
            {
                moteTimer = 0.12f;
                var sr = New("Mota del banco", 9);
                sr.sprite = dot;
                var p = at + new Vector2(Random.Range(-1.4f, 1.4f), Random.Range(-0.6f, 0.2f));
                motes.Add((sr, p, 0, Random.Range(1.6f, 2.6f), Random.Range(-0.08f, 0.08f)));
            }
            for (int i = motes.Count - 1; i >= 0; i--)
            {
                var (sr, pos, t, life, vx) = motes[i];
                t += Time.deltaTime;
                if (t >= life) { sr.gameObject.SetActive(false); pool.Push(sr); motes.RemoveAt(i); continue; }
                pos += new Vector2(vx + Mathf.Sin(t * 3 + i) * 0.05f, 0.45f) * Time.deltaTime;
                float a = Mathf.Clamp01(t / 0.3f) * Mathf.Clamp01((life - t) / 0.6f);
                sr.transform.position = new Vector3(pos.x, pos.y, 0);
                sr.transform.localScale = Vector3.one * 0.14f;
                sr.color = new Color(1, 1, 1, 0.85f * a);
                motes[i] = (sr, pos, t, life, vx);
            }
        }

        // Al sentarse: un anillo de motas que se abre.
        void Burst(PlayerMotor m)
        {
            var mid = RoomCoords.PixelToWorld(m.Room, m.X, m.Y - 10);
            for (int i = 0; i < 14; i++)
            {
                var sr = New("Mota del banco", 9);
                sr.sprite = dot;
                float a = i / 14f * Mathf.PI * 2;
                motes.Add((sr, mid + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.5f) * 0.5f, 0, Random.Range(0.8f, 1.4f), Mathf.Cos(a) * 0.5f));
            }
        }
    }
}
