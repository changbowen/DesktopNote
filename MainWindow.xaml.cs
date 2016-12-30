using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DesktopNote
{
    public partial class MainWindow : Window
    {
        private object Lock_Save = new object();
        private static int CountDown = 0;
        Point mousepos;
        private bool fbopen = false;
                
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Docking
        public DockStatus lastdockstatus;
        private Rect currScrnRect;

        public enum DockStatus { None, Docking, Left, Right, Top, Bottom }

        public DockStatus DockedTo
        {
            get { return (DockStatus)GetValue(DockedToProperty); }
            set { SetValue(DockedToProperty, value); }
        }

        public static readonly DependencyProperty DockedToProperty =
            DependencyProperty.Register("DockedTo", typeof(DockStatus), typeof(MainWindow), new PropertyMetadata(DockStatus.None, new PropertyChangedCallback(OnDockedToChanged)));

        private static void OnDockedToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ds = (DockStatus)e.NewValue;
            if (ds == DockStatus.None) ((Window)d).ResizeMode = ResizeMode.CanResizeWithGrip;
            else ((Window)d).ResizeMode = ResizeMode.NoResize;
        }

        internal void DockToSide(bool changpos = false)
        {
            if (DockedTo == DockStatus.None)
            {
                double toval;
                DependencyProperty tgtpro;
                double pad = 15d;
                DockStatus dockto;
                if (changpos)
                {
                    currScrnRect = new GetCurrentMonitor().GetInfo();
                    if (Left <= currScrnRect.Left) //dock left
                    {
                        toval = currScrnRect.Left - ActualWidth + pad;
                        tgtpro = LeftProperty;
                        dockto = DockStatus.Left;
                    }

                    else if (Left + ActualWidth >= currScrnRect.Right) //dock right
                    {
                        toval = currScrnRect.Right - pad;
                        tgtpro = LeftProperty;
                        dockto = DockStatus.Right;
                    }

                    else if (Top <= currScrnRect.Top) //dock top
                    {
                        toval = currScrnRect.Top - ActualHeight + pad;
                        tgtpro = TopProperty;
                        dockto = DockStatus.Top;
                    }

                    else if (Top + ActualHeight >= currScrnRect.Bottom) //dock bottom
                    {
                        toval = currScrnRect.Bottom - pad;
                        tgtpro = TopProperty;
                        dockto = DockStatus.Bottom;
                    }
                    else
                    {
                        lastdockstatus = DockStatus.None;
                        Topmost = false;
                        return;
                    }
                    lastdockstatus = dockto;
                }
                else //'restore last docking position
                {
                    dockto = lastdockstatus;
                    switch (lastdockstatus)
                    {
                        case DockStatus.Left:
                            toval = currScrnRect.Left - ActualWidth + pad;
                            tgtpro = LeftProperty;
                            break;
                        case DockStatus.Right:
                            toval = currScrnRect.Right - pad;
                            tgtpro = LeftProperty;
                            break;
                        case DockStatus.Top:
                            toval = currScrnRect.Top - ActualHeight + pad;
                            tgtpro = TopProperty;
                            break;
                        case DockStatus.Bottom:
                            toval = currScrnRect.Bottom - pad;
                            tgtpro = TopProperty;
                            break;
                        default:
                            return;
                    }
                }

                Topmost = true;
                var anim_move = new DoubleAnimation(toval, new Duration(new TimeSpan(0, 0, 0, 0, 500)), FillBehavior.Stop);
                anim_move.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                var anim_fade = new DoubleAnimation(0.4, new Duration(new TimeSpan(0, 0, 0, 0, 300)));
                anim_fade.BeginTime = new TimeSpan(0, 0, 0, 0, 200);
                var anim_prop = new ObjectAnimationUsingKeyFrames();
                anim_prop.KeyFrames.Add(new DiscreteObjectKeyFrame(DockStatus.Docking, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))));
                anim_prop.KeyFrames.Add(new DiscreteObjectKeyFrame(dockto, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 500))));
                BeginAnimation(tgtpro, anim_move);
                BeginAnimation(OpacityProperty, anim_fade);
                BeginAnimation(DockedToProperty, anim_prop);
            }
        }

        private void UnDock()
        {
            if (DockedTo != DockStatus.Docking && DockedTo != DockStatus.None)
            {
                double toval;
                DependencyProperty tgtpro;
                //double pad = 15d;
                DockStatus dockto;
                if (DockedTo == DockStatus.Left) //Me.Left = currScrnRect.left - Me.ActualWidth + pad Then
                {
                    toval = currScrnRect.Left;
                    tgtpro = LeftProperty;
                    dockto = DockStatus.None;
                }
                else if (DockedTo == DockStatus.Right) //Me.Left = currScrnRect.right - pad Then
                {
                    toval = currScrnRect.Right - ActualWidth;
                    tgtpro = LeftProperty;
                    dockto = DockStatus.None;
                }
                else if (DockedTo == DockStatus.Top) //Me.Top = currScrnRect.top - Me.ActualHeight + pad Then
                {
                    toval = currScrnRect.Top;
                    tgtpro = TopProperty;
                    dockto = DockStatus.None;
                }
                else if (DockedTo == DockStatus.Bottom) //Me.Top = currScrnRect.bottom - pad Then
                {
                    toval = currScrnRect.Bottom - ActualHeight;
                    tgtpro = TopProperty;
                    dockto = DockStatus.None;
                }
                else
                    return;

                Topmost = true;
                var anim_move = new DoubleAnimation(toval, new Duration(new TimeSpan(0, 0, 0, 0, 300)), FillBehavior.Stop);
                anim_move.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                var anim_fade = new DoubleAnimationUsingKeyFrames();
                anim_fade.KeyFrames.Add(new DiscreteDoubleKeyFrame(1, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))));
                var anim_prop = new ObjectAnimationUsingKeyFrames();
                anim_prop.KeyFrames.Add(new DiscreteObjectKeyFrame(DockStatus.Docking, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0))));
                anim_prop.KeyFrames.Add(new DiscreteObjectKeyFrame(dockto, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 500))));
                BeginAnimation(tgtpro, anim_move);
                BeginAnimation(OpacityProperty, anim_fade);
                BeginAnimation(DockedToProperty, anim_prop);
            }
        }

        private void Win_Main_MouseEnter(object sender, MouseEventArgs e)
        {
            //undocking
            if (Properties.Settings.Default.AutoDock) UnDock();
        }

        private void Win_Main_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Properties.Settings.Default.AutoDock && 
                Application.Current.Windows.Count <= App.MaxWindowCount && //to prevent docking when search window is visible. FormatBox is the 2nd window.
                !RTB_Main.IsKeyboardFocusWithin &&
                !fbopen)
                DockToSide();
        }
        #endregion

        #region RichTextBox Events
        private void RTB_Main_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RTB_Main.IsFocused)
            {
                CountDown = 2000;
                TB_Status.Visibility = Visibility.Hidden;
            }
        }

        private void RTB_Main_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
                App.fb.ToggleStrike(null, null);
            else if (e.Key == Key.V && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                App.fb.PasteAsText(null, null);
            else if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
                App.fb.Find(null, null);
            else if (e.Key == Key.H && Keyboard.Modifiers == ModifierKeys.Control)
                App.fb.ToggleHighlight(null, null);
        }

        private void RTB_Main_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //when padding is set on list, changing font size results in incorrect bullet position.
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (e.Delta > 0) //wheel up
                    App.fb.IncreaseSize(null, null);
                else
                    App.fb.DecreaseSize(null, null);
            }
        }

        private void RTB_Main_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Properties.Settings.Default.AutoDock &&
                Application.Current.Windows.Count <= App.MaxWindowCount &&
                !fbopen)
                DockToSide();
        }

        private void RTB_Main_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            fbopen = true;
            e.Handled = true;
            if (!RTB_Main.Selection.IsEmpty)
            {
                var caretfont = RTB_Main.Selection.GetPropertyValue(TextElement.FontFamilyProperty) as FontFamily;
                if (caretfont != null)
                    App.fb.CB_Font.SelectedValue = caretfont.Source;
                else //multiple fonts
                    App.fb.CB_Font.SelectedIndex = -1;
                App.fb.CB_Font.ToolTip = (string)Application.Current.Resources["tooltip_font_selection"];

                var caretfontcolor = RTB_Main.Selection.GetPropertyValue(TextElement.ForegroundProperty) as SolidColorBrush;
                var fontcolorpicker = (Xceed.Wpf.Toolkit.ColorPicker)App.fb.CP_Font.Content;
                if (caretfontcolor != null) fontcolorpicker.SelectedColor = new Color?(caretfontcolor.Color);
                else fontcolorpicker.SelectedColor = null;

                var caretbackcolor = RTB_Main.Selection.GetPropertyValue(TextElement.BackgroundProperty) as SolidColorBrush;
                var backcolorpicker = (Xceed.Wpf.Toolkit.ColorPicker)App.fb.CP_Back.Content;
                if (caretbackcolor != null) backcolorpicker.SelectedColor = new Color?(caretbackcolor.Color);
                else backcolorpicker.SelectedColor = null;
            }
            else
            {
                App.fb.CB_Font.SelectedValue = Properties.Settings.Default.Font;
                App.fb.CB_Font.ToolTip = (string)Application.Current.Resources["tooltip_font_default"];
                ((Xceed.Wpf.Toolkit.ColorPicker)App.fb.CP_Font.Content).SelectedColor = Properties.Settings.Default.FontColor;
                ((Xceed.Wpf.Toolkit.ColorPicker)App.fb.CP_Back.Content).SelectedColor = Properties.Settings.Default.BackColor;
            }
            App.fb.FadeIn();
        }

        private void RTB_Main_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && App.fb?.Opacity == 1)
            {
                fbopen = false;
                App.fb.FadeOut();
            }
        }

        #endregion

        #region Rect Events
        private void Rec_BG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rec_BG.CaptureMouse();
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                    WindowState = WindowState.Maximized;
                else if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
            }
            else
                mousepos = e.GetPosition(this);
        }

        private void Rec_BG_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rec_BG.ReleaseMouseCapture();
            if (Properties.Settings.Default.AutoDock) DockToSide(true);
        }

        private void Rec_BG_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && mousepos != null)
            {
                var pos = e.GetPosition(this);
                Left += pos.X - mousepos.X;
                Top += pos.Y - mousepos.Y;
            }
        }
        #endregion

        private void SaveNotes()
        {
            while (true)
            {
                while (CountDown<=0)
                {
                    Thread.Sleep(1000);
                }
                do
                {
                    Thread.Sleep(500);
                    CountDown -= 500;
                } while (CountDown>0);
                SaveToXamlPkg();
            }
        }

        private void SaveToXamlPkg()
        {
            lock (Lock_Save)
            {
                TextRange tr = null;
                bool isUIthread = Dispatcher.CheckAccess();
                string result;
                if (isUIthread)
                    tr = new TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd);
                else
                    Dispatcher.Invoke(delegate { tr = new TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd); });

                try
                {
                    if (isUIthread)
                    {
                        using (var ms = new FileStream(Properties.Settings.Default.Doc_Location, FileMode.Create))
                        {
                            tr.Save(ms, DataFormats.XamlPackage, true);
                        }
                        File.WriteAllText(Properties.Settings.Default.Bak_Location, tr.Text);
                    }
                    else
                    {
                        Dispatcher.Invoke(delegate
                        {
                            using (var ms = new FileStream(Properties.Settings.Default.Doc_Location, FileMode.Create))
                            {
                                tr.Save(ms, DataFormats.XamlPackage, true);
                            };
                            File.WriteAllText(Properties.Settings.Default.Bak_Location, tr.Text);
                        });
                    }
                    result = (string)Application.Current.Resources["status_saved"];
                }
                catch
                {
                    result = (string)Application.Current.Resources["status_save_failed"];
                }

                if (isUIthread)
                {
                    TB_Status.Text = result;
                    TB_Status.Visibility = Visibility.Visible;
                }
                else
                {
                    Dispatcher.Invoke(delegate
                    {
                        TB_Status.Text = result;
                        TB_Status.Visibility = Visibility.Visible;
                    });
                }
            }
        }

        internal void Quit(bool savesetting)
        {
            SaveToXamlPkg();
            var set = Properties.Settings.Default;
            if (savesetting)
            {
                set.Win_Pos = new System.Drawing.Point((int)Left, (int)Top);
                set.Win_Size = new System.Drawing.Size((int)Width, (int)Height);
                set.DockedTo = (int)lastdockstatus;
                set.Save();
            }
            Application.Current.Shutdown();
        }

        private void Win_Main_Loaded(object sender, RoutedEventArgs e)
        {
            var set = Properties.Settings.Default;
            //check and merge previous settings
            if (set.UpgradeFlag == true)
            {
                set.Upgrade();
                set.UpgradeFlag = false;
                set.Save();
            }

            //load settings
            Width = set.Win_Size.Width;
            Height = set.Win_Size.Height;
            if (set.Win_Pos != null)
            {
                Left = set.Win_Pos.X;
                Top = set.Win_Pos.Y;
            }

            App.fb = new Win_Format();
            App.fb.Tag = RTB_Main;
            App.fb.Owner = this; //causing fb to close when mainwin closes.
            lastdockstatus = (DockStatus)set.DockedTo;
            RTB_Main.FontFamily = new FontFamily(set.Font);
            RTB_Main.Foreground = new SolidColorBrush(set.FontColor);
            ((Xceed.Wpf.Toolkit.ColorPicker)App.fb.CP_Font.Content).SelectedColor = set.FontColor;
            RTB_Main.Background = new SolidColorBrush(set.BackColor);
            ((Xceed.Wpf.Toolkit.ColorPicker)App.fb.CP_Back.Content).SelectedColor = set.BackColor;
            Rec_BG.Fill = new SolidColorBrush(set.PaperColor);

            //add fonts to menu
            foreach (var f in Fonts.SystemFontFamilies)
            {
                var mi = new ComboBoxItem
                {
                    Content = f.Source,
                    FontFamily = f,
                    FontSize = this.FontSize + 4,
                    ToolTip = f.Source
                };
                App.fb.CB_Font.Items.Add(mi);
                if (f.Source == set.Font) mi.IsSelected = true;
            }
            App.fb.CB_Font.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Content", System.ComponentModel.ListSortDirection.Ascending));
            App.fb.CB_Font.SelectionChanged += (object s1, SelectionChangedEventArgs e1) =>
            {
                if (App.fb.Opacity == 1 && e1.AddedItems.Count == 1)
                {
                    var mi = (ComboBoxItem)e1.AddedItems[0];

                    if (!RTB_Main.Selection.IsEmpty) //only change selected
                            RTB_Main.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, mi.FontFamily);
                    else //change default
                        {
                        RTB_Main.FontFamily = mi.FontFamily;
                        set.Font = mi.FontFamily.Source;
                    }
                }
            };
               
            //loading contents
            if (File.Exists(Properties.Settings.Default.Doc_Location))
            {
                try
                {
                    var tr = new TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd);
                    tr.Load(new FileStream(Properties.Settings.Default.Doc_Location, FileMode.Open), DataFormats.XamlPackage);
                }
                catch
                {
                    MessageBox.Show((string)Application.Current.Resources["msgbox_load_error"] + "\r\n" + set.Bak_Location,
                        (string)Application.Current.Resources["msgbox_title_load_error"], MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }

            //unifying font for new paragraghs. without these, wont be able to change fonts after reload.
            //the following doesnt affect specifically set font sizes in Inlines & Run.
            if (RTB_Main.Document.Blocks.Count > 0)
            {
                RTB_Main.FontSize = RTB_Main.Document.Blocks.FirstBlock.FontSize;
                foreach (var b in RTB_Main.Document.Blocks)
                {
                    b.ClearValue(TextElement.FontSizeProperty);
                    b.ClearValue(TextElement.FontFamilyProperty);
                    b.ClearValue(TextElement.ForegroundProperty);
                    b.ClearValue(TextElement.BackgroundProperty);
                }
            }

            RTB_Main.IsUndoEnabled = false;
            RTB_Main.IsUndoEnabled = true;
            //without the above two lines, Load actions can be undone.


            currScrnRect = new GetCurrentMonitor().GetInfo();

            var task_save = new Thread(SaveNotes);
            task_save.IsBackground = true;
            task_save.Start();
        }


    }
}
