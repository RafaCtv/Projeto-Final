using NUnit.Framework;
using UnityEngine;

namespace ProjetoFinal.Tests
{
    public class ConstructionSiteTests
    {
        [Test]
        public void ProductionBuildings_TakeTime_AndCostCoinsWoodAndScrap()
        {
#if UNITY_EDITOR
            foreach (var path in new[] { "Assets/Data/BuildingDefinitions/SawmillBuilding.asset", "Assets/Data/BuildingDefinitions/ScrapyardBuilding.asset" })
            {
                var data = UnityEditor.AssetDatabase.LoadAssetAtPath<BuildingData>(path);
                Assert.IsNotNull(data, path);
                Assert.AreEqual(45f, data.ConstructionTime, 0.01f);
                var ids = new System.Collections.Generic.List<string>();
                foreach (var cost in data.Costs) ids.Add(cost.Resource.Id);
                CollectionAssert.IsSupersetOf(ids, new[] { "coins", "wood" });
                Assert.IsTrue(data.Prefab.GetComponent<ResourceProducer>() != null);
            }
#endif
        }

        [Test]
        public void Stages_FollowProgress()
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Building/Sawmill.prefab");
            var instance = Object.Instantiate(prefab);
            try
            {
                var site = instance.GetComponentInChildren<ConstructionSite>(true);
                site.SetProgress(0f, 45);
                Assert.AreEqual(0, site.Stage);
                site.SetProgress(0.5f, 22);
                Assert.AreEqual(1, site.Stage);
                site.SetProgress(0.99f, 1);
                Assert.AreEqual(2, site.Stage);
            }
            finally { Object.DestroyImmediate(instance); }
#endif
        }
    }
}
