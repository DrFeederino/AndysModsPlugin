using HarmonyLib;

namespace AndysModsPlugin.mods.QuickSwitch
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    [HarmonyWrapSafe]
    internal static class QuickSwitchPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        internal static void AddQuickSwitchBehaviour(GameNetworkManager __instance)
        {
            if (__instance == null) return;
            if (__instance.gameObject.GetComponent<QuickSwitchBehaviour>() != null)
            {
                return;
            }
            AndysModsPlugin.Log.LogInfo($"QuickSwitch: registering QuickSwitchBehaviour.");
            __instance.gameObject.AddComponent<QuickSwitchBehaviour>();
        }

    }

}