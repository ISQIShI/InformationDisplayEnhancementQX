using Duckov.UI;
using HarmonyLib;
using TMPro;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    // [HarmonyPatch(typeof(HealthBar))]
    public static class CharacterNameDisplayOld
    {
        [HarmonyPrefix]
        [HarmonyPatch("RefreshCharacterIcon")]
        public static bool RefreshCharacterIconPrefix(HealthBar __instance)
        {
            // 将非玩家的角色名称设置为允许显示
            if (!__instance.target) return true;
            CharacterMainControl characterMainControl = __instance.target.TryGetCharacter();
            if (!characterMainControl) return true;
            CharacterRandomPreset characterPreset = characterMainControl.characterPreset;
            if (!characterPreset) return true;
            characterPreset.showName = !characterMainControl.IsMainCharacter;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("RefreshCharacterIcon")]
        public static void RefreshCharacterIconPostfix(HealthBar __instance)
        {
            // 刷新血量文本
            InternalRefreshHealthText(__instance);

            do
            {
                if (!__instance.target) break;
                CharacterMainControl characterMainControl = __instance.target.TryGetCharacter();
                if (!characterMainControl) break;
                CharacterRandomPreset characterPreset = characterMainControl.characterPreset;
                if (!characterPreset) break;
                var typeName = characterPreset.nameKey;
                if (!TaskObjectiveMarker.Instance.IsKillTarget(typeName)) break;
                // 需要显示任务目标文本
                GameObject taskTargetTextObj = __instance.transform.Find("TaskTargetTextObj")?.gameObject;
                GameObject taskTargetText = null;
                if (taskTargetTextObj)
                {
                    taskTargetText = taskTargetTextObj.transform.Find("TaskTargetText")?.gameObject;
                }
                if (!taskTargetText)
                {
                    taskTargetTextObj = TextHelper.GetText(new TextHelper.TextConfigure
                    {
                        active = true,
                        parent = __instance.transform,
                        localPosition = new Vector3(0, -150, 0),
                        localScale = Vector3.one,
                        textTemplateName = "TaskTargetTextObj",
                        textName = "TaskTargetText"
                    });
                    taskTargetText = taskTargetTextObj.transform.Find("TaskTargetText").gameObject;
                }
                else
                {
                    taskTargetTextObj.SetActive(true);
                }
                if (taskTargetText.TryGetComponent<TextMeshProUGUI>(out var textMeshProUGUI))
                {
                    textMeshProUGUI.text = $"任务目标：{TaskObjectiveMarker.Instance.GetKillTargetAmount(typeName)}";
                    TaskObjectiveMarker.Instance.AddListener(typeName, textMeshProUGUI);
                    TaskObjectiveMarker.Instance.TextToType[textMeshProUGUI] = typeName;
                }
                return;
            } while (false);

            GameObject tempTaskTargetTextObj = __instance.transform.Find("TaskTargetTextObj")?.gameObject;
            if (tempTaskTargetTextObj)
            {
                tempTaskTargetTextObj.SetActive(false);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("DeathTask")]
        public static void DeathTaskPrefix(HealthBar __instance, Health health)
        {
            if (__instance.target == health)
            {
                GameObject tempTaskTargetText = __instance.transform.Find("TaskTargetTextObj/TaskTargetText")?.gameObject;
                if (tempTaskTargetText && tempTaskTargetText.TryGetComponent<TextMeshProUGUI>(out var textMeshProUGUI))
                {
                    if (TaskObjectiveMarker.Instance.TextToType.TryGetValue(textMeshProUGUI, out var typeName))
                    {
                        TaskObjectiveMarker.Instance.RemoveListener(typeName, textMeshProUGUI);
                        TaskObjectiveMarker.Instance.TextToType.Remove(textMeshProUGUI);
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
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
            GameObject healthText = healthTextObj.transform.Find("HealthText").gameObject;

            if (healthText.TryGetComponent<TextMeshProUGUI>(out var textMeshProUGUI))
            {
                textMeshProUGUI.text = string.Empty;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Refresh")]
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

        //[HarmonyPostfix]
        //[HarmonyPatch("RegisterEvents")]
        //public static void RegisterEventsPostfix(HealthBar __instance, TextMeshProUGUI ___nameText)
        //{
        //    if (__instance.target)
        //    {
        //        // 任务目标的文本
        //        GameObject gameObject = Object.Instantiate(___nameText.transform.parent.gameObject);
        //        gameObject.name = "TaskTargetObj";
        //        GameObject.Destroy(gameObject.transform.Find("Image").gameObject);
        //        var taskTargetTextObj = gameObject.transform.Find("NameText");
        //        taskTargetTextObj.name = "TaskTargetText";
        //        gameObject.transform.SetParent(__instance.transform);
        //        gameObject.transform.localPosition = new Vector3(0, -150, 0);
        //        gameObject.transform.localScale = Vector3.one;
        //        if (taskTargetTextObj.TryGetComponent<TextMeshProUGUI>(out var textMeshProUGUI))
        //        {
        //            textMeshProUGUI.text = $"任务目标：100";
        //        }
        //    }
        //}

        //[HarmonyPostfix]
        //[HarmonyPatch("UnregisterEvents")]
        //public static void UnregisterEventsPostfix(HealthBar __instance, TextMeshProUGUI ___nameText)
        //{
        //    if (__instance.target)
        //    {
        //        var gameObject = __instance.transform.Find("TaskTargetObj")?.gameObject;
        //        if (gameObject) GameObject.Destroy(gameObject);

        //    }
        //}

    }
}
