using System;
using ProjetoFinal.Input;
using UnityEngine;

namespace ProjetoFinal.UI.Settings
{
    /// <summary>Instancia uma <see cref="KeyBindingRow"/> por binding do mapa Gameplay.</summary>
    public sealed class ControlsList : MonoBehaviour
    {
        [SerializeField] private KeyBindingRow rowPrefab;
        [SerializeField] private Transform container;
        [Tooltip("Só mostra bindings deste control scheme.")]
        [SerializeField] private string bindingGroup = "Keyboard";

        private bool built;

        private void OnEnable()
        {
            if (!built)
                Build();
        }

        private void OnDisable() => GameInput.CancelRebind();

        public void Build()
        {
            var parent = container != null ? container : transform;
            for (var i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);

            foreach (var action in GameInput.Gameplay.actions)
            {
                for (var i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.isComposite || !InGroup(binding.groups))
                        continue;

                    Instantiate(rowPrefab, parent).Bind(action, i);
                }
            }
            built = true;
        }

        public void ResetToDefaults() => GameInput.ResetToDefaults();

        private bool InGroup(string groups) =>
            string.IsNullOrEmpty(bindingGroup)
            || (groups != null && Array.IndexOf(groups.Split(';'), bindingGroup) >= 0);
    }
}
