using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AndysModsPlugin.patches
{
    [HarmonyPatch(typeof(Landmine))]
    [HarmonyWrapSafe]
    internal static class LandminePatch
    {
        // Helper hash-set to detect an enemy we know. Needed for landmine. Postfix "(Clone)" is caused by game duplication.
        internal static HashSet<string> knownEnemies =
        [
            "MaskedPlayerEnemy(Clone)",
            "MaskedPlayerEnemy",
            "NutcrackerEnemy(Clone)",
            "NutcrackerEnemy",
            "BaboonHawkEnemy(Clone)",
            "BaboonHawkEnemy",
            "Flowerman(Clone)",
            "Flowerman",
            "SandSpider(Clone)",
            "SandSpider",
            "Centipede(Clone)",
            "Centipede",
            "SpringMan(Clone)",
            "SpringMan",
            "DressGirl(Clone)",
            "DressGirl",
            "HoarderBug(Clone)",
            "HoarderBug",
            "Blob(Clone)",
            "Blob",
            "JesterEnemy(Clone)",
            "JesterEnemy",
            "PufferEnemy(Clone)",
            "PufferEnemy",
            "Crawler(Clone)",
            "Crawler",
        ];


        /**
         * Helper method to detect if gameObject's name is one of Enemy.
         */
        public static bool IsEnemy(GameObject enemy)
        {
            if (enemy == null)
            {
                return false;
            }
            return knownEnemies.Contains(enemy.name);
        }

        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPrefix]
        internal static void OnTriggerEnter(Landmine __instance, Collider other, ref bool ___sendingExplosionRPC, ref float ___pressMineDebounceTimer)
        {
            if (!ModSettings.LandmineMod.IsEnabled)
            {
                return;
            }
            if (__instance.hasExploded || ___pressMineDebounceTimer > 0f)
            {
                return;
            }
            TriggerMineIfEnemy(__instance, other, ref ___sendingExplosionRPC);
            ___pressMineDebounceTimer = 0.5f;
        }

        private static void TriggerMineIfEnemy(Landmine __instance, Collider other, ref bool ___sendingExplosionRPC)
        {
            if (other == null) return;
            if (IsEnemy(other.transform.parent.gameObject))
            {
                AndysModsPlugin.Log.LogInfo($"Landmines Are No Joke: Triggering mine explosion for {other.transform.parent.gameObject}.");
                if (!__instance.hasExploded)
                {
                    __instance.SetOffMineAnimation();
                    ___sendingExplosionRPC = true;
                    __instance.ExplodeMineServerRpc();
                }

            }
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPrefix]
        internal static void OnTriggerExit(Landmine __instance, Collider other, ref bool ___sendingExplosionRPC)
        {
            if (!ModSettings.LandmineMod.IsEnabled)
            {
                return;
            }
            if (__instance.hasExploded)
            {
                return;
            }
            TriggerMineIfEnemy(__instance, other, ref ___sendingExplosionRPC);
        }
    }

}
