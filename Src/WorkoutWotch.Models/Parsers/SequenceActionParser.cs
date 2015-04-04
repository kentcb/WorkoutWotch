namespace WorkoutWotch.Models.Parsers
{
    using System;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Container;

    internal static class SequenceActionParser
    {
        public static Parser<SequenceAction> GetParser(int indentLevel, IContainerService containerService)
        {
            if (indentLevel < 0)
            {
                throw new ArgumentException("indentLevel must be greater than or equal to 0.", "indentLevel");
            }

            containerService.AssertNotNull(nameof(containerService));

            return
                from _ in Parse.IgnoreCase("sequence:")
                from __ in VerticalSeparationParser.Parser.AtLeastOnce()
                from actions in ActionListParser.GetParser(indentLevel + 1, containerService)
                select new SequenceAction(actions);
        }
    }
}