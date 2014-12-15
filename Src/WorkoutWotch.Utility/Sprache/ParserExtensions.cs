using Kent.Boogaart.HelperTrinity.Extensions;

namespace Sprache
{
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
    }
}

