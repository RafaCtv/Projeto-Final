using UnityEngine;

namespace ProjetoFinal.Audio
{
    /// <summary>Volumes persistidos do jogo. O geral é aplicado em <see cref="AudioListener.volume"/>.</summary>
    public static class AudioSettingsService
    {
        private const string MasterKey = "settings.volume.master";
        private const float DefaultMaster = 0.8f;

        private static VolumeSetting master;

        public static VolumeSetting Master
        {
            get
            {
                if (master == null)
                {
                    master = new VolumeSetting(new PlayerPrefsFloatStore(), MasterKey, DefaultMaster);
                    master.Changed += ApplyMaster;
                    ApplyMaster(master.Value);
                }
                return master;
            }
        }

        private static void ApplyMaster(float value) => AudioListener.volume = value;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            master = null;
            _ = Master;
        }
    }
}
