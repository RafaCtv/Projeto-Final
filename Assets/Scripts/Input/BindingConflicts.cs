using UnityEngine.InputSystem;

namespace ProjetoFinal.Input
{
    /// <summary>Resolve teclas duplicadas no mapa trocando-as com a tecla anterior do binding alterado.</summary>
    public static class BindingConflicts
    {
        /// <returns>Quantidade de bindings trocados.</returns>
        public static int SwapDuplicates(InputAction changedAction, int changedBindingIndex, string previousPath)
        {
            var map = changedAction.actionMap;
            var newPath = changedAction.bindings[changedBindingIndex].effectivePath;
            if (map == null || string.IsNullOrEmpty(newPath) || string.IsNullOrEmpty(previousPath) || newPath == previousPath)
                return 0;

            var swapped = 0;
            foreach (var action in map.actions)
            {
                for (var i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.isComposite || (action == changedAction && i == changedBindingIndex))
                        continue;

                    if (binding.effectivePath == newPath)
                    {
                        action.ApplyBindingOverride(i, previousPath);
                        swapped++;
                    }
                }
            }
            return swapped;
        }
    }
}
