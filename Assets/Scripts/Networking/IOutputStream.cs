namespace Networking
{
    public interface IOutputStream
    {
        void WritePackedUInt(uint count, int miscContext);
        void WriteRawBits(uint value, int numBits);
        void WriteRawBytes(byte[] buffer, int startIndex, int length);
    }
}