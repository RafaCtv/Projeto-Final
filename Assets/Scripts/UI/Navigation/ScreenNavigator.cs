using System.Collections.Generic;
using ProjetoFinal.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjetoFinal.UI.Navigation
{
    /// <summary>Pilha das <see cref="UIScreen"/> filhas; Esc volta para a anterior.</summary>
    public sealed class ScreenNavigator : MonoBehaviour
    {
        [SerializeField] private UIScreen initialScreen;

        private readonly Stack<UIScreen> stack = new();
        private readonly Dictionary<string, UIScreen> screensById = new();

        public UIScreen Current => stack.Count > 0 ? stack.Peek() : null;

        private void Awake()
        {
            foreach (var screen in GetComponentsInChildren<UIScreen>(true))
            {
                if (!string.IsNullOrEmpty(screen.Id))
                    screensById[screen.Id] = screen;
                if (screen != initialScreen)
                    screen.Hide();
            }
        }

        private void Start()
        {
            if (initialScreen != null)
                Open(initialScreen);
        }

        public void Open(string screenId)
        {
            if (screensById.TryGetValue(screenId ?? string.Empty, out var screen))
                Open(screen);
            else
                Debug.LogWarning($"[ScreenNavigator] Nenhuma UIScreen com Id '{screenId}'.", this);
        }

        public void Open(UIScreen screen)
        {
            if (screen == null || screen == Current)
                return;

            if (Current != null)
                Current.Hide();

            stack.Push(screen);
            screen.Show();
        }

        public void Back()
        {
            if (stack.Count <= 1)
                return;

            stack.Pop().Hide();
            Current.Show();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
                return;

            // O Esc que cancela um remapeamento não deve também fechar a tela.
            if (GameInput.IsRebinding || GameInput.LastRebindEndFrame == Time.frameCount)
                return;

            Back();
        }
    }
}
