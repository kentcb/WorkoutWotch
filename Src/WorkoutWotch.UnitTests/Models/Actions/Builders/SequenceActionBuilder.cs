namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System.Collections.Generic;
    using Genesis.TestUtil;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;

    public sealed class SequenceActionBuilder : IBuilder
    {
        private List<IAction> children;

        public SequenceActionBuilder()
        {
            this.children = new List<IAction>();
        }

        public SequenceActionBuilder WithChild(IAction child) =>
            this.With(ref this.children, child);

        public SequenceActionBuilder WithChildren(IEnumerable<IAction> children) =>
            this.With(ref this.children, children);

        public SequenceAction Build() =>
            new SequenceAction(this.children);

        public static implicit operator SequenceAction(SequenceActionBuilder builder) =>
            builder.Build();
    }
}