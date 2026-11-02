using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>Shared combat math from the balance model.</summary>
    public static class ArmourMath
    {
        /// <summary>Damage multiplier for armour A: 100 / (100 + A). Negative armour counts as 0.</summary>
        public static float DamageMultiplier(float armour) => 100f / (100f + Math.Max(0f, armour));
    }
}
