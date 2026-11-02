namespace QuietHull.Data
{
    /// <summary>One line of the company's file on the candidate, as mission 10 prints it.</summary>
    public readonly struct ForecastLine
    {
        public ForecastLine(DecisionRecord record, string text)
        {
            Record = record;
            Text = text;
        }

        public DecisionRecord Record { get; }
        public string Text { get; }

        public override string ToString() => Text;
    }
}
