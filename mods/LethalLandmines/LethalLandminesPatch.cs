using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AndysModsPlugin.mods.LethalLandmines
{
    [HarmonyPatch(typeof(Landmine))]
    [HarmonyWrapSafe]
    internal static class LethalLandminesPatch
    {
        // Handy hash-set to detect an enemy we know. Needed for landmine. Postfix "(Clone)" is caused by game duplication.
        internal static HashSet<string> knownEnemies =
        [
            "MaskedPlayerEnemy(Clone)",
            "NutcrackerEnemy(Clone)",
            "BaboonHawkEnemy(Clone)",
            "Flowerman(Clone)",
            "SandSpider(Clone)",
            "Centipede(Clone)",
            "SpringMan(Clone)",
            "DressGirl(Clone)",
            "HoarderBug(Clone)",
            "Blob(Clone)",
            "JesterEnemy(Clone)",
            "PufferEnemy(Clone)",
            "Crawler(Clone)"
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
            if (!ModManager.ModManager.LethalLandmines.IsEnabled)
            {
                return;
            }
            if (__instance.hasExploded || ___pressMineDebounceTimer > 0f)
            {
                return;
            }
            TriggerMineIfEnemy(__instance, other, ref ___sendingExplosionRPC, ref ___pressMineDebounceTimer, false);
        }

        private static void TriggerMineIfEnemy(Landmine __instance, Collider other, ref bool ___sendingExplosionRPC, ref float ___pressMineDebounceTimer, bool isExit)
        {
            if (IsEnemy(other?.transform?.parent?.gameObject))
            {
                if (isExit)
                {
                    AndysModsPlugin.Log.LogInfo($"Lethal Landmines: OnTriggerExit mine explosion for {other.transform.parent.gameObject}.");
                    __instance.SetOffMineAnimation();
                    ___sendingExplosionRPC = true;
                    __instance.ExplodeMineServerRpc();
                } else
                {
                    AndysModsPlugin.Log.LogInfo($"Lethal Landmines: OnTriggerEnter mine explosion for {other.transform.parent.gameObject}.");
                    ___pressMineDebounceTimer = 0.5f;
                    __instance.PressMineServerRpc();
                }
                

            }
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPrefix]
        internal static void OnTriggerExit(Landmine __instance, Collider other, ref bool ___sendingExplosionRPC, ref float ___pressMineDebounceTimer)
        {
            if (!ModManager.ModManager.LethalLandmines.IsEnabled)
            {
                return;
            }
            if (__instance.hasExploded)
            {
                return;
            }
            TriggerMineIfEnemy(__instance, other, ref ___sendingExplosionRPC, ref ___pressMineDebounceTimer, true);
        }
    }

}
