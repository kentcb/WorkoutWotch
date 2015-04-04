namespace WorkoutWotch.Models.Parsers
{
    using System;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.Logger;

    internal static class DoNotAwaitActionParser
    {
        public static Parser<DoNotAwaitAction> GetParser(int indentLevel, IContainerService containerService)
        {
            if (indentLevel < 0)
            {
                throw new ArgumentException("indentLevel must be greater than or equal to 0.", "indentLevel");
            }

            containerService.AssertNotNull(nameof(containerService));

            return
                from _ in Parse.IgnoreCase("don't")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from ___ in Parse.IgnoreCase("wait:")
                from ____ in VerticalSeparationParser.Parser.AtLeastOnce()
                from actions in ActionListParser.GetParser(indentLevel + 1, containerService)
                let child = new SequenceAction(actions)
                select new DoNotAwaitAction(
                    containerService.Resolve<ILoggerService>(),
                    child);
        }
    }
}