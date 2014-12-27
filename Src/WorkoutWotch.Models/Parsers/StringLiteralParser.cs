namespace WorkoutWotch.Models.Parsers
{
    using System;
    using System.Globalization;
    using System.Text;
    using Sprache;

    internal static class StringLiteralParser
    {
        private static Parser<string> GetStringLiteralParser(char delimiter)
        {
            return i =>
            {
                if (i.AtEnd)
                {
                    return Result.Failure<string>(
                        i,
                        "unexpected end of input",
                        new string[] { string.Format(CultureInfo.InvariantCulture, "string delimited by {0}", delimiter) });
                }

                if (i.Current != delimiter)
                {
                    return Result.Failure<string>(
                        i,
                        string.Format(CultureInfo.InvariantCulture, "unexpected '{0}'", i.Current),
                        new string[] { delimiter.ToString() });
                }

                var result = new StringBuilder();
                var escaped = false;

                while (true)
                {
                    i = i.Advance();

                    if (i.AtEnd)
                    {
                        return Result.Failure<string>(
                            i,
                            "unexpected end of input",
                            new string[] { string.Format(CultureInfo.InvariantCulture, "continued string contents or {0}", delimiter) });
                    }
                    else if (i.Current == '\n' || i.Current == '\r')
                    {
                        return Result.Failure<string>(
                            i,
                            "unexpected end of line",
                            new string[] { string.Format(CultureInfo.InvariantCulture, "continued string contents or {0}", delimiter) });
                    }

                    if (i.Current == '\\')
                    {
                        if (escaped)
                        {
                            result.Append(i.Current);
                            escaped = false;
                            continue;
                        }

                        escaped = true;
                    }
                    else if (i.Current == delimiter)
                    {
                        if (escaped)
                        {
                            result.Append(i.Current);
                            escaped = false;
                            continue;
                        }

                        i = i.Advance();
                        break;
                    }
                    else
                    {
                        escaped = false;
                        result.Append(i.Current);
                    }
                }

                return Result.Success(result.ToString(), i);
            };
        }

        public static readonly Parser<string> Parser = GetStringLiteralParser('"')
            .Or(GetStringLiteralParser('\''));
    }
}