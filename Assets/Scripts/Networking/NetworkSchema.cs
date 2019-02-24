using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Utils;
using Utils.Pool;
using Debug = UnityEngine.Debug;

namespace Networking
{
    public class NetworkSchema
    {
        private readonly List<FieldInfo> _fields = new List<FieldInfo>();
        private int _nextByteOffset = 0;
        private int _id;

        public NetworkSchema(int id)
        {
            Debug.Assert(id >= 0 && id < NetworkConfig.MaxSchemaIds);
            this._id = id;
        }

        public int GetNextFieldStartContext()
        {
            return _fields.Count * NetworkConfig.MaxContextsPerField + _id * NetworkConfig.MaxContextsPerSchema +
                   NetworkConfig.FirstSchemaContext;
        }

        public int GetByteSize()
        {
            return _nextByteOffset;
        }

        public FieldInfo GetField(int index)
        {
            Debug.Assert(index >= 0 && index < _fields.Count);
            return _fields[index];
        }

        public void AddField(FieldInfo field)
        {
            Debug.Assert(_fields.Count < NetworkConfig.MaxFieldsPerSchema);
            field.byteOffset = _nextByteOffset;
            field.stats = FieldStatsBase.CreateFieldStats(field);
            _fields.Add(field);
            _nextByteOffset += CalcFieldByteSize(field);
        }

        private static int CalcFieldByteSize(FieldInfo field)
        {
            int size;
            switch (field.fieldType)
            {
                case FieldType.Bool:
                    size = 1;
                    break;
                case FieldType.Int:
                case FieldType.UInt:
                case FieldType.Float:
                    size = (field.bits + 7) / 8;
                    break;
                case FieldType.Vector2:
                    size = (field.bits + 7) / 8 * 2;
                    break;
                case FieldType.Vector3:
                    size = (field.bits + 7) / 8 * 3;
                    break;
                case FieldType.Quaternion:
                    size = (field.bits + 7) / 8 * 4;
                    break;
                case FieldType.String:
                case FieldType.ByteArray:
                    size = 2 + field.arraySize;
                    break;
                default:
                    size = 0;
                    Debug.Assert(false);
                    break;
            }

            return size;
        }

        public static NetworkSchema ReadSchema<TInputStream>(ref TInputStream input) where TInputStream : IInputStream
        {
            var count = (int) input.ReadPackedUInt(NetworkConfig.MiscContext);
            var id = (int) input.ReadPackedUInt(NetworkConfig.MiscContext);
            var schema = new NetworkSchema(id);
            for (var i = 0; i < count; i++)
            {
                var fieldInfo = new FieldInfo
                {
                    fieldType = (FieldType) input.ReadRawBits(4),
                    delta = input.ReadRawBits(1) != 0,
                    bits = (int) input.ReadRawBits(6),
                    precision = (int) input.ReadRawBits(2),
                    arraySize = (int) input.ReadRawBits(16),
                    startContext = schema.GetNextFieldStartContext(),
                    fieldMask = (byte) input.ReadRawBits(8)
                };
                schema.AddField(fieldInfo);
            }

            return schema;
        }

        public static void WriteSchema<TOutputStream>(NetworkSchema schema, ref TOutputStream output)
            where TOutputStream : IOutputStream
        {
            output.WritePackedUInt((uint) schema._fields.Count, NetworkConfig.MiscContext);
            output.WritePackedUInt((uint) schema._id, NetworkConfig.MiscContext);
            foreach (FieldInfo field in schema._fields)
            {
                output.WriteRawBits((uint) field.fieldType, 4);
                output.WriteRawBits(field.delta ? 1u : 0u, 1);
                output.WriteRawBits((uint) field.bits, 6);
                output.WriteRawBits((uint) field.precision, 2);
                output.WriteRawBits((uint) field.arraySize, 16);
                output.WriteRawBits(field.fieldMask, 8);
            }
        }

