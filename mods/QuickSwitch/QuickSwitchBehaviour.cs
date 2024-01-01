using GameNetcodeStuff;
using HarmonyLib;
using System.Reflection;
using UnityEngine.InputSystem;
using UnityEngine;

namespace AndysModsPlugin.mods.QuickSwitch
{
    internal class QuickSwitchBehaviour : MonoBehaviour
    {
        private PlayerControllerB player;

        public void Awake()
        {
            AndysModsPlugin.Log.LogInfo("Quick Switch: setting up keyboard callbacks.");
            SetupKeybindCallbacks();
            QuickSwitchInput.Instance.EnableModInputActions();
        }

        public void SetupKeybindCallbacks()
        {
            QuickSwitchInput.Instance.QuickItemFirstKey.performed += OnFirstSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemSecondKey.performed += OnSecondSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemThirdKey.performed += OnThirdSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemFourthKey.performed += OnFourthSwitchKeyPressed;
            AndysModsPlugin.Log.LogInfo("Quick Switch: input callbacks are set up.");

        }

        public void OnDestroy()
        {
            QuickSwitchInput.Instance.DisableModInputActions();
            QuickSwitchInput.Instance.QuickItemFirstKey.performed -= OnFirstSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemSecondKey.performed -= OnSecondSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemThirdKey.performed -= OnThirdSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemFourthKey.performed -= OnFourthSwitchKeyPressed;
            AndysModsPlugin.Log.LogInfo("Quick Switch: input callbacks are removed up.");
        }

        private void OnFirstSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            ChangePlayerItemSlot(context, 0);
        }
        private void OnSecondSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            ChangePlayerItemSlot(context, 1);
        }
        private void OnThirdSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            ChangePlayerItemSlot(context, 2);
        }
        private void OnFourthSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            ChangePlayerItemSlot(context, 3);
        }

        /**
         * Game supports only going forward or backwards by 1 slot. It is decided solely by reading float value of CallbackContext. For buttons its positive.
         * So, the idea is to trick the game that current slot is "pressed_key - 1" so it would go forward to it!
         */
        private void ChangePlayerItemSlot(InputAction.CallbackContext context, int keyNum)
        {
            if (!QuickSwitchInput.QuickSwitchMod.IsEnabled)
            {
                QuickSwitchInput.Instance.DisableModInputActions();
                return;
            }
            else if (!QuickSwitchInput.Instance.Enabled)
            {
                QuickSwitchInput.Instance.EnableModInputActions();
            }
            if (player == null || GameNetworkManager.Instance?.localPlayerController != player)
            {
                player = GameNetworkManager.Instance?.localPlayerController;
                return;
            }
            if (player.inTerminalMenu)
            {
                return;
            }

            if (player.currentItemSlot != keyNum)
            {
                // Original game logic to check if we can switch slots
                if ((player.IsOwner && player.isPlayerControlled && (!player.IsServer || player.isHostPlayerObject) || player.isTestingPlayer) && !((float)Traverse.Create(player).Field("timeSinceSwitchingSlots").GetValue() < 0.3f) && !player.isGrabbingObjectAnimation && !player.quickMenuManager.isMenuOpen && !player.inSpecialInteractAnimation && !(bool)Traverse.Create(player).Field("throwingObject").GetValue() && !player.isTypingChat && !player.twoHanded && !player.activatingItem && !player.jetpackControls && !player.disablingJetpackControls)
                {
                    //Fingers fucking crossed [ServerRpc] method won't blow up in my face for this one
                    player.GetType().GetMethod("SwitchToItemSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player, [keyNum, null]);
                    player.GetType().GetMethod("SwitchItemSlotsServerRpc", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player, [true]);
                }
            }
        }

    }
}
