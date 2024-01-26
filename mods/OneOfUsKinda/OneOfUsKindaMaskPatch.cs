using GameNetcodeStuff;
using HarmonyLib;

namespace AndysModsPlugin.mods.OneOfUsKinda
{
    [HarmonyPatch(typeof(HauntedMaskItem))]
    internal class OneOfUsKindaMaskPatch
    {

        [HarmonyPrefix, HarmonyPatch(typeof(HauntedMaskItem), "BeginAttachment")]
        [HarmonyWrapSafe]
        internal static bool RerollLuckForPlayer()
        {
            if (!ModManager.ModManager.OneOfUsKinda.enabled.Value)
            {
                return true;
            }
            AndysModsPlugin.Log.LogInfo($"One Of Us: re-rolling mask attachment for player.");
            // 20% chance
            return UnityEngine.Random.Range(0, 100) <= 20;
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EnemyAI), "CheckLineOfSightForPlayer")]
        [HarmonyWrapSafe]
        internal static void CheckLineOfSightForPlayerPatch(ref PlayerControllerB __result)
        {
            if (!ModManager.ModManager.OneOfUsKinda.enabled.Value)
            {
                return;
            }
            DetargetIfMaskIsActivated(ref __result);
        }


        [HarmonyPostfix, HarmonyPatch(typeof(EnemyAI), "CheckLineOfSightForClosestPlayer")]
        [HarmonyWrapSafe]
        internal static void CheckLineOfSightForClosestPlayerPatch(ref PlayerControllerB __result)
        {
            if (!ModManager.ModManager.OneOfUsKinda.enabled.Value)
            {
                return;
            }
            DetargetIfMaskIsActivated(ref __result);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(EnemyAI), "PlayerIsTargetable")]
        [HarmonyWrapSafe]
        internal static void PlayerIsTargetablePatch(PlayerControllerB playerScript, ref bool __result)
        {
            if (!ModManager.ModManager.OneOfUsKinda.enabled.Value)
            {
                return;
            }
            DetargetIfMaskIsActivated(ref playerScript);
            __result = playerScript != null;
        }

        private static void DetargetIfMaskIsActivated(ref PlayerControllerB result)
        {
            if (result != null && result.ItemSlots[result.currentItemSlot] != null && result.ItemSlots[result.currentItemSlot].name.Contains("Mask"))
            {
                if (Traverse.Create((HauntedMaskItem)result.ItemSlots[result.currentItemSlot]).Field<bool>("maskOn").Value)
                {
                    AndysModsPlugin.Log.LogInfo($"One Of Us: de-targeting enemy from player {result.playerUsername}.");
                    result = null;
                }
            }
        }

    }
}
