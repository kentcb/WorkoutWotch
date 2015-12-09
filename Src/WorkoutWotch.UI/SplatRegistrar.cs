namespace WorkoutWotch.UI
{
    using global::ReactiveUI;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ReactiveUI;
    using Splat;
    using WorkoutWotch.ViewModels;

    // ReactiveUI depends on Splat, which is essentially a service locator. Thus, we cannot rely solely on our
    // composition root to supply dependencies throughout the application. We must also prime Splat with the
    // information it requires.
    public abstract class SplatRegistrar
    {
        public void Register(IMutableDependencyResolver splatLocator, CompositionRoot compositionRoot)
        {
            splatLocator.AssertNotNull(nameof(splatLocator));

            this.RegisterViews(splatLocator);
            this.RegisterScreen(splatLocator, compositionRoot);
            this.RegisterCommandBinders(splatLocator, compositionRoot);
            this.RegisterPlatformComponents(splatLocator, compositionRoot);
        }

        private void RegisterViews(IMutableDependencyResolver splatLocator)
        {
            splatLocator.Register(() => new ExerciseProgramsView(), typeof(IViewFor<ExerciseProgramsViewModel>));
            splatLocator.Register(() => new ExerciseProgramView(), typeof(IViewFor<ExerciseProgramViewModel>));
        }

        private void RegisterScreen(IMutableDependencyResolver splatLocator, CompositionRoot compositionRoot) =>
            splatLocator.RegisterConstant(compositionRoot.ResolveMainViewModel(), typeof(IScreen));

        private void RegisterCommandBinders(IMutableDependencyResolver splatLocator, CompositionRoot compositionRoot) =>
            splatLocator.RegisterConstant(new ControlButtonCommandBinder(), typeof(ICreatesCommandBinding));

        protected abstract void RegisterPlatformComponents(IMutableDependencyResolver splatLocator, CompositionRoot compositionRoot);
    }
}