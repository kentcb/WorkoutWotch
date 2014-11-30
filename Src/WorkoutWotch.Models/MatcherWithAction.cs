namespace WorkoutWotch.Models
{
    using Kent.Boogaart.HelperTrinity.Extensions;

	public sealed class MatcherWithAction 
	{
        private readonly IEventMatcher matcher;
        private readonly IAction action;

        public MatcherWithAction(IEventMatcher matcher, IAction action)
        {
            matcher.AssertNotNull("matcher");
            action.AssertNotNull("action");

            this.matcher = matcher;
            this.action = action;
        }

        public IEventMatcher Matcher
        {
            get { return this.matcher; }
        }

        public IAction Action
        {
            get { return this.action; }
        }
	}
}