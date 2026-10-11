using BepInEx;
using CodeRebirth;
using CodeRebirth.src.Content.Enemies;
using CodeRebirth.src.Content.Maps;
using CodeRebirth.src.Content.Unlockables;
using CodeRebirth.src.Util;
using Dawn;
using Dawn.Utils;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace CoolerOxydeShip
{
    [BepInDependency(LethalNetworkAPI.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CodeRebirth.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(PluginInfo.modGUID, PluginInfo.modName, PluginInfo.modVersion)]
    public class CoolerOxydeShipPatch : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.modGUID);
        public static BepInEx.Logging.ManualLogSource Logger;
        public static CoolerOxydeShipPatch Instance;
        internal static readonly Dictionary<OxydeCrashingShip, Vector3> TargetPositions = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            Logger = base.Logger;
            ConfigHandler.Init(Config); 
            OxydeShipNetwork.Init();
            harmony.PatchAll();
            Logger.LogInfo($"Plugin is loaded!");
        }
        [HarmonyPatch(typeof(OxydeCrashingShip), nameof(OxydeCrashingShip.OnNetworkSpawn))]
        internal static class OxydeCrashingShipSpawnPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(OxydeCrashingShip __instance)
            {
                bool oxydeUnlocked = false;
                if (!LethalContent.Moons.TryGetValue(CodeRebirthMoonKeys.Oxyde, out DawnMoonInfo moonInfo) || moonInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase() == TerminalPurchaseResult.SuccessPurchaseResult.Success() || moonInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase() == TerminalPurchaseResult.SuccessPurchaseResult.Hidden())
                {
                    oxydeUnlocked = true; //Triggered if Oxyde has already been unlocked
                }
                if (__instance.IsServer)
                {
                    if (oxydeUnlocked && new System.Random(StartOfRound.Instance.randomMapSeed).NextDouble() * 100.0 < ConfigHandler.chanceToDespawn.Value)
                    {
                        __instance.NetworkObject.Despawn(true);
                        return false;
                    }
                }
                Vector3 spawnPosition = __instance.transform.position;
                if (__instance.IsServer)
                {
                    TargetPositions[__instance] = spawnPosition;
                    OxydeShipNetwork.SendShipPosition(__instance.NetworkObjectId, spawnPosition);
                }
                __instance.transform.position = new Vector3(9999f, 9999f, 9999f);
                if (!__instance.IsServer) return false;
                __instance.StartCoroutine(__instance.WaitToDoYourThing(spawnPosition, Mathf.Lerp(ConfigHandler.shipWaitTimeMin.Value, ConfigHandler.shipWaitTimeMax.Value, (float)new System.Random(StartOfRound.Instance.randomMapSeed).NextDouble())));
                Logger.LogDebug($"Ship spawned at {spawnPosition}");
                return false;
            }
        }
        [HarmonyPatch(typeof(OxydeCrashingShip), "OnImpact")]
        internal static class OxydeCrashingShipImpactPatch
        {
            [HarmonyPostfix]
            private static void Postfix(OxydeCrashingShip __instance)
            {
                if (!__instance.IsServer) return;
                Vector3 actualCrashPosition = __instance.transform.position;
                Vector3 targetPosition;
                if (TargetPositions.TryGetValue(__instance, out Vector3 storedTargetPosition))
                {
                    targetPosition = storedTargetPosition;
                }
                else
                {
                    targetPosition = actualCrashPosition;
                }
                Logger.LogDebug($"Ship crashed at {actualCrashPosition}");
                Logger.LogDebug($"Ship target was {targetPosition}");
                bool oxydeUnlocked = false;
                if (!LethalContent.Moons.TryGetValue(CodeRebirthMoonKeys.Oxyde, out DawnMoonInfo moonInfo) || moonInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase() == TerminalPurchaseResult.SuccessPurchaseResult.Success() || moonInfo.DawnPurchaseInfo.PurchasePredicate.CanPurchase() == TerminalPurchaseResult.SuccessPurchaseResult.Hidden())
                {
                    oxydeUnlocked = true;
                }
                if (ConfigHandler.shouldSpawnScrap.Value)
                { //If Oxyde Locked still, and config, dont spawn scraps
                    if (!oxydeUnlocked && !ConfigHandler.shouldSpawnScrapOnFirstCrash.Value)
                    {
                        //Don't spawn stuff
                    }
                    else
                    {
                        SpawnLevelScrap(targetPosition);
                    }
                }
                if (ConfigHandler.shouldSpawnMasked.Value)
                {
                    if (!oxydeUnlocked && !ConfigHandler.shouldSpawnMaskedOnFirstCrash.Value) //Same with Masked
                    {
                        //Don't spawn stuff
                    }
                    else
                    {
                        SpawnMasked(targetPosition);
                    }
                }
            }
            public static void StopAlarm(OxydeCrashingShip shipRef)
            { 
                foreach (AudioSource audio in shipRef.GetComponentsInChildren<AudioSource>(true)) 
                {
                    if (audio.name != "ShipAlarm") continue; //I FOUND YOU you little rascal
                    audio.Stop();
                    audio.mute = true;
                    audio.enabled = false;
                    Logger.LogInfo("Ship alarm muted!");
                }
            }
            private static void SpawnLevelScrap(Vector3 spawnPosition)
            {
                string[] blacklistedOrWhitelistedScrap = ConfigHandler.scrapItemList.Value.Split(',').Select(s => s.Trim().ToLowerInvariant()).ToArray();
                bool useWhitelist = ConfigHandler.scrapUseWhitelist.Value;
                List<SpawnableItemWithRarity> scrapPool = new();
                foreach (SpawnableItemWithRarity scrap in RoundManager.Instance.currentLevel.spawnableScrap)
                {
                    if (scrap.rarity <= 0 || scrap.spawnableItem == null) continue;
                    string itemName = scrap.spawnableItem.itemName.ToLowerInvariant();
                    if (!useWhitelist && !blacklistedOrWhitelistedScrap.Contains(itemName))
                    {
                        scrapPool.Add(scrap);
                    }
                    else if (useWhitelist && blacklistedOrWhitelistedScrap.Contains(itemName))
                    {
                        scrapPool.Add(scrap);
                    }
                }
                if (scrapPool.Count == 0)
                {
                    Logger.LogWarning("WARNING: No scrap available after applying blacklist/whitelist filters :c or the level has no scrap???");
                    return;
                }
                System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);
                int numberOfScrapToSpawn = random.Next(ConfigHandler.scrapMin.Value, ConfigHandler.scrapMax.Value + 1);
                Logger.LogDebug($"Ship spawning {numberOfScrapToSpawn} scrap.");
                for (int i = 0; i < numberOfScrapToSpawn; i++)
                {
                    SpawnableItemWithRarity chosenItemWithRarity = random.NextItem(scrapPool);
                    Item item = chosenItemWithRarity.spawnableItem;
                    if (item == null || item.spawnPrefab == null) continue;
                    float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
                    float distance = Mathf.Lerp(ConfigHandler.scrapDistanceMin.Value, ConfigHandler.scrapDistanceMax.Value, (float)random.NextDouble());
                    Vector3 itemPosition = spawnPosition + new Vector3(Mathf.Cos(angle) * distance, 1f, Mathf.Sin(angle) * distance);
                    Vector3 itemTargetPosition = itemPosition; //Seperate so for the spawning we still have another coordinate to place it higher up
                    if (Physics.Raycast(itemPosition + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                    {
                        itemTargetPosition = hit.point + Vector3.up * item.verticalOffset;
                    }
                    GameObject scrapObject = UnityEngine.Object.Instantiate(item.spawnPrefab, itemTargetPosition, Quaternion.identity, null);
                    GrabbableObject grabbableObject = scrapObject.GetComponent<GrabbableObject>();
                    if (grabbableObject == null)
                    {
                        Logger.LogWarning($"Ship spawned {item.name}, but it has no GrabbableObject component so its being killed");
                        UnityEngine.Object.Destroy(scrapObject); //Idk a better way to do this 
                        continue;
                    }
                    NetworkObject networkObject = grabbableObject.NetworkObject;
                    if (networkObject == null)
                    {
                        Logger.LogWarning($"Ship spawned {item.name}, but it has no NetworkObject so its being killed");
                        UnityEngine.Object.Destroy(scrapObject);
                        continue;
                    }
                    networkObject.Spawn(false);
                    int value = (int)(Mathf.Lerp(item.minValue, item.maxValue, (float)random.NextDouble()) * RoundManager.Instance.scrapValueMultiplier * ConfigHandler.scrapValueMultiplier.Value);
                    CodeRebirthUtils.Instance.SyncScanNodeParentRotationsAndFallRpc(new NetworkBehaviourReference(grabbableObject), itemPosition, itemTargetPosition, Quaternion.Euler(item.restingRotation), value);
                }
            }
            private static void SpawnMasked(Vector3 spawnPosition) //Mostly the same as the scrap spawning one
            {
                if (!StartOfRound.Instance.IsServer) return;
                EnemyType? maskedEnemyType = null;
                foreach (SpawnableEnemyWithRarity enemyWithRarity in RoundManager.Instance.currentLevel.Enemies)
                {
                    if (enemyWithRarity.enemyType.enemyName == "Masked")
                    {
                        maskedEnemyType = enemyWithRarity.enemyType;
                        break;
                    }
                }
                if (maskedEnemyType == null)
                {
                    Logger.LogWarning("Could not find Masked EnemyType??");
                    return;
                }
                System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed);
                int numberOfMasked = random.Next(ConfigHandler.maskedMin.Value, ConfigHandler.maskedMax.Value + 1);
                Logger.LogDebug($"Ship spawning {numberOfMasked} Masked.");
                for (int i = 0; i < numberOfMasked; i++)
                {
                    float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
                    float distance = Mathf.Lerp(ConfigHandler.maskedDistanceMin.Value, ConfigHandler.maskedDistanceMax.Value, (float)random.NextDouble());
                    Vector3 maskedPosition = spawnPosition + new Vector3(Mathf.Cos(angle) * distance, 2f, Mathf.Sin(angle) * distance);
                    if (Physics.Raycast(maskedPosition, Vector3.down, out RaycastHit hit, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                    {
                        maskedPosition = hit.point;
                    }
                    GameObject maskedObject = UnityEngine.Object.Instantiate(maskedEnemyType.enemyPrefab, maskedPosition, Quaternion.identity);
                    NetworkObject networkObject = maskedObject.GetComponent<NetworkObject>();
                    if (networkObject == null)
                    {
                        Logger.LogWarning("Masked prefab has no NetworkObject??");
                        UnityEngine.Object.Destroy(maskedObject);
                        continue;
                    }
                    networkObject.Spawn(true);
                }
            }
        }
        [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.GrabItem))]
        public static class OxydeTabletGrabPatch
        {
            private const float ShipSearchRange = 30f;
            public static void Postfix(GrabbableObject __instance)
            {
                if (__instance.itemProperties == null) return;
                if (__instance.itemProperties.itemName != "Oxyde Tablet") return;
                if (__instance.playerHeldBy == null) return;
                if (!__instance.playerHeldBy.IsLocalPlayer()) return;
                Logger.LogDebug("Oxyde Tablet detected!");
                Vector3 tabletPosition = __instance.transform.position;
                ulong closestShipNetworkId = 0;
                bool foundShip = false;
                float closestDistance = ShipSearchRange;
                foreach (KeyValuePair<ulong, Vector3> entry in OxydeShipNetwork.ShipPositions)
                {
                    float distance = Vector3.Distance(tabletPosition, entry.Value);
                    if (distance <= closestDistance)
                    {
                        closestDistance = distance;
                        closestShipNetworkId = entry.Key;
                        foundShip = true;
                    }
                }
                if (foundShip)
                {
                    OxydeShipNetwork.SendMuteShip(closestShipNetworkId);
                    Logger.LogInfo($"Oxyde Tablet picked up near ship. Distance from ship: {closestDistance}");
                }
                else
                {
                    Logger.LogDebug("Oxyde Tablet was picked up, but no ship was found nearby :c");
                }
            }
        }
        [HarmonyPatch(typeof(Guillotine), nameof(Guillotine.PseudoKillPlayerServerRpc))]
        public static class GuillotineKillPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(PlayerControllerReference playerControllerReference)
            {
                if (!ConfigHandler.mistressAlwaysKill.Value) return true;
                if (!NetworkManager.Singleton.IsServer) return true; //Skip if client, config etc.
                PlayerControllerB player = playerControllerReference;
                if (player == null) return false;
                Logger.LogDebug($"Mistress executing player {player.playerClientId}");
                OxydeShipNetwork.SendMistressKill(player.playerClientId);
                return false;
            }
        }
        [HarmonyPatch(typeof(HauntedTeddyBear), "get_SpawnChance")]
        public static class HauntedTeddyBearPatch
        {
            [HarmonyPostfix]
            private static void Postfix(ref float __result)
            {
                if (ConfigHandler.killEugene.Value)
                {
                    __result = -1f; //Should just fuck up the spawn chance so it doesn't spawn
                }
            }
        }
        [HarmonyPatch(typeof(MoneyCounter), "CalculateCoinPenalty")]
        public static class MoneyCounterPenaltyPatch
        {
            [HarmonyPostfix]
            private static void Postfix(MoneyCounter __instance, int playersDead)
            {
                if (!ConfigHandler.debtOnAnyPenalty.Value) return;
                if (!NetworkManager.Singleton.IsServer) return; //Only the host should need to run it i think
                if (playersDead <= 0) return;
                __instance.RemoveMoney(playersDead * ConfigHandler.penaltyPerPlayer.Value);
            }
        }
    }
}
