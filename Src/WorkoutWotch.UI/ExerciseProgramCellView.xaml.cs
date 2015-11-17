namespace WorkoutWotch.UI
{
    using System.Reactive.Disposables;
    using ReactiveUI;
    using ReactiveUI.XamForms;
    using Xamarin.Forms;
    using WorkoutWotch.ViewModels;
    using Behaviors;

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