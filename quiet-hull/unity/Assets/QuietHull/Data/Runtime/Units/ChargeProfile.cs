using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A worker's charge, like stamina: a battery that drains in three bands (idle, light, heavy) and a
    /// charging cradle (or a live power junction) refills. Heavy work empties it fast; standing still
    /// barely touches it. A unit at 0 charge stops and waits to be recharged or carried.
    /// </summary>
    [Serializable]
    public struct ChargeProfile
    {
        [Tooltip("Battery capacity in charge units.")]
        [Min(1f)] public float capacity;
        [Tooltip("Charge per second while idle, guarding or waiting.")]
        [Min(0f)] public float idleDrain;
        [Tooltip("Charge per second while walking or pushing a cart.")]
        [Min(0f)] public float lightDrain;
        [Tooltip("Charge per second for tool work, oversize loads and defending.")]
        [Min(0f)] public float heavyDrain;
        [Tooltip("Charge per second restored at a charging cradle or a live junction.")]
        [Min(0f)] public float rechargePerSecond;

        public ChargeProfile(float capacity, float rechargePerSecond, float idleDrain, float lightDrain, float heavyDrain)
        {
            this.capacity = capacity;
            this.rechargePerSecond = rechargePerSecond;
            this.idleDrain = idleDrain;
            this.lightDrain = lightDrain;
            this.heavyDrain = heavyDrain;
        }

        /// <summary>The band each activity falls in.</summary>
        public static ChargeBand BandOf(WorkActivity activity)
        {
            switch (activity)
            {
                case WorkActivity.Idle:
                    return ChargeBand.Idle;
                case WorkActivity.Walking:
                case WorkActivity.PushingCart:
                    return ChargeBand.Light;
                default:
                    return ChargeBand.Heavy;
            }
        }

        public float Drain(ChargeBand band) => band == ChargeBand.Idle ? idleDrain : band == ChargeBand.Light ? lightDrain : heavyDrain;

        public float Drain(WorkActivity activity) => Drain(BandOf(activity));

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

        /// <summary>Idle ≤ light ≤ heavy, light above zero.</summary>
        public bool IsOrdered => lightDrain > 0f && idleDrain <= lightDrain && lightDrain <= heavyDrain;
    }
}
