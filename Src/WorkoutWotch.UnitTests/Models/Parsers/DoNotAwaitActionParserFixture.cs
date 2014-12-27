namespace WorkoutWotch.UnitTests.Models.Parsers
{
    using System;
    using System.Linq;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using Sprache;
    using WorkoutWotch.Models.Actions;
    using WorkoutWotch.Models.Parsers;
    using WorkoutWotch.UnitTests.Services.Container.Mocks;

    [TestFixture]
    public class DoNotAwaitActionParserFixture
    {
        [Test]
        public void get_parser_throws_if_indent_level_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => DoNotAwaitActionParser.GetParser(-1, new ContainerServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => DoNotAwaitActionParser.GetParser(0, null));
        }

        [TestCase(
            "Don't wait:\n  * Say 'foo'\n  * Wait for 2s",
            0,
            new [] { typeof(SayAction), typeof(WaitAction) })]
        [TestCase(
            "Don't wait:\n    * Say 'foo'\n    * Wait for 2s",
            1,
            new [] { typeof(SayAction), typeof(WaitAction) })]
        [TestCase(
            "DON'T WAIT:\n  * Say 'foo'\n  * Wait for 2s",
            0,
            new [] { typeof(SayAction), typeof(WaitAction) })]
        [TestCase(
            "Don't wait:  \t  \n  * Say 'foo'\n  * Say 'bar'\n  * Say 'biz'",
            0,
            new [] { typeof(SayAction), typeof(SayAction), typeof(SayAction) })]
        [TestCase(
            "Don't  \t\t   \t wait:\n\n\t\t  \n    \n\n  * Say 'foo'\n  * Say 'bar'\n  * Say 'biz'",
            0,
            new [] { typeof(SayAction), typeof(SayAction), typeof(SayAction) })]
        public void can_parse_valid_input(string input, int indentLevel, Type[] expectedActionTypes)
        {
            var result = DoNotAwaitActionParser
                .GetParser(indentLevel, new ContainerServiceMock(MockBehavior.Loose))
                .Parse(input);

            Assert.NotNull(result);
            var sequence = result.InnerAction as SequenceAction;
            Assert.NotNull(sequence);
            Assert.True(sequence.Children.Select(x => x.GetType()).SequenceEqual(expectedActionTypes));
        }

        [TestCase("Dont wait:\n  * Say 'foo'", 0)]
        [TestCase("Don'twait:\n  * Say 'foo'", 0)]
        [TestCase("  Don't wait:\n  * Say 'foo'", 0)]
        [TestCase("Don't wait:\n", 0)]
        [TestCase("Don't wait:\n  * Say 'foo'", 1)]
        public void cannot_parse_invalid_input(string input, int indentLevel)
        {
            var result = DoNotAwaitActionParser
                .GetParser(indentLevel, new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}

