namespace Sprache
{
    using System;
    using System.Reactive;
    using Genesis.Ensure;

    public static class ParserExtensions
    {
        public static Parser<T> Token<T, U>(this Parser<T> @this, Parser<U> whitespace)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(whitespace, nameof(whitespace));

            return
                from _ in whitespace.Many()
                from item in @this
                from __ in whitespace.Many()
                select item;
        }

        public static Parser<Unit> ToUnit<T>(this Parser<T> @this)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            return @this.Select(_ => Unit.Default);
        }

        public static Parser<T> Do<T>(this Parser<T> @this, Action<T> action)
        {
            Ensure.ArgumentNotNull(@this, nameof(@this));
            Ensure.ArgumentNotNull(action, nameof(action));

            return @this.Then(
                x =>
                {
                    action(x);
                    return Parse.Return(x);
                });
        }
    }

    public static class ParseExt
    {
        public static Parser<T> Default<T>() =>
            i => Result.Success(default(T), i);
    }
}