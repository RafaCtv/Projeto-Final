using ProjetoFinal.Input;
using ProjetoFinal.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjetoFinal.UI.Settings
{
    /// <summary>Linha da tela de controles: nome da ação e botão que inicia o remapeamento.</summary>
    public sealed class KeyBindingRow : MonoBehaviour
    {
        [SerializeField] private LocalizedText label;
        [SerializeField] private Button keyButton;
        [SerializeField] private TMP_Text keyText;

        private InputAction action;
        private int bindingIndex;
        private bool waiting;

        public InputAction Action => action;
        public int BindingIndex => bindingIndex;

        /// <summary>"action.&lt;Ação&gt;" ou, para partes de composite, "action.&lt;Ação&gt;.&lt;parte&gt;".</summary>
        public static string LabelKey(InputAction action, int bindingIndex)
        {
            var binding = action.bindings[bindingIndex];
            return binding.isPartOfComposite ? $"action.{action.name}.{binding.name}" : $"action.{action.name}";
        }

        public void Bind(InputAction inputAction, int index)
        {
            action = inputAction;
            bindingIndex = index;
            gameObject.name = $"Row_{LabelKey(action, bindingIndex)}";
            label.SetKey(LabelKey(action, bindingIndex));
            Refresh();
        }

        private void Awake() => keyButton.onClick.AddListener(StartRebind);

        private void OnEnable()
        {
            GameInput.BindingsChanged += Refresh;
            LocalizationService.LanguageChanged += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            GameInput.BindingsChanged -= Refresh;
            LocalizationService.LanguageChanged -= Refresh;
            waiting = false;
        }

        private void StartRebind()
        {
            if (action == null)
                return;

            waiting = true;
            Refresh();
            GameInput.StartRebind(action, bindingIndex, _ =>
            {
                waiting = false;
                Refresh();
            });
        }

        private void Refresh()
        {
            if (action == null)
                return;

            keyText.text = waiting
                ? LocalizationService.Get("controls.waitingKey")
                : GameInput.GetBindingDisplay(action, bindingIndex);
        }
    }
}
