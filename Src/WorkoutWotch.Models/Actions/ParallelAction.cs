namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
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

        public async Task ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull("context");

            var childrenGroupedBySkip = this
                .children
                .OrderByDescending(x => x.Duration)
                .GroupBy(x => context.SkipAhead > TimeSpan.Zero && context.SkipAhead >= x.Duration)
                .ToList();
            var childrenToExecute = childrenGroupedBySkip
                .SingleOrDefault(x => !x.Key)
                .ToList();

            Debug.Assert(childrenToExecute != null, "Always expecting at least one child to execute.");

            var shadowedContext = CreateShadowExecutionContext(context);
            var firstChildExecution = childrenToExecute
                .Take(1)
                .Select(x => x.ExecuteAsync(context))
                .ToList();
            var subsequentChildExecutions = childrenToExecute
                .Skip(1)
                .Select(x => x.ExecuteAsync(shadowedContext))
                .ToList();

            await Task
                .WhenAll(firstChildExecution.Concat(subsequentChildExecutions))
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