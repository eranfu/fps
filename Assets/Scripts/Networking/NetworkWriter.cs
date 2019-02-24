using UnityEngine;
using Utils;

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
                NetworkSchema.FieldInfo field = _schema.GetField(_nextFieldIndex);
                Debug.Assert(field.name == name);
                Debug.Assert(field.fieldType == fieldType);
                Debug.Assert(field.arraySize == arraySize);
                Debug.Assert(field.bits == bits);
                Debug.Assert(field.delta == delta);
                Debug.Assert(field.precision == precision);
                Debug.Assert(field.fieldMask == _fieldMask);
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
    }
}