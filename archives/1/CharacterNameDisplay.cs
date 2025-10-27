using Duckov.UI;
using HarmonyLib;
using TMPro;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    [HarmonyPatch]
    [PatchGroup(nameof(CharacterNameDisplay))]
    public class CharacterNameDisplay
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthBar), "RefreshCharacterIcon")]
        public static void RefreshCharacterIconPostfix(HealthBar __instance, TextMeshProUGUI ___nameText)
        {
            if (!__instance.target) return;
            CharacterMainControl characterMainControl = __instance.target.TryGetCharacter();
            if (!characterMainControl) return;
            CharacterRandomPreset characterPreset = characterMainControl.characterPreset;
            if (!characterPreset) return;
            // 如果非玩家的角色不显示名称，则强制显示名称
            if (!characterPreset.showName && !characterMainControl.IsMainCharacter)
            {
                ___nameText.text = characterPreset.DisplayName;
                ___nameText.gameObject.SetActive(value: true);
            }
        }
    }
}
