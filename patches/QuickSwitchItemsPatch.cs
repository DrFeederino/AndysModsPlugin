using HarmonyLib;
using UnityEngine.InputSystem;
using LethalCompanyInputUtils.Api;
using GameNetcodeStuff;
using UnityEngine;
using System.Reflection;
using LethalCompanyInputUtils;
using System.Runtime.Remoting.Contexts;
using System;

namespace AndysModsPlugin.patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    [HarmonyWrapSafe]
    internal static class QuickSwitchPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        internal static void addQuickSwitchBehaviour(PlayerControllerB __instance)
        {
            if (__instance == null) return;
            AndysModsPlugin.Log.LogInfo("QuickSwitch: registering QuickSwitchBehaviour to enable quick switching.");
            __instance.gameObject.AddComponent<QuickSwitchBehaviour>();
        }
    }
    
    public class QuickSwitchInputClass : LcInputActions
    {
        
        [InputAction("<Keyboard>/1", Name = "Change Player Item Slot #1")]
        public InputAction QuickItemFirstKey { get; set; }
        [InputAction("<Keyboard>/2", Name = "Change Player Item Slot #2")]
        public InputAction QuickItemSecondKey { get; set; }
        [InputAction("<Keyboard>/3", Name = "Change Player Item Slot #3")]
        public InputAction QuickItemThirdKey { get; set; }
        [InputAction("<Keyboard>/4", Name = "Change Player Item Slot #4")]
        public InputAction QuickItemFourthKey { get; set; }
        public static readonly QuickSwitchInputClass Instance = new();
        public void deregisterExistingButtonActions()
        {
            AndysModsPlugin.Log.LogInfo("QuickSwitch: Deregistering all known InputActions to avoid collisions.");
            GameObject gameObject = GameObject.Find("PlayerSettingsObject");
            if (gameObject == null) { return;  }
            PlayerInput component = gameObject?.GetComponent<PlayerInput>();
            component?.actions.FindAction("Emote1", false)?.Disable();
            component?.actions.FindAction("Emote2", false)?.Disable();
            component?.actions.FindAction("Middle Finger", false)?.Disable();
            component?.actions.FindAction("Clap", false)?.Disable();
        }
    }

    internal class QuickSwitchBehaviour : MonoBehaviour
    {
        private PlayerControllerB player;
        public void Awake()
        {
            AndysModsPlugin.Log.LogInfo("QuickSwitch: setting up keyboard callbacks.");
            SetupKeybindCallbacks();
        }

        public void SetupKeybindCallbacks()
        {
            QuickSwitchInputClass.Instance.deregisterExistingButtonActions();
            QuickSwitchInputClass.Instance.QuickItemFirstKey.performed += OnFirstSwitchKeyPressed;
            QuickSwitchInputClass.Instance.QuickItemSecondKey.performed += OnSecondSwitchKeyPressed;
            QuickSwitchInputClass.Instance.QuickItemThirdKey.performed += OnThirdSwitchKeyPressed;
            QuickSwitchInputClass.Instance.QuickItemFourthKey.performed += OnFourthSwitchKeyPressed;
            AndysModsPlugin.Log.LogInfo("QuickSwitch: Callbacks are set up");

        }

        private void OnFirstSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            changePlayerItemSlot(context, 0);
        }
        private void OnSecondSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            changePlayerItemSlot(context, 1);
        }
        private void OnThirdSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            changePlayerItemSlot(context, 2);
        }
        private void OnFourthSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            changePlayerItemSlot(context, 3);
        }

        /**
         * Game supports only going forward or backwards by 1 slot. It is decided solely by reading float value of CallbackContext. For buttons its positive.
         * So, the idea is to trick the game that current slot is "pressed_key - 1" so it would go forward to it!
         */
        private void changePlayerItemSlot(InputAction.CallbackContext context, int keyNum)
        {
            if (player == null)
            {
                player = StartOfRound.Instance.localPlayerController;
            }
            if (player.inTerminalMenu)
            {
                return;
            }

            if (player.currentItemSlot != keyNum)
            {
                // Original game logic to check if we can switch slots
                if (((player.IsOwner && player.isPlayerControlled && (!player.IsServer || player.isHostPlayerObject)) || player.isTestingPlayer) && !((float)Traverse.Create(player).Field("timeSinceSwitchingSlots").GetValue() < 0.3f) && !player.isGrabbingObjectAnimation && !player.quickMenuManager.isMenuOpen && !player.inSpecialInteractAnimation && !(bool)Traverse.Create(player).Field("throwingObject").GetValue() && !player.isTypingChat && !player.twoHanded && !player.activatingItem && !player.jetpackControls && !player.disablingJetpackControls) {
                    player.currentItemSlot = keyNum == 0 ? 3 : keyNum - 1;
                    MethodInfo dynMethod = player.GetType().GetMethod("ScrollMouse_performed", BindingFlags.NonPublic | BindingFlags.Instance);
                    dynMethod.Invoke(player, new object[] { context });
                }
            }
        }
        
    }
}
