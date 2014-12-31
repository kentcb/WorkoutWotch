using System;
using ReactiveUI;
using WorkoutWotch.Utility;
using WorkoutWotch.Models;

namespace WorkoutWotch.ViewModels
{
	public class ExerciseViewModel
	{
        public ExerciseViewModel(Exercise model)
        {
            this.Model = model;
        }

        public Exercise Model
        {
            get;
            set;
        }
	}


}

