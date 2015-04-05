namespace WorkoutWotch.UI.iOS.Utility
{
    using UIKit;

    // provides extensions that should be used when laying out via the Layout class
    // note the use of ints here rather than floats because comparing floats in our constraint expressions results in annoying compiler warnings
    public static class LayoutExtensions
    {
        public static int Width(this UIView @this) => 0;

        public static int Height(this UIView @this) => 0;

        public static int Left(this UIView @this) => 0;

        public static int X(this UIView @this) => 0;

        public static int Top(this UIView @this) => 0;

        public static int Y(this UIView @this) => 0;

        public static int Right(this UIView @this) => 0;

        public static int Bottom(this UIView @this) => 0;

        public static int Baseline(this UIView @this) => 0;

        public static int Leading(this UIView @this) => 0;

        public static int Trailing(this UIView @this) => 0;

        public static int CenterX(this UIView @this) => 0;

        public static int CenterY(this UIView @this) => 0;
    }
}