using IO;

namespace Networking
{
    public struct NetworkWriter
    {
        private ByteOutputStream _output;

        public NetworkWriter(byte[] buffer, NetworkSchema schema, bool generateSchema = false)
        {
            _output = new ByteOutputStream(buffer);
        }
    }
}