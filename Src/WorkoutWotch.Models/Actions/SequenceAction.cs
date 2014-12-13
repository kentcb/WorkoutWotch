namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public sealed class SequenceAction : IAction
    {
        private readonly IImmutableList<IAction> children;
        private readonly TimeSpan duration;

        public SequenceAction(IEnumerable<IAction> children)
        {
            children.AssertNotNull("children");

            this.children = children.ToImmutableList();
            this.duration = this
                .children
                .Select(x => x.Duration)
                .DefaultIfEmpty()
                .Aggregate((running, next) => running + next);
        }

        public TimeSpan Duration
        {
            get { return this.duration; }
        }

        public IImmutableList<IAction> Children
        {
            get { return this.children; }
        }

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull("context");

            foreach (var child in this.children)
            {
                context.CancellationToken.ThrowIfCancellationRequested();

                if (context.SkipAhead > TimeSpan.Zero && context.SkipAhead >= child.Duration)
                {
                    context.AddProgress(child.Duration);
                    continue;
                }

                await child.ExecuteAsync(context).ContinueOnAnyContext();
            }
        }
    }
}