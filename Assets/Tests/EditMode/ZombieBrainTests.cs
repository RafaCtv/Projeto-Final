using NUnit.Framework;
using UnityEngine;

namespace ProjetoFinal.Tests
{
    public class ZombieBrainTests
    {
        // Base ocupa x ∈ [-3, 3]; jogador detectado a 7, perdido a 10, até 3 de altura.
        private const float BaseMin = -3, BaseMax = 3;
        private static ZombieBrain NewBrain() => new ZombieBrain(7, 10, 3);

        [Test]
        public void WithoutPlayerNearby_WalksTowardsBase_FromBothSides()
        {
            var brain = NewBrain();
            Assert.AreEqual(-1, brain.Decide(new Vector2(30, 0), false, Vector2.zero, BaseMin, BaseMax));
            Assert.AreEqual(ZombieGoal.Base, brain.Goal);
            Assert.AreEqual(1, NewBrain().Decide(new Vector2(-30, 0), false, Vector2.zero, BaseMin, BaseMax));
        }

        [Test]
        public void StopsWhenReachingTheBase()
        {
            Assert.AreEqual(0, NewBrain().Decide(new Vector2(3, 0), false, Vector2.zero, BaseMin, BaseMax));
        }

        [Test]
        public void ChasesNearbyPlayer_EvenAwayFromTheBase()
        {
            var brain = NewBrain();
            // Zumbi à direita da base, jogador mais à direita ainda: o zumbi vira as costas para a base.
            Assert.AreEqual(1, brain.Decide(new Vector2(20, 0), true, new Vector2(25, 0), BaseMin, BaseMax));
            Assert.AreEqual(ZombieGoal.Player, brain.Goal);
            Assert.AreEqual(1, brain.Facing);
        }

        [Test]
        public void IgnoresPlayerOutOfRange_OrAtDifferentHeight()
        {
            var brain = NewBrain();
            Assert.AreEqual(-1, brain.Decide(new Vector2(20, 0), true, new Vector2(28, 0), BaseMin, BaseMax));
            Assert.AreEqual(ZombieGoal.Base, brain.Goal);
            brain.Decide(new Vector2(20, 0), true, new Vector2(22, 5), BaseMin, BaseMax);
            Assert.AreEqual(ZombieGoal.Base, brain.Goal);
        }

        [Test]
        public void KeepsChasingUntilLoseRange_ThenReturnsToBase()
        {
            var brain = NewBrain();
            brain.Decide(new Vector2(20, 0), true, new Vector2(26, 0), BaseMin, BaseMax);
            Assert.AreEqual(ZombieGoal.Player, brain.Goal);
            // 9 de distância: fora da detecção, mas dentro do raio para perder o jogador.
            brain.Decide(new Vector2(20, 0), true, new Vector2(29, 0), BaseMin, BaseMax);
            Assert.AreEqual(ZombieGoal.Player, brain.Goal);
            Assert.AreEqual(-1, brain.Decide(new Vector2(20, 0), true, new Vector2(31, 0), BaseMin, BaseMax));
            Assert.AreEqual(ZombieGoal.Base, brain.Goal);
        }

        [Test]
        public void DeadPlayerIsIgnored()
        {
            var brain = NewBrain();
            brain.Decide(new Vector2(20, 0), true, new Vector2(22, 0), BaseMin, BaseMax);
            brain.Decide(new Vector2(20, 0), false, new Vector2(22, 0), BaseMin, BaseMax);
            Assert.AreEqual(ZombieGoal.Base, brain.Goal);
        }
    }
}
