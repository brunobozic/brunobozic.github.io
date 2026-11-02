using System;
using System.Collections.Generic;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>The extra a chunk yields from the surface the acid ate into.</summary>
    [Serializable]
    public struct SurfaceYield
    {
        public SurfaceKind surface;
        public MaterialDefinition material;
        [Min(1)] public int amount;

        public SurfaceYield(SurfaceKind surface, MaterialDefinition material, int amount)
        {
            this.surface = surface;
            this.material = material;
            this.amount = amount;
        }
    }

    /// <summary>
    /// Acid-fused amalgam. Alien blood sprays when they die and eats into the bulkheads; enough kills
    /// in one place leave a half-melted deposit of alien acid and ship metal. Workers mine the
    /// deposit, and any station that offers Breakdown splits a chunk into components: a base yield
    /// plus an extra that depends on the surface (bulkhead gives Plating, conduit gives Circuitry,
    /// munitions give Ordnance). Kill zones become mines. They return less ammo than they cost, so
    /// they never fund themselves.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/World/Amalgam", fileName = "amalgam_")]
    public sealed class AmalgamDefinition : DefinitionBase
    {
        [Tooltip("The mined chunk.")]
        [SerializeField] internal MaterialDefinition chunk;
        [Tooltip("Summed alien amalgam weight in one segment (a 4 m stretch) that forms one deposit.")]
        [Min(1f)] [SerializeField] internal float weightPerDeposit = 25f;
        [Min(1)] [SerializeField] internal int maxDepositsPerSegment = 3;
        [Min(0.1f)] [SerializeField] internal float mineSeconds = 8f;
        [Min(0f)] [SerializeField] internal float mineNoise = 2f;
        [SerializeField] internal MaterialCost[] baseYield = Array.Empty<MaterialCost>();
        [SerializeField] internal SurfaceYield[] surfaceYields = Array.Empty<SurfaceYield>();
        [Min(0.1f)] [SerializeField] internal float breakdownSeconds = 6f;

        public MaterialDefinition Chunk => chunk;
        public float WeightPerDeposit => weightPerDeposit;
        public int MaxDepositsPerSegment => maxDepositsPerSegment;
        public float MineSeconds => mineSeconds;
        public float MineNoise => mineNoise;
        public IReadOnlyList<MaterialCost> BaseYield => baseYield ?? Array.Empty<MaterialCost>();
        public IReadOnlyList<SurfaceYield> SurfaceYields => surfaceYields ?? Array.Empty<SurfaceYield>();
        public float BreakdownSeconds => breakdownSeconds;

        /// <summary>Deposits formed by <paramref name="totalWeight"/> of kills spread over <paramref name="segments"/> segments.</summary>
        public int DepositsFrom(float totalWeight, int segments)
        {
            if (totalWeight <= 0f || segments <= 0)
            {
                return 0;
            }

            var formed = (int)Math.Floor(totalWeight / weightPerDeposit);
            return Math.Min(formed, segments * maxDepositsPerSegment);
        }

        /// <summary>Amount of <paramref name="material"/> one chunk from <paramref name="surface"/> breaks down into.</summary>
        public int YieldOf(MaterialDefinition material, SurfaceKind surface)
        {
            var total = 0;
            foreach (var cost in BaseYield)
            {
                if (cost.material == material)
                {
                    total += cost.amount;
                }
            }

            foreach (var extra in SurfaceYields)
            {
                if (extra.surface == surface && extra.material == material)
                {
                    total += extra.amount;
                }
            }

            return total;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            report.NotNull(this, nameof(chunk), chunk);
            report.NotEmpty(this, nameof(baseYield), baseYield);

            var surfaces = new HashSet<SurfaceKind>();
            foreach (var extra in SurfaceYields)
            {
                report.NotNull(this, $"{nameof(surfaceYields)}.material", extra.material);
                if (!surfaces.Add(extra.surface))
                {
                    report.Warning(this, nameof(surfaceYields), $"{extra.surface} has two entries.");
                }
            }
        }
    }
}
