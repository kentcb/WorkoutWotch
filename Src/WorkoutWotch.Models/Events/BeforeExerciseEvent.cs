using System;
using System.Threading.Tasks;
using Kent.Boogaart.HelperTrinity.Extensions;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace WorkoutWotch.Models.Events
{
	public sealed class BeforeExerciseEvent : EventBase
	{
        private readonly Exercise exercise;

        public BeforeExerciseEvent(ExecutionContext executionContext, Exercise exercise)
            : base(executionContext)
        {
            exercise.AssertNotNull("exercise");
            this.exercise = exercise;
        }

        public Exercise Exercise
        {
            get{return this.exercise;}
        }
	}

}