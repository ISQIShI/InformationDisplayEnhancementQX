using InformationDisplayEnhancementQX.Utils;
using SodaCraft.Localizations;

namespace InformationDisplayEnhancementQX.ModSetting.OptionsProvider
{
    public class DisplayBuildingName : OptionsProviderBase
    {
        public const string KEY = LocalizationHelper.KeyPrefix + nameof(DisplayBuildingName);

        public override string Key => KEY;

        public const string Option_0 = LocalizationHelper.KeyPrefix + "Option_Off";
        public const string Option_1 = LocalizationHelper.KeyPrefix + "Option_On";

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
            int index = ModSettingCenter.ShowBuildingName ? 1 : 0;
            Set(index);
        }

        public override string GetCurrentOption()
        {
            return ModSettingCenter.ShowBuildingName ? Option_1.ToPlainText() : Option_0.ToPlainText();
        }

        public override string[] GetOptions()
        {
            return new string[]
            {
                Option_0.ToPlainText(),
                Option_1.ToPlainText()
            };
        }

        public override void Set(int index)
        {
            switch (index)
            {
                case 0:
                    ModSettingCenter.ShowBuildingName = false;
                    break;
                case 1:
                    ModSettingCenter.ShowBuildingName = true;
                    break;
            }

        }
    }
}
