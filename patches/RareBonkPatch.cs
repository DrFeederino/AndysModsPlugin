using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace AndysModsPlugin.patches
{
    [HarmonyPatch(typeof(Shovel))]
    [HarmonyWrapSafe]
    internal static class RareBonkPatch
    {
        static readonly System.Random random = new();
        static readonly AssetBundle BonkAssetBundle = AssetBundle.LoadFromFile($"{AndysModsPlugin.PluginPath}bonk");
        public static readonly AudioClip[] BonkHitSfx = BonkAssetBundle.LoadAssetWithSubAssets<AudioClip>("bonk.mp3");
        public static readonly GameObject BonkNetworkPrefab = BonkAssetBundle.LoadAsset<GameObject>("BonkNetworkHandler");
        public const int BonkShovelForce = 100; // should 100 be enough to BONK?
        public const int OriginalShovelForce = 1; 

        [HarmonyPatch("HitShovel")]
        [HarmonyPrefix]
        static void PatchBonkShovel(Shovel __instance, ref AudioClip[] __state)
        {
            if (!ModSettings.RareBonkMod.IsEnabled)
            {
                return;
            }
            int bonkChance = random.Next(1, 100);
            if (__instance != null && bonkChance <= 10 && BonkHitSfx != null && BonkHitSfx.Length != 0)
            {
                AndysModsPlugin.Log.LogInfo($"Rare Bonk: BONK by {__instance.playerHeldBy.playerUsername}!");
                __instance.shovelHitForce = BonkShovelForce;
                __state = __instance.hitSFX;
                __instance.hitSFX = BonkHitSfx;
                BonkNetworkHandler.Instance?.PlayBonkServerRpc(__instance.NetworkObjectId);
            }
        }

        [HarmonyPatch("HitShovel")]
        [HarmonyPostfix]
        static void RemovePatchShovel(Shovel __instance, ref AudioClip[] __state)
        {
            if (!ModSettings.RareBonkMod.IsEnabled)
            {
                return;
            }
            if (__instance != null && __instance.shovelHitForce != OriginalShovelForce && __state != null && __state.Length != 0)
            {
                __instance.shovelHitForce = OriginalShovelForce;
                __instance.hitSFX = __state;
                AndysModsPlugin.Log.LogInfo($"Rare Bonk: restored original shovelHitForce.");
            }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void AddBonkNetworkPrefab()
        {
            BonkNetworkPrefab.AddComponent<BonkNetworkHandler>();
            NetworkManager.Singleton.AddNetworkPrefab(BonkNetworkPrefab);
            AndysModsPlugin.Log.LogInfo($"Rare Bonk: added BonkNetworkHandler to NetworkManager.");
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHandler()
        {
            GameObject networkHandlerHost = Object.Instantiate(BonkNetworkPrefab, Vector3.zero, Quaternion.identity);
            networkHandlerHost.GetComponent<NetworkObject>().Spawn();
        }

    }

    public class BonkNetworkHandler : NetworkBehaviour
    {
        public static BonkNetworkHandler Instance { get; private set; }

        public override void OnNetworkSpawn()
        {
            AndysModsPlugin.Log.LogInfo("Rare Bonk: spawned network handler for sounds!");
            Instance = this;
            base.OnNetworkSpawn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void PlayBonkServerRpc(ulong shovelId)
        {
            if (!ModSettings.RareBonkMod.IsEnabled)
            {
                return;
            }
            PlayBonkClientRpc(shovelId);
        }

        [ClientRpc]
        public void PlayBonkClientRpc(ulong shovelId)
        {
            AndysModsPlugin.Log.LogInfo("Rare Bonk: client received BONK!");
            NetworkManager.Singleton.SpawnManager.SpawnedObjects[shovelId].TryGetComponent(out Shovel shovel);
            AndysModsPlugin.Log.LogInfo($"Rare Bonk: found shovel {shovel}!");
            RoundManager.PlayRandomClip(shovel.shovelAudio, RareBonkPatch.BonkHitSfx);
        }

    }

}