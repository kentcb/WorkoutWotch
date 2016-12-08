namespace WorkoutWotch.Models.Parsers
{
    using Genesis.Ensure;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Speech;
    using Sprache;
    using WorkoutWotch.Models.Actions;

    internal static class DoNotAwaitActionParser
    {
        public static Parser<DoNotAwaitAction> GetParser(
            int indentLevel,
            IAudioService audioService,
            IDelayService delayService,
            ISpeechService speechService)
        {
            Ensure.ArgumentCondition(indentLevel >= 0, "indentLevel must be greater than or equal to 0.", "indentLevel");
            Ensure.ArgumentNotNull(audioService, nameof(audioService));
            Ensure.ArgumentNotNull(delayService, nameof(delayService));
            Ensure.ArgumentNotNull(speechService, nameof(speechService));

            return
                from _ in Parse.IgnoreCase("don't")
                from __ in HorizontalWhitespaceParser.Parser.AtLeastOnce()
                from ___ in Parse.IgnoreCase("wait:")
                from ____ in VerticalSeparationParser.Parser.AtLeastOnce()
                from actions in ActionListParser.GetParser(indentLevel + 1, audioService, delayService, speechService)
                let child = new SequenceAction(actions)
                select new DoNotAwaitAction(
                    child);
        }
    }
}