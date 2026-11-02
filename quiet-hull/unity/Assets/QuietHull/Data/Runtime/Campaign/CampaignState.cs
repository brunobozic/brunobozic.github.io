using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>
    /// What the player has between missions: materials, items, standing, flags. Used to evaluate goals
    /// and hub visibility. Campaign state lives on the main thread; not thread-safe.
    /// </summary>
    public sealed class CampaignState
    {
        private readonly Dictionary<ItemDefinition, int> _counts = new Dictionary<ItemDefinition, int>();

        public CampaignState(PlayerStanding standing) => Standing = standing ?? throw new ArgumentNullException(nameof(standing));

        public PlayerStanding Standing { get; }

        public int Count(ItemDefinition item) => item != null && _counts.TryGetValue(item, out var n) ? n : 0;

        public void Add(ItemDefinition item, int count)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var next = Count(item) + count;
            if (next < 0)
            {
                throw new InvalidOperationException($"Not enough {item.Id}: have {Count(item)}, removing {-count}.");
            }

            _counts[item] = next;
        }
    }
}
