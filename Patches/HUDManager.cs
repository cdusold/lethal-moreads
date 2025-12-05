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
            // The original default, just because.
            string result = "AVAILABLE NOW!";
            // This kinda syncs the clients?
            int totalWeight = 0;
            for (int i = 0; i < Configs.ConfigManager.SalesTextList.Count; i++)
            {
                totalWeight += Configs.ConfigManager.SalesTextList[i].Item2;
            }
            int num = new System.Random(StartOfRound.Instance.randomMapSeed + TimeOfDayPatch.adCount).Next(0, totalWeight);
            for (int i = 0; i < Configs.ConfigManager.SalesTextList.Count; i++)
            {
                if (num < Configs.ConfigManager.SalesTextList[i].Item2)
                {
                    result = Configs.ConfigManager.SalesTextList[i].Item1;
                    break;
                }
                else
                {
                    num -= Configs.ConfigManager.SalesTextList[i].Item2;
                }
            }
            result = result.Replace("{me}", GameNetworkManager.Instance.localPlayerController.playerUsername);
            result = result.Replace("&comma;", ",");
            result = result.Replace("&colon;", ":");
            if (result.Contains("{player}"))
            {
                var player = StartOfRound.Instance.allPlayerScripts[new System.Random(StartOfRound.Instance.randomMapSeed + TimeOfDayPatch.adCount + 1).Next(0, StartOfRound.Instance.connectedPlayersAmount + 1)].playerUsername;
                result = result.Replace("{player}", player);
            }
            if (result.Contains("{planet}"))
            {
                Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                var planet = terminal.moonsCatalogueList[new System.Random(StartOfRound.Instance.randomMapSeed + TimeOfDayPatch.adCount + 2).Next(0, terminal.moonsCatalogueList.Length)].PlanetName;
                result = result.Replace("{planet}", planet);
            }
            if (result.Contains("{here}"))
            {
                var here = StartOfRound.Instance.currentLevel.PlanetName;
                result = result.Replace("{here}", here);
            }
            if (Plugin.debug)
            {
                Plugin.logger.LogInfo($"Sale text chosen: {result}");
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
            string topText = "";
            string saleText = ChooseSaleText();
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
                if (num2 <= 70)
                {
                    saleText = $"{100 - num2}% OFF!";
                }
                topText = item.itemName;
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
                    topText = StartOfRound.Instance.unlockablesList.unlockables[list[num3].shipUnlockableID].unlockableName;
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
            if (saleText.Contains("{product}"))
            {
                // At this point, the item name is in the topText variable.
                saleText = saleText.Replace("{product}", topText);
            }
            if (saleText.Contains("/"))
            {
                var splits = saleText.Split('/');
                topText = splits[0].Trim();
                saleText = splits[1].Trim();
            }
            HUDManager.Instance.BeginDisplayAd(topText, saleText);
            // HUDManager.Instance.BeginDisplayAd("Never gonna", "give you up");
            return false; // Skip vanilla ad, we already did one.
        }

        [HarmonyPatch("CreateToolAdModelAndDisplayAdClientRpc")]
        [HarmonyPrefix]
        private static bool CreateToolAdModelAndDisplayAdClientRpcReplacement(int itemIndex)
        {
            NetworkManager networkManager = HUDManager.Instance.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening || networkManager.IsHost || HUDManager.Instance.IsServer)
            {
                return true; // Let the base game handle RPC sending.
            }

            var __rpc_exec_stage = (int)Traverse.Create(HUDManager.Instance).Field("__rpc_exec_stage").GetValue();
            if (Plugin.debug)
            {
                Plugin.logger.LogInfo($"__rpc_exec_stage: {__rpc_exec_stage}");
            }
            // // In v72- we were looking for the Client enum, 2.
            // var stage_Client = 2;
            // In v73+ we look for stage_Execute, 1.
            int stage_Execute = 1;
            if (Plugin.debug)
            {
                try
                { // This ain't it.
                    Type ___RpcExecStage = Traverse.Create(HUDManager.Instance).Field("__RpcExecStage").GetValue<Type>();
                    stage_Execute = (int)___RpcExecStage.GetField("Execute").GetValue(___RpcExecStage);
                    if (Plugin.debug)
                    {
                        Plugin.logger.LogInfo($"___RpcExecStage.Execute: {stage_Execute}");
                    }
                }
                catch (Exception)
                {
                }

            }
            if (__rpc_exec_stage != stage_Execute && (networkManager.IsServer || networkManager.IsHost))
            {
                return true; // Let the base game handle RPC sending.
            }

            if (__rpc_exec_stage == stage_Execute && (networkManager.IsClient || networkManager.IsHost) && !HUDManager.Instance.IsServer)
            {
                for (int num = HUDManager.Instance.advertItemParent.transform.childCount - 1; num >= 0; num--)
                {
                    UnityEngine.Object.Destroy(HUDManager.Instance.advertItemParent.transform.GetChild(num).gameObject);
                }

                HUDManager.Instance.advertItem = null;
                Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
                Item item = terminal.buyableItemsList[itemIndex];
                int saleValue = terminal.itemSalesPercentages[itemIndex];
                if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                {
                    HUDManager.Instance.CreateToolAdModel(saleValue, item);
                }

                var saleText = ChooseSaleText();
                string topText = item.itemName;
                if (saleValue <= 70)
                {
                    saleText = $"{100 - saleValue}% OFF!";
                }
                if (saleText.Contains("{product}"))
                {
                    saleText = saleText.Replace("{product}", item.itemName);
                }
                if (saleText.Contains("/"))
                {
                    var splits = saleText.Split('/');
                    topText = splits[0].Trim();
                    saleText = splits[1].Trim();
                }
                HUDManager.Instance.BeginDisplayAd(topText, saleText);
            }
            return false; // Skip original method
        }

        [HarmonyPatch("displayAd")]
        [HarmonyPostfix]
        private static void displayAdPost()
        {
            Plugin.logger.LogInfo("Ad done, reset stuff.");
            // ResetAdTime in AdIncrement also resets hasShownAdThisQuota
            TimeOfDayPatch.AdIncrement();
        }

    }
}