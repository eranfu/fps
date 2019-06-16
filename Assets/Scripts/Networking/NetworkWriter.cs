using System;
using UnityEngine;
using Utils;
using Utils.Pool;

namespace Networking
{
    public struct NetworkWriter
    {
        #region

        public enum FieldSectionType
        {
            OnlyPredicting,
            OnlyNotPredicting
        }

        public enum OverrunBehaviour
        {
            AssertMaxLength,
            WarnAndTrunc,
            SilentTrunc,
        }

        #endregion

        private readonly NetworkSchema _schema;
        private NetworkSchema.FieldInfo _currentField;
        private ByteOutputStream _output;
        private int _nextFieldIndex;
        private byte _fieldMask;
        private readonly bool _generateSchema;

        public NetworkWriter(byte[] buffer, NetworkSchema schema, bool generateSchema = false)
        {
            _output = new ByteOutputStream(buffer);
            _schema = schema;
            _currentField = null;
            _nextFieldIndex = 0;
            _generateSchema = generateSchema;
            _fieldMask = 0;
        }

        public int GetLength()
        {
            return _output.GetBytePosition();
        }

        private void ValidateOrGenerateSchema(
            string name, NetworkSchema.FieldType fieldType, int bits = 0,
            bool delta = false, int precision = 0, int arraySize = 0)
        {
            Debug.Assert(precision < 4);
            if (_generateSchema)
            {
                _schema.AddField(new NetworkSchema.FieldInfo
                {
                    name = name,
                    fieldType = fieldType,
                    bits = bits,
                    arraySize = arraySize,
                    delta = delta,
                    fieldMask = _fieldMask,
                    precision = precision,
                    startContext = _schema.GetNextFieldStartContext()
                });
            }
            else if (_schema != null)
            {
                _currentField = _schema.GetField(_nextFieldIndex);
                Debug.Assert(_currentField.name == name);
                Debug.Assert(_currentField.fieldType == fieldType);
                Debug.Assert(_currentField.arraySize == arraySize);
                Debug.Assert(_currentField.bits == bits);
                Debug.Assert(_currentField.delta == delta);
                Debug.Assert(_currentField.precision == precision);
                Debug.Assert(_currentField.fieldMask == _fieldMask);
                ++_nextFieldIndex;
            }
        }

        public void SetFieldSection(FieldSectionType type)
        {
            Debug.Assert(_fieldMask == 0, "Field masks can not be combined.");
            _fieldMask = type == FieldSectionType.OnlyNotPredicting ? (byte) 0x1 : (byte) 0x2;
        }

        public void ClearFieldSection()
        {
            Debug.Assert(_fieldMask != 0, "Trying to clear a field mask but none has been set.");
            _fieldMask = 0;
        }

        public void WriteBoolean(string name, bool value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Bool, 1);
            _output.WriteUInt8((byte) (value ? 1 : 0));
        }

