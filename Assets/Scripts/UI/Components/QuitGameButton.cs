using UnityEngine;
using UnityEngine.UI;

namespace ProjetoFinal.UI.Components
{
    [RequireComponent(typeof(Button))]
    public sealed class QuitGameButton : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            gameObject.SetActive(false);
#else
            GetComponent<Button>().onClick.AddListener(Quit);
#endif
        }

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
