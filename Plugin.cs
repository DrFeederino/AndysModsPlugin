using AndysModsPlugin.utils;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System.Reflection;
using UnityEngine;

namespace AndysModsPlugin
{

    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Lethal Company.exe")]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    public sealed class AndysModsPlugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static string PluginPath;
        internal static AndysModsPlugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            PluginPath = Info.Location.TrimEnd($"{PluginInfo.PLUGIN_GUID}.dll".ToCharArray());
            GameObject val = new($"{PluginInfo.PLUGIN_NAME}-{PluginInfo.PLUGIN_VERSION}");
            val.AddComponent<AndysMods>().Init();
            DontDestroyOnLoad(val);
            Log.LogInfo($"Full plugin path: {Info.Location}");
            Log.LogInfo($"Path to plugin is: {PluginPath}");
            NetcodeWeaver();
        }

        public void BindConfig<T>(ref ConfigEntry<T> config, string section, string key, T defaultValue, string description = "")
        {
            config = Config.Bind(section, key, defaultValue, description);
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
