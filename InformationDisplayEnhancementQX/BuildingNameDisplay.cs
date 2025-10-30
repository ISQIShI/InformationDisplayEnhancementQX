
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

        private static bool _initialized = false;

        public static void Init()
        {
            if (_initialized) return;
            _initialized = true;

            List<Building> buildingPrefabss = AccessTools.FieldRefAccess<BuildingDataCollection, List<Building>>(BuildingDataCollection.Instance, "prefabs");
            var graphicsContainerField = AccessTools.FieldRefAccess<Building, GameObject>("graphicsContainer");
            // var functionContainerField = AccessTools.FieldRefAccess<Building, GameObject>("functionContainer");
            foreach (Building building in buildingPrefabss)
            {
                var graphicsContainer = graphicsContainerField(building);
                if (graphicsContainer == null) continue;
                switch (building.ID)
                {
                    case "Gem":// 健身房
                        AddBuildingName(building, graphicsContainer,
                            new Vector3(0f, 3f, 1.5f),
                            new Vector3(90f, 270f, 0f),
                            new Vector3(0.3f, 0.3f, 1f));
                        break;
                    case "WorkbenchAdvance":// 工作台
                        AddBuildingName(building, graphicsContainer,
                            new Vector3(0.5f, 3f, -0.3f),
                            new Vector3(90f, 0f, 0f),
                            new Vector3(0.2f, 0.2f, 1f));
                        break;
                    case "BlackMarket":// 黑市联络点
                        AddBuildingName(building, graphicsContainer,
                            new Vector3(0.5f, 3f, -0.5f),
                            new Vector3(90f, 0f, 0f),
                            new Vector3(0.2f, 0.2f, 1f));
                        break;
                    case "Merchant_Ming":// 技术中心
                        AddBuildingName(building, graphicsContainer,
                            new Vector3(0.5f, 3f, 0f),
                            new Vector3(90f, 0f, 0f),
                            new Vector3(0.25f, 0.25f, 1f));
                        break;
                    case "Kitchen":// 厨房
                        AddBuildingName(building, graphicsContainer,
                            new Vector3(-0.25f, 3f, -1.5f),
                            new Vector3(90f, 90f, 0f),
                            new Vector3(0.2f, 0.2f, 1f));
                        break;
                    case "Merchant_Weapon":// 武器店
                        AddBuildingName(building, graphicsContainer,
                            new Vector3(1f, 3f, 0f),
                            new Vector3(90f, 0f, 0f),
                            new Vector3(0.3f, 0.3f, 1f));
                        break;
                    case "TeleportMachine"://传送装置
                        AddBuildingName(building, graphicsContainer,
                            new Vector3(1f, 3f, 0f),
                            new Vector3(90f, 0f, 0f),
                            new Vector3(0.25f, 0.25f, 1f));
                        break;
                    case "MedicStation":// 医疗站
                        AddBuildingName(building, graphicsContainer,
                            new Vector3(0.5f, 3f, -0.25f),
                            new Vector3(90f, 0f, 0f),
                            new Vector3(0.2f, 0.2f, 1f));
                        break;
                    case "Merchant_Equipment":// 防具店
                        AddBuildingName(building, graphicsContainer,
                            new Vector3(0.25f, 3f, 1.25f),
                            new Vector3(90f, 0f, 0f),
                            new Vector3(0.2f, 0.2f, 1f));
                        break;
                }
            }
        }

        public static void Release()
        {
            if (!_initialized) return;
            _initialized = false;
            foreach (var data in _datas)
            {
                data.Release();
            }
        }

        private static void AddBuildingName(Building building, GameObject graphicsContainer, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
        {
            GameObject nameDisplayObject = new GameObject($"BuildingNameDisplay_{building.ID}");
            nameDisplayObject.AddComponent<MeshRenderer>();
            var textComponent = nameDisplayObject.AddComponent<TextMeshPro>();
            var textLocalization = nameDisplayObject.AddComponent<BuildingNameDisplayHelper>();

            var nameObjectTransform = nameDisplayObject.transform;
            nameObjectTransform.SetParent(graphicsContainer.transform);
            nameObjectTransform.localPosition = localPosition;
            nameObjectTransform.localEulerAngles = localEulerAngles;
            nameObjectTransform.localScale = localScale;

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
