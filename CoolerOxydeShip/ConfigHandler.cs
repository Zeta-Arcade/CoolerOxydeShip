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
        public static ConfigEntry<int> minMaskedPerPlayer;
        public static ConfigEntry<int> maxMaskedPerPlayer;
        public static ConfigEntry<int> maskedCap;
        public static ConfigEntry<float> maskedDistanceMin;
        public static ConfigEntry<float> maskedDistanceMax;
        public static ConfigEntry<bool> muteShipOnTabletCollection;
        public static ConfigEntry<float> shipWaitTimeMin;
        public static ConfigEntry<float> shipWaitTimeMax;
        public static ConfigEntry<bool> debtOnAnyPenalty;
        public static ConfigEntry<int> penaltyPerPlayer;
        public static ConfigEntry<int> coinsPerKill;
        public static ConfigEntry<bool> noExtraDebtOnWipe;
        public static ConfigEntry<bool> noDebtShield;
        public static ConfigEntry<float> merchantStealMultiplier;
        public static ConfigEntry<float> merchantSpeedMultiplier;
        public static ConfigEntry<float> maxTipDivisor;
        public static ConfigEntry<float> oxydeFlareDuration;
        public static ConfigEntry<bool> preventBloodInOrbit;
        public static ConfigEntry<bool> preventBloodInCompanyMoons;

        public static ConfigEntry<bool> killEugene;
        public static ConfigEntry<bool> mistressAlwaysKill;
        public static ConfigEntry<float> rabbitDistanceLimit;
        public static ConfigEntry<bool> oxydeDeleteSpawnedGear;
        public static void Init(ConfigFile config)
        {
            //Crashing Ship
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
            maskedMax = config.Bind<int>("Masked Spawning", "Maximum Masked to Spawn", 2, new ConfigDescription("The maximum number of Masked enemies (inclusive) to spawn when a ship crashes. Shouldn't be lower than the minimum.", new AcceptableValueRange<int>(1, 20)));
            minMaskedPerPlayer = config.Bind<int>("Masked Spawning", "Masked Min Per Player Increase", 0, new ConfigDescription("Adds this value to the Min Masked spawn count, for each player in the lobby after the first. Useful if you want to increase the amount of Masked that spawn with lobby size", new AcceptableValueRange<int>(0, 10)));
            maxMaskedPerPlayer = config.Bind<int>("Masked Spawning", "Masked Max Per Player Increase", 0, new ConfigDescription("Adds this value to the Max Masked spawn count, for each player in the lobby after the first. Useful if you want to increase the amount of Masked that spawn with lobby size", new AcceptableValueRange<int>(0, 10)));
            maskedCap = config.Bind<int>("Masked Spawning", "Masked Cap", 99, new ConfigDescription("The maximum amount of Masked that can spawn from the ship, overriding the above values. No more Masked can spawn than this value from one crashed ship.", new AcceptableValueRange<int>(1, 100)));
            maskedDistanceMin = config.Bind<float>("Masked Spawning", "Minimum Masked Distance", 7f, new ConfigDescription("The minimum distance from the ship's crash site that Masked enemies can spawn. A higher value will make Masked enemies spawn further away from the crash site. Shouldn't be higher than the maximum. Be careful to not make it too low, or they might be killed by the initial explosion.", new AcceptableValueRange<float>(0, 30)));
            maskedDistanceMax = config.Bind<float>("Masked Spawning", "Maximum Masked Distance", 12f, new ConfigDescription("The maximum distance from the ship's crash site that Masked enemies can spawn. A higher value will make Masked enemies spawn further away from the crash site. Shouldn't be lower than the minimum. Be carefful to not make it too low, or they might be killed by the initial explosion.", new AcceptableValueRange<float>(0, 30)));
            //Debt
            debtOnAnyPenalty = config.Bind<bool>("Debt", "Debt on Any Penalty", true, new ConfigDescription("If true, Debt will also be added to the Denomination Analyser for any dead crew, not just during a wipe."));
            penaltyPerPlayer = config.Bind<int>("Debt", "Penalty per Player", 2, new ConfigDescription("If 'Debt on Any Penalty' is true, this is the amount of debt added for each dead player.", new AcceptableValueRange<int>(0, 999)));
            coinsPerKill = config.Bind<int>("Debt", "Coins per Kill", 1, new ConfigDescription("The amount of coins added to the Denomination Analyser for each player killed by the Debt Collector. When Debt is calculated at the end of the round with 'Debt on Any Penalty', players killed by the Debt Collector will not be counted. Set to 0 to disable.", new AcceptableValueRange<int>(0, 999)));
            noExtraDebtOnWipe = config.Bind<bool>("Debt", "No Extra Debt on Wipe", true, new ConfigDescription("If true, the above extra debt will not be added to the Denomination Analyser when a wipe occurs. If false, you'll be getting 2 punishments!! (1 from this mod, the other from base CodeRebirth)"));
            noDebtShield = config.Bind<bool>("Debt", "No Debt Shield", false, new ConfigDescription("By default, the Denomination Anaylser has a shield mechanic, where you cannot enter Debt the first time your counter gets that low on a save file. This can be abused to, for example, steal a high value item from the store without much punishment. When this setting is enabled, any saves loaded with this on will be marked as 'Having entered Debt before', meaning any losses can immediately put you in the negative, making it behave like how you would expect it to."));
            //Merchant
            merchantStealMultiplier = config.Bind<float>("Merchant", "Merchant Steal Multiplier", 2f, new ConfigDescription("By default, when you shoplift from the Merchant, the amount of coins you will lose is the cost of the item multiplied by this value.", new AcceptableValueRange<float>(0, 10)));
            maxTipDivisor = config.Bind<float>("Merchant", "Maximum Tip Effectiveness", 1.3f, new ConfigDescription("Tipping the Merchant on Oxyde reduces the next quota's increase. By default, at 1.3, donating 100 coins results in a 23% quota reduction. By increasing this value, you can make it reduce the quota even further E.g. 1 = 0%, 1.3 = 23%, 1.5 = 33%, 2 = 50%, 3 = 66%, 4 = 75%, 10 = 90%, 20 = 95%", new AcceptableValueRange<float>(1, 20)));
            merchantSpeedMultiplier = config.Bind<float>("Merchant", "Merchant Speed Multiplier", 1f, new ConfigDescription("Increases the speed of the Merchant's animations, making transactions faster. Might break shit if too high. Tested up to 4x speed where it seemed fine!", new AcceptableValueRange<float>(1, 10)));
            //Enemies
            killEugene = config.Bind<bool>("Enemies", "Kill Eugene", true, new ConfigDescription("If true, Eugene will no longer randomly be forcibly-unlocked when landing the ship by setting the chance to 0%."));
            mistressAlwaysKill = config.Bind<bool>("Enemies", "Mistress Always Kills", true, new ConfigDescription("If true, the Mistress will always kill a player when they catch them, instead of doing the whole beheading thing."));
            rabbitDistanceLimit = config.Bind<float>("Enemies", "Rabbit Distance Limit", 40f, new ConfigDescription("The maximum distance you can actually trigger a Rabbit Magician's kill on a player. Set to 0 to disable, for the default infinite range.", new AcceptableValueRange<float>(0, 999)));
            //Items
            preventBloodInOrbit = config.Bind<bool>("Items", "Marrow Splitter - Prevent Blood in Orbit", true, new ConfigDescription("If true, The Marrow Splitter cannot generate blood when in Orbit."));
            preventBloodInCompanyMoons = config.Bind<bool>("Items", "Marrow Splitter - Prevent Blood in Company Moons", true, new ConfigDescription("If true, The Marrow Splitter cannot generate blood when at Company Moons (Including Gordion, Oxyde, and Galetry) since they could be revisited infinitely to charge it as much as you want."));
            //Oxyde
            oxydeFlareDuration = config.Bind<float>("Oxyde", "Oxyde Flare Duration", 20f, new ConfigDescription("The duration (in seconds) of the flare when you used a LethalThingsReloaded Flaregun on Oxyde. Set to 0 to disable.", new AcceptableValueRange<float>(0f, 60f)));
            oxydeDeleteSpawnedGear = config.Bind<bool>("Oxyde", "Delete Spawned Gear", true, new ConfigDescription("If true, MOST of the extra gear that can spawn on Oxyde 'E.g. Jetpacks, Nitro crates, Flashlights' won't spawn in."));
        }
    }
}
