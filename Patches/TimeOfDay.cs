using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection.Emit;
using UnityEngine;
namespace MoreAds.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {

        public static int adCount { get; private set; } = 0;
        public static int quotaAdCount { get; private set; } = 0;

        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPrefix]
        private static bool SetNewProfitQuota()
        {
            Plugin.logger.LogInfo("Quota ad count reset.");
            quotaAdCount = 0;
            return true;
        }

        [HarmonyPatch("SetTimeForAdToPlay")]
        [HarmonyPrefix]
        private static bool SetTimeForAdToPlay()
        {
            if (Plugin.debug)
            {
                Plugin.logger.LogWarning("POIK: SetTimeForAdToPlay was called.");
            }
            SetGameToAllowAds();
            adCount = 0;
            if (CanSetAd())
            {
                var chanceOnLanding = TimeOfDay.Instance.daysUntilDeadline <= 1 ? Configs.ConfigManager.ChanceOnLandingLastDay.Value : Configs.ConfigManager.ChanceOnLanding.Value;
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo($"Chance on landing: {chanceOnLanding}% chance.");
                }
                var chanceForFirstAd = TimeOfDay.Instance.daysUntilDeadline <= 1 ? Configs.ConfigManager.ChanceForFirstAdLastDay.Value : Configs.ConfigManager.ChanceForFirstAd.Value;
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo($"Chance for first ad: {chanceForFirstAd}% chance.");
                }
                if (chanceOnLanding > 0 && (chanceOnLanding >= 100 || UnityEngine.Random.Range(0, 100) < chanceOnLanding))
                {
                    if (Plugin.debug)
                    {
                        Plugin.logger.LogInfo("Chance on landing: Trigger successful.");
                    }
                    TimeOfDay.Instance.normalizedTimeToShowAd = Configs.ConfigManager.EarliestTimeToShowAd.Value;
                }
                else if (chanceForFirstAd > 0 && (chanceForFirstAd >= 100 || UnityEngine.Random.Range(0, 100) < chanceForFirstAd))
                {
                    if (Plugin.debug)
                    {
                        Plugin.logger.LogInfo("Chance for first ad: Trigger successful.");
                    }
                    TimeOfDay.Instance.normalizedTimeToShowAd = RollAdTime();
                }
                else
                {
                    if (Plugin.debug)
                    {
                        Plugin.logger.LogInfo("No ad scheduled on landing.");
                    }
                    TimeOfDay.Instance.normalizedTimeToShowAd = -1f;
                }
            }
            else
            {
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo("I believe we've reached max ad-itude.");
                }
                TimeOfDay.Instance.normalizedTimeToShowAd = -1f;
            }
            Traverse.Create(TimeOfDay.Instance).Field("adWaitInterval").SetValue(0f);
            return false;
        }

        [HarmonyPatch("GetClientInfo")]
        [HarmonyPostfix]
        private static void GetClientInfoPost()
        {
            SetGameToAllowAds();
        }

        [HarmonyPatch("MeetsRequirementsToShowAd")]
        [HarmonyPostfix]
        private static void MeetsRequirementsToShowAdPatch(ref bool __result)
        { // A little hacky.
            __result = true;
        }

        [HarmonyPatch("DisplayAdAtScheduledTime")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DisplayAdAtScheduledTimeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Changing check for more than one living player to more than zero living players
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 1; i < codes.Count; i++)
            {
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo($"Instruction {i}: {codes[i].opcode} {codes[i].operand}");
                }
                if (codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i - 1].opcode == OpCodes.Ldfld && codes[i - 1].operand.ToString().Contains("livingPlayers"))
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldc_I4_0); // Change to zero
                    if (Plugin.debug)
                    {
                        Plugin.logger.LogInfo($"Changed instruction {i} to Ldc_I4_0.");
                    }
                    Plugin.logger.LogInfo("Patched ads to display even if only one player is alive.");
                    break;
                }
            }
            return codes;
        }

        [HarmonyPatch("DisplayAdAtScheduledTime")]
        [HarmonyPrefix]
        private static bool DisplayAdAtScheduledTime()
        {
            var __adWaitInterval = Traverse.Create(TimeOfDay.Instance).Field("adWaitInterval").GetValue<float>();
            if (__adWaitInterval > 0f)
            {
                Traverse.Create(TimeOfDay.Instance).Field("adWaitInterval").SetValue(__adWaitInterval - Time.deltaTime);
                return false; // Skip original method
            }
            if (!CanShowAd())
            {
                return false; // Skip original method
            }
            // Nuclear option. We don't care about vanilla checks, we have our own.
            SetGameToAllowAds();
            if (false) // TODO: REMOVE
            {
                if (Traverse.Create(TimeOfDay.Instance).Field("checkingIfClientsAreReadyForAd").GetValue<bool>())
                {
                    if (Plugin.debug)
                    {
                        Plugin.logger.LogInfo("Ad check happened, but no ad was shown.");
                    }
                    Traverse.Create(TimeOfDay.Instance).Field("adWaitInterval").SetValue(15f);
                    Traverse.Create(TimeOfDay.Instance).Field("checkingIfClientsAreReadyForAd").SetValue(false);
                    ResetAdTime();
                    TimeOfDay.Instance.hasShownAdThisQuota = false;
                }
                else if (TimeOfDay.Instance.hasShownAdThisQuota)
                {
                    Plugin.logger.LogInfo("POIK: NOTICE: Ad shown bool being reset.");
                    TimeOfDay.Instance.hasShownAdThisQuota = false;
                }
            }
            return true;
        }

        [HarmonyPatch("ReceiveInfoFromClientForShowingAdServerRpc")]
        [HarmonyPrefix]
        private static bool ReceiveInfoFromClientForShowingAdServerRpcPre(ref bool doesntMeetRequirements)
        {
            if (doesntMeetRequirements)
            {
                Plugin.logger.LogInfo($"Doesn't meet requirements: {doesntMeetRequirements}");
                doesntMeetRequirements = false; // Force it to always meet requirements
            }
            return true;
        }

        private static bool CanSetAd()
        {
            if (TimeOfDay.Instance.normalizedTimeOfDay > Configs.ConfigManager.LatestTimeToShowAd.Value)
            {
                return false;
            }
            if (Configs.ConfigManager.MaxAdsPerDay.Value != -1 && adCount >= Configs.ConfigManager.MaxAdsPerDay.Value)
            {
                return false;
            }
            if (Configs.ConfigManager.MaxAdsPerQuota.Value != -1 && quotaAdCount >= Configs.ConfigManager.MaxAdsPerQuota.Value)
            {
                return false;
            }
            if (StartOfRound.Instance.livingPlayers <= 0)
            {
                return false;
            }
            return true;
        }

        private static bool CanShowAd()
        {
            if (TimeOfDay.Instance.normalizedTimeOfDay < Configs.ConfigManager.EarliestTimeToShowAd.Value)
            {
                return false;
            }
            // This is actually a vanilla restriction, but we could override it...
            if (TimeOfDay.Instance.normalizedTimeOfDay > 0.9f)
            {
                return false;
            }
            if (Configs.ConfigManager.MaxAdsPerDay.Value != -1 && adCount >= Configs.ConfigManager.MaxAdsPerDay.Value)
            {
                return false;
            }
            if (Configs.ConfigManager.MaxAdsPerQuota.Value != -1 && quotaAdCount >= Configs.ConfigManager.MaxAdsPerQuota.Value)
            {
                return false;
            }
            return true;
        }

        public static void AdIncrement()
        {
            adCount++;
            Plugin.logger.LogInfo($"Ad #{adCount} shown.");
            quotaAdCount++;
            Plugin.logger.LogInfo($"This quota has had {quotaAdCount} ads.");
            Traverse.Create(TimeOfDay.Instance).Field("adWaitInterval").SetValue(Configs.ConfigManager.MinimumTimeBetweenAds.Value);
            SetGameToAllowAds();
            if (CanSetAd())
            {
                ResetAdTime();
            }
        }

        private static void SetGameToAllowAds()
        { // Setup all vanilla ad-related variables
            TimeOfDay.Instance.hasShownAdThisQuota = false;
            Traverse.Create(TimeOfDay.Instance).Field("checkingIfClientsAreReadyForAd").SetValue(false);
        }

        private static float RollAdTime()
        {
            float adTime = UnityEngine.Random.Range(Math.Max(TimeOfDay.Instance.normalizedTimeOfDay, Configs.ConfigManager.EarliestTimeToShowAd.Value), Configs.ConfigManager.LatestTimeToShowAd.Value);
            return adTime;
        }

        private static void ResetAdTime()
        {
            SetGameToAllowAds();
            var chance = TimeOfDay.Instance.daysUntilDeadline <= 1 ? Configs.ConfigManager.ChanceForFirstAdLastDay.Value : Configs.ConfigManager.ChanceForFirstAd.Value;
            if (Plugin.debug)
            {
                Plugin.logger.LogInfo($"Resetting ad time with {chance}% chance.");
            }
            if (chance > 0 && (chance >= 100 || UnityEngine.Random.Range(0, 100) < chance))
            {
                TimeOfDay.Instance.normalizedTimeToShowAd = RollAdTime();
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo($"Ad time reset to {TimeOfDay.Instance.normalizedTimeToShowAd} (normalized time of day).");
                }
            }
            else
            {
                TimeOfDay.Instance.normalizedTimeToShowAd = -1f;
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo("Ad time reset to -1 (no ad scheduled).");
                }
            }
        }

        private static void RerollAdTime()
        {
            TimeOfDay.Instance.normalizedTimeToShowAd = Math.Min(
                TimeOfDay.Instance.normalizedTimeToShowAd,
                RollAdTime()
            );
            if (Plugin.debug)
            {
                Plugin.logger.LogInfo($"Ad time rerolled to {TimeOfDay.Instance.normalizedTimeToShowAd} (normalized time of day).");
            }
        }

        public static void RollAds()
        {
            if (CanSetAd())
            {
                Plugin.logger.LogInfo("Rolling ads...");
                TimeOfDay.Instance.normalizedTimeToShowAd = TimeOfDay.Instance.normalizedTimeOfDay;
            }
        }

        public static void PokeAds()
        {
            if (CanSetAd())
            {
                Plugin.logger.LogInfo("Poking ads...");
                RerollAdTime();
            }
        }

    }

}