using AndysModsPlugin.patches;
using LC_API.ClientAPI;
using System.Collections;
using System.Collections.Generic;

namespace AndysModsPlugin
{
    public class ModManager
    {
        public bool IsEnabled = true; // default state of all mods
        public void Toggle() => IsEnabled = !IsEnabled;
    }

    internal class ModSettings : ModManager
    {
        public static readonly ModManager RareBonkMod = new();
        public static readonly ModManager LandmineMod = new();
        private static readonly Dictionary<string, System.Action<string[]>> AvailableCommands = new()
        {
            { "bonk", (_) => ToggleMod("bonk")},
            { "quickitems", (_) => ToggleMod("quickitems") },
            { "landmine", (_) => ToggleMod("landmine") }
        };

        public static void ToggleMod(string mod)
        {
            switch (mod)
            {
                case "bonk":
                    RareBonkMod.Toggle();
                    AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: RareBonk is {(RareBonkMod.IsEnabled ? "enabled" : "disabled")}!");
                    break;
                case "quickitems":
                    QuickSwitchInputClass.Instance.ToggleMod();
                    AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: Quick Switch is {(QuickSwitchInputClass.QuickSwitchMod.IsEnabled ? "enabled" : "disabled")}!");
                    break;
                case "landmine":
                    LandmineMod.Toggle();
                    AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: Landmines Are No Joke is {(LandmineMod.IsEnabled ? "enabled" : "disabled")}!");
                    break;
                default:
                    AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: unrecognized mod {mod}. Ignoring.");
                    break;
            };
        }

        public static void RegisterChatCommands()
        {
            AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: registering chat commands.");
            foreach (var command in AvailableCommands)
            {
                AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: registering chat command {command.Key}.");
                CommandHandler.RegisterCommand(command.Key, command.Value);
            }
        }
    }

}