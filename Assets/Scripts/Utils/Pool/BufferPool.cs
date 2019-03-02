using System.Collections.Generic;
using UnityEngine;

namespace Utils.Pool
{
    public static class BufferPool
    {
        public static T[] Pop<T>(int minLength = 16)
        {
            return Inner<T>.Pop(minLength);
        }

        public static void Push<T>(T[] buffer)
        {
            Inner<T>.Push(buffer);
        }

        private static class Inner<T>
        {
            private static readonly Stack<T[]> Pool = new Stack<T[]>(1);

            public static T[] Pop(int minLength)
            {
                while (Pool.Count > 0)
                {
                    T[] top = Pool.Pop();
                    if (top.Length >= minLength)
                        return top;
                }

                return new T[minLength];
            }

            public static void Push(T[] buffer)
            {
                Debug.Assert(buffer != null);
                Pool.Push(buffer);
            }
        }
    }
}