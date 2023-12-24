using HarmonyLib;
using UnityEngine;
using Unity.Netcode;

namespace AndysModsPlugin.patches
{
    [HarmonyPatch(typeof(Shovel))]
    internal static class RareBonkPatch
    {
        public static AudioClip[] originalSfx;
        static AssetBundle bonkBundle = AssetBundle.LoadFromFile($"{Plugin.PluginAssetsPath}bonk");
        public static AudioClip[] bonkSfx = bonkBundle.LoadAssetWithSubAssets<AudioClip>("bonk.mp3");

        [HarmonyPatch("__initializeVariables")]
        [HarmonyPrefix]
        internal static void Start(Shovel __instance)
        {
            BonkSyncBehaviour component;
            if (__instance.TryGetComponent(out component))
            {
                Plugin.Log.LogInfo("Special BONK game component has already been added.");
                return;
            }
            if (__instance != null && __instance.hitSFX != null && __instance.hitSFX.Length > 0 && originalSfx == null)
            {
                Plugin.Log.LogInfo("Saved shovel's original sound for later.");
                originalSfx = __instance.hitSFX;
                Plugin.Log.LogInfo("BONK is hooking up to shovel instance.");
                __instance.gameObject.AddComponent<BonkSyncBehaviour>().Init(__instance);
            }
        }

        [HarmonyPatch("HitShovel")]
        [HarmonyPrefix]
        static void replaceShovelSound(Shovel __instance)
        {
            System.Random random = new System.Random();
            int bonkChance = random.Next(1, 100);
            if (__instance != null && bonkChance < 10)
            {
                Plugin.Log.LogInfo("BONK!");
                __instance.shovelHitForce = 100; // should 100 be enough to BONK?
                replaceSoundsForPlayers(__instance);
            }
        }

        [HarmonyPatch("HitShovel")]
        [HarmonyPostfix]
        static void restoreShovelDamage(Shovel __instance)
        {
            if (__instance.shovelHitForce != 1)
            {
                Plugin.Log.LogInfo("BONK restored original shovelHitForce!");
                __instance.shovelHitForce = 1;
                __instance.hitSFX = originalSfx;
                BonkSyncBehaviour component;
                if (__instance.TryGetComponent(out component))
                {
                    component.restorePlayerShovelSound();
                    component.RestoreShovelSoundServerRpc();
                }
            }
        }

        static void replaceSoundsForPlayers(Shovel __instance)
        {
            if (bonkBundle != null && bonkSfx != null && bonkSfx.Length != 0)
            {
                Plugin.Log.LogInfo("Found BONK sound file. Replacing the original shovel sounds.");
                __instance.hitSFX = bonkSfx;
                BonkSyncBehaviour component;
                if (__instance.TryGetComponent(out component))
                {
                    component.changeAudioToPlayerBonk();
                    component.RestoreShovelSoundServerRpc();
                }
            }
            else
            {
                Plugin.Log.LogInfo("No BONK sound asset found. Sad shibe.");
            }
        }

    }

    internal class BonkSyncBehaviour : NetworkBehaviour
    {
        private Shovel shovel;

        public void Init(Shovel shovel)
        {
            this.shovel = shovel;
        }
        public void changeAudioToPlayerBonk()
        {
            if (RareBonkPatch.bonkSfx == null)
            {
                Plugin.Log.LogInfo("No BONK SFX has been found. Not changing shovel's sounds.");
                return;
            }
            Plugin.Log.LogInfo("Switched the sound of the shovel for player.");
            shovel.hitSFX = RareBonkPatch.bonkSfx;
        }

        public void restorePlayerShovelSound()
        {
            if (RareBonkPatch.originalSfx == null)
            {
                Plugin.Log.LogInfo("No original SFX for shovel found.");
                return;
            }
            Plugin.Log.LogInfo("Restored shovel's original SFX.");
            shovel.hitSFX = RareBonkPatch.originalSfx;
        }

        [ServerRpc]
        public void ReplaceShovelSoundToBonk()
        {
            NetworkManager networkManager = shovel.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
            {
                if (shovel.OwnerClientId != networkManager.LocalClientId)
                {
                    return;
                }

                ServerRpcParams serverRpcParams = default;
                FastBufferWriter bufferWriter = __beginSendServerRpc(4113335123u, serverRpcParams, RpcDelivery.Reliable);
                __endSendServerRpc(ref bufferWriter, 4113335123u, serverRpcParams, RpcDelivery.Reliable);
            }

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
            {
                ReplaceShovelSoundToBonkClientRpc();
            }
        }

        [ClientRpc]
        public void ReplaceShovelSoundToBonkClientRpc()
        {
            NetworkManager networkManager = shovel.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default;
                    FastBufferWriter bufferWriter = __beginSendClientRpc(2042054613u, clientRpcParams, RpcDelivery.Reliable);
                    __endSendClientRpc(ref bufferWriter, 2042054613u, clientRpcParams, RpcDelivery.Reliable);
                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost) && !IsOwner)
                {
                    changeAudioToPlayerBonk();
                }
            }
        }

        [ServerRpc]
        public void RestoreShovelSoundServerRpc()
        {
            NetworkManager networkManager = shovel.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }

            if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
            {
                if (shovel.OwnerClientId != networkManager.LocalClientId)
                {
                    if (networkManager.LogLevel <= LogLevel.Normal)
                    {
                        Debug.LogError("Only the owner can invoke a ServerRpc that requires ownership!");
                    }

                    return;
                }

                ServerRpcParams serverRpcParams = default;
                FastBufferWriter bufferWriter = __beginSendServerRpc(4113335123u, serverRpcParams, RpcDelivery.Reliable);
                __endSendServerRpc(ref bufferWriter, 4113335123u, serverRpcParams, RpcDelivery.Reliable);
            }

            if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
            {
                RestoreShovelSoundClientRpc();
            }
        }

        [ClientRpc]
        public void RestoreShovelSoundClientRpc()
        {
            NetworkManager networkManager = shovel.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default;
                    FastBufferWriter bufferWriter = __beginSendClientRpc(2042054613u, clientRpcParams, RpcDelivery.Reliable);
                    __endSendClientRpc(ref bufferWriter, 2042054613u, clientRpcParams, RpcDelivery.Reliable);
                }

                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost) && !IsOwner)
                {
                    restorePlayerShovelSound();
                }
            }
        }
    }

}
