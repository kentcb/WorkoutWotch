namespace System
{
    using System.Collections.Generic;

    // some stuff that should be in System.Math but isn't
    public static class MathExt
    {
        public static T Max<T>(T first, T second)
            where T : IComparable<T>
        {
            return Comparer<T>.Default.Compare(first, second) < 0 ? second : first;
        }

        public static T Min<T>(T first, T second)
            where T : IComparable<T>
        {
            return Comparer<T>.Default.Compare(first, second) < 0 ? first : second;
        }
    }
}