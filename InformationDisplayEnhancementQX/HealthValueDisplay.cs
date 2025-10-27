using Duckov.UI;
using HarmonyLib;
using InformationDisplayEnhancementQX.ModSetting;
using InformationDisplayEnhancementQX.ModSetting.OptionsProvider;
using InformationDisplayEnhancementQX.Utils;
using TMPro;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    [HarmonyPatch]
    [PatchGroup(nameof(HealthValueDisplay))]
    public class HealthValueDisplay
    {
        ///// <summary>
        ///// 用于在初始化血条图标后刷新血量文本
        ///// </summary>
        ///// <param name="__instance"></param>
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HealthBar), "RefreshCharacterIcon")]
        //public static void RefreshCharacterIconPostfix(HealthBar __instance)
        //{
        //    // 刷新血量文本
        //    InternalRefreshHealthText(__instance);
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(HealthBar), "Awake")]
        /// <summary>
        /// 用于创建血量文本
        /// </summary>
        /// <param name="__instance"></param>
        public static GameObject CreateHealthText(HealthBar __instance)
        {
            return TextHelper.Instance.GetText(new TextHelper.TextConfigure
            {
                active = true,
                parent = __instance.transform,
                localPosition = new Vector3(0, -9, 0),
                localScale = new Vector3(0.7f, 0.7f, 0.7f),
                textTemplateName = "HealthTextObj",
                textName = "HealthText"
            });
        }


        /// <summary>
        /// 用于在血量更改时刷新血量文本
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthBar), "Refresh")]
        public static void RefreshHealthText(HealthBar __instance)
        {
            InternalRefreshHealthText(__instance);
        }

        private static void InternalRefreshHealthText(HealthBar __instance)
        {
            if (!__instance.target) return;
            float currentHealth = __instance.target.CurrentHealth;
            float maxHealth = __instance.target.MaxHealth;
            if (currentHealth < 0) currentHealth = 0;
            if (maxHealth < 0) maxHealth = 0;



            GameObject healthTextObj = __instance.transform.Find("HealthTextObj")?.gameObject;

            switch (ModSettingCenter.ShowHealthValue)
            {
                case DisplayHealthValue.Option.AllCharacter:
                    // 所有角色均显示血量文本
                    if (!healthTextObj)
                    {
                        healthTextObj = CreateHealthText(__instance);
                        if (!healthTextObj) return;
                    }
                    break;
                case DisplayHealthValue.Option.OnlyPlayer:
                    {
                        // 仅玩家显示血量文本
                        bool isPlayer = __instance.target.IsMainCharacterHealth;
                        if (isPlayer && !healthTextObj)
                        {
                            healthTextObj = CreateHealthText(__instance);
                            if (!healthTextObj) return;
                        }
                        else if (!isPlayer && healthTextObj)
                        {
                            GameObject.Destroy(healthTextObj);
                            healthTextObj = null;
                        }
                    }
                    break;
                case DisplayHealthValue.Option.OnlyEnemy:
                    {
                        // 仅敌人显示血量文本
                        bool isPlayer = __instance.target.IsMainCharacterHealth;
                        if (!isPlayer && !healthTextObj)
                        {
                            healthTextObj = CreateHealthText(__instance);
                            if (!healthTextObj) return;
                        }
                        else if (isPlayer && healthTextObj)
                        {
                            GameObject.Destroy(healthTextObj);
                            healthTextObj = null;
                        }
                    }
                    break;
                case DisplayHealthValue.Option.Off:
                    // 不显示血量文本
                    if (healthTextObj)
                    {
                        GameObject.Destroy(healthTextObj);
                        healthTextObj = null;
                    }
                    break;
                default:
                    return;
            }


            var healthText = healthTextObj?.transform.Find("HealthText");
            if (healthText == null) return;
            if (healthText.TryGetComponent<TextMeshProUGUI>(out var textMeshProUGUI))
            {
                textMeshProUGUI.text = ModSettingCenter.DecimalPlacesOfHealthValue switch
                {
                    0 => $"{currentHealth:N0} / {maxHealth:N0}",
                    1 => $"{currentHealth:N1} / {maxHealth:N1}",
                    2 => $"{currentHealth:N2} / {maxHealth:N2}",
                    _ => $"{currentHealth:N1} / {maxHealth:N1}",
                };
            }
        }
    }
}
