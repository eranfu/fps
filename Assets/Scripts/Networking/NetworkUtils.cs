using System.Diagnostics;

namespace Networking
{
    public static class NetworkUtils
    {
        public static readonly Stopwatch StopWatch = new Stopwatch();

        public static void MemCopy(byte[] src, int srcIndex, byte[] dest, int destIndex, int count)
        {
            for (var i = 0; i < count; i++)
            {
                dest[destIndex + i] = src[srcIndex + i];
            }
        }
    }
}