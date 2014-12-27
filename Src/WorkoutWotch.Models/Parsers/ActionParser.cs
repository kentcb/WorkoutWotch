namespace WorkoutWotch.Models.Parsers
{
    using System;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Services.Contracts.Container;

    internal static class ActionParser
    {
        public static Parser<IAction> GetParser(int indentLevel, IContainerService containerService)
        {
            if (indentLevel < 0)
            {
                throw new ArgumentException("indentLevel must be greater than or equal to 0.", "indentLevel");
            }

            containerService.AssertNotNull("containerService");

            return BreakActionParser.GetParser(containerService)
                .Or<IAction>(MetronomeActionParser.GetParser(containerService))
                .Or<IAction>(PrepareActionParser.GetParser(containerService))
                .Or<IAction>(SayActionParser.GetParser(containerService))
                .Or<IAction>(WaitActionParser.GetParser(containerService))
                .Or<IAction>(DoNotAwaitActionParser.GetParser(indentLevel, containerService))
                .Or<IAction>(ParallelActionParser.GetParser(indentLevel, containerService))
                .Or<IAction>(SequenceActionParser.GetParser(indentLevel, containerService));
        }
    }
}