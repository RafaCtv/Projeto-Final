using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjetoFinal.Core
{
    public static class SceneLoader
    {
        public static bool CanLoad(string sceneName) => Application.CanStreamedLevelBeLoaded(sceneName);

        public static bool TryLoad(string sceneName)
        {
            if (!CanLoad(sceneName))
            {
                Debug.LogWarning($"[SceneLoader] Cena '{sceneName}' não está na Scene List do build.");
                return false;
            }

            SceneManager.LoadScene(sceneName);
            return true;
        }
    }
}
