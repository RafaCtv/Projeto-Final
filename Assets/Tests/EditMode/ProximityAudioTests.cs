using NUnit.Framework;
using ProjetoFinal.Audio;

namespace ProjetoFinal.Tests
{
    public class ProximityAudioTests
    {
        [Test]
        public void Volume_IsMaxWhenNear_ZeroWhenFar_AndGrowsWhileApproaching()
        {
            Assert.AreEqual(0.6f, ProximityAudio.VolumeAt(1f, 2f, 22f, 0.6f), 1e-4f);
            Assert.AreEqual(0f, ProximityAudio.VolumeAt(30f, 2f, 22f, 0.6f), 1e-4f);
            Assert.AreEqual(0.3f, ProximityAudio.VolumeAt(12f, 2f, 22f, 0.6f), 1e-4f);
            Assert.Greater(ProximityAudio.VolumeAt(5f, 2f, 22f, 0.6f), ProximityAudio.VolumeAt(15f, 2f, 22f, 0.6f));
        }

        [Test]
        public void Volume_DegenerateRange_IsStep()
        {
            Assert.AreEqual(1f, ProximityAudio.VolumeAt(2f, 5f, 5f, 1f), 1e-4f);
            Assert.AreEqual(0f, ProximityAudio.VolumeAt(6f, 5f, 5f, 1f), 1e-4f);
        }
    }
}
