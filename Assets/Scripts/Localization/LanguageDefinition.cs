using UnityEngine;

namespace ProjetoFinal.Localization
{
    /// <summary>Idioma suportado. <see cref="Code"/> deve corresponder a uma coluna do CSV.</summary>
    [CreateAssetMenu(fileName = "Language", menuName = "Projeto Final/Localization/Language")]
    public sealed class LanguageDefinition : ScriptableObject
    {
        [SerializeField] private string code = "en";
        [SerializeField] private string nativeName = "English";
        [SerializeField] private Sprite flag;
        [Tooltip("Idioma do sistema operacional que seleciona este idioma no primeiro acesso.")]
        [SerializeField] private SystemLanguage systemLanguage = SystemLanguage.English;

        public string Code => code;
        public string NativeName => nativeName;
        public Sprite Flag => flag;
        public SystemLanguage SystemLanguage => systemLanguage;
    }
}
