namespace WorkoutWotch.UI
{
    using System.Reactive.Disposables;
    using Behaviors;
    using global::ReactiveUI;
    using global::ReactiveUI.XamForms;
    using WorkoutWotch.ViewModels;

    public partial class ExerciseProgramCellView : ReactiveTextCell<ExerciseProgramViewModel>
    {
        public ExerciseProgramCellView()
        {
            InitializeComponent();

            // TODO: setting this from XAML is currently causing a compilation exception (last tried with XF 2.0.0)
            CellBehavior.SetAccessory(this, AccessoryType.HasChildView);

            this
                .WhenActivated(
                    disposables =>
                    {
                        this
                            .OneWayBind(this.ViewModel, x => x.Name, x => x.Text)
                            .AddTo(disposables);
                        this
                            .OneWayBind(this.ViewModel, x => x.Duration, x => x.Detail, x => x.ToString("mm\\:ss"))
                            .AddTo(disposables);
                    });
        }
    }
}