namespace WorkoutWotch.Models.Parsers
{
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Speech;

    internal static class PrepareActionParser
    {
        public static Parser<PrepareAction> GetParser(IContainerService containerService)
        {
            containerService.AssertNotNull("containerService");

            return
                from _ in Parse.IgnoreCase("prepare")
                from __ in Parse.WhiteSpace.AtLeastOnce()
                from ___ in Parse.IgnoreCase("for")
                from ____ in Parse.WhiteSpace.AtLeastOnce()
                from duration in TimeSpanParser.Parser
                select new PrepareAction(
                    containerService.Resolve<IDelayService>(),
                    containerService.Resolve<ISpeechService>(),
                    duration);
        }
    }
}