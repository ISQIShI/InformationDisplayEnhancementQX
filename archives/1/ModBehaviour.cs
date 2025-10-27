using System.Reflection;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        public const string HarmonyId = "InformationDisplayEnhancementQX";

        public static ModBehaviour Instance { get; private set; }

        public HarmonyHelper HarmonyHelperObj { get; private set; }



        protected override void OnAfterSetup()
        {
            base.OnAfterSetup();
            // 加载 Harmony 补丁
            var assembly = Assembly.GetExecutingAssembly();
            HarmonyHelper.CheckHarmonyVersion();
            HarmonyHelper.CheckHarmonyPatchClasses(assembly);
            if (HarmonyHelperObj == null) HarmonyHelperObj = new HarmonyHelper(HarmonyId);

            HarmonyHelperObj.PatchAllUngrouped(assembly);
            HarmonyHelperObj.PatchGroup(assembly, nameof(CharacterNameDisplay));
            HarmonyHelperObj.PatchGroup(assembly, nameof(HealthValueDisplay));

            TextHelper.Initialize(this);

            //gameObject.AddComponent<TaskObjectiveMarker>();

            Instance = this;

            Debug.Log("InformationDisplayEnhancementQX 已加载");
        }

        protected override void OnBeforeDeactivate()
        {
            base.OnBeforeDeactivate();
            // 卸载 Harmony 补丁
            HarmonyHelperObj.HarmonyInstance.UnpatchAll(HarmonyId);


            TextHelper.Release();

            //if (gameObject.TryGetComponent<TaskObjectiveMarker>(out var taskObjectiveMarker))
            //{
            //    GameObject.Destroy(taskObjectiveMarker);
            //}


            Instance = null;

            Debug.Log("InformationDisplayEnhancementQX 已卸载");
        }

    }

}
