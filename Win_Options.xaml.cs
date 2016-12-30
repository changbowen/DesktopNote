//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Input;
using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Shapes;

namespace DesktopNote
{
    public partial class Win_Options : RoundedWindow
    {
        RichTextBox RTB_Main = App.mainwin.RTB_Main;
        private string assname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

        public Win_Options()
        {
            InitializeComponent();
        }

        private void CB_AutoStart_Click(object sender, RoutedEventArgs e)
        {
            var run = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (CB_AutoStart.IsChecked == true)
                run.SetValue(assname, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
            else
                run.DeleteValue(assname, false);
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

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {
            Close();
            System.Threading.Tasks.Task.Run(delegate
            {
                if (MessageBox.Show((string)Application.Current.Resources["msgbox_help"], "", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    System.Diagnostics.Process.Start("iexplore.exe", "https://github.com/changbowen/DesktopNote");
            });
        }

        private void ColorChange(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue && ((Xceed.Wpf.Toolkit.ColorPicker)sender).IsOpen)
            {
                App.mainwin.Rec_BG.Fill = new SolidColorBrush(e.NewValue.Value);
                Properties.Settings.Default.PaperColor = e.NewValue.Value;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var set = Properties.Settings.Default;

            //check auto dock
            if (set.AutoDock == true) CB_AutoDock.IsChecked = true;

            //check auto start
            var run = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            string run_value = (string)run.GetValue(assname);

            if (!string.IsNullOrEmpty(run_value))
            {//update the exe location if Run contains assname.
                CB_AutoStart.IsChecked = true;
                if (run_value != System.Reflection.Assembly.GetExecutingAssembly().Location)
                    run.SetValue(assname, System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
            }

            //set paper color
            CP_Paper.SelectedColor = set.PaperColor;

            //set path
            TB_SavePath.Text = set.Doc_Location;
            TB_SavePathTxt.Text = set.Bak_Location;

        }

        private void TB_SavePath_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var savedlg = new Microsoft.Win32.SaveFileDialog();
            savedlg.Filter = "DesktopNote Content|*";
            if (savedlg.ShowDialog(this) == true &&
                savedlg.FileName != System.IO.Path.GetFullPath(Properties.Settings.Default.Doc_Location))
            {
                Properties.Settings.Default.Doc_Location = savedlg.FileName;
                Properties.Settings.Default.Save();
                TB_SavePath.Text = savedlg.FileName;
            }
        }

        private void TB_SavePathTxt_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var savedlg = new Microsoft.Win32.SaveFileDialog();
            savedlg.Filter = "DesktopNote Text Content|*.txt";
            if (savedlg.ShowDialog(this) == true &&
                savedlg.FileName != System.IO.Path.GetFullPath(Properties.Settings.Default.Bak_Location))
            {
                Properties.Settings.Default.Bak_Location = savedlg.FileName;
                Properties.Settings.Default.Save();
                TB_SavePathTxt.Text = savedlg.FileName;
            }
        }
    }
}
