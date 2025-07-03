using BepInEx;
using LethalConfig.ConfigItems;
using BepInEx.Configuration;
using LethalConfig.ConfigItems.Options;

namespace MoreAds.Configs
{
    [BepInDependency("ainavt.lc.lethalconfig")]
    internal class LethalConfigManager
    {
        public static LethalConfigManager Instance { get; private set; }

        public static void Init(ConfigFile config)
        {
            if (Instance == null)
            {
                Instance = new LethalConfigManager(config);
            }
        }

        private LethalConfigManager(ConfigFile config)
        {
            LethalConfig.LethalConfigManager.SetModDescription("Wait... This isn't an adblocker?");

            LethalConfig.LethalConfigManager.AddConfigItem(new IntSliderConfigItem(ConfigManager.MaxAdsPerDay, new IntSliderOptions
            {
                Min = -1,
                Max = 100,
                RequiresRestart = false
            }));
            LethalConfig.LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(ConfigManager.OnLanding, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(ConfigManager.EarliestTimeToShowAd, new FloatSliderOptions
            {
                Min = 0.0f,
                Max = 1.0f,
                RequiresRestart = false
            }));
            LethalConfig.LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(ConfigManager.LatestTimeToShowAd, new FloatSliderOptions
            {
                Min = 0.0f,
                Max = 1.0f,
                RequiresRestart = false
            }));
            LethalConfig.LethalConfigManager.AddConfigItem(new FloatSliderConfigItem(ConfigManager.MinimumTimeBetweenAds, new FloatSliderOptions
            {
                Min = 0.0f,
                Max = 300.0f,
                RequiresRestart = false
            }));
            LethalConfig.LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<ConfigManager.NextAdAction>(ConfigManager.PlayOnDeath, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<ConfigManager.NextAdAction>(ConfigManager.PlayOnHurt, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(ConfigManager.Blacklist, false));
        }
    }
}