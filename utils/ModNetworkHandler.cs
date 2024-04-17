using AndysModsPlugin.mods.LethalTurrets;
using AndysModsPlugin.mods.ModManager;
using AndysModsPlugin.mods.QuickSwitch;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AndysModsPlugin.utils
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    [HarmonyWrapSafe]
    internal static class GamenetworkPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void AddBonkNetworkPrefab()
        {
            AssetBundleClass.AndysModsNetworkPrefab.AddComponent<ModNetworkHandler>();
            AssetBundleClass.AndysModsNetworkPrefab.AddComponent<QuickSwitchBehaviour>();
            AndysModsPlugin.Log.LogInfo("Rare Bonk: added ModNetworkHandler and QuickSwitchBehaviour to custom network prefab.");

            AssetBundleClass.LethalTurretsNetworkPrefab.AddComponent<LethalTurretBehaviour>();
            AndysModsPlugin.Log.LogInfo("Lethal Turrets: added LethalTurretBehaviour to custom network prefab.");

            NetworkManager.Singleton.AddNetworkPrefab(AssetBundleClass.AndysModsNetworkPrefab);
            NetworkManager.Singleton.AddNetworkPrefab(AssetBundleClass.LethalTurretsNetworkPrefab);
            AndysModsPlugin.Log.LogInfo("AndysModsPlugin: Custom NetworkPrefabs were added to NetworkManager.");
        }
    }

    [HarmonyPatch(typeof(StartOfRound))]
    internal static class ModNetworkHandlerPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
        [HarmonyWrapSafe]
        static void SpawnNetworkHandler()
        {
            GameObject networkHandlerHost = Object.Instantiate(AssetBundleClass.AndysModsNetworkPrefab, Vector3.zero, Quaternion.identity);
            AndysModsPlugin.Log.LogInfo("AndysModsPlugin: Custom NetworkPrefab was created for client.");
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                AndysModsPlugin.Log.LogInfo("AndysModsPlugin: Custom NetworkPrefab was spawned on the host.");
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
            else if (networkHandlerHost.GetComponent<NetworkObject>() != null && networkHandlerHost.GetComponent<NetworkObject>().IsSpawned)
            {
                AndysModsPlugin.Log.LogInfo("AndysModsPlugin: Custom NetworkPrefab was despawned for client.");
                networkHandlerHost.GetComponent<NetworkObject>()?.Despawn();
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Turret), "Start")]
        [HarmonyWrapSafe]
        static void SpawnTurretsServer(Turret __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                ModNetworkHandler.Instance.ReplaceTurretServerRpc(__instance.NetworkObjectId);
            }
        }

    }

    /**
     * ModNetworkHandler for my mods.
     */
    public class ModNetworkHandler : NetworkBehaviour
    {

        public static ModNetworkHandler Instance { get; private set; }

        public override void OnNetworkSpawn()
        {
            AndysModsPlugin.Log.LogInfo("AndysModsPlugin: spawned ModNetworkHandler for sounds!");
            Instance = this;
            base.OnNetworkSpawn();
        }

        /**
         * Server RPC method to replace game spawned turrets with modded versions on the host and clients
         */
        [ServerRpc]
        public void ReplaceTurretServerRpc(ulong turretId)
        {
            if (!ModManager.LethalTurrets.enabled.Value)
            {
                return;
            }
            ReplaceTurretClientRpc(turretId);
        }

        [ClientRpc]
        public void ReplaceTurretClientRpc(ulong turretId)
        {
            if (!ModManager.LethalTurrets.enabled.Value)
            {
                return;
            }
            ReplaceTurret(turretId);
        }

        private void ReplaceTurret(ulong turretId)
        {
            AndysModsPlugin.Log.LogInfo($"Lethal Turrets: replacing turret with a more lethal version {GameNetworkManager.Instance.localPlayerController?.playerUsername}.");
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(turretId, out NetworkObject turretObject);
            Turret turret = turretObject.gameObject.GetComponentInChildren<Turret>();
            if (turret == null)
            {
                AndysModsPlugin.Log.LogInfo($"Lethal Turrets: can't spawn a NULL turret {GameNetworkManager.Instance.localPlayerController?.playerUsername}, turretID: {turretId}.");
                return;
            }
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                GameObject networkHandlerHost = Instantiate(AssetBundleClass.LethalTurretsNetworkPrefab, turret.gameObject.transform.position, turret.gameObject.transform.rotation);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn(true);
                networkHandlerHost.GetComponent<LethalTurretBehaviour>().SpawnTurretServerRpc(turret.NetworkObjectId);
            }
        }

        /**
         * Server RPC method for Optimal Sells
         */
        [ServerRpc(RequireOwnership = false)]
        public void SellScrapServerRpc(ulong[] soldItems)
        {
            SellScrapClientRpc(soldItems);
        }

        [ClientRpc]
        public void SellScrapClientRpc(ulong[] soldItems)
        {
            SellScrapLocally(soldItems);
        }

        public void SellScrapLocally(ulong[] soldItems)
        {
            AndysModsPlugin.Log.LogInfo("Optimal Sell: selling items to the company.");
            DepositItemsDesk desk = FindObjectOfType<DepositItemsDesk>();
            soldItems.Do(itemId =>
            {
                NetworkObject item = PlaceObjectOnDesk(itemId, desk);
                desk?.AddObjectToDeskServerRpc(item);
            });
        }

        private NetworkObject PlaceObjectOnDesk(ulong itemId, DepositItemsDesk desk)
        {
            Vector3 positionOffset = RoundManager.RandomPointInBounds(desk.triggerCollider.bounds);
            positionOffset.y = desk.triggerCollider.bounds.min.y;
            if (Physics.Raycast(new Ray(positionOffset + Vector3.up * 3f, Vector3.down), out var hitInfo, 8f, 1048640, QueryTriggerInteraction.Collide))
            {
                positionOffset = hitInfo.point;
            }
            NetworkObject item = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemId].gameObject.GetComponent<NetworkObject>();
            GrabbableObject grabbyItem = item.gameObject.GetComponentInChildren<GrabbableObject>();
            positionOffset.y += grabbyItem.itemProperties.verticalOffset;
            positionOffset = desk.deskObjectsContainer.transform.InverseTransformPoint(positionOffset);
            grabbyItem.EnablePhysics(enable: true);
            grabbyItem.EnableItemMeshes(enable: true);
            grabbyItem.isHeld = false;
            grabbyItem.isPocketed = false;
            grabbyItem.heldByPlayerOnServer = false;
            grabbyItem.parentObject = null;
            grabbyItem.transform.SetParent(desk.deskObjectsContainer.transform, worldPositionStays: true);
            grabbyItem.startFallingPosition = grabbyItem.transform.localPosition;
            grabbyItem.transform.localScale = grabbyItem.originalScale;
            grabbyItem.transform.localPosition = positionOffset;
            grabbyItem.targetFloorPosition = positionOffset;
            grabbyItem.fallTime = 0f;
            grabbyItem.OnPlaceObject();
            return item;
        }

        /**
         * Server RPC method for Rare Bonk mod.
         */
        [ServerRpc(RequireOwnership = false)]
        public void PlayBonkServerRpc(ulong shovelId)
        {
            if (!ModManager.RareBonk.enabled.Value)
            {
                return;
            }
            PlayBonkClientRpc(shovelId);
        }

        /**
         * Client RPC method for Rare Bonk mod.
         */
        [ClientRpc]
        public void PlayBonkClientRpc(ulong shovelId)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[shovelId].TryGetComponent(out Shovel shovel);
            AndysModsPlugin.Log.LogInfo($"Rare Bonk: playing BONK with found shovel {shovel}!");
            RoundManager.PlayRandomClip(shovel.shovelAudio, AssetBundleClass.BonkHitSfx);
        }

    }

}