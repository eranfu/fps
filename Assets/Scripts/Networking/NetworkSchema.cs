using System.Diagnostics;
using System.Globalization;
using Utils;
using Utils.Pool;
using Debug = UnityEngine.Debug;

namespace Networking
{
    public class NetworkSchema
    {
        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        public static void AddStatsToFieldBool(FieldInfo fieldInfo, bool value, bool prediction, int numBits)
        {
            ((FieldStats<FieldValueBool>) fieldInfo.stats)
                .Add(new FieldValueBool(value), new FieldValueBool(prediction), numBits);
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
                    int charLength = NetworkConfig.encoding.GetChars(valueBuffer, valueOffset, valueLength, chars, 0);
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