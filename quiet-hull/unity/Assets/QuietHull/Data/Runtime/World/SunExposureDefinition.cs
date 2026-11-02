using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>One room's Heat intensity at each facing of the hulk toward the sun.</summary>
    [Serializable]
    public struct RoomExposure
    {
        public string roomId;
        [Tooltip("Heat intensity 0–3 for facing 0, 1, 2, ... (one entry per facing).")]
        public int[] heatByFacing;

        public RoomExposure(string roomId, params int[] heatByFacing)
        {
            this.roomId = roomId;
            this.heatByFacing = heatByFacing;
        }
    }

    /// <summary>
    /// How a hulk sits toward the sun. Restoring the attitude thrusters lets the player turn it to
    /// another facing; each turn cools some rooms (unlocking the sun-locked ones) and heats others
    /// (locking them, and burning the aliens inside). Some hulks need several turns in a row.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/World/Sun Exposure", fileName = "sun_")]
    public sealed class SunExposureDefinition : DefinitionBase
    {
        [Min(2)] [SerializeField] internal int facings = 4;
        [SerializeField] internal int startFacing;
        [Tooltip("Seconds a turn takes; rooms change heat gradually over it.")]
        [Min(1f)] [SerializeField] internal float turnSeconds = 75f;
        [Tooltip("Rooms at or above this Heat can't be entered.")]
        [Range(1, 3)] [SerializeField] internal int lockIntensity = 3;
        [Tooltip("Interaction id of the attitude thruster console.")]
        [SerializeField] internal string thrusterConsoleId = string.Empty;
        [SerializeField] internal RoomExposure[] rooms = Array.Empty<RoomExposure>();

        public int Facings => facings;
        public int StartFacing => startFacing;
        public float TurnSeconds => turnSeconds;
        public int LockIntensity => lockIntensity;
        public string ThrusterConsoleId => thrusterConsoleId;
        public IReadOnlyList<RoomExposure> Rooms => rooms ?? Array.Empty<RoomExposure>();

        public int HeatOf(string roomId, int facing)
        {
            foreach (var room in Rooms)
            {
                if (string.Equals(room.roomId, roomId, StringComparison.Ordinal))
                {
                    return room.heatByFacing[Wrap(facing)];
                }
            }

            throw new KeyNotFoundException($"{id} has no room '{roomId}'.");
        }

        public bool IsSunLocked(string roomId, int facing) => HeatOf(roomId, facing) >= lockIntensity;

        /// <summary>Rooms open at <paramref name="facing"/>.</summary>
        public List<string> OpenRooms(int facing)
        {
            var open = new List<string>(Rooms.Count);
            foreach (var room in Rooms)
            {
                if (room.heatByFacing[Wrap(facing)] < lockIntensity)
                {
                    open.Add(room.roomId);
                }
            }

            return open;
        }

        /// <summary>Rooms a turn from <paramref name="from"/> to <paramref name="to"/> opens, and the ones it closes.</summary>
        public (List<string> unlocked, List<string> locked) Turn(int from, int to)
        {
            var unlocked = new List<string>();
            var locked = new List<string>();
            foreach (var room in Rooms)
            {
                var before = room.heatByFacing[Wrap(from)] >= lockIntensity;
                var after = room.heatByFacing[Wrap(to)] >= lockIntensity;
                if (before && !after) unlocked.Add(room.roomId);
                if (!before && after) locked.Add(room.roomId);
            }

            return (unlocked, locked);
        }

        private int Wrap(int facing) => ((facing % facings) + facings) % facings;

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (startFacing < 0 || startFacing >= facings)
            {
                report.Error(this, nameof(startFacing), $"must be in 0..{facings - 1}.");
            }

            if (string.IsNullOrWhiteSpace(thrusterConsoleId))
            {
                report.Error(this, nameof(thrusterConsoleId), "the player needs a console to turn the ship.");
            }

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var room in Rooms)
            {
                if (string.IsNullOrWhiteSpace(room.roomId) || !ids.Add(room.roomId))
                {
                    report.Error(this, nameof(rooms), $"room id '{room.roomId}' is empty or duplicated.");
                }

                if (room.heatByFacing == null || room.heatByFacing.Length != facings)
                {
                    report.Error(this, nameof(rooms), $"'{room.roomId}' needs {facings} heat values.");
                    continue;
                }

                var reachable = false;
                foreach (var heat in room.heatByFacing)
                {
                    if (heat < 0 || heat > 3)
                    {
                        report.Error(this, nameof(rooms), $"'{room.roomId}' has heat {heat} (0–3).");
                    }

                    reachable |= heat < lockIntensity;
                }

                if (!reachable)
                {
                    report.Error(this, nameof(rooms), $"'{room.roomId}' is sun-locked at every facing.");
                }
            }
        }
    }
}
