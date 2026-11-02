using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A worker's charge, like stamina: a battery that every activity drains at its own rate and a
    /// charging cradle (or a live power junction) refills. Heavy work empties it fast; standing
    /// still barely touches it. A unit at 0 charge stops and waits to be recharged or carried.
    /// </summary>
    [Serializable]
    public struct ChargeProfile
    {
        [Tooltip("Battery capacity in charge units.")]
        [Min(1f)] public float capacity;
        [Tooltip("Charge per second for each activity, indexed by WorkActivity.")]
        public float[] drainPerSecond;
        [Tooltip("Charge per second restored at a charging cradle or a live junction.")]
        [Min(0f)] public float rechargePerSecond;

        public ChargeProfile(float capacity, float rechargePerSecond, params float[] drainPerSecond)
        {
            this.capacity = capacity;
            this.rechargePerSecond = rechargePerSecond;
            this.drainPerSecond = drainPerSecond;
        }

        public static int ActivityCount => Enum.GetValues(typeof(WorkActivity)).Length;

        public float Drain(WorkActivity activity)
        {
            var index = (int)activity;
            return drainPerSecond != null && index < drainPerSecond.Length ? drainPerSecond[index] : 0f;
        }

        /// <summary>
        /// Seconds of continuous <paramref name="activity"/> from full, with modifiers applied
        /// (BatteryCapacity, ChargeDrain, and ToolEnergyCost for tool work).
        /// </summary>
        public float SecondsOf(WorkActivity activity, IReadOnlyList<StatModifier> modifiers)
        {
            var battery = StatMath.Apply(UnitStat.BatteryCapacity, capacity, modifiers);
            var drain = StatMath.Apply(UnitStat.ChargeDrain, Drain(activity), modifiers);
            if (activity == WorkActivity.Mining || activity == WorkActivity.Welding || activity == WorkActivity.Building)
            {
                drain = StatMath.Apply(UnitStat.ToolEnergyCost, drain, modifiers);
            }

            return drain > 0f ? battery / drain : float.PositiveInfinity;
        }

        /// <summary>Seconds at a cradle to go from empty to full.</summary>
        public float SecondsToFull(IReadOnlyList<StatModifier> modifiers)
        {
            var battery = StatMath.Apply(UnitStat.BatteryCapacity, capacity, modifiers);
            return rechargePerSecond > 0f ? battery / rechargePerSecond : float.PositiveInfinity;
        }
    }
}
