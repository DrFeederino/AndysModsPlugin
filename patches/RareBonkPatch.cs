using HarmonyLib;
using UnityEngine;

namespace AndysModsPlugin.patches
{
    [HarmonyPatch(typeof(Shovel))]
    [HarmonyWrapSafe]
    internal static class RareBonkPatch
    {
        static AudioClip[] originalSfx;
        static AssetBundle bonkBundle = AssetBundle.LoadFromFile($"{AndysModsPlugin.PluginPath}bonk");
        static AudioClip[] bonkSfx = bonkBundle.LoadAssetWithSubAssets<AudioClip>("bonk.mp3");
        static System.Random random = new System.Random();

        [HarmonyPatch("HitShovel")]
        [HarmonyPrefix]
        static void patchBonkShovel(Shovel __instance)
        {
            int bonkChance = random.Next(1, 100);
            if (__instance != null && bonkChance <= 10)
            {
                AndysModsPlugin.Log.LogInfo("Rare Bonk: BONK!");
                __instance.shovelHitForce = 100; // should 100 be enough to BONK?
                replaceHitAudioToBonk(__instance);
            }
        }

        [HarmonyPatch("HitShovel")]
        [HarmonyPostfix]
        static void removePatchShovel(Shovel __instance)
        {
            if (__instance != null)
            {
                __instance.shovelHitForce = 1;
                restoreHitAudio(__instance);
            }
        }
        static void replaceHitAudioToBonk(Shovel shovel)
        {
            if (shovel == null)
            {
                AndysModsPlugin.Log.LogInfo("Rare Bonk: No Shovel? Who calls us?");
                return;
            }
            if (bonkSfx == null || bonkSfx.Length == 0)
            {
                AndysModsPlugin.Log.LogInfo("Rare Bonk: No BONK SFX has been found. Not changing shovel's sounds.");
                return;
            }
            originalSfx = shovel.hitSFX;
            shovel.hitSFX = bonkSfx;
        }

        static void restoreHitAudio(Shovel shovel)
        {
            if (shovel == null)
            {
                AndysModsPlugin.Log.LogInfo("Rare Bonk: No Shovel? Who calls us?");
                return;
            }
            if (originalSfx == null || originalSfx.Length == 0)
            {
                AndysModsPlugin.Log.LogInfo("Rare Bonk: No original SFX for shovel found.");
                return;
            }
            AndysModsPlugin.Log.LogInfo("Rare Bonk: Restored shovel's original SFX.");
            shovel.hitSFX = originalSfx;
        }

    }

}