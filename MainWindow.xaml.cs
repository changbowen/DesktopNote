using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopNote
{
    public partial class MainWindow : Window
    {
        private object Lock_Save = new object();
        private static int CountDown = 0;
        string doc_loc = AppDomain.CurrentDomain.BaseDirectory + Properties.Settings.Default.Doc_Location;
        string assname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        Point mousepos;
        
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
            DependencyProperty.Register("DockedTo", typeof(DockStatus), typeof(MainWindow), new PropertyMetadata(DockStatus.None));

        private void DockToSide(bool changpos = false)
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
            if (Properties.Settings.Default.AutoDock && Application.Current.Windows.Count == 1 && !RTB_Main.IsKeyboardFocused && !RTB_Main.ContextMenu.IsOpen)
                DockToSide();
        }
        #endregion

        #region Menu Events
        private void MenuItem_Help_Click()
        {
            if (MessageBox.Show("Available editing features can be accessd from menu or keyboard combination.\r\n" +
                "Use Ctrl + mouse wheel to change font size.\r\n" +
                "Change font or font size when there is a selection will only change selected text.\r\n" +
               "Note content will be auto saved to application root.\r\n" +
               "You will be directed to the homepage if you click OK.", "", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                System.Diagnostics.Process.Start("iexplore.exe", "https://github.com/changbowen/DesktopNote");
        }

        private void MenuItem_Exit_Click()
        {
            Quit(true);
        }

        private void MenuItem_Bullet_Click()
        {
            EditingCommands.ToggleBullets.Execute(null, RTB_Main);
        }

        private void MI_AutoStart_Click(object sender, RoutedEventArgs e)
        {
            var run = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (MI_AutoStart.IsChecked)
                run.SetValue(assname, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
            else
                run.DeleteValue(assname, false);
        }

        private void MI_AutoDock_Click(object sender, RoutedEventArgs e)
        {
            if (MI_AutoDock.IsChecked)
            {
                Properties.Settings.Default.AutoDock = true;
                DockToSide(true);
            }
            else
            {
                Properties.Settings.Default.AutoDock = false;
                Topmost = false;
            }
            Properties.Settings.Default.Save();
        }

        private void MenuItem_ResetFormats_Click(object sender, RoutedEventArgs e)
        {
            var tr = new TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd);
            tr.ClearAllProperties();

            var cp = (Color)ColorConverter.ConvertFromString((string)Properties.Settings.Default.Properties["PaperColor"].DefaultValue);
            Properties.Settings.Default.PaperColor = cp;
            Rec_BG.Fill = new SolidColorBrush(cp);
        }

        private void MenuItem_ResetSet_Click()
        {
            Properties.Settings.Default.Reset();
            Properties.Settings.Default.Save();

            Close();
            var win = new MainWindow();
            App.Current.MainWindow = win;
            win.Show();
        }

        private void ToggleStrike()
        {
            //strike-through
            dynamic tdc = RTB_Main.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (tdc == DependencyProperty.UnsetValue || tdc.Count > 0)
                tdc = null;
            else
                tdc = TextDecorations.Strikethrough;
            RTB_Main.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, tdc);
        }

        private void ToggleHighlight()
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

        private void PasteAsText()
        {
            RTB_Main.CaretPosition.InsertTextInRun(Clipboard.GetText());
        }

        private void IncreaseSize()
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

        private void DecreaseSize()
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

        private void Find()
        {
            new Win_Search().Show();
        }
        #endregion

        #region RichTextBox Events

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
                        using (var ms = new FileStream(doc_loc, FileMode.Create))
                        {
                            tr.Save(ms, DataFormats.XamlPackage, true);
                        }
                        File.WriteAllText(Properties.Settings.Default.Bak_Location, tr.Text);
                    }
                    else
                    {
                        Dispatcher.Invoke(delegate
                        {
                            using (var ms = new FileStream(doc_loc, FileMode.Create))
                            {
                                tr.Save(ms, DataFormats.XamlPackage, true);
                            };
                            File.WriteAllText(Properties.Settings.Default.Bak_Location, tr.Text);
                        });
                    }
                    result = "Saved";
                }
                catch
                {
                    result = "Save failed";
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

        private void Quit(bool savesetting)
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
            App.Current.Shutdown();
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
                            Rec_BG.Fill = new SolidColorBrush(e.NewValue.Value);
                            Properties.Settings.Default.PaperColor = e.NewValue.Value;
                            break;
                    }
                }
            }
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

            lastdockstatus = (DockStatus)set.DockedTo;
            RTB_Main.FontFamily = new FontFamily(set.Font);
            RTB_Main.Foreground = new SolidColorBrush(set.FontColor);
            ((Xceed.Wpf.Toolkit.ColorPicker)CP_Font.Content).SelectedColor = set.FontColor;
            RTB_Main.Background = new SolidColorBrush(set.BackColor);
            ((Xceed.Wpf.Toolkit.ColorPicker)CP_Back.Content).SelectedColor = set.BackColor;
            Rec_BG.Fill = new SolidColorBrush(set.PaperColor);
            ((Xceed.Wpf.Toolkit.ColorPicker)CP_Paper.Content).SelectedColor = set.PaperColor;

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
                CB_Font.Items.Add(mi);
                if (f.Source == set.Font) mi.IsSelected = true;
            }
            CB_Font.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Content", System.ComponentModel.ListSortDirection.Ascending));
            CB_Font.SelectionChanged += (object s1, SelectionChangedEventArgs e1) =>
              {
                  if (RTB_Main.ContextMenu.IsOpen && e1.AddedItems.Count == 1)
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
            if (File.Exists(doc_loc))
            {
                try
                {
                    var tr = new TextRange(RTB_Main.Document.ContentStart, RTB_Main.Document.ContentEnd);
                    tr.Load(new FileStream(doc_loc, FileMode.Open), DataFormats.XamlPackage);
                }
                catch
                {
                    MessageBox.Show("There was an error loading the note contents. Please refer to the following backup file at application root for recovery.\r\n" + set.Bak_Location, "Loading Notes Failed", MessageBoxButton.OK, MessageBoxImage.Stop);
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

            //check auto dock
            if (set.AutoDock == true) MI_AutoDock.IsChecked = true;

            //check auto start
            var run = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            string run_value = (string)run.GetValue(assname);

            if (run_value != "")
            {
                MI_AutoStart.IsChecked = true;
                if (run_value != System.Reflection.Assembly.GetExecutingAssembly().Location)
                    run.SetValue(assname, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
            }

            currScrnRect = new GetCurrentMonitor().GetInfo();

            var task_save = new Thread(SaveNotes);
            task_save.IsBackground = true;
            task_save.Start();
        }

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
                ToggleStrike();
            else if (e.Key == Key.V && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                PasteAsText();
            else if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
                Find();
            else if (e.Key == Key.H && Keyboard.Modifiers == ModifierKeys.Control)
                ToggleHighlight();
        }

        private void RTB_Main_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            //when padding is set on list, changing font size results in incorrect bullet position.
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (e.Delta > 0) //wheel up
                    IncreaseSize();
                else
                    DecreaseSize();
            }
        }

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

        private void RTB_Main_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (Properties.Settings.Default.AutoDock && App.Current.Windows.Count == 1 && !RTB_Main.ContextMenu.IsOpen)
                DockToSide();
        }

        private void RTB_Main_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            //update combobox selection etc
            if (!RTB_Main.Selection.IsEmpty)
            {
                var caretfont = RTB_Main.Selection.GetPropertyValue(TextElement.FontFamilyProperty) as FontFamily;
                if (caretfont != null)
                    CB_Font.SelectedValue = caretfont.Source;
                else //multiple fonts
                    CB_Font.SelectedIndex = -1;
                CB_Font.ToolTip = "Font (Selection)";

                var caretfontcolor = RTB_Main.Selection.GetPropertyValue(TextElement.ForegroundProperty) as SolidColorBrush;
                var fontcolorpicker = (Xceed.Wpf.Toolkit.ColorPicker)CP_Font.Content;
                if (caretfontcolor != null) fontcolorpicker.SelectedColor = new Color?(caretfontcolor.Color);
                else fontcolorpicker.SelectedColor = null;

                var caretbackcolor = RTB_Main.Selection.GetPropertyValue(TextElement.BackgroundProperty) as SolidColorBrush;
                var backcolorpicker = (Xceed.Wpf.Toolkit.ColorPicker)CP_Back.Content;
                if (caretbackcolor != null) backcolorpicker.SelectedColor = new Color?(caretbackcolor.Color);
                else backcolorpicker.SelectedColor = null;
            }
            else
            {
                CB_Font.SelectedValue = Properties.Settings.Default.Font;
                CB_Font.ToolTip = "Font (Default)";
                ((Xceed.Wpf.Toolkit.ColorPicker)CP_Font.Content).SelectedColor = Properties.Settings.Default.FontColor;
                ((Xceed.Wpf.Toolkit.ColorPicker)CP_Back.Content).SelectedColor = Properties.Settings.Default.BackColor;
            }
        }
    }
}
