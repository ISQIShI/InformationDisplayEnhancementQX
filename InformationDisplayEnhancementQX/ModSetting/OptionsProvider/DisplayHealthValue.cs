using InformationDisplayEnhancementQX.Utils;
using SodaCraft.Localizations;

namespace InformationDisplayEnhancementQX.ModSetting.OptionsProvider
{
    public class DisplayHealthValue : OptionsProviderBase
    {
        public const string KEY = LocalizationHelper.KeyPrefix + nameof(DisplayHealthValue);
        public override string Key => KEY;

        public const string Option_0 = LocalizationHelper.KeyPrefix + "Option_AllCharacter";
        public const string Option_1 = LocalizationHelper.KeyPrefix + "Option_OnlyPlayer";
        public const string Option_2 = LocalizationHelper.KeyPrefix + "Option_OnlyEnemy";
        public const string Option_3 = LocalizationHelper.KeyPrefix + "Option_Off";

        private void Awake()
        {
            LevelManager.OnLevelInitialized += this.RefreshOnLevelInited;
        }

        private void OnDestroy()
        {
            LevelManager.OnLevelInitialized -= this.RefreshOnLevelInited;
        }
        private void RefreshOnLevelInited()
        {
            int index = (int)ModSettingCenter.ShowHealthValue;
            Set(index);
        }

        public override string GetCurrentOption()
        {
            return ModSettingCenter.ShowHealthValue switch
            {
                Option.AllCharacter => Option_0.ToPlainText(),
                Option.OnlyPlayer => Option_1.ToPlainText(),
                Option.OnlyEnemy => Option_2.ToPlainText(),
                Option.Off => Option_3.ToPlainText(),
                _ => Option_0.ToPlainText(),
            };
        }

        public override string[] GetOptions()
        {
            return new string[]
            {
                Option_0.ToPlainText(),
                Option_1.ToPlainText(),
                Option_2.ToPlainText(),
                Option_3.ToPlainText()
            };
        }

        public override void Set(int index)
        {
            ModSettingCenter.ShowHealthValue = (Option)index;
        }

        public enum Option
        {
            AllCharacter = 0,
            OnlyPlayer = 1,
            OnlyEnemy = 2,
            Off = 3
        }
    }
}
