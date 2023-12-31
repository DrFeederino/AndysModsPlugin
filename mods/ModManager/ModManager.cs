﻿using AndysModsPlugin.mods.ModToggle;
using AndysModsPlugin.mods.QuickSwitch;
using LC_API.ClientAPI;
using System.Collections.Generic;

namespace AndysModsPlugin.mods.ModManager
{
    public class ModManager : ToggleModClass
    {
        public static readonly ToggleModClass RareBonk = new();
        public static readonly ToggleModClass LethalLandmines = new();
        public static readonly ToggleModClass LethalTurrets = new();
        private static readonly Dictionary<string, System.Action<string[]>> AvailableCommands = new()
        {
            { "bonk", (_) => ToggleMod("bonk") },
            { "quick", (_) => ToggleMod("quick") },
            { "turrets", (_) => ToggleMod("turrets") },
            // Debug command for Lethal Turrets
            //{ "tur" , (_) => LethalTurretBehaviour.TeleportPlayerToTurret() },
            { "mines", (_) => ToggleMod("mines") }
        };

        public static void ToggleMod(string mod)
        {
            switch (mod)
            {
                case "bonk":
                    RareBonk.Toggle();
                    AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: RareBonk is {(RareBonk.IsEnabled ? "enabled" : "disabled")}!");
                    break;
                case "quick":
                    QuickSwitchInput.Instance.ToggleMod();
                    AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: Quick Switch is {(QuickSwitchInput.QuickSwitchMod.IsEnabled ? "enabled" : "disabled")}!");
                    break;
                case "mines":
                    LethalLandmines.Toggle();
                    AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: Lethal Mines is {(LethalLandmines.IsEnabled ? "enabled" : "disabled")}!");
                    break;
                case "turrets":
                    LethalTurrets.Toggle();
                    AndysModsPlugin.Log.LogInfo($"ModToggleEnabler: Lethal Turrets is {(LethalLandmines.IsEnabled ? "enabled" : "disabled")}!");
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
