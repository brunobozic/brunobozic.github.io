namespace QuietHull.Data
{
    /// <summary>
    /// A unit's signature as the player reads it: Noise (footsteps and vibration folded together), Heat and
    /// Aggro, plus Visibility. The sim keeps the detailed stats; the UI and the doc only use these.
    /// </summary>
    public readonly struct SignatureSummary
    {
        public SignatureSummary(float noise, float heat, float aggroPercent, float visibilityPercent)
        {
            Noise = noise;
            Heat = heat;
            AggroPercent = aggroPercent;
            VisibilityPercent = visibilityPercent;
        }

        public float Noise { get; }
        public float Heat { get; }

        /// <summary>How much more (or less) aliens prefer this unit as a target, e.g. 0.6 = +60%.</summary>
        public float AggroPercent { get; }

        /// <summary>Change to the range alien eyes see this unit at, e.g. −0.55 = seen at 45% range.</summary>
        public float VisibilityPercent { get; }

        public override string ToString() => $"Noise {Noise:0.#} · Heat {Heat:0.#} · Aggro {AggroPercent:+0%;-0%;0%}";
    }
}
