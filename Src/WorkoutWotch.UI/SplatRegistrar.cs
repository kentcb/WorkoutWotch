namespace WorkoutWotch.UI
{
    using Genesis.Ensure;
    using Genesis.Logging;
    using global::ReactiveUI;
    using ReactiveUI;
    using Splat;
    using WorkoutWotch.ViewModels;

    // ReactiveUI depends on Splat, which is essentially a service locator. Thus, we cannot rely solely on our
    // composition root to supply dependencies throughout the application. We must also prime Splat with the
    // information it requires.
    public abstract class SplatRegistrar
    {
        private readonly Genesis.Logging.ILogger logger;

        protected SplatRegistrar()
        {
            this.logger = LoggerService.GetLogger(this.GetType());
        }

        public void Register(IMutableDependencyResolver splatLocator, CompositionRoot compositionRoot)
        {
            Ensure.ArgumentNotNull(splatLocator, nameof(splatLocator));

            using (this.logger.Perf("Registration."))
            {
                this.RegisterViews(splatLocator);
                this.RegisterScreen(splatLocator, compositionRoot);
                this.RegisterCommandBinders(splatLocator, compositionRoot);
                this.RegisterPlatformComponents(splatLocator, compositionRoot);
            }
        }

        private void RegisterViews(IMutableDependencyResolver splatLocator)
        {
            splatLocator.Register(
                this.LoggedCreation<ExerciseProgramsView>,
                typeof(IViewFor<ExerciseProgramsViewModel>));
            splatLocator.Register(
                this.LoggedCreation<ExerciseProgramView>,
                typeof(IViewFor<ExerciseProgramViewModel>));
        }

        private void RegisterScreen(IMutableDependencyResolver splatLocator, CompositionRoot compositionRoot) =>
            splatLocator.RegisterConstant(compositionRoot.ResolveMainViewModel(), typeof(IScreen));

        private void RegisterCommandBinders(IMutableDependencyResolver splatLocator, CompositionRoot compositionRoot) =>
            splatLocator.RegisterConstant(new ControlButtonCommandBinder(), typeof(ICreatesCommandBinding));

        protected abstract void RegisterPlatformComponents(IMutableDependencyResolver splatLocator, CompositionRoot compositionRoot);

        protected T LoggedCreation<T>()
            where T : new()
        {
            this.logger.Debug("Instance of {0} requested.", typeof(T).FullName);

            using (this.logger.Perf("Create {0}.", typeof(T).FullName))
            {
                return new T();
            }
        }
    }
}