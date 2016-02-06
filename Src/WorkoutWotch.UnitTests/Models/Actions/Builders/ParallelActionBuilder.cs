namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System.Collections.Generic;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;

    internal sealed class ParallelActionBuilder : IBuilder
    {
        private List<IAction> children;

        public ParallelActionBuilder()
        {
            this.children = new List<IAction>();
        }

        public ParallelActionBuilder WithChild(IAction child) =>
            this.With(ref this.children, child);

        public ParallelActionBuilder WithChildren(IEnumerable<IAction> children) =>
            this.With(ref this.children, children);

        public ParallelAction Build() =>
            new ParallelAction(this.children);
        

        public static implicit operator ParallelAction(ParallelActionBuilder builder) =>
            builder.Build();
    }
}