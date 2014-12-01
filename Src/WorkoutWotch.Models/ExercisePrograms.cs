using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using Kent.Boogaart.HelperTrinity.Extensions;

namespace WorkoutWotch.Models
{
    public sealed class ExercisePrograms
    {
        private readonly IImmutableList<ExerciseProgram> programs;

        public ExercisePrograms(IEnumerable<ExerciseProgram> programs)
        {
            programs.AssertNotNull("programs", assertContentsNotNull: true);
            this.programs = programs.ToImmutableList();
        }

        public IImmutableList<ExerciseProgram> Programs
        {
            get { return this.programs; }
        }
    }
}

