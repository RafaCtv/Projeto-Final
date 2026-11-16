using ProjetoFinal.Core;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjetoFinal.Editor
{
    internal static class PlayFromMainMenu
    {
        [InitializeOnLoadMethod]
        private static void Configure()
        {
            // Scene assets are available after the editor finishes importing them.
            EditorApplication.delayCall += SetStartScene;
        }

        private static void SetStartScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            var menu = AssetDatabase.LoadAssetAtPath<SceneAsset>($"Assets/Scenes/{SceneNames.MainMenu}.unity");
            if (menu != null) EditorSceneManager.playModeStartScene = menu;
        }
    }
}
