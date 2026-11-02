using ProjetoFinal.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoFinal.UI.Components
{
    /// <summary>Exibe a bandeira do idioma atual e, ao clicar, avança para o próximo idioma.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class LanguageToggleButton : MonoBehaviour
    {
        [SerializeField] private Image flagImage;

        private void Awake() => GetComponent<Button>().onClick.AddListener(Cycle);

        private void OnEnable()
        {
            LocalizationService.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable() => LocalizationService.LanguageChanged -= Refresh;

        public void Cycle() => LocalizationService.CycleLanguage();

        private void Refresh()
        {
            var language = LocalizationService.CurrentLanguageDefinition;
            if (flagImage != null && language != null && language.Flag != null)
                flagImage.sprite = language.Flag;
        }
    }
}
