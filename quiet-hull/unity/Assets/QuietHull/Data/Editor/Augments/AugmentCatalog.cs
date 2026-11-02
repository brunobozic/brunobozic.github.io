using System.Collections.Generic;

namespace QuietHull.Data.Editor
{
    /// <summary>Handles to every augment family, worker module, unit curve and frame the augment builder made.</summary>
    public sealed class AugmentCatalog
    {
        private readonly Dictionary<(AugmentSlot, AugmentLine), AugmentDefinition> _synth = new Dictionary<(AugmentSlot, AugmentLine), AugmentDefinition>();

        public UnitMarkCurveDefinition WorkerCurve { get; internal set; }
        public UnitMarkCurveDefinition MilitaryCurve { get; internal set; }
        public UnitDefinition Shade { get; internal set; }

        public IReadOnlyCollection<AugmentDefinition> SynthAugments => _synth.Values;
        public IReadOnlyList<AugmentDefinition> WorkerModules { get; internal set; }
        public IReadOnlyList<AugmentDefinition> HardeningModules { get; internal set; }
        public AugmentDefinition LoaderFrame { get; internal set; }
        public AugmentDefinition MuffledFrame { get; internal set; }
        public AugmentDefinition ReinforcedFrame { get; internal set; }
        public AugmentDefinition SelfRepair { get; internal set; }
        public AugmentDefinition BreachingCutter { get; internal set; }
        public AugmentDefinition SilentTools { get; internal set; }
        public AugmentDefinition MiningBeam { get; internal set; }
        public AugmentDefinition ConstructionRig { get; internal set; }

        public SpecializationDefinition Hauler { get; internal set; }
        public SpecializationDefinition Miner { get; internal set; }
        public SpecializationDefinition Constructor { get; internal set; }
        public SpecializationDefinition Technician { get; internal set; }
        public SpecializationDefinition Sapper { get; internal set; }
        public IReadOnlyList<SpecializationDefinition> WorkerTrades => new[] { Hauler, Miner, Constructor, Technician, Sapper };

        /// <summary>The standard worker battery: about 20 minutes walking, 5 minutes mining, 4 carrying a cryo bed.</summary>
        public static ChargeProfile WorkerCharge => new ChargeProfile(600f, 10f,
            0.05f, // Idle
            0.5f,  // Walking
            1.0f,  // PushingCart
            2.5f,  // CarryingOversize
            2.0f,  // Mining
            2.0f,  // Welding
            1.5f,  // Building
            1.5f); // Defending

        public DefinitionCatalog Catalog { get; internal set; }

        public AugmentDefinition Synth(AugmentSlot slot, AugmentLine line) => _synth[(slot, line)];

        internal void AddSynth(AugmentDefinition augment) => _synth.Add((augment.Slot, augment.Line), augment);

        /// <summary>The Standard line, one per body slot: what the Patusan clinic sells.</summary>
        public IReadOnlyList<AugmentDefinition> StandardLine()
        {
            var list = new List<AugmentDefinition>(8);
            for (var slot = AugmentSlot.Arms; slot <= AugmentSlot.Brain; slot++)
            {
                list.Add(_synth[(slot, AugmentLine.Standard)]);
            }

            return list;
        }

        /// <summary>A whole body of one line at one mark (Arms to Brain).</summary>
        public IReadOnlyList<InstalledAugment> FullBody(AugmentLine line, Mark mark)
        {
            var list = new List<InstalledAugment>(8);
            for (var slot = AugmentSlot.Arms; slot <= AugmentSlot.Brain; slot++)
            {
                list.Add(new InstalledAugment(_synth[(slot, line)], mark));
            }

            return list;
        }
    }
}
