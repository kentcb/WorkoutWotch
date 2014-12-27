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
    public class ActionListParserFixture
    {
        [Test]
        public void get_parser_throws_if_indent_level_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() => ActionListParser.GetParser(-1, new ContainerServiceMock()));
        }

        [Test]
        public void get_parser_throws_if_container_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => ActionListParser.GetParser(0, null));
        }

        [TestCase(
            "* Wait for 2s\n* Break for 1m",
            0,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "* Wait for 2s  \n* Break for 1m   \t  ",
            0,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "* Say 'foo'\n* Wait for 1s\n* Wait for 2s\n* Break for 1m",
            0,
            new [] { typeof(SayAction), typeof(WaitAction), typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "  * Wait for 2s\n  * Break for 1m",
            1,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "\t* Wait for 2s\n\t* Break for 1m",
            1,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "\t* Wait for 2s\n  * Break for 1m",
            1,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "      * Wait for 2s\n      * Break for 1m",
            3,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "    *       Wait for 2s  \t \n    *  \t Break for 1m  \t\t  ",
            2,
            new [] { typeof(WaitAction), typeof(BreakAction) })]
        [TestCase(
            "* Sequence:\n  * Say 'foo'\n  * Sequence:\n    * Say 'bar'",
            0,
            new [] { typeof(SequenceAction) })]
        public void can_parse_valid_input(string input, int indentLevel, Type[] expectedActionTypes)
        {
            var result = ActionListParser
                .GetParser(indentLevel, new ContainerServiceMock(MockBehavior.Loose))(new Input(input));

            Assert.True(result.WasSuccessful);
            Assert.True(result.Remainder.AtEnd);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.Select(x => x.GetType()).SequenceEqual(expectedActionTypes));
        }

        [TestCase("", 0)]
        [TestCase("* ", 0)]
        [TestCase("Wait for 2s", 0)]
        [TestCase("  * Wait for 2s", 0)]
        [TestCase("\t* Wait for 2s", 0)]
        [TestCase("* Wait for 2s", 1)]
        [TestCase("* Wait for 2s* Wait for 3s", 0)]
        public void cannot_parse_invalid_input(string input, int indentLevel)
        {
            var result = ActionListParser
                .GetParser(indentLevel, new ContainerServiceMock(MockBehavior.Loose))(new Input(input));
            Assert.False(result.WasSuccessful && result.Remainder.AtEnd);
        }
    }
}