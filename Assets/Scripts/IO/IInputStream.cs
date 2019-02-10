namespace IO
{
    public interface IInputStream
    {
        uint ReadPackedUInt(int context);
        void ReadRawBytes(byte[] dstBuffer, int dstIndex, int count);
    }
}