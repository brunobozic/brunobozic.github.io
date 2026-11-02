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

    /// <summary>Equipment mark (Mk I–V). Prologue content is always <see cref="MkI"/>.</summary>
    public enum Mark
    {
        MkI = 1,
        MkII = 2,
        MkIII = 3,
        MkIV = 4,
        MkV = 5
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
        Key = 11
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
        FireWeapon = 1 << 8
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
        Fabricate = 1 << 2
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
        Heat = 2
    }

    public enum ValidationSeverity
    {
        Warning = 0,
        Error = 1
    }

    /// <summary>Syndicate-style augment slots on a synthetic body.</summary>
    public enum AugmentSlot
    {
        Arms = 0,
        Legs = 1,
        Eyes = 2,
        Ears = 3,
        Lungs = 4,
        Heart = 5,
        Dermis = 6,
        Brain = 7
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
        SentryHitRateVsSlowed = 24
    }

    public enum ModifierOperation
    {
        /// <summary>value += amount</summary>
        Add = 0,
        /// <summary>value *= (1 + amount); amount 0.15 = +15%</summary>
        AddPercent = 1
    }

    public enum Archetype
    {
        Marksman = 0,
        Controller = 1,
        Bulwark = 2,
        HeavyGunner = 3
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
        Vow = 9
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
        AwarenessJumpPercent = 7
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
        Item = 4
    }
}
