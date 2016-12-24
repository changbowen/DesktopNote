using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

namespace DesktopNote
{
    public partial class FormatBox : Window
    {
        RichTextBox RTB_Main;

        //private const int GWL_EXSTYLE = -20;
        //private const int WS_EX_NOACTIVATE = 0x08000000;
        //private const int WS_EX_TOOLWINDOW = 0x00000080;
        //[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        //public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        //[System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        //public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //private void FB1_SourceInitialized(object sender, EventArgs e)
        //{
        //    //not working
        //    var interopHelper = new System.Windows.Interop.WindowInteropHelper(this);
        //    int exStyle = GetWindowLong(interopHelper.Handle, GWL_EXSTYLE);
        //    SetWindowLong(interopHelper.Handle, GWL_EXSTYLE, exStyle | WS_EX_NOACTIVATE);
        //}

        public FormatBox()
        {
            InitializeComponent();
        }

        private void FB1_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            e.Handled = true;
            Owner.Focus();
        }

        public void Popup()
        {
            //refresh reference for when reset setting is executed
            RTB_Main = App.mainwin.RTB_Main;
            //compute mouse position
            var newpos = RTB_Main.PointToScreen(Mouse.GetPosition(RTB_Main));
            var realpos = PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformFromDevice.Transform(newpos);

            if (Grid_Main.Opacity == 0)
            {
                Left = realpos.X;
                Top = realpos.Y;
                //Rec_BG.Fill = new SolidColorBrush(Colors.WhiteSmoke); //new SolidColorBrush(Properties.Settings.Default.PaperColor);
                //Topmost = false;
                //Topmost = true;
                Show();
                //why show() would make the window flash once???
                //to avoid that, SB_Out sets scale transform to 0 at the end.
                //turns out the above method is not working well.
                ((Storyboard)FindResource("SB_In")).Begin(App.fb.Grid_Main);
            }
            else
            {
                //move window to the new cursor location
                var easefunc = new CubicEase() { EasingMode = EasingMode.EaseInOut };
                var anim_move_x = new DoubleAnimation(realpos.X, new Duration(new TimeSpan(0, 0, 0, 0, 300)), FillBehavior.Stop) { EasingFunction = easefunc };
                var anim_move_y = new DoubleAnimation(realpos.Y, new Duration(new TimeSpan(0, 0, 0, 0, 300)), FillBehavior.Stop) { EasingFunction = easefunc };
                //Topmost = false;
                //Topmost = true;
                BeginAnimation(LeftProperty, anim_move_x);
                BeginAnimation(TopProperty, anim_move_y);
            }
        }
        
        public async void Unpop()
        {
            var sb = ((Storyboard)FindResource("SB_Out"));
            //Topmost = false;
            //Topmost = true;
            sb.Begin();
            await Task.Run(() => System.Threading.Thread.Sleep(200));
            Hide();
        }

        #region Menu Events
        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show((string)Application.Current.Resources["msgbox_help"], "", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                System.Diagnostics.Process.Start("iexplore.exe", "https://github.com/changbowen/DesktopNote");
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.mainwin.Quit(true);
        }

        internal void Button_ResetFormats_Click(object sender, RoutedEventArgs e)
        {
            //clear custom formats
            var tr = new TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd);
            tr.ClearAllProperties();

            //reset global font size
            RTB_Main.FontSize = App.mainwin.FontSize;

