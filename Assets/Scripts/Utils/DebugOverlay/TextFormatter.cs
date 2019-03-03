using System;
using UnityEngine;

namespace Utils.DebugOverlay
{
    public struct CharBufView
    {
        public char[] buf;
        public int start;
        public int length;

        public CharBufView(char[] buf, int start, int length)
        {
            this.buf = buf;
            this.start = start;
            this.length = length;
        }

        public CharBufView(char[] buf, int length) : this()
        {
            this.buf = buf;
            start = 0;
            this.length = length;
        }
    }

    public static class StringFormatter
    {
        public static int Write(ref char[] destBuf, int destIndex, string format)
        {
            return Write<NoArg, NoArg, NoArg, NoArg, NoArg, NoArg>(
                ref destBuf, destIndex, format,
                null, null, null, null, null, null);
        }

        public static int Write<T0>(ref char[] destBuf, int destIndex, string format, T0 arg0)
        {
            return Write<T0, NoArg, NoArg, NoArg, NoArg, NoArg>(
                ref destBuf, destIndex, format,
                arg0, null, null, null, null, null);
        }

        private static int Write<T0, T1, T2, T3, T4, T5>(
            ref char[] destBuf, int destIndex, string format,
            T0 arg0, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            int written;
            unsafe
            {
                fixed (char* p = format, d = &destBuf[0])
                {
                    char* dest = d + destIndex;
                    char* end = d + destBuf.Length;
                    char* src = p;
                    while (*src > 0 && dest < end)
                    {
                        // Simplified parsing of {<argnum>[,<width>][:<format>]} where <format> is one of either 0000.00 or ####.## type formatters.
                        switch (*src)
                        {
                            case '{' when *(src + 1) == '}':
                                *dest++ = *src++;
                                ++src;
                                break;
                            case '}' when *(src + 1) == '}':
                                *dest++ = *src++;
                                ++src;
                                break;
                            case '}':
                                throw new FormatException("You must escape curly braces");
                            case '{':
                            {
                                ++src;

                                // Default values of FormatSpec in case none are given in format string
                                FormatSpec s;
                                s.argWidth = 0;
                                s.leadingZero = false;
                                s.numberWidth = 0;
                                s.fractWidth = 0;

                                // Parse argument number
                                int argNum = ReadNum(ref src);
                                if (*src == ',')
                                {
                                    ++src;
                                    s.argWidth = ReadNum(ref src);
                                }

                                if (*src == ':')
                                {
                                    ++src;
                                    char ch = *src;
                                    if (ch == '0')
                                    {
                                        s.leadingZero = true;
                                        s.numberWidth = CountChar(ref src, ch);
                                        if (*src == '.')
                                        {
                                            ++src;
                                            s.fractWidth = CountChar(ref src, ch);
                                        }
                                    }
                                }

                                // Skip to '}'
                                while (*src != '\0' && *src != '}')
                                {
                                    ++src;
                                }

                                if (*src == '\0')
                                {
                                    throw new FormatException("Invalid format. Missing '}'?");
                                }

                                ++src;

                                switch (argNum)
                                {
                                    case 0:
                                        ((IConverter<T0>) Converter.Instance).Convert(ref dest, end, arg0, s);
                                        break;
                                    case 1:
                                        ((IConverter<T1>) Converter.Instance).Convert(ref dest, end, arg1, s);
                                        break;
                                    case 2:
                                        ((IConverter<T2>) Converter.Instance).Convert(ref dest, end, arg2, s);
                                        break;
                                    case 3:
                                        ((IConverter<T3>) Converter.Instance).Convert(ref dest, end, arg3, s);
                                        break;
                                    case 4:
                                        ((IConverter<T4>) Converter.Instance).Convert(ref dest, end, arg4, s);
                                        break;
                                    case 5:
                                        ((IConverter<T5>) Converter.Instance).Convert(ref dest, end, arg5, s);
                                        break;
                                }

                                break;
                            }
                            default:
                                *dest++ = *src++;
                                break;
                        }
                    }

                    written = (int) (dest - d + destIndex);
                }
            }

            return written;
        }

        private static unsafe int CountChar(ref char* p, char ch)
        {
            var res = 0;
            while (*p == ch)
            {
                ++res;
                ++p;
            }

            return res;
        }

        private static unsafe int ReadNum(ref char* p)
        {
            var res = 0;
            var neg = false;
            if (*p == '-')
            {
                neg = true;
                ++p;
            }

            while (*p >= '0' && *p <= '9')
            {
                res *= 10;
                res += *p - '0';
                ++p;
            }

            return neg ? -res : res;
        }

        private class NoArg
        {
        }

        private struct FormatSpec
        {
            public int argWidth;
            public bool leadingZero;
            public int numberWidth;
            public int fractWidth;
        }

