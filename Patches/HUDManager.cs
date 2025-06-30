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
            string result = "AVAILABLE NOW!";
            int num = new System.Random(StartOfRound.Instance.randomMapSeed).Next(0, 100);
            if (num < 3)
            {
                result = "CURES CANCER!";
            }
            else if (num < 6)
            {
                result = "NO WAY!";
            }
            else if (num < 30)
            {
                result = "LIMITED TIME ONLY!";
            }
            else if (num < 60)
            {
                result = "GET YOURS TODAY!";
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
            if (TimeOfDayPatch.adCount == 0)
            {
                return true; // Use vanilla first
            }
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
            // We grabbed the steeped sale for the first ad (Vanilla) so let's get a different ad.
            // Instead, pick a random item on the list of all unowned furniture or tools.
            List<TerminalNode> list = new List<TerminalNode>();
            for (int j = 0; j < terminal.ShipDecorSelection.Count; j++)
            {
                if (!StartOfRound.Instance.unlockablesList.unlockables[terminal.ShipDecorSelection[j].shipUnlockableID].hasBeenUnlockedByPlayer)
                {
                    list.Add(terminal.ShipDecorSelection[j]);
                }
            }
            string itemName = "";
            string saleText = HUDManagerPatch.ChooseSaleText();
            num3 = UnityEngine.Random.Range(0, terminal.buyableItemsList.Length + list.Count);
            if (num3 >= list.Count)
            {
                Debug.Log("Picking a tool to hawk.");
                num3 -= list.Count;
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
                    Plugin.logger.LogWarning($"Error creating furniture ad model: {e.Message}");
                    Plugin.logger.LogWarning($"{e.StackTrace}");
                    return true; // Try a vanilla ad instead.
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