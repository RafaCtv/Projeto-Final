using System;
using UnityEngine;

namespace ProjetoFinal.Audio
{
    public interface IFloatStore
    {
        float Get(string key, float defaultValue);
        void Set(string key, float value);
    }

    public sealed class PlayerPrefsFloatStore : IFloatStore
    {
        public float Get(string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);

        public void Set(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }
    }

    /// <summary>Volume persistido, limitado entre 0 e 1.</summary>
    public sealed class VolumeSetting
    {
        private readonly IFloatStore store;
        private readonly string key;

        public float Value { get; private set; }

        public event Action<float> Changed;

        public VolumeSetting(IFloatStore store, string key, float defaultValue)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.key = key;
            Value = Mathf.Clamp01(store.Get(key, defaultValue));
        }

        public void Set(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(value, Value))
                return;

            Value = value;
            store.Set(key, value);
            Changed?.Invoke(value);
        }
    }
}
