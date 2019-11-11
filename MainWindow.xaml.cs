using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DesktopNote
{
    public class ThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness p;
            if (parameter == null) p = new Thickness(1);
            else p = (Thickness)new System.Windows.ThicknessConverter().ConvertFrom(parameter);
            var d = (double)value;
            return new Thickness(d * p.Left, d * p.Top, d * p.Right, d * p.Bottom);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public partial class MainWindow : Window
    {
        internal readonly Setting CurrentSetting;
        internal readonly object lock_save = new object();

        //override CurrentSetting with values loaded from note content file
        private const int SaveCountDownValue = 2000;
        private int SaveCountDown = 0;
        private readonly CancellationTokenSource SaveCoundDownCancel = new CancellationTokenSource();
        private Task SaveNoteTask;
        private Point MousePos;
        private DispatcherTimer DockTimer;
        private int DockTimerCountDown;
        private bool IsResizing;


        public MainWindow(Setting setting)
        {
            InitializeComponent();
            CurrentSetting = setting;
            CurrentSetting.MainWin = this;
        }

        //public MainWindow(string path)
        //{
        //    InitializeComponent();
        //    note_path = path;
        //}


        #region Docking
        public DockStatus LastDockStatus;

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
            if (ds == DockStatus.None) {
                win.ResizeMode = ResizeMode.CanResizeWithGrip;
                if (!win.CurrentSetting.AutoDock) win.Topmost = false;
                else win.Topmost = true;
            }
            else {
                win.ResizeMode = ResizeMode.NoResize;
                win.Topmost = true;
            }
        }

        /// <summary>
        /// Dock the MainWindow based on its location.
        /// </summary>
        /// <param name="changePos">Update LastDockStatus if set to True.</param>
        internal void DockToSide(bool changePos = false)
        {
            if (DockedTo == DockStatus.None) {
                double toval;
                DependencyProperty tgtpro;
                double pad = 15d;
                DockStatus dockto;
                if (changePos) {
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
                    else {
                        LastDockStatus = DockStatus.None;
                        return;
                    }
                    LastDockStatus = dockto;
                }
                else //'restore last docking position
                {
                    dockto = LastDockStatus;
                    switch (LastDockStatus) {
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

                var anim_move = new DoubleAnimation(toval, new Duration(new TimeSpan(0, 0, 0, 0, 500))) {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                var anim_fade = new DoubleAnimation(0.4, new Duration(new TimeSpan(0, 0, 0, 0, 300))) {
                    BeginTime = new TimeSpan(0, 0, 0, 0, 200)
                };
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
            if ((int)DockedTo > 1) {
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

                var anim_move = new DoubleAnimation(toval, new Duration(new TimeSpan(0, 0, 0, 0, 300)), FillBehavior.Stop) {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
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

        /// <summary>
        /// Check whether MainWindow should dock based on various mouse, keyboard and window conditions.
        /// Should be used with DockToSide(false) as when changePos is true, additional conditions will cause LastDockStatus update to be ignored.
        /// </summary>
        private bool ShouldDock()
        {
#if DEBUG
            System.Diagnostics.Debug.Print(
                (LastDockStatus != DockStatus.None) + ", " +
                !RTB_Main.IsKeyboardFocusWithin + ", " +
                !Win_Main.IsMouseOver + ", " +
                !IsResizing + ", " +
                (App.FormatWindow == null || !App.FormatWindow.MainWin.Equals(this) || App.FormatWindow.Opacity == 0) + ", " +
                (App.SearchWindow == null || !App.SearchWindow.MainWin.Equals(this) || App.SearchWindow.Opacity == 0) + ", " +
                (App.OptionsWindow == null || !App.OptionsWindow.MainWin.Equals(this) || App.OptionsWindow.Opacity == 0));
#endif
            if (CurrentSetting.AutoDock &&
                LastDockStatus != DockStatus.None &&
                !RTB_Main.IsKeyboardFocusWithin &&
                !Win_Main.IsMouseOver &&
                !IsResizing &&
                (App.FormatWindow == null || !App.FormatWindow.MainWin.Equals(this) || App.FormatWindow.Opacity == 0) &&
                (App.SearchWindow == null || !App.SearchWindow.MainWin.Equals(this) || App.SearchWindow.Opacity == 0) &&
                (App.OptionsWindow == null || !App.OptionsWindow.MainWin.Equals(this) || App.OptionsWindow.Opacity == 0))
                return true;
            return false;
        }

        private void Win_Main_MouseEnter(object sender, MouseEventArgs e)
        {
            //undocking
            if (CurrentSetting.AutoDock && (DockedTo != DockStatus.None || (int)LastDockStatus > 1)) {
                UnDock();
                //dock if conditions met in 1 min
                if (DockTimer != null && DockTimer.IsEnabled) return;
                DockTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
                DockTimerCountDown = 60;
                DockTimer.Tick += (s1, e1) => {
                    DockTimerCountDown -= 2;
                    if (DockTimerCountDown <= 0 || (int)DockedTo > 1 || (int)LastDockStatus < 2) {
                        DockTimer.Stop();
                        DockTimer = null;
                        return;
                    }

                    if (ShouldDock()) {
                        DockToSide();
                        DockTimer.Stop();
                        DockTimer = null;
                        return;
                    }
                };
                DockTimer.Start();
            }
        }

        private void Rec_BG_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rec_BG.ReleaseMouseCapture();
            if (CurrentSetting.AutoDock) DockToSide(true);
        }

        //private void Win_Main_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    if (ShouldDock()) DockToSide();
        //}

        //private void RTB_Main_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    if (ShouldDock()) DockToSide();
        //}
        #endregion

        #region RichTextBox Events
        private void RTB_Main_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (RTB_Main.IsFocused) {
                SaveCountDown = SaveCountDownValue;
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
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                e.Handled = true;
                if (e.Delta > 0) //wheel up
                    App.FormatWindow.IncreaseSize(null, null);
                else
                    App.FormatWindow.DecreaseSize(null, null);
            }
        }

        private void RTB_Main_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            App.FormatWindow.UpdateTargets(this);
        }

        private void RTB_Main_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
            //update caller
            App.FormatWindow.UpdateTargets(this);
            App.FormatWindow.FadeIn();
            if (App.FormatWindow.Opacity == 1)//refresh values manually when the window is already visible.
                App.FormatWindow.LoadValues();
        }

        private void RTB_Main_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && App.FormatWindow?.Opacity == 1) {
                App.FormatWindow.FadeOut();
            }
        }
        #endregion

        #region Rect Events
        private void Rec_BG_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rec_BG.CaptureMouse();
            if (e.ClickCount == 2) {
                if (WindowState == WindowState.Normal)
                    WindowState = WindowState.Maximized;
                else if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
            }
            else
                MousePos = e.GetPosition(this);
        }

        private void Rec_BG_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && MousePos != null) {
                var pos = e.GetPosition(this);
                Left += pos.X - MousePos.X;
                Top += pos.Y - MousePos.Y;
            }
        }
        #endregion

        /// <summary>
        /// Calls SaveNote on the UI thread after CoundDown.
        /// </summary>
        private void SaveNoteThread()
        {
            while (true) {
                while (SaveCountDown <= 0) {
                    if (SaveCoundDownCancel.IsCancellationRequested) return;
                    Thread.Sleep(1000);
                }
                while (SaveCountDown > 0) {
                    if (SaveCoundDownCancel.IsCancellationRequested) return;
                    Thread.Sleep(500);
                    SaveCountDown -= 500;
                }

                bool isUIthread = Dispatcher.CheckAccess();
                if (isUIthread) Helpers.SaveNote(this);
                else Dispatcher.Invoke(() => Helpers.SaveNote(this));
            }
        }

        public async Task Close(bool dontSave = false)
        {
            if (!dontSave) {
                //update window size and position
                CurrentSetting.Win_Pos = new Point(Left, Top);
                CurrentSetting.Win_Size = new Size(Width, Height);
                CurrentSetting.DockedTo = (int)LastDockStatus;

                //save per-note, app settings and contents to file
                var exMsg = Helpers.SaveNote(this);
                if (exMsg != null &&
                    Helpers.MsgBox(
                        body: exMsg + "\r\n" + (string)App.Res["msgbox_not_saved_confirm"],
                        button: MessageBoxButton.YesNo,
                        image: MessageBoxImage.Exclamation) != MessageBoxResult.Yes) {
                    return;
                }
            }

            //cleaning up
            SaveCoundDownCancel.Cancel();
            CurrentSetting.PropertyChanged -= CurrentSetting_PropertyChanged;
            App.MainWindows.Remove(this);
            await SaveNoteTask;
            base.Close();
        }

        private void Win_Main_Loaded(object sender, RoutedEventArgs e)
        {
            //check and merge previous settings
            Setting.Upgrade();

            //create tray icon so you can still use the app when notes fail to load
            if (App.TrayIcon == null) {
                using (var stream = Application.GetResourceStream(new Uri("pack://application:,,,/DesktopNote;component/Resources/stickynote.ico")).Stream) {
                    App.TrayIcon = new TaskbarIcon {
                        Icon = new System.Drawing.Icon(stream),
                        ToolTipText = nameof(DesktopNote),
                        ContextMenu = (ContextMenu)Resources["TrayMenu"],
                    };
                }

                //tray icon double click
                App.TrayIcon.TrayMouseDoubleClick += TrayIcon_TrayMouseDoubleClick;
            }

            //load settings and content
            if (CurrentSetting.Flags.HasFlag(Setting.NoteFlag.Existing) && !Helpers.LoadNote(this)) {
                Close(true); return;
            }

            //save when settings changed
            CurrentSetting.PropertyChanged += CurrentSetting_PropertyChanged;

            //apply settings
            Width = CurrentSetting.Win_Size.Width;
            Height = CurrentSetting.Win_Size.Height;
            Left = CurrentSetting.Win_Pos.X;
            Top = CurrentSetting.Win_Pos.Y;

            LastDockStatus = (DockStatus)CurrentSetting.DockedTo;
            //DockedTo = DockStatus.None;
            RTB_Main.FontFamily = new FontFamily(CurrentSetting.Font);
            RTB_Main.Foreground = new SolidColorBrush(CurrentSetting.FontColor);
            RTB_Main.Background = new SolidColorBrush(CurrentSetting.BackColor);
            Rec_BG.Fill = new SolidColorBrush(CurrentSetting.PaperColor);

            if (App.FormatWindow == null) {
                App.FormatWindow = new Win_Format(this);
                ((Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Font.Content).SelectedColor = CurrentSetting.FontColor;
                ((Xceed.Wpf.Toolkit.ColorPicker)App.FormatWindow.CP_Back.Content).SelectedColor = CurrentSetting.BackColor;

                //add fonts to menu
                foreach (var f in Fonts.SystemFontFamilies) {
                    var mi = new ComboBoxItem {
                        Content = f.Source,
                        FontFamily = f,
                        FontSize = FontSize + 4,
                        ToolTip = f.Source
                    };
                    App.FormatWindow.CB_Font.Items.Add(mi);
                    if (f.Source == CurrentSetting.Font) mi.IsSelected = true;
                }
                App.FormatWindow.CB_Font.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));
            }

            //reset dependency property values saved explicitly
            //this is to reduce format inconsistency across reloads
            if (RTB_Main.Document.Blocks.Count > 0) {
                RTB_Main.FontSize = RTB_Main.Document.Blocks.FirstBlock.FontSize;
                foreach (var b in RTB_Main.Document.Blocks) {
                    //unify font for new paragraghs. otherwise wont be able to change fonts after reload.
                    //doesnt affect specifically set font sizes in Inlines & Run.
                    b.ClearValue(TextElement.FontSizeProperty);
                    b.ClearValue(TextElement.FontFamilyProperty);
                    b.ClearValue(TextElement.ForegroundProperty);
                    b.ClearValue(TextElement.BackgroundProperty);
                }

                //reset Padding on List
                foreach (var block in RTB_Main.Document.Blocks) {
                    resetPadding(block);
                }
            }

            //without the below two lines, Load actions can be undone.
            RTB_Main.IsUndoEnabled = false;
            RTB_Main.IsUndoEnabled = true;

            //trigger dock after 2s
            if (CurrentSetting.AutoDock)
                Task.Run(() => { Thread.Sleep(2000); Dispatcher.Invoke(() => DockToSide(true)); });

            //start save note thread
            SaveNoteTask = Task.Run(SaveNoteThread, SaveCoundDownCancel.Token);

            //add hook
            var source = PresentationSource.FromVisual(this) as System.Windows.Interop.HwndSource;
            source.AddHook(WndProc);

            //update stuff
            if (!Setting.NoteList.Contains(CurrentSetting.Doc_Location)) {
                Setting.NoteList.Add(CurrentSetting.Doc_Location);
                Setting.Save();
            }
            App.MainWindows.Add(this);
        }

        private void CurrentSetting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveCountDown = SaveCountDownValue;
        }

        private void resetPadding(TextElement ele)
        {
            switch (ele) {
                case List lst:
                    lst.ClearValue(Block.PaddingProperty);
                    foreach (var lstItem in lst.ListItems) {
                        resetPadding(lstItem);
                    }
                    break;
                case ListItem lstItem:
                    foreach (var blk in lstItem.Blocks) {
                        resetPadding(blk);
                    }
                    break;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == SingleInstance.RegisteredWM) {
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
            else if (msg == 0x231) //WM_ENTERSIZEMOVE
                IsResizing = true;
            else if (msg == 0x232) //WM_EXITSIZEMOVE
                IsResizing = false;

            return IntPtr.Zero;
        }


        #region TrayIcon Events
        private void TM_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.Quit();
        }

        private void TM_ResetPos_Click(object sender, RoutedEventArgs e)
        {
            App.CurrScrnRect = new GetCurrentMonitor().GetInfo(this);
            var i = 0;
            foreach (var win in App.MainWindows) {
                var delta = i * 20;
                win.BeginAnimation(LeftProperty, null); //might be required to change position.
                win.BeginAnimation(TopProperty, null);
                win.BeginAnimation(DockedToProperty, null);
                win.BeginAnimation(OpacityProperty, null);
                win.Top = App.CurrScrnRect.Top + delta;
                win.Left = App.CurrScrnRect.Left + +delta;
                win.DockedTo = DockStatus.None;
                win.LastDockStatus = DockStatus.None;
                i += 1;
            }
        }

        private void TM_NewNote_Click(object sender, RoutedEventArgs e)
        {
            Helpers.NewNote();
        }

        private void TM_OpenNote_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog needs at least one loaded parent window
            if (App.MainWindows.Count == 0) return;
            var visWin = App.MainWindows.Find(w => PresentationSource.FromVisual(w) != null);
            if (visWin == null) return;
            //using the last opened location
            var path = Helpers.OpenFileDialog(visWin, false);
            if (path == null) return;
            Helpers.OpenNote(path)?.Show();
        }

        private void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            foreach (var win in App.MainWindows) {
                win.Activate();
                win.UnDock();
            }
        }
        #endregion
    }
}
