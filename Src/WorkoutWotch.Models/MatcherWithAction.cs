namespace WorkoutWotch.Models
{
    using Utility;

    public sealed class MatcherWithAction 
    {
        private readonly IEventMatcher matcher;
        private readonly IAction action;

        public MatcherWithAction(IEventMatcher matcher, IAction action)
        {
            Ensure.ArgumentNotNull(matcher, nameof(matcher));
            Ensure.ArgumentNotNull(action, nameof(action));

            this.matcher = matcher;
            this.action = action;
        }

        public IEventMatcher Matcher => this.matcher;

        public IAction Action => this.action;
    }
}