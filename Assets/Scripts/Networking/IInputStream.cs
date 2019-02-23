namespace Networking
{
    public interface IInputStream
    {
        uint ReadPackedUInt(int context);

        uint ReadRawBits(int numBits);

        void ReadRawBytes(byte[] dstBuffer, int dstIndex, int length);
        void SkipRawBytes(int length);
    }
}