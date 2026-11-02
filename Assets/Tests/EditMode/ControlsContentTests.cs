using System.Linq;
using NUnit.Framework;
using ProjetoFinal.Input;
using ProjetoFinal.Localization;
using ProjetoFinal.UI.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjetoFinal.Tests
{
    public class ControlsContentTests
    {
        private InputActionMap gameplay;
        private LocalizationTable table;

        [SetUp]
        public void SetUp()
        {
            var asset = Resources.Load<InputActionAsset>(GameInput.ResourcesPath);
            Assert.IsNotNull(asset, "GameControls não encontrado em Resources.");
            gameplay = asset.FindActionMap(GameInput.GameplayMapName, true);

            var settings = Resources.Load<LocalizationSettings>(LocalizationSettings.ResourcesPath);
            table = LocalizationTable.FromCsv(settings.StringsCsv.text);
        }

        private string DefaultPath(string action, string part = null) =>
            gameplay.FindAction(action, true).bindings
                .First(b => !b.isComposite && (part == null || b.name == part)).path;

        [Test]
        public void DefaultKeys_MatchGameDesign()
        {
            Assert.AreEqual("<Keyboard>/a", DefaultPath("Move", "left"));
            Assert.AreEqual("<Keyboard>/d", DefaultPath("Move", "right"));
            Assert.AreEqual("<Keyboard>/space", DefaultPath("Fire"));
            Assert.AreEqual("<Keyboard>/r", DefaultPath("Reload"));
            Assert.AreEqual("<Keyboard>/f", DefaultPath("Melee"));
            Assert.AreEqual("<Keyboard>/q", DefaultPath("Heal"));
            Assert.AreEqual("<Keyboard>/e", DefaultPath("Interact"));
        }

        [Test]
        public void AlternateBindings_StayOutOfControlsScreen()
        {
            var alternates = gameplay.bindings.Where(b => !b.isComposite && b.groups != "Keyboard").Select(b => b.path).ToList();
            CollectionAssert.IsSubsetOf(new[] { "<Keyboard>/enter",
                "<Mouse>/leftButton", "<Gamepad>/leftStick", "<Gamepad>/buttonWest", "<Gamepad>/buttonNorth" }, alternates);
            Assert.AreEqual(7, gameplay.bindings.Count(b => !b.isComposite && b.groups == "Keyboard"),
                "A tela de Controles lista só o grupo Keyboard.");
        }

        [Test]
        public void DefaultKeys_AreNotDuplicated()
        {
            var paths = gameplay.bindings.Where(b => !b.isComposite).Select(b => b.path).ToList();
            CollectionAssert.AllItemsAreUnique(paths);
        }

        [Test]
        public void EveryBinding_HasLabelInEveryLanguage()
        {
            foreach (var action in gameplay.actions)
            for (var i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].isComposite)
                    continue;

                var key = KeyBindingRow.LabelKey(action, i);
                foreach (var language in table.Languages)
                    Assert.IsTrue(table.TryGet(language, key, out _), $"Falta '{key}' em '{language}' no CSV.");
            }
        }
    }
}
