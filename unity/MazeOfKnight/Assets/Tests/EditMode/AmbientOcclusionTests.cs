using MazeOfKnight.Game;
using MazeOfKnight.Game.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Oclusión ambiental a partir de la colisión (Visuals/AmbientOcclusion) y proyección de la cámara en perspectiva
    // (RoomCamera): la sala tiene que quedar clavada en pantalla aunque la cámara se desplace.
    public class AmbientOcclusionTests
    {
        const int Cols = 8, Rows = 6, T = AmbientOcclusion.TexelsPerTile;

        // Alfa del texel que está en la columna col (en tiles, desde la izquierda) y a la altura y (en tiles, desde abajo).
        static float At(float[] a, float col, float y) => a[(int)(y * T) * Cols * T + (int)(col * T)];

        [Test]
        public void OpenAirHasNoOcclusionAndSolidIsLeftAlone()
        {
            var a = AmbientOcclusion.Compute((c, r) => false, Cols, Rows);
            Assert.AreEqual(Cols * T * Rows * T, a.Length);
            Assert.That(a, Is.All.EqualTo(0f));
            // Suelo en la fila de abajo: dentro del macizo no se pinta nada.
            var floor = AmbientOcclusion.Compute((c, r) => r == Rows - 1, Cols, Rows);
            Assert.AreEqual(0f, At(floor, 4, 0.5f));
        }

        [Test]
        public void OcclusionFadesWithHeightAboveTheFloor()
        {
            var a = AmbientOcclusion.Compute((c, r) => r == Rows - 1, Cols, Rows);
            float touching = At(a, 4.1f, 1.1f), one = At(a, 4.1f, 2.1f), two = At(a, 4.1f, 3.1f);
            Assert.Greater(touching, 0.25f);
            Assert.Greater(touching, one);
            Assert.Greater(one, 0f);
            Assert.AreEqual(0f, two);
            Assert.LessOrEqual(touching, AmbientOcclusion.MaxAlpha);
        }

        [Test]
        public void CornersAreDarkerThanFlatFloor()
        {
            var a = AmbientOcclusion.Compute((c, r) => r == Rows - 1 || c == 0, Cols, Rows);
            float corner = At(a, 1.1f, 1.1f), flat = At(a, 5.1f, 1.1f);
            Assert.Greater(corner, flat);
        }

        [Test]
        public void HashChangesWhenAWallOpens()
        {
            int closed = AmbientOcclusion.Hash((c, r) => r == Rows - 1 || c == 3, Cols, Rows);
            int open = AmbientOcclusion.Hash((c, r) => r == Rows - 1, Cols, Rows);
            Assert.AreNotEqual(closed, open);
            Assert.AreEqual(closed, AmbientOcclusion.Hash((c, r) => r == Rows - 1 || c == 3, Cols, Rows));
        }

        [Test]
        public void PerspectiveKeepsTheRoomPlaneFixedWhileTheCameraShifts()
        {
            // Encuadre de 8 × 5 unidades a Depth; la cámara se corre (0,7, −0,4) y el lens shift lo compensa en z = 0.
            float halfW = 8, halfH = 5;
            var shift = new Vector2(0.7f, -0.4f);
            var proj = RoomCamera.Projection(halfW, halfH, shift);
            // Vista: cámara en (shift, −Depth) mirando a +z (misma convención que Matrix4x4.Frustum: z de la cámara negativa hacia delante).
            var view = Matrix4x4.TRS(new Vector3(shift.x, shift.y, -RoomCamera.Depth), Quaternion.identity, new Vector3(1, 1, -1)).inverse;
            Vector2 Screen(Vector3 world)
            {
                var clip = proj * view * new Vector4(world.x, world.y, world.z, 1);
                return new Vector2(clip.x / clip.w, clip.y / clip.w);
            }
            // Las esquinas del plano de la sala caen en las esquinas de la pantalla.
            var tl = Screen(new Vector3(-halfW, halfH, 0));
            var br = Screen(new Vector3(halfW, -halfH, 0));
            Assert.AreEqual(-1f, tl.x, 1e-4f);
            Assert.AreEqual(1f, tl.y, 1e-4f);
            Assert.AreEqual(1f, br.x, 1e-4f);
            Assert.AreEqual(-1f, br.y, 1e-4f);
            // Un punto a profundidad z detrás se corre con la cámara en proporción ParallaxAt(z) (en unidades de la sala).
            float z = 10;
            var back = Screen(new Vector3(0, 0, z));
            float expectedX = shift.x * RoomCamera.ParallaxAt(z) / halfW;
            Assert.AreEqual(expectedX, back.x, 1e-4f);
            Assert.AreEqual(0.5f, RoomCamera.ParallaxAt(10), 1e-6f);
            Assert.AreEqual(2f, RoomCamera.ScaleAt(10), 1e-6f);
        }
    }
}
