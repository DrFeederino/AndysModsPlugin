using AndysModsPlugin.mods.ModToggle;
using AndysModsPlugin.mods.QuickSwitch;
using LC_API.ClientAPI;
using LC_API.GameInterfaceAPI.Features;
using System.Collections.Generic;

namespace AndysModsPlugin.mods.ModManager
{
    public class ModManager
    {
        public static readonly ToggleModClass RareBonk = new("Rare Bonk");
        public static readonly ToggleModClass LethalLandmines = new("Lethal Mines");
        public static readonly ToggleModClass LethalTurrets = new("Lethal Turrets");
        public static readonly ToggleModClass OneOfUsKinda = new("One Of Us, Kinda");
        public static readonly ToggleModClass OptimalSells = new("Optimal Sells");
        private static readonly Dictionary<string, System.Action<string[]>> AvailableCommands = new()
        {
            { "bonk", (_) => ToggleMod("bonk") },
            { "quick", (_) => ToggleMod("quick") },
            { "turrets", (_) => ToggleMod("turrets") },
            { "mines", (_) => ToggleMod("mines") },
            { "mask", (_) => ToggleMod("mask") },
            { "sell", (_) => ToggleMod("sell") }
        };

        public static void ToggleMod(string mod)
        {
            string displayMessage;
            switch (mod)
            {
                case "bonk":
                    RareBonk.Toggle();
                    displayMessage = $"{RareBonk.name} is {(RareBonk.enabled.Value ? "enabled" : "disabled")}";
                    break;
                case "quick":
                    QuickSwitchInput.Instance.ToggleMod();
                    displayMessage = $"{QuickSwitchInput.QuickSwitchMod.name} is {(QuickSwitchInput.QuickSwitchMod.enabled.Value ? "enabled" : "disabled")}";
                    break;
                case "mines":
                    LethalLandmines.Toggle();
                    displayMessage = $"{LethalLandmines.name} is {(LethalLandmines.enabled.Value ? "enabled" : "disabled")}";
                    break;
                case "turrets":
                    LethalTurrets.Toggle();
                    displayMessage = $"{LethalTurrets.name} is {(LethalTurrets.enabled.Value ? "enabled" : "disabled")}";
                    break;
                case "mask":
                    OneOfUsKinda.Toggle();
                    displayMessage = $"{OneOfUsKinda.name} is {(OneOfUsKinda.enabled.Value ? "enabled" : "disabled")}";
                    break;
                case "sell":
                    OptimalSells.Toggle();
                    displayMessage = $"{OptimalSells.name}  is {(OptimalSells.enabled.Value ? "enabled" : "disabled")}";
                    break;
                default:
                    displayMessage = $"Unrecognized mod {mod}. Ignoring.";
                    break;
            };
            AndysModsPlugin.Log.LogInfo($"Mod Manager: {displayMessage}!");
            Player.LocalPlayer.QueueTip("Andy Mods Manager", displayMessage);
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

        internal static void BindConfigs()
        {
            RareBonk.enabled = AndysModsPlugin.Instance.Config.Bind(RareBonk.name, "enabled", defaultValue: true, $"Enables/disables {RareBonk.name} mod.");
            LethalLandmines.enabled = AndysModsPlugin.Instance.Config.Bind(LethalLandmines.name, "enabled", defaultValue: true, $"Enables/disables {LethalLandmines.name} mod.");
            LethalTurrets.enabled = AndysModsPlugin.Instance.Config.Bind(LethalTurrets.name, "enabled", defaultValue: true, $"Enables/disables {LethalTurrets.name} mod.");
            OneOfUsKinda.enabled = AndysModsPlugin.Instance.Config.Bind(OneOfUsKinda.name, "enabled", defaultValue: true, $"Enables/disables {OneOfUsKinda.name} mod.");
            OptimalSells.enabled = AndysModsPlugin.Instance.Config.Bind(OptimalSells.name, "enabled", defaultValue: true, $"Enables/disables {OptimalSells.name} mod.");
            QuickSwitchInput.QuickSwitchMod.enabled = AndysModsPlugin.Instance.Config.Bind(QuickSwitchInput.QuickSwitchMod.name, "enabled", defaultValue: true, $"Enables/disables {QuickSwitchInput.QuickSwitchMod.name} mod.");
        }

        internal static void Init()
        {
            RegisterChatCommands();
            BindConfigs();
        }
    }

}
