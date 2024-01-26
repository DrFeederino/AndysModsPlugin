using HarmonyLib;
using System.Reflection;
using UnityEngine.InputSystem;
using Unity.Netcode;

namespace AndysModsPlugin.mods.QuickSwitch
{
    internal class QuickSwitchBehaviour : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            AndysModsPlugin.Log.LogInfo("Quick Switch: setting up keyboard callbacks.");
            SetupKeybindCallbacks();
            QuickSwitchInput.Instance.EnableModInputActions();
            base.OnNetworkSpawn();
        }

        public void SetupKeybindCallbacks()
        {
            QuickSwitchInput.Instance.QuickItemFirstKey.performed += OnFirstSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemSecondKey.performed += OnSecondSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemThirdKey.performed += OnThirdSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemFourthKey.performed += OnFourthSwitchKeyPressed;
            AndysModsPlugin.Log.LogInfo("Quick Switch: input callbacks are set up.");
        }

        public override void OnDestroy()
        {
            QuickSwitchInput.Instance.DisableModInputActions();
            QuickSwitchInput.Instance.QuickItemFirstKey.performed -= OnFirstSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemSecondKey.performed -= OnSecondSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemThirdKey.performed -= OnThirdSwitchKeyPressed;
            QuickSwitchInput.Instance.QuickItemFourthKey.performed -= OnFourthSwitchKeyPressed;
            AndysModsPlugin.Log.LogInfo("Quick Switch: input callbacks are removed.");
            base.OnDestroy();
        }

        private void OnFirstSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            ChangePlayerItemSlot(0);
        }
        private void OnSecondSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            ChangePlayerItemSlot(1);
        }
        private void OnThirdSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            ChangePlayerItemSlot(2);
        }
        private void OnFourthSwitchKeyPressed(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            ChangePlayerItemSlot(3);
        }

        /**
         * Game supports only going forward or backwards by 1 slot. It is decided solely by reading float value of CallbackContext. For buttons its positive.
         * So, the idea is to trick the game that current slot is "pressed_key - 1" so it would go forward to it!
         */
        private void ChangePlayerItemSlot(int keyNum)
        {
            if (!QuickSwitchInput.QuickSwitchMod.enabled.Value)
            {
                QuickSwitchInput.Instance.DisableModInputActions();
                return;
            }
            else if (!QuickSwitchInput.Instance.Enabled)
            {
                QuickSwitchInput.Instance.EnableModInputActions();
            }
            if (GameNetworkManager.Instance?.localPlayerController == null)
            {
                return;
            }
            if (GameNetworkManager.Instance.localPlayerController.inTerminalMenu)
            {
                return;
            }
            var player = StartOfRound.Instance.localPlayerController;
            var playerFields = Traverse.Create(player);
            if (GameNetworkManager.Instance.localPlayerController.currentItemSlot != keyNum)
            {
                // Original game logic to check if we can switch slots
                if ((player.IsOwner && player.isPlayerControlled && (!player.IsServer || player.isHostPlayerObject) || player.isTestingPlayer) && !(playerFields.Field<float>("timeSinceSwitchingSlots").Value < 0.3f) && !player.isGrabbingObjectAnimation && !player.quickMenuManager.isMenuOpen && !player.inSpecialInteractAnimation && !playerFields.Field<bool>("throwingObject").Value && !player.isTypingChat && !player.twoHanded && !player.activatingItem && !player.jetpackControls && !player.disablingJetpackControls)
                {
                    SwitchItemSlotsServerRpc(keyNum, GameNetworkManager.Instance.localPlayerController.NetworkObjectId);
                }
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void SwitchItemSlotsServerRpc(int slot, ulong playerId)
        {
            SwitchItemSlotsClientRpc(slot, playerId);
        }

        [ClientRpc]
        public void SwitchItemSlotsClientRpc(int slot, ulong playerId)
        {
            StartOfRound.Instance.allPlayerScripts.DoIf(player => player.NetworkObjectId == playerId, player =>
            {
                player?.GetType().GetMethod("SwitchToItemSlot", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(player, [slot, null]);
            });
        }

    }
}
