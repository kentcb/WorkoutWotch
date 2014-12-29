using System;
using ReactiveUI;

namespace WorkoutWotch.ViewModels
{
	public enum ExerciseProgramsViewModelStatus
	{
        Loading,
        ParseFailed,
        LoadFailed,
        LoadedFromCache,
        LoadedFromCloud
	}

}

