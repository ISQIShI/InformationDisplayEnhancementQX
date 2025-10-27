using Duckov.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    [HarmonyPatch]
    [PatchGroup(nameof(HealthValueDisplay))]
    public class HealthValueDisplay
    {
        /// <summary>
        /// 用于在初始化血条图标后刷新血量文本
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthBar), "RefreshCharacterIcon")]
        public static void RefreshCharacterIconPostfix(HealthBar __instance)
        {
            // 刷新血量文本
            InternalRefreshHealthText(__instance);
        }

        /// <summary>
        /// 用于创建血量文本
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="___nameText"></param>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthBar), "Awake")]
        public static void CreateHealthText(HealthBar __instance, TextMeshProUGUI ___nameText)
        {
            GameObject healthTextObj = TextHelper.GetText(new TextHelper.TextConfigure
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

            var healthText = __instance.transform.Find("HealthTextObj/HealthText");
            if (healthText.TryGetComponent<TextMeshProUGUI>(out var textMeshProUGUI))
            {
                textMeshProUGUI.text = $"{currentHealth:N1} / {maxHealth:N1}";
            }
        }
    }
}
