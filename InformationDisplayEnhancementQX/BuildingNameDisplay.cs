
using Duckov.Buildings;
using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    public class BuildingNameDisplay
    {
        private static LinkedList<BuildingNameDisplayData> _datas = new LinkedList<BuildingNameDisplayData>();

        public static void Init()
        {
            List<Building> buildingPrefabss = AccessTools.FieldRefAccess<BuildingDataCollection, List<Building>>(BuildingDataCollection.Instance, "prefabs");
            var graphicsContainerField = AccessTools.FieldRefAccess<Building, GameObject>("graphicsContainer");
            // var functionContainerField = AccessTools.FieldRefAccess<Building, GameObject>("functionContainer");
            foreach (Building building in buildingPrefabss)
            {
                var graphicsContainer = graphicsContainerField(building);
                if (graphicsContainer == null) continue;
                switch (building.ID)
                {
                    case "Gem":
                        AddBuildingNameForGem(building, graphicsContainer);
                        break;
                }
            }
        }

        public static void Release()
        {
            foreach (var data in _datas)
            {
                data.Release();
            }
        }

        private static void AddBuildingNameForGem(Building building, GameObject graphicsContainer)
        {
            GameObject nameDisplayObject = new GameObject("BuildingNameDisplay_Gem");
            nameDisplayObject.AddComponent<MeshRenderer>();
            var textComponent = nameDisplayObject.AddComponent<TextMeshPro>();
            var textLocalization = nameDisplayObject.AddComponent<TextMeshProLocalization>();
            textLocalization.LocalizationKey = building.DisplayNameKey;
            textComponent.text = building.DisplayName;

            var nameObjectTransform = nameDisplayObject.transform;
            nameObjectTransform.SetParent(graphicsContainer.transform);
            nameObjectTransform.localPosition = new Vector3(0f, 3f, 1.5f);
            nameObjectTransform.localEulerAngles = new Vector3(90f, 270f, 0f);
            nameObjectTransform.localScale = new Vector3(0.3f, 0.3f, 1f);


            var data = new BuildingNameDisplayData(nameDisplayObject);
            _datas.AddLast(data);
        }

        public struct BuildingNameDisplayData
        {
            private GameObject _nameDisplayObject;

            public BuildingNameDisplayData(GameObject nameDisplayObject)
            {
                _nameDisplayObject = nameDisplayObject;
            }

            public void Release()
            {
                if (_nameDisplayObject != null)
                {
                    GameObject.Destroy(_nameDisplayObject);
                    _nameDisplayObject = null;
                }
            }
        }
    }
}
