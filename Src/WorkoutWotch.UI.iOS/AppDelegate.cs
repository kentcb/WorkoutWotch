using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using WorkoutWotch.Services.iOS.Audio;
using WorkoutWotch.Services.iOS.Speech;
using WorkoutWotch.Services.iOS.SystemNotifications;
using ReactiveUI;
using System.Threading.Tasks;
using System.Drawing;
using WorkoutWotch.Models;
using WorkoutWotch.Services.Logger;
using WorkoutWotch.Services.Delay;
using WorkoutWotch.Models.EventMatchers;
using WorkoutWotch.Models.Events;
using WorkoutWotch.Models.Actions;

namespace WorkoutWotch.UI.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;

        //
        // This method is invoked when the application has loaded and is ready to run. In this
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            
            // If you have defined a root view controller, set it here:
            window.RootViewController = new TestView();

            // make the window visible
            window.MakeKeyAndVisible();
            
            return true;
        }
    }

    public class TestViewModel : ReactiveObject
    {
        private readonly IReactiveCommand testCommand;
        private readonly ExerciseProgram exerciseProgram;
        private ExecutionContext ec;

        public TestViewModel()
        {
            this.exerciseProgram = this.CreateExerciseProgram();
            this.testCommand = ReactiveCommand.CreateAsyncTask(this.OnTestAsync);
        }

        public IReactiveCommand TestCommand
        {
            get { return this.testCommand; }
        }

        private async Task OnTestAsync(object parameter)
        {
            this.ec = new ExecutionContext();
            await this.exerciseProgram.ExecuteAsync(this.ec);
        }

        private ExerciseProgram CreateExerciseProgram()
        {
            var loggerService = new LoggerService();
            var audioService = new AudioService();
            var delayService = new DelayService();
            var speechService = new SpeechService();

            return new ExerciseProgram(
                loggerService,
                "Example Program",
                new[]
                {
                    new Exercise(
                        loggerService,
                        speechService,
                        "Push-ups",
                        3,
                        5,
                        new[]
                        {
                            new MatcherWithAction(
                                new NumberedEventMatcher<BeforeSetEvent>(e => e.Number > 1),
                                new BreakAction(delayService, speechService, TimeSpan.FromSeconds(7))),
                            new MatcherWithAction(
                                new TypedEventMatcher<DuringRepetitionEvent>(),
                                new MetronomeAction(
                                    audioService,
                                    delayService,
                                    loggerService,
                                    new[]
                                    {
                                        new MetronomeTick(TimeSpan.Zero, MetronomeTickType.Bell),
                                        new MetronomeTick(TimeSpan.FromMilliseconds(500), MetronomeTickType.Click),
                                        new MetronomeTick(TimeSpan.FromMilliseconds(750), MetronomeTickType.Click),
                                        new MetronomeTick(TimeSpan.FromMilliseconds(500), MetronomeTickType.None)
                                    })),
                            new MatcherWithAction(
                                new NumberedEventMatcher<AfterRepetitionEvent>(e => e.Number == e.ExecutionContext.CurrentExercise.RepetitionCount - 2),
                                new SayAction(speechService, "Two left"))
                        })
                });
        }
    }

    public class TestView : ReactiveViewController, IViewFor<TestViewModel>
    {
        private TestViewModel vm = new TestViewModel();
        private UIButton button;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            button = new UIButton(this.View.Frame);
            button.SetTitle("Test", UIControlState.Normal);
            this.View.AddSubview(button);

            this.BindCommand(this.ViewModel, x => x.TestCommand, x => x.button);
        }

        public TestViewModel ViewModel
        {
            get { return this.vm; }
            set { this.RaiseAndSetIfChanged(ref this.vm, value); }
        }

        object IViewFor.ViewModel
        {
            get { return this.ViewModel; }
            set { this.ViewModel = (TestViewModel)value; }
        }
    }
}

