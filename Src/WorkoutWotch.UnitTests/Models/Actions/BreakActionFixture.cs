using System;
using NUnit.Framework;
using WorkoutWotch.UnitTests.Services.Speech.Mocks;
using WorkoutWotch.Models.Actions;
using WorkoutWotch.UnitTests.Services.Delay.Mocks;
using System.Collections.Generic;
using Kent.Boogaart.PCLMock;
using System.Threading;
using System.Threading.Tasks;
using WorkoutWotch.Models;
using System.Linq;

namespace WorkoutWotch.UnitTests.Models.Actions
{
    [TestFixture]
    public class BreakActionFixture
    {
        [Test]
        public void ctor_throws_if_delay_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new BreakAction(null, new SpeechServiceMock(), TimeSpan.FromSeconds(30)));
        }

        [Test]
        public void ctor_throws_if_speech_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new BreakAction(new DelayServiceMock(), null, TimeSpan.FromSeconds(30)));
        }

        [Test]
        public void ctor_throws_if_duration_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => new BreakAction(new DelayServiceMock(), new SpeechServiceMock(), TimeSpan.FromSeconds(-2)));
        }

        [Test]
        public void duration_returns_value_provided_in_ctor()
        {
            Assert.AreEqual(TimeSpan.FromSeconds(9), new BreakAction(new DelayServiceMock(), new SpeechServiceMock(), TimeSpan.FromSeconds(9)).Duration);
        }

        [Test]
        public void execute_async_throws_if_the_context_is_null()
        {
            Assert.Throws<ArgumentNullException>(async () => await new BreakAction(new DelayServiceMock(), new SpeechServiceMock(), TimeSpan.FromSeconds(1)).ExecuteAsync(null));
        }

        [Test]
        public async Task execute_async_composes_actions_appropriately_for_long_breaks()
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

            var sut = new BreakAction(delayService, speechService, TimeSpan.FromSeconds(5));

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
        public async Task execute_async_composes_actions_appropriately_for_short_breaks()
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

            var sut = new BreakAction(delayService, speechService, TimeSpan.FromSeconds(1));

            await sut.ExecuteAsync(new ExecutionContext());

            Assert.AreEqual(2, performedActions.Count);
            Assert.AreEqual("Saying 'break'", performedActions[0]);
            Assert.AreEqual("Delayed for 00:00:01", performedActions[1]);
        }
    }
}

