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

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static bool Update()
        {
            var __timeStartedThisFrame = Traverse.Create(TimeOfDay.Instance).Field("timeStartedThisFrame").GetValue<bool>();
            if (__timeStartedThisFrame)
            {
                TimeOfDay.Instance.hasShownAdThisQuota = false;
                adCount = 0;
                TimeOfDay.Instance.normalizedTimeToShowAd = Configs.ConfigManager.OnLanding.Value ? Configs.ConfigManager.EarliestTimeToShowAd.Value : -1f;
            }
            return true;
        }

        [HarmonyPatch("SetTimeForAdToPlay")]
        [HarmonyPrefix]
        private static bool SetTimeForAdToPlay()
        {
            if (!CanSetAd())
            {
                return false;
            }
            if (TimeOfDay.Instance.normalizedTimeToShowAd == -1f)
            {
                ResetAdTime();
            }
            else
            {
                RerollAdTime();
            }
            return false; // Skip original method
        }

        [HarmonyPatch("GetClientInfo")]
        [HarmonyPostfix]
        private static void GetClientInfoPatch()
        {
            if (TimeOfDay.Instance.hasShownAdThisQuota)
            {
                Plugin.logger.LogInfo("Setting time for next ad and resetting ad shown bool.");
                SetTimeForAdToPlay();
                TimeOfDay.Instance.hasShownAdThisQuota = false;
            }
        }

        [HarmonyPatch("MeetsRequirementsToShowAd")]
        [HarmonyPostfix]
        private static void MeetsRequirementsToShowAdPatch(ref bool __result)
        {
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
                // Plugin.logger.LogInfo($"Instruction {i}: {codes[i].opcode} {codes[i].operand}");
                if (codes[i].opcode == OpCodes.Ldc_I4_1 && codes[i - 1].opcode == OpCodes.Ldfld && codes[i - 1].operand.ToString().Contains("livingPlayers"))
                {
                    codes[i] = new CodeInstruction(OpCodes.Ldc_I4_0); // Change to zero
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
            if (Traverse.Create(TimeOfDay.Instance).Field("checkingIfClientsAreReadyForAd").GetValue<bool>())
            {
                // Plugin.logger.LogInfo("Ad check happened, but no ad was shown.");
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
            if (TimeOfDay.Instance.normalizedTimeToShowAd == -1f)
            {
                Plugin.logger.LogInfo("POIK: NOTICE: Ad time got unset, setting it now.");
                SetTimeForAdToPlay();
            }
            return true;
        }

        [HarmonyPatch("ReceiveInfoFromClientForShowingAdServerRpc")]
        [HarmonyPrefix]
        private static bool ReceiveInfoFromClientForShowingAdServerRpc(ref bool doesntMeetRequirements)
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
            return true;
        }

        public static void AdIncrement()
        {
            adCount++;
            Plugin.logger.LogInfo($"Ad #{adCount} shown.");
            Traverse.Create(TimeOfDay.Instance).Field("adWaitInterval").SetValue(Configs.ConfigManager.MinimumTimeBetweenAds.Value);
            if (CanSetAd())
            {
                ResetAdTime();
            }
        }

        private static void SetGameToAllowAds()
        { // Setup all vanilla ad-related variables
            TimeOfDay.Instance.hasShownAdThisQuota = false;
            ResetAdTime(); // Roll a new ad time
            Traverse.Create(TimeOfDay.Instance).Field("checkingIfClientsAreReadyForAd").SetValue(false);
        }

        private static float RollAdTime()
        {
            float adTime = UnityEngine.Random.Range(Math.Max(TimeOfDay.Instance.normalizedTimeOfDay, Configs.ConfigManager.EarliestTimeToShowAd.Value), Configs.ConfigManager.LatestTimeToShowAd.Value);
            return adTime;
        }

        private static void ResetAdTime()
        {
            TimeOfDay.Instance.hasShownAdThisQuota = false;
            TimeOfDay.Instance.normalizedTimeToShowAd = RollAdTime();
            if (Plugin.debug)
            {
                Plugin.logger.LogInfo($"Ad time reset to {TimeOfDay.Instance.normalizedTimeToShowAd} (normalized time of day).");
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