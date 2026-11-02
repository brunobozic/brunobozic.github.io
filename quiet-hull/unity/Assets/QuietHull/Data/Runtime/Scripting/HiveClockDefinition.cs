using System;
using UnityEngine;

namespace QuietHull.Data
{
    /// <summary>
    /// A real hive for semi-open missions: Dormant, Restless and Hunt, each entered by an event or by
    /// time, whichever comes first. Replaces scripted waves. Pure state transitions; the sim owns the clock.
    /// </summary>
    [CreateAssetMenu(menuName = "Quiet Hull/Data/Scripting/Hive Clock", fileName = "hive_")]
    public sealed class HiveClockDefinition : DefinitionBase
    {
        [Header("Dormant → Restless")]
        [Tooltip("Event that wakes it (e.g. console_powered). Empty = time only.")]
        [SerializeField] internal string restlessOnEvent = string.Empty;
        [Min(0f)] [SerializeField] internal float restlessAfterSeconds = 600f;

        [Header("Restless → Hunt")]
        [SerializeField] internal string huntOnEvent = string.Empty;
        [Min(0f)] [SerializeField] internal float huntAfterSeconds = 1500f;

        [Header("Pressure per stage (Dormant, Restless, Hunt)")]
        [SerializeField] internal int[] patrolsByStage = { 2, 4, 6 };
        [Tooltip("Surge budget per minute, in surge cost (Husk 1, Crawler 2, ...).")]
        [SerializeField] internal float[] surgeBudgetPerMinuteByStage = { 0f, 6f, 16f };

        public string RestlessOnEvent => restlessOnEvent;
        public float RestlessAfterSeconds => restlessAfterSeconds;
        public string HuntOnEvent => huntOnEvent;
        public float HuntAfterSeconds => huntAfterSeconds;

        public int PatrolsAt(HiveStage stage) => patrolsByStage[(int)stage - 1];
        public float SurgeBudgetPerMinuteAt(HiveStage stage) => surgeBudgetPerMinuteByStage[(int)stage - 1];

        /// <summary>The stage after <paramref name="elapsedSeconds"/>, given an event that just fired (or null). Never goes back down.</summary>
        public HiveStage Next(HiveStage current, float elapsedSeconds, string firedEvent)
        {
            var stage = current;
            if (stage == HiveStage.Dormant &&
                (elapsedSeconds >= restlessAfterSeconds || (!string.IsNullOrEmpty(restlessOnEvent) && string.Equals(firedEvent, restlessOnEvent, StringComparison.Ordinal))))
            {
                stage = HiveStage.Restless;
            }

            if (stage == HiveStage.Restless &&
                (elapsedSeconds >= huntAfterSeconds || (!string.IsNullOrEmpty(huntOnEvent) && string.Equals(firedEvent, huntOnEvent, StringComparison.Ordinal))))
            {
                stage = HiveStage.Hunt;
            }

            return stage;
        }

        protected override void ValidateDefinition(ValidationReport report)
        {
            if (huntAfterSeconds <= restlessAfterSeconds)
            {
                report.Error(this, nameof(huntAfterSeconds), "Hunt must come after Restless.");
            }

            if (patrolsByStage == null || patrolsByStage.Length != 3 || surgeBudgetPerMinuteByStage == null || surgeBudgetPerMinuteByStage.Length != 3)
            {
                report.Error(this, nameof(patrolsByStage), "needs one value per stage (3).");
                return;
            }

            for (var i = 1; i < 3; i++)
            {
                if (surgeBudgetPerMinuteByStage[i] < surgeBudgetPerMinuteByStage[i - 1])
                {
                    report.Error(this, nameof(surgeBudgetPerMinuteByStage), "pressure must not drop as the hive escalates.");
                }
            }
        }
    }
}
