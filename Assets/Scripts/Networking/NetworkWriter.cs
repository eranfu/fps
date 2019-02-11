using IO;

namespace Networking
{
    public struct NetworkWriter
    {
        private readonly ByteOutputStream _output;
        private readonly NetworkSchema _schema;
        private readonly bool _generateSchema;
        private NetworkSchema.FieldInfo currentField;

        public NetworkWriter(byte[] buffer, NetworkSchema schema, bool generateSchema = false)
        {
            _output = new ByteOutputStream(buffer);
            _schema = schema;
            _generateSchema = generateSchema;
            currentField = null;
        }
    }
}