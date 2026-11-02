using System;

namespace QuietHull.Data
{
    /// <summary>Item and card rarity. Prologue content is always <see cref="Common"/>.</summary>
    public enum Rarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }

    /// <summary>
    /// Mark. Equipment uses Mk I–V, military chassis and augments Mk I–VIII, worker androids and worker modules Mk I–XII.
    /// Prologue content is always <see cref="MkI"/>.
    /// </summary>
    public enum Mark
    {
        MkI = 1,
        MkII = 2,
        MkIII = 3,
        MkIV = 4,
        MkV = 5,
        MkVI = 6,
        MkVII = 7,
        MkVIII = 8,
        MkIX = 9,
        MkX = 10,
        MkXI = 11,
        MkXII = 12
    }

    /// <summary>Augment design lines: louder and stronger, or quieter and finer.</summary>
    public enum AugmentLine
    {
        /// <summary>Heavy military parts: strongest, loudest, hottest; draws aliens.</summary>
        Assault = 0,
        /// <summary>Clinic parts: balanced.</summary>
        Standard = 1,
        /// <summary>Precision civilian parts: weaker, quieter, cooler.</summary>
        Fine = 2,
        /// <summary>Illegal infiltration parts: suppress noise, heat and sight; very expensive.</summary>
        Shadow = 3
    }

    public enum Legality
    {
        Legal = 0,
        /// <summary>Military: needs a licence, or the black market at a markup.</summary>
        Restricted = 1,
        /// <summary>Black market only; confiscated if found in a Lane Authority scan.</summary>
        Contraband = 2
    }

    /// <summary>What a unit may be ordered to do in a fight.</summary>
    public enum CombatRole
    {
        /// <summary>Workers: defend themselves and company assets only; can't be ordered to attack.</summary>
        DefendOnly = 0,
        /// <summary>Military: can be ordered to attack.</summary>
        Combat = 1
    }

    /// <summary>
    /// Worker firmware. Older generations glitch: their asset-protection directive counts alien
    /// specimens (eggs, the Queen) as company property.
    /// </summary>
    public enum FirmwareGeneration
    {
        Gen1 = 1,
        Gen2 = 2,
        Gen3 = 3
    }

    public enum ItemCategory
    {
        MeleeWeapon = 0,
        RangedWeapon = 1,
        Throwable = 2,
        Tool = 3,
        Cargo = 4,
        Resource = 5,
        Material = 6,
        Blueprint = 7,
        Deployable = 8,
        Ammunition = 9,
        Vehicle = 10,
        Key = 11,
        WeaponMod = 12,
        Augment = 13,
        Mine = 14
    }

    /// <summary>What sets a mine off.</summary>
    public enum MineTrigger
    {
        /// <summary>Anything that walks within the trigger radius, including your own units if friendly fire is on.</summary>
        Proximity = 0,
        /// <summary>A directional charge: a cone toward the facing set when it was planted.</summary>
        Directional = 1,
        /// <summary>Only alien acid blood sets it off: safe for your units and synths.</summary>
        AcidSense = 2,
        /// <summary>Goes off by itself on a timer (decoys).</summary>
        Timed = 3,
        /// <summary>Goes off when the commander triggers it.</summary>
        Remote = 4
    }

    public enum FireMode
    {
        SemiAuto = 0,
        Burst = 1,
        Auto = 2
    }

    /// <summary>
    /// What a ship surface is made of. Acid-fused amalgam takes its extra yield from the surface
    /// the blood ate into. (The outer hull is never minable.)
    /// </summary>
    public enum SurfaceKind
    {
        Bulkhead = 0,
        Conduit = 1,
        Munitions = 2,
        Machinery = 3
    }

    /// <summary>Senses an alien can use. Matches the hive senses in the design doc.</summary>
    [Flags]
    public enum SenseFlags
    {
        None = 0,
        Hearing = 1 << 0,
        Vibration = 1 << 1,
        Vision = 1 << 2,
        Heat = 1 << 3,
        Scent = 1 << 4
    }

    public enum LightEmission
    {
        None = 0,
        Dim = 1,
        Bright = 2
    }

    public enum AttackKind
    {
        Melee = 0,
        Lob = 1,
        Projectile = 2
    }

    public enum TargetKind
    {
        NearestUnit = 0,
        Cargo = 1,
        Sentry = 2,
        UnitInOpen = 3,
        Shuttle = 4,
        Door = 5
    }

    public enum AlienBehaviour
    {
        Patrol = 0,
        HoldPosition = 1,
        Stream = 2
    }

    [Flags]
    public enum UnitAbility
    {
        None = 0,
        Move = 1 << 0,
        Interact = 1 << 1,
        Attack = 1 << 2,
        Patrol = 1 << 3,
        PushCart = 1 << 4,
        Throw = 1 << 5,
        RepairShuttle = 1 << 6,
        HackFabricator = 1 << 7,
        FireWeapon = 1 << 8,
        /// <summary>Standing order: ferry ammo boxes from the stockpile to any assigned sentry below a threshold.</summary>
        SupplySentries = 1 << 9,
        /// <summary>Standing order: defend a point or a company asset (workers can't attack).</summary>
        Guard = 1 << 10,
        /// <summary>Standing order: stay at a station and keep crafting a recipe (an ammo line).</summary>
        StaffStation = 1 << 11,
        /// <summary>Standing order: keep mining a seam or deposit and haul the output to a stockpile.</summary>
        Harvest = 1 << 12,
        /// <summary>Standing order: go and recharge below a charge threshold, then return to the previous order.</summary>
        AutoRecharge = 1 << 13
    }

    public enum ToolActionKind
    {
        CutBolt = 0,
        SealDoor = 1,
        RemoveDebris = 2,
        MineResource = 3,
        CutPanel = 4,
        DetachFixture = 5
    }

    [Flags]
    public enum WorkstationService
    {
        None = 0,
        RepairUnit = 1 << 0,
        Craft = 1 << 1,
        Fabricate = 1 << 2,
        Breakdown = 1 << 3,
        /// <summary>A charging cradle for workers.</summary>
        Recharge = 1 << 4
    }

    /// <summary>How a loud room's ambient noise level behaves over time.</summary>
    public enum AmbiencePattern
    {
        Silent = 0,
        Steady = 1,
        Pulsing = 2,
        Burst = 3
    }

    public enum BeatTrigger
    {
        MissionStart = 0,
        EnterRoom = 1,
        ItemPickedUp = 2,
        ItemDelivered = 3,
        CraftCompleted = 4,
        InteractionCompleted = 5,
        TimerReached = 6,
        WaveCleared = 7,
        PreviousBeatCompleted = 8
    }

    /// <summary>A room hazard that can be switched off (power restored, valve shut).</summary>
    public enum HazardKind
    {
        Dark = 0,
        Cold = 1,
        Heat = 2,
        Radiation = 3
    }

    /// <summary>Standing with a faction, from Hostile (−2) to Revered (3).</summary>
    public enum Standing
    {
        Hostile = -2,
        Unfriendly = -1,
        Neutral = 0,
        Friendly = 1,
        Trusted = 2,
        Revered = 3
    }

    /// <summary>The hive's escalation stages.</summary>
    public enum HiveStage
    {
        Dormant = 1,
        Restless = 2,
        Hunt = 3
    }

    public enum ValidationSeverity
    {
        Warning = 0,
        Error = 1
    }

    /// <summary>
    /// Upgrade slots. Military synths have the eight Syndicate-style body slots (Arms to Brain).
    /// Worker androids have work slots: Frame, two Hardening sockets, Maintenance and a Tool mount.
    /// </summary>
    public enum AugmentSlot
    {
        Arms = 0,
        Legs = 1,
        Eyes = 2,
        Ears = 3,
        Lungs = 4,
        Heart = 5,
        Dermis = 6,
        Brain = 7,
        Frame = 8,
        Hardening = 9,
        Maintenance = 10,
        ToolMount = 11,
        /// <summary>Worker brain: autonomy (scavenging, sentry tactics) and salvage sense.</summary>
        Processor = 12
    }

    public enum AugmentKind
    {
        Cyber = 0,
        Bio = 1
    }

    /// <summary>Stats that augments, specializations and Omens can modify.</summary>
    public enum UnitStat
    {
        MaxHealth = 0,
        Armour = 1,
        MoveSpeed = 2,
        NoiseWalking = 3,
        HeatSignature = 4,
        HeatAfterExertion = 5,
        VisionRange = 6,
        ContactDetectionRange = 7,
        DoorsHeardThrough = 8,
        CritChance = 9,
        CritMultiplier = 10,
        WeakPointMultiplier = 11,
        FireRate = 12,
        ReloadTime = 13,
        MeleeDamage = 14,
        HealthRegenPerSecond = 15,
        HealthRegenOutOfCombat = 16,
        HeatShielding = 17,
        ColdShielding = 18,
        AbilityCooldown = 19,
        ExperienceGain = 20,
        TargetAcquisitionTime = 21,
        EmpResistance = 22,
        SeesThroughSmoke = 23,
        SentryHitRateVsSlowed = 24,
        /// <summary>Expected hit rate, added as a fraction (0.15 = +15 points).</summary>
        Accuracy = 25,
        /// <summary>How far alien eyes see this unit, as a percent change (−0.3 = seen at 70% range).</summary>
        Visibility = 26,
        /// <summary>How much aliens prefer this unit as a target, as a percent change.</summary>
        Aggro = 27,
        VibrationSignature = 28,
        /// <summary>Noise of actions (reloads, melee, interactions), as a percent change.</summary>
        ActionNoise = 29,
        ThermalVision = 30,
        DarkVision = 31,
        HeavyWeaponHandling = 32,
        CarryCapacityKg = 33,
        /// <summary>Condition shielding ratings (0–3, matching hulk condition intensity).</summary>
        GravityShielding = 34,
        RadiationShielding = 35,
        AcidShielding = 36,
        IonShielding = 37,
        /// <summary>Speed of interactions and tool work, as a percent change.</summary>
        WorkSpeed = 38,
        /// <summary>Health per second a unit can restore on another unit.</summary>
        RepairOthersPerSecond = 39,
        /// <summary>Extra hand-carry slots (a loader frame carries cargo without a cart).</summary>
        CarrySlots = 40,
        MiningSpeed = 41,
        /// <summary>Welding, sealing and building speed.</summary>
        ConstructionSpeed = 42,
        /// <summary>Power a tool uses, as a percent change.</summary>
        ToolEnergyCost = 43,
        /// <summary>Oversize loads (a cryo bed) the unit can carry alone, without a cart or the Cradle sled.</summary>
        OversizeCarry = 44,
        SentryReloadSpeed = 45,
        /// <summary>Speed of placing and picking up sentries and mines, as a percent change.</summary>
        DeploySpeed = 48,
        /// <summary>Share of extra automatic salvage a worker adds (0.25 = +25%).</summary>
        SalvageYield = 49,
        /// <summary>How much a worker can do on its own: 2 = scavenge, 3 = sentry tactics.</summary>
        Autonomy = 50,
        BatteryCapacity = 46,
        /// <summary>Charge drained by every activity, as a percent change.</summary>
        ChargeDrain = 47
    }

    /// <summary>What a worker is doing, for charge (stamina) drain. (Named to avoid System.Diagnostics.Activity.)</summary>
    /// <summary>How hard an activity drains a worker's charge: three bands instead of a rate per activity.</summary>
    public enum ChargeBand
    {
        /// <summary>Standing, guarding, waiting.</summary>
        Idle = 0,
        /// <summary>Walking and pushing a cart.</summary>
        Light = 1,
        /// <summary>Tool work, oversize loads and defending.</summary>
        Heavy = 2
    }

    public enum WorkActivity
    {
        Idle = 0,
        Walking = 1,
        PushingCart = 2,
        CarryingOversize = 3,
        Mining = 4,
        Welding = 5,
        Building = 6,
        Defending = 7
    }

    public enum ModifierOperation
    {
        /// <summary>value += amount</summary>
        Add = 0,
        /// <summary>value *= (1 + amount); amount 0.15 = +15%</summary>
        AddPercent = 1
    }

    /// <summary>Military doctrine archetypes (0–3) and worker trades (4–7).</summary>
    public enum Archetype
    {
        Marksman = 0,
        Controller = 1,
        Bulwark = 2,
        HeavyGunner = 3,
        Hauler = 4,
        Miner = 5,
        Constructor = 6,
        Technician = 7,
        Sapper = 8
    }

    public enum OmenFamily
    {
        Hive = 0,
        Ship = 1,
        Condition = 2,
        Fabrication = 3,
        Expedition = 4,
        Fortune = 5,
        Culling = 6,
        Brutality = 7,
        Hybrid = 8,
        Vow = 9,
        /// <summary>Minor Arcana: a suit card that pushes one reward stream (see <see cref="OmenSuit"/>).</summary>
        Suit = 10
    }

    /// <summary>Major cards set the hulk's character (one per run); Minor cards each push one reward stream.</summary>
    public enum OmenArcana
    {
        Major = 0,
        Minor = 1
    }

    /// <summary>The six suits of the Minor Arcana, one per reward stream.</summary>
    public enum OmenSuit
    {
        None = 0,
        /// <summary>Contract pay and Scrip.</summary>
        Coins = 1,
        /// <summary>Automatic salvage and materials.</summary>
        Cogs = 2,
        /// <summary>Commander and specialization XP.</summary>
        Skulls = 3,
        /// <summary>Omen card and Catalyst drops.</summary>
        Candles = 4,
        /// <summary>Faction standing from the contract.</summary>
        Seals = 5,
        /// <summary>Intel: leads to charted hulks, logs, map reveals.</summary>
        Lanterns = 6
    }

    /// <summary>What the Cartomancer can do to a card with a Catalyst.</summary>
    public enum CardMutation
    {
        /// <summary>Reroll Catalyst: the handicap is redrawn from the family pool.</summary>
        Reroll = 0,
        /// <summary>Mastercraft Catalyst: a second handicap; benefit +25%, Threat +2.</summary>
        Deepen = 1,
        /// <summary>Refine Catalyst: +1 level without duplicates.</summary>
        Refine = 2,
        /// <summary>Hive-tainted Catalyst: irreversible corruption.</summary>
        Corrupt = 3
    }

    /// <summary>What corruption did to a card.</summary>
    public enum CardCorruption
    {
        None = 0,
        Nothing = 1,
        /// <summary>+1 rarity at once (a Legendary gains an extra twist instead).</summary>
        Ascended = 2,
        /// <summary>Both sides ×1.5; Threat counts double for the Brutality payout.</summary>
        Doubled = 3,
        /// <summary>Became the card's Heretic variant.</summary>
        Heretic = 4
    }

    /// <summary>The Catalysts (orbs): 11 gear, 5 meta and the 3 bench-only ones.</summary>
    public enum CatalystKind
    {
        Focus = 0,
        Attune = 1,
        Resonance = 2,
        Ascension = 3,
        Crown = 4,
        Mastercraft = 5,
        Imprint = 6,
        Refine = 7,
        Socket = 8,
        Reroll = 9,
        HiveTainted = 10,
        Lodestone = 11,
        Prism = 12,
        Augury = 13,
        Magnet = 14,
        Twin = 15,
        /// <summary>Bench: removes one affix (Rare and up: a chosen one).</summary>
        Strip = 16,
        /// <summary>Bench: rerolls the numbers within each affix's tier.</summary>
        Temper = 17,
        /// <summary>Bench: locks up to 2 affixes against the next Reroll or Strip.</summary>
        Seal = 18,
        /// <summary>Meta: a share of this run's Omen card drops become one chosen card (one already met).</summary>
        Beacon = 19,
        /// <summary>Meta: a share of this run's Omen card drops come from one chosen family or suit.</summary>
        Sigil = 20,
        /// <summary>Meta: each card slotted this run has a chance per card drop to drop a copy of itself.</summary>
        Echo = 21
    }

    /// <summary>Where a Catalyst can be used.</summary>
    [System.Flags]
    public enum CatalystUse
    {
        None = 0,
        /// <summary>In the Reliquary: shapes the drop before it rolls.</summary>
        Reliquary = 1,
        /// <summary>At the fabricator bench: changes an item already owned.</summary>
        Bench = 2,
        /// <summary>At the Cartomancer's table: mutates an Omen card.</summary>
        Card = 4
    }

    /// <summary>Amalgam infusion (Hive-tainted on owned gear): irreversible, 25% each.</summary>
    public enum InfusionOutcome
    {
        Nothing = 0,
        /// <summary>+1 Mark past the item's normal maximum.</summary>
        Overgrowth = 1,
        /// <summary>An alien implicit affix no crafting can roll.</summary>
        Xenomorphic = 2,
        /// <summary>One affix becomes a drawback; augments +1 signature.</summary>
        Rot = 3
    }

    /// <summary>Which rarity factor scales an effect: the benefit (m) or the handicap (h).</summary>
    public enum OmenSide
    {
        Benefit = 0,
        Handicap = 1
    }

    /// <summary>The scaling rules from the doc's rarity math (Content lists, section 2).</summary>
    public enum ScalingRule
    {
        /// <summary>Doesn't scale (on/off effects).</summary>
        None = 0,
        /// <summary>value × factor (percent bonuses and penalties).</summary>
        Linear = 1,
        /// <summary>value × factor; the fraction is a chance of one more.</summary>
        Count = 2,
        /// <summary>floor(value × factor) (slots, guaranteed sockets).</summary>
        Structural = 3,
        /// <summary>1 + (k − 1) × factor.</summary>
        Multiplier = 4,
        /// <summary>1 − (1 − p)^factor, for chances and biases.</summary>
        Chance = 5,
        /// <summary>1 − (1 − r)^factor, for reductions.</summary>
        Reduction = 6
    }

    public enum OmenEffectKind
    {
        LootPercent = 0,
        ContractPayPercent = 1,
        SurgeSizePercent = 2,
        AlienHealthPercent = 3,
        AlienDamagePercent = 4,
        NoEpicRolls = 5,
        RarityRolls = 6,
        AwarenessJumpPercent = 7,
        /// <summary>Adds intensity of a hulk condition (Condition Omens).</summary>
        ConditionIntensity = 8,
        /// <summary>Bias of loot affixes toward the card's family.</summary>
        AffixBias = 9,
        /// <summary>Percent more of the card's currency (Plating, Circuitry, ...).</summary>
        CurrencyPercent = 10,
        /// <summary>Fewer patrols in Stages 1–2 (Expedition Omens).</summary>
        PatrolReduction = 11,
        /// <summary>Percent more automatic salvage at the end of the mission.</summary>
        SalvagePercent = 12,
        /// <summary>Fewer spawns of the card's culled alien (Culling Omens); a fraction, Reduction rule.</summary>
        CulledSpawnReduction = 13,
        /// <summary>Percent change to how long welds hold (negative = weaker welds).</summary>
        WeldDurationPercent = 14,
        /// <summary>Seconds added to Stage 1 of the hive clock (negative = the hive stirs sooner).</summary>
        HiveClockSeconds = 15,
        /// <summary>Multiplier on how often champions spawn.</summary>
        ChampionMultiplier = 16,
        /// <summary>Extra hive spawn points on the hulk.</summary>
        ExtraHiveSpawns = 17,
        /// <summary>Multiplier on Catalyst (orb) drops.</summary>
        CatalystMultiplier = 18,
        /// <summary>The hulk is guaranteed a fabricator of at least this tier (ScalingRule.None).</summary>
        FabricatorTierMinimum = 19,
        /// <summary>Extra Omen card drops from Requisition.</summary>
        OmenDropCount = 20,
        /// <summary>Percent more commander and specialization XP (Skulls).</summary>
        CommanderXpPercent = 21,
        /// <summary>Percent more faction standing progress from the contract (Seals).</summary>
        StandingPercent = 22,
        /// <summary>Extra leads to charted hulks (Lanterns).</summary>
        IntelLeads = 23,
        /// <summary>Fraction of the hulk's map revealed at the start (Lanterns); Chance rule.</summary>
        MapRevealFraction = 24,
        /// <summary>Extra card-or-Catalyst rolls at Requisition (Candles).</summary>
        DropRolls = 25,
        /// <summary>The run's total Threat is at least this (the Anchorite).</summary>
        ThreatFloor = 26,
        /// <summary>No Common items drop this run (ScalingRule.None, value 1).</summary>
        NoCommonDrops = 27,
        /// <summary>Percent more chance of Rare-and-better drops.</summary>
        RareDropChancePercent = 28
    }

    public enum VowCondition
    {
        None = 0,
        /// <summary>No unit detected (an alien reaching Hunting on it) while the hive is Dormant.</summary>
        UndetectedWhileDormant = 1,
        NoUnitWrecked = 2,
        UnderParTime = 3
    }

    public enum OfferKind
    {
        Service = 0,
        Unit = 1,
        ShuttleModule = 2,
        Augment = 3,
        Item = 4,
        Blueprint = 5
    }

    /// <summary>What happens when the asteroid belt grinds through a section.</summary>
    public enum ErosionOutcome
    {
        /// <summary>The section decompresses: aliens inside are blown out or die; unsealed units take damage.</summary>
        Breach = 0,
        /// <summary>A radiation source's housing is torn off and drifts away: the target room's Radiation drops.</summary>
        StripRadiationSource = 1,
        /// <summary>A wall is opened: a new passage to the target room.</summary>
        OpenPassage = 2,
        /// <summary>The section caves in: the passage to the target room is blocked for good.</summary>
        Collapse = 3
    }

    /// <summary>How a beat teaches: a full prompt card, a one-line toast, or nothing (the level teaches it).</summary>
    public enum PromptStyle
    {
        Card = 0,
        Toast = 1,
        None = 2
    }

    /// <summary>What a commander countermeasure card does.</summary>
    public enum CountermeasureEffect
    {
        VentRoom = 0,
        SealDoor = 1,
        CutLights = 2,
        TurnHulk = 3
    }

    /// <summary>One step of a long-term goal, checked against the campaign state.</summary>
    public enum GoalStepKind
    {
        HaveMaterial = 0,
        HaveItem = 1,
        HaveFlag = 2,
        StandingAtLeast = 3,
        HaveScrip = 4
    }

    /// <summary>How a card from the tarot rollout reaches the player.</summary>
    public enum OmenSource
    {
        /// <summary>Found face-down; not playable until it is read (Gleaner's Moon in the prologue).</summary>
        FaceDown = 0,
        /// <summary>The Quiet Choir reader's free reading at Patusan.</summary>
        Reading = 1,
        /// <summary>Given for completing the mission.</summary>
        Reward = 2,
        /// <summary>Found on the hulk (a cache, a body, the crew's effects).</summary>
        Found = 3
    }

    /// <summary>What a worker can do on its own, granted by its Processor module and firmware.</summary>
    [System.Flags]
    public enum WorkerBehaviour
    {
        None = 0,
        /// <summary>Sent ahead to open rooms, search and salvage on its own (also needs to be quiet).</summary>
        Scavenge = 1,
        /// <summary>Places sentries where the swarm comes from, moves them when routes change, restocks them.</summary>
        SentryTactics = 2
    }
}
