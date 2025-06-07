using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace MoreAds.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {

        [HarmonyPatch("displayAd")]
        [HarmonyPostfix]
        private static void displayAd()
        {
            Plugin.logger.LogInfo("Ad done, reset boolean.");
            TimeOfDay.Instance.hasShownAdThisQuota = false;
        }

    }
}