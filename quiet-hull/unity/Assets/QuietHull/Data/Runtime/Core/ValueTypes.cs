using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>What an alien can sense and how far. Multipliers scale the noise model's signal.</summary>
    [Serializable]
    public struct SenseProfile
    {
        [Min(0f)] public float hearingMultiplier;
        [Min(0f)] public float vibrationMultiplier;
        [Min(0f)] public float visionRange;
        [Range(0f, 360f)] public float visionAngleDegrees;
        [Min(0f)] public float heatVisionRange;

        public SenseProfile(float hearingMultiplier, float vibrationMultiplier, float visionRange, float visionAngleDegrees, float heatVisionRange)
        {
            this.hearingMultiplier = hearingMultiplier;
            this.vibrationMultiplier = vibrationMultiplier;
            this.visionRange = visionRange;
            this.visionAngleDegrees = visionAngleDegrees;
            this.heatVisionRange = heatVisionRange;
        }

        /// <summary>The senses this profile actually uses; drives the icons above the alien's head.</summary>
        public SenseFlags Senses
        {
            get
            {
                var senses = SenseFlags.None;
                if (hearingMultiplier > 0f) senses |= SenseFlags.Hearing;
                if (vibrationMultiplier > 0f) senses |= SenseFlags.Vibration;
                if (visionRange > 0f && visionAngleDegrees > 0f) senses |= SenseFlags.Vision;
                if (heatVisionRange > 0f) senses |= SenseFlags.Heat;
                return senses;
            }
        }
    }

    /// <summary>One attack. Melee uses <see cref="damage"/> per <see cref="intervalSeconds"/>.</summary>
    [Serializable]
    public struct AttackProfile
    {
        public AttackKind kind;
        [Min(0f)] public float damage;
        [Min(0.01f)] public float intervalSeconds;
        [Min(0f)] public float range;
        [Min(0f)] public float splashRadius;

        public AttackProfile(AttackKind kind, float damage, float intervalSeconds, float range, float splashRadius)
        {
            this.kind = kind;
            this.damage = damage;
            this.intervalSeconds = intervalSeconds;
            this.range = range;
            this.splashRadius = splashRadius;
        }

        public float DamagePerSecond => intervalSeconds > 0f ? damage / intervalSeconds : 0f;
    }

    /// <summary>A quantity of a crafting material.</summary>
    [Serializable]
    public struct MaterialCost
    {
        public MaterialDefinition material;
        [Min(1)] public int amount;

        public MaterialCost(MaterialDefinition material, int amount)
        {
            this.material = material;
            this.amount = amount;
        }
    }

    /// <summary>A timed action a tool can perform.</summary>
    [Serializable]
    public struct ToolAction
    {
        public ToolActionKind kind;
        [Min(0.1f)] public float durationSeconds;

        public ToolAction(ToolActionKind kind, float durationSeconds)
        {
            this.kind = kind;
            this.durationSeconds = durationSeconds;
        }
    }

    /// <summary>One group of aliens in a scripted wave.</summary>
    [Serializable]
    public struct WaveSpawn
    {
        public AlienDefinition alien;
        [Min(1)] public int count;
        [Tooltip("Spawn point id placed in the level (e.g. lane_a_vent).")]
        public string spawnPointId;
        [Min(0f)] public float delaySeconds;
        [Min(0f)] public float intervalSeconds;

        public WaveSpawn(AlienDefinition alien, int count, string spawnPointId, float delaySeconds, float intervalSeconds)
        {
            this.alien = alien;
            this.count = count;
            this.spawnPointId = spawnPointId;
            this.delaySeconds = delaySeconds;
            this.intervalSeconds = intervalSeconds;
        }

        /// <summary>When the last alien of this group spawns, relative to the wave start.</summary>
        public float LastSpawnSeconds => delaySeconds + (count - 1) * intervalSeconds;
    }

    /// <summary>An objective line shown in the objective tracker.</summary>
    [Serializable]
    public struct ObjectiveEntry
    {
        public string objectiveId;
        public string text;
        public bool optional;

        public ObjectiveEntry(string objectiveId, string text, bool optional)
        {
            this.objectiveId = objectiveId;
            this.text = text;
            this.optional = optional;
        }
    }

    /// <summary>A number of one item.</summary>
    [Serializable]
    public struct ItemStack
    {
        public ItemDefinition item;
        [Min(1)] public int count;

        public ItemStack(ItemDefinition item, int count)
        {
            this.item = item;
            this.count = count;
        }
    }
}
