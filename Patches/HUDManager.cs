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
            TimeOfDay.Instance.hasShownAdThisQuota = true;
        }

    }
}