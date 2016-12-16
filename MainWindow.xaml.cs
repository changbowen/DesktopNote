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
    }
}
