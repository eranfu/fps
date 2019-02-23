using System.Net;
using System.Text;
using UnityEngine;

namespace Networking
{
    public struct ByteOutputStream
    {
        private readonly byte[] _buffer;
        private int _currentByteIndex;

        public ByteOutputStream(byte[] buffer)
        {
            _buffer = buffer;
            _currentByteIndex = 0;
        }

        public int GetBytePosition()
        {
            return _currentByteIndex;
        }

        public void WriteBits(uint value, int numBits)
        {
            switch (numBits)
            {
                case 1:
                case 8:
                    WriteUInt8((byte) value);
                    break;
                case 16:
                    WriteUInt16((ushort) value);
                    break;
                case 32:
                    WriteUInt32(value);
                    break;
                default:
                    Debug.Assert(false, $"Error argument, numBits: {numBits}");
                    break;
            }
        }

        private void WriteUInt8(byte value)
        {
            _buffer[_currentByteIndex + 0] = value;
            _currentByteIndex += 1;
        }

        private void WriteUInt16(ushort value)
        {
            _buffer[_currentByteIndex + 0] = (byte) value;
            _buffer[_currentByteIndex + 1] = (byte) (value >> 8);
            _currentByteIndex += 2;
        }

        // NBO: Network byte order
        public void WriteUInt16_NBO(ushort value)
        {
            WriteUInt16((ushort) IPAddress.HostToNetworkOrder((short) value));
        }

        public void WriteUInt32(uint value)
        {
            _buffer[_currentByteIndex + 0] = (byte) value;
            _buffer[_currentByteIndex + 1] = (byte) (value >> 8);
            _buffer[_currentByteIndex + 2] = (byte) (value >> 16);
            _buffer[_currentByteIndex + 3] = (byte) (value >> 24);
            _currentByteIndex += 4;
        }

        public void WriteUInt32_NBO(uint value)
        {
            WriteUInt32((uint) IPAddress.HostToNetworkOrder((int) value));
        }

        private void WriteBytes(byte[] data, int srcIndex, int length)
        {
            Debug.Assert(data != null);
            IoUtils.MemCopy(data, srcIndex, _buffer, _currentByteIndex, length);
            _currentByteIndex += length;
        }

        private void WriteBytesOffset(byte[] data, int srcIndex, int offset, int length)
        {
            Debug.Assert(data != null);
            IoUtils.MemCopy(data, srcIndex, _buffer, offset, length);
            _currentByteIndex = offset + length;
        }

        public void WriteByteArray(byte[] data, int srcIndex, int length, int maxCount)
        {
            Debug.Assert(length <= maxCount);
            WriteUInt16((ushort) length);

            var i = 0;
            for (; i < length; i++)
            {
                _buffer[_currentByteIndex + i] = data[srcIndex + i];
            }

            for (; i < maxCount; i++)
            {
                _buffer[_currentByteIndex + i] = 0;
            }

            _currentByteIndex += maxCount;
        }

        public void CopyByteArray<T>(ref T input, int maxCount, int context) where T : IInputStream
        {
            var count = (int) input.ReadPackedUInt(context);
            Debug.Assert(count <= maxCount);
            WriteUInt16((ushort) count);
            if (count > 0)
            {
                input.ReadRawBytes(_buffer, _currentByteIndex, count);
            }

            for (int i = count; i < maxCount; i++)
            {
                _buffer[_currentByteIndex + i] = 0;
            }

            _currentByteIndex += maxCount;
        }

        public void CopyByteArray(ref ByteInputStream input, int maxCount)
        {
            int count = input.ReadUInt16();
            Debug.Assert(count <= maxCount);
            WriteUInt16((ushort) count);

            input.ReadBytes(_buffer, _currentByteIndex, count, maxCount);
            for (int i = count; i < maxCount; i++)
            {
                _buffer[_currentByteIndex + i] = 0;
            }

            _currentByteIndex += maxCount;
        }

        public void CopyBytes(ref ByteInputStream input, int count)
        {
            input.ReadBytes(_buffer, _currentByteIndex, count, count);
            _currentByteIndex += count;
        }

        public void Flush()
        {
        }

        public void WriteString(string value, Encoding encoding)
        {
            Encoder encoder = encoding.GetEncoder();
            char[] charArray = value.ToCharArray();
            encoder.Convert(charArray, 0, charArray.Length, _buffer, _currentByteIndex + 1,
                _buffer.Length - (_currentByteIndex + 2), true,
                out int _, out int byteUsed, out bool completed);
            Debug.Assert(completed, "Writing string overflow.");
            _buffer[_currentByteIndex + 0] = (byte) byteUsed;
            _currentByteIndex += 1 + byteUsed;
        }
    }
}