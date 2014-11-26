namespace WorkoutWotch.Models
{
    public interface IEvent
    {
        ExecutionContext ExecutionContext
        {
            get;
        }
    }
}