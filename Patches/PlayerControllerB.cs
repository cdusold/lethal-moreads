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
            var chance = TimeOfDay.Instance.daysUntilDeadline <= 1 ? Configs.ConfigManager.ChanceOnDeathLastDay.Value : Configs.ConfigManager.ChanceOnDeath.Value;
            if (Plugin.debug)
            {
                Plugin.logger.LogInfo($"OnDeath: {chance}% chance.");
            }
            if (chance > 0 && (chance >= 100 || Random.Range(0, 100) < chance))
            {
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo("OnDeath: Trigger successful.");
                }
                if (Configs.ConfigManager.PlayOnDeath.Value == Configs.ConfigManager.NextAdAction.Immediately)
                {
                    TimeOfDayPatch.RollAds();
                }
                else if (Configs.ConfigManager.PlayOnDeath.Value == Configs.ConfigManager.NextAdAction.RerollNext)
                {
                    TimeOfDayPatch.PokeAds();
                }
            }
        }

        [HarmonyPatch("DamagePlayerServerRpc")]
        [HarmonyPostfix]
        private static void OnHurt()
        {
            var chance = TimeOfDay.Instance.daysUntilDeadline <= 1 ? Configs.ConfigManager.ChanceOnHurtLastDay.Value : Configs.ConfigManager.ChanceOnHurt.Value;
            if (Plugin.debug)
            {
                Plugin.logger.LogInfo($"OnHurt: {chance}% chance.");
            }
            if (chance > 0 && (chance >= 100 || Random.Range(0, 100) < chance))
            {
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo("OnHurt: Trigger successful.");
                }
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
}