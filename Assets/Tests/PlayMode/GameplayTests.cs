using System.Collections;
using System.Linq;
using NUnit.Framework;
using ProjetoFinal.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjetoFinal.Tests
{
    public class GameplayTests
    {
        private GameManager game;
        private Health player;

        [UnitySetUp]
        public IEnumerator LoadGame()
        {
            yield return SceneManager.LoadSceneAsync(SceneNames.Game);
            game = Object.FindAnyObjectByType<GameManager>();
            player = Object.FindAnyObjectByType<PlayerController>().GetComponent<Health>();
        }

        [TearDown]
        public void RestoreTime() => Time.timeScale = 1;

        private static T Load<T>(string path) where T : Object
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<T>(path);
#else
            Assert.Ignore("Precisa do editor para carregar prefabs.");
            return null;
#endif
        }

        private ZombieController SpawnZombie(float x)
        {
            var prefab = Load<GameObject>("Assets/Prefabs/Combat/Zombie.prefab");
            var zombie = Object.Instantiate(prefab, new Vector3(x, -3.8f, 0), Quaternion.identity).GetComponent<ZombieController>();
            zombie.Initialize(game);
            return zombie;
        }

        private static IEnumerator WaitSeconds(float seconds)
        {
            var end = Time.time + seconds;
            while (Time.time < end) yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator Bullet_HitsZombieThroughTrigger()
        {
            var zombie = SpawnZombie(40f);
            yield return new WaitForFixedUpdate();
            int before = zombie.Health.Current;

            var bullet = Object.Instantiate(Load<GameObject>("Assets/Prefabs/Combat/Bullet.prefab"), new Vector3(36f, -2.7f, 0), Quaternion.identity)
                .GetComponent<Projectile>();
            bullet.Initialize(Vector2.right, 20, game);
            yield return WaitSeconds(0.5f);

            Assert.AreEqual(before - 20, zombie.Health.Current);
            Assert.IsTrue(bullet == null, "A bala deve ser destruída ao acertar.");
        }

        [UnityTest]
        public IEnumerator Zombie_DamagesPlayerInAttackRange()
        {
            var body = player.GetComponent<Rigidbody2D>();
            body.position = new Vector2(20f, body.position.y);
            yield return new WaitForFixedUpdate();
            SpawnZombie(20.8f);

            yield return WaitSeconds(1.5f);

            Assert.Less(player.Current, player.Maximum, "O zumbi deveria atacar o jogador à sua frente.");
        }

        [UnityTest]
        public IEnumerator Zombie_AnimatesWalking_AndStopsToAttack()
        {
            var body = player.GetComponent<Rigidbody2D>();
            body.position = new Vector2(20f, body.position.y);
            var walker = SpawnZombie(45f);
            var attacker = SpawnZombie(20.8f);
            yield return WaitSeconds(0.6f);

            Assert.IsTrue(walker.GetComponentInChildren<Animator>().GetBool("IsWalking"));
            Assert.IsFalse(attacker.GetComponentInChildren<Animator>().GetBool("IsWalking"));
        }

        [UnityTest]
        public IEnumerator Zombies_WalkTowardsBase_FromBothSides()
        {
            var body = player.GetComponent<Rigidbody2D>();
            float baseX = game.Base.transform.position.x;
            body.position = new Vector2(baseX + 40f, body.position.y);
            var fromLeft = SpawnZombie(baseX - 20f);
            var fromRight = SpawnZombie(baseX + 15f);
            yield return WaitSeconds(0.5f);

            Assert.Greater(fromLeft.GetComponent<Rigidbody2D>().linearVelocityX, 0, "Zumbi à esquerda deve andar para a direita.");
            Assert.Less(fromRight.GetComponent<Rigidbody2D>().linearVelocityX, 0, "Zumbi à direita deve andar para a esquerda.");
            Assert.AreEqual(1, fromLeft.Facing);
            Assert.AreEqual(ZombieGoal.Base, fromLeft.Goal);
        }

        [UnityTest]
        public IEnumerator Zombie_ChasesNearbyPlayer_AwayFromTheBase()
        {
            var body = player.GetComponent<Rigidbody2D>();
            float baseX = game.Base.transform.position.x;
            body.position = new Vector2(baseX + 25f, body.position.y);
            var chaser = SpawnZombie(baseX + 21f);
            yield return WaitSeconds(0.5f);

            Assert.AreEqual(ZombieGoal.Player, chaser.Goal);
            Assert.Greater(chaser.GetComponent<Rigidbody2D>().linearVelocityX, 0, "Deve virar as costas para a base e ir atrás do jogador.");
        }

        [UnityTest]
        public IEnumerator Zombie_FacingRight_DamagesPlayer()
        {
            var body = player.GetComponent<Rigidbody2D>();
            float baseX = game.Base.transform.position.x;
            body.position = new Vector2(baseX - 15f, body.position.y);
            yield return new WaitForFixedUpdate();
            SpawnZombie(baseX - 15.8f);

            yield return WaitSeconds(1.5f);

            Assert.Less(player.Current, player.Maximum, "A área de ataque deve espelhar quando o zumbi olha para a direita.");
        }

        [UnityTest]
        public IEnumerator ZombieFromTheLeft_DamagesTheBase()
        {
            var body = player.GetComponent<Rigidbody2D>();
            var baseCollider = game.Base.GetComponent<Collider2D>();
            body.position = new Vector2(baseCollider.bounds.max.x + 30f, body.position.y);
            SpawnZombie(baseCollider.bounds.min.x - 1.5f);

            yield return WaitSeconds(2.5f);

            Assert.Less(game.Base.Health.Current, game.Base.Health.Maximum);
        }

        [UnityTest]
        public IEnumerator Player_WalksThroughTheBase()
        {
            yield return new WaitForFixedUpdate();
            var baseCollider = game.Base.GetComponent<Collider2D>();
            foreach (var collider in player.GetComponents<Collider2D>())
                Assert.IsTrue(Physics2D.GetIgnoreCollision(baseCollider, collider), "A base fica no centro: o jogador precisa atravessá-la.");
        }

        [UnityTest]
        public IEnumerator AmmoPickup_AddsBulletsToReserve_AndDisappears()
        {
            var combat = player.GetComponent<PlayerCombat>();
            int before = combat.Ammo.Reserve;
            var pickup = Object.Instantiate(Load<GameObject>("Assets/Prefabs/Combat/AmmoPickup.prefab"), player.transform.position, Quaternion.identity)
                .GetComponent<AmmoPickup>();
            pickup.Initialize(7);
            yield return WaitSeconds(0.2f);

            Assert.AreEqual(before + 7, combat.Ammo.Reserve);
            Assert.IsTrue(pickup == null, "O item deve sumir ao ser coletado.");
        }

        [UnityTest]
        public IEnumerator OffscreenIndicator_ShowsSideOfHiddenZombies_AndHidesWhenVisible()
        {
            var indicator = Object.FindAnyObjectByType<OffscreenThreatIndicator>();
            var cam = Camera.main;
            float halfWidth = cam.orthographicSize * cam.aspect;
            var zombie = SpawnZombie(cam.transform.position.x + halfWidth + 8f);
            yield return null;
            yield return null;
            Assert.AreEqual(1, indicator.RightCount);
            Assert.AreEqual(0, indicator.LeftCount);
            Assert.IsTrue(GameObject.Find("RightThreat") != null, "O aviso da direita deve aparecer.");

            zombie.GetComponent<Rigidbody2D>().position = new Vector2(cam.transform.position.x + 1f, -3.8f);
            zombie.transform.position = new Vector3(cam.transform.position.x + 1f, -3.8f, 0);
            yield return null;
            yield return null;
            Assert.AreEqual(0, indicator.RightCount);
            Assert.IsTrue(GameObject.Find("RightThreat") == null, "O aviso some quando o zumbi entra na tela.");
        }

        [UnityTest]
        public IEnumerator DepletedResource_ShowsRespawnTimer()
        {
            var node = Object.FindObjectsByType<ResourceNode>(FindObjectsInactive.Exclude).First();
            var timer = node.GetComponentInChildren<ResourceRespawnTimer>().GetComponent<TMPro.TMP_Text>();
            Assert.IsFalse(timer.enabled);
            for (int i = 0; i < 20 && node.Remaining > 0; i++) node.Interact(player.gameObject);
            yield return null;

            Assert.IsTrue(node.IsRegenerating);
            Assert.IsTrue(timer.enabled);
            StringAssert.EndsWith("s", timer.text);
        }

        private static ConstructionPoint PointFor(string buildingId) =>
            Object.FindObjectsByType<ConstructionPoint>(FindObjectsInactive.Exclude).First(p => p.Building != null && p.Building.Id == buildingId);

        [UnityTest]
        public IEnumerator Sawmill_IsBoughtWithCoins_BuildsFor45s_ThenProduces()
        {
            var point = PointFor("sawmill");
            var resources = Object.FindAnyObjectByType<ResourceManager>();
            foreach (var cost in point.Building.Costs) resources.TryAdd(cost.Resource, cost.Amount);
            var coins = point.Building.Costs.First(c => c.Resource.Id == "coins").Resource;
            int coinsBefore = resources.GetAmount(coins);

            point.Interact(player.gameObject);
            yield return null;

            var building = point.InstalledBuilding;
            Assert.IsNotNull(building, "A serraria deve ser instalada.");
            Assert.Less(resources.GetAmount(coins), coinsBefore, "Deve gastar moedas.");
            Assert.AreEqual(45f, point.Building.ConstructionTime, 0.01f);
            Assert.IsTrue(building.IsUnderConstruction);
            var producer = building.GetComponent<ResourceProducer>();
            Assert.IsFalse(producer.enabled, "Não produz durante a obra.");
            Assert.IsTrue(building.GetComponentInChildren<ConstructionSite>() != null, "Mostra o visual de obra.");

            building.CompleteConstruction();
            yield return null;
            Assert.IsFalse(building.IsUnderConstruction);
            Assert.IsTrue(producer.enabled);
            Assert.IsTrue(building.GetComponentInChildren<ConstructionSite>() == null, "O visual de obra some ao terminar.");
            Assert.AreEqual("wood", producer.Resource.Id);
        }

        [UnityTest]
        public IEnumerator Producer_StoresUpToCapacity_AndPlayerCollectsWithInteract()
        {
            var point = PointFor("scrapyard");
            var resources = Object.FindAnyObjectByType<ResourceManager>();
            foreach (var cost in point.Building.Costs) resources.TryAdd(cost.Resource, cost.Amount);
            point.Interact(player.gameObject);
            yield return null;
            var building = point.InstalledBuilding;
            building.CompleteConstruction();
            var producer = building.GetComponent<ResourceProducer>();
            producer.ApplyStats(5, 0.5f, 10);
            int stockBefore = resources.GetAmount(producer.Resource);

            yield return WaitSeconds(1.6f);
            Assert.AreEqual(10, producer.Stored, "Produz até encher o depósito.");
            Assert.IsTrue(producer.IsFull);
            Assert.AreEqual(stockBefore, resources.GetAmount(producer.Resource), "Não entrega sozinho ao jogador.");

            var body = player.GetComponent<Rigidbody2D>();
            body.position = new Vector2(building.transform.position.x, body.position.y);
            yield return WaitSeconds(0.1f);
            Assert.IsTrue(player.GetComponent<PlayerInteraction>().TryInteract());
            Assert.AreEqual(stockBefore + 10, resources.GetAmount(producer.Resource));
            Assert.AreEqual(0, producer.Stored);
        }

        [UnityTest]
        public IEnumerator DamagedBase_CanBeRepaired_SpendingResources()
        {
            var resources = Object.FindAnyObjectByType<ResourceManager>();
            var repair = game.Base.GetComponent<Repairable>();
            game.Base.Health.TakeDamage(100);
            yield return null;
            foreach (var cost in repair.CurrentCost()) resources.TryAdd(cost.Resource, cost.Amount);
            var costs = repair.CurrentCost().Select(c => (c.Resource, c.Amount)).ToArray();
            var before = costs.Select(c => resources.GetAmount(c.Resource)).ToArray();

            Assert.IsTrue(repair.CanInteract(player.gameObject));
            repair.Interact(player.gameObject);

            Assert.AreEqual(game.Base.Health.Maximum, game.Base.Health.Current);
            for (int i = 0; i < costs.Length; i++)
                Assert.AreEqual(before[i] - costs[i].Amount, resources.GetAmount(costs[i].Resource));
            Assert.IsFalse(repair.CanInteract(player.gameObject), "Com vida cheia não há o que consertar.");
        }

        [UnityTest]
        public IEnumerator ConstructionPreview_OnlyShowsWhenPlayerIsNear()
        {
            var point = PointFor("sawmill");
            var preview = point.GetComponent<ConstructionPreview>();
            var body = player.GetComponent<Rigidbody2D>();
            body.position = new Vector2(point.transform.position.x + 15f, body.position.y);
            yield return WaitSeconds(0.1f);
            Assert.IsFalse(preview.IsShowing, "Longe do ponto a prévia fica escondida.");

            body.position = new Vector2(point.transform.position.x + 0.5f, body.position.y);
            yield return WaitSeconds(0.1f);
            Assert.IsTrue(preview.IsShowing, "Perto do ponto a prévia aparece.");
        }

        [UnityTest]
        public IEnumerator Turret_AimsLeft_AndShootsZombiesOnThatSide()
        {
            var tower = Object.Instantiate(Load<GameObject>("Assets/Prefabs/Building/Tower.prefab"), new Vector3(10f, -3.8f, 0), Quaternion.identity);
            tower.GetComponent<TurretController>().Initialize(game);
            var body = player.GetComponent<Rigidbody2D>();
            body.position = new Vector2(60f, body.position.y);
            var zombie = SpawnZombie(2f);
            int before = zombie.Health.Current;
            yield return WaitSeconds(2.5f);

            Assert.Less(tower.transform.Find("TurretHead").localScale.y, 0, "A cabeça espelha ao mirar para a esquerda.");
            Assert.IsTrue(zombie == null || zombie.Health.Current < before, "A torreta deve acertar o zumbi.");
        }

        [UnityTest]
        public IEnumerator Reload_TiltsTheWeapon_AndReturnsToRest()
        {
            var combat = player.GetComponent<PlayerCombat>();
            var held = player.transform.Find("Visual/SniperAppearance/Body/HeldWeapon");
            var rest = held.localRotation;
            combat.Ammo.TryConsume();
            Assert.IsTrue(combat.StartReload());
            yield return WaitSeconds(combat.Weapon.ReloadDuration * 0.45f);
            Assert.Greater(Quaternion.Angle(rest, held.localRotation), 10f, "A arma deve levantar durante a recarga.");

            yield return WaitSeconds(combat.Weapon.ReloadDuration * 0.55f + 0.2f);
            Assert.IsFalse(combat.IsReloading);
            Assert.Less(Quaternion.Angle(rest, held.localRotation), 0.5f, "A arma volta à pose original.");
        }

        [UnityTest]
        public IEnumerator SwitchingWeaponMidReload_KeepsTheNewWeaponPose()
        {
            var combat = player.GetComponent<PlayerCombat>();
            var held = player.transform.Find("Visual/SniperAppearance/Body/HeldWeapon");
            var sniper = Resources.FindObjectsOfTypeAll<WeaponData>().FirstOrDefault(w => w.name == "Sniper");
#if UNITY_EDITOR
            if (sniper == null) sniper = AssetDatabase.LoadAssetAtPath<WeaponData>("Assets/Data/Weapons/Sniper.asset");
#endif
            Assert.IsNotNull(sniper);
            combat.Ammo.TryConsume();
            Assert.IsTrue(combat.StartReload());
            yield return WaitSeconds(combat.Weapon.ReloadDuration * 0.45f);

            combat.Equip(sniper);
            var sniperPos = held.localPosition;
            var sniperRot = held.localRotation;
            yield return null;
            yield return null;
            Assert.Less(Vector3.Distance(sniperPos, held.localPosition), 0.001f, "A troca de arma não pode ser desfeita pela animação.");
            Assert.Less(Quaternion.Angle(sniperRot, held.localRotation), 0.5f);

            combat.Ammo.TryConsume();
            Assert.IsTrue(combat.StartReload());
            yield return WaitSeconds(sniper.ReloadDuration * 0.45f);
            Assert.Greater(Quaternion.Angle(sniperRot, held.localRotation), 10f, "A sniper também anima a recarga.");
            yield return WaitSeconds(sniper.ReloadDuration * 0.55f + 0.2f);
            Assert.Less(Vector3.Distance(sniperPos, held.localPosition), 0.001f, "Volta à pose da sniper, não da arma anterior.");
        }

        [UnityTest]
        public IEnumerator PlayerHit_FlashesRed()
        {
            var head = player.transform.Find("Visual/SniperAppearance/Body/Head").GetComponent<SpriteRenderer>();
            var original = head.color;
            player.TakeDamage(5);
            yield return null;
            Assert.AreNotEqual(original, head.color, "O jogador pisca ao levar dano.");
            yield return WaitSeconds(0.4f);
            Assert.AreEqual(original, head.color);
        }

        [UnityTest]
        public IEnumerator ZombieSound_GetsLouderAsItApproachesThePlayer()
        {
            var body = player.GetComponent<Rigidbody2D>();
            body.position = new Vector2(20f, body.position.y);
            var far = SpawnZombie(38f);
            var near = SpawnZombie(25f);
            yield return null;
            yield return null;

            float farVolume = far.GetComponent<ProjetoFinal.Audio.ProximityAudio>().CurrentVolume;
            float nearVolume = near.GetComponent<ProjetoFinal.Audio.ProximityAudio>().CurrentVolume;
            Assert.Greater(nearVolume, farVolume);
        }

        [UnityTest]
        public IEnumerator PlayerDamage_UpdatesLifeOnHud()
        {
            player.TakeDamage(10);
            yield return null;

            var label = GameObject.Find("PlayerCard").transform.Find("PlayerHealth").GetComponent<TMPro.TMP_Text>();
            Assert.AreEqual((player.Maximum - 10) + " / " + player.Maximum, label.text);
            Assert.IsNull(player.transform.Find("PlayerHealth"), "A barra sobre o jogador foi removida.");
        }

        [UnityTest]
        public IEnumerator Music_FollowsTurnPhase()
        {
            yield return null;
            var music = Object.FindAnyObjectByType<ProjetoFinal.Audio.MusicPlayer>();
            Assert.AreEqual("Preparation", music.Current.name);

            game.TransitionTo(GameState.Combat);
            yield return null;
            Assert.AreEqual("WaveCombat", music.Current.name);
        }

        [UnityTest]
        public IEnumerator PlayerDeath_EndsTheGame()
        {
            player.TakeDamage(player.Maximum);
            yield return null;

            Assert.AreEqual(GameState.GameOver, game.State);
            Assert.IsTrue(game.PlayerDefeated);
        }

        [UnityTest]
        public IEnumerator Camera_StartsOnPlayer_AndFollowsWhenMoving()
        {
            yield return WaitSeconds(0.3f);
            var cam = Camera.main;
            float halfWidth = cam.orthographicSize * cam.aspect;
            Assert.Less(Mathf.Abs(cam.transform.position.x - player.transform.position.x), halfWidth, "O jogador deve aparecer na tela ao iniciar.");

            var body = player.GetComponent<Rigidbody2D>();
            body.position = new Vector2(30f, body.position.y);
            yield return WaitSeconds(1.5f);

            Assert.AreEqual(30f, cam.transform.position.x, 0.5f, "A câmera deve acompanhar o jogador.");
        }

        [Test]
        public void Survivor_HasAnimatorWithIsRunningParameter()
        {
            var animator = Object.FindAnyObjectByType<PlayerController>().GetComponentInChildren<Animator>();
            Assert.IsNotNull(animator);
            Assert.IsTrue(animator.parameters.Any(p => p.name == "IsRunning" && p.type == AnimatorControllerParameterType.Bool));
        }

        [Test]
        public void CollisionMatrix_ProjectileOnlyHitsZombies_AttackOnlyReachesPlayerAndStructures()
        {
            int projectile = LayerMask.NameToLayer("Projectile"), zombie = LayerMask.NameToLayer("Zombie");
            int attack = LayerMask.NameToLayer("EnemyAttack"), playerLayer = LayerMask.NameToLayer("Player");
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(projectile, zombie));
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(projectile, 0));
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(projectile, playerLayer));
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(attack, playerLayer));
            Assert.IsFalse(Physics2D.GetIgnoreLayerCollision(attack, 0));
            Assert.IsTrue(Physics2D.GetIgnoreLayerCollision(attack, zombie));
        }
    }
}
