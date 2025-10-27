using InformationDisplayEnhancementQX.Utils;

namespace InformationDisplayEnhancementQX.ModSetting.OptionsProvider
{
    public class DecimalPlacesOfHealthValue : OptionsProviderBase
    {
        public const string KEY = LocalizationHelper.KeyPrefix + nameof(DecimalPlacesOfHealthValue);
        public override string Key => KEY;
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
            int index = ModSettingCenter.DecimalPlacesOfHealthValue;
            Set(index);
        }

        public override string GetCurrentOption()
        {
            return ModSettingCenter.DecimalPlacesOfHealthValue.ToString();
        }

        public override string[] GetOptions()
        {
            return new string[]
            {
                "0",
                "1",
                "2"
            };
        }

        public override void Set(int index)
        {
            ModSettingCenter.DecimalPlacesOfHealthValue = index;
        }
    }
}
