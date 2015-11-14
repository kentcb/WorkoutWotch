namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using Kent.Boogaart.HelperTrinity.Extensions;

    public sealed class SequenceAction : IAction
    {
        private readonly IImmutableList<IAction> children;
        private readonly TimeSpan duration;

        public SequenceAction(IEnumerable<IAction> children)
        {
            children.AssertNotNull(nameof(children), assertContentsNotNull: true);

            this.children = children.ToImmutableList();
            this.duration = this
                .children
                .Select(x => x.Duration)
                .DefaultIfEmpty()
                .Aggregate((running, next) => running + next);
        }

        public TimeSpan Duration => this.duration;

        public IImmutableList<IAction> Children =>  this.children;

        public IObservable<Unit> ExecuteAsync(ExecutionContext context)
        {
            context.AssertNotNull(nameof(context));

            var childExecutions = this
                .children
                .Select(
                    child => Observable
                        .Return(Unit.Default)
                        .Select(
                            _ =>
                            {
                                context.CancellationToken.ThrowIfCancellationRequested();
                                var execute = true;

                                if (context.SkipAhead > TimeSpan.Zero && context.SkipAhead >= child.Duration)
                                {
                                    context.AddProgress(child.Duration);
                                    execute = false;
                                }

                                return new
                                {
                                    Child = child,
                                    Execute = execute
                                };
                            })
                        .Where(x => x.Execute)
                        .SelectMany(x => x.Child.ExecuteAsync(context)))
                .DefaultIfEmpty(Observable.Return(Unit.Default));

            return Observable
                .Concat(childExecutions)
                .RunAsync(context.CancellationToken);
        }
    }
}