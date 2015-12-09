namespace WorkoutWotch.UI
{
    using global::ReactiveUI.XamForms;
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ViewModels;

    public sealed class MainView : RoutedViewHost
    {
        public MainView(MainViewModel viewModel)
        {
            viewModel.AssertNotNull(nameof(viewModel));
            this.Router = viewModel.Router;
        }
    }
}