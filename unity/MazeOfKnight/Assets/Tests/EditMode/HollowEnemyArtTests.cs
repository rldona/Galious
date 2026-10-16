using MazeOfKnight.Game.Visuals;
using NUnit.Framework;

namespace MazeOfKnight.Tests
{
    // Tamaño de los enemigos con dibujo de Hollow Knight (Visuals/HollowEnemyArt.Scale): una sola escala para los dos ejes
    // (no se deforman) y su cuerpo no pasa de K veces su caja de contacto original ni por arriba ni por los lados.
    public class HollowEnemyArtTests
    {
        static void Check(float k, float refH, float refW, float w, float h)
        {
            var info = new HollowEnemyArt.Info { K = k, RefHeight = refH, RefWidth = refW };
            float s = HollowEnemyArt.Scale(info, w, h);
            float bodyH = refH * s / 8f, bodyW = refW * s / 8f;    // en píxeles del original
            string what = $"cuerpo {refW}×{refH} texels en caja {w}×{h} px, K = {k}: dibujado {bodyW:0.0}×{bodyH:0.0} px";
            Assert.LessOrEqual(bodyH, k * h + 1e-3f, what);
            Assert.LessOrEqual(bodyW, k * w + 1e-3f, what);
            // Toca el límite en uno de los dos ejes: no se queda más pequeño de lo necesario.
            Assert.IsTrue(UnityEngine.Mathf.Approximately(bodyH, k * h) || UnityEngine.Mathf.Approximately(bodyW, k * w), what);
            // La misma escala en los dos ejes: la proporción del cuerpo se conserva.
            Assert.AreEqual(refW / refH, bodyW / bodyH, 1e-4f, what);
        }

        [Test]
        public void TallBodyIsLimitedByHeight() => Check(1.3f, 200, 80, 16, 16);

        [Test]
        public void WideBodyIsLimitedByWidth() => Check(1.3f, 100, 680, 26, 16);

        [Test]
        public void WideBoxAndTallBody() => Check(1.25f, 110, 102, 26, 16);
    }
}
