namespace WorkoutWotch.Models.Parsers
{
    using Kent.Boogaart.HelperTrinity.Extensions;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Container;
    using WorkoutWotch.Services.Contracts.Delay;
    using WorkoutWotch.Services.Contracts.Logger;

    internal static class MetronomeActionParser
    {
        private static Parser<MetronomeTick> metronomeTickParser =
            from periodBefore in TimeSpanParser.Parser
            from type in Parse.Chars("*-").Optional()
            select new MetronomeTick(periodBefore, GetTickType(type.GetOrDefault()));

        public static Parser<MetronomeAction> GetParser(IContainerService containerService)
        {
            containerService.AssertNotNull("containerService");

            return
                from _ in Parse.IgnoreCase("metronome")
                from __ in Parse.WhiteSpace.AtLeastOnce()
                from ___ in Parse.IgnoreCase("at")
                from ____ in Parse.WhiteSpace.AtLeastOnce()
                from ticks in metronomeTickParser.DelimitedBy(Parse.Char(',').Token())
                select new MetronomeAction(
                    containerService.Resolve<IAudioService>(),
                    containerService.Resolve<IDelayService>(),
                    containerService.Resolve<ILoggerService>(),
                    ticks);
        }

        private static MetronomeTickType GetTickType(char ch)
        {
            switch (ch)
            {
                case '*':
                    return MetronomeTickType.Bell;
                case '-':
                    return MetronomeTickType.None;
                default:
                    return MetronomeTickType.Click;
            }
        }
    }
}