        public void WriteByte(string name, byte value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.UInt, 8, true);
            _output.WriteUInt8(value);
        }

        public void WriteInt16(string name, short value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.UInt, 16, true);
            _output.WriteUInt16((ushort) value);
        }

        public void WriteUInt16(string name, ushort value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.UInt, 16, true);
            _output.WriteUInt16(value);
        }

        public void WriteInt32(string name, int value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Int, 32, true);
            _output.WriteUInt32((uint) value);
        }

        public void WriteUInt32(string name, uint value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.UInt, 32, true);
            _output.WriteUInt32(value);
        }

        public void WriteFloat(string name, float value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Float, 32);
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value));
        }

        public void WriteFloatQ(string name, float value, int precision = 3)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Float, 32, true, precision);
            _output.WriteUInt32((uint) Mathf.RoundToInt(value * NetworkConfig.EncoderPrecisionScales[precision]));
        }

        public void WriteString(string name, string value, int maxLength = 64,
            OverrunBehaviour overrunBehaviour = OverrunBehaviour.WarnAndTrunc)
        {
            if (value == null)
            {
                value = "";
            }

            char[] buffer = BufferPool.Pop<char>(value.Length);
            value.CopyTo(0, buffer, 0, value.Length);
            WriteString(name, buffer, value.Length, maxLength, overrunBehaviour);
            BufferPool.Push(buffer);
        }

        public void WriteString(string name, char[] value, int length, int maxLength, OverrunBehaviour overrunBehaviour)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.String, 0, false, 0, maxLength);
            byte[] buffer = BufferPool.Pop<byte>(maxLength);
            int byteCount = NetworkConfig.Encoding.GetBytes(value, 0, length, buffer, 0);
            if (byteCount > maxLength)
            {
                if (overrunBehaviour == OverrunBehaviour.AssertMaxLength)
                {
                    Debug.Assert(
                        false,
                        $"NetworkWriter: string {value} is too long. (Using {byteCount / maxLength} allowed encoded bytes): ");
                }

                // truncate
                string truncWithBadEnd = NetworkConfig.Encoding.GetString(buffer, 0, maxLength);
                string truncOk = truncWithBadEnd.Substring(0, truncWithBadEnd.Length - 1);
                int newByteCount = NetworkConfig.Encoding.GetBytes(truncOk, 0, truncOk.Length, buffer, 0);

                if (overrunBehaviour == OverrunBehaviour.WarnAndTrunc)
                {
                    Debug.LogWarning(
                        $"NetworkWriter: string {value} is truncated with {byteCount - newByteCount} bytes. (result: {truncOk})");
                }

                byteCount = newByteCount;
                Debug.Assert(byteCount <= maxLength);
            }

            _output.WriteByteArray(buffer, 0, byteCount, maxLength);
            BufferPool.Push(buffer);
        }

        public void WriteBytes(string name, byte[] value, int startIndex, int length, int maxLength)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.ByteArray, 0, false, 0, maxLength);
            if (length > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"NetworkWriter: byte buffer is too big, length: {length}");
            _output.WriteByteArray(value, startIndex, length, maxLength);
        }

        public void WriteVector2(string name, Vector2 value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Vector2, 32);
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value.x));
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value.y));
        }

        public void WriteVector2Q(string name, Vector2 value, int precision = 3)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Vector3, 32, true, precision);
            _output.WriteUInt32((uint) Mathf.RoundToInt(value.x * NetworkConfig.EncoderPrecisionScales[precision]));
            _output.WriteUInt32((uint) Mathf.RoundToInt(value.y * NetworkConfig.EncoderPrecisionScales[precision]));
        }

        public void WriteVector3(string name, Vector3 value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Vector3, 32);
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value.x));
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value.y));
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value.z));
        }

        public void WriteVector3Q(string name, Vector3 value, int precision = 3)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Vector3, 32, true, precision);
            _output.WriteUInt32((uint) Mathf.RoundToInt(value.x * NetworkConfig.EncoderPrecisionScales[precision]));
            _output.WriteUInt32((uint) Mathf.RoundToInt(value.y * NetworkConfig.EncoderPrecisionScales[precision]));
            _output.WriteUInt32((uint) Mathf.RoundToInt(value.z * NetworkConfig.EncoderPrecisionScales[precision]));
        }

        public void WriteQuaternion(string name, Quaternion value)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Quaternion, 32);
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value.x));
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value.y));
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value.z));
            _output.WriteUInt32(ConversionUtility.FloatToUInt32(value.w));
        }

        public void WriteQuaternionQ(string name, Quaternion value, int precision = 3)
        {
            ValidateOrGenerateSchema(name, NetworkSchema.FieldType.Vector3, 32, true, precision);
            _output.WriteUInt32((uint) Mathf.RoundToInt(value.x * NetworkConfig.EncoderPrecisionScales[precision]));
            _output.WriteUInt32((uint) Mathf.RoundToInt(value.y * NetworkConfig.EncoderPrecisionScales[precision]));
            _output.WriteUInt32((uint) Mathf.RoundToInt(value.z * NetworkConfig.EncoderPrecisionScales[precision]));
            _output.WriteUInt32((uint) Mathf.RoundToInt(value.w * NetworkConfig.EncoderPrecisionScales[precision]));
        }
    }
}