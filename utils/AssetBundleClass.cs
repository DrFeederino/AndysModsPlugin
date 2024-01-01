using AndysModsPlugin.mods.LethalTurrets;
using AndysModsPlugin.mods.RareBonk;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace AndysModsPlugin.utils
{
    internal static class AssetBundleClass
    {
        public static readonly AssetBundle AndysModsAssetBundle = AssetBundle.LoadFromFile($"{AndysModsPlugin.PluginPath}andysmods");
        public static readonly AudioClip[] BonkHitSfx = AndysModsAssetBundle.LoadAssetWithSubAssets<AudioClip>("bonk.mp3");
        public static readonly GameObject AndysModsNetworkPrefab = AndysModsAssetBundle.LoadAsset<GameObject>("AndysModsNetworkHandler");
        public static readonly GameObject LethalTurretsNetworkPrefab = AndysModsAssetBundle.LoadAsset<GameObject>("LethalTurretsNetworkPrefab");

    }
    
}