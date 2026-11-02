using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// How a unit class improves with its own mark (worker androids Mk I–XII, military chassis Mk I–V),
    /// and, for workers, which firmware generation each mark runs.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Units/Unit Mark Curve", fileName = "curve_")]
    public sealed class UnitMarkCurveDefinition : DefinitionBase
    {
        [SerializeField] internal Mark maxMark = Mark.MkXII;

        [Header("Per mark above I")]
        [Tooltip("Max health +x per mark (0.15 = +15% of the Mk I value).")]
        [Min(0f)] [SerializeField] internal float healthPerMark = 0.15f;
        [Min(0f)] [SerializeField] internal float armourPerMark = 3f;
        [Min(0f)] [SerializeField] internal float moveSpeedPerMark = 0.05f;
        [Min(0f)] [SerializeField] internal float workSpeedPerMark = 0.06f;
        [Min(0f)] [SerializeField] internal float defendDamagePerMark = 0.1f;
        [Tooltip("Noise change per mark (newer models are quieter).")]
        [SerializeField] internal float noisePerMark = -0.1f;
        [Min(0f)] [SerializeField] internal float minimumNoise = 1f;
        [Min(0f)] [SerializeField] internal float powerOutputPerMark = 25f;
        [Tooltip("One more hand-carry slot every N marks (0 = never).")]
        [Min(0)] [SerializeField] internal int extraCarrySlotEvery = 4;
        [Min(1f)] [SerializeField] internal float priceFactorPerMark = 1.35f;

        [Header("Firmware (workers)")]
        [SerializeField] internal bool hasFirmware = true;
        [SerializeField] internal Mark gen2FromMark = Mark.MkV;
        [SerializeField] internal Mark gen3FromMark = Mark.MkIX;
        [Tooltip("Chance per incident that a Gen 1 worker's asset directive overrides your orders.")]
        [Range(0f, 1f)] [SerializeField] internal float gen1GlitchChance = 0.25f;
        [Tooltip("Gen 2 only glitches over alien specimens (eggs, the Queen).")]
        [Range(0f, 1f)] [SerializeField] internal float gen2GlitchChance = 0.10f;

        public Mark MaxMark => maxMark;
        public float HealthPerMark => healthPerMark;
        public float ArmourPerMark => armourPerMark;
        public float MoveSpeedPerMark => moveSpeedPerMark;
        public float WorkSpeedPerMark => workSpeedPerMark;
        public float DefendDamagePerMark => defendDamagePerMark;
        public float NoisePerMark => noisePerMark;
        public float MinimumNoise => minimumNoise;
        public float PowerOutputPerMark => powerOutputPerMark;
        public int ExtraCarrySlotEvery => extraCarrySlotEvery;
        public float PriceFactorPerMark => priceFactorPerMark;
        public bool HasFirmware => hasFirmware;
        public Mark Gen2FromMark => gen2FromMark;
        public Mark Gen3FromMark => gen3FromMark;

        public FirmwareGeneration FirmwareAt(Mark mark) =>
            !hasFirmware || mark >= gen3FromMark ? FirmwareGeneration.Gen3 : mark >= gen2FromMark ? FirmwareGeneration.Gen2 : FirmwareGeneration.Gen1;

        /// <summary>Chance per incident that the asset directive overrides orders; Gen 2 only for specimens.</summary>
        public float GlitchChance(Mark mark, bool incidentInvolvesSpecimen)
        {
            switch (FirmwareAt(mark))
            {
                case FirmwareGeneration.Gen1: return gen1GlitchChance;
                case FirmwareGeneration.Gen2: return incidentInvolvesSpecimen ? gen2GlitchChance : 0f;
                default: return 0f;
            }
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (hasFirmware && gen3FromMark <= gen2FromMark)
            {
                report.Error(this, nameof(gen3FromMark), "Gen 3 must start after Gen 2.");
            }

            if (gen2GlitchChance > gen1GlitchChance)
            {
                report.Warning(this, nameof(gen2GlitchChance), "newer firmware glitches more than older firmware.");
            }
        }
    }

    /// <summary>A unit's own stats at a mark, before augments. Immutable.</summary>
    public readonly struct UnitStatsAtMark
    {
        public UnitStatsAtMark(Mark mark, float maxHealth, float armour, float moveSpeed, float workSpeed, float defendDamagePerSecond,
            float noiseWalking, float heatIdle, float powerOutputWatts, int handCarrySlots, int priceScrip, FirmwareGeneration firmware)
        {
            Mark = mark;
            MaxHealth = maxHealth;
            Armour = armour;
            MoveSpeed = moveSpeed;
            WorkSpeed = workSpeed;
            DefendDamagePerSecond = defendDamagePerSecond;
            NoiseWalking = noiseWalking;
            HeatIdle = heatIdle;
            PowerOutputWatts = powerOutputWatts;
            HandCarrySlots = handCarrySlots;
            PriceScrip = priceScrip;
            Firmware = firmware;
        }

        public Mark Mark { get; }
        public float MaxHealth { get; }
        public float Armour { get; }
        public float MoveSpeed { get; }
        public float WorkSpeed { get; }
        public float DefendDamagePerSecond { get; }
        public float NoiseWalking { get; }
        public float HeatIdle { get; }
        public float PowerOutputWatts { get; }
        public int HandCarrySlots { get; }
        public int PriceScrip { get; }
        public FirmwareGeneration Firmware { get; }
    }
}
