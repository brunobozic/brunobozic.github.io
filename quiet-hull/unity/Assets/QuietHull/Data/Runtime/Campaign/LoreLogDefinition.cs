using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>A collectible log (recorder, terminal, note): the exploration reward that carries the story.</summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Campaign/Lore Log", fileName = "log_")]
    public sealed class LoreLogDefinition : DefinitionBase
    {
        [SerializeField] internal string hulk = string.Empty;
        [SerializeField] internal string roomId = string.Empty;
        [TextArea(3, 10)] [SerializeField] internal string text = string.Empty;

        public string Hulk => hulk;
        public string RoomId => roomId;
        public string Text => text;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(roomId))
            {
                report.Error(this, nameof(text), "a log needs text and a room.");
            }
        }
    }
}
