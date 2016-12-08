namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using Genesis.Ensure;
    using ReactiveUI;

    public sealed class ParallelAction : IAction
    {
        private readonly IImmutableList<IAction> children;
        private readonly TimeSpan duration;

        public ParallelAction(IEnumerable<IAction> children)
        {
            Ensure.ArgumentNotNull(children, nameof(children), assertContentsNotNull: true);

            this.children = children.ToImmutableList();
            this.duration = this
                .children
                .Select(x => x.Duration)
                .DefaultIfEmpty()
                .Max();
        }

        public TimeSpan Duration => this.duration;

        public IImmutableList<IAction> Children =>  this.children;

        public IObservable<Unit> Execute(ExecutionContext context)
        {
            Ensure.ArgumentNotNull(context, nameof(context));

            var childrenToExecute = this
                .children
                .Where(x => context.SkipAhead == TimeSpan.Zero || context.SkipAhead < x.Duration)
                .OrderByDescending(x => x.Duration)
                .ToList();

            if (childrenToExecute.Count == 0)
            {
                // although this shouldn't really happen, we've been asked to execute even though the skip ahead exceeds even our longest-running child
                context.AddProgress(this.Duration);
                return Observables.Unit;
            }

            var shadowedContext = CreateShadowExecutionContext(context);

            // only the longest-running child gets the real execution context. The other actions get a shadowed context so that progress does not compound incorrectly
            var childExecutions = childrenToExecute
                .Select((action, index) => action.Execute(index == 0 ? context : shadowedContext))
                .ToList();

            return Observable
                .CombineLatest(childExecutions)
                .Select(_ => Unit.Default);
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