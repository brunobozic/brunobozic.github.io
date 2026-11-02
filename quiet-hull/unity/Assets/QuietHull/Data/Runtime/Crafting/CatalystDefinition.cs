using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A Catalyst (orb). Where it can be used is fixed by its kind (<see cref="CatalystRules.UsesOf"/>): the
    /// Reliquary shapes a drop before it rolls, the fabricator bench changes an item already owned, the
    /// Cartomancer mutates an Omen card. Per-rarity effects follow the "Catalysts by rarity" table.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Crafting/Catalyst", fileName = "cat_")]
    public sealed class CatalystDefinition : DefinitionBase
    {
        [SerializeField] internal CatalystKind kind;
        [SerializeField] internal Rarity rarity = Rarity.Common;
        [Tooltip("What it does at this rarity, as the player reads it.")]
        [TextArea(1, 3)] [SerializeField] internal string effectText = string.Empty;
        [Tooltip("First mission it can drop (the rollout gates bench and card use separately).")]
        [Min(1)] [SerializeField] internal int dropsFromMission = 1;

        public CatalystKind Kind => kind;
        public Rarity Rarity => rarity;
        public string EffectText => effectText;
        public int DropsFromMission => dropsFromMission;
        public CatalystUse Uses => CatalystRules.UsesOf(kind);
        public bool IsMeta => CatalystRules.IsMeta(kind);
        public bool CanUse(CatalystUse where) => (Uses & where) == where && where != CatalystUse.None;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(effectText))
            {
                report.Error(this, nameof(effectText), "say what it does.");
            }

            if (Uses == CatalystUse.None)
            {
                report.Error(this, nameof(kind), $"{kind} has no use.");
            }
        }
    }
}
