using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using DunGen;

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
        // - Max ads per quota: -1 is unlimited
        public static ConfigEntry<int> MaxAdsPerQuota { get; private set; }
        // - Chance for ad on landing, in percent
        public static ConfigEntry<int> ChanceOnLanding { get; private set; }
        // - Chance for ad on landing on the last day, in percent
        public static ConfigEntry<int> ChanceOnLandingLastDay { get; private set; }
        // - Chance for an ad to be scheduled on landing, if the above is skipped, in percent
        public static ConfigEntry<int> ChanceForFirstAd { get; private set; }
        // - Chance for an ad to be scheduled on landing on the last day, if the above is skipped, in percent
        public static ConfigEntry<int> ChanceForFirstAdLastDay { get; private set; }
        // - Chance for an ad to be scheduled after an ad is played, in percent
        public static ConfigEntry<int> ChanceForReset { get; private set; }
        // - Chance for an ad to be scheduled after an ad is played on the last day, in percent
        public static ConfigEntry<int> ChanceForResetLastDay { get; private set; }
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
        // - Chance on death, in percent
        public static ConfigEntry<int> ChanceOnDeath { get; private set; }
        // - Chance on death on the last day, in percent
        public static ConfigEntry<int> ChanceOnDeathLastDay { get; private set; }
        // - Play on hurt: enum (immediately, reroll, nah bro)
        public static ConfigEntry<NextAdAction> PlayOnHurt { get; private set; }
        // - Chance on hurt, in percent
        public static ConfigEntry<int> ChanceOnHurt { get; private set; }
        // - Chance on hurt on the last day, in percent
        public static ConfigEntry<int> ChanceOnHurtLastDay { get; private set; }
        // - Blacklist: list of ad names to never show
        public static ConfigEntry<string> Blacklist { get; private set; }
        public static List<string> BlacklistItems
        {
            get
            {
                if (Blacklist == null || string.IsNullOrEmpty(Blacklist.Value))
                {
                    return [];
                }
                return Blacklist.Value.Split(',').ToList();
            }
        }
        public static ConfigEntry<string> SalesText { get; private set; }
        public static List<Tuple<string, int>> SalesTextList
        {
            get
            {
                if (SalesText == null || string.IsNullOrEmpty(SalesText.Value))
                {
                    return [];
                }
                return SalesText.Value.Split(',')
                    .Select(s => s.Split(':'))
                    .Where(parts => parts.Length == 2 && int.TryParse(parts[1], out _))
                    .Select(parts => new Tuple<string, int>(parts[0], int.Parse(parts[1])))
                    .ToList();
            }
        }


        private ConfigManager(ConfigFile config)
        {
            MaxAdsPerDay = config.Bind(
                "General",
                "Max ads per day",
                -1,
                "Maximum number of ads to show per day. -1 means unlimited."
            );
            MaxAdsPerQuota = config.Bind(
                "General",
                "Max ads per quota",
                -1,
                "Maximum number of ads to show each quota. -1 means unlimited. Vanilla is 1"
            );
            ChanceOnLanding = config.Bind(
                "General",
                "Chance to play ad on landing",
                100,
                "Chance to show an ad as soon as possible upon landing. In percent. Vanilla is 0%."
            );
            ChanceOnLandingLastDay = config.Bind(
                "General",
                "Chance to play ad on landing on last day",
                100,
                "Chance to show an ad as soon as possible upon landing on the last day before quota is due. In percent. Vanilla is 0%."
            );
            ChanceForFirstAd = config.Bind(
                "General",
                "Chance to play at least one ad without other triggers",
                100,
                "Chance for an ad to be scheduled on landing, if the one during landing is skipped. In percent. Vanilla is 33%. (Except on final day when it's 60% but there's no setting for that yet.)"
            );
            ChanceForFirstAdLastDay = config.Bind(
                "General",
                "Chance to play at least one ad without other triggers on last day",
                100,
                "Chance for an ad to be scheduled on landing on the last day before quota is due, if the one during landing is skipped. In percent. Vanilla is 60%."
            );
            ChanceForReset = config.Bind(
                "General",
                "Chance to schedule an ad after one is played",
                100,
                "Chance for an ad to be scheduled on landing, if the one during landing is skipped. In percent. Vanilla is 0%"
            );
            ChanceForResetLastDay = config.Bind(
                "General",
                "Chance to schedule an ad after one is played on last day",
                100,
                "Chance for an ad to be scheduled on landing on the last day before quota is due, if the one during landing is skipped. In percent. Vanilla is 0%"
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
            ChanceOnDeath = config.Bind(
                "General",
                "Chance to play ad on death",
                100,
                "Chance to apply the action above when the player dies. In percent. If you want more control."
            );
            ChanceOnDeathLastDay = config.Bind(
                "General",
                "Chance to play ad on death on last day",
                100,
                "Chance to apply the action above when the player dies on the last day before quota is due. In percent. If you want more control."
            );
            PlayOnHurt = config.Bind(
                "General",
                "Play ad on hurt",
                NextAdAction.RerollNext,
                "What to do with ads when the player is hurt. Options: Immediately, RerollNext, None."
            );
            ChanceOnHurt = config.Bind(
                "General",
                "Chance to play ad on hurt",
                100,
                "Chance to apply the action above when the player is hurt. In percent. If you want more control."
            );
            ChanceOnHurtLastDay = config.Bind(
                "General",
                "Chance to play ad on hurt on last day",
                100,
                "Chance to apply the action above when the player is hurt on the last day before quota is due. In percent. If you want more control."
            );
            Blacklist = config.Bind(
                "General",
                "Blacklist",
                "Bee Suit,Bunny Suit,Green suit,Hazard suit,Pajama suit,Purple Suit",
                "List of item names to never show. Use the exact name as it appears in the game, comma separated."
            );
            SalesText = config.Bind(
                "General",
                "Sales Text",
                "CURES CANCER!:3,NO WAY!:3,LIMITED TIME ONLY!:24,GET YOURS TODAY!:30,AVAILABLE NOW!:40",
                "List of sales text to use for ads. Format: 'Text:Weight', comma separated. Defaults to vanilla values."
            );
        }
    }
}