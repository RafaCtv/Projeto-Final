using UnityEngine;
using UnityEngine.UI;

namespace ProjetoFinal.UI.Navigation
{
    public enum NavigationAction
    {
        Open,
        Back,
    }

    /// <summary>Abre uma <see cref="UIScreen"/> pelo Id ou volta, no <see cref="ScreenNavigator"/> pai.</summary>
    [RequireComponent(typeof(Button))]
    public sealed class NavigationButton : MonoBehaviour
    {
        [SerializeField] private NavigationAction action = NavigationAction.Open;
        [Tooltip("ID da tela a abrir (campo Id da UIScreen). Ignorado em Back.")]
        [SerializeField] private string targetScreenId;

        private void Awake() => GetComponent<Button>().onClick.AddListener(Navigate);

        public void Navigate()
        {
            var navigator = GetComponentInParent<ScreenNavigator>(true);
            if (navigator == null)
            {
                Debug.LogWarning($"[NavigationButton] '{name}' não está dentro de um ScreenNavigator.", this);
                return;
            }

            if (action == NavigationAction.Back)
                navigator.Back();
            else
                navigator.Open(targetScreenId);
        }
    }
}
