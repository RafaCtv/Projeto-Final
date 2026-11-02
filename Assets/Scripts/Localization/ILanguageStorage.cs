using UnityEngine;

namespace ProjetoFinal.Localization
{
    public interface ILanguageStorage
    {
        string Load();
        void Save(string languageCode);
    }

    public sealed class PlayerPrefsLanguageStorage : ILanguageStorage
    {
        private const string Key = "settings.language";

        public string Load() => PlayerPrefs.GetString(Key, null);

        public void Save(string languageCode)
        {
            PlayerPrefs.SetString(Key, languageCode);
            PlayerPrefs.Save();
        }
    }
}
