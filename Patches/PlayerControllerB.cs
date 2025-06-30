using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace MoreAds.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        [HarmonyPatch("KillPlayerServerRpc")]
        [HarmonyPostfix]
        private static void OnDeath()
        {
            if (Configs.ConfigManager.PlayOnDeath.Value == Configs.ConfigManager.NextAdAction.Immediately)
            {
                TimeOfDayPatch.RollAds();
            }
            else if (Configs.ConfigManager.PlayOnDeath.Value == Configs.ConfigManager.NextAdAction.RerollNext)
            {
                TimeOfDayPatch.PokeAds();
            }
        }

        [HarmonyPatch("DamagePlayerServerRpc")]
        [HarmonyPostfix]
        private static void OnHurt()
        {
            if (Configs.ConfigManager.PlayOnHurt.Value == Configs.ConfigManager.NextAdAction.Immediately)
            {
                TimeOfDayPatch.RollAds();
            }
            else if (Configs.ConfigManager.PlayOnHurt.Value == Configs.ConfigManager.NextAdAction.RerollNext)
            {
                TimeOfDayPatch.PokeAds();
            }
        }
    }
}