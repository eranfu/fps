namespace Networking
{
    public interface IInputStream
    {
        uint ReadRawBits(int numBits);
        void ReadRawBytes(byte[] dstBuffer, int dstIndex, int count);
     
        uint ReadPackedUInt(int context);
    }
}