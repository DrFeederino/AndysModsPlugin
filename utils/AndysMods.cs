using AndysModsPlugin.mods.ModManager;
using HarmonyLib;
using UnityEngine;

namespace AndysModsPlugin.utils
{
    /**
     * Main class to enable all mods patches.
     */
    public class AndysMods : MonoBehaviour
    {

        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        public void Init()
        {
            AndysModsPlugin.Log.LogInfo("AndysMods is installed. Have fun!");

            harmony.PatchAll();

            ModManager.RegisterChatCommands();
        }
    }

}
