using AndysModsPlugin.utils;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AndysModsPlugin.mods.OptimizeMySellsPatch
{
    [HarmonyPatch(typeof(Terminal))]
    internal class OptimizeMySellsPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch("ParsePlayerSentence")]
        private static void ParsePlayerText(ref Terminal __instance, ref TerminalNode __result)
        {
            string text = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
            ParseOptimizeMySells(text, ref __result);
        }

        internal static void ParseOptimizeMySells(string fullText, ref TerminalNode node)
        {

            string[] textArray = fullText.ToLower().Split();
            string firstWord = textArray[0];
            string secondWord = textArray.Length > 1 ? textArray[1] : "";
            if (firstWord.Contains("sell"))
            {
                if (!ModManager.ModManager.OptimalSells.enabled.Value)
                {
                    node = CreateTerminalNode("Optimal Sells mod is disabled. Enable it via command chat \"/sell\".\n\n");
                    return;
                }
                if (Object.FindObjectOfType<DepositItemsDesk>() == null)
                {
                    node = CreateTerminalNode("You have to be on the Company planet in order to sell.\n\n");
                    return;
                }
                switch (secondWord)
                {
                    case "-all":
                        {
                            node = SellScrap(true);
                            return;
                        }
                    default:
                        {
                            node = SellScrap(false);
                            return;
                        }
                }
            }
        }

        private static TerminalNode SellScrap(bool isSellingAll)
        {
            // first find all scrap on the ship
            List<GrabbableObject> sellingScrap = (from gameObject in GameObject.Find("/Environment/HangarShip").GetComponentsInChildren<GrabbableObject>()
                                                  where gameObject.name != "ClipboardManual" && 
                                                  gameObject.name != "StickyNoteItem" && 
                                                  gameObject.name != "KeyItem" && 
                                                  !gameObject.isBeingUsed && 
                                                  !gameObject.isHeld && 
                                                  !gameObject.isHeldByEnemy && 
                                                  !gameObject.isHeld && 
                                                  gameObject.itemProperties.isScrap && 
                                                  !gameObject.isPocketed
                                                  select gameObject).ToList();
            AndysModsPlugin.Log.LogInfo($"{sellingScrap[0]}");
            sellingScrap.Sort((firstItem, secondItem) => firstItem.scrapValue <= secondItem.scrapValue ? 1 : 0);

            string[] profitText = StartOfRound.Instance.profitQuotaMonitorText.text.Split('/');
            int.TryParse(profitText[0].Replace("PROFIT QUOTA:", "").Trim().Substring(1), out int currentQuota);
            int.TryParse(profitText[1].Trim().Substring(1), out int neededQuota);
            int scrapSum = 0;
            float paycheck = 0;
            if (isSellingAll)
            {
                scrapSum = sellingScrap.Sum(item => item.scrapValue);
            }
            else if (currentQuota < neededQuota)
            {
                // optimize the list of scrap
                int count = 0;
                sellingScrap.Do(scrap =>
                {
                    // tldr: the array is sorted by scrap value from the lowest to highest,
                    // the gist is to sell the lowest first until we hit the quota
                    if (currentQuota + scrapSum * StartOfRound.Instance.companyBuyingRate < neededQuota)
                    {
                        scrapSum += scrap.scrapValue;
                        count++;
                    }
                });
                sellingScrap = sellingScrap.Take(count).ToList();
            }
            else // when we hit the quota
            {
                return CreateTerminalNode("The quota has been hit! No optimal item selling strategy can be provided. If you want to force sell everything on the ship, call the command with \"-all\" option.\n\n");
            }
            if (sellingScrap.Count == 0)
            {
                return CreateTerminalNode("Optimal item selling strategy has not been found. Check if you hit the quota and/or have items on the ship!\n\n");
            }
            paycheck = scrapSum * StartOfRound.Instance.companyBuyingRate;
            ulong[] soldItemsIds = sellingScrap.Select(scrap => scrap.NetworkObjectId).ToArray();
            ModNetworkHandler.Instance?.SellScrapServerRpc(soldItemsIds);
            return CreateTerminalNode($"{soldItemsIds.Length} items are placed on the desk counter. Total: {scrapSum}, Paycheck: {paycheck} with Company Buying Rate {StartOfRound.Instance.companyBuyingRate * 100}%. Don't forget to ring the bell. Thank you for the hard work!\n\n");
        }

        internal static TerminalNode CreateTerminalNode(string text)
        {
            TerminalNode node = ScriptableObject.CreateInstance<TerminalNode>();
            node.displayText = text;
            node.clearPreviousText = true;
            return node;
        }

    }
}
