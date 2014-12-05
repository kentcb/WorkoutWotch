namespace System
{
    // some stuff that should be in System.Math but isn't
    public static class MathExt
    {
        public static T Max<T>(T first, T second)
            where T : IComparable<T>
        {
            if (first == null)
            {
                return second;
            }
            else if (second == null)
            {
                return first;
            }

            return first.CompareTo(second) < 0 ? second : first;
        }

        public static T Min<T>(T first, T second)
            where T : IComparable<T>
        {
            if (first == null)
            {
                return first;
            }
            else if (second == null)
            {
                return second;
            }

            return first.CompareTo(second) > 0 ? second : first;
        }
    }
}