using System;
using ProjetoFinal.Localization;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjetoFinal.Input
{
    /// <summary>
    /// Controles do jogo (Resources/Input/GameControls) e remapeamento persistido em PlayerPrefs.
    /// Cada binding do mapa Gameplay precisa da chave "action.&lt;Ação&gt;" no CSV.
    /// </summary>
    public static class GameInput
    {
        public const string ResourcesPath = "Input/GameControls";
        public const string GameplayMapName = "Gameplay";
        private const string OverridesKey = "settings.bindings";

        private static InputActionAsset asset;
        private static InputActionRebindingExtensions.RebindingOperation currentRebind;

        public static event Action BindingsChanged;

        public static InputActionAsset Asset
        {
            get
            {
                if (asset == null)
                    asset = Load();
                return asset;
            }
        }

        public static InputActionMap Gameplay => Asset.FindActionMap(GameplayMapName, true);

        public static bool IsRebinding => currentRebind != null;

        /// <summary>Permite ignorar o Esc que cancelou o remapeamento no mesmo frame.</summary>
        public static int LastRebindEndFrame { get; private set; } = -1;

        private static InputActionAsset Load()
        {
            var loaded = Resources.Load<InputActionAsset>(ResourcesPath);
            if (loaded == null)
                throw new InvalidOperationException($"InputActionAsset não encontrado em Resources/{ResourcesPath}.");

            var json = PlayerPrefs.GetString(OverridesKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                loaded.RemoveAllBindingOverrides();
            else
                loaded.LoadBindingOverridesFromJson(json);

            return loaded;
        }

        /// <summary>Nome da tecla: usa "key.&lt;controle&gt;" do CSV se existir, senão o nome do Input System.</summary>
        public static string GetBindingDisplay(InputAction action, int bindingIndex)
        {
            var path = action.bindings[bindingIndex].effectivePath;
            if (string.IsNullOrEmpty(path))
                return "-";

            var control = path.Substring(path.LastIndexOf('/') + 1);
            if (LocalizationService.Instance.TryGet($"key.{control}", out var localized))
                return localized;

            return InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice).ToUpperInvariant();
        }

        /// <summary>Aguarda uma tecla do teclado; Esc cancela. <paramref name="onFinished"/> recebe true se o binding mudou.</summary>
        public static void StartRebind(InputAction action, int bindingIndex, Action<bool> onFinished)
        {
            CancelRebind();

            var previousPath = action.bindings[bindingIndex].effectivePath;
            var wasEnabled = action.enabled;
            action.Disable();

            currentRebind = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsHavingToMatchPath("<Keyboard>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.05f)
                .OnComplete(op =>
                {
                    BindingConflicts.SwapDuplicates(action, bindingIndex, previousPath);
                    Finish(action, wasEnabled);
                    Save();
                    onFinished?.Invoke(true);
                })
                .OnCancel(op =>
                {
                    Finish(action, wasEnabled);
                    onFinished?.Invoke(false);
                })
                .Start();
        }

        public static void CancelRebind() => currentRebind?.Cancel();

        public static void ResetToDefaults()
        {
            CancelRebind();
            Asset.RemoveAllBindingOverrides();
            Save();
        }

        private static void Finish(InputAction action, bool wasEnabled)
        {
            currentRebind?.Dispose();
            currentRebind = null;
            LastRebindEndFrame = Time.frameCount;
            if (wasEnabled)
                action.Enable();
        }

        private static void Save()
        {
            PlayerPrefs.SetString(OverridesKey, Asset.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
            BindingsChanged?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            currentRebind?.Dispose();
            currentRebind = null;
            asset = null;
            BindingsChanged = null;
            LastRebindEndFrame = -1;
        }
    }
}
