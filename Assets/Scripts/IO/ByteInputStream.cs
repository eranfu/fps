using System.Net;
using System.Text;
using UnityEngine;

namespace IO
{
    public struct ByteInputStream
    {
        private readonly byte[] _buffer;
        private int _currentByteIndex;

        public ByteInputStream(byte[] buffer) : this()
        {
            _buffer = buffer;
            _currentByteIndex = 0;
        }

        public int GetBytePosition()
        {
            return _currentByteIndex;
        }

        public uint ReadBits(int numBits)
        {
            switch (numBits)
            {
                case 1:
                case 8:
                    return ReadUInt8();
                case 16:
                    return ReadUInt16();
                case 32:
                    return ReadUInt32();
                default:
                    Debug.Assert(false, $"Argument out of range, numBits: {numBits}");
                    return 0;
            }
        }

        private byte ReadUInt8()
        {
            return _buffer[_currentByteIndex++];
        }

        public ushort ReadUInt16()
        {
            var value = (ushort) (_buffer[_currentByteIndex] | _buffer[_currentByteIndex + 1] << 8);
            _currentByteIndex += 2;
            return value;
        }

        // NBO: Network byte order
        public ushort ReadUInt16_NBO()
        {
            return (ushort) IPAddress.NetworkToHostOrder((short) ReadUInt16());
        }

        private uint ReadUInt32()
        {
            var value = (uint) (
                _buffer[_currentByteIndex + 0] |
                _buffer[_currentByteIndex + 1] << 8 |
                _buffer[_currentByteIndex + 2] << 16 |
                _buffer[_currentByteIndex + 3] << 32);
            _currentByteIndex += 4;
            return value;
        }

        public uint ReadUInt32_NBO()
        {
            return (uint) IPAddress.NetworkToHostOrder((int) ReadUInt32());
        }

        public void GetByteArray(out byte[] buffer, out int startIndex, out int length, int maxCount)
        {
            length = ReadUInt16();
            if (length > 0)
            {
                buffer = _buffer;
                startIndex = _currentByteIndex;
            }
            else
            {
                buffer = null;
                startIndex = -1;
            }

            _currentByteIndex += maxCount;
        }

        public void ReadBytes(byte[] dest, int destIndex, int length, int maxCount)
        {
            if (dest != null)
                IoUtils.MemCopy(_buffer, _currentByteIndex, dest, destIndex, length);
            _currentByteIndex += maxCount;
        }

        public int ReadByteArray(byte[] dest, int destIndex, int maxCount)
        {
            int length = ReadUInt16();
            ReadBytes(dest, destIndex, length, maxCount);
            return length;
        }

        public void SkipBytes(int count)
        {
            _currentByteIndex += count;
        }

        public void SkipByteArray(int maxCount)
        {
            _currentByteIndex += 2 + maxCount;
        }

        public void Reset()
        {
            _currentByteIndex = 0;
        }

        public string ReadString(Encoding encoding)
        {
            int length = ReadUInt16();
            string value = encoding.GetString(_buffer, _currentByteIndex, length);
            _currentByteIndex += length;
            return value;
        }
    }
}