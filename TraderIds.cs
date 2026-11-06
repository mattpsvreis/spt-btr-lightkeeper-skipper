using System.Collections.Generic;

namespace Dark.BTRLightkeeperSkipper
{
    internal static class TraderIds
    {
        internal const string Lightkeeper = "638f541a29ffd1183d187f57";
        internal const string BtrDriver = "656f0f98d80a697f855d34b1";

        private static readonly HashSet<string> SupportedTraders = new HashSet<string>
        {
            Lightkeeper,
            BtrDriver
        };

        internal static bool IsSupported(string traderId)
        {
            return !string.IsNullOrEmpty(traderId) && SupportedTraders.Contains(traderId);
        }
    }
}
