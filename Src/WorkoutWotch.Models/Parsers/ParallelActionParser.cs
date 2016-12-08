namespace WorkoutWotch.Models.Parsers
{
    using Genesis.Ensure;
    using Services.Contracts.Audio;
    using Services.Contracts.Delay;
    using Services.Contracts.Speech;
    using Sprache;
    using WorkoutWotch.Models.Actions;

    internal static class ParallelActionParser
    {
        public static Parser<ParallelAction> GetParser(
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
                from _ in Parse.IgnoreCase("parallel:")
                from __ in VerticalSeparationParser.Parser.AtLeastOnce()
                from actions in ActionListParser.GetParser(indentLevel + 1, audioService, delayService, speechService)
                select new ParallelAction(actions);
        }
    }
}