        public static void CopyFieldsFromBuffer<TOutputStream>(
            NetworkSchema schema, byte[] inputBuffer, ref TOutputStream output) where TOutputStream : IOutputStream
        {
            var input = new ByteInputStream(inputBuffer);
            foreach (FieldInfo field in schema._fields)
            {
                switch (field.fieldType)
                {
                    case FieldType.Bool:
                    case FieldType.Int:
                    case FieldType.UInt:
                    case FieldType.Float:
                        output.WriteRawBits(input.ReadBits(field.bits), field.bits);
                        break;
                    case FieldType.Vector2:
                        output.WriteRawBits(input.ReadUInt32(), field.bits);
                        output.WriteRawBits(input.ReadUInt32(), field.bits);
                        break;
                    case FieldType.Vector3:
                        output.WriteRawBits(input.ReadUInt32(), field.bits);
                        output.WriteRawBits(input.ReadUInt32(), field.bits);
                        output.WriteRawBits(input.ReadUInt32(), field.bits);
                        break;
                    case FieldType.Quaternion:
                        output.WriteRawBits(input.ReadUInt32(), field.bits);
                        output.WriteRawBits(input.ReadUInt32(), field.bits);
                        output.WriteRawBits(input.ReadUInt32(), field.bits);
                        output.WriteRawBits(input.ReadUInt32(), field.bits);
                        break;
                    case FieldType.String:
                    case FieldType.ByteArray:
                    {
                        input.GetByteArray(out byte[] buffer, out int startIndex, out int length, field.arraySize);
                        output.WritePackedUInt((uint) length, field.startContext);
                        output.WriteRawBytes(buffer, startIndex, length);
                    }
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        public static void CopyFieldsToBuffer<TInputStream>(
            NetworkSchema schema, ref TInputStream input, byte[] outputBuffer) where TInputStream : IInputStream
        {
            var output = new ByteOutputStream(outputBuffer);
            foreach (FieldInfo field in schema._fields)
            {
                switch (field.fieldType)
                {
                    case FieldType.Bool:
                    case FieldType.Int:
                    case FieldType.UInt:
                    case FieldType.Float:
                        output.WriteBits(input.ReadRawBits(field.bits), field.bits);
                        break;
                    case FieldType.Vector2:
                        output.WriteUInt32(input.ReadRawBits(field.bits));
                        output.WriteUInt32(input.ReadRawBits(field.bits));
                        break;
                    case FieldType.Vector3:
                        output.WriteUInt32(input.ReadRawBits(field.bits));
                        output.WriteUInt32(input.ReadRawBits(field.bits));
                        output.WriteUInt32(input.ReadRawBits(field.bits));
                        break;
                    case FieldType.Quaternion:
                        output.WriteUInt32(input.ReadRawBits(field.bits));
                        output.WriteUInt32(input.ReadRawBits(field.bits));
                        output.WriteUInt32(input.ReadRawBits(field.bits));
                        output.WriteUInt32(input.ReadRawBits(field.bits));
                        break;
                    case FieldType.String:
                    case FieldType.ByteArray:
                        output.CopyByteArray(ref input, field.arraySize, field.startContext);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        public static void SkipFields<TInputStream>(NetworkSchema schema, ref TInputStream input)
            where TInputStream : IInputStream
        {
            foreach (FieldInfo field in schema._fields)
            {
                switch (field.fieldType)
                {
                    case FieldType.Bool:
                    case FieldType.Int:
                    case FieldType.UInt:
                    case FieldType.Float:
                        input.ReadRawBits(field.bits);
                        break;
                    case FieldType.Vector2:
                        input.ReadRawBits(field.bits);
                        input.ReadRawBits(field.bits);
                        break;
                    case FieldType.Vector3:
                        input.ReadRawBits(field.bits);
                        input.ReadRawBits(field.bits);
                        input.ReadRawBits(field.bits);
                        break;
                    case FieldType.Quaternion:
                        input.ReadRawBits(field.bits);
                        input.ReadRawBits(field.bits);
                        input.ReadRawBits(field.bits);
                        input.ReadRawBits(field.bits);
                        break;
                    case FieldType.String:
                    case FieldType.ByteArray:
                        input.SkipRawBytes((int) input.ReadPackedUInt(field.startContext));
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        // Functions for updating stats on a field that can be conditionally excluded from the build
        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldBool(FieldInfo fieldInfo, bool value, bool prediction, int numBits)
        {
            ((FieldStats<FieldValueBool>) fieldInfo.stats).Add(
                new FieldValueBool(value), new FieldValueBool(prediction), numBits);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldInt(FieldInfo fieldInfo, int value, int prediction, int numBits)
        {
            ((FieldStats<FieldValueInt>) fieldInfo.stats).Add(
                new FieldValueInt(value), new FieldValueInt(prediction), numBits);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldUInt(FieldInfo fieldInfo, uint value, uint prediction, int numBits)
        {
            ((FieldStats<FieldValueUInt>) fieldInfo.stats).Add(
                new FieldValueUInt(value), new FieldValueUInt(prediction), numBits);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldFloat(FieldInfo fieldInfo, uint value, uint prediction, int numBits)
        {
            ((FieldStats<FieldValueFloat>) fieldInfo.stats).Add(
                new FieldValueFloat(value), new FieldValueFloat(prediction), numBits);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldVector2(FieldInfo fieldInfo, uint vx, uint vy, uint px, uint py, int numBits)
        {
            ((FieldStats<FieldValueVector2>) fieldInfo.stats).Add(
                new FieldValueVector2(vx, vy), new FieldValueVector2(px, py), numBits);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldVector3(FieldInfo fieldInfo, uint vx, uint vy, uint vz, uint px, uint py,
            uint pz, int numBits)
        {
            ((FieldStats<FieldValueVector3>) fieldInfo.stats).Add(
                new FieldValueVector3(vx, vy, vz), new FieldValueVector3(px, py, pz), numBits);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldQuaternion(FieldInfo fieldInfo, uint vx, uint vy, uint vz, uint vw, uint px,
            uint py, uint pz, uint pw, int numBits)
        {
            ((FieldStats<FieldValueQuaternion>) fieldInfo.stats).Add(
                new FieldValueQuaternion(vx, vy, vz, vw), new FieldValueQuaternion(px, py, pz, pw), numBits);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldString(FieldInfo fieldInfo, byte[] valueBuffer, int valueOffset,
            int valueLength, int numBits)
        {
            ((FieldStats<FieldValueString>) fieldInfo.stats).Add(
                new FieldValueString(valueBuffer, valueOffset, valueLength), new FieldValueString(""), numBits);
        }

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldByteArray(FieldInfo fieldInfo, byte[] valueBuffer, int valueOffset,
            int valueLength, int numBits)
        {
            ((FieldStats<FieldValueByteArray>) fieldInfo.stats).Add(
                new FieldValueByteArray(valueBuffer, valueOffset, valueLength), new FieldValueByteArray(), numBits);
        }

        public enum FieldType
        {
            Bool,
            Int,
            UInt,
            Float,
            Vector2,
            Vector3,
            Quaternion,
            String,
            ByteArray,
        }

        public abstract class FieldStatsBase
        {
            public int NumWrites { get; protected set; }
            public int NumBitsWritten { get; protected set; }

            public static FieldStatsBase CreateFieldStats(FieldInfo fieldInfo)
            {
                switch (fieldInfo.fieldType)
                {
                    case FieldType.Bool:
                        return new FieldStats<FieldValueBool>(fieldInfo);
                    case FieldType.Int:
                        return new FieldStats<FieldValueInt>(fieldInfo);
                    case FieldType.UInt:
                        return new FieldStats<FieldValueUInt>(fieldInfo);
                    case FieldType.Float:
                        return new FieldStats<FieldValueFloat>(fieldInfo);
                    case FieldType.Vector2:
                        return new FieldStats<FieldValueVector2>(fieldInfo);
                    case FieldType.Vector3:
                        return new FieldStats<FieldValueVector3>(fieldInfo);
                    case FieldType.Quaternion:
                        return new FieldStats<FieldValueQuaternion>(fieldInfo);
                    case FieldType.String:
                        return new FieldStats<FieldValueString>(fieldInfo);
                    case FieldType.ByteArray:
                        return new FieldStats<FieldValueByteArray>(fieldInfo);
                    default:
                        Debug.Assert(false, $"Error field type: {(int) fieldInfo.fieldType}");
                        return null;
                }
            }

            public abstract string GetValue(bool showRaw);
            public abstract string GetValueMin(bool showRaw);
            public abstract string GetValueMax(bool showRaw);

            public abstract string GetPrediction(bool showRaw);
            public abstract string GetPredictionMin(bool showRaw);
            public abstract string GetPredictionMax(bool showRaw);

            public abstract string GetDelta(bool showRaw);
            public abstract string GetDeltaMin(bool showRaw);
            public abstract string GetDeltaMax(bool showRaw);
        }

        private class FieldStats<T> : FieldStatsBase where T : IFieldValue<T>
        {
            private T _value;
            private T _valueMin;
            private T _valueMax;
            private T _prediction;
            private T _predictionMin;
            private T _predictionMax;
            private T _delta;
            private T _deltaMin;
            private T _deltaMax;

            private readonly FieldInfo _fieldInfo;

            public FieldStats(FieldInfo fieldInfo)
            {
                _fieldInfo = fieldInfo;
            }

            public void Add(T value, T prediction, int numBits)
            {
                this._value = value;
                this._prediction = prediction;
                this._delta = value.Sub(prediction);

                if (NumWrites > 0)
                {
                    this._valueMin = this._valueMin.Min(value);
                    this._valueMax = this._valueMax.Max(value);
                    this._predictionMin = this._predictionMin.Min(prediction);
                    this._predictionMax = this._predictionMax.Max(prediction);
                    this._deltaMin = this._deltaMin.Min(this._delta);
                    this._deltaMax = this._deltaMax.Max(this._delta);
                }
                else
                {
                    this._valueMin = value;
                    this._valueMax = value;
                    this._predictionMin = prediction;
                    this._predictionMax = prediction;
                    this._deltaMin = this._delta;
                    this._deltaMax = this._delta;
                }

                this.NumWrites++;
                this.NumBitsWritten += numBits;
            }

            public override string GetValue(bool showRaw)
            {
                return this._value.ToString(this._fieldInfo, showRaw);
            }

            public override string GetValueMin(bool showRaw)
            {
                return this._valueMin.ToString(this._fieldInfo, showRaw);
            }

            public override string GetValueMax(bool showRaw)
            {
                return this._valueMax.ToString(this._fieldInfo, showRaw);
            }

            public override string GetPrediction(bool showRaw)
            {
                return this._prediction.ToString(this._fieldInfo, showRaw);
            }

            public override string GetPredictionMin(bool showRaw)
            {
                return this._predictionMin.ToString(this._fieldInfo, showRaw);
            }

            public override string GetPredictionMax(bool showRaw)
            {
                return this._predictionMax.ToString(this._fieldInfo, showRaw);
            }

            public override string GetDelta(bool showRaw)
            {
                return this._delta.ToString(this._fieldInfo, showRaw);
            }

            public override string GetDeltaMin(bool showRaw)
            {
                return this._deltaMin.ToString(this._fieldInfo, showRaw);
            }

            public override string GetDeltaMax(bool showRaw)
            {
                return this._deltaMax.ToString(this._fieldInfo, showRaw);
            }
        }

        public class FieldInfo
        {
            public string name;
            public FieldType fieldType;
            public int bits;
            public bool delta;
            public int precision;
            public int arraySize;
            public int byteOffset;
            public int startContext;
            public byte fieldMask;
            public FieldStatsBase stats;
        }

        private interface IFieldValue<T>
        {
            T Min(T other);
            T Max(T other);
            T Sub(T other);
            string ToString(FieldInfo fieldInfo, bool showRaw);
        }

        public struct FieldValueBool : IFieldValue<FieldValueBool>
        {
            private readonly bool _value;

            public FieldValueBool(bool value)
            {
                _value = value;
            }

            public FieldValueBool Min(FieldValueBool other)
            {
                return new FieldValueBool(_value && other._value);
            }

            public FieldValueBool Max(FieldValueBool other)
            {
                return new FieldValueBool(_value || other._value);
            }

            public FieldValueBool Sub(FieldValueBool other)
            {
                return new FieldValueBool(_value != other._value);
            }

            public string ToString(FieldInfo fieldInfo, bool showRaw)
            {
                return _value.ToString();
            }
        }

        public struct FieldValueInt : IFieldValue<FieldValueInt>
        {
            private readonly int _value;

            public FieldValueInt(int value)
            {
                _value = value;
            }

            public FieldValueInt Min(FieldValueInt other)
            {
                int otherValue = other._value;
                return new FieldValueInt(_value < otherValue ? _value : otherValue);
            }

            public FieldValueInt Max(FieldValueInt other)
            {
                int otherValue = other._value;
                return new FieldValueInt(_value > otherValue ? _value : otherValue);
            }

            public FieldValueInt Sub(FieldValueInt other)
            {
                return new FieldValueInt(_value - other._value);
            }

            public string ToString(FieldInfo fieldInfo, bool showRaw)
            {
                return _value.ToString();
            }
        }

        public struct FieldValueUInt : IFieldValue<FieldValueUInt>
        {
            private readonly uint _value;

            public FieldValueUInt(uint value)
            {
                _value = value;
            }

            public FieldValueUInt Min(FieldValueUInt other)
            {
                uint otherValue = other._value;
                return new FieldValueUInt(_value < otherValue ? _value : otherValue);
            }

            public FieldValueUInt Max(FieldValueUInt other)
            {
                uint otherValue = other._value;
                return new FieldValueUInt(_value > otherValue ? _value : otherValue);
            }

            public FieldValueUInt Sub(FieldValueUInt other)
            {
                return new FieldValueUInt(_value - other._value);
            }

            public string ToString(FieldInfo fieldInfo, bool showRaw)
            {
                return _value.ToString();
            }
        }

        public struct FieldValueFloat : IFieldValue<FieldValueFloat>
        {
            private readonly uint _value;

            public FieldValueFloat(uint value)
            {
                _value = value;
            }

            public FieldValueFloat Min(FieldValueFloat other)
            {
                uint otherValue = other._value;
                return new FieldValueFloat(_value < otherValue ? _value : otherValue);
            }

            public FieldValueFloat Max(FieldValueFloat other)
            {
                uint otherValue = other._value;
                return new FieldValueFloat(_value > otherValue ? _value : otherValue);
            }

            public FieldValueFloat Sub(FieldValueFloat other)
            {
                return new FieldValueFloat(_value - other._value);
            }

            public string ToString(FieldInfo fieldInfo, bool showRaw)
            {
                if (showRaw)
                {
                    return _value.ToString();
                }
                else
                {
                    if (fieldInfo.delta)
                    {
                        return
                            ((int) _value * NetworkConfig.DecoderPrecisionScales[fieldInfo.precision]).ToString(
                                CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        return ConversionUtility.UInt32ToFloat(_value).ToString(CultureInfo.CurrentCulture);
                    }
                }
            }
        }

        public struct FieldValueVector2 : IFieldValue<FieldValueVector2>
        {
            private readonly uint _x, _y;

            public FieldValueVector2(uint x, uint y)
            {
                this._x = x;
                this._y = y;
            }

            public FieldValueVector2 Min(FieldValueVector2 other)
            {
                return new FieldValueVector2(0, 0);
            }

            public FieldValueVector2 Max(FieldValueVector2 other)
            {
                return new FieldValueVector2(0, 0);
            }

            public FieldValueVector2 Sub(FieldValueVector2 other)
            {
                return new FieldValueVector2(this._x - other._x, this._y - other._y);
            }

            public string ToString(FieldInfo fieldInfo, bool showRaw)
            {
                if (fieldInfo.delta)
                {
                    float scale = NetworkConfig.DecoderPrecisionScales[fieldInfo.precision];
                    return $"({(int) this._x * scale}, {(int) this._y * scale})";
                }
                else
                {
                    return $"({ConversionUtility.UInt32ToFloat(this._x)}, {ConversionUtility.UInt32ToFloat(this._y)})";
                }
            }
        }

        public struct FieldValueVector3 : IFieldValue<FieldValueVector3>
        {
            private readonly uint _x, _y, _z;

            public FieldValueVector3(uint x, uint y, uint z)
            {
                _x = x;
                _y = y;
                _z = z;
            }

            public FieldValueVector3 Min(FieldValueVector3 other)
            {
                return new FieldValueVector3(0, 0, 0);
            }

            public FieldValueVector3 Max(FieldValueVector3 other)
            {
                return new FieldValueVector3(0, 0, 0);
            }

            public FieldValueVector3 Sub(FieldValueVector3 other)
            {
                return new FieldValueVector3(this._x - other._x, this._y - other._y, this._z - other._z);
            }

            public string ToString(FieldInfo fieldInfo, bool showRaw)
            {
                if (fieldInfo.delta)
                {
                    float scale = NetworkConfig.DecoderPrecisionScales[fieldInfo.precision];
                    return $"({(int) _x * scale}, {(int) _y * scale}, {(int) _z * scale})";
                }
                else
                {
                    return
                        $"({ConversionUtility.UInt32ToFloat(_x)}, {ConversionUtility.UInt32ToFloat(_y)}, {ConversionUtility.UInt32ToFloat(_z)})";
                }
            }
        }

        public struct FieldValueQuaternion : IFieldValue<FieldValueQuaternion>
        {
            private readonly uint _x, _y, _z, _w;

            public FieldValueQuaternion(uint x, uint y, uint z, uint w)
            {
                _x = x;
                _y = y;
                _z = z;
                _w = w;
            }

            public FieldValueQuaternion Min(FieldValueQuaternion other)
            {
                return new FieldValueQuaternion(0, 0, 0, 0);
            }

            public FieldValueQuaternion Max(FieldValueQuaternion other)
            {
                return new FieldValueQuaternion(0, 0, 0, 0);
            }

            public FieldValueQuaternion Sub(FieldValueQuaternion other)
            {
                return new FieldValueQuaternion(this._x - other._x, this._y - other._y, this._z - other._z,
                    this._w - other._w);
            }

            public string ToString(FieldInfo fieldInfo, bool showRaw)
            {
                if (fieldInfo.delta)
                {
                    float scale = NetworkConfig.DecoderPrecisionScales[fieldInfo.precision];
                    return $"({(int) _x * scale}, {(int) _y * scale}, {(int) _z * scale}, {(int) _w * scale})";
                }
                else
                {
                    return
                        $"({ConversionUtility.UInt32ToFloat(_x)}, {ConversionUtility.UInt32ToFloat(_y)}, " +
                        $"{ConversionUtility.UInt32ToFloat(_z)}, {ConversionUtility.UInt32ToFloat(_w)})";
                }
            }
        }

        public struct FieldValueString : IFieldValue<FieldValueString>
        {
            private readonly string _value;

            public FieldValueString(string value)
            {
                _value = value;
            }

            public FieldValueString(byte[] valueBuffer, int valueOffset, int valueLength)
            {
                if (valueBuffer == null)
                {
                    _value = "";
                }
                else
                {
                    char[] chars = BufferPool.Pop<char>(1024 * 32);
                    int charLength = NetworkConfig.Encoding.GetChars(valueBuffer, valueOffset, valueLength, chars, 0);
                    _value = new string(chars, 0, charLength);
                    BufferPool.Push(chars);
                }
            }

            public FieldValueString Min(FieldValueString other)
            {
                return new FieldValueString("");
            }

            public FieldValueString Max(FieldValueString other)
            {
                return new FieldValueString("");
            }

            public FieldValueString Sub(FieldValueString other)
            {
                return new FieldValueString("");
            }

            public string ToString(FieldInfo fieldInfo, bool showRaw)
            {
                return _value;
            }
        }

        public struct FieldValueByteArray : IFieldValue<FieldValueByteArray>
        {
            private readonly byte[] _buffer;

            public FieldValueByteArray(byte[] value, int offset, int length)
            {
                if (value != null)
                {
                    _buffer = new byte[length];
                    Array.Copy(value, offset, _buffer, 0, length);
                }
                else
                {
                    _buffer = null;
                }
            }

            public FieldValueByteArray Min(FieldValueByteArray other)
            {
                return new FieldValueByteArray();
            }

            public FieldValueByteArray Max(FieldValueByteArray other)
            {
                return new FieldValueByteArray();
            }

            public FieldValueByteArray Sub(FieldValueByteArray other)
            {
                return new FieldValueByteArray();
            }

            public string ToString(FieldInfo fieldInfo, bool showRaw)
            {
                return "";
            }
        }
    }
}