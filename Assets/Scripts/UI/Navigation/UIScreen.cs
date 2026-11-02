using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjetoFinal.UI.Navigation
{
    /// <summary>Tela controlada por um <see cref="ScreenNavigator"/>.</summary>
    [DisallowMultipleComponent]
    public sealed class UIScreen : MonoBehaviour
    {
        private const float StartScale = 0.85f;

        [Tooltip("Identificador usado pelo NavigationButton.")]
        [SerializeField] private string id;
        [Tooltip("Selecionado ao abrir a tela (navegação por teclado/gamepad).")]
        [SerializeField] private GameObject firstSelected;
        [Tooltip("Alvo da animação de abertura. Vazio desativa a animação.")]
        [SerializeField] private RectTransform popTarget;
        [SerializeField] private float popDuration = 0.18f;

        public string Id => id;
        public bool IsVisible => gameObject.activeSelf;

        public void Show()
        {
            gameObject.SetActive(true);

            if (firstSelected != null && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstSelected);

            if (popTarget != null && popDuration > 0f)
                StartCoroutine(Pop());
        }

        public void Hide() => gameObject.SetActive(false);

        private IEnumerator Pop()
        {
            for (var t = 0f; t < popDuration; t += Time.unscaledDeltaTime)
            {
                // easeOutBack
                var x = t / popDuration - 1f;
                var ease = 1f + 2.7f * x * x * x + 1.7f * x * x;
                popTarget.localScale = Vector3.one * Mathf.LerpUnclamped(StartScale, 1f, ease);
                yield return null;
            }
            popTarget.localScale = Vector3.one;
        }

        private void OnDisable()
        {
            if (popTarget != null)
                popTarget.localScale = Vector3.one;
        }
    }
}
