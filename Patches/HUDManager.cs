using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
namespace MoreAds.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        private static string ChooseSaleText()
        {
            // TODO: Make this configurable
            // This is just the code from the original game (v72)
            // It'll make more sense to make this configurable if we can sync text with clients.

            // The original default, just because.
            string result = "AVAILABLE NOW!";
            // This kinda syncs the clients?
            int num = new System.Random(StartOfRound.Instance.randomMapSeed).Next(0, 100);
            for (int i = 0; i < Configs.ConfigManager.SalesTextList.Count; i++)
            {
                if (num < Configs.ConfigManager.SalesTextList[i].Item2)
                {
                    result = Configs.ConfigManager.SalesTextList[i].Item1;
                    break;
                }
            }

            return result;
        }

        [HarmonyPatch("ChooseAdItem")]
        [HarmonyPrefix]
        private static bool ChooseAdItemPre()
        {
            Plugin.logger.LogInfo("Ad starting, setting check timer.");
            // This will be reset by AdIncrement
            Traverse.Create(TimeOfDay.Instance).Field("adWaitInterval").SetValue(600f);
            if (!HUDManager.Instance.IsServer)
            {
                return true;
            }

            for (int num = HUDManager.Instance.advertItemParent.transform.childCount - 1; num >= 0; num--)
            {
                UnityEngine.Object.Destroy(HUDManager.Instance.advertItemParent.transform.GetChild(num).gameObject);
            }

            HUDManager.Instance.advertItem = null;
            int num2 = 100;
            int num3 = -1;
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            // Pick a random item on the list of all unowned furniture or tools not in the blacklist.
            // Let's weight the items by their sale percentage, showing the steepest discount most often.
            List<TerminalNode> list = new List<TerminalNode>();
            if (Plugin.debug)
            {
                Plugin.logger.LogInfo("Ad possible items:");
            }
            for (int j = 0; j < terminal.ShipDecorSelection.Count; j++)
            {
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo($"- {StartOfRound.Instance.unlockablesList.unlockables[terminal.ShipDecorSelection[j].shipUnlockableID].unlockableName}");
                }
                if (!StartOfRound.Instance.unlockablesList.unlockables[terminal.ShipDecorSelection[j].shipUnlockableID].hasBeenUnlockedByPlayer)
                {
                    // Skip if in blacklist
                    if (Configs.ConfigManager.BlacklistItems.Contains(StartOfRound.Instance.unlockablesList.unlockables[terminal.ShipDecorSelection[j].shipUnlockableID].unlockableName))
                    {
                        continue;
                    }
                    list.Add(terminal.ShipDecorSelection[j]);
                }
            }
            List<int> buyableItemsList = new List<int>();
            for (int j = 0; j < terminal.buyableItemsList.Length; j++)
            {
                if (Plugin.debug)
                {
                    Plugin.logger.LogInfo($"- {terminal.buyableItemsList[j].itemName}");
                }
                // Skip if in blacklist
                if (Configs.ConfigManager.BlacklistItems.Contains(terminal.buyableItemsList[j].itemName))
                {
                    continue;
                }
                buyableItemsList.Add(j);
                for (int k = 0; k < (100-terminal.itemSalesPercentages[j])/10; k++)
                {
                    buyableItemsList.Add(j);
                }
            }
            string itemName = "";
            string saleText = HUDManagerPatch.ChooseSaleText();
            num3 = UnityEngine.Random.Range(0, terminal.buyableItemsList.Length + list.Count);
            if (num3 >= list.Count)
            {
                Debug.Log("Picking a tool to hawk.");
                num3 -= list.Count;
                // That's the index in the reduplicated list, get the original index.
                num3 = buyableItemsList[num3];
                num2 = terminal.itemSalesPercentages[num3];
                Item item = terminal.buyableItemsList[num3];
                HUDManager.Instance.CreateToolAdModelAndDisplayAdClientRpc(num2, num3);
                HUDManager.Instance.CreateToolAdModel(num2, item);
                saleText = $"{100 - num2}% OFF!";
                itemName = item.itemName;
            }
            else
            {
                Debug.Log("Putting furniture in the ad");
                int num5 = -1;
                for (int k = 0; k < terminal.ShipDecorSelection.Count; k++)
                {
                    if (terminal.ShipDecorSelection[k].shipUnlockableID == list[num3].shipUnlockableID)
                    {
                        num5 = k;
                        break;
                    }
                }
                try
                {
                    HUDManager.Instance.CreateFurnitureAdModelAndDisplayAdClientRpc(num5);
                    HUDManager.Instance.CreateFurnitureAdModel(StartOfRound.Instance.unlockablesList.unlockables[list[num3].shipUnlockableID]);
                    itemName = StartOfRound.Instance.unlockablesList.unlockables[list[num3].shipUnlockableID].unlockableName;
                }
                catch (ArgumentException e)
                {
                    // Hopefully this can recover from the null reference exception that happens when a suit ad tries to play.
                    // If not we need to add more to the blacklist.
                    Plugin.logger.LogWarning($"Error creating furniture ad model: {e.Message}");
                    Plugin.logger.LogWarning($"{e.StackTrace}");
                    return false; // Vanilla ads are broken, never play one.
                }
            }

            HUDManager.Instance.BeginDisplayAd(itemName, saleText);
            // HUDManager.Instance.BeginDisplayAd("Never gonna", "give you up");
            return false; // Skip vanilla ad, we already did one.
        }

        [HarmonyPatch("displayAd")]
        [HarmonyPostfix]
        private static void displayAdPost()
        {
            Plugin.logger.LogInfo("Ad done, reset stuff.");
            // ResetAdTime in AdIncrement also resets hasShownAdThisQuota
            TimeOfDayPatch.AdIncrement();
        }

        [HarmonyPatch("CreateToolAdModelAndDisplayAdClientRpc")]
        [HarmonyPrefix]
        private static bool CreateToolAdModelAndDisplayAdClientRpcPre(ref int steepestSale)
        {
            if ((HUDManager.Instance.IsClient || HUDManager.Instance.IsHost) && !HUDManager.Instance.IsServer)
            {
                // At least in v72, the sale is inverted for some reason.
                steepestSale = 100 - steepestSale;
            }
            return true;
        }

    }
}