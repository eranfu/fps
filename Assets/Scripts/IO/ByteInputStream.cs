namespace IO
{
    public struct ByteInputStream
    {
        private byte[] _buffer;
        private int _currentByteIndex;

        public ushort ReadUInt16()
        {
            var value = (ushort) (_buffer[_currentByteIndex] | _buffer[_currentByteIndex + 1] << 8);
            _currentByteIndex += 2;
            return value;
        }

        public void ReadBytes(byte[] dest, int destIndex, int count, int maxCount)
        {
            if (dest != null)
                IoUtils.MemCopy(_buffer, _currentByteIndex, dest, destIndex, count);
            _currentByteIndex += maxCount;
        }
    }
}