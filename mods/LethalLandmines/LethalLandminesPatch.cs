using HarmonyLib;
using UnityEngine;

namespace AndysModsPlugin.mods.LethalLandmines
{
    [HarmonyPatch(typeof(Landmine))]
    [HarmonyWrapSafe]
    internal static class LethalLandminesPatch
    {

        /**
         * Helper method to detect if gameObject's name is one of Enemy.
         */
        public static bool IsEnemy(Collider enemy)
        {
            if (enemy == null || 
                enemy.transform == null || 
                enemy.transform.parent == null || 
                enemy.transform.parent.gameObject == null)
            {
                return false;
            }
            return enemy.CompareTag("Enemy") && enemy.transform.parent.gameObject.GetComponent<EnemyAI>().enemyType.canDie;
        }

        [HarmonyPatch("OnTriggerEnter")]
        [HarmonyPrefix]
        internal static void OnTriggerEnter(Landmine __instance, Collider other, ref bool ___sendingExplosionRPC, ref float ___pressMineDebounceTimer)
        {
            if (!ModManager.ModManager.LethalLandmines.enabled.Value)
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
            if (IsEnemy(other))
            {
                if (isExit)
                {
                    AndysModsPlugin.Log.LogInfo($"Lethal Landmines: OnTriggerExit mine explosion for {other.transform.parent.gameObject}.");
                    __instance.SetOffMineAnimation();
                    ___sendingExplosionRPC = true;
                    __instance.ExplodeMineServerRpc();
                }
                else
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
            if (!ModManager.ModManager.LethalLandmines.enabled.Value)
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
