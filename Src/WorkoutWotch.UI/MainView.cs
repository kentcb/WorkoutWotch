namespace WorkoutWotch.UI
{
    using Kent.Boogaart.HelperTrinity.Extensions;
    using ReactiveUI.XamForms;
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