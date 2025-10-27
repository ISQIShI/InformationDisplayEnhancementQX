using Duckov.Options;
using Duckov.Options.UI;
using HarmonyLib;
using InformationDisplayEnhancementQX.ModSetting.OptionsProvider;
using InformationDisplayEnhancementQX.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX.ModSetting
{
    [HarmonyPatch]
    public class ModSettingCenter
    {
        private static DisplayHealthValue.Option _showHealthValue = DisplayHealthValue.Option.AllCharacter;
        public static DisplayHealthValue.Option ShowHealthValue
        {
            get
            {
                return _showHealthValue;
            }
            set
            {
                if (_showHealthValue != value)
                {
                    _showHealthValue = value;
                    OptionsManager.Save<int>(DisplayHealthValue.KEY, (int)value);
                }
            }
        }

        private static bool _showCharacterName = true;
        public static bool ShowCharacterName
        {
            get
            {
                return _showCharacterName;
            }
            set
            {
                if (_showCharacterName != value)
                {
                    _showCharacterName = value;
                    OptionsManager.Save<int>(DisplayCharacterName.KEY, value ? 1 : 0);
                    if (value)
                    {
                        ModBehaviour.Instance.HarmonyHelperObj.PatchGroup(typeof(ModBehaviour).Assembly, nameof(CharacterNameDisplay));
                    }
                    else
                    {
                        ModBehaviour.Instance.HarmonyHelperObj.UnpatchGroup(typeof(ModBehaviour).Assembly, nameof(CharacterNameDisplay));
                    }
                }
            }
        }

        private static bool _showKillCountTaskTarget = true;
        public static bool ShowKillCountTaskTarget
        {
            get
            {
                return _showKillCountTaskTarget;
            }
            set
            {
                if (_showKillCountTaskTarget != value)
                {
                    _showKillCountTaskTarget = value;
                    OptionsManager.Save<int>(DisplayKillCountTaskTarget.KEY, value ? 1 : 0);
                    if (value)
                    {
                        GameObject tempObj = null;
                        if (!ModBehaviour.Instance.ChildGameObject.Any((gameObj) => gameObj != null && gameObj.name == nameof(KillCountTaskTargetDisplay)))
                        {
                            tempObj = new GameObject(nameof(KillCountTaskTargetDisplay), typeof(KillCountTaskTargetDisplay));
                            ModBehaviour.Instance.ChildGameObject.AddLast(tempObj);
                        }
                        ModBehaviour.Instance.HarmonyHelperObj.PatchGroup(typeof(ModBehaviour).Assembly, nameof(KillCountTaskTargetDisplay));
                        if (tempObj != null)
                        {
                            tempObj.SetActive(false);
                            tempObj.transform.SetParent(ModBehaviour.Instance.transform);
                        }
                    }
                    else
                    {
                        ModBehaviour.Instance.HarmonyHelperObj.UnpatchGroup(typeof(ModBehaviour).Assembly, nameof(KillCountTaskTargetDisplay));
                        GameObject tempObj = null;
                        foreach (var gameObj in ModBehaviour.Instance.ChildGameObject)
                        {
                            if (gameObj != null && gameObj.name == nameof(KillCountTaskTargetDisplay))
                            {
                                tempObj = gameObj;
                                break;
                            }
                        }
                        if (tempObj != null)
                        {
                            ModBehaviour.Instance.ChildGameObject.Remove(tempObj);
                            GameObject.Destroy(tempObj);
                        }
                    }
                }
            }
        }

        private static int _decimalPlacesOfHealthValue = 1;

        public static int DecimalPlacesOfHealthValue
        {
            get
            {
                return _decimalPlacesOfHealthValue;
            }
            set
            {
                if (_decimalPlacesOfHealthValue != value)
                {
                    _decimalPlacesOfHealthValue = value;
                    OptionsManager.Save<int>(OptionsProvider.DecimalPlacesOfHealthValue.KEY, value);
                }
            }
        }

        public static LinkedList<(OptionsPanel_TabButton tabButton, OptionsPanel optionsPanel)> TabButtons { get; } = new LinkedList<(OptionsPanel_TabButton tabButton, OptionsPanel optionsPanel)>();

        public static void Init()
        {
            _showHealthValue = (DisplayHealthValue.Option)OptionsManager.Load<int>(DisplayHealthValue.KEY, (int)DisplayHealthValue.Option.AllCharacter);
            _showCharacterName = OptionsManager.Load<int>(DisplayCharacterName.KEY, 1) == 0 ? false : true;
            _showKillCountTaskTarget = OptionsManager.Load<int>(DisplayKillCountTaskTarget.KEY, 1) == 0 ? false : true;
            _decimalPlacesOfHealthValue = OptionsManager.Load<int>(OptionsProvider.DecimalPlacesOfHealthValue.KEY, 1);
        }

        public static void Release()
        {
            var tabFieldAccess = AccessTools.FieldRefAccess<OptionsPanel_TabButton, GameObject>("tab");
            foreach (var (tabButton, optionsPanel) in TabButtons)
            {
                if (tabButton)
                {
                    if (optionsPanel)
                    {
                        var tabButtons = AccessTools.FieldRefAccess<OptionsPanel, List<OptionsPanel_TabButton>>(optionsPanel, "tabButtons");
                        tabButtons.Remove(tabButton);
                        if (optionsPanel.GetSelection() == tabButton)
                        {
                            optionsPanel.SetSelection(tabButtons.FirstOrDefault());
                        }
                    }

                    ref GameObject panel = ref tabFieldAccess(tabButton);
                    if (panel)
                    {
                        GameObject.Destroy(panel);
                        panel = null;
                    }
                    GameObject.Destroy(tabButton.gameObject);
                }
            }
            TabButtons.Clear();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OptionsPanel), "Setup")]
        public static void InitSettingUI(OptionsPanel __instance, List<OptionsPanel_TabButton> ___tabButtons)
        {
            //// 调整设置面板中鼠标滚轮滚动速度
            //GameObject scrollViewObj = __instance.transform.Find("ScrollView")?.gameObject;
            //if (scrollViewObj != null && scrollViewObj.TryGetComponent<ScrollRect>(out var scrollRect))
            //{
            //    scrollRect.scrollSensitivity = 40f;
            //}

            if (___tabButtons.Any((tabButton) => tabButton != null && tabButton.gameObject.name == SettingUIHelper.DefaultSettingTabButtonName))
            {
                return;
            }

            if (!SettingUIHelper.Instance)
            {
                LogHelper.Instance.LogError($"在 {nameof(ModSettingCenter)}.{nameof(InitSettingUI)} 中无法获取到 {nameof(SettingUIHelper)} 实例");
                return;
            }
            if (!SettingUIHelper.Instance.InitSettingUITemplate(___tabButtons))
            {
                return;
            }
            GameObject settingPanelObj = SettingUIHelper.Instance.CreateSettingPanel(__instance, SettingUIHelper.DefaultSettingTabButtonName, $"{LocalizationHelper.KeyPrefix}ModSettingButton", SettingUIHelper.DefaultSettingPanelName, out var tabButton);
            if (!settingPanelObj)
            {
                LogHelper.Instance.LogError($"在 {nameof(ModSettingCenter)}.{nameof(InitSettingUI)} 中无法创建设置面板");
                return;
            }
            SettingUIHelper.Instance.CreateDropdown<DisplayHealthValue>(settingPanelObj);
            SettingUIHelper.Instance.CreateDropdown<OptionsProvider.DecimalPlacesOfHealthValue>(settingPanelObj);
            SettingUIHelper.Instance.CreateDropdown<DisplayCharacterName>(settingPanelObj);
            SettingUIHelper.Instance.CreateDropdown<DisplayKillCountTaskTarget>(settingPanelObj);
            ___tabButtons.Add(tabButton);
            TabButtons.AddLast((tabButton, __instance));
        }


    }
}
