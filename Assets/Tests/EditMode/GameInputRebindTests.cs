using NUnit.Framework;
using ProjetoFinal.Input;
using UnityEngine.InputSystem;

namespace ProjetoFinal.Tests
{
    // Altera os overrides salvos em PlayerPrefs; o TearDown restaura os padrões.
    public class GameInputRebindTests : InputTestFixture
    {
        private Keyboard keyboard;
        private InputAction fire;
        private InputAction reload;

        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            GameInput.ResetToDefaults();
            fire = GameInput.Gameplay.FindAction("Fire", true);
            reload = GameInput.Gameplay.FindAction("Reload", true);
        }

        public override void TearDown()
        {
            GameInput.ResetToDefaults();
            base.TearDown();
        }

        private bool? RebindFireWith(UnityEngine.InputSystem.Controls.KeyControl key)
        {
            bool? changed = null;
            GameInput.StartRebind(fire, 0, c => changed = c);
            Assert.IsTrue(GameInput.IsRebinding);

            Press(key);
            currentTime += 1;
            InputSystem.Update();
            return changed;
        }

        [Test]
        public void Rebind_ToFreeKey_ChangesBinding()
        {
            Assert.AreEqual(true, RebindFireWith(keyboard.gKey));
            Assert.AreEqual("<Keyboard>/g", fire.bindings[0].effectivePath);
            Assert.IsFalse(GameInput.IsRebinding);
        }

        [Test]
        public void Rebind_ToKeyUsedByAnotherAction_SwapsKeys()
        {
            Assert.AreEqual(true, RebindFireWith(keyboard.rKey));
            Assert.AreEqual("<Keyboard>/r", fire.bindings[0].effectivePath);
            Assert.AreEqual("<Keyboard>/space", reload.bindings[0].effectivePath);
        }

        [Test]
        public void Escape_CancelsWithoutChanging()
        {
            Assert.AreEqual(false, RebindFireWith(keyboard.escapeKey));
            Assert.AreEqual("<Keyboard>/space", fire.bindings[0].effectivePath);
        }

        [Test]
        public void ResetToDefaults_RestoresOriginalKeys()
        {
            RebindFireWith(keyboard.gKey);

            GameInput.ResetToDefaults();

            Assert.AreEqual("<Keyboard>/space", fire.bindings[0].effectivePath);
        }
    }
}
