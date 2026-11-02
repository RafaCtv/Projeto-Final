using TMPro;
using UnityEngine;

namespace ProjetoFinal.Localization
{
    /// <summary>Mantém o texto TMP sincronizado com a tradução da chave no idioma atual.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;

        private TMP_Text text;

        public string Key => key;

        public void SetKey(string newKey)
        {
            key = newKey;
            Refresh();
        }

        public void Refresh()
        {
            if (text == null)
                text = GetComponent<TMP_Text>();
            if (!string.IsNullOrEmpty(key))
                text.text = LocalizationService.Get(key);
        }

        private void OnEnable()
        {
            LocalizationService.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable() => LocalizationService.LanguageChanged -= Refresh;
    }
}
