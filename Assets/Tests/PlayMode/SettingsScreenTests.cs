using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjetoFinal.Audio;
using ProjetoFinal.Core;
using ProjetoFinal.UI.Navigation;
using ProjetoFinal.UI.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjetoFinal.Tests
{
    public class SettingsScreenTests
    {
        private float originalVolume;

        [UnitySetUp]
        public IEnumerator LoadMenu()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.MainMenu);
            yield return null; // ScreenNavigator.Start
            originalVolume = AudioSettingsService.Master.Value;
        }

        [TearDown]
        public void Restore() => AudioSettingsService.Master.Set(originalVolume);

        private static UIScreen Screen(string name) =>
            Resources.FindObjectsOfTypeAll<UIScreen>().Single(s => s.name == name && s.gameObject.scene.IsValid());

        private static void Click(string buttonName) =>
            Object.FindObjectsByType<Button>().Single(b => b.name == buttonName).onClick.Invoke();

        [UnityTest]
        public IEnumerator SettingsButton_OpensSettings_ControlsOpensControls_BackReturns()
        {
            Assert.IsTrue(Screen("MainScreen").IsVisible);
            Assert.IsFalse(Screen("SettingsScreen").IsVisible);

            Click("SettingsButton");
            yield return null;
            Assert.IsFalse(Screen("MainScreen").IsVisible);
            Assert.IsTrue(Screen("SettingsScreen").IsVisible);

            Click("ControlsButton");
            yield return null;
            Assert.IsTrue(Screen("ControlsScreen").IsVisible);
            Assert.AreEqual(8, Object.FindObjectsByType<KeyBindingRow>().Length, "3 movimentos + atirar, recarregar, corpo a corpo, curar, interagir.");

            Click("BackButton");
            yield return null;
            Assert.IsTrue(Screen("SettingsScreen").IsVisible);

            Click("BackButton");
            yield return null;
            Assert.IsTrue(Screen("MainScreen").IsVisible);
        }

        [UnityTest]
        public IEnumerator VolumeSlider_ChangesMasterVolume()
        {
            Click("SettingsButton");
            yield return null;

            var slider = Object.FindAnyObjectByType<VolumeSlider>().GetComponent<Slider>();
            slider.value = 0.35f;

            Assert.AreEqual(0.35f, AudioSettingsService.Master.Value, 0.001f);
            Assert.AreEqual(0.35f, AudioListener.volume, 0.001f);
        }
    }
}
