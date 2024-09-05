using AndysModsPlugin.utils;
using HarmonyLib;
using UnityEngine;

namespace AndysModsPlugin.mods.RareBonk
{
    [HarmonyPatch(typeof(Shovel))]
    [HarmonyWrapSafe]
    internal static class RareBonkPatch
    {
        public const int BonkShovelForce = 100; // should 100 be enough to BONK?
        public const int OriginalShovelForce = 1;

        [HarmonyPatch("HitShovel")]
        [HarmonyPrefix]
        static void PatchBonkShovel(Shovel __instance)
        {
            if (!ModManager.ModManager.RareBonk.enabled.Value)
            {
                return;
            }
            int bonkChance = Random.Range(1, 100);
            if (__instance != null && bonkChance <= 10)
            {
                AndysModsPlugin.Log.LogInfo($"Rare Bonk: BONK by {__instance.playerHeldBy?.playerUsername}!");
                __instance.shovelHitForce = BonkShovelForce;
                ModNetworkHandler.Instance?.PlayBonkServerRpc(__instance.NetworkObjectId);
            }
        }

        [HarmonyPatch("HitShovel")]
        [HarmonyPostfix]
        static void RemovePatchShovel(Shovel __instance)
        {
            if (!ModManager.ModManager.RareBonk.enabled.Value)
            {
                return;
            }
            if (__instance != null && __instance.shovelHitForce != OriginalShovelForce)
            {
                __instance.shovelHitForce = OriginalShovelForce;
                AndysModsPlugin.Log.LogInfo($"Rare Bonk: restored original shovelHitForce {__instance?.shovelHitForce}.");
            }
        }

    }

}