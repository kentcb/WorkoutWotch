namespace WorkoutWotch.Models.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;
    using Genesis.Ensure;

    public sealed class SequenceAction : IAction
    {
        private readonly IImmutableList<IAction> children;
        private readonly TimeSpan duration;

        public SequenceAction(IEnumerable<IAction> children)
        {
            Ensure.ArgumentNotNull(children, nameof(children), assertContentsNotNull: true);

            this.children = children.ToImmutableList();
            this.duration = this
                .children
                .Select(x => x.Duration)
                .DefaultIfEmpty()
                .Aggregate((running, next) => running + next);
        }

        public TimeSpan Duration => this.duration;

        public IImmutableList<IAction> Children =>  this.children;

        public IObservable<Unit> Execute(ExecutionContext context)
        {
            Ensure.ArgumentNotNull(context, nameof(context));

            var childExecutions = this
                .children
                .Select(
                    child =>
                        Observables
                            .Unit
                            .Select(
                                _ =>
                                {
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
                            .SelectMany(x => x.Child.Execute(context)));

            return Observable
                .Concat(childExecutions)
                .DefaultIfEmpty();
        }
    }
}