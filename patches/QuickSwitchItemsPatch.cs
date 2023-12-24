using HarmonyLib;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.Collections;

namespace AndysModsPlugin.patches
{
    internal class QuickSwitchInput 
    {
        private PlayerActions actions;
        private const string ACTION_NAME = "QuickSwitchItemButton";
        private static string[] PATHS_KEYBOARD = new string[]
        {
            "<Keyboard>/1",
            "<Keyboard>/2",
            "<Keyboard>/3",
            "<Keyboard>/4"
        };


        public void Init(PlayerActions actions)
        {
            this.actions = actions;
            addBindings();
            addAction();
        }

        private void addBindings()
        {
            //actions.Movement.AddCallbacks
            //actions.bindings.AddItem
            foreach (string path in PATHS_KEYBOARD)
            {
                InputBinding binding = new InputBinding();
                binding.action = ACTION_NAME;
                binding.path = path;
                actions.bindings.AddItem(binding);
            }
            

        }

        private void addAction()
        {
            InputAction inputAction = new InputAction(ACTION_NAME, InputActionType.Value, null, null, null, InputActionType.Button.ToString());
            inputAction.wantsInitialStateCheck = true;
            actions.asset.AddItem(inputAction);
        }
    }
}
