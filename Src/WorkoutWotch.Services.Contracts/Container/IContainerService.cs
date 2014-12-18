namespace WorkoutWotch.Services.Contracts.Container
{
    public interface IContainerService
    {
        T Resolve<T>()
            where T : class;
    }
}