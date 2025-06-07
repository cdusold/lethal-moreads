using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
namespace MoreAds.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        private static void Start()
        {
            Plugin.logger.LogInfo("Ad not shown yet, game just started.");
            TimeOfDay.Instance.hasShownAdThisQuota = false;
        }

        [HarmonyPatch("SetTimeForAdToPlay")]
        [HarmonyPostfix]
        private static void SetTimeForAdToPlay()
        {
            Plugin.logger.LogInfo("Setting time to play to 0.04f.");
            TimeOfDay.Instance.normalizedTimeToShowAd = 0.04f;
        }

        [HarmonyPatch("GetClientInfo")]
        [HarmonyPostfix]
        private static void GetClientInfoPatch()
        {
            Plugin.logger.LogInfo("Resetting client info time to show ad to 0.04f and resetting ad shown bool.");
            TimeOfDay.Instance.normalizedTimeToShowAd = 0.04f;
            TimeOfDay.Instance.hasShownAdThisQuota = false;
        }

        [HarmonyPatch("MeetsRequirementsToShowAd")]
        [HarmonyPostfix]
        private static void MeetsRequirementsToShowAdPatch(ref bool __result)
        {
            Plugin.logger.LogInfo("Checking ad requirements.");
            Plugin.logger.LogInfo("Should be good to go.");

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
            if (TimeOfDay.Instance.normalizedTimeToShowAd == -1f)
            {
                TimeOfDay.Instance.normalizedTimeToShowAd = 0.04f;
            }
            if (TimeOfDay.Instance.hasShownAdThisQuota)
            {
                TimeOfDay.Instance.hasShownAdThisQuota = false;
            }
            var __checkingIfClientsAreReadyForAd = Traverse.Create(TimeOfDay.Instance).Field("checkingIfClientsAreReadyForAd").GetValue<bool>();
            if (__checkingIfClientsAreReadyForAd)
            {
                Traverse.Create(TimeOfDay.Instance).Field("adWaitInterval").SetValue(30f);
                Traverse.Create(TimeOfDay.Instance).Field("checkingIfClientsAreReadyForAd").SetValue(false);
            }
            return true;
        }

        [HarmonyPatch("ReceiveInfoFromClientForShowingAdServerRpc")]
        [HarmonyPrefix]
        private static bool ReceiveInfoFromClientForShowingAdServerRpc(ref bool doesntMeetRequirements)
        {
            Plugin.logger.LogInfo("Received info from client for showing ad server RPC.");
            Plugin.logger.LogInfo($"Doesn't meet requirements: {doesntMeetRequirements}");
            doesntMeetRequirements = false; // Force it to always meet requirements
            return true;
        }

    }
    
}