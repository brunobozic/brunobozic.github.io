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
        public AugmentDefinition SalvageScanner { get; internal set; }
        public AugmentDefinition SurveyAi { get; internal set; }
        public AugmentDefinition GhostRoutine { get; internal set; }
        public AugmentDefinition SwarmTracker { get; internal set; }

        /// <summary>Worker slots: frame, two hardening sockets, maintenance, tool mount, processor.</summary>
        public static AugmentSlot[] WorkerSlots => new[]
        {
            AugmentSlot.Frame, AugmentSlot.Hardening, AugmentSlot.Hardening, AugmentSlot.Maintenance, AugmentSlot.ToolMount, AugmentSlot.Processor
        };
        public AugmentDefinition ConstructionRig { get; internal set; }
        public AugmentDefinition FieldRepairKit { get; internal set; }

        public SpecializationDefinition Hauler { get; internal set; }
        public SpecializationDefinition Miner { get; internal set; }
        public SpecializationDefinition Constructor { get; internal set; }
        public SpecializationDefinition Technician { get; internal set; }
        public SpecializationDefinition Sapper { get; internal set; }
        public IReadOnlyList<SpecializationDefinition> WorkerTrades => new[] { Hauler, Miner, Constructor, Technician, Sapper };

        /// <summary>
        /// The standard worker battery, in three drain bands: about 3 hours idle, 20 minutes of light work
        /// (walking, a cart) and 5 minutes of heavy work (tools, a cryo bed, a fight).
        /// </summary>
        public static ChargeProfile WorkerCharge => new ChargeProfile(600f, 10f, 0.05f, 0.5f, 2.0f);

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
