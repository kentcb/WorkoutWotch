namespace WorkoutWotch.Models
{
    using System;
    using System.Threading.Tasks;

    public interface IAction
    {
        TimeSpan Duration
        {
            get;
        }

        Task ExecuteAsync(ExecutionContext context);
    }
}