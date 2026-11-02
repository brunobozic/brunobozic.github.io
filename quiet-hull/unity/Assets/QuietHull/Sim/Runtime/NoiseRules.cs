using System;

namespace QuietHull.Sim
{
    /// <summary>How sound travels through a hulk. Immutable; tune by making a new one.</summary>
    public sealed class NoiseRules
    {
        public NoiseRules(int lossPerHop, int closedDoorLoss)
        {
            if (lossPerHop < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lossPerHop), lossPerHop, "sound must fade with distance.");
            }

            if (closedDoorLoss < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(closedDoorLoss), closedDoorLoss, null);
            }

            LossPerHop = lossPerHop;
            ClosedDoorLoss = closedDoorLoss;
        }

        /// <summary>2 lost per room crossed; a closed door costs 3 more; a sealed door stops it.</summary>
        public static NoiseRules Default { get; } = new NoiseRules(2, 3);

        public int LossPerHop { get; }
        public int ClosedDoorLoss { get; }
    }
}