            //resetting paper color should be processed with Reset Settings
            //var cp = (Color)ColorConverter.ConvertFromString((string)Properties.Settings.Default.Properties["PaperColor"].DefaultValue);
            //Properties.Settings.Default.PaperColor = cp;
            //Rec_BG.Fill = new SolidColorBrush(cp);
        }

        private void Button_ResetSet_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();

            App.mainwin.Close();
            App.mainwin = new MainWindow();
            Application.Current.MainWindow = App.mainwin;
            App.mainwin.Show();
        }

        internal void ToggleStrike(object sender, RoutedEventArgs e)
        {
            //strike-through
            dynamic tdc = RTB_Main.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (tdc == DependencyProperty.UnsetValue || tdc.Count > 0)
                tdc = null;
            else
                tdc = TextDecorations.Strikethrough;
            RTB_Main.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, tdc);
        }

        internal void ToggleHighlight(object sender, RoutedEventArgs e)
        {
            var tdc = RTB_Main.Selection.GetPropertyValue(TextElement.BackgroundProperty) as SolidColorBrush;
            if (tdc != null)
                RTB_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            else
            {
                RTB_Main.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Black));
                RTB_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.Yellow));
            }
        }

        internal void IncreaseSize(object sender, RoutedEventArgs e)
        {
            if (RTB_Main.Selection.IsEmpty)
                RTB_Main.FontSize += 1;
            else
            {
                var ele = RTB_Main.Selection.Start.GetNextContextPosition(LogicalDirection.Forward).GetAdjacentElement(LogicalDirection.Forward);
                Image img = null;
                switch (ele.GetType().Name)
                {
                    case "BlockUIContainer":
                        img = ((BlockUIContainer)ele).Child as Image;
                        break;
                    case "InlineUIContainer":
                        img = ((InlineUIContainer)ele).Child as Image;
                        break;
                    case "Image":
                        img = (Image)ele;
                        break;
                }
                if (img != null)
                {
                    img.Width += 2;
                    img.Height += 2;
                }
                else
                    EditingCommands.IncreaseFontSize.Execute(null, RTB_Main);
            }
        }

        internal void DecreaseSize(object sender, RoutedEventArgs e)
        {
            if (RTB_Main.Selection.IsEmpty)
            {
                if (RTB_Main.FontSize > 1) RTB_Main.FontSize -= 1;
            }
            else
            {
                var ele = RTB_Main.Selection.Start.GetNextContextPosition(LogicalDirection.Forward).GetAdjacentElement(LogicalDirection.Forward);
                Image img = null;
                switch (ele.GetType().Name)
                {
                    case "BlockUIContainer":
                        img = ((BlockUIContainer)ele).Child as Image;
                        break;
                    case "InlineUIContainer":
                        img = ((InlineUIContainer)ele).Child as Image;
                        break;
                    case "Image":
                        img = (Image)ele;
                        break;
                }
                if (img != null)
                {
                    if (img.Width > 2 && img.Height > 2)
                    {
                        img.Width -= 2;
                        img.Height -= 2;
                    }
                }
                else
                    EditingCommands.DecreaseFontSize.Execute(null, RTB_Main);
            }
        }

        private void ColorChange(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue && ((Xceed.Wpf.Toolkit.ColorPicker)sender).IsOpen)
            {
                var cp = (ContentPresenter)VisualTreeHelper.GetParent((DependencyObject)sender);
                if (cp != null)
                {
                    switch (cp.Name)
                    {
                        case "CP_Font":
                            if (!RTB_Main.Selection.IsEmpty) //only change selected
                                RTB_Main.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(e.NewValue.Value));
                            else //change default
                            {
                                RTB_Main.Foreground = new SolidColorBrush(e.NewValue.Value);
                                Properties.Settings.Default.FontColor = e.NewValue.Value;
                            }
                            break;
                        case "CP_Back":
                            if (!RTB_Main.Selection.IsEmpty) //only change selected
                                RTB_Main.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(e.NewValue.Value)); //the caret color will be changed as well
                            else //change default
                            {
                                RTB_Main.Background = new SolidColorBrush(e.NewValue.Value);
                                Properties.Settings.Default.BackColor = e.NewValue.Value;
                            }
                            break;
                        case "CP_Paper":
                            App.mainwin.Rec_BG.Fill = new SolidColorBrush(e.NewValue.Value);
                            Properties.Settings.Default.PaperColor = e.NewValue.Value;
                            break;
                    }
                }
            }
        }

        private void CB_AutoStart_Click(object sender, RoutedEventArgs e)
        {
            var run = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (CB_AutoStart.IsChecked == true)
                run.SetValue(App.mainwin.assname, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
            else
                run.DeleteValue(App.mainwin.assname, false);
        }

        private void CB_AutoDock_Click(object sender, RoutedEventArgs e)
        {
            if (CB_AutoDock.IsChecked == true)
            {
                Properties.Settings.Default.AutoDock = true;
                App.mainwin.DockToSide(true);
            }
            else
            {
                Properties.Settings.Default.AutoDock = false;
                App.mainwin.Topmost = false;
            }
            Properties.Settings.Default.Save();
        }

        internal void PasteAsText(object sender, RoutedEventArgs e)
        {
            var txt = Clipboard.GetText();
            RTB_Main.CaretPosition.InsertTextInRun(txt);
            var newpos = RTB_Main.CaretPosition.GetPositionAtOffset(txt.Length);
            if (newpos != null) RTB_Main.CaretPosition = newpos;
        }

        internal void Find(object sender, RoutedEventArgs e)
        {
            new Win_Search().Show();
        }

        #endregion
    }
}
