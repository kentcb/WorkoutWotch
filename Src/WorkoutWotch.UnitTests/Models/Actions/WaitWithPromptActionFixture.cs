namespace WorkoutWotch.UnitTests.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.UnitTests.Services.Delay.Mocks;
    using WorkoutWotch.UnitTests.Services.Speech.Mocks;

    [TestFixture]
    public class WaitWithPromptActionFixture
    {
        [Test]
        public void ctor_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitWithPromptAction(null, new SpeechServiceMock(), TimeSpan.FromSeconds(30), "whatever"));
        }

        [Test]
        public void ctor_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitWithPromptAction(new DelayServiceMock(), null, TimeSpan.FromSeconds(30), "whatever"));
        }

        [Test]
        public void ctor_throws_if_duration_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => new WaitWithPromptAction(new DelayServiceMock(), new SpeechServiceMock(), TimeSpan.FromSeconds(-2), "whatever"));
        }

        [Test]
        public void ctor_throws_if_start_speech_text_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new WaitWithPromptAction(new DelayServiceMock(), new SpeechServiceMock(), TimeSpan.Zero, null));
        }

        [Test]
        public void duration_returns_value_provided_in_ctor()
        {
            Assert.AreEqual(TimeSpan.FromSeconds(9), new WaitWithPromptAction(new DelayServiceMock(), new SpeechServiceMock(), TimeSpan.FromSeconds(9), "whatever").Duration);
        }

        [Test]
        public void execute_async_throws_if_the_context_is_null()
        {
            Assert.Throws<ArgumentNullException>(async () => await new WaitWithPromptAction(new DelayServiceMock(), new SpeechServiceMock(), TimeSpan.FromSeconds(1), "whatever").ExecuteAsync(null));
        }

        [Test]
        public async Task execute_async_composes_actions_appropriately_for_long_durations()
        {
            var delayService = new DelayServiceMock();
            var speechService = new SpeechServiceMock();
            var performedActions = new List<string>();

            delayService
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Do<TimeSpan, CancellationToken>((duration, ct) => performedActions.Add("Delayed for " + duration))
                .Return(Task.FromResult(true));
            speechService
                .When(x => x.SpeakAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Do<string, CancellationToken>((speechText, cty) => performedActions.Add("Saying '" + speechText + "'"))
                .Return(Task.FromResult(true));

            var sut = new WaitWithPromptAction(delayService, speechService, TimeSpan.FromSeconds(5), "break");

            await sut.ExecuteAsync(new ExecutionContext());

            Assert.AreEqual(7, performedActions.Count);
            Assert.AreEqual("Saying 'break'", performedActions[0]);
            Assert.AreEqual("Delayed for 00:00:01", performedActions[1]);
            Assert.AreEqual("Delayed for 00:00:01", performedActions[2]);
            Assert.AreEqual("Delayed for 00:00:01", performedActions[3]);
            Assert.AreEqual("Saying 'ready?'", performedActions[4]);
            Assert.AreEqual("Delayed for 00:00:01", performedActions[5]);
            Assert.AreEqual("Delayed for 00:00:01", performedActions[6]);
        }

        [Test]
        public async Task execute_async_composes_actions_appropriately_for_short_durations()
        {
            var delayService = new DelayServiceMock();
            var speechService = new SpeechServiceMock();
            var performedActions = new List<string>();

            delayService
                .When(x => x.DelayAsync(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Do<TimeSpan, CancellationToken>((duration, ct) => performedActions.Add("Delayed for " + duration))
                .Return(Task.FromResult(true));
            speechService
                .When(x => x.SpeakAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Do<string, CancellationToken>((speechText, cty) => performedActions.Add("Saying '" + speechText + "'"))
                .Return(Task.FromResult(true));

            var sut = new WaitWithPromptAction(delayService, speechService, TimeSpan.FromSeconds(1), "break");

            await sut.ExecuteAsync(new ExecutionContext());

            Assert.AreEqual(2, performedActions.Count);
            Assert.AreEqual("Saying 'break'", performedActions[0]);
            Assert.AreEqual("Delayed for 00:00:01", performedActions[1]);
        }

        [Test]
        public async Task execute_async_uses_the_specified_prompt_speech_text()
        {
            var delayService = new DelayServiceMock(MockBehavior.Loose);
            var speechService = new SpeechServiceMock(MockBehavior.Loose);

            var sut = new WaitWithPromptAction(delayService, speechService, TimeSpan.FromSeconds(1), "whatever");

            await sut.ExecuteAsync(new ExecutionContext());

            speechService.Verify(x => x.SpeakAsync(It.Is("whatever"), It.IsAny<CancellationToken>())).WasCalledExactlyOnce();
        }
    }
}