using System;
using UnityEngine;

namespace Utils
{
    public class Interpolator
    {
        public enum CurveType
        {
            Linear,
            SmoothArrival,
            SmoothDeparture,
            SmoothStep
        }

        private float _startTime;
        private float _startValue;
        private float _targetTime;
        public float TargetValue { get; private set; }
        private readonly CurveType _type;

        public Interpolator(float startValue, CurveType curveType)
        {
            _type = curveType;
            SetValue(startValue);
        }

        private void SetValue(float value)
        {
            _startValue = value;
            TargetValue = value;
            _startTime = 0;
            _targetTime = 0;
        }

        public void MoveTo(float target, float duration)
        {
            _startValue = GetValue();
            TargetValue = target;
            _startTime = Time.realtimeSinceStartup;
            _targetTime = _startTime + duration;
        }

        public float GetValue()
        {
            float now = Time.realtimeSinceStartup;
            float timeToLive = _targetTime - now;
            if (timeToLive <= 0)
                return TargetValue;

            float t = (now - _startTime) / (_targetTime - _startTime);
            switch (_type)
            {
                case CurveType.Linear:
                    return _startValue + (TargetValue - _startValue) * t;
                case CurveType.SmoothArrival:
                    float s = 1 - t;
                    return _startValue + (TargetValue - _startValue) * (1 - s * s * s * s);
                case CurveType.SmoothDeparture:
                    return _startValue + (TargetValue - _startValue) * t * t * t * t;
                case CurveType.SmoothStep:
                    return _startValue + (TargetValue - _startValue) * t * t * (3 - 2 * t);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}