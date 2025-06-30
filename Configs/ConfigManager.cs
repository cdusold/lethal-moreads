using BepInEx.Configuration;

namespace MoreAds.Configs
{
    internal class ConfigManager
    {
        public static ConfigManager Instance { get; private set; }

        public static void Init(ConfigFile config)
        {
            if (Instance == null)
            {
                Instance = new ConfigManager(config);
            }
        }

        // TODO: Configs:
        // - Play on fear up: enum (immediately, reroll, nah bro)
        // - Delay after fear up/death/hurt: default to 15 seconds (vanilla)

        // Configs:
        // - Max ads per day: -1 is unlimited
        public static ConfigEntry<int> MaxAdsPerDay { get; private set; }
        // - Ad on landing
        public static ConfigEntry<bool> OnLanding { get; private set; }
        // - Earliest time to show ad: default to 0.0f or 0%
        public static ConfigEntry<float> EarliestTimeToShowAd { get; private set; }
        // - Latest time to show ad: default to 0.9f or 90% (Vanilla is 0.7f or 70%)
        public static ConfigEntry<float> LatestTimeToShowAd { get; private set; }
        // - Minimum time between ads in seconds: default is 60 seconds
        public static ConfigEntry<float> MinimumTimeBetweenAds { get; private set; }
        public enum NextAdAction
        {
            Immediately,
            RerollNext,
            None
        }
        // - Play on death: enum (immediately, reroll, nah bro)
        public static ConfigEntry<NextAdAction> PlayOnDeath { get; private set; }
        // - Play on hurt: enum (immediately, reroll, nah bro)
        public static ConfigEntry<NextAdAction> PlayOnHurt { get; private set; }


        private ConfigManager(ConfigFile config)
        {
            MaxAdsPerDay = config.Bind(
                "General",
                "Max ads per day",
                -1,
                "Maximum number of ads to show per day. -1 means unlimited."
            );
            OnLanding = config.Bind(
                "General",
                "Play ad on landing",
                true,
                "Shows an ad as soon as possible upon landing."
            );
            EarliestTimeToShowAd = config.Bind(
                "General",
                "Earliest time to show ad",
                0.0f,
                "The earliest time in the day to show an ad, in normalized time (0.0f to 1.0f). Vanilla is 0.0f."
            );
            LatestTimeToShowAd = config.Bind(
                "General",
                "Latest time to show ad",
                0.9f,
                "The latest time in the day to show an ad, in normalized time (0.0f to 1.0f). Vanilla is 0.7f."
            );
            MinimumTimeBetweenAds = config.Bind(
                "General",
                "Minimum time between ads (seconds)",
                60f,
                "The minimum time in seconds between ads."
            );
            PlayOnDeath = config.Bind(
                "General",
                "Play ad on death",
                NextAdAction.RerollNext,
                "What to do with ads when the player dies. Options: Immediately, RerollNext, None."
            );
            PlayOnHurt = config.Bind(
                "General",
                "Play ad on hurt",
                NextAdAction.RerollNext,
                "What to do with ads when the player is hurt. Options: Immediately, RerollNext, None."
            );
        }
    }
}