using NUnit.Framework;

namespace ProjetoFinal.Tests
{
    public class WeaponAmmoTests
    {
        [Test]
        public void StartsWithFullMagazine_AndConsumesOneBulletPerShot()
        {
            var ammo = new WeaponAmmo(12, 36, 120);
            Assert.AreEqual(12, ammo.Magazine);
            Assert.AreEqual(36, ammo.Reserve);
            Assert.IsTrue(ammo.TryConsume());
            Assert.AreEqual(11, ammo.Magazine);
        }

        [Test]
        public void EmptyMagazine_CannotShoot()
        {
            var ammo = new WeaponAmmo(2, 0, 10);
            ammo.TryConsume(); ammo.TryConsume();
            Assert.IsFalse(ammo.TryConsume());
            Assert.IsTrue(ammo.IsOutOfAmmo);
            Assert.IsFalse(ammo.CanReload);
        }

        [Test]
        public void Reload_MovesOnlyWhatFitsAndWhatIsAvailable()
        {
            var ammo = new WeaponAmmo(12, 5, 120);
            for (int i = 0; i < 10; i++) ammo.TryConsume();
            Assert.AreEqual(5, ammo.Reload());
            Assert.AreEqual(7, ammo.Magazine);
            Assert.AreEqual(0, ammo.Reserve);
        }

        [Test]
        public void FullMagazine_DoesNotReload()
        {
            Assert.IsFalse(new WeaponAmmo(12, 30, 120).CanReload);
        }

        [Test]
        public void AddReserve_IsCappedAtMaximum()
        {
            var ammo = new WeaponAmmo(12, 115, 120);
            Assert.AreEqual(5, ammo.AddReserve(10));
            Assert.IsTrue(ammo.ReserveIsFull);
            Assert.AreEqual(0, ammo.AddReserve(3));
        }
    }

    public class ZombieDropTests
    {
        [Test]
        public void ConfiguredZombies_DropAmmoOnlyBelowChance_WithinRange()
        {
#if UNITY_EDITOR
            var data = UnityEditor.AssetDatabase.LoadAssetAtPath<ZombieData>("Assets/Data/Enemies/BasicZombie.asset");
            Assert.IsNotNull(data.AmmoPickup, "O zumbi básico deve ter um item de munição configurado.");
            Assert.AreEqual(0, data.RollAmmoDrop(0.99f, 0.5f));
            int min = data.RollAmmoDrop(0f, 0f), max = data.RollAmmoDrop(0f, 0.9999f);
            Assert.Greater(min, 0);
            Assert.GreaterOrEqual(max, min);
#endif
        }
    }

    public class StaminaTests
    {
        private static Stamina NewStamina() => new Stamina(100, 50, 20, 0.5f, 25);

        [Test]
        public void Sprinting_DrainsStamina_OnlyWhileMoving()
        {
            var stamina = NewStamina();
            Assert.IsFalse(stamina.Tick(1, true, false), "Parado não corre.");
            Assert.AreEqual(100, stamina.Current, 1e-4f);
            Assert.IsTrue(stamina.Tick(1, true, true));
            Assert.AreEqual(50, stamina.Current, 1e-4f);
        }

        [Test]
        public void Regenerates_AfterDelay()
        {
            var stamina = NewStamina();
            stamina.Tick(1, true, true);
            stamina.Tick(0.5f, false, true);
            Assert.AreEqual(50, stamina.Current, 1e-4f, "Durante a pausa não recupera.");
            stamina.Tick(1, false, true);
            Assert.AreEqual(70, stamina.Current, 1e-4f);
        }

        [Test]
        public void Exhaustion_BlocksSprintUntilMinimumIsRecovered()
        {
            var stamina = NewStamina();
            stamina.Tick(2, true, true);
            Assert.IsTrue(stamina.IsExhausted);
            Assert.IsFalse(stamina.Tick(0.5f, true, true));
            stamina.Tick(1, false, false);
            Assert.IsFalse(stamina.Tick(0.01f, true, true), "Com 20 de estamina ainda está exausto.");
            stamina.Tick(0.5f, false, false);
            Assert.IsTrue(stamina.Tick(0.01f, true, true));
        }
    }
}
