using System.Collections.Generic;

namespace Utils.Pool
{
    public static partial class Pools
    {
        public static class SimpleObject
        {
            public static T Pop<T>() where T : new()
            {
                return Inner<T>.Pop();
            }

            public static void Push<T>(T obj) where T : new()
            {
                Inner<T>.Push(obj);
            }

            private static class Inner<T> where T : new()
            {
                private static readonly Stack<T> Pool = new Stack<T>(1);

                public static T Pop()
                {
                    return Pool.Count > 0 ? Pool.Pop() : new T();
                }

                public static void Push(T obj)
                {
                    if (obj == null || Pool.Contains(obj))
                        return;
                    Pool.Push(obj);
                }
            }
        }
    }
}