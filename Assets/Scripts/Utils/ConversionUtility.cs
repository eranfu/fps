using System.Runtime.InteropServices;

namespace Utils
{
    public class ConversionUtility
    {
        public static float UInt32ToFloat(uint value)
        {
            return new UIntFloat {uintValue = value}.floatValue;
        }

        public static uint FloatToUInt32(float value)
        {
            return new UIntFloat {floatValue = value}.uintValue;
        }

        public static double ULongToDouble(ulong value)
        {
            return new ULongDouble() {ulongValue = value}.doubleValue;
        }

        public static ulong DoubleToULong(double value)
        {
            return new ULongDouble {doubleValue = value}.ulongValue;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UIntFloat
        {
            [FieldOffset(0)] public float floatValue;
            [FieldOffset(0)] public uint uintValue;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct ULongDouble
        {
            [FieldOffset(0)] public double doubleValue;
            [FieldOffset(0)] public ulong ulongValue;
        }
    }
}