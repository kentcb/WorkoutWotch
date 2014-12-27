namespace Sprache
{
    using System;
    using System.Reactive;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public static class ParserExtensions
    {
        public static Parser<T> Token<T, U>(this Parser<T> @this, Parser<U> whitespace)
        {
            @this.AssertNotNull("@this");
            whitespace.AssertNotNull("whitespace");

            return
                from _ in whitespace.Many()
                from item in @this
                from __ in whitespace.Many()
                select item;
        }

        public static Parser<Unit> ToUnit<T>(this Parser<T> @this)
        {
            @this.AssertNotNull("@this");
            return @this.Select(_ => Unit.Default);
        }

        public static Parser<T> Do<T>(this Parser<T> @this, Action<T> action)
        {
            @this.AssertNotNull("@this");
            action.AssertNotNull("action");

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
        public static Parser<T> Default<T>()
        {
            return i => Result.Success(default(T), i);
        }
    }
}