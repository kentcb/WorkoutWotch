namespace WorkoutWotch.UI.iOS.Renderers
{
    using CoreGraphics;
    using UIKit;
    using Xamarin.Forms.Platform.iOS;

    // because iOS does not allow a custom colour for cell disclosure accessories, we need our own custom control :S
    // translated and adapted from: http://www.cocoanetics.com/2010/10/custom-colored-disclosure-indicators/
    public sealed class CellDisclosureAccessory : UIControl
    {
        private const float Width = 4.5f;
        private UIColor color;
        private UIColor highlightedColor;
        private bool isHighlighted;

        public CellDisclosureAccessory()
            : this(new CGRect(0, 0, 11f, 15f))
        {
        }

        public CellDisclosureAccessory(CGRect frame)
            : base(frame)
        {
            this.BackgroundColor = UIColor.Clear;
            this.color = App.ForegroundColor.ToUIColor();
            this.highlightedColor = App.NavigationColor.ToUIColor();
        }

        public UIColor Color
        {
            get { return this.color; }
            set
            {
                this.color = value;
                this.SetNeedsDisplay();
            }
        }

        public UIColor HighlightedColor
        {
            get { return this.highlightedColor; }
            set
            {
                this.highlightedColor = value;
                this.SetNeedsDisplay();
            }
        }

        public bool IsHighlighted
        {
            get { return this.isHighlighted; }
            set
            {
                this.isHighlighted = value;
                this.SetNeedsDisplay();
            }
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            var x = this.Bounds.Right - 3;
            var y = this.Bounds.Top + (this.Bounds.Height / 2);

            using (var context = UIGraphics.GetCurrentContext())
            {
                context.MoveTo(x - Width, y - Width);
                context.AddLineToPoint(x, y);
                context.AddLineToPoint(x - Width, y + Width);
                context.SetLineCap(CGLineCap.Square);
                context.SetLineJoin(CGLineJoin.Miter);
                context.SetLineWidth(3);
                context.SetStrokeColor(this.isHighlighted ? this.highlightedColor.CGColor : this.color.CGColor);
                context.StrokePath();
            }
        }
    }
}