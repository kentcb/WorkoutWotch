namespace WorkoutWotch.Models
{
    using Kent.Boogaart.HelperTrinity.Extensions;

    public sealed class MatcherWithAction 
    {
        private readonly IEventMatcher matcher;
        private readonly IAction action;

        public MatcherWithAction(IEventMatcher matcher, IAction action)
        {
            matcher.AssertNotNull(nameof(matcher));
            action.AssertNotNull(nameof(action));

            this.matcher = matcher;
            this.action = action;
        }

        public IEventMatcher Matcher => this.matcher;

        public IAction Action => this.action;
    }
}