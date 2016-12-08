namespace WorkoutWotch.Models.Parsers
{
    using Genesis.Ensure;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Services.Contracts.Audio;
    using WorkoutWotch.Services.Contracts.Delay;

    internal static class MetronomeActionParser
    {
        private static Parser<MetronomeTick> metronomeTickParser =
            from periodBefore in TimeSpanParser.Parser
            from type in Parse.Chars("*-").Optional()
            select new MetronomeTick(periodBefore, GetTickType(type.GetOrDefault()));

        public static Parser<MetronomeAction> GetParser(
            IAudioService audioService,
            IDelayService delayService)
        {
            Ensure.ArgumentNotNull(audioService, nameof(audioService));
            Ensure.ArgumentNotNull(delayService, nameof(delayService));

            return
                from _ in Parse.IgnoreCase("metronome")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from ___ in Parse.IgnoreCase("at")
                from ____ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from ticks in metronomeTickParser.DelimitedBy(Parse.Char(',').Token(HorizontalWhitespaceParser.Parser))
                select new MetronomeAction(
                    audioService,
                    delayService,
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