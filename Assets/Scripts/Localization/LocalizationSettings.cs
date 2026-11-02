using System.Collections.Generic;
using UnityEngine;

namespace ProjetoFinal.Localization
{
    /// <summary>Carregado de Resources por <see cref="LocalizationService"/>.</summary>
    [CreateAssetMenu(fileName = "LocalizationSettings", menuName = "Projeto Final/Localization/Settings")]
    public sealed class LocalizationSettings : ScriptableObject
    {
        public const string ResourcesPath = "Localization/LocalizationSettings";

        [SerializeField] private TextAsset stringsCsv;
        [SerializeField] private List<LanguageDefinition> languages = new();
        [Tooltip("Idioma usado quando o idioma do sistema não é suportado ou falta uma tradução.")]
        [SerializeField] private LanguageDefinition fallbackLanguage;

        public TextAsset StringsCsv => stringsCsv;
        public IReadOnlyList<LanguageDefinition> Languages => languages;
        public LanguageDefinition FallbackLanguage => fallbackLanguage;

        public LanguageDefinition FindByCode(string code) => languages.Find(l => l != null && l.Code == code);

        public LanguageDefinition FindBySystemLanguage(SystemLanguage systemLanguage) =>
            languages.Find(l => l != null && l.SystemLanguage == systemLanguage);
    }
}
