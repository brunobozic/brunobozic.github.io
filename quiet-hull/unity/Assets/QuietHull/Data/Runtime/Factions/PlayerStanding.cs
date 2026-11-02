using System;
using System.Collections.Generic;

namespace QuietHull.Data
{
    /// <summary>A change to a faction standing: raise or lower by steps, or set at least / at most a level.</summary>
    [Serializable]
    public struct StandingChange
    {
        public FactionDefinition faction;
        public int steps;
        [UnityEngine.Tooltip("If set, the standing becomes at least this (applied after steps).")]
        public bool setAtLeast;
        public Standing atLeast;
        [UnityEngine.Tooltip("If set, the standing becomes at most this (applied after steps).")]
        public bool setAtMost;
        public Standing atMost;

        public static StandingChange By(FactionDefinition faction, int steps) => new StandingChange { faction = faction, steps = steps };

        public static StandingChange AtLeast(FactionDefinition faction, Standing standing) =>
            new StandingChange { faction = faction, setAtLeast = true, atLeast = standing };

        public static StandingChange AtMost(FactionDefinition faction, Standing standing) =>
            new StandingChange { faction = faction, setAtMost = true, atMost = standing };
    }

    /// <summary>One condition for buying something: a faction at a level, or a licence.</summary>
    [Serializable]
    public struct Requirement
    {
        public FactionDefinition faction;
        public Standing minimum;
        public LicenceDefinition licence;

        public static Requirement Faction(FactionDefinition faction, Standing minimum) => new Requirement { faction = faction, minimum = minimum };

        public static Requirement Holds(LicenceDefinition licence) => new Requirement { licence = licence };

        public bool IsMetBy(PlayerStanding standing) =>
            standing != null && (licence != null ? standing.Holds(licence) : faction != null && standing.Of(faction) >= minimum);

        public override string ToString() => licence != null ? $"Licence: {licence.DisplayName}" : $"{faction?.DisplayName}: {minimum}";
    }

    /// <summary>
    /// The player's standing with every faction and the licences they hold. Campaign state, owned by
    /// the campaign service on the main thread; not thread-safe.
    /// </summary>
    public sealed class PlayerStanding
    {
        private readonly Dictionary<FactionDefinition, Standing> _standing = new Dictionary<FactionDefinition, Standing>();
        private readonly HashSet<LicenceDefinition> _licences = new HashSet<LicenceDefinition>();
        private readonly HashSet<string> _flags = new HashSet<string>(StringComparer.Ordinal);

        public PlayerStanding(IEnumerable<FactionDefinition> factions)
        {
            if (factions == null)
            {
                throw new ArgumentNullException(nameof(factions));
            }

            foreach (var faction in factions)
            {
                _standing[faction] = faction.StartingStanding;
            }
        }

        public int Scrip { get; private set; }
        public int CommanderXp { get; private set; }

        public void AddCommanderXp(int amount) => CommanderXp = checked(CommanderXp + Math.Max(0, amount));
        public IReadOnlyCollection<string> Flags => _flags;

        public Standing Of(FactionDefinition faction) => faction != null && _standing.TryGetValue(faction, out var s) ? s : Standing.Neutral;

        public bool Holds(LicenceDefinition licence) => licence != null && _licences.Contains(licence);

        public bool HasFlag(string flag) => _flags.Contains(flag);

        public void Apply(StandingChange change)
        {
            if (change.faction == null)
            {
                throw new ArgumentException("A standing change needs a faction.", nameof(change));
            }

            var value = (int)Of(change.faction) + change.steps;
            if (change.setAtLeast) value = Math.Max(value, (int)change.atLeast);
            if (change.setAtMost) value = Math.Min(value, (int)change.atMost);
            _standing[change.faction] = (Standing)Math.Max((int)Standing.Hostile, Math.Min((int)Standing.Revered, value));
        }

        public void Grant(LicenceDefinition licence)
        {
            if (licence != null)
            {
                _licences.Add(licence);
            }
        }

        public void ClearFlag(string flag) => _flags.Remove(flag ?? string.Empty);

        public void SetFlag(string flag)
        {
            if (!string.IsNullOrWhiteSpace(flag))
            {
                _flags.Add(flag);
            }
        }

        public void AddScrip(int amount) => Scrip = checked(Scrip + amount);
    }
}
