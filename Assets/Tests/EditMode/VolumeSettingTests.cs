using System.Collections.Generic;
using NUnit.Framework;
using ProjetoFinal.Audio;

namespace ProjetoFinal.Tests
{
    public class VolumeSettingTests
    {
        private sealed class FakeStore : IFloatStore
        {
            public readonly Dictionary<string, float> Values = new();
            public float Get(string key, float defaultValue) => Values.TryGetValue(key, out var v) ? v : defaultValue;
            public void Set(string key, float value) => Values[key] = value;
        }

        [Test]
        public void StartsWithSavedValue_OrDefault()
        {
            var store = new FakeStore();
            Assert.AreEqual(0.8f, new VolumeSetting(store, "vol", 0.8f).Value);

            store.Values["vol"] = 0.3f;
            Assert.AreEqual(0.3f, new VolumeSetting(store, "vol", 0.8f).Value);
        }

        [Test]
        public void Set_ClampsBetweenZeroAndOne()
        {
            var volume = new VolumeSetting(new FakeStore(), "vol", 0.5f);

            volume.Set(2f);
            Assert.AreEqual(1f, volume.Value);
            volume.Set(-1f);
            Assert.AreEqual(0f, volume.Value);
        }

        [Test]
        public void Set_PersistsAndRaisesEventOnlyOnChange()
        {
            var store = new FakeStore();
            var volume = new VolumeSetting(store, "vol", 0.5f);
            var raised = 0;
            volume.Changed += _ => raised++;

            volume.Set(0.25f);
            volume.Set(0.25f);

            Assert.AreEqual(1, raised);
            Assert.AreEqual(0.25f, store.Values["vol"]);
        }
    }
}
