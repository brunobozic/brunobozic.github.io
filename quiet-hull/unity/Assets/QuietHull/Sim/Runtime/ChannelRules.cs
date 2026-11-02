using System;

namespace QuietHull.Sim
{
    /// <summary>How one sense channel travels: loss per room crossed, extra loss at a closed door, and whether sealed doors stop it.</summary>
    public sealed class ChannelRules
    {
        public ChannelRules(int lossPerHop, int closedDoorLoss, bool sealedBlocks)
        {
            if (lossPerHop < 1) throw new ArgumentOutOfRangeException(nameof(lossPerHop), lossPerHop, "a signal must fade with distance.");
            if (closedDoorLoss < 0) throw new ArgumentOutOfRangeException(nameof(closedDoorLoss), closedDoorLoss, null);

            LossPerHop = lossPerHop;
            ClosedDoorLoss = closedDoorLoss;
            SealedBlocks = sealedBlocks;
        }

        public int LossPerHop { get; }
        public int ClosedDoorLoss { get; }
        public bool SealedBlocks { get; }
    }
}
