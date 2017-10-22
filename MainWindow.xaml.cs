using Hardcodet.Wpf.TaskbarNotification;
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
        public Setting CurrentSetting;
        private object Lock_Save = new object();
        private int CountDown = 0;
        private Point mousepos;

        public MainWindow(int setidx)
        {
            InitializeComponent();
            CurrentSetting = new Setting(setidx);
        }

        #region Docking
        public DockStatus lastdockstatus;

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
            var win = (MainWindow)d;
            if (ds == DockStatus.None)
            {
                win.ResizeMode = ResizeMode.CanResizeWithGrip;
                if (!win.CurrentSetting.AutoDock) win.Topmost = false;
                else win.Topmost = true;
            }
            else
            {
                win.ResizeMode = ResizeMode.NoResize;
                win.Topmost = true;
            }
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
                    App.CurrScrnRect = new GetCurrentMonitor().GetInfo(this);
                    if (Left <= App.CurrScrnRect.Left) //dock left
                    {
                        toval = App.CurrScrnRect.Left - ActualWidth + pad;
                        tgtpro = LeftProperty;
                        dockto = DockStatus.Left;
                    }

                    else if (Left + ActualWidth >= App.CurrScrnRect.Right) //dock right
                    {
                        toval = App.CurrScrnRect.Right - pad;
                        tgtpro = LeftProperty;
                        dockto = DockStatus.Right;
                    }

                    else if (Top <= App.CurrScrnRect.Top) //dock top
                    {
                        toval = App.CurrScrnRect.Top - ActualHeight + pad;
                        tgtpro = TopProperty;
                        dockto = DockStatus.Top;
                    }

                    else if (Top + ActualHeight >= App.CurrScrnRect.Bottom) //dock bottom
                    {
                        toval = App.CurrScrnRect.Bottom - pad;
                        tgtpro = TopProperty;
                        dockto = DockStatus.Bottom;
                    }
                    else
                    {
                        lastdockstatus = DockStatus.None;
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
                            toval = App.CurrScrnRect.Left - ActualWidth + pad;
                            tgtpro = LeftProperty;
                            break;
                        case DockStatus.Right:
                            toval = App.CurrScrnRect.Right - pad;
                            tgtpro = LeftProperty;
                            break;
                        case DockStatus.Top:
                            toval = App.CurrScrnRect.Top - ActualHeight + pad;
                            tgtpro = TopProperty;
                            break;
                        case DockStatus.Bottom:
                            toval = App.CurrScrnRect.Bottom - pad;
                            tgtpro = TopProperty;
                            break;
                        default:
                            return;
                    }
                }

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

        internal void UnDock()
        {
            if (DockedTo != DockStatus.Docking && DockedTo != DockStatus.None)
            {
                double toval;
                DependencyProperty tgtpro;
                //double pad = 15d;
                DockStatus dockto;
                if (DockedTo == DockStatus.Left) //Me.Left = App.CurrScrnRect.left - Me.ActualWidth + pad Then
                {
                    toval = App.CurrScrnRect.Left;
                    tgtpro = LeftProperty;
                    dockto = DockStatus.None;
                }
                else if (DockedTo == DockStatus.Right) //Me.Left = App.CurrScrnRect.right - pad Then
                {
                    toval = App.CurrScrnRect.Right - ActualWidth;
                    tgtpro = LeftProperty;
                    dockto = DockStatus.None;
                }
                else if (DockedTo == DockStatus.Top) //Me.Top = App.CurrScrnRect.top - Me.ActualHeight + pad Then
                {
                    toval = App.CurrScrnRect.Top;
                    tgtpro = TopProperty;
                    dockto = DockStatus.None;
                }
                else if (DockedTo == DockStatus.Bottom) //Me.Top = App.CurrScrnRect.bottom - pad Then
                {
                    toval = App.CurrScrnRect.Bottom - ActualHeight;
                    tgtpro = TopProperty;
                    dockto = DockStatus.None;
                }
                else
                    return;

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
            if (CurrentSetting.AutoDock && DockedTo != DockStatus.None) UnDock();
        }

        private void Win_Main_MouseLeave(object sender, MouseEventArgs e)
        {
            if (CurrentSetting.AutoDock && 
                !RTB_Main.IsKeyboardFocusWithin &&
                App.FormatWindow.Opacity != 1 &&
                App.SearchWindow == null)
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
                App.FormatWindow.ToggleStrike(null, null);
            else if (e.Key == Key.V && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                App.FormatWindow.PasteAsText(null, null);
            else if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
                App.FormatWindow.Find(null, null);
            else if (e.Key == Key.H && Keyboard.Modifiers == ModifierKeys.Control)
                App.FormatWindow.ToggleHighlight(null, null);
        }

        private void RTB_Main_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //when padding is set on list, changing font size results in incorrect bullet position.
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (e.Delta > 0) //wheel up
                    App.FormatWindow.IncreaseSize(null, null);
                else
                    App.FormatWindow.DecreaseSize(null, null);
            }
        }

        private void RTB_Main_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (CurrentSetting.AutoDock &&
                App.FormatWindow.Opacity != 1 &&
                App.SearchWindow == null)
                DockToSide();
        }

        private void RTB_Main_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
            //update caller
            App.FormatWindow.UpdateCaller(this, this);
            App.FormatWindow.FadeIn();
            if (App.FormatWindow.Opacity == 1)//refresh values manually when the window is already visible.
                App.FormatWindow.LoadValues();
        }

        private void RTB_Main_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && App.FormatWindow?.Opacity == 1)
            {
                App.FormatWindow.FadeOut();
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
            if (CurrentSetting.AutoDock) DockToSide(true);
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

        internal void SaveToXamlPkg()
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
                        using (var ms = new FileStream(CurrentSetting.Doc_Location, FileMode.Create))
                        {
                            tr.Save(ms, DataFormats.XamlPackage, true);
                        }
                        File.WriteAllText(CurrentSetting.Bak_Location, tr.Text);
                    }
                    else
                    {
                        Dispatcher.Invoke(delegate
                        {
                            using (var ms = new FileStream(CurrentSetting.Doc_Location, FileMode.Create))
                            {
                                tr.Save(ms, DataFormats.XamlPackage, true);
                            };
                            File.WriteAllText(CurrentSetting.Bak_Location, tr.Text);
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

        private void Win_Main_Loaded(object sender, RoutedEventArgs e)
        {
            //WinAPI.SetToolWindow(this);

            //create tray icon
            if (App.TrayIcon == null)
            {
                using (var stream = Application.GetResourceStream(new Uri("pack://application:,,,/DesktopNote;component/Resources/stickynote.ico")).Stream)
                {
                    App.TrayIcon = new TaskbarIcon
                    {
                        Icon = new System.Drawing.Icon(stream),
                        ToolTipText = nameof(DesktopNote),
                        ContextMenu = (ContextMenu)Resources["TrayMenu"],
                    };
                }

                //tray icon double click
                App.TrayIcon.TrayMouseDoubleClick += (obj, args) =>
                {
                    foreach (var win in App.MainWindows)
                    {
                        win.Activate();
                        win.UnDock();
                    }
                };
            }

            //check and merge previous settings
            if (Properties.Settings.Default.UpgradeFlag == true)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeFlag = false;
                Properties.Settings.Default.Save();
            }

            //load settings
            Width = CurrentSetting.Win_Size.Width;
            Height = CurrentSetting.Win_Size.Height;
            Left = CurrentSetting.Win_Pos.X;
            Top = CurrentSetting.Win_Pos.Y;

            lastdockstatus = (DockStatus)CurrentSetting.DockedTo;
            DockedTo = lastdockstatus;
            RTB_Main.FontFamily = new FontFamily(CurrentSetting.Font);
            RTB_Main.Foreground = new SolidColorBrush(CurrentSetting.FontColor);
            RTB_Main.Background = new SolidColorBrush(CurrentSetting.BackColor);
            Rec_BG.Fill = new SolidColorBrush(CurrentSetting.PaperColor);

            if (App.FormatWindow == null)
            {
                App.FormatWindow = new Win_Format(null, this) { Tag = RTB_Main };
                ((Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Font.Content).SelectedColor = CurrentSetting.FontColor;
                ((Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Back.Content).SelectedColor = CurrentSetting.BackColor;

                //add fonts to menu
                foreach (var f in Fonts.SystemFontFamilies)
                {
                    var mi = new ComboBoxItem
                    {
                        Content = f.Source,
                        FontFamily = f,
                        FontSize = FontSize + 4,
                        ToolTip = f.Source
                    };
                    App.FormatWindow.CB_Font.Items.Add(mi);
                    if (f.Source == CurrentSetting.Font) mi.IsSelected = true;
                }
                App.FormatWindow.CB_Font.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Content", System.ComponentModel.ListSortDirection.Ascending));
                App.FormatWindow.CB_Font.SelectionChanged += (object s1, SelectionChangedEventArgs e1) =>
                {
                    if (App.FormatWindow.Opacity == 1 && e1.AddedItems.Count == 1)
                    {
                        var mi = (ComboBoxItem)e1.AddedItems[0];

                        if (!RTB_Main.Selection.IsEmpty) //only change selected
                            RTB_Main.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, mi.FontFamily);
                        else //change default
                        {
                            RTB_Main.FontFamily = mi.FontFamily;
                            CurrentSetting.Font = mi.FontFamily.Source;
                        }
                    }
                };
            }

            //loading contents
            lock (Lock_Save)
            {
                if (File.Exists(CurrentSetting.Doc_Location))
                {
                    try
                    {
                        var tr = new TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd);
                        tr.Load(new FileStream(CurrentSetting.Doc_Location, FileMode.Open), DataFormats.XamlPackage);
                    }
                    catch
                    {
                        MessageBox.Show((string)Application.Current.Resources["msgbox_load_error"] + "\r\n" + CurrentSetting.Bak_Location,
                            (string)Application.Current.Resources["msgbox_title_load_error"], MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
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

            App.CurrScrnRect = new GetCurrentMonitor().GetInfo(this);

            var task_save = new Thread(SaveNotes) { IsBackground = true };
            task_save.Start();

            var source = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            source.AddHook(WndProc);

        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == SingleInstance.RegisteredWM)
            {
                //activate and reset dock status when a second instance is requested
                //in case of resolution changes etc that might cause the main window to be "lost".
                Activate();
                //App.CurrScrnRect = new GetCurrentMonitor().GetInfo(this);
                //BeginAnimation(DockedToProperty, null); //required to modify the DockedTo property.
                //Top = App.CurrScrnRect.Top; //in case it was docked to bottom or top.
                //lastdockstatus = DockStatus.Left;
                //DockedTo = DockStatus.Left;
                //resetting position was moved to tray menu.
                UnDock();
            }
            return IntPtr.Zero;
        }


        #region TrayIcon Events
        private void TM_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Quit(true);
        }

        private void TM_ResetPos_Click(object sender, RoutedEventArgs e)
        {
            App.CurrScrnRect = new GetCurrentMonitor().GetInfo(this);
            var i = 0;
            foreach (var win in App.MainWindows)
            {
                var delta = i * 20;
                win.BeginAnimation(LeftProperty, null); //might be required to change position.
                win.BeginAnimation(TopProperty, null);
                win.BeginAnimation(DockedToProperty, null);
                win.BeginAnimation(OpacityProperty, null);
                win.Top = App.CurrScrnRect.Top + delta;
                win.Left = App.CurrScrnRect.Left + +delta;
                win.DockedTo = DockStatus.None;
                win.lastdockstatus = DockStatus.None;
                i += 1;
            }
        }

        private void TM_NewNote_Click(object sender, RoutedEventArgs e)
        {
            Win_Format.NewNote();
        }
        #endregion


    }
}
