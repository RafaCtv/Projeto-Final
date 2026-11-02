using UnityEngine;
using UnityEngine.UI;

namespace ProjetoFinal.UI.Components
{
    [RequireComponent(typeof(Button))]
    public sealed class QuitGameButton : MonoBehaviour
    {
        private void Awake() => GetComponent<Button>().onClick.AddListener(Quit);

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
