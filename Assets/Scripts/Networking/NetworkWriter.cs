using UnityEngine;

namespace Networking
{
    public struct NetworkWriter
    {
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
    }
}