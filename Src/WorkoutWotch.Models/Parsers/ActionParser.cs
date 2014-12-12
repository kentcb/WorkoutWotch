using System;
using Sprache;
using WorkoutWotch.Services.Contracts.Speech;
using WorkoutWotch.Services.Contracts.Logger;
using WorkoutWotch.Services.Contracts.Delay;
using WorkoutWotch.Services.Contracts.Audio;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace WorkoutWotch.Models.Parsers
{
    internal static class ActionParser
    {
        public static Parser<IAction> GetParser(int indentLevel, IAudioService audioService, IDelayService delayService, ILoggerService loggerService, ISpeechService speechService)
        {
            if (indentLevel < 0)
            {
                throw new ArgumentException("indentLevel must be greater than or equal to 0.", "indentLevel");
            }

            audioService.AssertNotNull("audioService");
            delayService.AssertNotNull("delayService");
            loggerService.AssertNotNull("loggerService");
            speechService.AssertNotNull("speechService");

            return BreakActionParser.GetParser(delayService, speechService)
                .Or<IAction>(MetronomeActionParser.GetParser(audioService, delayService, loggerService))
                .Or<IAction>(PrepareActionParser.GetParser(delayService, speechService))
                .Or<IAction>(SayActionParser.GetParser(speechService))
                .Or<IAction>(WaitActionParser.GetParser(delayService));
                //.Or<IAction>(ParallelActionParser.GetParser(indentLevel + 1, audioService, delayService, loggerService, speechService))
                //.Or<IAction>(SequenceActionParser.GetParser(indentLevel + 1, audioService, delayService, loggerService, speechService))
        }
    }
}

