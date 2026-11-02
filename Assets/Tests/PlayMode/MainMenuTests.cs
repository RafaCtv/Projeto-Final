using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjetoFinal.Core;
using ProjetoFinal.Localization;
using ProjetoFinal.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ProjetoFinal.Tests
{
    public class MainMenuTests
    {
        private string originalLanguage;

        [UnitySetUp]
        public IEnumerator LoadMenu()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.MainMenu);
            originalLanguage = LocalizationService.CurrentLanguage;
        }

        [TearDown]
        public void RestoreLanguage() => LocalizationService.SetLanguage(originalLanguage);

        [Test]
        public void Menu_HasFourLocalizedButtonsWithActions()
        {
            Assert.IsNotNull(Object.FindAnyObjectByType<LoadSceneButton>());
            Assert.IsNotNull(Object.FindAnyObjectByType<QuitGameButton>());

            var keys = Object.FindObjectsByType<Button>()
                .Select(b => b.GetComponentInChildren<LocalizedText>())
                .Where(t => t != null)
                .Select(t => t.Key)
                .ToArray();

            CollectionAssert.IsSubsetOf(new[] { "menu.play", "menu.settings", "menu.market", "menu.quit" }, keys);
        }

        [UnityTest]
        public IEnumerator Menu_PlaysThemeMusic_AndAnimatesTitle()
        {
            yield return null;
            var music = Object.FindAnyObjectByType<ProjetoFinal.Audio.MusicPlayer>();
            Assert.IsNotNull(music);
            Assert.AreEqual("MenuTheme", music.Current.name);

            var title = Object.FindObjectsByType<LocalizedText>().Single(t => t.Key == "game.title");
            var animator = title.GetComponent<Animator>();
            Assert.IsNotNull(animator);
            Assert.IsTrue(animator.isActiveAndEnabled);
        }

        [Test]
        public void Menu_HasExactlyOneLanguageButton()
        {
            Assert.AreEqual(1, Object.FindObjectsByType<LanguageToggleButton>().Length);
        }

        [UnityTest]
        public IEnumerator ClickingFlag_CyclesLanguage_TranslatesTextAndSwapsFlag()
        {
            var toggle = Object.FindAnyObjectByType<LanguageToggleButton>();
            var flag = toggle.transform.Find("Flag").GetComponent<Image>();
            var play = Object.FindObjectsByType<LocalizedText>()
                .Single(t => t.Key == "menu.play")
                .GetComponent<TMP_Text>();

            LocalizationService.SetLanguage("pt-BR");
            yield return null;
            Assert.AreEqual("JOGAR", play.text);
            Assert.AreEqual(LocalizationService.Settings.FindByCode("pt-BR").Flag, flag.sprite);

            toggle.GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.AreEqual("en", LocalizationService.CurrentLanguage);
            Assert.AreEqual("PLAY", play.text);
            Assert.AreEqual(LocalizationService.Settings.FindByCode("en").Flag, flag.sprite);

            toggle.GetComponent<Button>().onClick.Invoke();
            yield return null;
            Assert.AreEqual("pt-BR", LocalizationService.CurrentLanguage);
            Assert.AreEqual("JOGAR", play.text);
        }
    }
}
