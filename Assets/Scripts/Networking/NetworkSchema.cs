using UnityEngine;

namespace Networking
{
    public class NetworkSchema
    {
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
                        return null;
                    case FieldType.Int:
                        return null;
                    case FieldType.UInt:
                        return null;
                    case FieldType.Float:
                        return null;
                    case FieldType.Vector2:
                        return null;
                    case FieldType.Vector3:
                        return null;
                    case FieldType.Quaternion:
                        return null;
                    case FieldType.String:
                        return null;
                    case FieldType.ByteArray:
                        return null;
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

            private FieldInfo _fieldInfo;

            public FieldStats(FieldInfo fieldInfo)
            {
                _fieldInfo = fieldInfo;
            }

            public void Add(T value, T prediction, int bitsWritten)
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
                this.NumBitsWritten += bitsWritten;
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
    }
}