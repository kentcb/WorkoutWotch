namespace WorkoutWotch.Models.Parsers
{
    using System;
    using System.Collections.Generic;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Container;

    internal static class ActionListParser
    {
        public static Parser<IEnumerable<IAction>> GetParser(int indentLevel, IContainerService containerService)
        {
            if (indentLevel < 0)
            {
                throw new ArgumentException("indentLevel must be greater than or equal to 0.", "indentLevel");
            }

            containerService.AssertNotNull("containerService");

            return
                (from _ in Parse.String("  ").Repeat(indentLevel)
                 from __ in Parse.String("* ")
                 from action in ActionParser.GetParser(indentLevel, containerService)
                 from ___ in Parse.WhiteSpace.Except(NewLineParser.Parser).Many()
                 select action).DelimitedBy(NewLineParser.Parser);
        }
    }
}