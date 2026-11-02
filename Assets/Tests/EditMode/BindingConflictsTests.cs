using NUnit.Framework;
using ProjetoFinal.Input;
using UnityEngine.InputSystem;

namespace ProjetoFinal.Tests
{
    public class BindingConflictsTests
    {
        private InputActionMap map;
        private InputAction fire;
        private InputAction reload;

        [SetUp]
        public void SetUp()
        {
            map = new InputActionMap("Gameplay");
            fire = map.AddAction("Fire", binding: "<Keyboard>/f");
            reload = map.AddAction("Reload", binding: "<Keyboard>/r");
        }

        [TearDown]
        public void TearDown() => map.Dispose();

        [Test]
        public void KeyAlreadyUsed_IsSwappedWithPreviousKey()
        {
            fire.ApplyBindingOverride(0, "<Keyboard>/r");

            var swapped = BindingConflicts.SwapDuplicates(fire, 0, "<Keyboard>/f");

            Assert.AreEqual(1, swapped);
            Assert.AreEqual("<Keyboard>/r", fire.bindings[0].effectivePath);
            Assert.AreEqual("<Keyboard>/f", reload.bindings[0].effectivePath);
        }

        [Test]
        public void FreeKey_ChangesNothingElse()
        {
            fire.ApplyBindingOverride(0, "<Keyboard>/g");

            var swapped = BindingConflicts.SwapDuplicates(fire, 0, "<Keyboard>/f");

            Assert.AreEqual(0, swapped);
            Assert.AreEqual("<Keyboard>/r", reload.bindings[0].effectivePath);
        }
    }
}
