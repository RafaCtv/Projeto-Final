using UnityEngine;
using UnityEngine.UI;

namespace ProjetoFinal.UI.Components
{
    public enum BackgroundFit
    {
        Contain,
        Cover,
    }

    /// <summary>
    /// Imagem de fundo que preserva a proporção. Sem sprite atribuído, carrega de
    /// Resources/<see cref="resourcesPath"/>; sem nenhuma imagem, fica desabilitada.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Image), typeof(AspectRatioFitter))]
    public sealed class BackgroundArt : MonoBehaviour
    {
        [Tooltip("Caminho dentro de Assets/Resources, sem extensão.")]
        [SerializeField] private string resourcesPath = "Backgrounds/MainMenu";
        [Tooltip("Contain: mostra a imagem inteira. Cover: preenche a tela cortando as bordas.")]
        [SerializeField] private BackgroundFit fit = BackgroundFit.Contain;

        public void Apply()
        {
            var image = GetComponent<Image>();
            var fitter = GetComponent<AspectRatioFitter>();

            var sprite = image.sprite;
            if (sprite == null && !string.IsNullOrEmpty(resourcesPath))
                sprite = Resources.Load<Sprite>(resourcesPath);

            // overrideSprite não é serializado, então carregar de Resources não marca a cena como alterada.
            image.overrideSprite = sprite;
            image.enabled = sprite != null;
            image.color = Color.white;
            fitter.aspectMode = fit == BackgroundFit.Cover
                ? AspectRatioFitter.AspectMode.EnvelopeParent
                : AspectRatioFitter.AspectMode.FitInParent;
            if (sprite != null)
                fitter.aspectRatio = sprite.rect.width / sprite.rect.height;
        }

        private void OnEnable() => Apply();

#if UNITY_EDITOR
        private void OnValidate() => Apply();
#endif
    }
}
