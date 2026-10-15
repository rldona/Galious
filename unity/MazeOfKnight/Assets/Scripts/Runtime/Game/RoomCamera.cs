using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game
{
    // Cámara fija por sala, como en el original: encuadra la sala entera (16 x 10 unidades) y salta a la
    // siguiente al cambiar de sala.
    //
    // Con el estilo del remake (Perspective) es una cámara en perspectiva, como en Hollow Knight: la sala está en z = 0,
    // a Depth unidades, y las capas del ambiente a distintas profundidades (z > 0 detrás, z < 0 delante). La cámara se
    // desplaza un poco (Shift) siguiendo al jugador y un desplazamiento del encuadre (lens shift) deja la sala clavada
    // en la pantalla: el parallax de las demás capas sale solo de su profundidad (docs/design_bible/12_estilo_visual.md,
    // "Capas de una sala").
    [RequireComponent(typeof(Camera))]
    public class RoomCamera : MonoBehaviour
    {
        public PlayerController player;
        Camera cam;

        // Distancia de la cámara al plano de la sala (z = 0).
        public const float Depth = 10f;
        public const float Near = 0.3f, Far = 80f;
        // Perspectiva (estilo del remake) u ortográfica (tiles provisionales). La pone Atmosphere.
        public static bool Perspective;
        public static RoomCamera Instance { get; private set; }

        // Desplazamiento de la cámara respecto al centro de la sala, en unidades del plano de la sala. La sala no se
        // mueve en pantalla; las capas a otra profundidad sí (ParallaxAt).
        public Vector2 Shift;
        // Medio alto y medio ancho del encuadre en el plano de la sala.
        public float HalfHeight { get; private set; } = RoomCoords.RoomSize.y / 2f;
        public float HalfWidth { get; private set; } = RoomCoords.RoomSize.x / 2f;
        public Vector2 Center { get; private set; }

        // Sacudida al golpear (Visuals/HitEffects, solo con el estilo del remake): amplitud en unidades, se apaga sola.
        static float shake;
        public static void Shake(float amount) => shake = Mathf.Max(shake, amount);

        // Tamaño aparente de un plano a profundidad z respecto al de la sala: para que cubra lo mismo en pantalla hay
        // que escalarlo por este factor.
        public static float ScaleAt(float z) => (Depth + z) / Depth;

        // Cuánto se desplaza en pantalla, respecto a la sala, un plano a profundidad z cuando la cámara se mueve 1
        // unidad: positivo detrás (sigue a la cámara), negativo delante.
        public static float ParallaxAt(float z) => z / (Depth + z);

        // Matriz de proyección en perspectiva que muestra exactamente [−halfW, halfW] × [−halfH, halfH] del plano a
        // Depth con la cámara desplazada shift: el encuadre se corre en sentido contrario (lens shift).
        public static Matrix4x4 Projection(float halfW, float halfH, Vector2 shift, float near = Near, float far = Far)
        {
            float k = near / Depth;
            return Matrix4x4.Frustum((-halfW - shift.x) * k, (halfW - shift.x) * k, (-halfH - shift.y) * k, (halfH - shift.y) * k, near, far);
        }

        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            // Las capas van a distinta profundidad: el orden lo decide sortingOrder, no la distancia.
            cam.transparencySortMode = TransparencySortMode.Orthographic;
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void LateUpdate()
        {
            if (player == null || player.Motor == null)
                return;
            var room = player.Motor.Room;
            Center = RoomCoords.RoomOrigin(room) + RoomCoords.RoomSize / 2f;
            // Que quepa la sala entera sea cual sea la proporción de la ventana.
            Fit(cam.aspect);
            var shift = Perspective ? Shift : Vector2.zero;
            transform.position = new Vector3(Center.x + shift.x, Center.y + shift.y, -Depth);
            if (shake > 0.001f)
            {
                transform.position += (Vector3)(Random.insideUnitCircle * shake);
                shake *= Mathf.Exp(-Time.deltaTime * 22f);
            }
            else shake = 0;
            ApplyProjection(cam.aspect);
        }

        void Fit(float aspect)
        {
            float byHeight = RoomCoords.RoomSize.y / 2f;
            float byWidth = RoomCoords.RoomSize.x / 2f / Mathf.Max(aspect, 0.01f);
            HalfHeight = Mathf.Max(byHeight, byWidth);
            HalfWidth = HalfHeight * Mathf.Max(aspect, 0.01f);
        }

        // Proyección para una proporción dada (las capturas a textura la piden antes de Camera.Render).
        public void ApplyProjection(float aspect)
        {
            if (cam == null) cam = GetComponent<Camera>();
            Fit(aspect);
            if (Perspective)
            {
                cam.orthographic = false;
                cam.nearClipPlane = Near;
                cam.farClipPlane = Far;
                cam.fieldOfView = 2f * Mathf.Atan(HalfHeight / Depth) * Mathf.Rad2Deg;
                cam.projectionMatrix = Projection(HalfWidth, HalfHeight, Shift);
            }
            else
            {
                cam.ResetProjectionMatrix();
                cam.orthographic = true;
                cam.orthographicSize = HalfHeight;
            }
        }
    }
}
