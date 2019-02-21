namespace Networking
{
    public static class IoUtils
    {
        public static void MemCopy(byte[] src, int srcIndex, byte[] dest, int destIndex, int count)
        {
            for (var i = 0; i < count; i++)
            {
                dest[destIndex + i] = src[srcIndex + i];
            }
        }
    }
}