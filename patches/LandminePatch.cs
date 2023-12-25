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
        internal static HashSet<string> knownEnemies = new HashSet<string>
        {
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
        };


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
        internal static bool OnTriggerEnter(Landmine __instance, Collider other)
        {
            Traverse pressMineDebounceTimer = Traverse.Create(__instance).Field("pressMineDebounceTimer");
            if (__instance.hasExploded || (float)pressMineDebounceTimer.GetValue() > 0f)
            {
                return false;
            }
            triggerMineIfEnemy(__instance, other);
            pressMineDebounceTimer.SetValue(0.5f);
            return true;
        }

        private static void triggerMineIfEnemy(Landmine __instance, Collider other)
        {
            if (other == null) return;
            if (IsEnemy(other.transform.parent.gameObject))
            {
                AndysModsPlugin.Log.LogInfo($"Landmines Are No Joke: Triggering mine explosion for {other.transform.parent.gameObject}.");
                if (!__instance.hasExploded)
                {
                    __instance.SetOffMineAnimation();
                    Traverse.Create(__instance).Field("sendingExplosionRPC").SetValue(true);
                    __instance.ExplodeMineServerRpc();
                }

            }
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPrefix]
        internal static bool OnTriggerExit(Landmine __instance, Collider other)
        {
            if (__instance.hasExploded)
            {
                return false;
            }
            triggerMineIfEnemy(__instance, other);
            return true;
        }
    }

}