        private interface IConverter<in T>
        {
            unsafe void Convert(ref char* destBuf, char* end, T value, FormatSpec formatSpec);
        }

        private class Converter
            : IConverter<int>, IConverter<float>, IConverter<string>, IConverter<byte>, IConverter<CharBufView>
        {
            public static readonly Converter Instance = new Converter();

            unsafe void IConverter<int>.Convert(ref char* destBuf, char* end, int value, FormatSpec formatSpec)
            {
                ConvertInt(ref destBuf, end, value, formatSpec.argWidth, formatSpec.numberWidth,
                    formatSpec.leadingZero);
            }

            unsafe void IConverter<float>.Convert(ref char* destBuf, char* end, float value, FormatSpec formatSpec)
            {
                if (formatSpec.fractWidth == 0)
                    formatSpec.fractWidth = 2;

                int intWidth = formatSpec.argWidth - formatSpec.fractWidth - 1;

                var neg = false;
                if (value < 0)
                {
                    neg = true;
                    value = -value;
                }

                int intPart = Mathf.FloorToInt(value);
                var fractMulti = (int) Mathf.Pow(10, formatSpec.fractWidth);
                int fractPart = Mathf.FloorToInt(value * fractMulti) % fractMulti;
                ConvertInt(ref destBuf, end, neg ? -intPart : intPart, intWidth, formatSpec.numberWidth,
                    formatSpec.leadingZero);
                if (destBuf < end)
                    *destBuf++ = '.';
                ConvertInt(ref destBuf, end, fractPart, formatSpec.fractWidth, formatSpec.fractWidth, true);
            }

            unsafe void IConverter<string>.Convert(ref char* destBuf, char* end, string value, FormatSpec formatSpec)
            {
                var lPadding = 0;
                var rPadding = 0;
                if (formatSpec.argWidth < 0)
                    rPadding = -formatSpec.argWidth - value.Length;
                else
                    lPadding = formatSpec.argWidth - value.Length;

                while (lPadding-- > 0 && destBuf < end)
                    *destBuf++ = ' ';
                for (int i = 0, l = value.Length; i < l && destBuf < end; i++)
                    *destBuf++ = value[i];
                while (rPadding-- > 0 && destBuf < end)
                    *destBuf++ = ' ';
            }

            unsafe void IConverter<byte>.Convert(ref char* destBuf, char* end, byte value, FormatSpec formatSpec)
            {
                ConvertInt(ref destBuf, end, value, formatSpec.argWidth, formatSpec.numberWidth,
                    formatSpec.leadingZero);
            }

            unsafe void IConverter<CharBufView>.Convert(ref char* destBuf, char* end, CharBufView value,
                FormatSpec formatSpec)
            {
                var lPadding = 0;
                var rPadding = 0;
                if (formatSpec.argWidth < 0)
                    rPadding = -formatSpec.argWidth - value.length;
                else
                    lPadding = formatSpec.argWidth - value.length;

                while (lPadding-- > 0 && destBuf < end)
                    *destBuf++ = ' ';
                for (int i = 0, l = value.length; i < l && destBuf < end; ++i)
                    *destBuf++ = value.buf[i + value.start];
                while (rPadding-- > 0 && destBuf < end)
                    *destBuf++ = ' ';
            }

            private unsafe void ConvertInt(ref char* destBuf, char* end, int value, int argWidth, int integerWidth,
                bool leadingZero)
            {
                // Dry run to calculate size
                var numberWidth = 0;
                var signWidth = 0;
                var intPaddingWidth = 0;
                var argPaddingWidth = 0;

                bool neg = value < 0;
                if (neg)
                {
                    value = -value;
                    signWidth = 1;
                }

                int v = value;
                do
                {
                    ++numberWidth;
                    v /= 10;
                } while (v != 0);

                if (numberWidth < integerWidth)
                {
                    intPaddingWidth = integerWidth - numberWidth;
                }

                if (numberWidth + intPaddingWidth + signWidth < argWidth)
                {
                    argPaddingWidth = argWidth - numberWidth - intPaddingWidth - signWidth;
                }

                destBuf += argPaddingWidth + signWidth + intPaddingWidth + numberWidth;

                if (destBuf > end)
                {
                    return;
                }

                do
                {
                    *--destBuf = (char) ('0' + (value % 10));
                    value /= 10;
                } while (value != 0);

                if (leadingZero)
                {
                    while (intPaddingWidth-- > 0)
                    {
                        *--destBuf = '0';
                    }
                }
                else
                {
                    while (intPaddingWidth-- > 0)
                    {
                        *--destBuf = ' ';
                    }
                }

                if (neg)
                {
                    *--destBuf = '-';
                }

                while (argPaddingWidth-- > 0)
                {
                    *--destBuf = ' ';
                }
            }
        }
    }
}