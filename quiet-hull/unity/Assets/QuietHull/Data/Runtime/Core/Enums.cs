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
        MineResource = 3
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
}
