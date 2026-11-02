using ProjetoFinal.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ProjetoFinal.UI.Components
{
    [RequireComponent(typeof(Button))]
    public sealed class LoadSceneButton : MonoBehaviour
    {
        [SerializeField] private string sceneName = SceneNames.Game;

        private void Awake() => GetComponent<Button>().onClick.AddListener(Load);

        public void Load() => SceneLoader.TryLoad(sceneName);
    }
}
