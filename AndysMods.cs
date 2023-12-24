using HarmonyLib;
using UnityEngine;

namespace AndysModsPlugin
{
    /**
     * Main class to enable all mods patches.
     */
    public class AndysMods : MonoBehaviour
    {

        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public void Init()
        {
            Plugin.Log.LogInfo("AndysMods is installed. Executing patches.");

            harmony.PatchAll();

            Plugin.Log.LogInfo("Successfully added patches to game. Enjoy!");
        }
    }

    
}
