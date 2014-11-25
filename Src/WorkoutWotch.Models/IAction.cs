using System;
using System.Threading.Tasks;

namespace WorkoutWotch.Models
{
    public interface IAction
    {
        TimeSpan Duration
        {
            get;
        }

        Task ExecuteAsync(ExecutionContext context);
    }
}

