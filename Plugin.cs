using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace AndysModsPlugin
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Lethal Company.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static string PluginPath;
        
        private void Awake()
        {
            Log = Logger;
            PluginPath = Info.Location.TrimEnd($"{PluginInfo.PLUGIN_GUID}.dll".ToCharArray());
            GameObject val = new GameObject($"{PluginInfo.PLUGIN_NAME}-{PluginInfo.PLUGIN_VERSION}");
            val.AddComponent<AndysMods>().Init();
            DontDestroyOnLoad(val);
            Log.LogInfo($"Full plugin path: {Info.Location}");
            Log.LogInfo($"Path to plugin is: {PluginPath}");
        }

    }
    
}
