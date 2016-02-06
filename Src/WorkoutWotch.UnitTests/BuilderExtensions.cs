namespace WorkoutWotch.UnitTests
{
    using System.Collections.Generic;

    public static class BuilderExtensions
    {
        public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref TField field, TField value)
            where TBuilder : IBuilder
        {
            field = value;
            return @this;
        }

        public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField> field, IEnumerable<TField> values)
            where TBuilder : IBuilder
        {
            if (values == null)
            {
                field = null;
            }
            else
            {
                field.AddRange(values);
            }

            return @this;
        }

        public static TBuilder With<TBuilder, TField>(this TBuilder @this, ref List<TField> field, TField value)
            where TBuilder : IBuilder
        {
            field.Add(value);
            return @this;
        }
    }
}