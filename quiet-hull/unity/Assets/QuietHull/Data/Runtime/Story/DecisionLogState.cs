using System;

namespace QuietHull.Data
{
    /// <summary>The decision log as saved (JsonUtility-friendly: public fields, ids only, no asset references).</summary>
    [Serializable]
    public sealed class DecisionLogState
    {
        public DecisionRecordState[] records = Array.Empty<DecisionRecordState>();
    }

    /// <summary>One saved decision.</summary>
    [Serializable]
    public sealed class DecisionRecordState
    {
        public int sequence;
        public string decisionId;
        public string optionId;
        public int mission;
        public float predicted;
        public ForecastVerdict verdict;
    }
}
