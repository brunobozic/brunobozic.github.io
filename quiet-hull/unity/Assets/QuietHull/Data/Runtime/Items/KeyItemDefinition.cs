using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A key item (keycard, code cylinder) that opens one door. Takes no cargo space.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Items/Key Item", fileName = "itm_key_")]
    public sealed class KeyItemDefinition : ItemDefinition
    {
        [Header("Key")]
        [Tooltip("Door id placed in the level that this key opens.")]
        [SerializeField] internal string opensDoorId = string.Empty;
        [Tooltip("Consumed when used (false: stays in the inventory).")]
        [SerializeField] internal bool consumedOnUse;

        public override ItemCategory Category => ItemCategory.Key;
        public string OpensDoorId => opensDoorId;
        public bool ConsumedOnUse => consumedOnUse;

        protected override void ValidateItem(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(opensDoorId))
            {
                report.Error(this, nameof(opensDoorId), "is empty.");
            }
        }
    }
}
