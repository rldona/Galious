using System.Collections.Generic;
using MazeOfKnight.Game.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace MazeOfKnight.Tests
{
    // Reproductor a 24 fps del arte dibujado (Visuals/FrameAnimator) y agrupación de PNG por nombre (ArtLibrary).
    public class FrameAnimatorTests
    {
        static FrameClip Clip(int n, bool loop, string[] tags = null) =>
            new FrameClip { Name = "prueba", Frames = new Sprite[n], Loop = loop, Tags = tags };

        [Test]
        public void TwentyFourFramesPerSecond()
        {
            var a = new FrameAnimator();
            a.Play(Clip(24, true));
            Assert.AreEqual(0, a.Frame);
            a.Tick(1f / 24f + 0.001f);
            Assert.AreEqual(1, a.Frame);
            a.Tick(0.5f);
            Assert.AreEqual(13, a.Frame);
        }

        [Test]
        public void LoopWrapsAndNonLoopStopsAtLastFrame()
        {
            var loop = new FrameAnimator();
            loop.Play(Clip(4, true));
            loop.Tick(5f / 24f + 0.001f);
            Assert.AreEqual(1, loop.Frame);
            Assert.IsFalse(loop.Finished);

            var once = new FrameAnimator();
            once.Play(Clip(4, false));
            once.Tick(10f);
            Assert.AreEqual(3, once.Frame);
            Assert.IsTrue(once.Finished);
        }

        [Test]
        public void EveryFrameIsEnteredOnceEvenWhenSkipped()
        {
            var a = new FrameAnimator();
            var entered = new List<int>();
            a.FrameEntered += (c, f) => entered.Add(f);
            a.Play(Clip(6, false));
            a.Tick(4.5f / 24f);          // un fotograma lento salta del 0 al 4
            Assert.AreEqual(new[] { 0, 1, 2, 3, 4 }, entered);
            a.Tick(1f);
            Assert.AreEqual(new[] { 0, 1, 2, 3, 4, 5 }, entered);
        }

        [Test]
        public void PlayAtStartsOnTheTaggedFrame()
        {
            var clip = Clip(5, false, new[] { null, null, "golpe", null, null });
            Assert.AreEqual(2, clip.TagFrame("golpe"));
            Assert.AreEqual(-1, clip.TagFrame("pisada"));
            var a = new FrameAnimator();
            a.PlayAt(clip, clip.TagFrame("golpe"));
            Assert.AreEqual(2, a.Frame);
            a.Tick(1f / 24f + 0.001f);
            Assert.AreEqual(3, a.Frame);
            // Play del mismo clip sin reiniciar sigue donde está.
            a.Play(clip);
            Assert.AreEqual(3, a.Frame);
            a.Play(clip, restart: true);
            Assert.AreEqual(0, a.Frame);
        }

        [Test]
        public void LibraryGroupsFilesByName()
        {
            Assert.IsTrue(ArtLibrary.Parse("popolon_correr_03", out var clip, out int n, out var tag));
            Assert.AreEqual("popolon_correr", clip);
            Assert.AreEqual(3, n);
            Assert.IsNull(tag);
            Assert.IsTrue(ArtLibrary.Parse("popolon_estocada_01_golpe", out clip, out n, out tag));
            Assert.AreEqual("popolon_estocada", clip);
            Assert.AreEqual(1, n);
            Assert.AreEqual("golpe", tag);
            Assert.IsTrue(ArtLibrary.Parse("enemigo25_andar_00", out clip, out n, out tag));
            Assert.AreEqual("enemigo25_andar", clip);
            Assert.AreEqual(0, n);
            Assert.IsNull(tag);
            Assert.IsTrue(ArtLibrary.Parse("mundo05_fondo_medio", out clip, out n, out tag));
            Assert.AreEqual("mundo05_fondo_medio", clip);
            Assert.AreEqual(0, n);
            Assert.IsNull(tag);

            var lib = new ArtLibrary(new (string, Sprite)[]
            {
                ("popolon_correr_02", null), ("popolon_correr_00", null), ("popolon_correr_01", null),
                ("popolon_estocada_00", null), ("popolon_estocada_01_golpe", null), ("mundo05_fondo_medio", null),
            });
            Assert.AreEqual(3, lib.Count);
            Assert.AreEqual(3, lib.Clip("popolon_correr").Length);
            Assert.IsNull(lib.Clip("popolon_correr").Tags);
            Assert.AreEqual(1, lib.Clip("popolon_estocada").TagFrame("golpe"));
            Assert.AreEqual(1, lib.Clip("mundo05_fondo_medio").Length);
            Assert.IsNull(lib.Clip("aphrodite_correr"));
        }
    }
}
