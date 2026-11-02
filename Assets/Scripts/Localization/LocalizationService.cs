using System;
using UnityEngine;

namespace ProjetoFinal.Localization
{
    /// <summary>Acesso global ao <see cref="Localizer"/> configurado em <see cref="LocalizationSettings"/>.</summary>
    public static class LocalizationService
    {
        private static Localizer instance;
        private static LocalizationSettings settings;

        public static event Action LanguageChanged;

        public static LocalizationSettings Settings =>
            settings != null ? settings : settings = Resources.Load<LocalizationSettings>(LocalizationSettings.ResourcesPath);

        public static Localizer Instance
        {
            get
            {
                if (instance == null)
                    Use(CreateDefault());
                return instance;
            }
        }

        public static string CurrentLanguage => Instance.CurrentLanguage;

        public static string Get(string key) => Instance.Get(key);

        public static void SetLanguage(string code) => Instance.SetLanguage(code);

        public static string CycleLanguage() => Instance.CycleLanguage();

        public static LanguageDefinition CurrentLanguageDefinition => Settings != null ? Settings.FindByCode(CurrentLanguage) : null;

        public static void Use(Localizer localizer)
        {
            if (instance != null)
                instance.LanguageChanged -= RaiseLanguageChanged;

            instance = localizer;

            if (instance != null)
                instance.LanguageChanged += RaiseLanguageChanged;

            LanguageChanged?.Invoke();
        }

        private static void RaiseLanguageChanged(string _) => LanguageChanged?.Invoke();

        private static Localizer CreateDefault()
        {
            var s = Settings;
            if (s == null || s.StringsCsv == null)
                throw new InvalidOperationException(
                    $"LocalizationSettings não encontrado em Resources/{LocalizationSettings.ResourcesPath} ou sem CSV.");

            var table = LocalizationTable.FromCsv(s.StringsCsv.text);
            var preferred = s.FindBySystemLanguage(Application.systemLanguage);
            var fallback = s.FallbackLanguage;

            return new Localizer(table, new PlayerPrefsLanguageStorage(), preferred?.Code, fallback?.Code);
        }

        // Necessário com Enter Play Mode Options sem domain reload.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            settings = null;
            LanguageChanged = null;
        }
    }
}
