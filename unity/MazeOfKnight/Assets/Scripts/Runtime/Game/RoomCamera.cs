using MazeOfKnight.Data;
using UnityEngine;

namespace MazeOfKnight.Game
{
    // Cámara fija por sala, como en el original: encuadra la sala entera (16 x 10 unidades) y salta a la
    // siguiente al cambiar de sala.
    [RequireComponent(typeof(Camera))]
    public class RoomCamera : MonoBehaviour
    {
        public PlayerController player;
        Camera cam;

        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
        }

        void LateUpdate()
        {
            if (player == null || player.Motor == null)
                return;
            var room = player.Motor.Room;
            var center = RoomCoords.RoomOrigin(room) + RoomCoords.RoomSize / 2f;
            transform.position = new Vector3(center.x, center.y, -10f);
            // Que quepa la sala entera sea cual sea la proporción de la ventana.
            float byHeight = RoomCoords.RoomSize.y / 2f;
            float byWidth = RoomCoords.RoomSize.x / 2f / Mathf.Max(cam.aspect, 0.01f);
            cam.orthographicSize = Mathf.Max(byHeight, byWidth);
        }
    }
}
