using AndysModsPlugin.patches;
using BepInEx;
using BepInEx.Logging;
using System.Reflection;
using UnityEngine;

namespace AndysModsPlugin
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Lethal Company.exe")]
    public sealed class AndysModsPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static string PluginPath;
        internal static readonly ModSettings SettingsManager = new();
        internal static readonly QuickSwitchInputClass quickSwitchInstance = QuickSwitchInputClass.Instance;
        private void Awake()
        {
            Log = Logger;
            PluginPath = Info.Location.TrimEnd($"{PluginInfo.PLUGIN_GUID}.dll".ToCharArray());
            GameObject val = new($"{PluginInfo.PLUGIN_NAME}-{PluginInfo.PLUGIN_VERSION}");
            val.AddComponent<AndysMods>().Init();
            DontDestroyOnLoad(val);
            Log.LogInfo($"Full plugin path: {Info.Location}");
            Log.LogInfo($"Path to plugin is: {PluginPath}");
            NetcodeWeaver();
        }

        private static void NetcodeWeaver()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }

    }

}
