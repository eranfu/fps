namespace Networking
{
    public class NetworkConnectionCounters
    {
    }

    public class PackageInfo
    {
    }

    public class NetworkConnection<TCounters, TPackageInfo> where TCounters : NetworkConnectionCounters, new()
        where TPackageInfo : PackageInfo, new()
    {
        public readonly int connectionId;
        public int inSequence; // The highest sequence of packages we have received
        public long inSequenceTime; // The time the last package was received

        public int outSequence = 1; // The sequence of the next outgoing package

        public virtual void Reset()
        {
        }
    }
}