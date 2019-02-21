using System.Text;

namespace Networking
{
    public static class NetworkConfig
    {
        public static readonly UTF8Encoding Encoding = new UTF8Encoding();
        public static readonly float[] EncoderPrecisionScales = {1.0f, 10.0f, 100.0f, 1000.0f};
        public static readonly float[] DecoderPrecisionScales = {1.0f, 0.1f, 0.01f, 0.001f};

        private const int MaxFixedSchemaIds = 2;
        private const int MaxEventTypeSchemaIds = 8;
        private const int MaxEntityTypeSchemaIds = 40;
        public const int MaxSchemaIds = MaxFixedSchemaIds + MaxEventTypeSchemaIds + MaxEntityTypeSchemaIds;

        public const int MaxFieldsPerSchema = 128;
        public const int MaxContextsPerField = 4;
        private const int MaxSkipContextsPerSchema = MaxContextsPerField / 4;
        public const int MaxContextsPerSchema = MaxSkipContextsPerSchema + MaxFieldsPerSchema * MaxContextsPerField;

        public const int MiscContext = 0;

        public const int FirstSchemaContext = 16;

        public const int MaxContexts = FirstSchemaContext + MaxSchemaIds * MaxContextsPerSchema;
    }
}