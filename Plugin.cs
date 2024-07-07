using AndysModsPlugin.utils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System.Reflection;
using UnityEngine;

namespace AndysModsPlugin
{

    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Lethal Company.exe")]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("LC_API_V50", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class AndysModsPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static string PluginPath;
        internal static AndysModsPlugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            PluginPath = Info.Location.TrimEnd($"{MyPluginInfo.PLUGIN_GUID}.dll".ToCharArray());
            GameObject val = new($"{MyPluginInfo.PLUGIN_NAME}-{MyPluginInfo.PLUGIN_VERSION}");
            val.AddComponent<AndysMods>().Init();
            DontDestroyOnLoad(val);
            Log.LogInfo($"Full plugin path: {Info.Location}");
            Log.LogInfo($"Path to plugin is: {PluginPath}");
            NetcodePatcher();
        }

        public void BindConfig<T>(ref ConfigEntry<T> config, string section, string key, T defaultValue, string description = "")
        {
            config = Config.Bind(section, key, defaultValue, description);
        }

        private static void NetcodePatcher()
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
