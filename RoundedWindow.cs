//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace DesktopNote
{
    public class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //parameter is the clipped width to preserve
            var offset = 0d; var thickness = 20d;
            if (parameter is string paraStr) {
                var paraAry = paraStr.Split(',', ' ');
                if (paraAry.Length > 0) offset = double.Parse(paraAry[0]);
                if (paraAry.Length > 1) thickness = double.Parse(paraAry[1]);
            }
            var width = (double)values[0];
            var height = (double)values[1];
            return new Rect(new Point(-offset, -offset), new Point(width + offset - thickness, height + offset - thickness));//20是两边空隙总和
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RoundedWindow : Window
    {
        public enum CloseBehaviors { Close, Hide, FadeOutAndHide, FadeOutAndClose }

        #region Properties
        /// <summary>
        /// Get or set the background of the rounded rectangle.
        /// </summary>
        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static readonly new DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(RoundedWindow), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(255, 247, 197))));


        public bool ButtonCloseVisible
        {
            get { return (bool)GetValue(ButtonCloseVisibleProperty); }
            set { SetValue(ButtonCloseVisibleProperty, value); }
        }
        public static readonly DependencyProperty ButtonCloseVisibleProperty =
            DependencyProperty.Register("ButtonCloseVisible", typeof(bool), typeof(RoundedWindow), new PropertyMetadata(true));


        public bool ButtonMaxVisible
        {
            get { return (bool)GetValue(ButtonMaxVisibleProperty); }
            set { SetValue(ButtonMaxVisibleProperty, value); }
        }
        public static readonly DependencyProperty ButtonMaxVisibleProperty =
            DependencyProperty.Register("ButtonMaxVisible", typeof(bool), typeof(RoundedWindow), new PropertyMetadata(false));


        public bool ButtonMinVisible
        {
            get { return (bool)GetValue(ButtonMinVisibleProperty); }
            set { SetValue(ButtonMinVisibleProperty, value); }
        }
        public static readonly DependencyProperty ButtonMinVisibleProperty =
            DependencyProperty.Register("ButtonMinVisible", typeof(bool), typeof(RoundedWindow), new PropertyMetadata(false));


        public CloseBehaviors CloseBehavior
        {
            get { return (CloseBehaviors)GetValue(CloseBehaviorProperty); }
            set { SetValue(CloseBehaviorProperty, value); }
        }
        public static readonly DependencyProperty CloseBehaviorProperty =
            DependencyProperty.Register("CloseBehavior", typeof(CloseBehaviors), typeof(RoundedWindow), new PropertyMetadata(CloseBehaviors.FadeOutAndClose));

        /// <summary>
        /// If set to false, keyboard focus will be set to the owner when the window gets focus.
        /// </summary>
        public new bool Focusable
        {
            get { return (bool)GetValue(FocusableProperty); }
            set { SetValue(FocusableProperty, value); }
        }
        public static readonly new DependencyProperty FocusableProperty =
            DependencyProperty.Register("Focusable", typeof(bool), typeof(RoundedWindow), new PropertyMetadata(true));

        /// <summary>
        /// This maps to the RenderTransformOrigin property of BackgroundGrid.
        /// </summary>
        public Point RenderTransformOrigin_BG
        {
            get { return (Point)GetValue(RenderTransformOrigin_BGProperty); }
            set { SetValue(RenderTransformOrigin_BGProperty, value); }
        }
        public static readonly DependencyProperty RenderTransformOrigin_BGProperty =
            DependencyProperty.Register("RenderTransformOrigin_BG", typeof(Point), typeof(RoundedWindow), new PropertyMetadata(new Point(0d, 0d)));


        public double CornerRadius
        {
            get { return (double)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(double), typeof(RoundedWindow), new PropertyMetadata(8d));


        #endregion


        static RoundedWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RoundedWindow), new FrameworkPropertyMetadata(typeof(RoundedWindow)));
        }

        public static readonly RoutedEvent FadingInEvent = EventManager.RegisterRoutedEvent("FadingIn", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RoundedWindow));
        public event RoutedEventHandler FadingIn
        {
            add
            {
                AddHandler(FadingInEvent, value);
            }
            remove
            {
                RemoveHandler(FadingInEvent, value);
            }
        }

        public static readonly RoutedEvent FadingOutEvent = EventManager.RegisterRoutedEvent("FadingOut", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RoundedWindow));
        public event RoutedEventHandler FadingOut
        {
            add
            {
                AddHandler(FadingOutEvent, value);
            }
            remove
            {
                RemoveHandler(FadingOutEvent, value);
            }
        }



        public RoundedWindow()
        {
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            base.Background = null;
        }

        /// <summary>
        /// This is a grid on which the scaling animations are applied.
        /// </summary>
        internal Grid BackgroundGrid { get; set; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BackgroundGrid = (Grid)GetTemplateChild("Grid_Main");

            Button minimizeButton = GetTemplateChild("minimizeButton") as Button;
            if (minimizeButton != null) minimizeButton.Click += MinimizeClick;

            Button restoreButton = GetTemplateChild("restoreButton") as Button;
            if (restoreButton != null) restoreButton.Click += RestoreClick;

            Button closeButton = GetTemplateChild("closeButton") as Button;
            if (closeButton != null) closeButton.Click += CloseClick;

            if (!Focusable && Owner != null)
            {
                PreviewGotKeyboardFocus += (object sender, KeyboardFocusChangedEventArgs e) =>
                {
                    e.Handled = true;
                    var o = sender as Window;
                    if (o != null && o != this) o.Focus();
                };
            }
        }

        #region Click Events
        protected void MinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        protected void RestoreClick(object sender, RoutedEventArgs e)
        {
            WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        protected void CloseClick(object sender, RoutedEventArgs e)
        {
            //make sure the scale animation ends at the top right corner when close is clicked.
            RenderTransformOrigin_BG = new Point(1d, 0d);
            Close();
        }
        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            switch (CloseBehavior)
            {
                case CloseBehaviors.Close:
                    break;
                case CloseBehaviors.FadeOutAndClose:
                    e.Cancel = true;
                    FadeOut(true);
                    break;
                case CloseBehaviors.FadeOutAndHide:
                    e.Cancel = true;
                    FadeOut();
                    break;
                case CloseBehaviors.Hide:
                    e.Cancel = true;
                    Hide();
                    break;
            }
        }

        /// <summary>
        /// Show the window without fading animation.
        /// </summary>
        public new void Show()
        {
            base.Show();
            //ensure the contents are visible when not using fading animation.
            BackgroundGrid.RenderTransform = new ScaleTransform(1d, 1d);
            Opacity = 1;
        }

        /// <summary>
        /// Fade in the window at mouse position. Or the specified Left and Top position.
        /// If the window is already faded in, move the window to the new position. In this case the FadingInEvent will not be raised.
        /// </summary>
        public void FadeIn(double left = double.NaN, double top = double.NaN)
        {
            if (!IsLoaded) base.Show();//how to call something similar to initializecomponent?
            
            //compute mouse position or set to existing values
            Point newpos, realpos;
            double currscrnW = App.CurrScrnRect.Right;
            double currscrnH = App.CurrScrnRect.Bottom;
            if (left.Equals(double.NaN) || top.Equals(double.NaN))//nan==nan returns false.
            {
                //get the physical pixel-based position.
                newpos = PointToScreen(Mouse.GetPosition(this));
                //convert to the actual position considering the DPI settings etc.
                realpos = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice.Transform(newpos);

                //make sure the window is displayed inside the screens.
                double originX, originY;
                if (currscrnW - realpos.X > ActualWidth)
                    originX = 0d;
                else
                {
                    originX = 1d;
                    realpos.X -= ActualWidth;
                }
                if (currscrnH - realpos.Y > ActualHeight)
                    originY = 0d;
                else
                {
                    originY = 1d;
                    realpos.Y -= ActualHeight;
                }
                RenderTransformOrigin_BG = new Point(originX, originY);
            }
            else
            {
                //make sure the window is displayed inside the screens.
                if (left < 0d) left = 0d;
                if (top < 0d) top = 0d;
                if (left + ActualWidth > currscrnW)
                    left = currscrnW - ActualWidth;
                if (top + ActualHeight > currscrnH)
                    top = currscrnH - ActualHeight;
                realpos = new Point(left, top);
            }

            if (Opacity == 0)
            {
                Left = realpos.X;
                Top = realpos.Y;
                base.Show();
                RaiseEvent(new RoutedEventArgs(FadingInEvent));
            }
            else
            {
                //move window to the new cursor location
                var easefunc = new CubicEase() { EasingMode = EasingMode.EaseInOut };
                var anim_move_x = new DoubleAnimation(realpos.X, new Duration(new TimeSpan(0, 0, 0, 0, 300)), FillBehavior.Stop) { EasingFunction = easefunc };
                var anim_move_y = new DoubleAnimation(realpos.Y, new Duration(new TimeSpan(0, 0, 0, 0, 300)), FillBehavior.Stop) { EasingFunction = easefunc };
                BeginAnimation(LeftProperty, anim_move_x);
                BeginAnimation(TopProperty, anim_move_y);
            }
        }
        /// <summary>
        /// Fade out the window. This ignores the CloseBehavior setting.
        /// </summary>
        /// <param name="closeafterfade">Set to true to close the window after. Otherwise it is only hidden.</param>
        public async Task FadeOut(bool closeafterfade = false)
        {
            if (IsLoaded)//without this it will crash at the below line.
            {
                RaiseEvent(new RoutedEventArgs(FadingOutEvent));
                //need to be longer than the fading animation otherwise the window will flash when Show() is called.
                await Task.Run(() => System.Threading.Thread.Sleep(250));
                if (closeafterfade)
                {
                    CloseBehavior = CloseBehaviors.Close;
                    Close();
                }
                else Hide();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed) DragMove();
        }
    }
}
