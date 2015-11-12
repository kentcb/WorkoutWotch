namespace WorkoutWotch.UnitTests.Models.Actions.Builders
{
    using System.Collections.Generic;
    using WorkoutWotch.Models;
    using WorkoutWotch.Models.Actions;

    internal sealed class SequenceActionBuilder
    {
        private readonly IList<IAction> children;

        public SequenceActionBuilder()
        {
            this.children = new List<IAction>();
        }

        public SequenceAction Build()
        {
            return new SequenceAction(this.children);
        }

        public SequenceActionBuilder AddChild(IAction child)
        {
            this.children.Add(child);
            return this;
        }
    }
}