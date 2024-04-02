using AndysModsPlugin.mods.ModToggle;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace AndysModsPlugin.mods.QuickSwitch
{
    public class QuickSwitchInput : LcInputActions
    {
        private PlayerInput playerInput = IngamePlayerSettings.Instance?.playerInput;
        [InputAction("<Keyboard>/1", Name = "Change Player Item Slot #1")]
        public InputAction QuickItemFirstKey { get; set; }
        [InputAction("<Keyboard>/2", Name = "Change Player Item Slot #2")]
        public InputAction QuickItemSecondKey { get; set; }
        [InputAction("<Keyboard>/3", Name = "Change Player Item Slot #3")]
        public InputAction QuickItemThirdKey { get; set; }
        [InputAction("<Keyboard>/4", Name = "Change Player Item Slot #4")]
        public InputAction QuickItemFourthKey { get; set; }
        public static readonly QuickSwitchInput Instance = new();
        public static readonly ToggleModClass QuickSwitchMod = new("Quick Switch");

        public void ToggleMod()
        {
            QuickSwitchMod.Toggle();
            if (QuickSwitchMod.enabled.Value)
            {
                EnableModInputActions();
            }
            else
            {
                DisableModInputActions();
            }
        }

        private void DisableExistingButtonActions()
        {
            if (playerInput == null)
            {
                playerInput = IngamePlayerSettings.Instance?.playerInput;
            }
            AndysModsPlugin.Log.LogInfo("Quick Switch: disabling all known InputActions to avoid collisions.");
            playerInput?.actions.FindAction("Emote1", false)?.Disable();
            playerInput?.actions.FindAction("Emote2", false)?.Disable();
            playerInput?.actions.FindAction("Middle Finger", false)?.Disable();
            playerInput?.actions.FindAction("Clap", false)?.Disable();
        }

        public void DisableModInputActions()
        {
            if (!Enabled) return;
            AndysModsPlugin.Log.LogInfo("QuickSwitch: disabling mod's buttons.");
            Disable();
            playerInput?.actions.FindAction("Emote1", false)?.Enable();
            playerInput?.actions.FindAction("Emote2", false)?.Enable();
            playerInput?.actions.FindAction("Middle Finger", false)?.Enable();
            playerInput?.actions.FindAction("Clap", false)?.Enable();
        }

        public void EnableModInputActions()
        {
            if (QuickSwitchMod.enabled.Value)
            {
                AndysModsPlugin.Log.LogInfo("Quick Switch: enabling mod's inputs.");
                Enable();
                DisableExistingButtonActions();
            } else
            {
                AndysModsPlugin.Log.LogInfo("Quick Switch: mod is disabled.");
                DisableModInputActions();
            }
        }
    }
}
