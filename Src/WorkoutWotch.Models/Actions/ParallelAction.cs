namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ReactiveUI;

    public sealed class ParallelAction : IAction
    {
        private readonly IImmutableList<IAction> children;
        private readonly TimeSpan duration;

        public ParallelAction(IEnumerable<IAction> children)
        {
            children.AssertNotNull("children");

            this.children = children.ToImmutableList();
            this.duration = this
                .children
                .Select(x => x.Duration)
                .DefaultIfEmpty()
                .Max();
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
                
            var childrenToExecute = this
                .children
                .Where(x => context.SkipAhead == TimeSpan.Zero || context.SkipAhead < x.Duration)
                .OrderByDescending(x => x.Duration)
                .ToList();

            if (childrenToExecute.Count == 0)
            {
                // although this shouldn't really happen, we've been asked to execute even though the skip ahead exceeds even our longest-running child
                context.AddProgress(this.Duration);
                return;
            }

            var shadowedContext = CreateShadowExecutionContext(context);

            // only the longest-running child gets the real execution context. The other actions get a shadowed context so that progress does not compound incorrectly
            var executionTasks = childrenToExecute
                .Select((action, index) => action.ExecuteAsync(index == 0 ? context : shadowedContext))
                .ToList();

            await Task
                .WhenAll(executionTasks)
                .ContinueOnAnyContext();
        }

        private static ExecutionContext CreateShadowExecutionContext(ExecutionContext context)
        {
            var shadowExecutionContext = new ExecutionContext(context.SkipAhead);
            shadowExecutionContext.SetCurrentExercise(context.CurrentExercise);
            shadowExecutionContext.SetCurrentSet(context.CurrentSet);
            shadowExecutionContext.SetCurrentRepetition(context.CurrentRepetition);

            // if either context is cancelled, cancel the other
            context
                .WhenAnyValue(x => x.IsCancelled)
                .Where(x => x)
                .Subscribe(_ => shadowExecutionContext.Cancel());
            shadowExecutionContext
                .WhenAnyValue(x => x.IsCancelled)
                .Where(x => x)
                .Subscribe(_ => context.Cancel());

            // if either context is paused, pause the other
            context
                .WhenAnyValue(x => x.IsPaused)
                .Subscribe(x => shadowExecutionContext.IsPaused = x);
            shadowExecutionContext
                .WhenAnyValue(x => x.IsPaused)
                .Subscribe(x => context.IsPaused = x);

            return shadowExecutionContext;
        }
    }
}