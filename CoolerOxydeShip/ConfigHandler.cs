using BepInEx.Configuration;
using UnityEngine;

namespace CoolerOxydeShip
{
    internal class ConfigHandler : MonoBehaviour
    {
        public static ConfigEntry<float> chanceToDespawn;
        public static ConfigEntry<bool> shouldSpawnScrap;
        public static ConfigEntry<bool> shouldSpawnScrapOnFirstCrash;
        public static ConfigEntry<int> scrapMin;
        public static ConfigEntry<int> scrapMax;
        public static ConfigEntry<float> scrapValueMultiplier;
        public static ConfigEntry<float> scrapDistanceMin;
        public static ConfigEntry<float> scrapDistanceMax;
        public static ConfigEntry<bool> shouldSpawnMasked;
        public static ConfigEntry<string> scrapItemList;
        public static ConfigEntry<bool> scrapUseWhitelist;
        public static ConfigEntry<bool> shouldSpawnMaskedOnFirstCrash;
        public static ConfigEntry<int> maskedMin;
        public static ConfigEntry<int> maskedMax;
        public static ConfigEntry<float> maskedDistanceMin;
        public static ConfigEntry<float> maskedDistanceMax;
        public static ConfigEntry<bool> muteShipOnTabletCollection;
        public static ConfigEntry<float> shipWaitTimeMin;
        public static ConfigEntry<float> shipWaitTimeMax;
        public static ConfigEntry<bool> debtOnAnyPenalty;
        public static ConfigEntry<int> penaltyPerPlayer;
        public static ConfigEntry<bool> killEugene;
        public static ConfigEntry<bool> mistressAlwaysKill;
        public static void Init(ConfigFile config)
        {
            chanceToDespawn = config.Bind<float>("Ship", "Chance to Despawn", 33f, new ConfigDescription("An additional chance that if the ship 'spawns' (See the CodeRebirth spawn curve config for it), it will despawn anyways. Only applies once Oxyde is unlocked. This can be used to make the Crashing Ship less common once Oxyde is unlocked. A higher value increases the chance it will despawn, making it rarer once Oxyde is unlocked. At 0, the chance will never decrease. At 100, the ship will never spawn once Oxyde is unlocked like in Base CodeRebirth.", new AcceptableValueRange<float>(0, 100)));
            shipWaitTimeMin = config.Bind<float>("Ship", "Minimum Wait Time", 180f, new ConfigDescription("The minimum time (in seconds) the ship will wait before crashing."));
            shipWaitTimeMax = config.Bind<float>("Ship", "Maximum Wait Time", 360f, new ConfigDescription("The maximum time (in seconds) the ship will wait before crashing."));
            muteShipOnTabletCollection = config.Bind<bool>("Ship", "Mute Ship on Tablet Collection", true, new ConfigDescription("If true, the alarm noise from the ship will be muted when the tablet is collected."));
            shouldSpawnScrap = config.Bind<bool>("Scrap Spawning", "Should Scrap Spawn", true, new ConfigDescription("If true, extra scrap will spawn when a ship crashes. If false, no extra scraps will spawn."));
            shouldSpawnScrapOnFirstCrash = config.Bind<bool>("Scrap Spawning", "Should Scrap Spawn on First Crash", false, new ConfigDescription("If true, extra scrap will always spawn at a crashed ship. If false, then only crashed ships encountered after Oxyde has been unlocked will spawn scrap."));
            scrapMin = config.Bind<int>("Scrap Spawning", "Minimum Scrap to Spawn", 3, new ConfigDescription("The minimum number of scrap items (inclusive) to spawn when a ship crashes. Shouldn't be higher than the maximum.", new AcceptableValueRange<int>(0, 20)));
            scrapMax = config.Bind<int>("Scrap Spawning", "Maximum Scrap to Spawn", 6, new ConfigDescription("The maximum number of scrap items (inclusive) to spawn when a ship crashes. Shouldn't be lower than the minimum.", new AcceptableValueRange<int>(1, 20)));
            scrapValueMultiplier = config.Bind<float>("Scrap Spawning", "Scrap Value Multiplier", 1f, new ConfigDescription("A multiplier for the value of scrap spawned when a ship crashes. This should stack with existing multipliers, such as moon scrap value multipliers or weather multipliers with WeatherRegistry.", new AcceptableValueRange<float>(0, 10)));
            scrapDistanceMin = config.Bind<float>("Scrap Spawning", "Minimum Scrap Distance", 7f, new ConfigDescription("The minimum distance from the ship's crash site that scrap can spawn. A higher value will make scrap spawn further away from the crash site. Shouldn't be higher than the maximum.", new AcceptableValueRange<float>(0, 30)));
            scrapDistanceMax = config.Bind<float>("Scrap Spawning", "Maximum Scrap Distance", 12f, new ConfigDescription("The maximum distance from the ship's crash site that scrap can spawn. A higher value will make scrap spawn further away from the crash site. Shouldn't be lower than the minimum.", new AcceptableValueRange<float>(0, 30)));
            scrapItemList = config.Bind<string>("Scrap Spawning", "Scrap Item Blacklist", "", new ConfigDescription("A comma-separated list of scrap items that can't spawn when a ship crashes."));
            scrapUseWhitelist = config.Bind<bool>("Scrap Spawning", "Blacklist is Whitelist", false, new ConfigDescription("If true, only the scrap items in the 'Scrap Item Blacklist' can spawn. If false, all scrap items EXCEPT those in the list will spawn."));
            shouldSpawnMasked = config.Bind<bool>("Masked Spawning", "Should Masked Spawn", true, new ConfigDescription("If true, Masked enemies will spawn when a ship crashes."));
            shouldSpawnMaskedOnFirstCrash = config.Bind<bool>("Masked Spawning", "Should Masked Spawn on First Crash", false, new ConfigDescription("If true, Masked enemies will always spawn at a crashed ship. If false, then only crashed ships encountered after Oxyde has been unlocked will spawn Masked enemies."));
            maskedMin = config.Bind<int>("Masked Spawning", "Minimum Masked to Spawn", 1, new ConfigDescription("The minimum number of Masked enemies (inclusive) to spawn when a ship crashes. Shouldn't be higher than the maximum.", new AcceptableValueRange<int>(0, 20)));
            maskedMax = config.Bind<int>("Masked Spawning", "Maximum Masked to Spawn", 3, new ConfigDescription("The maximum number of Masked enemies (inclusive) to spawn when a ship crashes. Shouldn't be lower than the minimum.", new AcceptableValueRange<int>(1, 20)));
            maskedDistanceMin = config.Bind<float>("Masked Spawning", "Minimum Masked Distance", 7f, new ConfigDescription("The minimum distance from the ship's crash site that Masked enemies can spawn. A higher value will make Masked enemies spawn further away from the crash site. Shouldn't be higher than the maximum. Be careful to not make it too low, or they might be killed by the initial explosion.", new AcceptableValueRange<float>(0, 30)));
            maskedDistanceMax = config.Bind<float>("Masked Spawning", "Maximum Masked Distance", 12f, new ConfigDescription("The maximum distance from the ship's crash site that Masked enemies can spawn. A higher value will make Masked enemies spawn further away from the crash site. Shouldn't be lower than the minimum. Be carefful to not make it too low, or they might be killed by the initial explosion.", new AcceptableValueRange<float>(0, 30)));
            debtOnAnyPenalty = config.Bind<bool>("Debt", "Debt on Any Penalty", true, new ConfigDescription("If true, Debt will be added to the Denomination Analyser for any dead crew, not just during a wipe. Note that this debt will stack on TOP of the wipe debt!"));
            penaltyPerPlayer = config.Bind<int>("Debt", "Penalty per Player", 2, new ConfigDescription("If 'Debt on Any Penalty' is true, this is the amount of debt added for each dead player.", new AcceptableValueRange<int>(0, 999)));
            killEugene = config.Bind<bool>("Misc", "Kill Eugene", true, new ConfigDescription("If true, Eugene will no longer randomly be forcibly-unlocked when landing the ship by setting the chance to 0%."));
            mistressAlwaysKill = config.Bind<bool>("Misc", "Mistress Always Kills", true, new ConfigDescription("If true, the Mistress will always kill a player when they catch them, instead of doing the whole beheading thing."));
        }
    }
}
