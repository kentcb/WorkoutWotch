namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;

    [TestFixture]
    public class WaitActionParserFixture
    {
        private const int msInSecond = 1000;
        private const int msInMinute = 60 * msInSecond;
        private const int msInHour = 60 * msInMinute;

        [Test]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => WaitActionParser.GetParser(null));
        }

        [TestCase("Wait for 1m 30s", 1 * msInMinute + 30 * msInSecond)]
        [TestCase("Wait for 1h 2m 5s", 1 * msInHour + 2 * msInMinute + 5 * msInSecond)]
        [TestCase("WAIT FOR 3s", 3 * msInSecond)]
        [TestCase("WaIt FoR 3s", 3 * msInSecond)]
        [TestCase("Wait    \t for \t   3s", 3 * msInSecond)]
        public void can_parse_correctly_formatted_input(string input, int expectedMilliseconds)
        {
            var result = WaitActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose)).Parse(input);
            Assert.NotNull(result);
            Assert.AreEqual(TimeSpan.FromMilliseconds(expectedMilliseconds), result.Duration);
        }

        [TestCase("  Wait for 1m")]
        [TestCase("Wait\n for 1m")]
        [TestCase("Wait for\n 1m")]
        [TestCase("Wayte for 1m")]
        [TestCase("Wait for abc")]
        [TestCase("WaitFor 1m")]
        [TestCase("Wait For1m")]
        [TestCase("WaitFor1m")]
        [TestCase("Wait 1m")]
        [TestCase("for 1m")]
        [TestCase("")]
        [TestCase("whatever")]
        public void cannot_parse_incorrectly_formatted_input(string input)
        {
            var result = WaitActionParser.GetParser(new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}